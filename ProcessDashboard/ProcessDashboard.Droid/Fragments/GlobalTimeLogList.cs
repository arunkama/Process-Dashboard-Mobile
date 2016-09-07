#region
using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ProcessDashboard.Droid.Adapter;
using ProcessDashboard.DTO;
using Debug = System.Diagnostics.Debug;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    public class GlobalTimeLogList : Fragment
    {
        private Dictionary<string, List<TimeLogEntry>> _headings = new Dictionary<string, List<TimeLogEntry>>();
        private List<string> _timelogs = new List<string>();

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            // Create your fragment here
            ((MainActivity)Activity).SetTitle("Global Time Log");

            // Create your fragment here
        }

        public override void OnResume()
        {
            base.OnResume();
            ((MainActivity)Activity).SetTitle("Global Time Log");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var v = inflater.Inflate(Resource.Layout.GlobalTimeLog, container, false);

            CreateExpendableListData(v);
            //   Debug.WriteLine("We are proceeding");

            //var listView = v.FindViewById<ExpandableListView>(Resource.Id.myExpandableListview);
            //listView.SetAdapter(new ExpandableDataAdapter(this, Data.SampleData()));

            return v;
        }

        private async void CreateExpendableListData(View v)
        {
            var ctrl = ((MainActivity)Activity).Ctrl;

            try
            {
                ProgressDialog pd = new ProgressDialog(this.Activity);
                pd.SetMessage("Loading");
                pd.Show();

                var timelogEntries = await ctrl.GetTimeLogs(AccountStorage.DataSet, 0, null, null, null, null);

                //  Debug.WriteLine("Got the values : " + timelogEntries.Count);
                var count = 0;


                foreach (var te in timelogEntries)
                {
                    try
                    {
                        var present = true;
                        List<TimeLogEntry> children;
                        _headings.TryGetValue(te.StartDate.ToShortDateString(), out children);
                        if (children == null)
                        {
                            // Debug.WriteLine("Children is null");
                            children = new List<TimeLogEntry>();
                            count++;
                            present = false;
                        }
                        //  Debug.WriteLine("Going to add children");
                        children.Add(te);

                        if (present)
                        {
                            // Debug.WriteLine("Going to remove");
                            _headings.Remove(te.StartDate.Date.ToShortDateString());
                        }
                        //Debug.WriteLine("Going to add to _headings");
                        _headings.Add(te.StartDate.Date.ToShortDateString(), children);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }



                // Debug.WriteLine("Count :" + count);

                _timelogs = new List<string>(_headings.Keys);
                var ctlExListBox = v.FindViewById<ExpandableListView>(Resource.Id.myExpandableListview);
                ctlExListBox.SetAdapter(new GlobalTimeLogAdapter(Activity, _headings));

                ctlExListBox.ChildClick += delegate(object sender, ExpandableListView.ChildClickEventArgs e)
                {
                    var itmGroup = _timelogs[e.GroupPosition];
                    var itmChild = _headings[itmGroup][e.ChildPosition];
                    ((MainActivity) Activity).TimeLogEditCallBack(itmChild.Task.Project.Name, itmChild.Task.FullName,
                        itmChild.Task.Id, itmChild);
                };
                pd.Dismiss();
            }
            catch (CannotReachServerException)
            {
                //TODO: Retry option ?
                Toast.MakeText(Activity, "Please check your internet connection and try again.", ToastLength.Long)
                    .Show();
            }
            catch (StatusNotOkayException)
            {
                Toast.MakeText(Activity, "An error occured. Please try again.", ToastLength.Short).Show();
            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = we.Response as HttpWebResponse;
                    if (response != null)
                    {
                        Console.WriteLine("HTTP Status Code: " + (int)response.StatusCode);
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            try
                            {
                                Toast.MakeText(this.Activity,"Username and password error.",ToastLength.Long).Show();
                                AccountStorage.ClearStorage();
                                Activity.FragmentManager.PopBackStack(null, PopBackStackFlags.Inclusive);
                                ((MainActivity)(Activity)).SetDrawerState(false);
                                ((MainActivity)(Activity)).SwitchToFragment(MainActivity.FragmentTypes.Login);
                            }
                            catch (System.Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine("We encountered an error :" + e.Message);
                            }
                        }
                    }
                    else
                    {
                        // no http status code available
                        Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
                    }
                }
                else
                {
                    // no http status code available
                    Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
                }
            }
             catch (Exception)
            {
                // For any other weird exceptions
                Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
            }
        }
    }
}