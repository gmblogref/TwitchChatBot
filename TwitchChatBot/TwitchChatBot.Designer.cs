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
            richTextBoxViewers = new RichTextBox();
            labelViewers = new Label();
            richTextBoxStreamChat = new RichTextBox();
            labelChatBox = new Label();
            tabPageLogging = new TabPage();
            textBoxLogging = new TextBox();
            labelLogging = new Label();
            tabTesting = new TabPage();
            groupBoxButtonTests = new GroupBox();
            buttonTestSub = new Button();
            buttonTestRaid = new Button();
            buttonClearFirst = new Button();
            buttonTestReSub = new Button();
            buttonTestFollow = new Button();
            buttonTestHypeTrain = new Button();
            buttonTestSubGift = new Button();
            groupBoxTextEntryTests = new GroupBox();
            comboBoxTtsSpeaker = new ComboBox();
            textBoxTtsText = new TextBox();
            buttonTestTts = new Button();
            buttonTestCheer = new Button();
            textBits = new TextBox();
            buttonTestMysteryGift = new Button();
            textFirstChatter = new TextBox();
            textMysterySubs = new TextBox();
            buttonTestFirstChat = new Button();
            buttonTestChannelPoint = new Button();
            textCommand = new TextBox();
            textChannelPoint = new TextBox();
            buttonTestCommand = new Button();
            tabControlChatBot.SuspendLayout();
            tabPageMainChat.SuspendLayout();
            tabPageLogging.SuspendLayout();
            tabTesting.SuspendLayout();
            groupBoxButtonTests.SuspendLayout();
            groupBoxTextEntryTests.SuspendLayout();
            SuspendLayout();
            // 
            // buttonStartBot
            // 
            buttonStartBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonStartBot.BackColor = Color.Green;
            buttonStartBot.Location = new Point(1007, 518);
            buttonStartBot.Name = "buttonStartBot";
            buttonStartBot.Size = new Size(127, 49);
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
            tabControlChatBot.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlChatBot.Font = new Font("Arial Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabControlChatBot.ItemSize = new Size(250, 35);
            tabControlChatBot.Location = new Point(12, 12);
            tabControlChatBot.Name = "tabControlChatBot";
            tabControlChatBot.SelectedIndex = 0;
            tabControlChatBot.Size = new Size(1126, 500);
            tabControlChatBot.SizeMode = TabSizeMode.Fixed;
            tabControlChatBot.TabIndex = 2;
            tabControlChatBot.DrawItem += tabControlChatBot_DrawItem;
            // 
            // tabPageMainChat
            // 
            tabPageMainChat.Controls.Add(richTextBoxViewers);
            tabPageMainChat.Controls.Add(labelViewers);
            tabPageMainChat.Controls.Add(richTextBoxStreamChat);
            tabPageMainChat.Controls.Add(labelChatBox);
            tabPageMainChat.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabPageMainChat.Location = new Point(4, 39);
            tabPageMainChat.Name = "tabPageMainChat";
            tabPageMainChat.Padding = new Padding(3);
            tabPageMainChat.Size = new Size(1118, 457);
            tabPageMainChat.TabIndex = 0;
            tabPageMainChat.Text = "Main Chat Bot Tab";
            tabPageMainChat.UseVisualStyleBackColor = true;
            // 
            // richTextBoxViewers
            // 
            richTextBoxViewers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            richTextBoxViewers.BackColor = Color.Black;
            richTextBoxViewers.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            richTextBoxViewers.ForeColor = Color.White;
            richTextBoxViewers.Location = new Point(732, 38);
            richTextBoxViewers.MinimumSize = new Size(303, 363);
            richTextBoxViewers.Name = "richTextBoxViewers";
            richTextBoxViewers.Size = new Size(303, 363);
            richTextBoxViewers.TabIndex = 6;
            richTextBoxViewers.Text = "";
            // 
            // labelViewers
            // 
            labelViewers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelViewers.AutoSize = true;
            labelViewers.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelViewers.Location = new Point(732, 3);
            labelViewers.Name = "labelViewers";
            labelViewers.Size = new Size(219, 32);
            labelViewers.TabIndex = 5;
            labelViewers.Text = "Current Viewers";
            // 
            // richTextBoxStreamChat
            // 
            richTextBoxStreamChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxStreamChat.BackColor = Color.Black;
            richTextBoxStreamChat.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            richTextBoxStreamChat.ForeColor = Color.White;
            richTextBoxStreamChat.Location = new Point(6, 38);
            richTextBoxStreamChat.MinimumSize = new Size(720, 363);
            richTextBoxStreamChat.Name = "richTextBoxStreamChat";
            richTextBoxStreamChat.Size = new Size(720, 363);
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
            tabPageLogging.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabPageLogging.Location = new Point(4, 39);
            tabPageLogging.Name = "tabPageLogging";
            tabPageLogging.Padding = new Padding(3);
            tabPageLogging.Size = new Size(1118, 457);
            tabPageLogging.TabIndex = 1;
            tabPageLogging.Text = "Logging Tab";
            tabPageLogging.UseVisualStyleBackColor = true;
            // 
            // textBoxLogging
            // 
            textBoxLogging.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxLogging.BackColor = Color.Black;
            textBoxLogging.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBoxLogging.ForeColor = Color.White;
            textBoxLogging.Location = new Point(6, 38);
            textBoxLogging.MinimumSize = new Size(1025, 365);
            textBoxLogging.Multiline = true;
            textBoxLogging.Name = "textBoxLogging";
            textBoxLogging.ScrollBars = ScrollBars.Both;
            textBoxLogging.Size = new Size(1025, 365);
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
            tabTesting.Controls.Add(groupBoxButtonTests);
            tabTesting.Controls.Add(groupBoxTextEntryTests);
            tabTesting.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabTesting.ForeColor = SystemColors.ControlText;
            tabTesting.Location = new Point(4, 39);
            tabTesting.Name = "tabTesting";
            tabTesting.Padding = new Padding(3);
            tabTesting.Size = new Size(1118, 457);
            tabTesting.TabIndex = 2;
            tabTesting.Text = "Testing Tab";
            // 
            // groupBoxButtonTests
            // 
            groupBoxButtonTests.Controls.Add(buttonTestSub);
            groupBoxButtonTests.Controls.Add(buttonTestRaid);
            groupBoxButtonTests.Controls.Add(buttonClearFirst);
            groupBoxButtonTests.Controls.Add(buttonTestReSub);
            groupBoxButtonTests.Controls.Add(buttonTestFollow);
            groupBoxButtonTests.Controls.Add(buttonTestHypeTrain);
            groupBoxButtonTests.Controls.Add(buttonTestSubGift);
            groupBoxButtonTests.Location = new Point(65, 45);
            groupBoxButtonTests.Name = "groupBoxButtonTests";
            groupBoxButtonTests.Size = new Size(394, 284);
            groupBoxButtonTests.TabIndex = 18;
            groupBoxButtonTests.TabStop = false;
            groupBoxButtonTests.Text = "Button Tests";
            // 
            // buttonTestSub
            // 
            buttonTestSub.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestSub.Location = new Point(206, 54);
            buttonTestSub.Name = "buttonTestSub";
            buttonTestSub.Size = new Size(163, 40);
            buttonTestSub.TabIndex = 0;
            buttonTestSub.Text = "Subscription";
            buttonTestSub.UseVisualStyleBackColor = true;
            buttonTestSub.Click += buttonTestSub_Click;
            // 
            // buttonTestRaid
            // 
            buttonTestRaid.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestRaid.Location = new Point(206, 124);
            buttonTestRaid.Name = "buttonTestRaid";
            buttonTestRaid.Size = new Size(163, 40);
            buttonTestRaid.TabIndex = 1;
            buttonTestRaid.Text = "Raid";
            buttonTestRaid.UseVisualStyleBackColor = true;
            buttonTestRaid.Click += buttonTestRaid_Click;
            // 
            // buttonClearFirst
            // 
            buttonClearFirst.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonClearFirst.Location = new Point(106, 175);
            buttonClearFirst.Name = "buttonClearFirst";
            buttonClearFirst.Size = new Size(163, 40);
            buttonClearFirst.TabIndex = 16;
            buttonClearFirst.Text = "Clear First Chatters";
            buttonClearFirst.UseVisualStyleBackColor = true;
            buttonClearFirst.Click += buttonClearFirst_Click;
            // 
            // buttonTestReSub
            // 
            buttonTestReSub.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestReSub.Location = new Point(208, 89);
            buttonTestReSub.Name = "buttonTestReSub";
            buttonTestReSub.Size = new Size(163, 40);
            buttonTestReSub.TabIndex = 2;
            buttonTestReSub.Text = "ReSub";
            buttonTestReSub.UseVisualStyleBackColor = true;
            buttonTestReSub.Click += buttonTestReSub_Click;
            // 
            // buttonTestFollow
            // 
            buttonTestFollow.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestFollow.Location = new Point(6, 54);
            buttonTestFollow.Name = "buttonTestFollow";
            buttonTestFollow.Size = new Size(163, 40);
            buttonTestFollow.TabIndex = 15;
            buttonTestFollow.Text = "Follow";
            buttonTestFollow.UseVisualStyleBackColor = true;
            buttonTestFollow.Click += buttonTestFollow_Click;
            // 
            // buttonTestHypeTrain
            // 
            buttonTestHypeTrain.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestHypeTrain.Location = new Point(6, 89);
            buttonTestHypeTrain.Name = "buttonTestHypeTrain";
            buttonTestHypeTrain.Size = new Size(163, 40);
            buttonTestHypeTrain.TabIndex = 4;
            buttonTestHypeTrain.Text = "Hype Train";
            buttonTestHypeTrain.UseVisualStyleBackColor = true;
            buttonTestHypeTrain.Click += buttonTestHypeTrain_Click;
            // 
            // buttonTestSubGift
            // 
            buttonTestSubGift.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestSubGift.Location = new Point(6, 124);
            buttonTestSubGift.Name = "buttonTestSubGift";
            buttonTestSubGift.Size = new Size(163, 40);
            buttonTestSubGift.TabIndex = 3;
            buttonTestSubGift.Text = "Gift Subs";
            buttonTestSubGift.UseVisualStyleBackColor = true;
            buttonTestSubGift.Click += buttonTestSubGift_Click;
            // 
            // groupBoxTextEntryTests
            // 
            groupBoxTextEntryTests.Controls.Add(comboBoxTtsSpeaker);
            groupBoxTextEntryTests.Controls.Add(textBoxTtsText);
            groupBoxTextEntryTests.Controls.Add(buttonTestTts);
            groupBoxTextEntryTests.Controls.Add(buttonTestCheer);
            groupBoxTextEntryTests.Controls.Add(textBits);
            groupBoxTextEntryTests.Controls.Add(buttonTestMysteryGift);
            groupBoxTextEntryTests.Controls.Add(textFirstChatter);
            groupBoxTextEntryTests.Controls.Add(textMysterySubs);
            groupBoxTextEntryTests.Controls.Add(buttonTestFirstChat);
            groupBoxTextEntryTests.Controls.Add(buttonTestChannelPoint);
            groupBoxTextEntryTests.Controls.Add(textCommand);
            groupBoxTextEntryTests.Controls.Add(textChannelPoint);
            groupBoxTextEntryTests.Controls.Add(buttonTestCommand);
            groupBoxTextEntryTests.Location = new Point(478, 45);
            groupBoxTextEntryTests.Name = "groupBoxTextEntryTests";
            groupBoxTextEntryTests.Size = new Size(596, 368);
            groupBoxTextEntryTests.TabIndex = 17;
            groupBoxTextEntryTests.TabStop = false;
            groupBoxTextEntryTests.Text = "Text Entry Tests";
            // 
            // comboBoxTtsSpeaker
            // 
            comboBoxTtsSpeaker.FormattingEnabled = true;
            comboBoxTtsSpeaker.Items.AddRange(new object[] { "p225", "p226", "p227", "p228", "p229", "p330" });
            comboBoxTtsSpeaker.Location = new Point(209, 291);
            comboBoxTtsSpeaker.Name = "comboBoxTtsSpeaker";
            comboBoxTtsSpeaker.Size = new Size(151, 40);
            comboBoxTtsSpeaker.TabIndex = 17;
            comboBoxTtsSpeaker.Text = "p225";
            // 
            // textBoxTtsText
            // 
            textBoxTtsText.Location = new Point(384, 290);
            textBoxTtsText.Name = "textBoxTtsText";
            textBoxTtsText.Size = new Size(206, 40);
            textBoxTtsText.TabIndex = 16;
            textBoxTtsText.KeyDown += textBoxTtsText_KeyDown;
            // 
            // buttonTestTts
            // 
            buttonTestTts.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestTts.Location = new Point(6, 291);
            buttonTestTts.Name = "buttonTestTts";
            buttonTestTts.Size = new Size(163, 40);
            buttonTestTts.TabIndex = 15;
            buttonTestTts.Text = "Text 2 Speach";
            buttonTestTts.UseVisualStyleBackColor = true;
            buttonTestTts.Click += buttonTestTts_Click;
            // 
            // buttonTestCheer
            // 
            buttonTestCheer.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestCheer.Location = new Point(6, 59);
            buttonTestCheer.Name = "buttonTestCheer";
            buttonTestCheer.Size = new Size(163, 40);
            buttonTestCheer.TabIndex = 5;
            buttonTestCheer.Text = "Cheer";
            buttonTestCheer.UseVisualStyleBackColor = true;
            buttonTestCheer.Click += buttonTestCheer_Click;
            // 
            // textBits
            // 
            textBits.Location = new Point(209, 59);
            textBits.Name = "textBits";
            textBits.Size = new Size(206, 40);
            textBits.TabIndex = 6;
            textBits.KeyDown += textBits_KeyDown;
            // 
            // buttonTestMysteryGift
            // 
            buttonTestMysteryGift.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestMysteryGift.Location = new Point(6, 105);
            buttonTestMysteryGift.Name = "buttonTestMysteryGift";
            buttonTestMysteryGift.Size = new Size(163, 40);
            buttonTestMysteryGift.TabIndex = 7;
            buttonTestMysteryGift.Text = "Mystery Gifts";
            buttonTestMysteryGift.UseVisualStyleBackColor = true;
            buttonTestMysteryGift.Click += buttonTestMysteryGift_Click;
            // 
            // textFirstChatter
            // 
            textFirstChatter.Location = new Point(209, 244);
            textFirstChatter.Name = "textFirstChatter";
            textFirstChatter.Size = new Size(206, 40);
            textFirstChatter.TabIndex = 14;
            textFirstChatter.KeyDown += textFirstChatter_KeyDown;
            // 
            // textMysterySubs
            // 
            textMysterySubs.Location = new Point(209, 105);
            textMysterySubs.Name = "textMysterySubs";
            textMysterySubs.Size = new Size(206, 40);
            textMysterySubs.TabIndex = 8;
            textMysterySubs.KeyDown += textMysterySubs_KeyDown;
            // 
            // buttonTestFirstChat
            // 
            buttonTestFirstChat.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestFirstChat.Location = new Point(6, 245);
            buttonTestFirstChat.Name = "buttonTestFirstChat";
            buttonTestFirstChat.Size = new Size(163, 40);
            buttonTestFirstChat.TabIndex = 13;
            buttonTestFirstChat.Text = "First Chat";
            buttonTestFirstChat.UseVisualStyleBackColor = true;
            buttonTestFirstChat.Click += buttonTestFirstChat_Click;
            // 
            // buttonTestChannelPoint
            // 
            buttonTestChannelPoint.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestChannelPoint.Location = new Point(6, 152);
            buttonTestChannelPoint.Name = "buttonTestChannelPoint";
            buttonTestChannelPoint.Size = new Size(163, 40);
            buttonTestChannelPoint.TabIndex = 9;
            buttonTestChannelPoint.Text = "Channel Points";
            buttonTestChannelPoint.UseVisualStyleBackColor = true;
            buttonTestChannelPoint.Click += buttonTestChannelPoint_Click;
            // 
            // textCommand
            // 
            textCommand.Location = new Point(209, 198);
            textCommand.Name = "textCommand";
            textCommand.Size = new Size(206, 40);
            textCommand.TabIndex = 12;
            textCommand.KeyDown += textCommand_KeyDown;
            // 
            // textChannelPoint
            // 
            textChannelPoint.Location = new Point(209, 151);
            textChannelPoint.Name = "textChannelPoint";
            textChannelPoint.Size = new Size(206, 40);
            textChannelPoint.TabIndex = 10;
            textChannelPoint.KeyDown += textChannelPoint_KeyDown;
            // 
            // buttonTestCommand
            // 
            buttonTestCommand.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestCommand.Location = new Point(6, 198);
            buttonTestCommand.Name = "buttonTestCommand";
            buttonTestCommand.Size = new Size(163, 40);
            buttonTestCommand.TabIndex = 11;
            buttonTestCommand.Text = "Commands";
            buttonTestCommand.UseVisualStyleBackColor = true;
            buttonTestCommand.Click += buttonTestCommand_Click;
            // 
            // TwitchChatBot
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1150, 579);
            Controls.Add(tabControlChatBot);
            Controls.Add(buttonStartBot);
            MinimumSize = new Size(1091, 551);
            Name = "TwitchChatBot";
            Text = "Twitch Chat Bot";
            tabControlChatBot.ResumeLayout(false);
            tabPageMainChat.ResumeLayout(false);
            tabPageMainChat.PerformLayout();
            tabPageLogging.ResumeLayout(false);
            tabPageLogging.PerformLayout();
            tabTesting.ResumeLayout(false);
            groupBoxButtonTests.ResumeLayout(false);
            groupBoxTextEntryTests.ResumeLayout(false);
            groupBoxTextEntryTests.PerformLayout();
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
        private RichTextBox richTextBoxViewers;
        private GroupBox groupBoxTextEntryTests;
        private GroupBox groupBoxButtonTests;
        private Button buttonTestTts;
        private TextBox textBoxTtsText;
        private ComboBox comboBoxTtsSpeaker;
    }
}
