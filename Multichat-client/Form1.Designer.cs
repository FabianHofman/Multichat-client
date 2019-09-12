namespace Multichat_client
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtChatServerIP = new System.Windows.Forms.TextBox();
            this.btnConnectWithServer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnListen = new System.Windows.Forms.Button();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.txtMessageToBeSend = new System.Windows.Forms.TextBox();
            this.listChats = new System.Windows.Forms.ListBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.txtChatServerIP);
            this.groupBox1.Controls.Add(this.btnConnectWithServer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(635, 59);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(184, 163);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connect to Server";
            // 
            // txtChatServerIP
            // 
            this.txtChatServerIP.Location = new System.Drawing.Point(29, 66);
            this.txtChatServerIP.Margin = new System.Windows.Forms.Padding(2);
            this.txtChatServerIP.Name = "txtChatServerIP";
            this.txtChatServerIP.Size = new System.Drawing.Size(141, 23);
            this.txtChatServerIP.TabIndex = 2;
            this.txtChatServerIP.Text = "127.0.0.1";
            // 
            // btnConnectWithServer
            // 
            this.btnConnectWithServer.Location = new System.Drawing.Point(29, 116);
            this.btnConnectWithServer.Margin = new System.Windows.Forms.Padding(2);
            this.btnConnectWithServer.Name = "btnConnectWithServer";
            this.btnConnectWithServer.Size = new System.Drawing.Size(139, 27);
            this.btnConnectWithServer.TabIndex = 1;
            this.btnConnectWithServer.Text = "Connect";
            this.btnConnectWithServer.UseVisualStyleBackColor = true;
            this.btnConnectWithServer.Click += new System.EventHandler(this.BtnConnectWithServer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Chat Server IP";
            // 
            // btnListen
            // 
            this.btnListen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnListen.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnListen.Location = new System.Drawing.Point(636, 16);
            this.btnListen.Margin = new System.Windows.Forms.Padding(2);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(184, 29);
            this.btnListen.TabIndex = 6;
            this.btnListen.Text = "Listen";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.BtnListen_Click);
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSendMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendMessage.Location = new System.Drawing.Point(545, 454);
            this.btnSendMessage.Margin = new System.Windows.Forms.Padding(2);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(76, 27);
            this.btnSendMessage.TabIndex = 9;
            this.btnSendMessage.Text = "Send";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.BtnSendMessage_Click);
            // 
            // txtMessageToBeSend
            // 
            this.txtMessageToBeSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessageToBeSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessageToBeSend.Location = new System.Drawing.Point(11, 456);
            this.txtMessageToBeSend.Margin = new System.Windows.Forms.Padding(2);
            this.txtMessageToBeSend.Name = "txtMessageToBeSend";
            this.txtMessageToBeSend.Size = new System.Drawing.Size(530, 23);
            this.txtMessageToBeSend.TabIndex = 8;
            // 
            // listChats
            // 
            this.listChats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listChats.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listChats.FormattingEnabled = true;
            this.listChats.ItemHeight = 16;
            this.listChats.Location = new System.Drawing.Point(11, 11);
            this.listChats.Margin = new System.Windows.Forms.Padding(2);
            this.listChats.Name = "listChats";
            this.listChats.Size = new System.Drawing.Size(610, 420);
            this.listChats.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(816, 489);
            this.ClientSize = new System.Drawing.Size(831, 490);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnListen);
            this.Controls.Add(this.btnSendMessage);
            this.Controls.Add(this.txtMessageToBeSend);
            this.Controls.Add(this.listChats);
            this.MinimumSize = new System.Drawing.Size(847, 529);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtChatServerIP;
        private System.Windows.Forms.Button btnConnectWithServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.TextBox txtMessageToBeSend;
        private System.Windows.Forms.ListBox listChats;
    }
}

