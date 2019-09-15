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


        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;

            byte[] buffer = Encoding.ASCII.GetBytes(message);
            networkStream.Write(buffer, 0, buffer.Length);

            AddMessage(message);
            txtMessage.Clear();
            txtMessage.Focus();
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

        private void TxtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                string message = txtMessage.Text;

                if (message == "" || networkStream == null)
                {
                    return;
                }


                byte[] buffer = Encoding.ASCII.GetBytes(message);
                networkStream.Write(buffer, 0, buffer.Length);

                AddMessage(message);
                txtMessage.Clear();
                txtMessage.Focus();
            }
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
                        message = "bye";
                        break;
                    }
                }
                while (networkStream.DataAvailable);

                if (message == "bye")
                {
                    break;
                }

                AddMessage(completeMessage.ToString());
            }

            networkStream.Close();
            tcpClient.Close();

            AddMessage("Connection closed!");
        }

        private async Task CreateServerAsync()
        {
            int portNumber = StringToInt(txtPort.Text);
            int bufferSize = StringToInt(txtBufferSize.Text);

            if (!ValidateIPv4(txtServerIP.Text))
            {
                MessageBox.Show("An invalid IP address has been given! Try another IP address", "Invalid IP address", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!(portNumber >= 1024 && portNumber <= 65535))
            {
                MessageBox.Show("Port had an invalid value or is not within the range of 1024 - 65535", "Invalid Port number", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (bufferSize < 1)
            {
                MessageBox.Show("An invalid amount of buffer size has been given! Try something else.", "Invalid amount of Buffer Size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            TcpListener tcpListener = new TcpListener(IPAddress.Parse(txtServerIP.Text), portNumber);
            tcpListener.Start();

            AddMessage($"[Server] Server started! Accepting users on port {portNumber}");

            btnStartStop.Enabled = false;

            tcpClient = await tcpListener.AcceptTcpClientAsync();

            btnSend.Enabled = true;
            await Task.Run(() => ReceiveData(bufferSize));
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
    }
}
