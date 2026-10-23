using TwitchChatBot.Controls;

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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TwitchChatBot));
			DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
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
			tabWheel = new TabPage();
			panelSpinResultOverlay = new Panel();
			buttonSpinResultOk = new Button();
			labelSpinResult = new Label();
			labelWheelWarning = new Label();
			buttonDeleteWheel = new Button();
			buttonShuffleEntries = new Button();
			buttonAddWheel = new Button();
			buttonRemoveEntry = new Button();
			buttonAddEntry = new Button();
			buttonSaveEntries = new Button();
			buttonSpin = new Button();
			labelWheels = new Label();
			wheelSpinnerControl = new WheelSpinnerControl();
			dataGridViewEntries = new DataGridView();
			comboBoxWheels = new ComboBox();
			tabTesting = new TabPage();
			groupBoxButtonTests = new GroupBox();
			buttonTestHypeTrainEnd = new Button();
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
			buttonClearLog = new Button();
			buttonCopyAllLog = new Button();
			buttonTestBot = new Button();
			buttonStopBot = new Button();
			buttonTestDonation = new Button();
			toolTipMain = new ToolTip(components);
			tabControlChatBot.SuspendLayout();
			tabPageMainChat.SuspendLayout();
			tabPageUtilities.SuspendLayout();
			groupBoxAlertHistory.SuspendLayout();
			groupBoxTts.SuspendLayout();
			tabWheel.SuspendLayout();
			panelSpinResultOverlay.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dataGridViewEntries).BeginInit();
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
			buttonStartBot.Location = new Point(1151, 517);
			buttonStartBot.Name = "buttonStartBot";
			buttonStartBot.Size = new Size(127, 49);
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
			tabControlChatBot.Controls.Add(tabWheel);
			tabControlChatBot.Controls.Add(tabTesting);
			tabControlChatBot.Controls.Add(tabPageLogging);
			tabControlChatBot.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControlChatBot.Font = new Font("Arial Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
			tabControlChatBot.ItemSize = new Size(250, 35);
			tabControlChatBot.Location = new Point(12, 12);
			tabControlChatBot.Name = "tabControlChatBot";
			tabControlChatBot.SelectedIndex = 0;
			tabControlChatBot.Size = new Size(1270, 500);
			tabControlChatBot.SizeMode = TabSizeMode.Fixed;
			tabControlChatBot.TabIndex = 2;
			tabControlChatBot.DrawItem += tabControlChatBot_DrawItem;
			tabControlChatBot.SelectedIndexChanged += tabControlChatBot_SelectedIndexChanged;
			tabControlChatBot.Selecting += tabControlChatBot_Selecting;
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
			tabPageMainChat.Size = new Size(1262, 457);
			tabPageMainChat.TabIndex = 0;
			tabPageMainChat.Text = "Main Chat";
			tabPageMainChat.UseVisualStyleBackColor = true;
			// 
			// richTextBoxViewers
			// 
			richTextBoxViewers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			richTextBoxViewers.BackColor = Color.Black;
			richTextBoxViewers.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			richTextBoxViewers.ForeColor = Color.White;
			richTextBoxViewers.Location = new Point(953, 37);
			richTextBoxViewers.MinimumSize = new Size(303, 363);
			richTextBoxViewers.Name = "richTextBoxViewers";
			richTextBoxViewers.ReadOnly = true;
			richTextBoxViewers.Size = new Size(303, 363);
			richTextBoxViewers.TabIndex = 6;
			richTextBoxViewers.Text = "";
			// 
			// labelViewers
			// 
			labelViewers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelViewers.AutoSize = true;
			labelViewers.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			labelViewers.Location = new Point(953, 3);
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
			richTextBoxStreamChat.Location = new Point(6, 37);
			richTextBoxStreamChat.MinimumSize = new Size(719, 363);
			richTextBoxStreamChat.Name = "richTextBoxStreamChat";
			richTextBoxStreamChat.ReadOnly = true;
			richTextBoxStreamChat.Size = new Size(926, 363);
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
			// tabPageUtilities
			// 
			tabPageUtilities.Controls.Add(groupBoxAlertHistory);
			tabPageUtilities.Controls.Add(buttonClearFirst);
			tabPageUtilities.Controls.Add(groupBoxTts);
			tabPageUtilities.Location = new Point(4, 39);
			tabPageUtilities.Name = "tabPageUtilities";
			tabPageUtilities.Size = new Size(1262, 457);
			tabPageUtilities.TabIndex = 3;
			tabPageUtilities.Text = "Utilities";
			tabPageUtilities.UseVisualStyleBackColor = true;
			// 
			// groupBoxAlertHistory
			// 
			groupBoxAlertHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			groupBoxAlertHistory.Controls.Add(buttonReplayAlert);
			groupBoxAlertHistory.Controls.Add(listViewAlertHistory);
			groupBoxAlertHistory.Location = new Point(497, 24);
			groupBoxAlertHistory.Name = "groupBoxAlertHistory";
			groupBoxAlertHistory.Size = new Size(735, 412);
			groupBoxAlertHistory.TabIndex = 18;
			groupBoxAlertHistory.TabStop = false;
			groupBoxAlertHistory.Text = "Alert History";
			// 
			// buttonReplayAlert
			// 
			buttonReplayAlert.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			buttonReplayAlert.Location = new Point(72, 357);
			buttonReplayAlert.Name = "buttonReplayAlert";
			buttonReplayAlert.Size = new Size(101, 49);
			buttonReplayAlert.TabIndex = 1;
			buttonReplayAlert.Text = "Replay";
			buttonReplayAlert.UseVisualStyleBackColor = true;
			buttonReplayAlert.Click += buttonReplayAlert_Click;
			// 
			// listViewAlertHistory
			// 
			listViewAlertHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			listViewAlertHistory.Location = new Point(23, 35);
			listViewAlertHistory.Name = "listViewAlertHistory";
			listViewAlertHistory.Size = new Size(667, 303);
			listViewAlertHistory.TabIndex = 0;
			listViewAlertHistory.UseCompatibleStateImageBehavior = false;
			listViewAlertHistory.DoubleClick += listViewAlertHistory_DoubleClick;
			// 
			// buttonClearFirst
			// 
			buttonClearFirst.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonClearFirst.Location = new Point(33, 180);
			buttonClearFirst.Name = "buttonClearFirst";
			buttonClearFirst.Size = new Size(220, 40);
			buttonClearFirst.TabIndex = 17;
			buttonClearFirst.Text = "Reset First Chatters";
			buttonClearFirst.UseVisualStyleBackColor = true;
			buttonClearFirst.Click += buttonClearFirst_Click;
			// 
			// groupBoxTts
			// 
			groupBoxTts.Controls.Add(buttonTtsReset);
			groupBoxTts.Controls.Add(buttonTtsSkip);
			groupBoxTts.Location = new Point(23, 24);
			groupBoxTts.Name = "groupBoxTts";
			groupBoxTts.Size = new Size(250, 96);
			groupBoxTts.TabIndex = 0;
			groupBoxTts.TabStop = false;
			groupBoxTts.Text = "TTS Controls";
			// 
			// buttonTtsReset
			// 
			buttonTtsReset.Location = new Point(126, 35);
			buttonTtsReset.Name = "buttonTtsReset";
			buttonTtsReset.Size = new Size(94, 37);
			buttonTtsReset.TabIndex = 1;
			buttonTtsReset.Text = "Reset";
			buttonTtsReset.UseVisualStyleBackColor = true;
			buttonTtsReset.Click += buttonTtsReset_Click;
			// 
			// buttonTtsSkip
			// 
			buttonTtsSkip.Location = new Point(10, 35);
			buttonTtsSkip.Name = "buttonTtsSkip";
			buttonTtsSkip.Size = new Size(94, 37);
			buttonTtsSkip.TabIndex = 0;
			buttonTtsSkip.Text = "Skip";
			buttonTtsSkip.UseVisualStyleBackColor = true;
			buttonTtsSkip.Click += buttonTtsSkip_Click;
			// 
			// tabWheel
			// 
			tabWheel.Controls.Add(panelSpinResultOverlay);
			tabWheel.Controls.Add(labelWheelWarning);
			tabWheel.Controls.Add(buttonDeleteWheel);
			tabWheel.Controls.Add(buttonShuffleEntries);
			tabWheel.Controls.Add(buttonAddWheel);
			tabWheel.Controls.Add(buttonRemoveEntry);
			tabWheel.Controls.Add(buttonAddEntry);
			tabWheel.Controls.Add(buttonSaveEntries);
			tabWheel.Controls.Add(buttonSpin);
			tabWheel.Controls.Add(labelWheels);
			tabWheel.Controls.Add(wheelSpinnerControl);
			tabWheel.Controls.Add(dataGridViewEntries);
			tabWheel.Controls.Add(comboBoxWheels);
			tabWheel.Location = new Point(4, 39);
			tabWheel.Name = "tabWheel";
			tabWheel.Padding = new Padding(3);
			tabWheel.Size = new Size(1262, 457);
			tabWheel.TabIndex = 4;
			tabWheel.Text = "Wheel";
			tabWheel.UseVisualStyleBackColor = true;
			// 
			// panelSpinResultOverlay
			// 
			panelSpinResultOverlay.BackColor = Color.Black;
			panelSpinResultOverlay.BorderStyle = BorderStyle.FixedSingle;
			panelSpinResultOverlay.Controls.Add(buttonSpinResultOk);
			panelSpinResultOverlay.Controls.Add(labelSpinResult);
			panelSpinResultOverlay.Location = new Point(556, 172);
			panelSpinResultOverlay.Name = "panelSpinResultOverlay";
			panelSpinResultOverlay.Size = new Size(500, 240);
			panelSpinResultOverlay.TabIndex = 14;
			panelSpinResultOverlay.Visible = false;
			// 
			// buttonSpinResultOk
			// 
			buttonSpinResultOk.BackColor = Color.FromArgb(40, 40, 40);
			buttonSpinResultOk.FlatAppearance.BorderColor = Color.Gold;
			buttonSpinResultOk.FlatAppearance.BorderSize = 2;
			buttonSpinResultOk.FlatStyle = FlatStyle.Flat;
			buttonSpinResultOk.ForeColor = Color.Gold;
			buttonSpinResultOk.Location = new Point(205, 166);
			buttonSpinResultOk.Name = "buttonSpinResultOk";
			buttonSpinResultOk.Size = new Size(100, 40);
			buttonSpinResultOk.TabIndex = 1;
			buttonSpinResultOk.Text = "OK";
			buttonSpinResultOk.UseVisualStyleBackColor = false;
			buttonSpinResultOk.Click += buttonSpinResultOk_Click;
			// 
			// labelSpinResult
			// 
			labelSpinResult.Dock = DockStyle.Top;
			labelSpinResult.Font = new Font("Arial Black", 28.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
			labelSpinResult.ForeColor = Color.Gold;
			labelSpinResult.Location = new Point(0, 0);
			labelSpinResult.Name = "labelSpinResult";
			labelSpinResult.Size = new Size(498, 140);
			labelSpinResult.TabIndex = 0;
			labelSpinResult.TextAlign = ContentAlignment.MiddleCenter;
			// 
			// labelWheelWarning
			// 
			labelWheelWarning.AutoSize = true;
			labelWheelWarning.Location = new Point(300, 91);
			labelWheelWarning.Name = "labelWheelWarning";
			labelWheelWarning.Size = new Size(78, 28);
			labelWheelWarning.TabIndex = 13;
			labelWheelWarning.Text = "label1";
			// 
			// buttonDeleteWheel
			// 
			buttonDeleteWheel.Location = new Point(585, 23);
			buttonDeleteWheel.Name = "buttonDeleteWheel";
			buttonDeleteWheel.Size = new Size(176, 33);
			buttonDeleteWheel.TabIndex = 12;
			buttonDeleteWheel.Text = "Delete Wheel";
			buttonDeleteWheel.UseVisualStyleBackColor = true;
			buttonDeleteWheel.Click += buttonDeleteWheel_Click;
			// buttonShuffleEntries
			// 
			buttonShuffleEntries.Location = new Point(836, 15);
			buttonShuffleEntries.Name = "buttonShuffleEntries";
			buttonShuffleEntries.Size = new Size(100, 36);
			buttonShuffleEntries.TabIndex = 11;
			buttonShuffleEntries.Text = "Shuffle";
			buttonShuffleEntries.UseVisualStyleBackColor = true;
			buttonShuffleEntries.Click += buttonShuffleEntries_Click;
			// 
			// buttonAddWheel
			// 
			buttonAddWheel.Location = new Point(429, 23);
			buttonAddWheel.Name = "buttonAddWheel";
			buttonAddWheel.Size = new Size(150, 37);
			buttonAddWheel.TabIndex = 10;
			buttonAddWheel.Text = "New Wheel";
			buttonAddWheel.UseVisualStyleBackColor = true;
			buttonAddWheel.Click += buttonAddWheel_Click;
			// 
			// buttonRemoveEntry
			// 
			buttonRemoveEntry.Location = new Point(873, 17);
			buttonRemoveEntry.Name = "buttonRemoveEntry";
			buttonRemoveEntry.Size = new Size(143, 34);
			buttonRemoveEntry.TabIndex = 8;
			buttonRemoveEntry.Text = "Remove Entry";
			buttonRemoveEntry.UseVisualStyleBackColor = true;
			buttonRemoveEntry.Click += buttonRemoveEntry_Click;
			// 
			// buttonAddEntry
			// 
			buttonAddEntry.Location = new Point(1022, 16);
			buttonAddEntry.Name = "buttonAddEntry";
			buttonAddEntry.Size = new Size(124, 36);
			buttonAddEntry.TabIndex = 7;
			buttonAddEntry.Text = "Add Entry";
			buttonAddEntry.UseVisualStyleBackColor = true;
			buttonAddEntry.Click += buttonAddEntry_Click;
			// 
			// buttonSaveEntries
			// 
			buttonSaveEntries.Enabled = false;
			buttonSaveEntries.Location = new Point(1152, 19);
			buttonSaveEntries.Name = "buttonSaveEntries";
			buttonSaveEntries.Size = new Size(94, 32);
			buttonSaveEntries.TabIndex = 6;
			buttonSaveEntries.Text = "Save";
			buttonSaveEntries.UseVisualStyleBackColor = true;
			buttonSaveEntries.Click += buttonSaveEntries_Click;
			// 
			// buttonSpin
			// 
			buttonSpin.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			buttonSpin.BackColor = Color.Black;
			buttonSpin.ForeColor = Color.White;
			buttonSpin.Location = new Point(300, 404);
			buttonSpin.Name = "buttonSpin";
			buttonSpin.Size = new Size(102, 34);
			buttonSpin.TabIndex = 4;
			buttonSpin.Text = "SPIN";
			buttonSpin.UseVisualStyleBackColor = false;
			buttonSpin.Click += buttonSpin_Click;
			// 
			// labelWheels
			// 
			labelWheels.AutoSize = true;
			labelWheels.Location = new Point(6, 24);
			labelWheels.Name = "labelWheels";
			labelWheels.Size = new Size(79, 28);
			labelWheels.TabIndex = 3;
			labelWheels.Text = "Wheel";
			// 
			// wheelSpinnerControl
			// 
			wheelSpinnerControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
			wheelSpinnerControl.BackColor = Color.White;
			wheelSpinnerControl.Location = new Point(20, 66);
			wheelSpinnerControl.Name = "wheelSpinnerControl";
			wheelSpinnerControl.RotationAngle = 0F;
			wheelSpinnerControl.Size = new Size(699, 332);
			wheelSpinnerControl.TabIndex = 9;
			// 
			// dataGridViewEntries
			// 
			dataGridViewEntries.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			dataGridViewEntries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle2.BackColor = SystemColors.Control;
			dataGridViewCellStyle2.Font = new Font("Arial Black", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
			dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
			dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
			dataGridViewEntries.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
			dataGridViewEntries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewEntries.Location = new Point(836, 66);
			dataGridViewEntries.Name = "dataGridViewEntries";
			dataGridViewEntries.RowHeadersWidth = 51;
			dataGridViewEntries.Size = new Size(410, 372);
			dataGridViewEntries.TabIndex = 1;
			dataGridViewEntries.CellValueChanged += dataGridViewEntries_CellValueChanged;
			dataGridViewEntries.CurrentCellDirtyStateChanged += dataGridViewEntries_CurrentCellDirtyStateChanged;
			// 
			// comboBoxWheels
			// 
			comboBoxWheels.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBoxWheels.FormattingEnabled = true;
			comboBoxWheels.Location = new Point(91, 21);
			comboBoxWheels.Name = "comboBoxWheels";
			comboBoxWheels.Size = new Size(287, 36);
			comboBoxWheels.TabIndex = 0;
			comboBoxWheels.SelectedIndexChanged += comboBoxWheels_SelectedIndexChanged;
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
			tabTesting.Size = new Size(1262, 457);
			tabTesting.TabIndex = 2;
			tabTesting.Text = "Testing";
			// 
			// groupBoxButtonTests
			// 
			groupBoxButtonTests.Controls.Add(buttonTestDonation);
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
			groupBoxButtonTests.Location = new Point(24, 45);
			groupBoxButtonTests.Name = "groupBoxButtonTests";
			groupBoxButtonTests.Size = new Size(466, 359);
			groupBoxButtonTests.TabIndex = 18;
			groupBoxButtonTests.TabStop = false;
			groupBoxButtonTests.Text = "Button Tests";
			// 
			// buttonTestHypeTrainEnd
			// 
			buttonTestHypeTrainEnd.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestHypeTrainEnd.Location = new Point(10, 295);
			buttonTestHypeTrainEnd.Name = "buttonTestHypeTrainEnd";
			buttonTestHypeTrainEnd.Size = new Size(163, 40);
			buttonTestHypeTrainEnd.TabIndex = 17;
			buttonTestHypeTrainEnd.Text = "Hype Train End";
			buttonTestHypeTrainEnd.UseVisualStyleBackColor = true;
			buttonTestHypeTrainEnd.Click += buttonTestHypeTrainEnd_Click;
			// 
			// buttonTestWatchStreak
			// 
			buttonTestWatchStreak.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestWatchStreak.Location = new Point(10, 249);
			buttonTestWatchStreak.Name = "buttonTestWatchStreak";
			buttonTestWatchStreak.Size = new Size(163, 40);
			buttonTestWatchStreak.TabIndex = 16;
			buttonTestWatchStreak.Text = "Watch Streak";
			buttonTestWatchStreak.UseVisualStyleBackColor = true;
			buttonTestWatchStreak.Click += buttonTestWatchStreak_Click;
			// 
			// labelUserName
			// 
			labelUserName.AutoSize = true;
			labelUserName.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			labelUserName.Location = new Point(10, 35);
			labelUserName.Name = "labelUserName";
			labelUserName.Size = new Size(118, 24);
			labelUserName.TabIndex = 0;
			labelUserName.Text = "User Name:";
			// 
			// textUserName
			// 
			textUserName.Font = new Font("Arial", 10F);
			textUserName.Location = new Point(160, 29);
			textUserName.Name = "textUserName";
			textUserName.Size = new Size(210, 27);
			textUserName.TabIndex = 1;
			// 
			// labelAmount
			// 
			labelAmount.AutoSize = true;
			labelAmount.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			labelAmount.Location = new Point(41, 73);
			labelAmount.Name = "labelAmount";
			labelAmount.Size = new Size(87, 24);
			labelAmount.TabIndex = 2;
			labelAmount.Text = "Amount:";
			// 
			// textAmount
			// 
			textAmount.Font = new Font("Arial", 10F);
			textAmount.Location = new Point(160, 69);
			textAmount.Name = "textAmount";
			textAmount.Size = new Size(210, 27);
			textAmount.TabIndex = 3;
			// 
			// buttonTestFollow
			// 
			buttonTestFollow.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestFollow.Location = new Point(10, 109);
			buttonTestFollow.Name = "buttonTestFollow";
			buttonTestFollow.Size = new Size(163, 40);
			buttonTestFollow.TabIndex = 15;
			buttonTestFollow.Text = "Follow";
			buttonTestFollow.UseVisualStyleBackColor = true;
			buttonTestFollow.Click += buttonTestFollow_Click;
			// 
			// buttonTestSub
			// 
			buttonTestSub.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestSub.Location = new Point(207, 109);
			buttonTestSub.Name = "buttonTestSub";
			buttonTestSub.Size = new Size(163, 40);
			buttonTestSub.TabIndex = 0;
			buttonTestSub.Text = "Subscription";
			buttonTestSub.UseVisualStyleBackColor = true;
			buttonTestSub.Click += buttonTestSub_Click;
			// 
			// buttonTestReSub
			// 
			buttonTestReSub.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestReSub.Location = new Point(207, 245);
			buttonTestReSub.Name = "buttonTestReSub";
			buttonTestReSub.Size = new Size(163, 40);
			buttonTestReSub.TabIndex = 2;
			buttonTestReSub.Text = "ReSub";
			buttonTestReSub.UseVisualStyleBackColor = true;
			buttonTestReSub.Click += buttonTestReSub_Click;
			// 
			// buttonTestSubGift
			// 
			buttonTestSubGift.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestSubGift.Location = new Point(207, 155);
			buttonTestSubGift.Name = "buttonTestSubGift";
			buttonTestSubGift.Size = new Size(163, 40);
			buttonTestSubGift.TabIndex = 3;
			buttonTestSubGift.Text = "Gift Subs";
			buttonTestSubGift.UseVisualStyleBackColor = true;
			buttonTestSubGift.Click += buttonTestSubGift_Click;
			// 
			// buttonTestRaid
			// 
			buttonTestRaid.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestRaid.Location = new Point(10, 157);
			buttonTestRaid.Name = "buttonTestRaid";
			buttonTestRaid.Size = new Size(163, 40);
			buttonTestRaid.TabIndex = 1;
			buttonTestRaid.Text = "Raid";
			buttonTestRaid.UseVisualStyleBackColor = true;
			buttonTestRaid.Click += buttonTestRaid_Click;
			// 
			// buttonTestMysteryGift
			// 
			buttonTestMysteryGift.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestMysteryGift.Location = new Point(207, 200);
			buttonTestMysteryGift.Name = "buttonTestMysteryGift";
			buttonTestMysteryGift.Size = new Size(163, 40);
			buttonTestMysteryGift.TabIndex = 7;
			buttonTestMysteryGift.Text = "Mystery Gifts";
			buttonTestMysteryGift.UseVisualStyleBackColor = true;
			buttonTestMysteryGift.Click += buttonTestMysteryGift_Click;
			// 
			// buttonTestHypeTrain
			// 
			buttonTestHypeTrain.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonTestHypeTrain.Location = new Point(10, 203);
			buttonTestHypeTrain.Name = "buttonTestHypeTrain";
			buttonTestHypeTrain.Size = new Size(163, 40);
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
			groupBoxTextEntryTests.Location = new Point(569, 45);
			groupBoxTextEntryTests.Name = "groupBoxTextEntryTests";
			groupBoxTextEntryTests.Size = new Size(585, 359);
			groupBoxTextEntryTests.TabIndex = 17;
			groupBoxTextEntryTests.TabStop = false;
			groupBoxTextEntryTests.Text = "Text Entry Tests";
			// 
			// comboBoxTtsSpeaker
			// 
			comboBoxTtsSpeaker.FormattingEnabled = true;
			comboBoxTtsSpeaker.Items.AddRange(new object[] { "Matthew", "Joanna", "Brian", "Emma", "Justin", "Kimberly", "Amy", "Russel", "Nicole", "Olivia", "Stephen", "Salli", "Joey", "Ivy", "Kendra" });
			comboBoxTtsSpeaker.Location = new Point(209, 260);
			comboBoxTtsSpeaker.Name = "comboBoxTtsSpeaker";
			comboBoxTtsSpeaker.Size = new Size(151, 40);
			comboBoxTtsSpeaker.TabIndex = 17;
			comboBoxTtsSpeaker.Text = "Matthew";
			// 
			// textBoxTtsText
			// 
			textBoxTtsText.Location = new Point(366, 260);
			textBoxTtsText.Name = "textBoxTtsText";
			textBoxTtsText.Size = new Size(206, 40);
			textBoxTtsText.TabIndex = 16;
			textBoxTtsText.KeyDown += textBoxTtsText_KeyDown;
			// 
			// buttonTestTts
			// 
			buttonTestTts.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
			buttonTestTts.Location = new Point(6, 260);
			buttonTestTts.Name = "buttonTestTts";
			buttonTestTts.Size = new Size(163, 40);
			buttonTestTts.TabIndex = 15;
			buttonTestTts.Text = "Text to Speech";
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
			// textFirstChatter
			// 
			textFirstChatter.Location = new Point(209, 213);
			textFirstChatter.Name = "textFirstChatter";
			textFirstChatter.Size = new Size(206, 40);
			textFirstChatter.TabIndex = 14;
			textFirstChatter.KeyDown += textFirstChatter_KeyDown;
			// 
			// buttonTestFirstChat
			// 
			buttonTestFirstChat.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
			buttonTestFirstChat.Location = new Point(6, 214);
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
			buttonTestChannelPoint.Location = new Point(6, 109);
			buttonTestChannelPoint.Name = "buttonTestChannelPoint";
			buttonTestChannelPoint.Size = new Size(163, 40);
			buttonTestChannelPoint.TabIndex = 9;
			buttonTestChannelPoint.Text = "Channel Points";
			buttonTestChannelPoint.UseVisualStyleBackColor = true;
			buttonTestChannelPoint.Click += buttonTestChannelPoint_Click;
			// 
			// textCommand
			// 
			textCommand.Location = new Point(209, 167);
			textCommand.Name = "textCommand";
			textCommand.Size = new Size(206, 40);
			textCommand.TabIndex = 12;
			textCommand.KeyDown += textCommand_KeyDown;
			// 
			// textChannelPoint
			// 
			textChannelPoint.Location = new Point(209, 109);
			textChannelPoint.Name = "textChannelPoint";
			textChannelPoint.Size = new Size(206, 40);
			textChannelPoint.TabIndex = 10;
			textChannelPoint.KeyDown += textChannelPoint_KeyDown;
			// 
			// buttonTestCommand
			// 
			buttonTestCommand.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
			buttonTestCommand.Location = new Point(6, 167);
			buttonTestCommand.Name = "buttonTestCommand";
			buttonTestCommand.Size = new Size(163, 40);
			buttonTestCommand.TabIndex = 11;
			buttonTestCommand.Text = "Commands";
			buttonTestCommand.UseVisualStyleBackColor = true;
			buttonTestCommand.Click += buttonTestCommand_Click;
			// 
			// tabPageLogging
			// 
			tabPageLogging.BorderStyle = BorderStyle.Fixed3D;
			tabPageLogging.Controls.Add(buttonCopyAllLog);
			tabPageLogging.Controls.Add(buttonClearLog);
			tabPageLogging.Controls.Add(textBoxLogging);
			tabPageLogging.Controls.Add(labelLogging);
			tabPageLogging.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			tabPageLogging.Location = new Point(4, 39);
			tabPageLogging.Name = "tabPageLogging";
			tabPageLogging.Padding = new Padding(3);
			tabPageLogging.Size = new Size(1262, 457);
			tabPageLogging.TabIndex = 1;
			tabPageLogging.Text = "Logging";
			tabPageLogging.UseVisualStyleBackColor = true;
			// 
			// textBoxLogging
			// 
			textBoxLogging.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			textBoxLogging.BackColor = Color.Black;
			textBoxLogging.Font = new Font("Arial Black", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
			textBoxLogging.ForeColor = Color.White;
			textBoxLogging.Location = new Point(6, 37);
			textBoxLogging.MinimumSize = new Size(1025, 365);
			textBoxLogging.Multiline = true;
			textBoxLogging.Name = "textBoxLogging";
			textBoxLogging.ReadOnly = true;
			textBoxLogging.ScrollBars = ScrollBars.Both;
			textBoxLogging.Size = new Size(1216, 365);
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
			labelLogging.Text = "Application Log";
			// 
			// buttonClearLog
			// 
			buttonClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			buttonClearLog.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonClearLog.Location = new Point(6, 410);
			buttonClearLog.Name = "buttonClearLog";
			buttonClearLog.Size = new Size(140, 37);
			buttonClearLog.TabIndex = 2;
			buttonClearLog.Text = "Clear Log";
			buttonClearLog.UseVisualStyleBackColor = true;
			buttonClearLog.Click += buttonClearLog_Click;
			// 
			// buttonCopyAllLog
			// 
			buttonCopyAllLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			buttonCopyAllLog.Font = new Font("Arial Black", 10.2F, FontStyle.Bold);
			buttonCopyAllLog.Location = new Point(152, 410);
			buttonCopyAllLog.Name = "buttonCopyAllLog";
			buttonCopyAllLog.Size = new Size(140, 37);
			buttonCopyAllLog.TabIndex = 3;
			buttonCopyAllLog.Text = "Copy All";
			buttonCopyAllLog.UseVisualStyleBackColor = true;
			buttonCopyAllLog.Click += buttonCopyAllLog_Click;
			// 
			// buttonTestBot
			// 
			buttonTestBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			buttonTestBot.BackColor = Color.Yellow;
			buttonTestBot.Location = new Point(16, 517);
			buttonTestBot.Name = "buttonTestBot";
			buttonTestBot.Size = new Size(127, 49);
			buttonTestBot.TabIndex = 3;
			buttonTestBot.Text = "Test Bot";
			buttonTestBot.UseVisualStyleBackColor = false;
			buttonTestBot.Click += buttonTestBot_Click;
			// 
			// buttonStopBot
			// 
			buttonStopBot.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonStopBot.BackColor = Color.Crimson;
			buttonStopBot.Enabled = false;
			buttonStopBot.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
			buttonStopBot.Location = new Point(1018, 517);
			buttonStopBot.Name = "buttonStopBot";
			buttonStopBot.Size = new Size(127, 49);
			buttonStopBot.TabIndex = 4;
			buttonStopBot.Text = "Stop Bot";
			buttonStopBot.UseVisualStyleBackColor = false;
			buttonStopBot.Click += buttonStopBot_Click;
			// 
			// buttonTestDonation
			// 
			buttonTestDonation.Font = new Font("Arial Black", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
			buttonTestDonation.Location = new Point(207, 291);
			buttonTestDonation.Name = "buttonTestDonation";
			buttonTestDonation.Size = new Size(163, 40);
			buttonTestDonation.TabIndex = 18;
			buttonTestDonation.Text = "Donations";
			buttonTestDonation.UseVisualStyleBackColor = true;
			buttonTestDonation.Click += buttonTestDonation_Click;
			// 
			// toolTipMain
			// 
			toolTipMain.SetToolTip(buttonTtsSkip, "Stops the current speech during generation or active playback; later speech remains queued.");
			toolTipMain.SetToolTip(buttonTtsReset, "Stops current speech and clears all pending speech without affecting other alerts.");
			toolTipMain.SetToolTip(buttonClearFirst, "Clears the current first-chatter tracking.");
			toolTipMain.SetToolTip(buttonReplayAlert, "Replays the selected alert.");
			toolTipMain.SetToolTip(buttonShuffleEntries, "Randomizes the displayed entry order.");
			toolTipMain.SetToolTip(buttonSaveEntries, "Persists changes to wheel entries.");
			toolTipMain.SetToolTip(buttonSpin, "Selects a winner using the configured weights.");
			toolTipMain.SetToolTip(buttonDeleteWheel, "Permanently deletes the selected wheel.");
			toolTipMain.SetToolTip(comboBoxWheels, "Selects the wheel to view, edit, or spin.");
			toolTipMain.SetToolTip(buttonAddWheel, "Creates a new wheel with one default entry.");
			toolTipMain.SetToolTip(buttonAddEntry, "Adds a new unsaved entry to the selected wheel.");
			toolTipMain.SetToolTip(buttonRemoveEntry, "Removes the selected entry from the edit list; use Save to persist the removal.");
			toolTipMain.SetToolTip(dataGridViewEntries, "Edit wheel entries here; use Save to persist changes.");
			toolTipMain.SetToolTip(buttonSpinResultOk, "Confirms the winner and dispatches its configured wheel result.");
			toolTipMain.SetToolTip(buttonTestFollow, "Simulates a follow through production alert services; may trigger alerts, TTS, or external API usage.");
			toolTipMain.SetToolTip(buttonTestSub, "Simulates a subscription using User Name; may trigger alerts, TTS, or external API usage.");
			toolTipMain.SetToolTip(buttonTestReSub, "Simulates a resubscription using User Name and Amount as months; may trigger alerts, TTS, or external API usage.");
			toolTipMain.SetToolTip(buttonTestSubGift, "Simulates a gifted subscription using User Name as the gifter and Amount as the recipient; may trigger alerts or TTS.");
			toolTipMain.SetToolTip(buttonTestRaid, "Simulates a raid using User Name and Amount as viewer count; may trigger alerts, TTS, or external API usage.");
			toolTipMain.SetToolTip(buttonTestMysteryGift, "Simulates a mystery gift bundle using Amount as the subscription count; may trigger alerts, TTS, or external API usage.");
			toolTipMain.SetToolTip(buttonTestHypeTrain, "Simulates a Hype Train start through production alert services; may trigger alerts or TTS.");
			toolTipMain.SetToolTip(buttonTestHypeTrainEnd, "Simulates a Hype Train end with predefined test statistics; may trigger alerts or TTS.");
			toolTipMain.SetToolTip(buttonTestWatchStreak, "Simulates a watch-streak notice using User Name and Amount as the streak count; may trigger alerts, TTS, or external API usage.");
			toolTipMain.SetToolTip(buttonTestDonation, "Simulates a donation using User Name and Amount through production donation services; may trigger alerts, TTS, or external API usage.");
			toolTipMain.SetToolTip(buttonTestCheer, "Simulates a cheer using the entered Bits value; may trigger alerts or TTS.");
			toolTipMain.SetToolTip(buttonTestChannelPoint, "Simulates the entered reward through production channel-point alert handling; may trigger configured alerts or services.");
			toolTipMain.SetToolTip(buttonTestCommand, "Runs the entered command through production command handling; may send real chat or invoke alerts and external services.");
			toolTipMain.SetToolTip(buttonTestFirstChat, "Simulates first chat through production handling; may update first-chatter data, send chat, or trigger alerts.");
			toolTipMain.SetToolTip(buttonTestTts, "Sends the entered text and selected voice to production TTS; may call AWS Polly and incur usage.");
			toolTipMain.SetToolTip(textUserName, "Used as the simulated username or donor name by applicable tests; blank uses TestUser.");
			toolTipMain.SetToolTip(textAmount, "Used as raid viewers, resub months, gift recipient, mystery-gift count, watch-streak count, or donation amount, depending on the test.");
			toolTipMain.SetToolTip(buttonStartBot, "Starts live mode and begins the configured bot, overlay, Twitch, EventSub, and optional provider services.");
			toolTipMain.SetToolTip(buttonTestBot, "Starts Test Bot mode using production services; it can still connect externally, send chat, trigger alerts, and incur API usage.");
			toolTipMain.SetToolTip(buttonStopBot, "Attempts to stop the current bot session, disconnect integrations, and flush pending state.");
			toolTipMain.SetToolTip(buttonClearLog, "Clears the displayed log without disabling logging or deleting log files.");
			toolTipMain.SetToolTip(buttonCopyAllLog, "Copies all displayed log text to the clipboard.");
			// 
			// TwitchChatBot
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1296, 587);
			Controls.Add(buttonStopBot);
			Controls.Add(buttonTestBot);
			Controls.Add(tabControlChatBot);
			Controls.Add(buttonStartBot);
			Icon = (Icon)resources.GetObject("$this.Icon");
			MinimumSize = new Size(1168, 623);
			Name = "TwitchChatBot";
			Text = "Twitch Chat Bot";
			FormClosing += TwitchChatBot_FormClosing;
			tabControlChatBot.ResumeLayout(false);
			tabPageMainChat.ResumeLayout(false);
			tabPageMainChat.PerformLayout();
			tabPageUtilities.ResumeLayout(false);
			groupBoxAlertHistory.ResumeLayout(false);
			groupBoxTts.ResumeLayout(false);
			tabWheel.ResumeLayout(false);
			tabWheel.PerformLayout();
			panelSpinResultOverlay.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)dataGridViewEntries).EndInit();
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
		private Button buttonClearLog;
		private Button buttonCopyAllLog;
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
		private TabPage tabWheel;
		private DataGridView dataGridViewEntries;
		private ComboBox comboBoxWheels;
		private Label labelWheels;
		private Button buttonSpin;
		private Button buttonSaveEntries;
		private Button buttonAddEntry;
		private Button buttonRemoveEntry;
		private WheelSpinnerControl wheelSpinnerControl;
		private Button buttonAddWheel;
		private Button buttonShuffleEntries;
		private Button buttonDeleteWheel;
		private Label labelWheelWarning;
		private Panel panelSpinResultOverlay;
		private Button buttonSpinResultOk;
		private Label labelSpinResult;
		private Button buttonTestDonation;
		private ToolTip toolTipMain;
	}
}
