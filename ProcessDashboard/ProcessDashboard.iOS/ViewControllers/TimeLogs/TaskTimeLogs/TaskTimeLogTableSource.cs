using System;
using Foundation;
using UIKit;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using ProcessDashboard.DTO;
using System.Collections.Generic;

namespace ProcessDashboard.iOS
{
	public class TaskTimeLogTableSource : UITableViewSource
	{
		protected List<TimeLogEntry> tableItems;
		protected string cellIdentifier = "projectCell";
		UIViewController owner;

		public TaskTimeLogTableSource(List<TimeLogEntry> items, UIViewController owner)
		{
			tableItems = items;
			this.owner = owner;
		}

		/// <summary>
		/// Called by the TableView to determine how many cells to create for that particular section.
		/// </summary>
		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return tableItems == null ? 0 : tableItems.Count;
		}

		/// <summary>
		/// Called when a row is touched
		/// </summary>
		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			// Display view
			owner.PerformSegue("eachTimeLogSegue", indexPath);

			tableView.DeselectRow(indexPath, true);
		}

		/// <summary>
		/// Called by the TableView to get the actual UITableViewCell to render for the particular row
		/// </summary>
		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			// request a recycled cell to save memory
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
			// if there are no cells to reuse, create a new one
			if (cell == null)
			{ 
				cell = new UITableViewCell(UITableViewCellStyle.Value1, cellIdentifier);
			}
		
			cell.TextLabel.Text = tableItems[indexPath.Row].StartDate.ToLocalTime().ToString("g");
			cell.DetailTextLabel.Text = TimeSpan.FromMinutes(tableItems[indexPath.Row].LoggedTime).ToString(@"hh\:mm"); ;
			cell.TextLabel.Font = UIFont.SystemFontOfSize(12);
			cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(12);
			cell.TextLabel.Lines = 1;
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.TextLabel.TextColor = UIColor.Black;
			cell.TextLabel.LineBreakMode = UILineBreakMode.HeadTruncation;
			return cell;
		}
	}
}
