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

namespace Mutliclient_server
{
    public partial class Server : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;

        protected delegate void UpdateDisplayDelegate(string message);

        public Server()
        {
            InitializeComponent();
            btnSend.Enabled = false;
        }
        private void AddMessage(string message)
        {
            if (listMessages.InvokeRequired)
            {
                listMessages.Invoke(new UpdateDisplayDelegate(UpdateDisplay), new object[] { message });
            }
            else
            {
                UpdateDisplay(message);
            }
        }

        private void UpdateDisplay(string message)
        {
            listMessages.Items.Add(message);
        }


        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                await HandleMessageTransactionAsync(txtMessage.Text);
            }
            catch
            {
                MessageBox.Show("Something went wrong, try again later!", "Invalid operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnStartStop_Click(object sender, EventArgs e)
        {
            try
            {
                await CreateServerAsync();
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message, "Port already in use", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private async void TxtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (txtMessage.Text == "" || networkStream == null || !(e.KeyChar == (char)13))
            {
                return;
            }

            try
            {
                await HandleMessageTransactionAsync(txtMessage.Text);
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
            txtMessage.Clear();
            txtMessage.Focus();
        }

        private async Task SendMessageAsync(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            await networkStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task CreateServerAsync()
        {
            string IPaddress = txtServerIP.Text;
            int portNumber = StringToInt(txtPort.Text);
            int bufferSize = StringToInt(txtBufferSize.Text);

            if (!ValidateClientPreferences(IPaddress, portNumber, bufferSize))
            {
                return;
            }

            TcpListener tcpListener = new TcpListener(IPAddress.Parse(IPaddress), portNumber);
            tcpListener.Start();

            AddMessage($"[Server] Server started! Accepting users on port {portNumber}");

            btnStartStop.Enabled = false;

            tcpClient = await tcpListener.AcceptTcpClientAsync();

            btnSend.Enabled = true;
            await Task.Run(() => ReceiveData(bufferSize));
        }

        private async void ReceiveData(int bufferSize)
        {
            string message = "";
            byte[] buffer = new byte[bufferSize];

            networkStream = tcpClient.GetStream();
            AddMessage($"[Server] Client has connected!"); //TODO: Change client for username (verzin eigen protocol)

            while (networkStream.CanRead)
            {
                StringBuilder completeMessage = new StringBuilder();

                do
                {
                    try
                    {
                        int readBytes = await networkStream.ReadAsync(buffer, 0, bufferSize);
                        message = Encoding.ASCII.GetString(buffer, 0, readBytes);
                        completeMessage.Append(message);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }
                }
                while (networkStream.DataAvailable);

                if(completeMessage.ToString() == "disconnect")
                {
                    await Task.Run(() => SendMessageAsync(message));
                    break;
                }

                AddMessage(completeMessage.ToString());
            }

            networkStream.Close();
            tcpClient.Close();

            AddMessage("Connection closed!");
        }

        private int StringToInt(string text)
        {
            int number;
            int.TryParse(text, out number);

            return number;
        }

        public bool ValidateIPv4(string ipString)
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
