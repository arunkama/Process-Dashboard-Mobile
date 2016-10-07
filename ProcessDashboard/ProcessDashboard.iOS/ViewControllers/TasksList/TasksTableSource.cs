using System;
using System.Collections.Generic;
using Foundation;
using ProcessDashboard.DTO;
using UIKit;

namespace ProcessDashboard.iOS
{
	public class TasksTableSource : UITableViewSource
	{
		public List<Task> tableItems;
		protected string cellIdentifier = "taskCell";
		public Task selectedTask;
		UIViewController owner;

		public TasksTableSource(List<Task> items, UIViewController owner)
		{
			tableItems = items;
			this.owner = owner;

			if (tableItems == null || tableItems.Count == 0)
			{
				UIAlertController okAlertController = UIAlertController.Create("No Tasks Found", "", UIAlertControllerStyle.Alert);
				okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
				owner.PresentViewController(okAlertController, true, null);
			}
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return tableItems == null ? 0 : tableItems.Count;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			selectedTask = tableItems[indexPath.Row];
			tableView.DeselectRow(indexPath, true);
			owner.PerformSegue("task2TaskDetail", owner);
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);
			}

			if (tableItems[indexPath.Row].CompletionDate != null)
			{
				var attriStr = new NSAttributedString(tableItems[indexPath.Row].FullName, strikethroughStyle: NSUnderlineStyle.Single);
				cell.TextLabel.AttributedText = attriStr;
			}
			else {
				var attriStr = new NSAttributedString(tableItems[indexPath.Row].FullName, strikethroughStyle: NSUnderlineStyle.None);
				cell.TextLabel.AttributedText = attriStr;
			}

			cell.TextLabel.Font = UIFont.SystemFontOfSize(13);
			cell.TextLabel.Lines = 0;
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.TextLabel.TextColor = UIColor.Black;
			cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;

			return cell;
		}
	}
}

