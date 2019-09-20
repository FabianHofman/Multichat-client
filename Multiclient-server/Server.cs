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

        /// <summary>
        /// Checkt of de lijst met clients een invoke nodig heeft of niet en stuurt die vervolgens door.
        /// </summary>
        /// <param name="message">Het bericht wat toegevoegd moet worden</param>
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

        /// <summary>
        /// Het toevegen aan de lijst met berichten.
        /// </summary>
        /// <param name="message"></param>
        private void UpdateMessageList(string message)
        {
            listMessages.Items.Add(message);
            listMessages.SelectedIndex = listMessages.Items.Count - 1;
        }

        /// <summary>
        /// Checkt of de lijst met clients een invoke nodig heeft of niet en stuurt die vervolgens door.
        /// </summary>
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

        /// <summary>
        /// Het aanpassen van de lijst met verbonden clients.
        /// </summary>
        private void ControlClientList()
        {
            listClients.Items.Clear();
            foreach (TcpClient client in listConnectedClients)
            {
                listClients.Items.Add(client.Client.RemoteEndPoint.ToString());
            }
        }

        /// <summary>
        /// Stuurt een bericht naar alle andere clients (die niet de client is die het bericht verstuurd) die verbonden zijn met de server.
        /// </summary>
        /// <param name="client">De client waarvan het bericht af komt</param>
        /// <param name="type">Het type bericht dat verstuurd moet worden</param>
        /// <param name="username">De gebruikersnaam waarvan het gestuurd is</param>
        /// <param name="message">Het bericht dat verstuurd moet worden</param>
        /// <returns></returns>
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

        /// <summary>
        /// Verstuurd een disconnect bericht naar een gebruiker zonder dit toe te voegen aan de chat.
        /// </summary>
        /// <param name="stream">De NetworkStream waarop het bericht verstuurd moet worden</param>
        /// <param name="type">Het type bericht dat verstuurd moet worden</param>
        /// <param name="username">De gebruikersnaam waarvan het gestuurd is</param>
        /// <param name="message">Het bericht dat verstuurd moet worden</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
        private async Task SendDisconnectMessageAsync(NetworkStream stream, string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);

            await SendMessageOnNetworkAsync(stream, completeMessage);
        }

        /// <summary>
        /// Verstuurd een bericht op de NetworkStream met de ingestelde buffersize
        /// </summary>
        /// <param name="stream">De NetworkStream van iedere gebruiker</param>
        /// <param name="message">Het hele bericht dat gestuurd moet worden op de NetworkStream</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
        private async Task SendMessageOnNetworkAsync(NetworkStream stream, string message)
        {
            int bufferSize = StringToInt(txtBufferSize.Text);

            do
            {
                if (bufferSize > message.Length)
                {
                    bufferSize = message.Length;
                }

                string substring = message.Substring(0, bufferSize);
                message = message.Remove(0, bufferSize);
                byte[] buffer = Encoding.ASCII.GetBytes(substring);
                await stream.WriteAsync(buffer, 0, bufferSize);
            }
            while (message.Length > 0);
            
        }

        /// <summary>
        /// Creëert de server mits er niet al een runt
        /// </summary>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
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

            txtBufferSize.Enabled = false;
            txtPort.Enabled = false;
            txtServerIP.Enabled = false;

            do
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();

                listConnectedClients.Add(client);
                UpdateClientList();
                await Task.Run(() => ReceiveData(client, bufferSize));
            }
            while (started);

            tcpListener.Stop();

            AddMessage("[Server] Listener stopped!");

            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;

            txtBufferSize.Enabled = true;
            txtPort.Enabled = true;
            txtServerIP.Enabled = true;

        }

        /// <summary>
        /// Het stoppen van de server
        /// </summary>
        /// <param name="type">Het type bericht dat verstuurd moet worden</param>
        /// <param name="username">De gebruikersnaam waarvan het gestuurd is</param>
        /// <param name="message">Het bericht dat verstuurd moet worden</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
        private async Task StopServerAsync(string type, string username, string message)
        {
            AddMessage("[Server] Closing...");
            string completeMessage = EncodeMessage(type, username, message);

            foreach (TcpClient user in listConnectedClients.ToList())
            {
                await SendMessageOnNetworkAsync(user.GetStream(), completeMessage);
            }

            started = false;

            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(txtServerIP.Text, StringToInt(txtPort.Text));
            await SendDisconnectMessageAsync(tcpClient.GetStream(), "INFO", username, "DISCONNECT");
            tcpClient.Close();

            btnStopServer.Enabled = false;
            btnStartServer.Enabled = true;
        }

        /// <summary>
        /// In deze functie wordt de data opgehaald die een verbonden gebruiker stuurt
        /// </summary>
        /// <param name="client">De client die verbonden is met de server</param>
        /// <param name="bufferSize">De buffersize die de functie moet gebruiken</param>
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
                        while (completeMessage.ToString().IndexOf("@", 1) < 0);
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

        /// <summary>
        /// Hierin wordt het te versturen "bericht" encode en wordt message markers toegevoegd.
        /// </summary>
        /// <param name="type">Het type bericht dat verstuurd moet worden</param>
        /// <param name="username">De gebruikersnaam waarvan het gestuurd is</param>
        /// <param name="message">Het bericht dat verstuurd moet worden</param>
        /// <returns>Een string met message markers</returns>
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

        /// <summary>
        /// Hierin wordt de juiste stukken gepakt die nodig zijn om het bericht goed te zetten
        /// </summary>
        /// <param name="message">Het bericht dat binnen komt op de NetworkStream</param>
        /// <param name="regex">De regex die uitgevoerd moet worden om alle benodigde informatie te krijgen</param>
        /// <returns>Een deel van de message die nodig is</returns>
        private string FilterProtocol(string message, Regex regex)
        {
            return regex.Match(message).ToString();
        }

        /// <summary>
        /// In de encode message worden teken die verward kunnen worden met de message markers omgezet, dit zal weer terug gezet moeten worden.
        /// </summary>
        /// <param name="str">De string dat gedecode moet worden</param>
        /// <returns>String met daarin de juiste tekens</returns>
        private string DecodeMessage(string str)
        {
            str = Regex.Replace(str, "&#124", "|");
            str = Regex.Replace(str, "&#64", "@");

            return str;
        }

        /// <summary>
        /// Deze functie valideert of het ingegeven IP adres wel goed is. 
        /// </summary>
        /// <param name="ipString">Het IP adres dat gevalideerd moet worden</param>
        /// <returns>Boolean die aan geeft of het IP adres goed is.</returns>
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

        /// <summary>
        /// Valideert de ingevulde gegevens voor het starten van een server
        /// </summary>
        /// <param name="IPaddress">IP adres waarop de server runt</param>
        /// <param name="portNumber">Poort waarop de server runt</param>
        /// <param name="bufferSize">Buffer size die de server gaat gebruiken</param>
        /// <returns>Boolean die aan geeft of de ingevulde gegevens goed zijn</returns>
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

            if (bufferSize <= 0)
            {
                MessageBox.Show("An invalid amount of buffer size has been given! Try something else.", "Invalid amount of Buffer Size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Haalt de int waarde uit een string
        /// </summary>
        /// <param name="text">String waaruit de int gehaald moet worden</param>
        /// <returns>Int uit de string</returns>
        private int StringToInt(string text)
        {
            int number;
            int.TryParse(text, out number);

            return number;
        }
    }
}
