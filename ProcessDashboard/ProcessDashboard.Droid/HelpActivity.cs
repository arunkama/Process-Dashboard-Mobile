using System;
using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;

namespace ProcessDashboard.Droid
{
    [Activity(Label = "Help",ParentActivity = typeof(LoginActivity))]
    [MetaData(NavUtils.ParentActivity, Value = ".LoginActivity")]
    public class HelpActivity : AppCompatActivity
    {

        private static readonly string help_url = "http://www.processdash.com/static/mobile/login.html";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.HelpActivityLayout);

            try
            {
                Android.Support.V7.Widget.Toolbar tb = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.help_toolbar);
                tb.Title = "Process Dashboard Companion";
                SetSupportActionBar(tb);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Message : " + e.Message);
            }

            var web_view = FindViewById<WebView>(Resource.Id.help_webview);
            web_view.Settings.JavaScriptEnabled = true;
            web_view.LoadUrl(help_url);

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                // Respond to the action bar's Up/Home button
                case Android.Resource.Id.Home:
                    NavUtils.NavigateUpFromSameTask(this);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }




    }
}