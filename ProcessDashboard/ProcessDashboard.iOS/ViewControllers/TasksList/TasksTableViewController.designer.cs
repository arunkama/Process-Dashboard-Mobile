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
    [Register ("TasksTableViewController")]
    partial class TasksTableViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView tasksTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (tasksTableView != null) {
                tasksTableView.Dispose ();
                tasksTableView = null;
            }
        }
    }
}