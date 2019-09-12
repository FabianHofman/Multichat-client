using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Multichat_client
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        Thread thread;

        protected delegate void UpdateDisplayDelegate(string message);

        public Form1()
        {
            InitializeComponent();
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

        private void BtnListen_Click(object sender, EventArgs e)
        {

            AddMessage("Listening for client.");

            TcpListener tcpListener = new TcpListener(IPAddress.Any, 9000);
            tcpListener.Start();

            tcpClient = tcpListener.AcceptTcpClient();
            thread = new Thread(new ThreadStart(ReceiveData));
            thread.Start();
        }

        private void ReceiveData()
        {
            int bufferSize = 1024;
            string message = "";
            byte[] buffer = new byte[bufferSize];

            networkStream = tcpClient.GetStream();
            AddMessage("Connected!");

            while (true)
            {
                int readBytes = networkStream.Read(buffer, 0, bufferSize);
                message = Encoding.ASCII.GetString(buffer, 0, readBytes);

                btnListen.Enabled = false;
                btnConnectWithServer.Enabled = false;

                if (message == "bye")
                {
                    btnListen.Enabled = true;
                    btnConnectWithServer.Enabled = true;
                    break;
                }
                    

                AddMessage(message);
            }

            buffer = Encoding.ASCII.GetBytes("bye");
            networkStream.Write(buffer, 0, buffer.Length);

            networkStream.Close();
            tcpClient.Close();

            AddMessage("Connection closed");
        }

        private void BtnConnectWithServer_Click(object sender, EventArgs e)
        {
            AddMessage("Connecting...");

            tcpClient = new TcpClient(txtChatServerIP.Text, 9000);
            thread = new Thread(new ThreadStart(ReceiveData));
            thread.Start();
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
    }
}
