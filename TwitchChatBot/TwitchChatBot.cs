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

                // Append username
                richTextBoxStreamChat.AppendText(username);
                richTextBoxStreamChat.Select(start, username.Length);
                richTextBoxStreamChat.SelectionColor = nameColor;

                // Append message
                richTextBoxStreamChat.AppendText(": " + message + Environment.NewLine);
                richTextBoxStreamChat.SelectionColor = richTextBoxStreamChat.ForeColor; // Reset to default
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

        public void SetViewerList(IEnumerable<string> viewers)
        {
            if(listBoxCurrentViewers.InvokeRequired)
            {
                listBoxCurrentViewers.Invoke(() => listBoxCurrentViewers.Items.AddRange(viewers.ToArray()));
            }
            else
            {
                listBoxCurrentViewers.Items.Clear();
                listBoxCurrentViewers.DataSource = viewers;
            }
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
            }
        }

        private void textMysterySubs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestMysteryGift.PerformClick();
            }
        }

        private void textChannelPoint_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestChannelPoint.PerformClick();
            }
        }

        private void textCommand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestCommand.PerformClick();
            }
        }

        private void textFirstChatter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonTestFirstChat.PerformClick();
            }
        }
    }
}