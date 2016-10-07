using System;
using System.Drawing;
using System.Collections.Generic;
using Foundation;
using UIKit;
using CoreGraphics;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using ProcessDashboard.DTO;
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
	public partial class TimeLogPageViewController : UIViewController
	{
		UIBarButtonItem delete, done;
		NSIndexPath currentPath;
		List<TimeLogEntry> globalTimeLogCache;
		UIActivityIndicatorView activityView;

		public TimeLogPageViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			activityView.Frame = View.Frame;
			activityView.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0.6f);
			activityView.Center = View.Center;
			activityView.HidesWhenStopped = true;
			View.AddSubview(activityView);

			TimelogsTable = new UITableView(new CGRect(0, 100, View.Bounds.Width, View.Bounds.Height - 100), UITableViewStyle.Grouped);

			done = new UIBarButtonItem(UIBarButtonSystemItem.Done, (s, e) =>
			{
				TimelogsTable.SetEditing(false, true);
				NavigationItem.RightBarButtonItem = delete;
			});
			delete = new UIBarButtonItem(UIBarButtonSystemItem.Trash, (s, e) =>
			{
				if (TimelogsTable.Editing)
				{
					TimelogsTable.SetEditing(false, true); // if we've half-swiped a row
				}

				TimelogsTable.SetEditing(true, true);
				NavigationItem.LeftBarButtonItem = null;
				NavigationItem.RightBarButtonItem = done;
			});

			NavigationItem.RightBarButtonItem = delete;

			TimelogsTable.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;

			Add(TimelogsTable);
		}

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue(segue, sender);

			// set the View Controller that’s powering the screen we’re
			// transitioning to
			if (segue.Identifier == "detailSegue")
			{
				var destVc = segue.DestinationViewController as TimelogDetailViewController;

				var source = TimelogsTable.Source as TimelogTableSource;
				var rowPath = TimelogsTable.IndexPathForSelectedRow;
				var item = source.GetItem(rowPath);
				destVc.timeLogEntry = item;
				destVc.isAddingMode = false;
				destVc.owner = this;
			}
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			refreshData();
		}

		public async void refreshData()
		{
			activityView.StartAnimating();
			try
			{
				globalTimeLogCache = await PDashAPI.Controller.GetTimeLogs(0, null, null, null, null);
				TimelogTableSource source = new TimelogTableSource(globalTimeLogCache, this);
				TimelogsTable.Source = source;
				TimelogsTable.ReloadData();
				TimelogsTable.ScrollToRow(NSIndexPath.FromRowSection(0, source.keys.Length - 1), UITableViewScrollPosition.Top, true);
			}
			catch (Exception ex)
			{
				ViewControllerHelper.ShowAlert(this, null, ex.Message + " Please try again later.");
			}
			finally
			{
				activityView.StopAnimating();
			}
		}
	}
}