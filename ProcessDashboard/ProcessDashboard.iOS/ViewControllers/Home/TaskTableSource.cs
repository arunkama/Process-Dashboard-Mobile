using System;
using System.Collections.Generic;
using Foundation;
using ProcessDashboard.DTO;
using UIKit;

namespace ProcessDashboard.iOS
{
	public class TaskTableSource : UITableViewSource
	{

		List<Task> TableItems;
		List<Task> TasksSorted;
		List<string> ProjectNames;
		List<string> ProjectIDs;
		string CellIdentifier = "RecentTaskCell";
		HomePageViewController tvcontroller;
		public Task selectedTask;

		public TaskTableSource(List<Task> items, HomePageViewController tvcontroller)
		{
			this.tvcontroller = tvcontroller;
			TableItems = items;

			ProjectNames = new List<string>();
			ProjectIDs = new List<string>();
			foreach (Task t in TableItems)
			{
				if (ProjectIDs.IndexOf(t.Project.Id) < 0)
				{
					ProjectIDs.Add(t.Project.Id);
					ProjectNames.Add(t.Project.Name);
				}
			}

			TasksSorted = new List<Task>();
			foreach (string id in ProjectIDs)
			{
				foreach (Task t in TableItems)
				{
					if (t.Project.Id.Equals(id))
					{
						TasksSorted.Add(t);
					}
				}
			}
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			string projectId = ProjectIDs[(int)section];
			int i = 0;
			foreach (Task t in TableItems)
			{
				if (t.Project.Id.Equals(projectId))
				{
					i++;
				}
			}
			return i;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			int idx = indexPath.Row;
			for (int sec = 0; sec < indexPath.Section; sec++)
			{
				idx += (int)RowsInSection(tableView, sec);
			}

			selectedTask = TasksSorted[idx];
			tvcontroller.PerformSegue("home2taskDetailsSegue", indexPath);
			tableView.DeselectRow(indexPath, true);
		}

		public override void AccessoryButtonTapped(UITableView tableView, NSIndexPath indexPath)
		{
			int idx = indexPath.Row;
			for (int sec = 0; sec < indexPath.Section; sec++)
			{
				idx += (int)RowsInSection(tableView, sec);
			}

			selectedTask = TasksSorted[idx];
			tvcontroller.PerformSegue("home2taskDetailsSegue", indexPath);
			tableView.DeselectRow(indexPath, true);
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{

			var cell = tableView.DequeueReusableCell(CellIdentifier);

			int idx = indexPath.Row;
			for (int sec = 0; sec < indexPath.Section; sec++)
			{
				idx += (int) RowsInSection(tableView, sec);
			}

			Task item = TasksSorted[idx];

			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);

			if (item.CompletionDate != null)
			{
				var attriStr = new NSAttributedString(item.FullName, strikethroughStyle: NSUnderlineStyle.Single);
				cell.TextLabel.AttributedText = attriStr;
			}
			else {
				var attriStr = new NSAttributedString(item.FullName, strikethroughStyle: NSUnderlineStyle.None);
				cell.TextLabel.AttributedText = attriStr;
			}
			cell.TextLabel.Font = UIFont.SystemFontOfSize(13);
			cell.TextLabel.Lines = 0;
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.TextLabel.TextColor = UIColor.Black;
			cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;

			return cell;
		}

		public override nint NumberOfSections(UITableView tableView)
		{
			return ProjectIDs.Count;
		}

		public override string TitleForHeader(UITableView tableView, nint section)
		{
			return ProjectNames[(int)section];
		}

		public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section)
		{
			UITableViewHeaderFooterView header = (UITableViewHeaderFooterView)headerView;
			header.TextLabel.Font = UIFont.BoldSystemFontOfSize(13);
		}

		public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		{
			return 18;
		}
	}
}


