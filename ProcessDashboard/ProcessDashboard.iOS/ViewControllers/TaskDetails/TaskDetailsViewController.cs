using Foundation;
using System;
using UIKit;
using CoreGraphics;
using ProcessDashboard.DTO;
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
using System.Drawing;

namespace ProcessDashboard.iOS
{
	public partial class TaskDetailsViewController : UIViewController
	{
		public Task task;
		TimeLoggingController timeLoggingController;
		DateTime completeTimeSelectedDate;
		UIToolbar toolbar;
		UIBarButtonItem saveButton, cancelButton;
		UIDatePicker CompleteTimePicker;
		String saveButtonLabel = "Save";

		StateChangedEventHandler stateHandler;

		public TaskDetailsViewController(IntPtr handle) : base(handle)
		{
		}

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue(segue, sender);
			if (segue.Identifier == "TaskTimeLogsSegue")
			{
				var controller = segue.DestinationViewController as TaskTimeLogViewController;
				controller.taskId = task.Id;
				controller.task = task;
			}
			if (segue.Identifier == "showListofTasks")
			{
				TasksTableViewController controller = (TasksTableViewController)segue.DestinationViewController;
				controller.projectId = task.Project.Id;
				controller.projectName = task.Project.Name;
			}
		}

		public async void refreshData()
		{
			try
			{
				task = await PDashAPI.Controller.GetTask(task.Id);
				PlanTable.Source = new TaskDetailTableSource(task, this);
				PlanTable.ReloadData();
				refreshControlButtons();
			}
			catch (Exception ex)
			{
				ViewControllerHelper.ShowAlert(this, null, ex.Message + " Please try again later.");
			}
		}

		public override void ViewDidAppear(bool animated)
		{
			PDashAPI.UIHandlerToDispatch += stateHandler;
			refreshData();
			base.ViewDidAppear(animated);
		}

		public override void ViewWillDisappear(bool animated)
		{
			PDashAPI.UIHandlerToDispatch -= stateHandler;
			base.ViewWillDisappear(animated);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			if (task == null)
			{
				return;
			}

			timeLoggingController = TimeLoggingController.GetInstance();
			stateHandler = new StateChangedEventHandler(timeLoggingStateChanged);

			if (NavigationController.NavigationBar.TopItem.Title.Equals("Process Dashboard"))
			{
				NavigationController.NavigationBar.TopItem.Title = "Back";
			}

			TdPlayBtn.TouchUpInside += PlayBtnOnClick;
			TdPauseBtn.TouchUpInside += PauseBtnOnClick;
			refreshControlButtons();

			TdCheckboxBtn.TouchUpInside += delegate
			{
				((TaskDetailTableSource)PlanTable.Source).completeDateText.BecomeFirstResponder();
			};

			tdProjectNameBtn.SetTitle(task.Project != null ? task.Project.Name : "", UIControlState.Normal);
			tdProjectNameBtn.SetTitleColor(UIColor.Black, UIControlState.Normal);
			TdTaskNameLb.Text = task.FullName ?? "";

			TdNotesTf.Layer.BorderColor = UIColor.LightGray.CGColor;
			TdNotesTf.Layer.BorderWidth = 2.0f;
			TdNotesTf.Layer.CornerRadius = 10.0f;
			TdNotesTf.Text = task.Note ?? "";

			PlanTable.Source = new TaskDetailTableSource(task, this);
			View.AddSubview(PlanTable);
		}

		public void refreshControlButtons()
		{
			if (task == null)
			{
				TdPauseBtn.SetImage(UIImage.FromBundle("pause-deactivated"), UIControlState.Normal);
				TdPlayBtn.SetImage(UIImage.FromBundle("play-activated"), UIControlState.Normal);
				TdCheckboxBtn.SetImage(UIImage.FromBundle("checkbox-unchecked"), UIControlState.Normal);
				TdPauseBtn.Enabled = false;
				TdPlayBtn.Enabled = false;
				TdCheckboxBtn.Enabled = false;
			}
			else
			{
				if (task.CompletionDate == null)
				{
					TdCheckboxBtn.SetImage(UIImage.FromBundle("checkbox-unchecked"), UIControlState.Normal);
				}
				else
				{
					TdCheckboxBtn.SetImage(UIImage.FromBundle("checkbox-checked"), UIControlState.Normal);
				}
				TdCheckboxBtn.Enabled = true;

				if (timeLoggingController.IsTimerRunning() && timeLoggingController.GetTimingTaskId() == task.Id)
				{
					TdPauseBtn.SetImage(UIImage.FromBundle("pause-deactivated"), UIControlState.Normal);
					TdPlayBtn.SetImage(UIImage.FromBundle("play-activated"), UIControlState.Normal);
					TdPauseBtn.Enabled = true;
					TdPlayBtn.Enabled = false;
				}
				else
				{
					TdPauseBtn.SetImage(UIImage.FromBundle("pause-activated"), UIControlState.Normal);
					TdPlayBtn.SetImage(UIImage.FromBundle("play-deactivated"), UIControlState.Normal);
					TdPauseBtn.Enabled = false;
					TdPlayBtn.Enabled = true;
				}
			}
		}

		private void timeLoggingStateChanged(object sender, StateChangedEventArgs ea)
		{
			if (ea.NewState.Equals(TimeLoggingControllerStates.TimeLogCanceled) ||
				ea.NewState.Equals(TimeLoggingControllerStates.TimeLogStopped))
			{
				refreshControlButtons();
			}
		}

		public void PauseBtnOnClick(object sender, EventArgs ea)
		{
			timeLoggingController.StopTiming();
			TdPauseBtn.SetImage(UIImage.FromBundle("pause-activated"), UIControlState.Normal);
			TdPauseBtn.Enabled = false;
			TdPlayBtn.SetImage(UIImage.FromBundle("play-deactivated"), UIControlState.Normal);
			TdPlayBtn.Enabled = true;
		}

		public void PlayBtnOnClick(object sender, EventArgs ea)
		{
			if (timeLoggingController.WasNetworkAvailable)
			{
				timeLoggingController.StartTiming(task.Id);
				TdPauseBtn.SetImage(UIImage.FromBundle("pause-deactivated"), UIControlState.Normal);
				TdPauseBtn.Enabled = true;
				TdPlayBtn.SetImage(UIImage.FromBundle("play-activated"), UIControlState.Normal);
				TdPlayBtn.Enabled = false;
			}
			else
			{
				UIAlertController okAlertController = UIAlertController.Create("Oops", "The previous time log is not yet updated to the server. Please try again later.", UIAlertControllerStyle.Alert);
				okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
				PresentViewController(okAlertController, true, null);
				return;
			}
		}
	}
}