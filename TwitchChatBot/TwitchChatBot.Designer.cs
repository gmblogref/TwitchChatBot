namespace TwitchChatBot
{
    partial class TwitchChatBot
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            buttonStartBot = new Button();
            tabControlChatBot = new TabControl();
            tabPageMainChat = new TabPage();
            labelChatBox = new Label();
            textBoxStreamChat = new TextBox();
            tabPageLogging = new TabPage();
            textBoxLogging = new TextBox();
            labelLogging = new Label();
            tabTesting = new TabPage();
            tabControlChatBot.SuspendLayout();
            tabPageMainChat.SuspendLayout();
            tabPageLogging.SuspendLayout();
            SuspendLayout();
            // 
            // buttonStartBot
            // 
            buttonStartBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonStartBot.BackColor = Color.Green;
            buttonStartBot.Location = new Point(667, 407);
            buttonStartBot.Name = "buttonStartBot";
            buttonStartBot.Size = new Size(94, 29);
            buttonStartBot.TabIndex = 0;
            buttonStartBot.Text = "Start Bot";
            buttonStartBot.UseVisualStyleBackColor = false;
            buttonStartBot.EnabledChanged += buttonStartBot_EnabledChanged;
            buttonStartBot.Click += startBotButton_Click;
            // 
            // tabControlChatBot
            // 
            tabControlChatBot.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControlChatBot.Controls.Add(tabPageMainChat);
            tabControlChatBot.Controls.Add(tabPageLogging);
            tabControlChatBot.Controls.Add(tabTesting);
            tabControlChatBot.Location = new Point(12, 12);
            tabControlChatBot.Name = "tabControlChatBot";
            tabControlChatBot.SelectedIndex = 0;
            tabControlChatBot.Size = new Size(783, 475);
            tabControlChatBot.TabIndex = 2;
            // 
            // tabPageMainChat
            // 
            tabPageMainChat.Controls.Add(labelChatBox);
            tabPageMainChat.Controls.Add(textBoxStreamChat);
            tabPageMainChat.Controls.Add(buttonStartBot);
            tabPageMainChat.Location = new Point(4, 29);
            tabPageMainChat.Name = "tabPageMainChat";
            tabPageMainChat.Padding = new Padding(3);
            tabPageMainChat.Size = new Size(775, 442);
            tabPageMainChat.TabIndex = 0;
            tabPageMainChat.Text = "Main Chat Bot Tab";
            tabPageMainChat.UseVisualStyleBackColor = true;
            // 
            // labelChatBox
            // 
            labelChatBox.AutoSize = true;
            labelChatBox.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelChatBox.Location = new Point(6, 3);
            labelChatBox.Name = "labelChatBox";
            labelChatBox.Size = new Size(170, 32);
            labelChatBox.TabIndex = 3;
            labelChatBox.Text = "Stream Chat";
            // 
            // textBoxStreamChat
            // 
            textBoxStreamChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxStreamChat.BackColor = Color.Black;
            textBoxStreamChat.Font = new Font("Times New Roman", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxStreamChat.ForeColor = Color.White;
            textBoxStreamChat.Location = new Point(6, 38);
            textBoxStreamChat.MinimumSize = new Size(755, 363);
            textBoxStreamChat.Multiline = true;
            textBoxStreamChat.Name = "textBoxStreamChat";
            textBoxStreamChat.Size = new Size(755, 363);
            textBoxStreamChat.TabIndex = 2;
            // 
            // tabPageLogging
            // 
            tabPageLogging.BorderStyle = BorderStyle.Fixed3D;
            tabPageLogging.Controls.Add(textBoxLogging);
            tabPageLogging.Controls.Add(labelLogging);
            tabPageLogging.Location = new Point(4, 29);
            tabPageLogging.Name = "tabPageLogging";
            tabPageLogging.Padding = new Padding(3);
            tabPageLogging.Size = new Size(775, 442);
            tabPageLogging.TabIndex = 1;
            tabPageLogging.Text = "Logging Tab";
            tabPageLogging.UseVisualStyleBackColor = true;
            // 
            // textBoxLogging
            // 
            textBoxLogging.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxLogging.BackColor = Color.Black;
            textBoxLogging.Font = new Font("Times New Roman", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxLogging.ForeColor = Color.White;
            textBoxLogging.Location = new Point(6, 38);
            textBoxLogging.MinimumSize = new Size(654, 349);
            textBoxLogging.Multiline = true;
            textBoxLogging.Name = "textBoxLogging";
            textBoxLogging.Size = new Size(762, 398);
            textBoxLogging.TabIndex = 1;
            // 
            // labelLogging
            // 
            labelLogging.AutoSize = true;
            labelLogging.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelLogging.Location = new Point(6, 3);
            labelLogging.Name = "labelLogging";
            labelLogging.Size = new Size(265, 32);
            labelLogging.TabIndex = 0;
            labelLogging.Text = "Logging Information";
            // 
            // tabTesting
            // 
            tabTesting.BackColor = Color.Transparent;
            tabTesting.ForeColor = SystemColors.ControlText;
            tabTesting.Location = new Point(4, 29);
            tabTesting.Name = "tabTesting";
            tabTesting.Padding = new Padding(3);
            tabTesting.Size = new Size(775, 442);
            tabTesting.TabIndex = 2;
            tabTesting.Text = "Testing Tab";
            // 
            // TwitchChatBot
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(807, 499);
            Controls.Add(tabControlChatBot);
            MinimumSize = new Size(825, 546);
            Name = "TwitchChatBot";
            Text = "Twitch Chat Bot";
            tabControlChatBot.ResumeLayout(false);
            tabPageMainChat.ResumeLayout(false);
            tabPageMainChat.PerformLayout();
            tabPageLogging.ResumeLayout(false);
            tabPageLogging.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button buttonStartBot;
        private TabControl tabControlChatBot;
        private TabPage tabPageMainChat;
        private TabPage tabPageLogging;
        private TextBox textBoxStreamChat;
        private Label labelChatBox;
        private TextBox textBoxLogging;
        private Label labelLogging;
        private TabPage tabTesting;
    }
}
