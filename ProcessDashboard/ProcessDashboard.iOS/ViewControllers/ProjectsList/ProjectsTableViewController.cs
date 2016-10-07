using System;
using UIKit;
using ProcessDashboard.Model;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessDashboard.Service.Interface;
using Fusillade;
using ProcessDashboard.APIRoot;
using ProcessDashboard.DBWrapper;
using ProcessDashboard.DTO;
using Task = System.Threading.Tasks.Task;


namespace ProcessDashboard.iOS
{
    public partial class ProjectsTableViewController : UITableViewController
    {
		List<Project> projectsCache;

        public ProjectsTableViewController (IntPtr handle) : base (handle)
        {
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			this.RefreshControl = new UIRefreshControl();
			this.RefreshControl.ValueChanged += (sender, e) => { refreshData();};
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			refreshData();
		}

		public override void PrepareForSegue(UIKit.UIStoryboardSegue segue, Foundation.NSObject sender)
		{
			base.PrepareForSegue(segue, sender);
			if (segue.Identifier.Equals("project2Tasks"))
			{
				TasksTableViewController controller = (TasksTableViewController)segue.DestinationViewController;
				controller.projectId = ((ProjectsTableSource)projectsTableView.Source).selectedProjectId;
				controller.projectName = ((ProjectsTableSource)projectsTableView.Source).selectedProjectName;
			}
		}

		public async void refreshData()
		{
			if (!this.RefreshControl.Refreshing)
			{
				this.RefreshControl.BeginRefreshing();
			}

			try
			{
				List<Project> projectsList = await PDashAPI.Controller.GetProjects();
				projectsCache = projectsList;
				projectsTableView.Source = new ProjectsTableSource(projectsCache, this);
				projectsTableView.ReloadData();

				String refreshTime = DateTime.Now.ToString("g");
				String subTitle = "Last refresh: " + refreshTime;
				this.RefreshControl.AttributedTitle = new Foundation.NSAttributedString(subTitle);
			}
			catch (Exception ex)
			{
				ViewControllerHelper.ShowAlert(this, null, ex.Message + " Please try again later.");
			}
			finally
			{
				if (this.RefreshControl.Refreshing)
				{
					this.RefreshControl.EndRefreshing();
				}
			}
		}
    }
}