using Foundation;
using System;
using UIKit;
using ProcessDashboard.Model;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProcessDashboard.Service.Interface;
using Fusillade;
using ProcessDashboard.APIRoot;
using ProcessDashboard.DBWrapper;
using ProcessDashboard.DTO;
using System.Drawing;
using CoreGraphics;

namespace ProcessDashboard.iOS
{
	public partial class TasksTableViewController : UIViewController
	{
		public string projectId;
		public string projectName;
		List<Task> tasksCache;
		UILabel StaticLabel;
		// TODO: Fixed the Refreshcontrol, refreshing the list of tasks.
		//UIRefreshControl RefreshControl;

		public TasksTableViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			StaticLabel = new UILabel(new CGRect(0, 60, View.Bounds.Width, 42))
			{
				Text = " ",
				Font = UIFont.SystemFontOfSize(12),
				TextColor = UIColor.Black,
				TextAlignment = UITextAlignment.Center,
				BackgroundColor = UIColor.FromRGB(225, 225, 225),
				Lines = 0,
				LineBreakMode = UILineBreakMode.WordWrap,
			};
			tasksTableView.Frame = new CGRect(0, 42, View.Bounds.Width, View.Bounds.Height - 42);
			StaticLabel.AutoresizingMask = UIViewAutoresizing.All;
			View.AddSubview(StaticLabel);
			// TODO: Fixed the Refreshcontrol, refreshing the list of tasks.
			//RefreshControl = new UIRefreshControl();
			//tasksTableView.Add(RefreshControl);
			//RefreshControl.ValueChanged += (sender, e) => { refreshData(); };
			refreshData();

		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			//refreshData();
		}

		public override void PrepareForSegue(UIKit.UIStoryboardSegue segue, Foundation.NSObject sender)
		{
			base.PrepareForSegue(segue, sender);
			if (segue.Identifier.Equals("task2TaskDetail"))
			{
				TaskDetailsViewController controller = (TaskDetailsViewController)segue.DestinationViewController;
				controller.task = ((TasksTableSource)tasksTableView.Source).selectedTask;
			}
		}

		public async void refreshData()
		{
			// TODO: Fixed the Refreshcontrol, refreshing the list of tasks.
			//if (!RefreshControl.Refreshing)
			//{
			//	RefreshControl.BeginRefreshing();
			//}

			await getDataOfTask();

			TasksTableSource source = new TasksTableSource(tasksCache, this);
			tasksTableView.Source = source;
			NavigationItem.Title = "Tasks";
			StaticLabel.Text = projectName;

			int pos = 0;
			for (int i = 0; i < tasksCache.Count; i++)
			{
				if (tasksCache[i].CompletionDate == null)
				{
					if (i <= 4)
					{
						pos = i;
					}
					else {
						pos = i - 4;
					}
					break;
				}

			}
			String refreshTime = DateTime.Now.ToString("g");
			String subTitle = "Last refresh: " + refreshTime;
			// TODO: Fixed the Refreshcontrol, refreshing the list of tasks.
			//RefreshControl.AttributedTitle = new Foundation.NSAttributedString(subTitle);

			tasksTableView.ReloadData();
			// The scroll bar should be scrolled so that the first incomplete task is the first task in the screen.
			if (tasksCache.Count != 0)
			{
				tasksTableView.ScrollToRow(NSIndexPath.FromRowSection(pos, 0), UITableViewScrollPosition.Top, true);
			}
			// TODO: Fixed the Refreshcontrol, refreshing the list of tasks.
			//if (this.RefreshControl.Refreshing)
			//{
			//	this.RefreshControl.EndRefreshing();
			//}
		}


		public async System.Threading.Tasks.Task<int> getDataOfTask()
		{
			List<Task> tasksList = await PDashAPI.Controller.GetTasks(projectId);
			tasksCache = tasksList;

			try
			{
				foreach (var task in tasksList)  //.Select(x => x.estimatedTime)
				{
					System.Diagnostics.Debug.WriteLine(task.FullName);

				}

			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("We are in an error state :" + e);
			}

			return 0;
		}
	}
}