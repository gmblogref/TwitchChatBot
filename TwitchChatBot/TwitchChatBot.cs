using TwitchChatBot.Core.Controller;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;

namespace TwitchChatBot
{
    public partial class TwitchChatBot : Form, IUiBridge
    {
        private readonly ChatBotController _chatBotController;
        private readonly ITestUtilityService _testUtilityService;
        private readonly ITtsService _ttsService;
        private readonly IAlertHistoryService _alertHistoryService;
        private readonly IAlertReplayService _alertReplayService;
        private readonly IAppFlags _appFlags;

        public TwitchChatBot(ChatBotController chatBotController,
            ITestUtilityService testUtilityService,
            ITtsService ttsService,
            IAlertHistoryService alertHistoryService,
            IAlertReplayService alertReplayService,
            IAppFlags appFlags)
        {
            InitializeComponent();
            _chatBotController = chatBotController;
            _testUtilityService = testUtilityService;
            _ttsService = ttsService;
            _alertHistoryService = alertHistoryService;
            _alertReplayService = alertReplayService;
            _appFlags = appFlags;

            InitializeAlertHistoryUi();
        }

        public void AppendChat(string username, string message, Color nameColor)
        {
            if (richTextBoxStreamChat.InvokeRequired)
            {
                richTextBoxStreamChat.Invoke(() => AppendChat(username, message, nameColor));
            }
            else
            {
                int start = richTextBoxStreamChat.TextLength;

                // Fix Name Color
                if (IsTooDark(nameColor))
                {
                    nameColor = Color.White; // fallback to white if too dark
                }

                // Append username
                richTextBoxStreamChat.AppendText(username);
                richTextBoxStreamChat.Select(start, username.Length);
                richTextBoxStreamChat.SelectionColor = nameColor;

                // Append message
                richTextBoxStreamChat.AppendText(": " + message + Environment.NewLine);
                richTextBoxStreamChat.SelectionColor = richTextBoxStreamChat.ForeColor; // Reset to default
                richTextBoxStreamChat.ScrollToCaret();
            }
        }

        public void AppendLog(string message)
        {
            if (textBoxLogging.InvokeRequired)
            {
                textBoxLogging.Invoke(() => textBoxLogging.AppendText(message + Environment.NewLine));
            }
            else
            {
                textBoxLogging.AppendText(message + Environment.NewLine);
            }
        }

        public void SetViewerListByGroup(List<ViewerEntry> groupedViewers)
        {
            if (richTextBoxViewers.InvokeRequired)
            {
                richTextBoxViewers.Invoke(() => SetViewerListByGroup(groupedViewers));
                return;
            }

            richTextBoxViewers.Clear();

            string? currentRole = null;

            foreach (var viewer in groupedViewers)
            {
                if (viewer.Role != currentRole)
                {
                    currentRole = viewer.Role;
                    richTextBoxViewers.SelectionFont = new Font(richTextBoxViewers.Font, FontStyle.Bold);
                    richTextBoxViewers.SelectionColor = Color.White;
                    richTextBoxViewers.AppendText($"-- {currentRole.ToUpper()}S --{Environment.NewLine}");
                }

                Color color = viewer.Role switch
                {
                    "mod" => Color.DeepSkyBlue,
                    "vip" => Color.MediumPurple,
                    _ => Color.Gray
                };

                richTextBoxViewers.SelectionFont = new Font(richTextBoxViewers.Font, FontStyle.Regular);
                richTextBoxViewers.SelectionColor = color;
                richTextBoxViewers.AppendText($"{viewer.Username}{Environment.NewLine}");
            }

            richTextBoxViewers.SelectionColor = richTextBoxViewers.ForeColor;
        }

        private void InitializeAlertHistoryUi()
        {
            // Columns
            listViewAlertHistory.Columns.Clear();
            listViewAlertHistory.Columns.Add("Time", 90);
            listViewAlertHistory.Columns.Add("Type", 140);
            listViewAlertHistory.Columns.Add("User", 140);
            listViewAlertHistory.Columns.Add("Summary", 600);

            listViewAlertHistory.View = View.Details;
            listViewAlertHistory.FullRowSelect = true;
            listViewAlertHistory.GridLines = true;
            listViewAlertHistory.HideSelection = false;
            listViewAlertHistory.MultiSelect = false;

            // Seed from snapshot
            foreach (var e in _alertHistoryService.Snapshot())
                AddHistoryRow(e);

            // Live updates
            _alertHistoryService.EntryAdded += entry =>
            {
                if (InvokeRequired)
                    BeginInvoke((Action)(() => AddHistoryRow(entry)));
                else
                    AddHistoryRow(entry);
            };
        }

        private void AddHistoryRow(AlertHistoryEntry e)
        {
            var lvi = new ListViewItem(new[]
            {
                e.Timestamp.ToString("HH:mm:ss"),
                e.Type,
                e.Username ?? string.Empty,
                e.Display
            })
            { Tag = e };

            listViewAlertHistory.Items.Add(lvi);
            listViewAlertHistory.Columns[listViewAlertHistory.Columns.Count - 1].Width = -2;

            if (listViewAlertHistory.Items.Count > 0)
                listViewAlertHistory.EnsureVisible(listViewAlertHistory.Items.Count - 1);
        }

        private async void ReplaySelectedAlert()
        {
            if (listViewAlertHistory.SelectedItems.Count == 0)
                return;

            if (listViewAlertHistory.SelectedItems[0].Tag is AlertHistoryEntry entry)
                await _alertReplayService.ReplayAsync(entry);
        }

        private bool IsTooDark(Color color)
        {
            // Basic luminance check
            double brightness = color.GetBrightness(); // 0 = dark, 1 = bright
            return brightness < 0.3;
        }

        private void buttonStartBot_EnabledChanged(object sender, EventArgs e)
        {
            Button currentButton = (Button)sender;
            if (!currentButton.Enabled)
            {
                currentButton.BackColor = Color.Gray;
            }
            else
            {
                currentButton.BackColor = Color.Green;
            }
        }

        private async void buttonTestSub_Click(object sender, EventArgs e)
        {
            string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();
            await _testUtilityService.TriggerSubTestAsync(userName);
        }

        private async void buttonTestRaid_Click(object sender, EventArgs e)
        {
            string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();
            int viewers = int.TryParse(textAmount.Text?.Trim(), out var parsed) ? parsed : 123;

            await _testUtilityService.TriggerRaidTestAsync(userName, viewers);
        }

        private async void buttonTestReSub_Click(object sender, EventArgs e)
        {
            string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();
            int months = int.TryParse(textAmount.Text?.Trim(), out var parsed) ? parsed : 12;

            await _testUtilityService.TriggerReSubTestAsync(userName, months);
        }

        private async void buttonTestSubGift_Click(object sender, EventArgs e)
        {
            string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();
            string recipient = string.IsNullOrWhiteSpace(textAmount.Text) ? "RecipientUser" : textAmount.Text.Trim();
            await _testUtilityService.TriggerSubGiftTestAsync(userName, recipient);
        }

        private async void buttonTestHypeTrain_Click(object sender, EventArgs e) =>
            await _testUtilityService.TriggerHypeTrainTestAsync();

        private async void buttonTestCheer_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBits.Text, out var bits))
                await _testUtilityService.TriggerCheerTestAsync(bits);
        }

        private async void buttonTestMysteryGift_Click(object sender, EventArgs e)
        {
            string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();
            int subs = int.TryParse(textAmount.Text?.Trim(), out var parsed) ? parsed : 12;

            await _testUtilityService.TriggerSubMysteryGiftTestAsync(userName, subs);
        }

        private async void buttonTestChannelPoint_Click(object sender, EventArgs e)
        {
            var redemption = textChannelPoint.Text.Trim();
            if (!string.IsNullOrEmpty(redemption))
                await _testUtilityService.TriggerChannelPointTestAsync(redemption);
        }

        private async void buttonTestCommand_Click(object sender, EventArgs e)
        {
            var command = textCommand.Text.Trim();
            if (!string.IsNullOrEmpty(command))
                await _testUtilityService.TriggerCommandTestAsync(command);
        }

        private async void buttonTestFirstChat_Click(object sender, EventArgs e)
        {
            var user = textFirstChatter.Text.Trim();
            if (!string.IsNullOrEmpty(user))
                await _testUtilityService.TriggerFirstChatTestAsync(user);
        }

        private void buttonTestFollow_Click(object sender, EventArgs e)
        {
            string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();

            _testUtilityService.TriggerFollowTest(userName);
        }

        private void buttonClearFirst_Click(object sender, EventArgs e)
        {
            _testUtilityService.TriggerFirstChatClear();
        }

        private void textBits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestCheer.PerformClick();
                textBits.Text = string.Empty;
            }
        }

        private void textChannelPoint_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestChannelPoint.PerformClick();
                textChannelPoint.Text = string.Empty;
            }
        }

        private void textCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestCommand.PerformClick();
                textCommand.Text = string.Empty;
            }
        }

        private void textFirstChatter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestFirstChat.PerformClick();
                textFirstChatter.Text = string.Empty;
            }
        }

        private void tabControlChatBot_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage tab = tabControlChatBot.TabPages[e.Index];
            Rectangle tabRect = tabControlChatBot.GetTabRect(e.Index);
            bool isSelected = (e.Index == tabControlChatBot.SelectedIndex);

            // Background
            e.Graphics.FillRectangle(isSelected ? Brushes.White : Brushes.LightGray, tabRect);

            // Border
            e.Graphics.DrawRectangle(Pens.Black, tabRect);

            // Text
            TextRenderer.DrawText(
                e.Graphics,
                tab.Text,
                tab.Font,
                tabRect,
                Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        private async void buttonTestTts_Click(object sender, EventArgs e)
        {
            string text = textBoxTtsText.Text.Trim();
            string speaker = comboBoxTtsSpeaker.Text.Trim();

            if (text != string.Empty)
            {
                await _testUtilityService.TriggerTextToSpeech(text, speaker);
            }
        }

        private void textBoxTtsText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestTts.PerformClick();
                textBoxTtsText.Text = string.Empty;
            }
        }

        private async void buttonTestBot_Click(object sender, EventArgs e)
        {
            try
            {
                _appFlags.IsTesting = true;
                await _chatBotController.StartAsync();
                buttonStartBot.Enabled = false;
                buttonTestBot.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting bot: {ex.Message}");
            }
        }

        private async void buttonStartBot_Click(object sender, EventArgs e)
        {
            try
            {
                _appFlags.IsTesting = false;
                await _chatBotController.StartAsync();
                buttonStartBot.Enabled = false;
                buttonTestBot.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting bot: {ex.Message}");
            }
        }

        private async void buttonStopBot_Click(object sender, EventArgs e)
        {
            try
            {
                await _chatBotController.StopAsync();
                buttonStartBot.Enabled = true;
                buttonTestBot.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping bot: {ex.Message}");
            }
        }

        private async void TwitchChatBot_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                await _chatBotController.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing: {ex.Message}");
            }
        }

        private void buttonTtsSkip_Click(object sender, EventArgs e)
        {
            _ttsService.SkipCurrent();
        }

        private void buttonTtsReset_Click(object sender, EventArgs e)
        {
            _ttsService.ResetQueue();
        }

        private void buttonReplayAlert_Click(object sender, EventArgs e)
        {
            ReplaySelectedAlert();
        }

        private void listViewAlertHistory_DoubleClick(object sender, EventArgs e)
        {
            ReplaySelectedAlert();
        }
    }
}