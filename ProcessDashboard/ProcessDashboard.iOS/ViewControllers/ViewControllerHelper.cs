using System;
using Foundation;
using UIKit;

namespace ProcessDashboard.iOS
{
	public class ViewControllerHelper
	{
		public ViewControllerHelper()
		{
		}

		public static UIViewController GetTopMostViewController()
		{
			UIViewController topVC = UIApplication.SharedApplication.KeyWindow.RootViewController;
			while (topVC.PresentedViewController != null)
			{
				topVC = topVC.PresentedViewController;
			}
			return topVC;
		}

		public static void ShowAlert(UIViewController vc, string title, string msg)
		{
			vc = vc ?? GetTopMostViewController();
			title = title ?? "Connection Unavailable";
			UIAlertController okAlertController = UIAlertController.Create(title, msg, UIAlertControllerStyle.Alert);
			okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
			vc.PresentViewController(okAlertController, true, null);
		}

		// requires UTC time both input and output
		public static DateTime NSDateToDateTimeUtc(NSDate dateUtc)
		{
			DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return reference.AddSeconds(dateUtc.SecondsSinceReferenceDate);
		}

		// requires UTC time both input and output
		public static NSDate DateTimeUtcToNSDate(DateTime dateUtc)
		{
			DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return NSDate.FromTimeIntervalSinceReferenceDate((dateUtc - reference).TotalSeconds);
		}
	}
}

