using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using System.IO;
using System.Linq;
using CoreGraphics;
using System.Drawing;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using ProcessDashboard.DTO;


namespace ProcessDashboard.iOS
{
	public class TimelogTableSource : UITableViewSource
	{
		protected string cellIdentifier = "TableCell";

		public Dictionary<string, List<TimeLogEntry>> indexedTableItems;
		public string[] keys;
		public TimeLogPageViewController owner;

		public TimelogTableSource(List<TimeLogEntry> items, TimeLogPageViewController owner)
		{
			this.owner = owner;

			indexedTableItems = new Dictionary<string, List<TimeLogEntry>>();
			foreach (var t in items)
			{
				if (indexedTableItems.ContainsKey(t.StartDate.ToShortDateString()))
				{
					indexedTableItems[t.StartDate.ToShortDateString()].Add(t);
				}
				else {
					indexedTableItems.Add(t.StartDate.ToShortDateString(), new List<TimeLogEntry>() { t });
				}
			}
			keys = indexedTableItems.Keys.ToArray();
		}


		/// <summary>
		/// Called by the TableView to determine how many sections(groups) there are.
		/// </summary>
		public override nint NumberOfSections(UITableView tableView)
		{
			return keys.Length;
		}

		/// <summary>
		/// Called by the TableView to determine how many cells to create for that particular section.
		/// </summary>
		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return indexedTableItems[keys[section]].Count;
		}


		/// <summary>
		/// The string to show in the section header
		/// </summary>
		public override string TitleForHeader(UITableView tableView, nint section)
		{
			return keys[section];
		}

		//public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		//{
		//	return 30.0f;
		//}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{

			// Display view
			owner.PerformSegue("detailSegue", indexPath);

			tableView.DeselectRow(indexPath, true);
		}

		/// <summary>
		/// Called by the TableView to get the actual UITableViewCell to render for the particular section and row
		/// </summary>
		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{


			CustomTimeLogCell cell = tableView.DequeueReusableCell(cellIdentifier) as CustomTimeLogCell;
			TimeLogEntry item = indexedTableItems[keys[indexPath.Section]][indexPath.Row];

			// if there are no cells to reuse, create a new one
			if (cell == null)
			{
				cell = new CustomTimeLogCell ((NSString)cellIdentifier);
			}

			//cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

			cell.UpdateCell(item.Task.FullName.ToString()
			                , item.StartDate.ToLocalTime().ToShortTimeString()
			                , TimeSpan.FromMinutes(item.LoggedTime).ToString(@"hh\:mm"));

			return cell;
		}

		public TimeLogEntry GetItem(NSIndexPath indexPath)
		{
			return indexedTableItems[keys[indexPath.Section]][indexPath.Row];
		}

		#region -= editing methods =-

		private bool isActiveTimeLog(TimeLogEntry log)
		{
			TimeLoggingController tlc = TimeLoggingController.GetInstance();
			return tlc.IsTimerRunning() && tlc.GetActiveTimeLogEntryId().Equals(log.Id.ToString());
		}

		public override async void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			switch (editingStyle)
			{
				case UITableViewCellEditingStyle.Delete:
					TimeLogEntry item = indexedTableItems[keys[indexPath.Section]][indexPath.Row];

					if (isActiveTimeLog(item))
					{
						ViewControllerHelper.ShowAlert(owner, "Oops", "You are currently logging time to this time log. Please stop the timmer first.");
					}
					else
					{
						try
						{
							await PDashAPI.Controller.DeleteTimeLog(item.Id.ToString());
						}
						catch (Exception ex)
						{
							ViewControllerHelper.ShowAlert(owner, "Delete Time Log", ex.Message + " Please try again later.");
						}
					}

					indexedTableItems[keys[indexPath.Section]].RemoveAt(indexPath.Row);
					tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
					// save the time log entry deletion to server

					break;

				case UITableViewCellEditingStyle.None:
					Console.WriteLine("CommitEditingStyle:None called");
					break;
			}
		}

		/// <summary>
		/// Called by the table view to determine whether or not the row is editable
		/// </summary>
		public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
		{
			return true; // return false if you wish to disable editing for a specific indexPath or for all rows
		}

		/// <summary>
		/// Called by the table view to determine whether or not the row is moveable
		/// </summary>
		public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
		{
			return false;
		}

		/// <summary>
		/// Custom text for delete button
		/// </summary>
		public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
		{
			//return "Trash (" + indexedTableItems[keys[indexPath.Section]][indexPath.Row].SubHeading + ")";
			return "Delete";
		}

		/// <summary>
		/// Called by the table view to determine whether the editing control should be an insert
		/// or a delete.
		/// </summary>
		public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
		{
			
			return UITableViewCellEditingStyle.Delete;
		}

		/// <summary>
		/// called by the table view when a row is moved.
		/// </summary>
		public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
		{
			//---- get a reference to the item
			var item = indexedTableItems[keys[sourceIndexPath.Section]][sourceIndexPath.Row];
			var deleteAt = sourceIndexPath.Row;
			var insertAt = destinationIndexPath.Row;

			//---- if we're inserting it before, the one to delete will have a higher index 
			if (destinationIndexPath.Row < sourceIndexPath.Row)
			{
				//---- add one to where we delete, because we're increasing the index by inserting
				deleteAt += 1;
			}
			else {
				//---- add one to where we insert, because we haven't deleted the original yet
				insertAt += 1;
			}

			//---- copy the item to the new location
			indexedTableItems[keys[destinationIndexPath.Section]].Insert(insertAt, item);

			//---- remove from the old
			indexedTableItems[keys[sourceIndexPath.Section]].RemoveAt(deleteAt);
		}


		#endregion


	}
}

