using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multichat_client
{
    public partial class Client : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;

        protected delegate void UpdateDisplayDelegate(string message);

        public Client()
        {
            InitializeComponent();
            btnSendMessage.Enabled = false;
            btnDisconnectFromServer.Enabled = false;
            txtMessageToBeSend.Enabled = false;
        }

        // Everything related to user interface
        private async void BtnDisconnectFromServer_Click(object sender, EventArgs e)
        {
            try
            {
                await DisconnectFromServerAsync(txtUsername.Text);
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnConnectWithServer_Click(object sender, EventArgs e)
        {
            try
            {
                await CreateConnectionAsync();
            }
            catch (IOException ex)
            {
                AddMessage("Disconnected!");
                MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // disable or enable buttons
                btnConnectWithServer.Enabled = true;
                btnSendMessage.Enabled = false;
                btnDisconnectFromServer.Enabled = false;

                //disable or enable text fields
                txtUsername.Enabled = true;
                txtChatServerIP.Enabled = true;
                txtChatServerPort.Enabled = true;
                txtMessageToBeSend.Enabled = false;
                txtBufferSize.Enabled = true;
                
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSendMessage_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtMessageToBeSend.Text) || networkStream == null)
            {
                return;
            }

            try
            {
                await SendMessageAsync("MESSAGE", txtUsername.Text, txtMessageToBeSend.Text);
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TxtMessageToBeSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtMessageToBeSend.Text) || networkStream == null || !(e.KeyChar == (char)13))
            {
                return;
            }

            try
            {
                await SendMessageAsync("MESSAGE", txtUsername.Text, txtMessageToBeSend.Text);
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void AddMessage(string message)
        {
            if (listChats.InvokeRequired)
            {
                listChats.Invoke(new UpdateDisplayDelegate(UpdateDisplay), new object[] { message });
            }
            else
            {
                UpdateDisplay(message);
            }
        }

        private void UpdateDisplay(string message)
        {
            listChats.Items.Add(message);
            listChats.SelectedIndex = listChats.Items.Count - 1;
        }

        // Everything related to messages
        private async Task SendMessageAsync(string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);

            await SendMessageOnNetwork(completeMessage);

            message = DecodeMessage(message);

            AddMessage($"{username}: {message}");
            txtMessageToBeSend.Clear();
            txtMessageToBeSend.Focus();
        }

        private async Task SendDisconnectMessageAsync(string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);
            await SendMessageOnNetwork(completeMessage);
        }

        private async Task SendMessageOnNetwork(string message)
        {
            int bufferSize = StringToInt(txtBufferSize.Text);
            do
            {
                if(bufferSize > message.Length)
                {
                    bufferSize = message.Length;
                }

                string substring = message.Substring(0, bufferSize);
                message = message.Remove(0, bufferSize);
                byte[] buffer = Encoding.ASCII.GetBytes(substring);
                await networkStream.WriteAsync(buffer, 0, bufferSize);
            }
            while (message.Length > 0);
        }

        // Everything related to buttons
        private async Task DisconnectFromServerAsync(string username)
        {
            await SendDisconnectMessageAsync("INFO", username, "DISCONNECTING");
            // disable or enable buttons
            btnConnectWithServer.Enabled = true;
            btnSendMessage.Enabled = false;
            btnDisconnectFromServer.Enabled = false;

            //disable or enable text fields
            txtUsername.Enabled = true;
            txtChatServerIP.Enabled = true;
            txtChatServerPort.Enabled = true;
            txtMessageToBeSend.Enabled = false;
            txtBufferSize.Enabled = true;
        }

        private async Task CreateConnectionAsync()
        {
            int portNumber = StringToInt(txtChatServerPort.Text);
            int bufferSize = StringToInt(txtBufferSize.Text);
            string IPaddress = txtChatServerIP.Text;
            string username = txtUsername.Text;

            if (!ValidateClientPreferences(username, IPaddress, portNumber, bufferSize))
            {
                return;
            }

            AddMessage("Connecting...");
            using (tcpClient = new TcpClient())
                try
                {
                    await tcpClient.ConnectAsync(IPaddress, portNumber);
                    // disable or enable buttons
                    btnConnectWithServer.Enabled = false;
                    btnSendMessage.Enabled = true;
                    btnDisconnectFromServer.Enabled = true;

                    //disable or enable text fields
                    txtUsername.Enabled = false;
                    txtChatServerIP.Enabled = false;
                    txtChatServerPort.Enabled = false;
                    txtMessageToBeSend.Enabled = true;
                    txtBufferSize.Enabled = false;

                    await Task.Run(() => ReceiveData(bufferSize));
                    // disable or enable buttons
                    btnConnectWithServer.Enabled = true;
                    btnSendMessage.Enabled = false;
                    btnDisconnectFromServer.Enabled = false;

                    //disable or enable text fields
                    txtUsername.Enabled = true;
                    txtChatServerIP.Enabled = true;
                    txtChatServerPort.Enabled = true;
                    txtMessageToBeSend.Enabled = false;
                    txtBufferSize.Enabled = true;
                }
                catch (SocketException ex)
                {
                    AddMessage("No connection possible, try again later!");
                    MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

        }

        // Everything related to receiving data.
        private async Task ReceiveData(int bufferSize)
        {
            string message = "";
            byte[] buffer = new byte[bufferSize];

            networkStream = tcpClient.GetStream();
            AddMessage("Connected!");

            while (networkStream.CanRead)
            {
                StringBuilder completeMessage = new StringBuilder();

                do
                {
                    do
                    {
                        int readBytes = await networkStream.ReadAsync(buffer, 0, bufferSize);
                        message = Encoding.ASCII.GetString(buffer, 0, readBytes);
                        completeMessage.Append(message);
                    }
                    while (completeMessage.ToString().IndexOf("@", 1) < 0);                    
                }
                while (networkStream.DataAvailable);

                string decodedType = FilterProtocol(completeMessage.ToString(), new Regex(@"(?<=\@)(.*?)(?=\|{2})"));
                string decodedUsername = FilterProtocol(completeMessage.ToString(), new Regex(@"(?<=\|{2})(.*?)(?=\|{2})"));
                string decodedMessage = DecodeMessage(FilterProtocol(FilterProtocol(completeMessage.ToString(), new Regex(@"\|(?:.(?!\|))+$")), new Regex(@"(?<=\|{2})(.*?)(?=\@)")));

                if (decodedType == "INFO" && decodedMessage == "DISCONNECTING")
                {
                    AddMessage("Server is closing!");
                    await SendDisconnectMessageAsync("INFO", txtUsername.Text, "DISCONNECT");
                    break;
                }

                if (decodedType == "INFO" && decodedMessage == "DISCONNECT")
                {
                    break;
                }

                if (decodedType == "MESSAGE")
                {
                    if (decodedUsername == "Server")
                    {
                        AddMessage($"{decodedMessage}");
                    } else
                    {
                        AddMessage($"{decodedUsername}: {decodedMessage}");
                    }
                }
            }

            networkStream.Close();
            tcpClient.Close();

            AddMessage("Connection closed");
        }


        // Everything related to encoding/decoding and the protocol
        private string EncodeMessage(string type, string username, string message)
        {
            type = Regex.Replace(type, "[|]", "&#124");
            type = Regex.Replace(type, "[@]", "&#64");

            username = Regex.Replace(username, "[|]", "&#124");
            username = Regex.Replace(username, "[@]", "&#64");

            message = Regex.Replace(message, "[|]", "&#124");
            message = Regex.Replace(message, "[@]", "&#64");

            return $"@{type}||{username}||{message}@";
        }

        private string FilterProtocol(string message, Regex regex)
        {
            return regex.Match(message).ToString();
        }

        private string DecodeMessage(string str)
        {
            str = Regex.Replace(str, "&#124", "|");
            str = Regex.Replace(str, "&#64", "@");

            return str;
        }

        // Everything related to validation of input
        private bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        private bool ValidateClientPreferences(string username, string IPaddress, int portNumber, int bufferSize)
        {

            if (String.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please fill in a username", "No username", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!ValidateIPv4(IPaddress))
            {
                MessageBox.Show("An invalid IP address has been given! Try another IP address", "Invalid IP address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!(portNumber >= 1024 && portNumber <= 65535))
            {
                MessageBox.Show("Port had an invalid value or is not within the range of 1024 - 65535", "Invalid Port number", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (bufferSize <= 0)
            {
                MessageBox.Show("An invalid amount of buffer size has been given! Try something else.", "Invalid amount of Buffer Size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // Everything needed to make other things work
        private int StringToInt(string text)
        {
            int number;
            int.TryParse(text, out number);

            return number;
        }
    }
}
