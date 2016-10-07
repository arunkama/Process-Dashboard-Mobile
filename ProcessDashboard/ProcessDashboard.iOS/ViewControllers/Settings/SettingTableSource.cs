using System;
using System.Collections.Generic;
using Foundation;
using ProcessDashboard.DTO;
using UIKit;
using CoreGraphics;

namespace ProcessDashboard.iOS
{
	public class SettingTableSource : UITableViewSource
	{
		public SettingTableSource()
		{
		}
		protected string cellIdentifier = "Cell";
		UIViewController owner;
		UITextField InterruptTimeText, ForgottenTimerText;
		UIPickerView InterruptTimePicker, ForgottenTimePicker;
		UIToolbar toolbar;
		UIBarButtonItem saveButton, cancelButton;
		String saveButtonLabel = "Save";
		string intSelectedHour, intSelectedMinute;
		string fgtSelectedHour, fgtSelectedMinute;

		public SettingTableSource(UIViewController owner)
		{
			this.owner = owner;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return 4;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			// request a recycled cell to save memory
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Value1, cellIdentifier);
			cell.TextLabel.Font = UIFont.SystemFontOfSize(13);

			if (indexPath.Row == 0)
			{
				cell.TextLabel.Text = "Logged in as: \"" + AccountStorage.UserId + "\"";

				UIButton logoutBtn = new UIButton(UIButtonType.RoundedRect);
				logoutBtn.Frame = new CGRect(0, 0, 65, 30);
				logoutBtn.SetTitle("Log Out", UIControlState.Normal);
				logoutBtn.TouchUpInside += (sender, e) => { 
					UIAlertController actionSheetAlert = UIAlertController.Create(null, "You are going to log out", UIAlertControllerStyle.ActionSheet);

					actionSheetAlert.AddAction(UIAlertAction.Create("Log Out", UIAlertActionStyle.Destructive, (action) =>
					{
						AccountStorage.ClearPassword();
						AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
						del.BindLoginViewController();
					}));

					actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

					UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
					if (presentationPopover != null)
					{
						presentationPopover.SourceView = this.owner.View;
						presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
					}
					this.owner.PresentViewController(actionSheetAlert, true, null);
				};
				cell.AccessoryView = logoutBtn;
			}
			else if (indexPath.Row == 1)
			{
				cell.TextLabel.Text = "Connect only over WiFi";
				UISwitch wifiSwitch = new UISwitch(new CGRect(0, 0, 80, 20));
				wifiSwitch.On = SettingsData.WiFiOnly;
				wifiSwitch.ValueChanged += (sender, e) => {
					SettingsData.WiFiOnly = wifiSwitch.On;
				};
				cell.AccessoryView = wifiSwitch;
			}
			else if (indexPath.Row == 2)
			{
				cell.TextLabel.Text = "Max continuous interrupt time";
				InterruptTimeText = new UITextField(new CGRect(0, 400, 150, 30));
				cell.AccessoryView = InterruptTimeText;
				InterruptTimeText.TextColor = UIColor.LightGray;
				InterruptTimeText.TextAlignment = UITextAlignment.Right;
				newInterruptTimePicker();
			}
			else if (indexPath.Row == 3){
				cell.TextLabel.Text = "Forgotten timer threshold";
				ForgottenTimerText = new UITextField(new CGRect(0, 500, 150, 30));
				cell.AccessoryView = ForgottenTimerText;
				ForgottenTimerText.TextColor = UIColor.LightGray;
				ForgottenTimerText.TextAlignment = UITextAlignment.Right;
				newForgottenTimePicker();
			}
			return cell;
		}

		public void newInterruptTimePicker()
		{
			InterruptTimePicker = new UIPickerView(new CoreGraphics.CGRect(0, 300, 400, 200f));
			InterruptTimePicker.BackgroundColor = UIColor.FromRGB(220, 220, 220);

			InterruptTimePicker.UserInteractionEnabled = true;
			InterruptTimePicker.ShowSelectionIndicator = true;

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

			planModel.NumberSelected += (Object sender, EventArgs e) =>
			{
				this.intSelectedHour = planModel.selectedHour;
				this.intSelectedMinute = planModel.selectedMinute;

				if (this.intSelectedHour.Equals("0") && this.intSelectedMinute.Equals("00"))
				{
					this.intSelectedMinute = "01";
					InterruptTimePicker.Select(1, 1, true);
				}
			};

			InterruptTimePicker.Model = planModel;
			InterruptTimePicker.BackgroundColor = UIColor.White;

			int maxContIntTimeHour = SettingsData.MaxContIntTimeMin / 60;
			int maxContIntTimeMin = SettingsData.MaxContIntTimeMin % 60;

			this.intSelectedHour = maxContIntTimeHour.ToString();
			this.intSelectedMinute = maxContIntTimeMin.ToString("00");
			this.InterruptTimeText.Text = this.intSelectedHour + ":" + this.intSelectedMinute;

			InterruptTimePicker.Select(maxContIntTimeHour, 0, true);
			InterruptTimePicker.Select(maxContIntTimeMin, 1, true);

			//Setup the toolbar
			toolbar = new UIToolbar();
			toolbar.BarStyle = UIBarStyle.Default;
			toolbar.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			toolbar.Translucent = true;
			toolbar.SizeToFit();

			// Create a 'done' button for the toolbar and add it to the toolbar

			saveButton = new UIBarButtonItem("Save", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.InterruptTimeText.Text = this.intSelectedHour + ":" + this.intSelectedMinute;
				SettingsData.MaxContIntTimeMin = int.Parse(this.intSelectedHour) * 60 + int.Parse(this.intSelectedMinute);
				this.InterruptTimeText.ResignFirstResponder();
			});

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };

			cancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.InterruptTimeText.ResignFirstResponder();
			});

			toolbar.SetItems(new UIBarButtonItem[] { cancelButton, spacer, saveButton }, true);

			this.InterruptTimeText.InputView = InterruptTimePicker;
			this.InterruptTimeText.InputAccessoryView = toolbar;
		}

		public void newForgottenTimePicker()
		{
			ForgottenTimePicker = new UIPickerView(new CoreGraphics.CGRect(0, 300, 400, 200f));
			ForgottenTimePicker.BackgroundColor = UIColor.FromRGB(220, 220, 220);

			ForgottenTimePicker.UserInteractionEnabled = true;
			ForgottenTimePicker.ShowSelectionIndicator = true;

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

			planModel.NumberSelected += (Object sender, EventArgs e) =>
			{
				this.fgtSelectedHour = planModel.selectedHour;
				this.fgtSelectedMinute = planModel.selectedMinute;

				if (this.fgtSelectedHour.Equals("0") && this.fgtSelectedMinute.Equals("00"))
				{
					this.fgtSelectedMinute = "01";
					ForgottenTimePicker.Select(1, 1, true);
				}
			};

			ForgottenTimePicker.Model = planModel;
			ForgottenTimePicker.BackgroundColor = UIColor.White;

			int maxFgtHour = SettingsData.ForgottenTmrThsMin / 60;
			int maxFgtMin = SettingsData.ForgottenTmrThsMin % 60;
			this.fgtSelectedHour = maxFgtHour.ToString();
			this.fgtSelectedMinute = maxFgtMin.ToString("00");

			this.ForgottenTimerText.Text = this.fgtSelectedHour + ":" + this.fgtSelectedMinute;
			ForgottenTimePicker.Select(maxFgtHour, 0, true);
			ForgottenTimePicker.Select(maxFgtMin, 1, true);

			//Setup the toolbar
			toolbar = new UIToolbar();
			toolbar.BarStyle = UIBarStyle.Default;
			toolbar.BackgroundColor = UIColor.FromRGB(220, 220, 220);
			toolbar.Translucent = true;
			toolbar.SizeToFit();

			// Create a 'done' button for the toolbar and add it to the toolbar

			saveButton = new UIBarButtonItem("Save", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.ForgottenTimerText.Text = this.fgtSelectedHour + ":" + this.fgtSelectedMinute;
				SettingsData.ForgottenTmrThsMin = int.Parse(this.fgtSelectedHour) * 60 + int.Parse(this.fgtSelectedMinute);
				this.ForgottenTimerText.ResignFirstResponder();
			});

			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };

			cancelButton = new UIBarButtonItem("Cancel", UIBarButtonItemStyle.Bordered,
			(s, e) =>
			{
				this.ForgottenTimerText.ResignFirstResponder();
			});

			toolbar.SetItems(new UIBarButtonItem[] { cancelButton, spacer, saveButton }, true);

			this.ForgottenTimerText.InputView = ForgottenTimePicker;
			this.ForgottenTimerText.InputAccessoryView = toolbar;
		}
	}
}

