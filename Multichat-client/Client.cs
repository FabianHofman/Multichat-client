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

        // De delegate die nodig is als een update ui request wordt gedaan van een non-ui thread.
        protected delegate void UpdateDisplayDelegate(string message);

        public Client()
        {
            InitializeComponent();
            btnSendMessage.Enabled = false;
            btnDisconnectFromServer.Enabled = false;
            txtMessageToBeSend.Enabled = false;
        }

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

                // disable or enable buttons and text fields
                btnConnectWithServer.Enabled = true;
                btnSendMessage.Enabled = false;
                btnDisconnectFromServer.Enabled = false;
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

        /// <summary>
        /// Checkt of de lijst met berichten een invoke nodig heeft of niet en stuurt die vervolgens door.
        /// </summary>
        /// <param name="message">Het bericht wat toegevoegd moet worden</param>
        private void AddMessage(string message)
        {
            if (listChats.InvokeRequired)
            {
                listChats.Invoke(new UpdateDisplayDelegate(UpdateListChats), new object[] { message });
            }
            else
            {
                UpdateListChats(message);
            }
        }

        /// <summary>
        /// Voegt een bericht toe aan de lijst met berichten
        /// </summary>
        /// <param name="message">Het bericht dat toegevoegd moet worden</param>
        private void UpdateListChats(string message)
        {
            listChats.Items.Add(message);
            listChats.SelectedIndex = listChats.Items.Count - 1;
        }

        /// <summary>
        /// Stuurt een bericht op het netwerk en voegt dit toe aan de lijst met berichten
        /// </summary>
        /// <param name="type">Het type bericht dat verstuurd moet worden</param>
        /// <param name="username">De gebruikersnaam waarvan het gestuurd is</param>
        /// <param name="message">Het bericht dat verstuurd moet worden</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
        private async Task SendMessageAsync(string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);

            await SendMessageOnNetworkAsync(completeMessage);

            message = DecodeMessage(message);

            AddMessage($"{username}: {message}");
            txtMessageToBeSend.Clear();
            txtMessageToBeSend.Focus();
        }

        /// <summary>
        /// Stuurt een disconnect bericht naar de server
        /// </summary>
        /// <param name="type">Het type bericht dat verstuurd moet worden</param>
        /// <param name="username">De gebruikersnaam waarvan het gestuurd is</param>
        /// <param name="message">Het bericht dat verstuurd moet worden</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
        private async Task SendDisconnectMessageAsync(string type, string username, string message)
        {
            string completeMessage = EncodeMessage(type, username, message);
            await SendMessageOnNetworkAsync(completeMessage);
        }

        /// <summary>
        /// Verstuurd een bericht een bericht in stukken op het netwerk (afhankelijk van de gekozen buffer size)
        /// </summary>
        /// <param name="message">Het hele bericht dat gestuurd moet worden op de NetworkStream</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
        private async Task SendMessageOnNetworkAsync(string message)
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
                await networkStream.WriteAsync(buffer, 0, bufferSize);
            }
            while (message.Length > 0);
        }

        /// <summary>
        /// De verbinding verbreken met de server waarmee je verbonden bent
        /// </summary>
        /// <param name="username">De gebruikersnaam die de gebruiker in gebruik heeft</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
        private async Task DisconnectFromServerAsync(string username)
        {
            await SendDisconnectMessageAsync("INFO", username, "DISCONNECTING");
            // disable or enable buttons and text fields
            btnConnectWithServer.Enabled = true;
            btnSendMessage.Enabled = false;
            btnDisconnectFromServer.Enabled = false;
            txtUsername.Enabled = true;
            txtChatServerIP.Enabled = true;
            txtChatServerPort.Enabled = true;
            txtMessageToBeSend.Enabled = false;
            txtBufferSize.Enabled = true;
        }

        /// <summary>
        /// Hierin wordt een verbinding gemaakt met de server nadat alle checks gedaan zijn.
        /// </summary>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
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
                    // disable or enable buttons and text fields
                    btnConnectWithServer.Enabled = false;
                    btnSendMessage.Enabled = true;
                    btnDisconnectFromServer.Enabled = true;
                    txtUsername.Enabled = false;
                    txtChatServerIP.Enabled = false;
                    txtChatServerPort.Enabled = false;
                    txtMessageToBeSend.Enabled = true;
                    txtBufferSize.Enabled = false;

                    await Task.Run(() => ReceiveData(bufferSize));
                    // disable or enable buttons and text fields
                    btnConnectWithServer.Enabled = true;
                    btnSendMessage.Enabled = false;
                    btnDisconnectFromServer.Enabled = false;
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

        /// <summary>
        /// In deze functie wordt de data opgehaald die een verbonden server stuurt
        /// </summary>
        /// <param name="bufferSize">De buffersize die de functie moet gebruiken</param>
        /// <returns>Task die op een aparte thread gerunt wordt</returns>
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
                    AddMessage($"{decodedUsername}: {decodedMessage}");
                }
            }

            networkStream.Close();
            tcpClient.Close();

            AddMessage("Connection closed");
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

        /// <summary>
        /// Valideert de ingevulde gegevens voor het starten van een client
        /// </summary>
        /// <param name="username">De gebruikersnaam die de gebruiker wil gebruiken</param>
        /// <param name="IPaddress">IP adres waarop de server runt</param>
        /// <param name="portNumber">Poort waarop de server runt</param>
        /// <param name="bufferSize">Buffer size die de server gaat gebruiken</param>
        /// <returns>Boolean die aan geeft of de ingevulde gegevens goed zijn</returns>
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
