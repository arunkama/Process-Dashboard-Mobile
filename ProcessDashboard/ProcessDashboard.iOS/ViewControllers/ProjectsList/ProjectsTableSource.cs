using System;
using Foundation;
using UIKit;
using ProcessDashboard.Model;
using ProcessDashboard.DTO;
using System.Collections.Generic;

namespace ProcessDashboard.iOS
{
	public class ProjectsTableSource : UITableViewSource
	{
		List<Project> tableItems;
		public string selectedProjectId;
		public string selectedProjectName;
		protected string cellIdentifier = "projectCell";
		UIViewController owner;

		public ProjectsTableSource(List<Project> items, UIViewController owner)
		{
			tableItems = items;
			this.owner = owner;
		}

		/// <summary>
		/// Called by the TableView to determine how many cells to create for that particular section.
		/// </summary>
		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return tableItems.Count;
		}

		/// <summary>
		/// Called when a row is touched
		/// </summary>
		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			selectedProjectId = tableItems[indexPath.Row].Id;
			selectedProjectName = tableItems[indexPath.Row].Name;
			tableView.DeselectRow(indexPath, true);
			owner.PerformSegue("project2Tasks", owner);

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
				cell = new UITableViewCell(UITableViewCellStyle.Default, cellIdentifier);

			cell.TextLabel.Text = tableItems[indexPath.Row].Name;
			return cell;
		}
	}
}
