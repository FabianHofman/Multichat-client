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

        private async Task UpdateUI(bool SendMessage, bool ConnectWithServer, bool DisconnectFromServer)
        {
            btnSendMessage.Enabled = SendMessage;
            btnConnectWithServer.Enabled = ConnectWithServer;
            btnConnectWithServer.Enabled = DisconnectFromServer;
        }

        private async void BtnDisconnectFromServer_Click(object sender, EventArgs e)
        {
            try
            {
                await DisconnectFromServerAsync();
            }
            catch
            {
                await Task.Run(() => UpdateUI(true, false, true));
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DisconnectFromServerAsync()
        {
            await SendMessageAsync("disconnect");
            await Task.Run(() => UpdateUI(false, true, false));
        }

        private async void BtnConnectWithServer_Click(object sender, EventArgs e)
        {
            try
            {
                await CreateConnectionAsync();
            }
            catch (IOException ex)
            {
                await Task.Run(() => UpdateUI(false, true, false));
                MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SocketException ex)
            {
                await Task.Run(() => UpdateUI(false, true, false));
                AddMessage("No connection possible, try again later!");
                MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                await Task.Run(() => UpdateUI(false, true, false));
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
                await HandleMessageTransactionAsync(txtMessageToBeSend.Text);
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
                await HandleMessageTransactionAsync(txtMessageToBeSend.Text);
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async Task HandleMessageTransactionAsync(string message)
        {
            await SendMessageAsync(message);
            AddMessage(message);
            txtMessageToBeSend.Clear();
            txtMessageToBeSend.Focus();
        }

        private async Task SendMessageAsync(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            await networkStream.WriteAsync(buffer, 0, buffer.Length);
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

            tcpClient = await Task.Run(() => CreateNewTcpClient(IPaddress, portNumber));
            await Task.Run(() => UpdateUI(true, false, true));
            await Task.Run(() => ReceiveData(bufferSize));

        }

        private async Task<TcpClient> CreateNewTcpClient(string IPaddress, int portNumber)
        {
            return new TcpClient(IPaddress, portNumber);
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

                if (completeMessage.ToString() == "disconnect")
                {
                    await Task.Run(() => DisconnectFromServerAsync());
                    break;
                }

                AddMessage(completeMessage.ToString());
            }

            networkStream.Close();
            tcpClient.Close();

            AddMessage("Connection closed");
        }

        private int StringToInt(string text)
        {
            int number;
            int.TryParse(text, out number);

            return number;
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

        
    }
}
