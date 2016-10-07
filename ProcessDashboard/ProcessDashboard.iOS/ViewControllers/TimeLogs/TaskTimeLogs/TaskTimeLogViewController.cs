using Foundation;
using System;
using UIKit;
using CoreGraphics;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using ProcessDashboard.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProcessDashboard.Service.Interface;
using Fusillade;
using ProcessDashboard.APIRoot;
using ProcessDashboard.DBWrapper;
using Exception = System.Exception;
using Task = ProcessDashboard.DTO.Task;

namespace ProcessDashboard.iOS
{
    public partial class TaskTimeLogViewController : UIViewController
    {

		//UILabel ProjectNameLabel, TaskNameLabel, TimelogsLabel;
		List<TimeLogEntry> timeLogCache;
		//UITableView TaskTimeLogTable;

		// This ID is used to fetch the time logs. It is set by the previous view controller
		public string taskId;
		public Task task;

		public TaskTimeLogViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			refreshData();
		}
		private async void refreshData()
		{
			try
			{
				timeLogCache = await PDashAPI.Controller.GetTimeLogs(0, null, null, taskId, null);
				TaskTimeLogTable.Source = new TaskTimeLogTableSource(timeLogCache, this);
				TaskTimeLogTable.ReloadData();
			}
			catch (Exception ex)
			{
				ViewControllerHelper.ShowAlert(this, null, ex.Message + " Please try again later.");
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			ProjectNameLabel.Text = task.Project != null ? task.Project.Name : "";
			TaskNameLabel.Text = task.FullName;
		}

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue(segue, sender);

			if (segue.Identifier == "eachTimeLogSegue")
			{
				var destVc = segue.DestinationViewController as TimelogDetailViewController;
				var rowPath = TaskTimeLogTable.IndexPathForSelectedRow;
				destVc.isAddingMode = false;
				destVc.owner = this;
				destVc.timeLogEntry = timeLogCache[rowPath.Row];
			}
			if (segue.Identifier == "AddTimeLogSegue")
			{
				var destVc = segue.DestinationViewController as TimelogDetailViewController;
				TimeLogEntry newTimeLog = new TimeLogEntry();
				newTimeLog.Task = task;
				newTimeLog.StartDate = DateTime.UtcNow;
				destVc.timeLogEntry = newTimeLog;
				destVc.isAddingMode = true;
				destVc.owner = this;
			}
		}
    }
}