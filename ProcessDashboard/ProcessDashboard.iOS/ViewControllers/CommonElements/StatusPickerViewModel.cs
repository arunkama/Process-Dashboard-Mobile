using System;
using Foundation;
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
	public class StatusPickerViewModel : UIPickerViewModel
	{
		public string[] Hour;
		public string[] Minute;
		public string selectedHour { get; set; }
		public string selectedMinute { get; set; }

		public event EventHandler NumberSelected;

		public StatusPickerViewModel(string[] Hour, string[] Minute)
		{
			this.Hour = Hour;
			this.Minute = Minute;
		}

		public override void Selected(UIPickerView picker, nint row, nint component)
		{


			selectedHour = Hour[picker.SelectedRowInComponent(0)];
			selectedMinute = Minute[picker.SelectedRowInComponent(1)];
			if (NumberSelected != null)
			{
				NumberSelected(this, EventArgs.Empty);
			}
		
		}

		public override nint GetComponentCount(UIPickerView v)
		{
			return 2;
		}

		public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
		{
			if (component == 0)
			{
				return Hour.Length;
			}
			else
			{
				return Minute.Length;
			}

		}

		public override string GetTitle(UIPickerView picker, nint row, nint component)
		{
			if (component == 0)
			{

				return Hour[row];
			}
			else
			{
				return Minute[row];
			}

		}


		public override nfloat GetComponentWidth(UIPickerView picker, nint component)
		{
			if (component == 0)
				return 100f;
			else
				return 40f;
		}

		public override nfloat GetRowHeight(UIPickerView picker, nint component)
		{
			return 40f;
		}
	}
}

