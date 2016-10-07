using System;
using System.Collections.Generic;
using Foundation;
using ProcessDashboard.DTO;
using UIKit;
using CoreGraphics;

namespace ProcessDashboard.iOS
{
	public class TaskDetailTableSource : UITableViewSource
	{
		public TaskDetailTableSource()
		{
		}
		Task TaskItem;
		protected string cellIdentifier = "Cell";

		TaskDetailsViewController owner;
		public UITextField completeDateText, planTimeText;
		DateTime completeTimeSelectedDate;
		UIToolbar toolbar;
		UIBarButtonItem saveButton, cancelButton;
		UIDatePicker CompleteTimePicker;
		UIPickerView PlanTimePicker;
		String saveButtonLabel = "Save";
		string planSelectedHour, planSelectedMinute;

		public TaskDetailTableSource(Task items, TaskDetailsViewController owner)
		{
			TaskItem = items;
			this.owner = owner;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return 3;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			switch (indexPath.Row)
			{
				case 0:
					planTimeText.BecomeFirstResponder();
					tableView.DeselectRow(indexPath, true);
					break;
				case 1:
					owner.PerformSegue("TaskTimeLogsSegue", owner);
					tableView.DeselectRow(indexPath, true);
					break;
				case 2:
					completeDateText.BecomeFirstResponder();
					tableView.DeselectRow(indexPath, true);
					break;
				default: break;	
			}
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Value1, cellIdentifier);

			if (indexPath.Row == 0)
			{
				cell.TextLabel.Text = "Planned Time";
				planTimeText = new UITextField(new CGRect(0, 300, 150, 30));
				cell.AccessoryView = planTimeText;
				planTimeText.TextColor = UIColor.LightGray;
				planTimeText.TextAlignment = UITextAlignment.Right;

				newPlanTimePicker();

				TimeSpan estimated = TimeSpan.FromMinutes(TaskItem.EstimatedTime);
				planTimeText.Text = estimated.ToString("hh\\:mm");
			}
			else if (indexPath.Row == 1)
			{
				cell.TextLabel.Text = "Actual Time";

				TimeSpan actual = TimeSpan.FromMinutes(TaskItem.ActualTime);
				cell.DetailTextLabel.Text = actual.ToString("hh\\:mm");
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			}
			else {
				cell.TextLabel.Text = "Completed Date";
				completeDateText = new UITextField(new CGRect(0, 400, 150, 30));
				cell.AccessoryView = completeDateText;
				completeDateText.TextColor = UIColor.LightGray;
				completeDateText.TextAlignment = UITextAlignment.Right;

				newCompleteDatePicker();

				if (TaskItem.CompletionDate == null)
				{
					completeDateText.Text = "-:-";
				}
				else 
				{
					completeDateText.Text = Util.GetInstance().GetLocalTime(TaskItem.CompletionDate.Value).ToString("MM/dd/yyyy");
				}
			}

			return cell;
		}

		public void newPlanTimePicker()
		{
			PlanTimePicker = new UIPickerView(new CoreGraphics.CGRect(0, 300, 400, 200f));
			PlanTimePicker.BackgroundColor = UIColor.FromRGB(220, 220, 220);

			PlanTimePicker.UserInteractionEnabled = true;
			PlanTimePicker.ShowSelectionIndicator = true;

			string[] hours = new string[24];
			string[] minutes = new string[60];

			for (int i = 0; i < hours.Length; i++)
			{
				hours[i] = i.ToString();
			}
			for (int i = 0; i < minutes.Length; i++)
			{
				minutes[i] = i.ToString("00");
			}

			StatusPickerViewModel planModel = new StatusPickerViewModel(hours, minutes);

			int h = (int)TaskItem.EstimatedTime / 60;
			int m = (int)TaskItem.EstimatedTime % 60;
			this.planSelectedHour = h.ToString();
			this.planSelectedMinute = m.ToString("00");

			planModel.NumberSelected += (Object sender, EventArgs e) =>
			{
				this.planSelectedHour = planModel.selectedHour;
				this.planSelectedMinute = planModel.selectedMinute;
			};

			PlanTimePicker.Model = planModel;
			PlanTimePicker.BackgroundColor = UIColor.White;
			PlanTimePicker.Select(h, 0, true);
			PlanTimePicker.Select(m, 1, true);

			//Setup the toolbar
			toolbar = new UIToolbar();
			toolbar.BarStyle = UIBarStyle.Default;
			toolbar.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			toolbar.Translucent = true;
			toolbar.SizeToFit();

			// Create a 'done' button for the toolbar and add it to the toolbar

			saveButton = new UIBarButtonItem(saveButtonLabel, UIBarButtonItemStyle.Bordered, null);

			saveButton.Clicked += async (s, e) =>
			{
				try
				{
					double planTime = int.Parse(this.planSelectedHour) * 60 + int.Parse(this.planSelectedMinute);
					// TODO:
					// service access Layer should provide nullable markTimeInComplete field!
					// otherwise, this value may be updated unexpectedly
					await PDashAPI.Controller.UpdateATask(TaskItem.Id, planTime, null, TaskItem.CompletionDate == null);
					this.planTimeText.Text = this.planSelectedHour + ":" + this.planSelectedMinute;
				}
				catch (Exception ex)
				{
					ViewControllerHelper.ShowAlert(owner, "Change Planed Time", ex.Message + " Please try again later.");
				}
				finally
				{
					this.planTimeText.ResignFirstResponder(); 
				}
			};

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };

			cancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.planTimeText.ResignFirstResponder();
			});

			toolbar.SetItems(new UIBarButtonItem[] { cancelButton, spacer, saveButton }, true);

			this.planTimeText.InputView = PlanTimePicker;
			this.planTimeText.InputAccessoryView = toolbar;
		}

		public void newCompleteDatePicker()
		{
			CompleteTimePicker = new UIDatePicker(new CoreGraphics.CGRect(0, 300, 400, 200f));
			CompleteTimePicker.BackgroundColor = UIColor.FromRGB(220, 220, 220);

			CompleteTimePicker.UserInteractionEnabled = true;
			CompleteTimePicker.Mode = UIDatePickerMode.DateAndTime;
			CompleteTimePicker.MaximumDate = ViewControllerHelper.DateTimeUtcToNSDate(DateTime.UtcNow);
			//Setup the toolbar
			toolbar = new UIToolbar();
			toolbar.BarStyle = UIBarStyle.Default;
			toolbar.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			toolbar.Translucent = true;
			toolbar.SizeToFit();

			// Create a 'done' button for the toolbar and add it to the toolbar
			saveButton = new UIBarButtonItem(saveButtonLabel, UIBarButtonItemStyle.Bordered, null);

			saveButton.Clicked += async (s, e) =>
			{
				if (saveButton.Title.Equals("Mark Complete"))
				{
					try
					{
						completeTimeSelectedDate = ViewControllerHelper.NSDateToDateTimeUtc(CompleteTimePicker.Date);
						await PDashAPI.Controller.UpdateATask(TaskItem.Id, null, completeTimeSelectedDate, false);
						TaskItem.CompletionDate = completeTimeSelectedDate;
						this.completeDateText.Text = Util.GetInstance().GetLocalTime(completeTimeSelectedDate).ToShortDateString();
						owner.task = TaskItem;
						owner.PauseBtnOnClick(null, null);
						owner.refreshControlButtons();
					}
					catch (Exception ex)
					{
						ViewControllerHelper.ShowAlert(owner, "Mark Complete", ex.Message + " Please try again later.");
					}
				}
				else if (saveButton.Title.Equals("Mark Incomplete"))
				{
					try
					{
						await PDashAPI.Controller.UpdateATask(TaskItem.Id, null, null, true);
						TaskItem.CompletionDate = null;
						this.completeDateText.Text = "-:-";
						owner.task = TaskItem;
						owner.refreshControlButtons();
					}
					catch (Exception ex)
					{
						ViewControllerHelper.ShowAlert(owner, "Mark Incomplete", ex.Message + " Please try again later.");
					}
				}
				else {
					// Change Completion Date
					try
					{
						DateTime newCompleteDate = ViewControllerHelper.NSDateToDateTimeUtc(CompleteTimePicker.Date);
						await PDashAPI.Controller.UpdateATask(TaskItem.Id, null, newCompleteDate, false);
						TaskItem.CompletionDate = newCompleteDate;
						this.completeDateText.Text = Util.GetInstance().GetLocalTime(newCompleteDate).ToString("d");
						owner.task = TaskItem;
						owner.refreshControlButtons();
					}
					catch (Exception ex)
					{
						ViewControllerHelper.ShowAlert(owner, "Change Completion Date", ex.Message + " Please try again later.");
					}
				}

				this.completeDateText.ResignFirstResponder();
				refreshPicker();
			};

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };

			cancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.completeDateText.ResignFirstResponder();
				refreshPicker();
			});

			toolbar.SetItems(new UIBarButtonItem[] { cancelButton, spacer, saveButton }, true);

			refreshPicker();

			CompleteTimePicker.ValueChanged += (Object s, EventArgs e) =>
			{
				if (TaskItem.CompletionDate != null)
				{
					saveButton.Title = "Change Completion Date";
				}

				completeTimeSelectedDate = ViewControllerHelper.NSDateToDateTimeUtc((s as UIDatePicker).Date);
			};

			CompleteTimePicker.BackgroundColor = UIColor.White;

			this.completeDateText.InputView = CompleteTimePicker;
			this.completeDateText.InputAccessoryView = toolbar;
		}

		private void refreshPicker()
		{
			if (TaskItem.CompletionDate == null)
			{
				saveButton.Title = "Mark Complete";
				CompleteTimePicker.SetDate(ViewControllerHelper.DateTimeUtcToNSDate(DateTime.UtcNow), true);
			}
			else
			{
				saveButton.Title = "Mark Incomplete";
				CompleteTimePicker.SetDate(ViewControllerHelper.DateTimeUtcToNSDate(Util.GetInstance().GetServerTime(TaskItem.CompletionDate.Value)), true);
			}
		}
	}
}

