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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TwitchChatBot));
            buttonStartBot = new Button();
            tabControlChatBot = new TabControl();
            tabPageMainChat = new TabPage();
            richTextBoxViewers = new RichTextBox();
            labelViewers = new Label();
            richTextBoxStreamChat = new RichTextBox();
            labelChatBox = new Label();
            tabPageUtilities = new TabPage();
            groupBoxAlertHistory = new GroupBox();
            buttonReplayAlert = new Button();
            listViewAlertHistory = new ListView();
            buttonClearFirst = new Button();
            groupBoxTts = new GroupBox();
            buttonTtsReset = new Button();
            buttonTtsSkip = new Button();
            tabTesting = new TabPage();
            groupBoxButtonTests = new GroupBox();
            buttonTestWatchStreak = new Button();
            labelUserName = new Label();
            textUserName = new TextBox();
            labelAmount = new Label();
            textAmount = new TextBox();
            buttonTestFollow = new Button();
            buttonTestSub = new Button();
            buttonTestReSub = new Button();
            buttonTestSubGift = new Button();
            buttonTestRaid = new Button();
            buttonTestMysteryGift = new Button();
            buttonTestHypeTrain = new Button();
            groupBoxTextEntryTests = new GroupBox();
            comboBoxTtsSpeaker = new ComboBox();
            textBoxTtsText = new TextBox();
            buttonTestTts = new Button();
            buttonTestCheer = new Button();
            textBits = new TextBox();
            textFirstChatter = new TextBox();
            buttonTestFirstChat = new Button();
            buttonTestChannelPoint = new Button();
            textCommand = new TextBox();
            textChannelPoint = new TextBox();
            buttonTestCommand = new Button();
            tabPageLogging = new TabPage();
            textBoxLogging = new TextBox();
            labelLogging = new Label();
            buttonTestBot = new Button();
            buttonStopBot = new Button();
            buttonTestHypeTrainEnd = new Button();
            tabControlChatBot.SuspendLayout();
            tabPageMainChat.SuspendLayout();
            tabPageUtilities.SuspendLayout();
            groupBoxAlertHistory.SuspendLayout();
            groupBoxTts.SuspendLayout();
            tabTesting.SuspendLayout();
            groupBoxButtonTests.SuspendLayout();
            groupBoxTextEntryTests.SuspendLayout();
            tabPageLogging.SuspendLayout();
            SuspendLayout();
            // 
            // buttonStartBot
            // 
            buttonStartBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonStartBot.BackColor = Color.Green;
            buttonStartBot.Location = new Point(881, 388);
            buttonStartBot.Margin = new Padding(3, 2, 3, 2);
            buttonStartBot.Name = "buttonStartBot";
            buttonStartBot.Size = new Size(111, 37);
            buttonStartBot.TabIndex = 0;
            buttonStartBot.Text = "Start Bot";
            buttonStartBot.UseVisualStyleBackColor = false;
            buttonStartBot.EnabledChanged += buttonStartBot_EnabledChanged;
            buttonStartBot.Click += buttonStartBot_Click;
            // 
            // tabControlChatBot
            // 
            tabControlChatBot.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControlChatBot.Controls.Add(tabPageMainChat);
            tabControlChatBot.Controls.Add(tabPageUtilities);
            tabControlChatBot.Controls.Add(tabTesting);
            tabControlChatBot.Controls.Add(tabPageLogging);
            tabControlChatBot.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlChatBot.Font = new Font("Arial Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabControlChatBot.ItemSize = new Size(250, 35);
            tabControlChatBot.Location = new Point(10, 9);
            tabControlChatBot.Margin = new Padding(3, 2, 3, 2);
            tabControlChatBot.Name = "tabControlChatBot";
            tabControlChatBot.SelectedIndex = 0;
            tabControlChatBot.Size = new Size(985, 375);
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
            tabPageMainChat.Margin = new Padding(3, 2, 3, 2);
            tabPageMainChat.Name = "tabPageMainChat";
            tabPageMainChat.Padding = new Padding(3, 2, 3, 2);
            tabPageMainChat.Size = new Size(977, 332);
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
            richTextBoxViewers.Location = new Point(640, 28);
            richTextBoxViewers.Margin = new Padding(3, 2, 3, 2);
            richTextBoxViewers.MinimumSize = new Size(266, 273);
            richTextBoxViewers.Name = "richTextBoxViewers";
            richTextBoxViewers.Size = new Size(266, 273);
            richTextBoxViewers.TabIndex = 6;
            richTextBoxViewers.Text = "";
            // 
            // labelViewers
            // 
            labelViewers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelViewers.AutoSize = true;
            labelViewers.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelViewers.Location = new Point(640, 2);
            labelViewers.Name = "labelViewers";
            labelViewers.Size = new Size(182, 27);
            labelViewers.TabIndex = 5;
            labelViewers.Text = "Current Viewers";
            // 
            // richTextBoxStreamChat
            // 
            richTextBoxStreamChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxStreamChat.BackColor = Color.Black;
            richTextBoxStreamChat.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            richTextBoxStreamChat.ForeColor = Color.White;
            richTextBoxStreamChat.Location = new Point(5, 28);
            richTextBoxStreamChat.Margin = new Padding(3, 2, 3, 2);
            richTextBoxStreamChat.MinimumSize = new Size(630, 273);
            richTextBoxStreamChat.Name = "richTextBoxStreamChat";
            richTextBoxStreamChat.Size = new Size(630, 273);
            richTextBoxStreamChat.TabIndex = 4;
            richTextBoxStreamChat.Text = "";
            // 
            // labelChatBox
            // 
            labelChatBox.AutoSize = true;
            labelChatBox.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelChatBox.Location = new Point(5, 2);
            labelChatBox.Name = "labelChatBox";
            labelChatBox.Size = new Size(142, 27);
            labelChatBox.TabIndex = 3;
            labelChatBox.Text = "Stream Chat";
            // 
            // tabPageUtilities
            // 
            tabPageUtilities.Controls.Add(groupBoxAlertHistory);
            tabPageUtilities.Controls.Add(buttonClearFirst);
            tabPageUtilities.Controls.Add(groupBoxTts);
            tabPageUtilities.Location = new Point(4, 39);
            tabPageUtilities.Margin = new Padding(3, 2, 3, 2);
            tabPageUtilities.Name = "tabPageUtilities";
            tabPageUtilities.Size = new Size(977, 332);
            tabPageUtilities.TabIndex = 3;
            tabPageUtilities.Text = "Utilities Tab";
            tabPageUtilities.UseVisualStyleBackColor = true;
            // 
            // groupBoxAlertHistory
            // 
            groupBoxAlertHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxAlertHistory.Controls.Add(buttonReplayAlert);
            groupBoxAlertHistory.Controls.Add(listViewAlertHistory);
            groupBoxAlertHistory.Location = new Point(435, 18);
            groupBoxAlertHistory.Margin = new Padding(3, 2, 3, 2);
            groupBoxAlertHistory.Name = "groupBoxAlertHistory";
            groupBoxAlertHistory.Padding = new Padding(3, 2, 3, 2);
            groupBoxAlertHistory.Size = new Size(528, 309);
            groupBoxAlertHistory.TabIndex = 18;
            groupBoxAlertHistory.TabStop = false;
            groupBoxAlertHistory.Text = "Alert History";
            // 
            // buttonReplayAlert
            // 
            buttonReplayAlert.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonReplayAlert.Location = new Point(63, 268);
            buttonReplayAlert.Margin = new Padding(3, 2, 3, 2);
            buttonReplayAlert.Name = "buttonReplayAlert";
            buttonReplayAlert.Size = new Size(88, 37);
            buttonReplayAlert.TabIndex = 1;
            buttonReplayAlert.Text = "Replay";
            buttonReplayAlert.UseVisualStyleBackColor = true;
            buttonReplayAlert.Click += buttonReplayAlert_Click;
            // 
            // listViewAlertHistory
            // 
            listViewAlertHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewAlertHistory.Location = new Point(20, 26);
            listViewAlertHistory.Margin = new Padding(3, 2, 3, 2);
            listViewAlertHistory.Name = "listViewAlertHistory";
            listViewAlertHistory.Size = new Size(469, 228);
            listViewAlertHistory.TabIndex = 0;
            listViewAlertHistory.UseCompatibleStateImageBehavior = false;
            listViewAlertHistory.DoubleClick += listViewAlertHistory_DoubleClick;
            // 
            // buttonClearFirst
            // 
            buttonClearFirst.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonClearFirst.Location = new Point(29, 135);
            buttonClearFirst.Margin = new Padding(3, 2, 3, 2);
            buttonClearFirst.Name = "buttonClearFirst";
            buttonClearFirst.Size = new Size(143, 30);
            buttonClearFirst.TabIndex = 17;
            buttonClearFirst.Text = "Clear First";
            buttonClearFirst.UseVisualStyleBackColor = true;
            buttonClearFirst.Click += buttonClearFirst_Click;
            // 
            // groupBoxTts
            // 
            groupBoxTts.Controls.Add(buttonTtsReset);
            groupBoxTts.Controls.Add(buttonTtsSkip);
            groupBoxTts.Location = new Point(20, 18);
            groupBoxTts.Margin = new Padding(3, 2, 3, 2);
            groupBoxTts.Name = "groupBoxTts";
            groupBoxTts.Padding = new Padding(3, 2, 3, 2);
            groupBoxTts.Size = new Size(219, 72);
            groupBoxTts.TabIndex = 0;
            groupBoxTts.TabStop = false;
            groupBoxTts.Text = "TTS Controls";
            // 
            // buttonTtsReset
            // 
            buttonTtsReset.Location = new Point(110, 26);
            buttonTtsReset.Margin = new Padding(3, 2, 3, 2);
            buttonTtsReset.Name = "buttonTtsReset";
            buttonTtsReset.Size = new Size(82, 28);
            buttonTtsReset.TabIndex = 1;
            buttonTtsReset.Text = "Reset";
            buttonTtsReset.UseVisualStyleBackColor = true;
            buttonTtsReset.Click += buttonTtsReset_Click;
            // 
            // buttonTtsSkip
            // 
            buttonTtsSkip.Location = new Point(9, 26);
            buttonTtsSkip.Margin = new Padding(3, 2, 3, 2);
            buttonTtsSkip.Name = "buttonTtsSkip";
            buttonTtsSkip.Size = new Size(82, 28);
            buttonTtsSkip.TabIndex = 0;
            buttonTtsSkip.Text = "Skip";
            buttonTtsSkip.UseVisualStyleBackColor = true;
            buttonTtsSkip.Click += buttonTtsSkip_Click;
            // 
            // tabTesting
            // 
            tabTesting.BackColor = Color.Transparent;
            tabTesting.Controls.Add(groupBoxButtonTests);
            tabTesting.Controls.Add(groupBoxTextEntryTests);
            tabTesting.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabTesting.ForeColor = SystemColors.ControlText;
            tabTesting.Location = new Point(4, 39);
            tabTesting.Margin = new Padding(3, 2, 3, 2);
            tabTesting.Name = "tabTesting";
            tabTesting.Padding = new Padding(3, 2, 3, 2);
            tabTesting.Size = new Size(977, 332);
            tabTesting.TabIndex = 2;
            tabTesting.Text = "Testing Tab";
            // 
            // groupBoxButtonTests
            // 
            groupBoxButtonTests.Controls.Add(buttonTestHypeTrainEnd);
            groupBoxButtonTests.Controls.Add(buttonTestWatchStreak);
            groupBoxButtonTests.Controls.Add(labelUserName);
            groupBoxButtonTests.Controls.Add(textUserName);
            groupBoxButtonTests.Controls.Add(labelAmount);
            groupBoxButtonTests.Controls.Add(textAmount);
            groupBoxButtonTests.Controls.Add(buttonTestFollow);
            groupBoxButtonTests.Controls.Add(buttonTestSub);
            groupBoxButtonTests.Controls.Add(buttonTestReSub);
            groupBoxButtonTests.Controls.Add(buttonTestSubGift);
            groupBoxButtonTests.Controls.Add(buttonTestRaid);
            groupBoxButtonTests.Controls.Add(buttonTestMysteryGift);
            groupBoxButtonTests.Controls.Add(buttonTestHypeTrain);
            groupBoxButtonTests.Font = new Font("Arial Black", 12F, FontStyle.Bold);
            groupBoxButtonTests.Location = new Point(21, 34);
            groupBoxButtonTests.Margin = new Padding(3, 2, 3, 2);
            groupBoxButtonTests.Name = "groupBoxButtonTests";
            groupBoxButtonTests.Padding = new Padding(3, 2, 3, 2);
            groupBoxButtonTests.Size = new Size(363, 269);
            groupBoxButtonTests.TabIndex = 18;
            groupBoxButtonTests.TabStop = false;
            groupBoxButtonTests.Text = "Button Tests";
            // 
            // buttonTestWatchStreak
            // 
            buttonTestWatchStreak.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestWatchStreak.Location = new Point(9, 187);
            buttonTestWatchStreak.Margin = new Padding(3, 2, 3, 2);
            buttonTestWatchStreak.Name = "buttonTestWatchStreak";
            buttonTestWatchStreak.Size = new Size(143, 30);
            buttonTestWatchStreak.TabIndex = 16;
            buttonTestWatchStreak.Text = "Watch Streak";
            buttonTestWatchStreak.UseVisualStyleBackColor = true;
            buttonTestWatchStreak.Click += buttonTestWatchStreak_Click;
            // 
            // labelUserName
            // 
            labelUserName.AutoSize = true;
            labelUserName.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            labelUserName.Location = new Point(9, 26);
            labelUserName.Name = "labelUserName";
            labelUserName.Size = new Size(99, 19);
            labelUserName.TabIndex = 0;
            labelUserName.Text = "User Name:";
            // 
            // textUserName
            // 
            textUserName.Font = new Font("Arial", 10F);
            textUserName.Location = new Point(140, 22);
            textUserName.Margin = new Padding(3, 2, 3, 2);
            textUserName.Name = "textUserName";
            textUserName.Size = new Size(184, 23);
            textUserName.TabIndex = 1;
            // 
            // labelAmount
            // 
            labelAmount.AutoSize = true;
            labelAmount.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            labelAmount.Location = new Point(36, 55);
            labelAmount.Name = "labelAmount";
            labelAmount.Size = new Size(72, 19);
            labelAmount.TabIndex = 2;
            labelAmount.Text = "Amount:";
            // 
            // textAmount
            // 
            textAmount.Font = new Font("Arial", 10F);
            textAmount.Location = new Point(140, 52);
            textAmount.Margin = new Padding(3, 2, 3, 2);
            textAmount.Name = "textAmount";
            textAmount.Size = new Size(184, 23);
            textAmount.TabIndex = 3;
            // 
            // buttonTestFollow
            // 
            buttonTestFollow.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestFollow.Location = new Point(9, 82);
            buttonTestFollow.Margin = new Padding(3, 2, 3, 2);
            buttonTestFollow.Name = "buttonTestFollow";
            buttonTestFollow.Size = new Size(143, 30);
            buttonTestFollow.TabIndex = 15;
            buttonTestFollow.Text = "Follow";
            buttonTestFollow.UseVisualStyleBackColor = true;
            buttonTestFollow.Click += buttonTestFollow_Click;
            // 
            // buttonTestSub
            // 
            buttonTestSub.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestSub.Location = new Point(181, 82);
            buttonTestSub.Margin = new Padding(3, 2, 3, 2);
            buttonTestSub.Name = "buttonTestSub";
            buttonTestSub.Size = new Size(143, 30);
            buttonTestSub.TabIndex = 0;
            buttonTestSub.Text = "Subscription";
            buttonTestSub.UseVisualStyleBackColor = true;
            buttonTestSub.Click += buttonTestSub_Click;
            // 
            // buttonTestReSub
            // 
            buttonTestReSub.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestReSub.Location = new Point(181, 184);
            buttonTestReSub.Margin = new Padding(3, 2, 3, 2);
            buttonTestReSub.Name = "buttonTestReSub";
            buttonTestReSub.Size = new Size(143, 30);
            buttonTestReSub.TabIndex = 2;
            buttonTestReSub.Text = "ReSub";
            buttonTestReSub.UseVisualStyleBackColor = true;
            buttonTestReSub.Click += buttonTestReSub_Click;
            // 
            // buttonTestSubGift
            // 
            buttonTestSubGift.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestSubGift.Location = new Point(181, 116);
            buttonTestSubGift.Margin = new Padding(3, 2, 3, 2);
            buttonTestSubGift.Name = "buttonTestSubGift";
            buttonTestSubGift.Size = new Size(143, 30);
            buttonTestSubGift.TabIndex = 3;
            buttonTestSubGift.Text = "Gift Subs";
            buttonTestSubGift.UseVisualStyleBackColor = true;
            buttonTestSubGift.Click += buttonTestSubGift_Click;
            // 
            // buttonTestRaid
            // 
            buttonTestRaid.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestRaid.Location = new Point(9, 118);
            buttonTestRaid.Margin = new Padding(3, 2, 3, 2);
            buttonTestRaid.Name = "buttonTestRaid";
            buttonTestRaid.Size = new Size(143, 30);
            buttonTestRaid.TabIndex = 1;
            buttonTestRaid.Text = "Raid";
            buttonTestRaid.UseVisualStyleBackColor = true;
            buttonTestRaid.Click += buttonTestRaid_Click;
            // 
            // buttonTestMysteryGift
            // 
            buttonTestMysteryGift.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestMysteryGift.Location = new Point(181, 150);
            buttonTestMysteryGift.Margin = new Padding(3, 2, 3, 2);
            buttonTestMysteryGift.Name = "buttonTestMysteryGift";
            buttonTestMysteryGift.Size = new Size(143, 30);
            buttonTestMysteryGift.TabIndex = 7;
            buttonTestMysteryGift.Text = "Mystery Gifts";
            buttonTestMysteryGift.UseVisualStyleBackColor = true;
            buttonTestMysteryGift.Click += buttonTestMysteryGift_Click;
            // 
            // buttonTestHypeTrain
            // 
            buttonTestHypeTrain.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestHypeTrain.Location = new Point(9, 152);
            buttonTestHypeTrain.Margin = new Padding(3, 2, 3, 2);
            buttonTestHypeTrain.Name = "buttonTestHypeTrain";
            buttonTestHypeTrain.Size = new Size(143, 30);
            buttonTestHypeTrain.TabIndex = 4;
            buttonTestHypeTrain.Text = "Hype Train";
            buttonTestHypeTrain.UseVisualStyleBackColor = true;
            buttonTestHypeTrain.Click += buttonTestHypeTrain_Click;
            // 
            // groupBoxTextEntryTests
            // 
            groupBoxTextEntryTests.Controls.Add(comboBoxTtsSpeaker);
            groupBoxTextEntryTests.Controls.Add(textBoxTtsText);
            groupBoxTextEntryTests.Controls.Add(buttonTestTts);
            groupBoxTextEntryTests.Controls.Add(buttonTestCheer);
            groupBoxTextEntryTests.Controls.Add(textBits);
            groupBoxTextEntryTests.Controls.Add(textFirstChatter);
            groupBoxTextEntryTests.Controls.Add(buttonTestFirstChat);
            groupBoxTextEntryTests.Controls.Add(buttonTestChannelPoint);
            groupBoxTextEntryTests.Controls.Add(textCommand);
            groupBoxTextEntryTests.Controls.Add(textChannelPoint);
            groupBoxTextEntryTests.Controls.Add(buttonTestCommand);
            groupBoxTextEntryTests.Location = new Point(457, 34);
            groupBoxTextEntryTests.Margin = new Padding(3, 2, 3, 2);
            groupBoxTextEntryTests.Name = "groupBoxTextEntryTests";
            groupBoxTextEntryTests.Padding = new Padding(3, 2, 3, 2);
            groupBoxTextEntryTests.Size = new Size(483, 244);
            groupBoxTextEntryTests.TabIndex = 17;
            groupBoxTextEntryTests.TabStop = false;
            groupBoxTextEntryTests.Text = "Text Entry Tests";
            // 
            // comboBoxTtsSpeaker
            // 
            comboBoxTtsSpeaker.FormattingEnabled = true;
            comboBoxTtsSpeaker.Items.AddRange(new object[] { "Matthew", "Joanna", "Brian", "Emma", "Justin", "Kimberly", "Amy", "Russel", "Nicole", "Olivia", "Stephen", "Salli", "Joey", "Ivy", "Kendra" });
            comboBoxTtsSpeaker.Location = new Point(183, 186);
            comboBoxTtsSpeaker.Margin = new Padding(3, 2, 3, 2);
            comboBoxTtsSpeaker.Name = "comboBoxTtsSpeaker";
            comboBoxTtsSpeaker.Size = new Size(133, 34);
            comboBoxTtsSpeaker.TabIndex = 17;
            comboBoxTtsSpeaker.Text = "Matthew";
            // 
            // textBoxTtsText
            // 
            textBoxTtsText.Location = new Point(320, 186);
            textBoxTtsText.Margin = new Padding(3, 2, 3, 2);
            textBoxTtsText.Name = "textBoxTtsText";
            textBoxTtsText.Size = new Size(181, 33);
            textBoxTtsText.TabIndex = 16;
            textBoxTtsText.KeyDown += textBoxTtsText_KeyDown;
            // 
            // buttonTestTts
            // 
            buttonTestTts.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestTts.Location = new Point(5, 186);
            buttonTestTts.Margin = new Padding(3, 2, 3, 2);
            buttonTestTts.Name = "buttonTestTts";
            buttonTestTts.Size = new Size(143, 30);
            buttonTestTts.TabIndex = 15;
            buttonTestTts.Text = "Text 2 Speech";
            buttonTestTts.UseVisualStyleBackColor = true;
            buttonTestTts.Click += buttonTestTts_Click;
            // 
            // buttonTestCheer
            // 
            buttonTestCheer.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestCheer.Location = new Point(5, 44);
            buttonTestCheer.Margin = new Padding(3, 2, 3, 2);
            buttonTestCheer.Name = "buttonTestCheer";
            buttonTestCheer.Size = new Size(143, 30);
            buttonTestCheer.TabIndex = 5;
            buttonTestCheer.Text = "Cheer";
            buttonTestCheer.UseVisualStyleBackColor = true;
            buttonTestCheer.Click += buttonTestCheer_Click;
            // 
            // textBits
            // 
            textBits.Location = new Point(183, 44);
            textBits.Margin = new Padding(3, 2, 3, 2);
            textBits.Name = "textBits";
            textBits.Size = new Size(181, 33);
            textBits.TabIndex = 6;
            textBits.KeyDown += textBits_KeyDown;
            // 
            // textFirstChatter
            // 
            textFirstChatter.Location = new Point(183, 152);
            textFirstChatter.Margin = new Padding(3, 2, 3, 2);
            textFirstChatter.Name = "textFirstChatter";
            textFirstChatter.Size = new Size(181, 33);
            textFirstChatter.TabIndex = 14;
            textFirstChatter.KeyDown += textFirstChatter_KeyDown;
            // 
            // buttonTestFirstChat
            // 
            buttonTestFirstChat.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestFirstChat.Location = new Point(5, 152);
            buttonTestFirstChat.Margin = new Padding(3, 2, 3, 2);
            buttonTestFirstChat.Name = "buttonTestFirstChat";
            buttonTestFirstChat.Size = new Size(143, 30);
            buttonTestFirstChat.TabIndex = 13;
            buttonTestFirstChat.Text = "First Chat";
            buttonTestFirstChat.UseVisualStyleBackColor = true;
            buttonTestFirstChat.Click += buttonTestFirstChat_Click;
            // 
            // buttonTestChannelPoint
            // 
            buttonTestChannelPoint.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestChannelPoint.Location = new Point(5, 82);
            buttonTestChannelPoint.Margin = new Padding(3, 2, 3, 2);
            buttonTestChannelPoint.Name = "buttonTestChannelPoint";
            buttonTestChannelPoint.Size = new Size(143, 30);
            buttonTestChannelPoint.TabIndex = 9;
            buttonTestChannelPoint.Text = "Channel Points";
            buttonTestChannelPoint.UseVisualStyleBackColor = true;
            buttonTestChannelPoint.Click += buttonTestChannelPoint_Click;
            // 
            // textCommand
            // 
            textCommand.Location = new Point(183, 117);
            textCommand.Margin = new Padding(3, 2, 3, 2);
            textCommand.Name = "textCommand";
            textCommand.Size = new Size(181, 33);
            textCommand.TabIndex = 12;
            textCommand.KeyDown += textCommand_KeyDown;
            // 
            // textChannelPoint
            // 
            textChannelPoint.Location = new Point(183, 82);
            textChannelPoint.Margin = new Padding(3, 2, 3, 2);
            textChannelPoint.Name = "textChannelPoint";
            textChannelPoint.Size = new Size(181, 33);
            textChannelPoint.TabIndex = 10;
            textChannelPoint.KeyDown += textChannelPoint_KeyDown;
            // 
            // buttonTestCommand
            // 
            buttonTestCommand.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonTestCommand.Location = new Point(5, 117);
            buttonTestCommand.Margin = new Padding(3, 2, 3, 2);
            buttonTestCommand.Name = "buttonTestCommand";
            buttonTestCommand.Size = new Size(143, 30);
            buttonTestCommand.TabIndex = 11;
            buttonTestCommand.Text = "Commands";
            buttonTestCommand.UseVisualStyleBackColor = true;
            buttonTestCommand.Click += buttonTestCommand_Click;
            // 
            // tabPageLogging
            // 
            tabPageLogging.BorderStyle = BorderStyle.Fixed3D;
            tabPageLogging.Controls.Add(textBoxLogging);
            tabPageLogging.Controls.Add(labelLogging);
            tabPageLogging.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabPageLogging.Location = new Point(4, 39);
            tabPageLogging.Margin = new Padding(3, 2, 3, 2);
            tabPageLogging.Name = "tabPageLogging";
            tabPageLogging.Padding = new Padding(3, 2, 3, 2);
            tabPageLogging.Size = new Size(977, 332);
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
            textBoxLogging.Location = new Point(5, 28);
            textBoxLogging.Margin = new Padding(3, 2, 3, 2);
            textBoxLogging.MinimumSize = new Size(897, 275);
            textBoxLogging.Multiline = true;
            textBoxLogging.Name = "textBoxLogging";
            textBoxLogging.ReadOnly = true;
            textBoxLogging.ScrollBars = ScrollBars.Both;
            textBoxLogging.Size = new Size(897, 275);
            textBoxLogging.TabIndex = 1;
            // 
            // labelLogging
            // 
            labelLogging.AutoSize = true;
            labelLogging.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelLogging.Location = new Point(5, 2);
            labelLogging.Name = "labelLogging";
            labelLogging.Size = new Size(223, 27);
            labelLogging.TabIndex = 0;
            labelLogging.Text = "Logging Information";
            // 
            // buttonTestBot
            // 
            buttonTestBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonTestBot.BackColor = Color.Yellow;
            buttonTestBot.Location = new Point(14, 388);
            buttonTestBot.Margin = new Padding(3, 2, 3, 2);
            buttonTestBot.Name = "buttonTestBot";
            buttonTestBot.Size = new Size(111, 37);
            buttonTestBot.TabIndex = 3;
            buttonTestBot.Text = "Test Bot";
            buttonTestBot.UseVisualStyleBackColor = false;
            buttonTestBot.Click += buttonTestBot_Click;
            // 
            // buttonStopBot
            // 
            buttonStopBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonStopBot.BackColor = Color.Crimson;
            buttonStopBot.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            buttonStopBot.Location = new Point(765, 388);
            buttonStopBot.Margin = new Padding(3, 2, 3, 2);
            buttonStopBot.Name = "buttonStopBot";
            buttonStopBot.Size = new Size(111, 37);
            buttonStopBot.TabIndex = 4;
            buttonStopBot.Text = "Stop Bot";
            buttonStopBot.UseVisualStyleBackColor = false;
            buttonStopBot.Click += buttonStopBot_Click;
            // 
            // buttonTestHypeTrainEnd
            // 
            buttonTestHypeTrainEnd.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
            buttonTestHypeTrainEnd.Location = new Point(9, 221);
            buttonTestHypeTrainEnd.Margin = new Padding(3, 2, 3, 2);
            buttonTestHypeTrainEnd.Name = "buttonTestHypeTrainEnd";
            buttonTestHypeTrainEnd.Size = new Size(143, 30);
            buttonTestHypeTrainEnd.TabIndex = 17;
            buttonTestHypeTrainEnd.Text = "Hype Train End";
            buttonTestHypeTrainEnd.UseVisualStyleBackColor = true;
            buttonTestHypeTrainEnd.Click += buttonTestHypeTrainEnd_Click;
            // 
            // TwitchChatBot
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1008, 440);
            Controls.Add(buttonStopBot);
            Controls.Add(buttonTestBot);
            Controls.Add(tabControlChatBot);
            Controls.Add(buttonStartBot);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(1024, 479);
            Name = "TwitchChatBot";
            Text = "Twitch Chat Bot";
            FormClosing += TwitchChatBot_FormClosing;
            tabControlChatBot.ResumeLayout(false);
            tabPageMainChat.ResumeLayout(false);
            tabPageMainChat.PerformLayout();
            tabPageUtilities.ResumeLayout(false);
            groupBoxAlertHistory.ResumeLayout(false);
            groupBoxTts.ResumeLayout(false);
            tabTesting.ResumeLayout(false);
            groupBoxButtonTests.ResumeLayout(false);
            groupBoxButtonTests.PerformLayout();
            groupBoxTextEntryTests.ResumeLayout(false);
            groupBoxTextEntryTests.PerformLayout();
            tabPageLogging.ResumeLayout(false);
            tabPageLogging.PerformLayout();
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
        private Button buttonTestMysteryGift;
        private TextBox textBits;
        private Button buttonTestCheer;
        private Button buttonTestFollow;
        private Label labelViewers;
        private RichTextBox richTextBoxViewers;
        private GroupBox groupBoxTextEntryTests;
        private GroupBox groupBoxButtonTests;
        private Button buttonTestTts;
        private TextBox textBoxTtsText;
        private ComboBox comboBoxTtsSpeaker;
        private TextBox textUserName;
        private TextBox textAmount;
        private Label labelUserName;
        private Label labelAmount;
        private Button buttonTestBot;
        private Button buttonStopBot;
        private TabPage tabPageUtilities;
        private GroupBox groupBoxTts;
        private Button buttonTtsReset;
        private Button buttonTtsSkip;
        private Button buttonClearFirst;
        private GroupBox groupBoxAlertHistory;
        private ListView listViewAlertHistory;
        private Button buttonReplayAlert;
        private Button buttonTestWatchStreak;
        private Button buttonTestHypeTrainEnd;
    }
}
