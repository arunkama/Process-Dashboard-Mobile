using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using UIKit;

namespace ProcessDashboard.iOS
{
	partial class LoginPageViewController : UIViewController
	{
		//Create an event when a authentication is successful
		public static event EventHandler OnLoginSuccess;
		private DataSetLocationResolver _resolver;

		public override void ViewDidLoad()
		{
			String dataToken = AccountStorage.DataToken;
			String userId = AccountStorage.UserId;

			if (!String.IsNullOrEmpty(dataToken) && !String.IsNullOrEmpty(userId))
			{
				DataTokenTextView.Text = dataToken;
				UserIDTextView.Text = userId;
			}
			
			DataTokenTextView.AutocorrectionType = UITextAutocorrectionType.No;
			DataTokenTextView.AutocapitalizationType = UITextAutocapitalizationType.AllCharacters;

			UserIDTextView.AutocapitalizationType = UITextAutocapitalizationType.None;
			UserIDTextView.AutocorrectionType = UITextAutocorrectionType.No;
			base.ViewDidLoad();
		}

		public override void ViewWillAppear(bool animated)
		{
			NavigationController.SetNavigationBarHidden(true, false);
			base.ViewWillAppear(animated);
		}

		public override void ViewWillDisappear(bool animated)
		{
			NavigationController.SetNavigationBarHidden(false, true);
			base.ViewWillDisappear(animated);
		}

		public LoginPageViewController(IntPtr handle) : base(handle)
		{
			_resolver = new DataSetLocationResolver();
		}

		partial void LoginButton_TouchUpInside(UIButton sender)
		{
			View.EndEditing(true);

			String userId = UserIDTextView.Text.Trim();
			String password = PasswordTextView.Text;
			String dataToken = DataTokenTextView.Text.Trim();

			Boolean dataReady = userId.Length * password.Length * dataToken.Length != 0;

			if (!dataReady)
			{
				new UIAlertView("Invalid Data", "Please make sure all blanks are filled before continue", null, "OK", null).Show();
				return;
			}

			if (!CrossConnectivity.Current.ConnectionTypes.Contains(ConnectionType.Cellular) &&
				!CrossConnectivity.Current.ConnectionTypes.Contains(ConnectionType.WiFi))
			{ 
				new UIAlertView("Connection Unavailable", "Please enable cellular data or WiFi and try again.", null, "OK", null).Show();
				return;
			}

			String baseUrl;
			String dataSet;
			try
			{
				_resolver.ResolveFromToken(dataToken, out baseUrl, out dataSet);
			}
			catch (Exception e)
			{
				new UIAlertView("Invalid DataToken", "DataToken not recognized", null, "OK", null).Show();
				return;
			}

			System.Net.HttpWebResponse resp;
			try
			{
				var req = System.Net.WebRequest.CreateHttp(baseUrl + "api/v1/datasets/" + dataSet + "/");
				req.Method = "GET";
				req.AllowAutoRedirect = false;
				String credential = userId + ":" + password;
				req.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(credential)));
				resp = (System.Net.HttpWebResponse)req.GetResponse();
			}
			catch (System.Net.WebException e)
			{
				resp = (System.Net.HttpWebResponse)e.Response;
				if (resp == null)
				{ 
					new UIAlertView("Server Unavailable", "Cannot reach server \"" + baseUrl + "\". " + e.Message, null, "OK", null).Show();
				}
				else if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					new UIAlertView("Bad Credentials", "Your user ID or password is not correct.", null, "OK", null).Show();
				}
				else if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
				{
					new UIAlertView("Permission Denied", "You may not have the permission to connect to the given dataset", null, "OK", null).Show();
				}
				else
				{
					new UIAlertView("Server Not Recognized", "Server \"" + baseUrl + "\" is not recognized. " + e.Message, null, "OK", null).Show();
				}
				return;
			}

			if (resp.StatusCode == System.Net.HttpStatusCode.OK)
			{
				if (resp.GetResponseStream().CanRead)
				{
					byte[] buffer = new byte[10001];
					resp.GetResponseStream().Read(buffer, 0, 10000);
					String responseStr = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
					if (responseStr.Contains("auth-required"))
					{
						new UIAlertView("Wrong Credentials", "Your user ID or password is not correct", null, "OK", null).Show();
						return;
					}
					else if (responseStr.Contains("permission-denied"))
					{
						new UIAlertView("Permission Denied", "You may not have the permission to connect to the given dataset", null, "OK", null).Show();
						return;
					}
					else if (responseStr.Contains("dataset"))
					{
						AccountStorage.Set(userId, password, baseUrl, dataSet, dataToken);
						PDashAPI.Controller.RefreshDataset();
						if (OnLoginSuccess != null)
						{
							OnLoginSuccess(sender, new EventArgs());
						}
					}
					else
					{
						new UIAlertView("Server Not Recognized", "Server \"" + baseUrl + "\" is not recognized.", null, "OK", null).Show();
						return;
					}
				}
				else
				{
					new UIAlertView("Error", "Comunication with the server failed. Status code: " + resp.StatusCode + ", " + resp.StatusDescription, null, "OK", null).Show();
					return;
				}
			}
			else
			{
				new UIAlertView("Invalid Data", "Server is not recognized. Please check the DataToken", null, "OK", null).Show();
				return;
			}
		}

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier.Equals("login2help"))
			{
				UIViewController vc = segue.DestinationViewController;
				var webView = new UIWebView(vc.View.Bounds);
				View.AddSubview(webView);
				var url = "http://www.processdash.com/mobile-login"; // NOTE: https secure request
				webView.LoadRequest(new NSUrlRequest(new NSUrl(url)));
				webView.ScalesPageToFit = true;
				vc.View.Add(webView);
			}
			base.PrepareForSegue(segue, sender);
		}
	}
}
