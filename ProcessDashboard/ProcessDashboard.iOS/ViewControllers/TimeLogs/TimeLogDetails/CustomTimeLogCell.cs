using System;
using Foundation;
using UIKit;
using CoreGraphics;

namespace ProcessDashboard.iOS
{
	public class CustomTimeLogCell : UITableViewCell
	{

		public UILabel headingLabel, subheadingLabel, thirdheadingLabel;
		//UIImageView imageView;

		public CustomTimeLogCell(NSString cellId) : base(UITableViewCellStyle.Default, cellId)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Gray;

			ContentView.BackgroundColor = UIColor.White;

			//imageView = new UIImageView();

			headingLabel = new UILabel()
			{
				Font = UIFont.SystemFontOfSize(12),
				  //  = UIFont.FromName("AmericanTypewriter", 12f),
				//TextColor = UIColor.FromRGB(127, 51, 0),
				TextAlignment = UITextAlignment.Left,
				BackgroundColor = UIColor.Clear,
				LineBreakMode = UILineBreakMode.HeadTruncation
			};

			subheadingLabel = new UILabel()
			{
				Font = UIFont.SystemFontOfSize(10),
				//TextColor = UIColor.FromRGB(38, 127, 0),
				TextAlignment = UITextAlignment.Center,
				BackgroundColor = UIColor.Clear
			};

			thirdheadingLabel = new UILabel()
			{
				Font = UIFont.SystemFontOfSize(10),
				//TextColor = UIColor.FromRGB(38, 127, 0),
				TextAlignment = UITextAlignment.Left,
				BackgroundColor = UIColor.Clear
			};

			ContentView.Add(headingLabel);
			ContentView.Add(subheadingLabel);
			ContentView.Add(thirdheadingLabel);
		}

		public void UpdateCell(string caption, string subtitle, string thirdtext)
		{
			//imageView.Image = image;
			headingLabel.Text = caption;
			subheadingLabel.Text = subtitle;
			thirdheadingLabel.Text = thirdtext;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			headingLabel.Frame = new CGRect(15, 10, ContentView.Bounds.Width - 180, 25);
			subheadingLabel.Frame = new CGRect(ContentView.Bounds.Width - 140, 14, 80, 20);
			thirdheadingLabel.Frame = new CGRect(ContentView.Bounds.Width - 55, 7, 40, 33);
		}
	}

}

