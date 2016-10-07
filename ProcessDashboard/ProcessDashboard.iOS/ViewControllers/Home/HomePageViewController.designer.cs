// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace ProcessDashboard.iOS
{
    [Register ("HomePageViewController")]
    partial class HomePageViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton completeBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel currentTaskLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton pauseBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton playBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton projectNameBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel recentTasksLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView recentTaskTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton taskNameBtn { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (completeBtn != null) {
                completeBtn.Dispose ();
                completeBtn = null;
            }

            if (currentTaskLabel != null) {
                currentTaskLabel.Dispose ();
                currentTaskLabel = null;
            }

            if (pauseBtn != null) {
                pauseBtn.Dispose ();
                pauseBtn = null;
            }

            if (playBtn != null) {
                playBtn.Dispose ();
                playBtn = null;
            }

            if (projectNameBtn != null) {
                projectNameBtn.Dispose ();
                projectNameBtn = null;
            }

            if (recentTasksLabel != null) {
                recentTasksLabel.Dispose ();
                recentTasksLabel = null;
            }

            if (recentTaskTableView != null) {
                recentTaskTableView.Dispose ();
                recentTaskTableView = null;
            }

            if (taskNameBtn != null) {
                taskNameBtn.Dispose ();
                taskNameBtn = null;
            }
        }
    }
}