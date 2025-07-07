using Microsoft.AspNetCore.Mvc;
using System.Windows.Forms;
using TwitchChatBot.Core.Controller;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot
{
    public partial class TwitchChatBot : Form, IUiBridge
    {
        private readonly ChatBotController _chatBotController;
        private readonly ITestUtilityService _testUtilityService;

        public TwitchChatBot(ChatBotController chatBotController, ITestUtilityService testUtilityService)
        {
            InitializeComponent();
            _chatBotController = chatBotController;
            _testUtilityService = testUtilityService;
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

        private bool IsTooDark(Color color)
        {
            // Basic luminance check
            double brightness = color.GetBrightness(); // 0 = dark, 1 = bright
            return brightness < 0.3;
        }

        private async void startBotButton_Click(object sender, EventArgs e)
        {
            try
            {
                await _chatBotController.StartAsync();
                buttonStartBot.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting bot: {ex.Message}");
            }
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

        private async void buttonTestSub_Click(object sender, EventArgs e) =>
            await _testUtilityService.TriggerSubTestAsync();

        private async void buttonTestRaid_Click(object sender, EventArgs e) =>
            await _testUtilityService.TriggerRaidTestAsync();

        private async void buttonTestReSub_Click(object sender, EventArgs e) =>
            await _testUtilityService.TriggerReSubTestAsync();

        private async void buttonTestSubGift_Click(object sender, EventArgs e) =>
            await _testUtilityService.TriggerSubGiftTestAsync();

        private async void buttonTestHypeTrain_Click(object sender, EventArgs e) =>
            await _testUtilityService.TriggerHypeTrainTestAsync();

        private async void buttonTestCheer_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBits.Text, out var bits))
                await _testUtilityService.TriggerCheerTestAsync(bits);
        }

        private async void buttonTestMysteryGift_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textMysterySubs.Text, out var subs))
                await _testUtilityService.TriggerSubMysteryGiftTestAsync(subs);
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
            _testUtilityService.TriggerFollowTest();
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

        private void textMysterySubs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestMysteryGift.PerformClick();
                textMysterySubs.Text = string.Empty;
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
    }
}