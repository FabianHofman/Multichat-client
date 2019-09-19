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

namespace Mutliclient_server
{
    public partial class Server : Form
    {

        protected delegate void UpdateMessageListDelegate(string message);
        protected delegate void UpdateClientListDelegate();

        List<TcpClient> listConnectedClients = new List<TcpClient>();
        bool started = false;

        public Server()
        {
            InitializeComponent();
            btnStopServer.Enabled = false;
        }

        // Everything related to user interface
        private async void BtnStartServer_Click(object sender, EventArgs e)
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

        private async void BtnStopServer_Click(object sender, EventArgs e)
        {
            await StopServerAsync("INFO", "Server", "DISCONNECTING");
        }

        private void AddMessage(string message)
        {
            if (listMessages.InvokeRequired)
            {
                listMessages.Invoke(new UpdateMessageListDelegate(UpdateMessageList), new object[] { message });
            }
            else
            {
                UpdateMessageList(message);
            }
        }

        private void UpdateMessageList(string message)
        {
            listMessages.Items.Add(message);
            listMessages.SelectedIndex = listMessages.Items.Count - 1;
        }

        private void UpdateClientList()
        {
            if (listClients.InvokeRequired)
            {
                listClients.Invoke(new UpdateClientListDelegate(ControlClientList));
            }
            else
            {
                ControlClientList();
            }
        }

        private void ControlClientList()
        {
            listClients.Items.Clear();
            foreach (TcpClient client in listConnectedClients)
            {
                listClients.Items.Add(client.Client.RemoteEndPoint.ToString());
            }
        }

        // Everything related to messages
        private async Task BroadcastMessage(TcpClient client, string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);

            foreach (TcpClient user in listConnectedClients)
            {
                if (user.Client.RemoteEndPoint != client.Client.RemoteEndPoint)
                {
                    await SendMessageOnNetworkAsync(user.GetStream(), completeMessage);
                }
            }
        }

        private async Task SendDisconnectMessageAsync(NetworkStream stream, string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);

            await SendMessageOnNetworkAsync(stream, completeMessage);
        }

        private async Task SendMessageOnNetworkAsync(NetworkStream stream, string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        // Eveything related to the buttons
        private async Task CreateServerAsync()
        {
            string IPaddress = txtServerIP.Text;
            int portNumber = StringToInt(txtPort.Text);
            int bufferSize = StringToInt(txtBufferSize.Text);

            if (!ValidateServerPreferences(IPaddress, portNumber, bufferSize))
            {
                return;
            }

            TcpListener tcpListener = new TcpListener(IPAddress.Parse(IPaddress), portNumber);
            tcpListener.Start();

            started = true;

            AddMessage($"[Server] Server started! Accepting users on port {portNumber}");

            btnStartServer.Enabled = false;
            btnStopServer.Enabled = true;

            do
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();

                listConnectedClients.Add(client);
                UpdateClientList();
                await Task.Run(() => ReceiveData(client, bufferSize));
            }
            while (started);

            tcpListener.Stop();

            AddMessage("[Server] Closed!");

        }

        private async Task StopServerAsync(string type, string username, string message)
        {
            AddMessage("[Server] Closing...");
            string completeMessage = EncodeMessage(type, username, message);

            foreach (TcpClient user in listConnectedClients)
            {
                await SendMessageOnNetworkAsync(user.GetStream(), completeMessage);
            }

            started = false;

            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", StringToInt(txtPort.Text));
            await SendDisconnectMessageAsync(tcpClient.GetStream(), "INFO", username, "DISCONNECTING");
            tcpClient.Close();

            btnStopServer.Enabled = false;
            btnStartServer.Enabled = true;
        }

        // Everything related to recieving data.
        private async void ReceiveData(TcpClient client, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];

            NetworkStream stream = client.GetStream();
            AddMessage($"[Server] A client has connected!");

            while (stream.CanRead)
            {
                StringBuilder completeMessage = new StringBuilder();

                do
                {
                    try
                    {
                        do
                        {
                            int readBytes = await stream.ReadAsync(buffer, 0, bufferSize);
                            string message = Encoding.ASCII.GetString(buffer, 0, readBytes);
                            completeMessage.Append(message);
                        }
                        while (completeMessage.ToString().IndexOf("@") < 0);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message, "No connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        completeMessage.Clear();
                        completeMessage.Append("@INFO||unknown||DISCONNECT@");
                        break;
                    }
                }
                while (stream.DataAvailable);

                string decodedType = FilterProtocol(completeMessage.ToString(), new Regex(@"(?<=\@)(.*?)(?=\|)"));
                string decodedUsername = FilterProtocol(completeMessage.ToString(), new Regex(@"(?<=\|{2})(.*?)(?=\|{2})"));
                string decodedMessage = DecodeMessage(FilterProtocol(FilterProtocol(completeMessage.ToString(), new Regex(@"\|(?:.(?!\|))+$")), new Regex(@"(?<=\|{2})(.*?)(?=\@)")));

                if (decodedType == "INFO" && decodedMessage == "DISCONNECTING")
                {
                    await SendDisconnectMessageAsync(stream, "INFO", decodedUsername, "DISCONNECT");
                    break;
                }

                if (decodedType == "INFO" && decodedMessage == "DISCONNECT")
                {
                    break;
                }

                if (decodedType == "MESSAGE")
                {
                    await BroadcastMessage(client, decodedType, decodedUsername, decodedMessage);
                    AddMessage($"{decodedUsername}: {decodedMessage}");
                }
            }

            stream.Close();
            client.Close();

            listConnectedClients.RemoveAll(user => !user.Connected);
            UpdateClientList();

            AddMessage($"[Server] Connection with a client has closed!");
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

        private bool ValidateServerPreferences(string IPaddress, int portNumber, int bufferSize)
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

            if (bufferSize <= 1)
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
