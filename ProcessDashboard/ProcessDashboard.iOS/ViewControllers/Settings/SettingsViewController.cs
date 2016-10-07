using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace ProcessDashboard.iOS
{
    public partial class SettingsViewController : UITableViewController
    {
        public SettingsViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			SettingTable = new UITableView(new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height));
			SettingTable.Source = new SettingTableSource(this);

			Add(SettingTable);
		}
    }
}