using TwitchChatBot.Core.Controller;
using TwitchChatBot.Core.Services.Contracts;

namespace TwitchChatBot
{
    public partial class TwitchChatBot : Form, IUiBridge
    {
        private readonly ChatBotController _chatBotController;

        public TwitchChatBot(ChatBotController chatBotController)
        {
            InitializeComponent();
            _chatBotController = chatBotController;
        }

        public void AppendChat(string message)
        {
            if (textBoxStreamChat.InvokeRequired)
            {
                textBoxStreamChat.Invoke(() => AppendChat(message));
            }
            else
            {
                textBoxStreamChat.AppendText(message + Environment.NewLine);
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
    }
}
