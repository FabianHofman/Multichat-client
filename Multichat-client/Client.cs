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
        Thread thread;

        protected delegate void UpdateDisplayDelegate(string message);

        public Client()
        {
            InitializeComponent();
            btnSendMessage.Enabled = false;
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

        private async void ReceiveData(int bufferSize)
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

            AddMessage("Connection closed");
        }

        private async void BtnConnectWithServer_Click(object sender, EventArgs e)
        {
            int portNumber = StringToInt(txtChatServerPort.Text);
            int bufferSize = StringToInt(txtBufferSize.Text);

            if (!ValidateIPv4(txtChatServerIP.Text))
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

            AddMessage("Connecting...");
            try
            {
                tcpClient = new TcpClient(txtChatServerIP.Text, portNumber);
                await Task.Run(() => ReceiveData(bufferSize));
                btnConnectWithServer.Enabled = false;
            }
            catch (SocketException ex)
            {
                AddMessage("No connection possible, try again later!");
                MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            btnSendMessage.Enabled = true;
        }

        private void BtnSendMessage_Click(object sender, EventArgs e)
        {
            string message = txtMessageToBeSend.Text;

            byte[] buffer = Encoding.ASCII.GetBytes(message);
            networkStream.Write(buffer, 0, buffer.Length);

            AddMessage(message);
            txtMessageToBeSend.Clear();
            txtMessageToBeSend.Focus();
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
