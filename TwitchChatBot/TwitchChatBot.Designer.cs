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
            listBoxCurrentViewers = new ListBox();
            labelViewers = new Label();
            richTextBoxStreamChat = new RichTextBox();
            labelChatBox = new Label();
            tabPageLogging = new TabPage();
            textBoxLogging = new TextBox();
            labelLogging = new Label();
            tabTesting = new TabPage();
            buttonClearFirst = new Button();
            buttonTestFollow = new Button();
            textFirstChatter = new TextBox();
            buttonTestFirstChat = new Button();
            textCommand = new TextBox();
            buttonTestCommand = new Button();
            textChannelPoint = new TextBox();
            buttonTestChannelPoint = new Button();
            textMysterySubs = new TextBox();
            buttonTestMysteryGift = new Button();
            textBits = new TextBox();
            buttonTestCheer = new Button();
            buttonTestHypeTrain = new Button();
            buttonTestSubGift = new Button();
            buttonTestReSub = new Button();
            buttonTestRaid = new Button();
            buttonTestSub = new Button();
            tabControlChatBot.SuspendLayout();
            tabPageMainChat.SuspendLayout();
            tabPageLogging.SuspendLayout();
            tabTesting.SuspendLayout();
            SuspendLayout();
            // 
            // buttonStartBot
            // 
            buttonStartBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonStartBot.BackColor = Color.Green;
            buttonStartBot.Location = new Point(967, 463);
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
            tabControlChatBot.Size = new Size(1049, 453);
            tabControlChatBot.TabIndex = 2;
            // 
            // tabPageMainChat
            // 
            tabPageMainChat.Controls.Add(listBoxCurrentViewers);
            tabPageMainChat.Controls.Add(labelViewers);
            tabPageMainChat.Controls.Add(richTextBoxStreamChat);
            tabPageMainChat.Controls.Add(labelChatBox);
            tabPageMainChat.Location = new Point(4, 29);
            tabPageMainChat.Name = "tabPageMainChat";
            tabPageMainChat.Padding = new Padding(3);
            tabPageMainChat.Size = new Size(1041, 420);
            tabPageMainChat.TabIndex = 0;
            tabPageMainChat.Text = "Main Chat Bot Tab";
            tabPageMainChat.UseVisualStyleBackColor = true;
            // 
            // listBoxCurrentViewers
            // 
            listBoxCurrentViewers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxCurrentViewers.BackColor = Color.Black;
            listBoxCurrentViewers.ForeColor = Color.White;
            listBoxCurrentViewers.FormattingEnabled = true;
            listBoxCurrentViewers.Location = new Point(784, 38);
            listBoxCurrentViewers.MaximumSize = new Size(238, 364);
            listBoxCurrentViewers.Name = "listBoxCurrentViewers";
            listBoxCurrentViewers.SelectionMode = SelectionMode.None;
            listBoxCurrentViewers.Size = new Size(238, 364);
            listBoxCurrentViewers.TabIndex = 6;
            // 
            // labelViewers
            // 
            labelViewers.AutoSize = true;
            labelViewers.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelViewers.Location = new Point(775, 3);
            labelViewers.Name = "labelViewers";
            labelViewers.Size = new Size(219, 32);
            labelViewers.TabIndex = 5;
            labelViewers.Text = "Current Viewers";
            // 
            // richTextBoxStreamChat
            // 
            richTextBoxStreamChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            richTextBoxStreamChat.BackColor = Color.Black;
            richTextBoxStreamChat.Font = new Font("Times New Roman", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            richTextBoxStreamChat.ForeColor = Color.White;
            richTextBoxStreamChat.Location = new Point(6, 38);
            richTextBoxStreamChat.MinimumSize = new Size(763, 363);
            richTextBoxStreamChat.Name = "richTextBoxStreamChat";
            richTextBoxStreamChat.Size = new Size(763, 363);
            richTextBoxStreamChat.TabIndex = 4;
            richTextBoxStreamChat.Text = "";
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
            // tabPageLogging
            // 
            tabPageLogging.BorderStyle = BorderStyle.Fixed3D;
            tabPageLogging.Controls.Add(textBoxLogging);
            tabPageLogging.Controls.Add(labelLogging);
            tabPageLogging.Location = new Point(4, 29);
            tabPageLogging.Name = "tabPageLogging";
            tabPageLogging.Padding = new Padding(3);
            tabPageLogging.Size = new Size(1041, 420);
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
            textBoxLogging.ScrollBars = ScrollBars.Both;
            textBoxLogging.Size = new Size(762, 372);
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
            tabTesting.Controls.Add(buttonClearFirst);
            tabTesting.Controls.Add(buttonTestFollow);
            tabTesting.Controls.Add(textFirstChatter);
            tabTesting.Controls.Add(buttonTestFirstChat);
            tabTesting.Controls.Add(textCommand);
            tabTesting.Controls.Add(buttonTestCommand);
            tabTesting.Controls.Add(textChannelPoint);
            tabTesting.Controls.Add(buttonTestChannelPoint);
            tabTesting.Controls.Add(textMysterySubs);
            tabTesting.Controls.Add(buttonTestMysteryGift);
            tabTesting.Controls.Add(textBits);
            tabTesting.Controls.Add(buttonTestCheer);
            tabTesting.Controls.Add(buttonTestHypeTrain);
            tabTesting.Controls.Add(buttonTestSubGift);
            tabTesting.Controls.Add(buttonTestReSub);
            tabTesting.Controls.Add(buttonTestRaid);
            tabTesting.Controls.Add(buttonTestSub);
            tabTesting.ForeColor = SystemColors.ControlText;
            tabTesting.Location = new Point(4, 29);
            tabTesting.Name = "tabTesting";
            tabTesting.Padding = new Padding(3);
            tabTesting.Size = new Size(1041, 420);
            tabTesting.TabIndex = 2;
            tabTesting.Text = "Testing Tab";
            // 
            // buttonClearFirst
            // 
            buttonClearFirst.Location = new Point(90, 273);
            buttonClearFirst.Name = "buttonClearFirst";
            buttonClearFirst.Size = new Size(152, 29);
            buttonClearFirst.TabIndex = 16;
            buttonClearFirst.Text = "Clear First Chatters";
            buttonClearFirst.UseVisualStyleBackColor = true;
            buttonClearFirst.Click += buttonClearFirst_Click;
            // 
            // buttonTestFollow
            // 
            buttonTestFollow.Location = new Point(517, 6);
            buttonTestFollow.Name = "buttonTestFollow";
            buttonTestFollow.Size = new Size(94, 29);
            buttonTestFollow.TabIndex = 15;
            buttonTestFollow.Text = "Test Follow";
            buttonTestFollow.UseVisualStyleBackColor = true;
            buttonTestFollow.Click += buttonTestFollow_Click;
            // 
            // textFirstChatter
            // 
            textFirstChatter.Location = new Point(175, 213);
            textFirstChatter.Name = "textFirstChatter";
            textFirstChatter.Size = new Size(125, 27);
            textFirstChatter.TabIndex = 14;
            textFirstChatter.KeyDown += textFirstChatter_KeyDown;
            // 
            // buttonTestFirstChat
            // 
            buttonTestFirstChat.Location = new Point(6, 213);
            buttonTestFirstChat.Name = "buttonTestFirstChat";
            buttonTestFirstChat.Size = new Size(138, 29);
            buttonTestFirstChat.TabIndex = 13;
            buttonTestFirstChat.Text = "Test First Chat";
            buttonTestFirstChat.UseVisualStyleBackColor = true;
            buttonTestFirstChat.Click += buttonTestFirstChat_Click;
            // 
            // textCommand
            // 
            textCommand.Location = new Point(175, 180);
            textCommand.Name = "textCommand";
            textCommand.Size = new Size(125, 27);
            textCommand.TabIndex = 12;
            textCommand.KeyDown += textCommand_KeyDown;
            // 
            // buttonTestCommand
            // 
            buttonTestCommand.Location = new Point(6, 182);
            buttonTestCommand.Name = "buttonTestCommand";
            buttonTestCommand.Size = new Size(138, 29);
            buttonTestCommand.TabIndex = 11;
            buttonTestCommand.Text = "Test Command";
            buttonTestCommand.UseVisualStyleBackColor = true;
            buttonTestCommand.Click += buttonTestCommand_Click;
            // 
            // textChannelPoint
            // 
            textChannelPoint.Location = new Point(175, 147);
            textChannelPoint.Name = "textChannelPoint";
            textChannelPoint.Size = new Size(125, 27);
            textChannelPoint.TabIndex = 10;
            textChannelPoint.KeyDown += textChannelPoint_KeyDown;
            // 
            // buttonTestChannelPoint
            // 
            buttonTestChannelPoint.Location = new Point(6, 147);
            buttonTestChannelPoint.Name = "buttonTestChannelPoint";
            buttonTestChannelPoint.Size = new Size(138, 29);
            buttonTestChannelPoint.TabIndex = 9;
            buttonTestChannelPoint.Text = "Test Channel Point";
            buttonTestChannelPoint.UseVisualStyleBackColor = true;
            buttonTestChannelPoint.Click += buttonTestChannelPoint_Click;
            // 
            // textMysterySubs
            // 
            textMysterySubs.Location = new Point(175, 114);
            textMysterySubs.Name = "textMysterySubs";
            textMysterySubs.Size = new Size(125, 27);
            textMysterySubs.TabIndex = 8;
            textMysterySubs.KeyDown += textMysterySubs_KeyDown;
            // 
            // buttonTestMysteryGift
            // 
            buttonTestMysteryGift.Location = new Point(6, 114);
            buttonTestMysteryGift.Name = "buttonTestMysteryGift";
            buttonTestMysteryGift.Size = new Size(138, 29);
            buttonTestMysteryGift.TabIndex = 7;
            buttonTestMysteryGift.Text = "Test Mystery Gift";
            buttonTestMysteryGift.UseVisualStyleBackColor = true;
            buttonTestMysteryGift.Click += buttonTestMysteryGift_Click;
            // 
            // textBits
            // 
            textBits.Location = new Point(175, 81);
            textBits.Name = "textBits";
            textBits.Size = new Size(125, 27);
            textBits.TabIndex = 6;
            textBits.KeyDown += textBits_KeyDown;
            // 
            // buttonTestCheer
            // 
            buttonTestCheer.Location = new Point(6, 78);
            buttonTestCheer.Name = "buttonTestCheer";
            buttonTestCheer.Size = new Size(138, 29);
            buttonTestCheer.TabIndex = 5;
            buttonTestCheer.Text = "Test Cheer";
            buttonTestCheer.UseVisualStyleBackColor = true;
            buttonTestCheer.Click += buttonTestCheer_Click;
            // 
            // buttonTestHypeTrain
            // 
            buttonTestHypeTrain.Location = new Point(417, 6);
            buttonTestHypeTrain.Name = "buttonTestHypeTrain";
            buttonTestHypeTrain.Size = new Size(94, 29);
            buttonTestHypeTrain.TabIndex = 4;
            buttonTestHypeTrain.Text = "Hype Train";
            buttonTestHypeTrain.UseVisualStyleBackColor = true;
            buttonTestHypeTrain.Click += buttonTestHypeTrain_Click;
            // 
            // buttonTestSubGift
            // 
            buttonTestSubGift.Location = new Point(306, 6);
            buttonTestSubGift.Name = "buttonTestSubGift";
            buttonTestSubGift.Size = new Size(105, 29);
            buttonTestSubGift.TabIndex = 3;
            buttonTestSubGift.Text = "Test SubGift";
            buttonTestSubGift.UseVisualStyleBackColor = true;
            buttonTestSubGift.Click += buttonTestSubGift_Click;
            // 
            // buttonTestReSub
            // 
            buttonTestReSub.Location = new Point(206, 6);
            buttonTestReSub.Name = "buttonTestReSub";
            buttonTestReSub.Size = new Size(94, 29);
            buttonTestReSub.TabIndex = 2;
            buttonTestReSub.Text = "Test ReSub";
            buttonTestReSub.UseVisualStyleBackColor = true;
            buttonTestReSub.Click += buttonTestReSub_Click;
            // 
            // buttonTestRaid
            // 
            buttonTestRaid.Location = new Point(106, 6);
            buttonTestRaid.Name = "buttonTestRaid";
            buttonTestRaid.Size = new Size(94, 29);
            buttonTestRaid.TabIndex = 1;
            buttonTestRaid.Text = "Test Raid";
            buttonTestRaid.UseVisualStyleBackColor = true;
            buttonTestRaid.Click += buttonTestRaid_Click;
            // 
            // buttonTestSub
            // 
            buttonTestSub.Location = new Point(6, 6);
            buttonTestSub.Name = "buttonTestSub";
            buttonTestSub.Size = new Size(94, 29);
            buttonTestSub.TabIndex = 0;
            buttonTestSub.Text = "Test Sub";
            buttonTestSub.UseVisualStyleBackColor = true;
            buttonTestSub.Click += buttonTestSub_Click;
            // 
            // TwitchChatBot
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1073, 504);
            Controls.Add(tabControlChatBot);
            Controls.Add(buttonStartBot);
            MinimumSize = new Size(825, 546);
            Name = "TwitchChatBot";
            Text = "Twitch Chat Bot";
            tabControlChatBot.ResumeLayout(false);
            tabPageMainChat.ResumeLayout(false);
            tabPageMainChat.PerformLayout();
            tabPageLogging.ResumeLayout(false);
            tabPageLogging.PerformLayout();
            tabTesting.ResumeLayout(false);
            tabTesting.PerformLayout();
            ResumeLayout(false);
        }



        #endregion

        private Button buttonStartBot;
        private TabControl tabControlChatBot;
        private TabPage tabPageMainChat;
        private TabPage tabPageLogging;
        private Label labelChatBox;
        private TextBox textBoxLogging;
        private Label labelLogging;
        private TabPage tabTesting;
        private RichTextBox richTextBoxStreamChat;
        private Button buttonTestHypeTrain;
        private Button buttonTestSubGift;
        private Button buttonTestReSub;
        private Button buttonTestRaid;
        private Button buttonTestSub;
        private TextBox textFirstChatter;
        private Button buttonTestFirstChat;
        private TextBox textCommand;
        private Button buttonTestCommand;
        private TextBox textChannelPoint;
        private Button buttonTestChannelPoint;
        private TextBox textMysterySubs;
        private Button buttonTestMysteryGift;
        private TextBox textBits;
        private Button buttonTestCheer;
        private Button buttonTestFollow;
        private Button buttonClearFirst;
        private Label labelViewers;
        private ListBox listBoxCurrentViewers;
    }
}
