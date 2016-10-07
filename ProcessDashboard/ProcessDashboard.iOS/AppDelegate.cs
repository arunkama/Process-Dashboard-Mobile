using System;
using Foundation;
using UIKit;

namespace ProcessDashboard.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window
		{
			get;
			set;
		}
		public UIStoryboard MainStoryboard
		{
			get { return UIStoryboard.FromName("Main", NSBundle.MainBundle); }
		}

		//Creates an instance of viewControllerName from storyboard
		public UIViewController GetViewController(UIStoryboard storyboard, string viewControllerName)
		{
			return storyboard.InstantiateViewController(viewControllerName);
		}

		public void SetRootViewController(UIViewController rootViewController, bool animate)
		{
			if (animate)
			{
				var transitionType = UIViewAnimationOptions.TransitionFlipFromRight;

				Window.RootViewController = rootViewController;
				UIView.Transition(Window, 0.5, transitionType,
								  () => Window.RootViewController = rootViewController,
								  null);
			}
			else
			{
				Window.RootViewController = rootViewController;
			}
		}


		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			if (!String.IsNullOrEmpty(AccountStorage.Password))
			{
				// Account information found. We can skip the login screen
				// If the account credentials are changed on the server side, the user will be requested to login upon the first API request
				var tabBarController = GetViewController(MainStoryboard, "rootTabBarViewController");
				SetRootViewController(tabBarController, false);
			}
			else
			{
				// Haven't logged in even once
				BindLoginViewController();
			}
			return true;
		}

		public void BindLoginViewController()
		{
			LoginPageViewController.OnLoginSuccess += LoginViewController_OnLoginSuccess;
			var loginNavigationController = GetViewController(MainStoryboard, "LoginNavigationController") as UINavigationController;
			SetRootViewController(loginNavigationController, false);
		}

		void LoginViewController_OnLoginSuccess(object sender, EventArgs e)
		{
			//We have successfully Logged In
			var tabBarController = GetViewController(MainStoryboard, "rootTabBarViewController");
			SetRootViewController(tabBarController, true);
		}

		public override void OnResignActivation(UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground(UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground(UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated(UIApplication application)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate(UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}
	}
}


