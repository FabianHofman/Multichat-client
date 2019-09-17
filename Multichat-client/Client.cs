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
        }

        private async void BtnDisconnectFromServer_Click(object sender, EventArgs e)
        {
            try
            {
                await DisconnectFromServerAsync();
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
                MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SocketException ex)
            {
                AddMessage("No connection possible, try again later!");
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSendMessage_Click(object sender, EventArgs e)
        {
            if (txtMessageToBeSend.Text == "" || networkStream == null)
            {
                return;
            }

            try
            {
                await SendMessageAsync("MESSAGE", "username", txtMessageToBeSend.Text);
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void TxtMessageToBeSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (txtMessageToBeSend.Text == "" || networkStream == null || !(e.KeyChar == (char)13))
            {
                return;
            }

            try
            {
                await SendMessageAsync("MESSAGE", "username", txtMessageToBeSend.Text);
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async Task SendMessageAsync(string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);

            byte[] buffer = Encoding.ASCII.GetBytes(completeMessage);
            await networkStream.WriteAsync(buffer, 0, buffer.Length);

            AddMessage(message);
            txtMessageToBeSend.Clear();
            txtMessageToBeSend.Focus();
        }


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
            str = Regex.Replace(str, "[&#124]", "|");
            str = Regex.Replace(str, "[&#64]", "@");

            return str;
        }

        private async Task DisconnectFromServerAsync()
        {
            await SendMessageAsync("INFO", "username", "DISCONNECT");
        }

        private async Task CreateConnectionAsync()
        {
            int portNumber = StringToInt(txtChatServerPort.Text);
            int bufferSize = StringToInt(txtBufferSize.Text);
            string IPaddress = txtChatServerIP.Text;

            if (!ValidateClientPreferences(IPaddress, portNumber, bufferSize))
            {
                return;
            }

            AddMessage("Connecting...");
            using(tcpClient = new TcpClient())
                try
                {
                    await tcpClient.ConnectAsync(IPaddress, portNumber);
                    btnConnectWithServer.Enabled = false;
                    btnSendMessage.Enabled = true;
                    btnDisconnectFromServer.Enabled = true;
                    await Task.Run(() => ReceiveData(bufferSize));
                }
                catch (SocketException ex)
                {
                    AddMessage("No connection possible, try again later!");
                    MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

        }

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
                    int readBytes = await networkStream.ReadAsync(buffer, 0, bufferSize);
                    message = Encoding.ASCII.GetString(buffer, 0, readBytes);
                    completeMessage.Append(message);
                }
                while (networkStream.DataAvailable);

                string decodedType = FilterProtocol(completeMessage.ToString(), new Regex(@"(?<=\@)(.*?)(?=\|)"));
                string decodedUsername = FilterProtocol(completeMessage.ToString(), new Regex(@"(?<=\|)(.*?)(?=\|)"));
                string decodedMessage = DecodeMessage(FilterProtocol(completeMessage.ToString(), new Regex(@"(?<=\|)(.*?)(?=\@)")));

                if (decodedType == "INFO" && decodedMessage == "disconnect")
                {
                    break;
                }

                AddMessage($"{decodedUsername}: {decodedMessage}");
            }

            networkStream.Close();
            tcpClient.Close();

            AddMessage("Connection closed");
        }

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

        private bool ValidateClientPreferences(string IPaddress, int portNumber, int bufferSize)
        {
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

            if (bufferSize < 1)
            {
                MessageBox.Show("An invalid amount of buffer size has been given! Try something else.", "Invalid amount of Buffer Size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private int StringToInt(string text)
        {
            int number;
            int.TryParse(text, out number);

            return number;
        }


    }
}
