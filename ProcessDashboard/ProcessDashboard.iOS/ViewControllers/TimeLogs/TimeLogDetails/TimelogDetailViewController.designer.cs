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
    [Register ("TimelogDetailViewController")]
    partial class TimelogDetailViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel CommentLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CommentText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel DeltaLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField DeltaText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel IntLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField IntText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ProjectNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel StartTimeLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField StartTimeText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TaskNameLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CommentLabel != null) {
                CommentLabel.Dispose ();
                CommentLabel = null;
            }

            if (CommentText != null) {
                CommentText.Dispose ();
                CommentText = null;
            }

            if (DeltaLabel != null) {
                DeltaLabel.Dispose ();
                DeltaLabel = null;
            }

            if (DeltaText != null) {
                DeltaText.Dispose ();
                DeltaText = null;
            }

            if (IntLabel != null) {
                IntLabel.Dispose ();
                IntLabel = null;
            }

            if (IntText != null) {
                IntText.Dispose ();
                IntText = null;
            }

            if (ProjectNameLabel != null) {
                ProjectNameLabel.Dispose ();
                ProjectNameLabel = null;
            }

            if (StartTimeLabel != null) {
                StartTimeLabel.Dispose ();
                StartTimeLabel = null;
            }

            if (StartTimeText != null) {
                StartTimeText.Dispose ();
                StartTimeText = null;
            }

            if (TaskNameLabel != null) {
                TaskNameLabel.Dispose ();
                TaskNameLabel = null;
            }
        }
    }
}