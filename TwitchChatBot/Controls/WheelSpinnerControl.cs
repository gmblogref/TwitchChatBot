using System.Drawing.Drawing2D;
using TwitchChatBot.Models;

namespace TwitchChatBot.Controls
{
	public class WheelSpinnerControl : Control
	{
		private List<WheelItem> _items = new List<WheelItem>();
		private float _rotationAngle = 0f;

		public List<WheelItem> Items
		{
			get { return _items; }
			set
			{
				_items = value ?? new List<WheelItem>();
				Invalidate();
			}
		}

		public float RotationAngle
		{
			get { return _rotationAngle; }
			set
			{
				_rotationAngle = value;
				Invalidate();
			}
		}

		public WheelSpinnerControl()
		{
			DoubleBuffered = true;
			ResizeRedraw = true;
			BackColor = Color.White;
		}

		public Point WheelCenter
		{
			get
			{
				return new Point(Width / 2, Height / 2);
			}
		}

		public Rectangle WheelBounds
		{
			get
			{
				return GetWheelBounds();
			}
		}

		public Point SuggestedSpinButtonLocation(Size buttonSize)
		{
			var center = WheelCenter;

			return new Point(
				center.X - (buttonSize.Width / 2),
				center.Y - (buttonSize.Height / 2));
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

			if (_items == null || !_items.Any(x => !x.IsHidden))
			{
				DrawEmpty(e.Graphics);
				return;
			}

			var visibleItems = _items
				.Where(x => !x.IsHidden)
				.ToList();

			int totalWeight = visibleItems.Sum(x => x.Weight > 0 ? x.Weight : 1);

			Rectangle wheelRect = GetWheelBounds();

			e.Graphics.TranslateTransform(Width / 2f, Height / 2f);
			e.Graphics.RotateTransform(_rotationAngle);
			e.Graphics.TranslateTransform(-Width / 2f, -Height / 2f);

			float startAngle = -90f;

			for (int i = 0; i < visibleItems.Count; i++)
			{
				var item = visibleItems[i];

				int weight = item.Weight > 0 ? item.Weight : 1;
				float sweepAngle = 360f * weight / totalWeight;

				using Brush brush = new SolidBrush(GetSegmentColor(i));
				e.Graphics.FillPie(brush, wheelRect, startAngle, sweepAngle);
				e.Graphics.DrawPie(Pens.Black, wheelRect, startAngle, sweepAngle);

				DrawLabel(e.Graphics, wheelRect, item.DisplayName, startAngle, sweepAngle);

				startAngle += sweepAngle;
			}

			e.Graphics.ResetTransform();

			DrawPointer(e.Graphics);
		}

		private Rectangle GetWheelBounds()
		{
			int size = Math.Min(Width - 20, Height - 20);
			return new Rectangle(
				(Width - size) / 2,
				(Height - size) / 2,
				size,
				size);
		}

		private void DrawEmpty(Graphics g)
		{
			using var sf = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};

			g.DrawString(
				"No entries available",
				Font,
				Brushes.Gray,
				ClientRectangle,
				sf);
		}

		private void DrawPointer(Graphics g)
		{
			Point[] points =
			{
				new Point(Width / 2 - 12, 8),
				new Point(Width / 2 + 12, 8),
				new Point(Width / 2, 28)
			};

			g.FillPolygon(Brushes.Red, points);
			g.DrawPolygon(Pens.Black, points);
		}

		private void DrawLabel(
			Graphics g,
			Rectangle rect,
			string text,
			float startAngle,
			float sweepAngle)
		{
			float midAngle = startAngle + (sweepAngle / 2f);
			double radians = Math.PI * midAngle / 180.0;

			float radius = rect.Width / 3.2f;

			float centerX = rect.Left + rect.Width / 2f;
			float centerY = rect.Top + rect.Height / 2f;

			float x = centerX + (float)(Math.Cos(radians) * radius);
			float y = centerY + (float)(Math.Sin(radians) * radius);

			var state = g.Save();

			g.TranslateTransform(x, y);
			g.RotateTransform(midAngle + 90f);

			var drawText = WrapWheelText(text, sweepAngle);
			float fontSize = GetBestFontSize(drawText, sweepAngle);

			using (var font = new Font(Font.FontFamily, fontSize, FontStyle.Bold))
			{
				SizeF size = g.MeasureString(drawText, font);

				g.DrawString(
					drawText,
					font,
					Brushes.Black,
					-size.Width / 2f,
					-size.Height / 2f);
			}

			g.Restore(state);
		}

		private float GetBestFontSize(string text, float sweepAngle)
		{
			float size = 16f;

			if (sweepAngle < 90f)
			{
				size = 14f;
			}

			if (sweepAngle < 70f)
			{
				size = 12f;
			}

			if (sweepAngle < 55f)
			{
				size = 10f;
			}

			if (sweepAngle < 40f)
			{
				size = 8f;
			}

			if (sweepAngle < 30f)
			{
				size = 7f;
			}

			if (sweepAngle < 22f)
			{
				size = 6f;
			}

			var lines = text.Split('\n');
			var longest = lines.Max(x => x.Length);
			var lineCount = lines.Length;

			if (longest > 8)
			{
				size -= 1f;
			}

			if (longest > 10)
			{
				size -= 1f;
			}

			if (lineCount >= 3)
			{
				size -= 1f;
			}

			if (lineCount >= 4)
			{
				size -= 1f;
			}

			if (size < 5f)
			{
				size = 5f;
			}

			return size;
		}

		private string WrapWheelText(string text, float sweepAngle)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}

			int maxChars = 12;

			if (sweepAngle < 70f)
			{
				maxChars = 10;
			}

			if (sweepAngle < 45f)
			{
				maxChars = 8;
			}

			if (sweepAngle < 30f)
			{
				maxChars = 6;
			}

			var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

			if (words.Length == 1)
			{
				return text;
			}

			var lines = new List<string>();
			var current = string.Empty;

			foreach (var word in words)
			{
				var test = string.IsNullOrWhiteSpace(current)
					? word
					: current + " " + word;

				if (test.Length > maxChars)
				{
					lines.Add(current);
					current = word;
				}
				else
				{
					current = test;
				}
			}

			if (!string.IsNullOrWhiteSpace(current))
			{
				lines.Add(current);
			}

			return string.Join("\n", lines);
		}

		private Color GetSegmentColor(int index)
		{
			Color[] colors =
			{
				Color.LightSkyBlue,
				Color.LightGreen,
				Color.Khaki,
				Color.LightPink,
				Color.Plum,
				Color.Orange,
				Color.Salmon,
				Color.Turquoise
			};

			return colors[index % colors.Length];
		}
	}
}