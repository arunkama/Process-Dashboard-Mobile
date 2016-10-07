using Foundation;
using System;
using UIKit;
using CoreGraphics;
using System.Drawing;
using SharpMobileCode.ModalPicker;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using ProcessDashboard.DTO;


namespace ProcessDashboard.iOS
{
	public partial class TimelogDetailViewController : UIViewController
	{
		// ---------- need to be set prior of segue to this view controller
		public TimeLogEntry timeLogEntry;
		public bool isAddingMode;
		public UIViewController owner;
		// ---------------------------------------------------------------
		UIActivityIndicatorView activityView;
		TimeLoggingController tlc;

		UIBarButtonItem barBtnItem;
		UIBarButtonItem saveButton, cancelButton;
		UIPickerView DeltaPicker, IntPicker;
		UIDatePicker  StartTimePicker;
		string deltaSelectedHour, deltaSelectedMinute;
		string intSelectedHour, intSelectedMinute;
		DateTime startTimeSelectedDate;
		UIToolbar toolbar;

		public TimelogDetailViewController(IntPtr handle) : base(handle)
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

			tlc = TimeLoggingController.GetInstance();

			if (timeLogEntry == null)
			{
				throw new ArgumentException("timeLogEntry is null.");
			}

			if (isAddingMode)
			{
				barBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, async (s, e) =>
				{
					activityView.StartAnimating();
					try
					{
						await PDashAPI.Controller.AddATimeLog(timeLogEntry.Comment, timeLogEntry.StartDate, timeLogEntry.Task.Id, timeLogEntry.LoggedTime, timeLogEntry.InterruptTime, false);
						activityView.StopAnimating();
						NavigationController.PopViewController(true);
					}
					catch (Exception ex)
					{
						activityView.StopAnimating();
						ViewControllerHelper.ShowAlert(this, "Add Time Log", ex.Message + " Please try again later.");
					}
				});
			}
			else
			{
				barBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Trash, (s, e) =>
				{
					UIAlertController actionSheetAlert = UIAlertController.Create(null, "This time log will be deleted", UIAlertControllerStyle.ActionSheet);

					actionSheetAlert.AddAction(UIAlertAction.Create("Delete", UIAlertActionStyle.Destructive, async (action) =>
					{
						if (isActiveTimeLog())
						{
							ViewControllerHelper.ShowAlert(this, "Oops", "You are currently logging time to this time log. Please stop the timmer first.");
						}
						else
						{
							activityView.StartAnimating();
							try
							{
								await PDashAPI.Controller.DeleteTimeLog(timeLogEntry.Id.ToString());
								activityView.StopAnimating();
								NavigationController.PopViewController(true);
							}
							catch (Exception ex)
							{
								activityView.StopAnimating();
								ViewControllerHelper.ShowAlert(this, "Delete Time Log", ex.Message + " Please try again later.");
							}
						}
					}));

					actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine("Cancel button pressed.")));

					UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
					if (presentationPopover != null)
					{
						presentationPopover.SourceView = this.View;
						presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
					}

					// Display the alertg
					this.PresentViewController(actionSheetAlert, true, null);
				});
			}

			NavigationItem.RightBarButtonItem = barBtnItem;

			ProjectNameLabel.Text = timeLogEntry.Task.Project.Name;
			TaskNameLabel.Text = timeLogEntry.Task.FullName;
			StartTimeText.Text = Util.GetInstance().GetLocalTime(timeLogEntry.StartDate).ToString("g");
		
			// set up start time customized UIpicker

			StartTimePicker = new UIDatePicker(new CoreGraphics.CGRect(10f, this.View.Frame.Height - 250, this.View.Frame.Width - 20, 200f));
			StartTimePicker.BackgroundColor = UIColor.FromRGB(220, 220, 220);

			StartTimePicker.UserInteractionEnabled = true;
			StartTimePicker.Mode = UIDatePickerMode.DateAndTime;
			StartTimePicker.MaximumDate = ViewControllerHelper.DateTimeUtcToNSDate(DateTime.UtcNow);

			startTimeSelectedDate = Util.GetInstance().GetServerTime(timeLogEntry.StartDate);

			StartTimePicker.ValueChanged += (Object sender, EventArgs e) =>
			{
				startTimeSelectedDate  = ViewControllerHelper.NSDateToDateTimeUtc((sender as UIDatePicker).Date);
			};

			StartTimePicker.BackgroundColor = UIColor.White;
			StartTimePicker.SetDate(ViewControllerHelper.DateTimeUtcToNSDate(Util.GetInstance().GetServerTime(timeLogEntry.StartDate)), true);

			//Setup the toolbar
			toolbar = new UIToolbar();
			toolbar.BarStyle = UIBarStyle.Default;
			toolbar.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			toolbar.Translucent = true;
			toolbar.SizeToFit();

			// Create a 'done' button for the toolbar and add it to the toolbar

			saveButton = new UIBarButtonItem("Save", UIBarButtonItemStyle.Bordered, null);

			saveButton.Clicked += async (s, e) =>
			{
				this.StartTimeText.Text = Util.GetInstance().GetLocalTime(startTimeSelectedDate).ToString();
				timeLogEntry.StartDate = startTimeSelectedDate;
				this.StartTimeText.ResignFirstResponder();

				if (!isAddingMode)
				{
					try
					{
						await PDashAPI.Controller.UpdateTimeLog(timeLogEntry.Id.ToString(), null, timeLogEntry.StartDate, timeLogEntry.Task.Id, null, null, false);
					}
					catch (Exception ex)
					{
						ViewControllerHelper.ShowAlert(this, "Change Start Time", ex.Message + " Please try again later.");
					}
				}
			};

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };

			cancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.StartTimeText.ResignFirstResponder();
			});

			toolbar.SetItems(new UIBarButtonItem[] { cancelButton, spacer, saveButton }, true);

			this.StartTimeText.InputView = StartTimePicker;
			this.StartTimeText.InputAccessoryView = toolbar;

			DeltaText.Text = TimeSpan.FromMinutes(timeLogEntry.LoggedTime).ToString(@"hh\:mm");

			IntText.Text = TimeSpan.FromMinutes(timeLogEntry.InterruptTime).ToString(@"hh\:mm");

			CommentText.SetTitle(timeLogEntry.Comment ?? "No Comment", UIControlState.Normal);

			CommentText.TouchUpInside += (sender, e) =>
			{
				UIAlertView alert = new UIAlertView();
				alert.Title = "Comment";
				alert.AddButton("Cancel");
				alert.AddButton("Save");
				alert.Message = "Please enter new Comment";
				alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
				UITextField textField = alert.GetTextField(0);
				textField.Placeholder = timeLogEntry.Comment ?? "No Comment";
				alert.Clicked += CommentButtonClicked;
				alert.Show();
			};

			/////Delta Picker 
			DeltaPicker = new UIPickerView(new CoreGraphics.CGRect(10f, this.View.Frame.Height - 250, this.View.Frame.Width - 20, 200f));
			DeltaPicker.BackgroundColor = UIColor.FromRGB(220, 220, 220);

			DeltaPicker.UserInteractionEnabled = true;
			DeltaPicker.ShowSelectionIndicator = true;

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

			StatusPickerViewModel deltaModel = new StatusPickerViewModel(hours, minutes);

			int h = (int)timeLogEntry.LoggedTime / 60;
			int m = (int)timeLogEntry.LoggedTime % 60;
			this.deltaSelectedHour = h.ToString();
			this.deltaSelectedMinute = m.ToString("00");

			deltaModel.NumberSelected += (Object sender, EventArgs e) =>
			{
				this.deltaSelectedHour = deltaModel.selectedHour;
				this.deltaSelectedMinute = deltaModel.selectedMinute;
			};

			DeltaPicker.Model = deltaModel;
			DeltaPicker.BackgroundColor = UIColor.White;
			DeltaPicker.Select(h, 0, true);
			DeltaPicker.Select(m, 1, true);

			//Setup the toolbar
			toolbar = new UIToolbar();
			toolbar.BarStyle = UIBarStyle.Default;
			toolbar.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			toolbar.Translucent = true;
			toolbar.SizeToFit();

			// Create a 'done' button for the toolbar and add it to the toolbar
			saveButton = new UIBarButtonItem("Save", UIBarButtonItemStyle.Bordered, null);

			saveButton.Clicked += async (s, e) =>
			{

				this.DeltaText.Text = this.deltaSelectedHour + ":" + this.deltaSelectedMinute;
				double oldLoggedTime = timeLogEntry.LoggedTime;
				timeLogEntry.LoggedTime = int.Parse(this.deltaSelectedHour) * 60 + int.Parse(this.deltaSelectedMinute);
				this.DeltaText.ResignFirstResponder();

				if (!isAddingMode)
				{
					if (isActiveTimeLog())
					{
						tlc.SetLoggedTime((int)timeLogEntry.LoggedTime);
					}
					else
					{
						try
						{
							await PDashAPI.Controller.UpdateTimeLog(timeLogEntry.Id.ToString(), null, null, timeLogEntry.Task.Id, timeLogEntry.LoggedTime - oldLoggedTime, null, false);
						}
						catch (Exception ex)
						{
							ViewControllerHelper.ShowAlert(this, "Change Logged Time", ex.Message + " Please try again later.");
						}
					}
				}
			};

			cancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.DeltaText.ResignFirstResponder();
			});

			toolbar.SetItems(new UIBarButtonItem[] { cancelButton, spacer, saveButton }, true);

			this.DeltaText.InputView = DeltaPicker;
			this.DeltaText.InputAccessoryView = toolbar;

			////// Int Picker
			IntPicker = new UIPickerView(new CoreGraphics.CGRect(10f, this.View.Frame.Height - 200, this.View.Frame.Width - 20, 200f));
			IntPicker.BackgroundColor = UIColor.FromRGB(220, 220, 220);

			IntPicker.UserInteractionEnabled = true;
			IntPicker.ShowSelectionIndicator = true;
			IntPicker.BackgroundColor = UIColor.White;

			IntPicker.Select(0, 0, true);

			StatusPickerViewModel intModel = new StatusPickerViewModel(hours, minutes);

			intModel.NumberSelected += (Object sender, EventArgs e) =>
			{
				this.intSelectedHour = intModel.selectedHour;
				this.intSelectedMinute = intModel.selectedMinute;
			};

			IntPicker.Model = intModel;

			IntPicker.Select((int)timeLogEntry.InterruptTime / 60, 0, true);
			IntPicker.Select((int)timeLogEntry.InterruptTime % 60, 1, true);

			//Setup the toolbar
			toolbar = new UIToolbar();
			toolbar.BarStyle = UIBarStyle.Default;
			toolbar.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			toolbar.Translucent = true;
			toolbar.SizeToFit();

			saveButton = new UIBarButtonItem("Save", UIBarButtonItemStyle.Bordered, null);

			saveButton.Clicked += async (s, e) =>
			{
				this.IntText.Text = this.intSelectedHour + ":" + this.intSelectedMinute;
				timeLogEntry.InterruptTime = int.Parse(this.intSelectedHour) * 60 + int.Parse(this.intSelectedMinute);
				this.IntText.ResignFirstResponder();

				if (!isAddingMode)
				{
					if (isActiveTimeLog())
					{
						tlc.SetInterruptTime((int)timeLogEntry.InterruptTime);
					}
					else 
					{
						try
						{
							await PDashAPI.Controller.UpdateTimeLog(timeLogEntry.Id.ToString(), null, null, timeLogEntry.Task.Id, null, timeLogEntry.InterruptTime, false);
						}
						catch (Exception ex)
						{
							ViewControllerHelper.ShowAlert(this, "Change Interrupt Time", ex.Message + " Please try again later.");
						}
					}
				}
			};

			cancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.IntText.ResignFirstResponder();
			});

			toolbar.SetItems(new UIBarButtonItem[] { cancelButton, spacer, saveButton }, true);

			this.IntText.InputView = IntPicker;
			this.IntText.InputAccessoryView = toolbar;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			TaskNameLabel.Text = timeLogEntry.Task.FullName;
			StartTimeText.Text = Util.GetInstance().GetLocalTime(timeLogEntry.StartDate).ToString("g");
			DeltaText.Text = TimeSpan.FromMinutes(timeLogEntry.LoggedTime).ToString(@"hh\:mm");
		}

		public async void CommentButtonClicked(object sender, UIButtonEventArgs e)
		{
			UIAlertView parent_alert = (UIAlertView)sender;

			if (e.ButtonIndex == 1)
			{
				CommentText.SetTitle(parent_alert.GetTextField(0).Text, UIControlState.Normal);
				timeLogEntry.Comment = parent_alert.GetTextField(0).Text;

				if (!isAddingMode)
				{
					try
					{
						await PDashAPI.Controller.UpdateTimeLog(timeLogEntry.Id.ToString(), timeLogEntry.Comment, null, timeLogEntry.Task.Id, null, null, false);
					}
					catch (Exception ex)
					{
						ViewControllerHelper.ShowAlert(this, "Change Comment", ex.Message + " Please try again later.");
					}
				}
			}
		}

		private bool isActiveTimeLog()
		{
			return !isAddingMode && tlc.IsTimerRunning() && tlc.GetActiveTimeLogEntryId().Equals(timeLogEntry.Id.ToString());
		}
	}
}