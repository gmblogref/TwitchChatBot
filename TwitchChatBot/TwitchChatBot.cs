using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Controller;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Models;
using TwitchChatBot.UI;

namespace TwitchChatBot
{
	public partial class TwitchChatBot : Form, IUiBridge
	{
		private readonly ChatBotController _chatBotController;
		private readonly ILogger<TwitchChatBot> _logger;
		private readonly ITestUtilityService _testUtilityService;
		private readonly ITtsService _ttsService;
		private readonly IAlertHistoryService _alertHistoryService;
		private readonly IAlertReplayService _alertReplayService;
		private readonly IWheelService _wheelService;
		private readonly IAppFlags _appFlags;

		private bool _isSpinning = false;
		private bool _hasUnsavedChanges = false;
		private bool _isLoadingWheels = false;
		private bool _isLoadingEntries = false;
		private string? _currentSelectedWheelId;

		public TwitchChatBot(ChatBotController chatBotController,
			ITestUtilityService testUtilityService,
			ITtsService ttsService,
			IAlertHistoryService alertHistoryService,
			IAlertReplayService alertReplayService,
			IAppFlags appFlags,
			IWheelService wheelService,
			ILogger<TwitchChatBot> logger)
		{
			InitializeComponent();
			_chatBotController = chatBotController;
			_testUtilityService = testUtilityService;
			_ttsService = ttsService;
			_alertHistoryService = alertHistoryService;
			_alertReplayService = alertReplayService;
			_appFlags = appFlags;
			_wheelService = wheelService;
			_logger = logger;

			InitializeAlertHistoryUi();
			ConfigureWinnerLabel();
			ConfigureSpinButtonImage();
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

		private void InitializeSpinResultOverlay()
		{
			panelSpinResultOverlay.Visible = false;
			panelSpinResultOverlay.BringToFront();
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

		private async Task LoadWheelsAsync()
		{
			try
			{
				_isLoadingWheels = true;
				var wheels = await _wheelService.GetAllWheelsAsync();

				if (wheels == null || wheels.Count == 0)
				{
					comboBoxWheels.DataSource = null;
					labelWheelWarning.Text = "No wheels available";
					labelWheelWarning.Visible = true;
					dataGridViewEntries.DataSource = null;
					return;
				}

				labelWheelWarning.Visible = false;

				comboBoxWheels.DataSource = wheels;
				comboBoxWheels.DisplayMember = "Name";
				comboBoxWheels.ValueMember = "Id";

				if (!string.IsNullOrWhiteSpace(_currentSelectedWheelId))
				{
					var selected = wheels.FirstOrDefault(x => x.Id == _currentSelectedWheelId);

					if (selected != null)
					{
						comboBoxWheels.SelectedItem = selected;
					}
					else
					{
						comboBoxWheels.SelectedIndex = 0;
					}
				}
				else
				{
					comboBoxWheels.SelectedIndex = 0;
				}
			}
			catch
			{
				labelWheelWarning.Text = "Failed to load wheels";
				labelWheelWarning.Visible = true;
				comboBoxWheels.DataSource = null;
				dataGridViewEntries.DataSource = null;
			}
			finally
			{
				_isLoadingWheels = false;
			}

			if (comboBoxWheels.SelectedItem is Wheel wheel)
			{
				await LoadEntriesAsync(wheel.Id);
			}
		}

		private async Task LoadEntriesAsync(string wheelId)
		{
			try
			{
				_isLoadingEntries = true;

				var wheel = await _wheelService.GetWheelAsync(wheelId);

				if (wheel == null || wheel.Items == null || wheel.Items.Count == 0)
				{
					dataGridViewEntries.DataSource = null;
					return;
				}

				wheelSpinnerControl.Items = wheel.Items;
				wheelSpinnerControl.Invalidate();
				wheelSpinnerControl.RotationAngle = 0f;

				var data = wheel.Items
					.Select(item => new WheelItemRow
					{
						Id = item.Id,
						Label = item.DisplayName,
						Weight = item.Weight,
						Hidden = item.IsHidden
					})
					.ToList();

				dataGridViewEntries.DataSource = data;
				ConfigureEntriesGrid();

				dataGridViewEntries.ReadOnly = false;
				dataGridViewEntries.AllowUserToAddRows = false;
				dataGridViewEntries.AllowUserToDeleteRows = false;
				_hasUnsavedChanges = false;
				buttonSaveEntries.Enabled = false;
			}
			catch
			{
				dataGridViewEntries.DataSource = null;
			}
			finally
			{
				_isLoadingEntries = false;
			}
		}

		private async void comboBoxWheels_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_isLoadingWheels)
			{
				return;
			}

			if (!ConfirmDiscardChanges())
			{
				return;
			}

			if (comboBoxWheels.SelectedItem is not Wheel selectedWheel)
			{
				return;
			}

			labelWheelWarning.Visible = false;
			_currentSelectedWheelId = selectedWheel.Id;
			await LoadEntriesAsync(selectedWheel.Id);
		}

		private async void tabControlChatBot_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControlChatBot.SelectedTab == tabWheel)
			{
				await LoadWheelsAsync();
				ConfigureSpinButtonImage();
			}
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

		private async void buttonSpinResultOk_Click(object sender, EventArgs e)
		{
			if (panelSpinResultOverlay.Tag is WheelItem winner)
			{
				await _wheelService.TriggerWheelAlertAsync(winner);
			}

			panelSpinResultOverlay.Visible = false;
		}

		private void buttonTestFollow_Click(object sender, EventArgs e)
		{
			string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();

			_testUtilityService.TriggerFollowTest(userName);
		}

		private void buttonTestWatchStreak_Click(object sender, EventArgs e)
		{
			string userName = string.IsNullOrWhiteSpace(textUserName.Text) ? "TestUser" : textUserName.Text.Trim();
			int streak = int.TryParse(textAmount.Text?.Trim(), out var parsed) ? parsed : 5;

			_testUtilityService.TriggerWatchStreakUserNoticeTestAsync(userName, streak);
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
				await _chatBotController.StopBotAsync();
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
			if (!ConfirmDiscardChanges())
			{
				e.Cancel = true;
			}

			// Prevent re-entrancy (FormClosing can fire more than once)
			if (e.CloseReason == CloseReason.None)
			{
				return;
			}

			e.Cancel = true;

			try
			{
				await _chatBotController.ShutdownAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error while closing: {ex.Message}");
			}
			finally
			{
				e.Cancel = false;

				// Now actually close
				BeginInvoke(new Action(Close));
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

		private async void buttonTestHypeTrainEnd_Click(object sender, EventArgs e)
		{
			await _testUtilityService.TriggerHypeTrainEndTestAsync();
		}

		private async void buttonSpin_Click(object sender, EventArgs e)
		{
			try
			{
				if (_isSpinning)
				{
					return;
				}

				if (comboBoxWheels.SelectedItem is not Wheel selectedWheel)
				{
					MessageBox.Show("Please select a wheel first.");
					return;
				}

				var visibleItems = selectedWheel.Items
					.Where(x => !x.IsHidden)
					.OrderBy(x => x.Position)
					.ToList();

				if (visibleItems.Count == 0)
				{
					MessageBox.Show("The selected wheel has no valid entries.");
					return;
				}

				_isSpinning = true;
				buttonShuffleEntries.Enabled = false;
				UpdateSpinButtonState();
				labelWheelWarning.Visible = false;

				var result = await _wheelService.SpinAsync(selectedWheel.Id);

				if (result == null)
				{
					MessageBox.Show("Spin failed.");
					return;
				}

				await AnimateWheelToResultAsync(visibleItems, result);

				ShowSpinResult(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during wheel spin.");
			}
			finally
			{
				_isSpinning = false;
				buttonShuffleEntries.Enabled = true;
				UpdateSpinButtonState();
			}
		}

		private void ShowSpinResult(WheelItem winner)
		{
			labelSpinResult.Text = winner.DisplayName;

			panelSpinResultOverlay.Left =
				(wheelSpinnerControl.ClientSize.Width - panelSpinResultOverlay.Width) / 2;

			buttonSpinResultOk.Top =
				panelSpinResultOverlay.Height - buttonSpinResultOk.Height - 20;

			panelSpinResultOverlay.Tag = winner;

			FitSpinResultText();
			
			panelSpinResultOverlay.Visible = true;
			panelSpinResultOverlay.BringToFront();
		}

		private void FitSpinResultText()
		{
			var maxSize = 32f;
			var minSize = 12f;

			for (float size = maxSize; size >= minSize; size -= 1f)
			{
				using var testFont = new Font("Segoe UI", size, FontStyle.Bold);

				var textSize = TextRenderer.MeasureText(
					labelSpinResult.Text,
					testFont,
					new Size(labelSpinResult.Width - 20, labelSpinResult.Height),
					TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter);

				if (textSize.Height <= labelSpinResult.Height &&
					textSize.Width <= labelSpinResult.Width - 20)
				{
					labelSpinResult.Font = new Font("Segoe UI", size, FontStyle.Bold);
					return;
				}
			}

			labelSpinResult.Font = new Font("Segoe UI", minSize, FontStyle.Bold);
		}

		private async void buttonSaveEntries_Click(object sender, EventArgs e)
		{
			await SaveEntriesAsync();
		}

		private async Task SaveEntriesAsync()
		{
			if (comboBoxWheels.SelectedItem is not Wheel wheel)
			{
				return;
			}

			dataGridViewEntries.EndEdit();

			var rows = dataGridViewEntries.DataSource as List<WheelItemRow>;

			if (rows == null)
			{
				return;
			}

			// Get IDs from UI
			var rowIds = rows.Select(x => x.Id).ToHashSet();

			// Find items that were removed in UI
			var itemsToRemove = wheel.Items
				.Where(x => !rowIds.Contains(x.Id))
				.ToList();

			foreach (var item in itemsToRemove)
			{
				await _wheelService.RemoveItemAsync(wheel.Id, item.Id);
			}

			foreach (var row in rows)
			{
				if (string.IsNullOrWhiteSpace(row.Label))
				{
					MessageBox.Show("Name cannot be empty.");
					return;
				}

				if (row.Weight <= 0)
				{
					MessageBox.Show("Weight must be greater than 0.");
					return;
				}
			}

			try
			{
				foreach (var row in rows)
				{
					var existing = wheel.Items.FirstOrDefault(x => x.Id == row.Id);

					if (existing == null)
					{
						var newItem = new WheelItem
						{
							Id = row.Id,
							DisplayName = row.Label.Trim(),
							Weight = row.Weight,
							IsHidden = row.Hidden
						};

						await _wheelService.AddItemAsync(wheel.Id, newItem);
					}
					else
					{
						existing.DisplayName = row.Label.Trim();
						existing.Weight = row.Weight > 0 ? row.Weight : 1;
						existing.IsHidden = row.Hidden;
					}
				}


				foreach (var item in wheel.Items)
				{
					await _wheelService.UpdateItemAsync(wheel.Id, item);
				}

				_hasUnsavedChanges = false;
				buttonSaveEntries.Enabled = false;

				await LoadEntriesAsync(wheel.Id);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error saving entries: {ex.Message}");
			}
		}

		private void dataGridViewEntries_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (_isLoadingEntries)
			{
				return;
			}

			_hasUnsavedChanges = true;
			buttonSaveEntries.Enabled = true;
		}

		private void dataGridViewEntries_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (dataGridViewEntries.IsCurrentCellDirty)
			{
				dataGridViewEntries.CommitEdit(DataGridViewDataErrorContexts.Commit);
			}
		}

		private bool ConfirmDiscardChanges()
		{
			if (!_hasUnsavedChanges)
			{
				return true;
			}

			var result = MessageBox.Show(
				"You have unsaved changes. Leaving will discard them. Continue?",
				"Unsaved Changes",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);

			if (result == DialogResult.No)
			{
				return false;
			}

			_hasUnsavedChanges = false;
			buttonSaveEntries.Enabled = false;

			ReloadCurrentWheel();

			return true;
		}

		private void tabControlChatBot_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (!ConfirmDiscardChanges())
			{
				e.Cancel = true;
			}
		}

		private async void ReloadCurrentWheel()
		{
			if (comboBoxWheels.SelectedItem is Wheel wheel)
			{
				await LoadEntriesAsync(wheel.Id);
			}
		}

		private void ConfigureEntriesGrid()
		{
			// Layout behavior
			dataGridViewEntries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dataGridViewEntries.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

			// Header styling
			dataGridViewEntries.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			// Row behavior (feels much better in use)
			dataGridViewEntries.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			dataGridViewEntries.MultiSelect = false;
			dataGridViewEntries.RowHeadersVisible = false;

			// Column naming
			if (dataGridViewEntries.Columns["Label"] != null)
			{
				dataGridViewEntries.Columns["Label"].HeaderText = "Name";
				dataGridViewEntries.Columns["Label"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			}

			if (dataGridViewEntries.Columns["Weight"] != null)
			{
				dataGridViewEntries.Columns["Weight"].HeaderText = "Weight";
				dataGridViewEntries.Columns["Weight"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			}

			if (dataGridViewEntries.Columns["Hidden"] != null)
			{
				dataGridViewEntries.Columns["Hidden"].HeaderText = "Hidden";
				dataGridViewEntries.Columns["Hidden"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			}

			// Hide non-user fields
			if (dataGridViewEntries.Columns["Position"] != null)
			{
				dataGridViewEntries.Columns["Position"].Visible = false;
			}

			if (dataGridViewEntries.Columns["Id"] != null)
			{
				dataGridViewEntries.Columns["Id"].Visible = false;
			}

			// Optional: improve header height if text wraps
			dataGridViewEntries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		}

		private void buttonAddEntry_Click(object sender, EventArgs e)
		{
			var rows = dataGridViewEntries.DataSource as List<WheelItemRow>;

			if (rows == null)
			{
				rows = new List<WheelItemRow>();
			}

			rows.Add(new WheelItemRow
			{
				Id = Guid.NewGuid().ToString(),
				Label = "New Entry",
				Weight = 1,
				Hidden = false
			});

			dataGridViewEntries.DataSource = null;
			dataGridViewEntries.DataSource = rows;

			ConfigureEntriesGrid();

			_hasUnsavedChanges = true;
			buttonSaveEntries.Enabled = true;
		}

		private void buttonRemoveEntry_Click(object sender, EventArgs e)
		{
			if (dataGridViewEntries.CurrentRow == null)
			{
				return;
			}

			var rows = dataGridViewEntries.DataSource as List<WheelItemRow>;

			if (rows == null)
			{
				return;
			}

			var selected = dataGridViewEntries.CurrentRow.DataBoundItem as WheelItemRow;

			if (selected == null)
			{
				return;
			}

			rows.Remove(selected);

			dataGridViewEntries.DataSource = null;
			dataGridViewEntries.DataSource = rows;

			ConfigureEntriesGrid();

			_hasUnsavedChanges = true;
			buttonSaveEntries.Enabled = true;
		}

		private void UpdateSpinButtonState()
		{
			var rows = dataGridViewEntries.DataSource as List<WheelItemRow>;

			var hasValidItems = rows != null && rows.Any(x => !x.Hidden);

			buttonSpin.Enabled = comboBoxWheels.SelectedItem != null && hasValidItems && !_isSpinning;
		}

		private async Task AnimateWheelToResultAsync(List<WheelItem> visibleItems, WheelItem winner)
		{
			float current = NormalizeAngle(wheelSpinnerControl.RotationAngle);

			float desired = NormalizeAngle(CalculateTargetAngle(visibleItems, winner));

			float delta = desired - current;

			if (delta < 0)
			{
				delta += 360f;
			}

			float end = current + 2160f + delta; // 6 spins + exact landing

			int durationMs = 4500;
			int steps = 120;

			for (int i = 1; i <= steps; i++)
			{
				float progress = (float)i / steps;
				float eased = 1f - (float)Math.Pow(1f - progress, 3);

				float angle = current + ((end - current) * eased);

				wheelSpinnerControl.RotationAngle = NormalizeAngle(angle);

				await Task.Delay(durationMs / steps);
			}

			wheelSpinnerControl.RotationAngle = NormalizeAngle(end);
		}

		private float NormalizeAngle(float angle)
		{
			angle %= 360f;

			if (angle < 0)
			{
				angle += 360f;
			}

			return angle;
		}

		private float CalculateTargetAngle(List<WheelItem> items, WheelItem winner)
		{
			int totalWeight = items.Sum(x => x.Weight > 0 ? x.Weight : 1);

			float startAngle = -90f;

			foreach (var item in items)
			{
				int weight = item.Weight > 0 ? item.Weight : 1;
				float sweep = 360f * weight / totalWeight;

				if (item.Id == winner.Id)
				{
					float padding = Math.Min(6f, sweep * 0.15f);

					float min = startAngle + padding;
					float max = startAngle + sweep - padding;

					float chosen =
						min + (float)(Random.Shared.NextDouble() * (max - min));

					return 270f - chosen;
				}

				startAngle += sweep;
			}

			return 0f;
		}

		private void ConfigureWinnerLabel()
		{
			labelWheelWarning.AutoSize = false;
			labelWheelWarning.Width = wheelSpinnerControl.Width;
			labelWheelWarning.Height = 45;

			labelWheelWarning.Left = wheelSpinnerControl.Left;
			labelWheelWarning.Top = wheelSpinnerControl.Bottom + 10;

			labelWheelWarning.TextAlign = ContentAlignment.MiddleCenter;

			labelWheelWarning.Font = new Font("Arial Black", 20F, FontStyle.Bold);

			labelWheelWarning.ForeColor = Color.Gold;
			labelWheelWarning.BackColor = Color.Black;
		}

		private void ConfigureSpinButtonImage()
		{
			buttonSpin.Region = null;
			buttonSpin.Image = null;
			buttonSpin.BackgroundImage = null;

			buttonSpin.FlatStyle = FlatStyle.Standard;
			buttonSpin.UseVisualStyleBackColor = true;

			buttonSpin.Width = 120;
			buttonSpin.Height = 40;

			buttonSpin.Text = "SPIN";
			buttonSpin.Font = new Font("Arial", 12F, FontStyle.Bold);
			buttonSpin.ForeColor = Color.White;
			buttonSpin.BackColor = Color.Black;
		}

		private async void buttonAddWheel_Click(object sender, EventArgs e)
		{
			try
			{
				using (var form = new Form())
				{
					form.Text = "Create Wheel";
					form.StartPosition = FormStartPosition.CenterParent;
					form.FormBorderStyle = FormBorderStyle.FixedDialog;
					form.MinimizeBox = false;
					form.MaximizeBox = false;
					form.ShowInTaskbar = false;
					form.ClientSize = new Size(340, 140);

					var labelName = new Label
					{
						Left = 15,
						Top = 15,
						Width = 300,
						Text = "Wheel Name:"
					};

					var textName = new TextBox
					{
						Left = 15,
						Top = 40,
						Width = 300
					};

					var buttonSave = new Button
					{
						Text = "Create",
						Left = 160,
						Top = 80,
						Width = 75,
						DialogResult = DialogResult.OK
					};

					var buttonCancel = new Button
					{
						Text = "Cancel",
						Left = 240,
						Top = 80,
						Width = 75,
						DialogResult = DialogResult.Cancel
					};

					form.Controls.Add(labelName);
					form.Controls.Add(textName);
					form.Controls.Add(buttonSave);
					form.Controls.Add(buttonCancel);

					form.AcceptButton = buttonSave;
					form.CancelButton = buttonCancel;

					if (form.ShowDialog(this) != DialogResult.OK)
					{
						return;
					}

					var wheelName = textName.Text.Trim();

					if (string.IsNullOrWhiteSpace(wheelName))
					{
						MessageBox.Show(
							"Wheel name is required.",
							"Validation",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);

						return;
					}

					var newWheel = new Wheel
					{
						Name = wheelName,
						Items = new List<WheelItem>
						{
							new WheelItem
							{
								Id = Guid.NewGuid().ToString(),
								DisplayName = "New Item",
								Weight = 1,
								IsHidden = false,
								Position = 1
							}
						}
					};

					var added = await _wheelService.AddWheelAsync(newWheel);

					if (!added)
					{
						MessageBox.Show(
							"Unable to create wheel. Name may already exist.",
							"Create Wheel",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);

						return;
					}

					await LoadWheelsAsync();

					for (var i = 0; i < comboBoxWheels.Items.Count; i++)
					{
						if (comboBoxWheels.Items[i] is Wheel wheel &&
							wheel.Id == newWheel.Id)
						{
							comboBoxWheels.SelectedIndex = i;
							break;
						}
					}

					await LoadEntriesAsync(newWheel.Id);
					comboBoxWheels.Refresh();
					dataGridViewEntries.Refresh();

					if (dataGridViewEntries.Rows.Count > 0 &&
						dataGridViewEntries.Columns.Contains("DisplayName"))
					{
						dataGridViewEntries.CurrentCell = dataGridViewEntries.Rows[0].Cells["DisplayName"];
						dataGridViewEntries.BeginEdit(true);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Failed to create wheel.{Environment.NewLine}{ex.Message}",
					"Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		private async void buttonShuffleEntries_Click(object sender, EventArgs e)
		{
			try
			{
				if (_isSpinning)
				{
					return;
				}

				if (comboBoxWheels.SelectedItem is not Wheel selectedWheel)
				{
					return;
				}

				if (_hasUnsavedChanges)
				{
					var result = MessageBox.Show(
						"You have unsaved changes. Save before shuffling?",
						"Unsaved Changes",
						MessageBoxButtons.YesNoCancel,
						MessageBoxIcon.Question);

					if (result == DialogResult.Cancel)
					{
						return;
					}

					if (result == DialogResult.Yes)
					{
						await SaveEntriesAsync();
					}
				}

				await _wheelService.ShuffleAsync(selectedWheel.Id);

				_hasUnsavedChanges = false;

				await ReloadCurrentWheelAsync();

				wheelSpinnerControl.Invalidate();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to shuffle entries.");
			}
		}

		private async Task ReloadCurrentWheelAsync()
		{
			if (comboBoxWheels.SelectedItem is not Wheel currentWheel)
			{
				return;
			}

			var wheelId = currentWheel.Id;

			await LoadWheelsAsync();

			for (int i = 0; i < comboBoxWheels.Items.Count; i++)
			{
				if (comboBoxWheels.Items[i] is Wheel wheel &&
					wheel.Id == wheelId)
				{
					comboBoxWheels.SelectedIndex = i;
					return;
				}
			}
		}

		private async void buttonDeleteWheel_Click(object sender, EventArgs e)
		{
			try
			{
				if (_isSpinning)
				{
					MessageBox.Show("Cannot delete a wheel while spinning.");
					return;
				}

				if (comboBoxWheels.SelectedItem is not Wheel selectedWheel)
				{
					MessageBox.Show("Please select a wheel to delete.");
					return;
				}

				if (!ConfirmDiscardChanges())
				{
					return;
				}

				var result = MessageBox.Show(
					$"Delete wheel '{selectedWheel.Name}'?",
					"Confirm Delete",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning);

				if (result != DialogResult.Yes)
				{
					return;
				}

				await _wheelService.DeleteWheelAsync(selectedWheel.Id);

				await LoadWheelsAsync();

				var wheels = comboBoxWheels.DataSource as List<Wheel>;

				if (wheels != null && wheels.Count > 0)
				{
					comboBoxWheels.SelectedIndex = 0;
				}
				else
				{
					comboBoxWheels.DataSource = null;
					dataGridViewEntries.DataSource = null;

					labelWheelWarning.Text = "No wheels available";
					labelWheelWarning.Visible = true;
				}

				_hasUnsavedChanges = false;
				buttonSaveEntries.Enabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error deleting wheel: {ex.Message}");
			}
		}
	}
}