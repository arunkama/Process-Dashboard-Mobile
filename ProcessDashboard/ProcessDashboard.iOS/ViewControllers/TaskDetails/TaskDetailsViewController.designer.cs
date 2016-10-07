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
    [Register ("TaskDetailsViewController")]
    partial class TaskDetailsViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView PlanTable { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton TdCheckboxBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextView TdNotesTf { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton TdPauseBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton TdPlayBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton tdProjectNameBtn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TdTaskNameLb { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (PlanTable != null) {
                PlanTable.Dispose ();
                PlanTable = null;
            }

            if (TdCheckboxBtn != null) {
                TdCheckboxBtn.Dispose ();
                TdCheckboxBtn = null;
            }

            if (TdNotesTf != null) {
                TdNotesTf.Dispose ();
                TdNotesTf = null;
            }

            if (TdPauseBtn != null) {
                TdPauseBtn.Dispose ();
                TdPauseBtn = null;
            }

            if (TdPlayBtn != null) {
                TdPlayBtn.Dispose ();
                TdPlayBtn = null;
            }

            if (tdProjectNameBtn != null) {
                tdProjectNameBtn.Dispose ();
                tdProjectNameBtn = null;
            }

            if (TdTaskNameLb != null) {
                TdTaskNameLb.Dispose ();
                TdTaskNameLb = null;
            }
        }
    }
}