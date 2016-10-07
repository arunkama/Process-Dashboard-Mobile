#region
using System;
using System.Net;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using ProcessDashboard.DTO;
using Debug = System.Diagnostics.Debug;
using Object = Java.Lang.Object;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    public class TaskTimeLogList : ListFragment
    {
        private string _id { get; set; }
        private string ProjectName { get; set; }
        private string TaskName { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            // Create your fragment here
            ((MainActivity) Activity).SetTitle("Task Timelogs");
            // Create your fragment here
        }

        public void SetData(string id, string projectName, string taskName)
        {
            _id = id;
            ProjectName = projectName;
            TaskName = TaskName;
        }

        public override void OnResume()
        {
            base.OnResume();
            ((MainActivity) Activity).SetTitle("Task Timelogs");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var v = inflater.Inflate(Resource.Layout.TaskTimeLogList, container, false);
            GetData(v);
            return v;
        }

        public async void GetData(View v)
        {
            var ctrl = ((MainActivity) Activity).Ctrl;

            var pn = v.FindViewById<TextView>(Resource.Id.TaskTimeLog_ProjectName);
            var tn = v.FindViewById<TextView>(Resource.Id.TaskTimeLog_TaskName);
            var fab = v.FindViewById<FloatingActionButton>(Resource.Id.TaskTimeLog_fab);

            pn.Text = ProjectName;
            tn.Text = TaskName;

            var pb = new ProgressDialog(Activity) {Indeterminate = true};
            pb.SetTitle("Loading");
            pb.SetCanceledOnTouchOutside(false);
            pb.Show();

            try
            {
                var timelogs = await ctrl.GetTimeLogs(AccountStorage.DataSet, null, null, null, _id, null);

                Debug.WriteLine("Got data for timelogs :" + timelogs.Count);
                fab.Click += (sender, args) =>
                {
                    if (timelogs.Count > 0)
                    {
                        Debug.WriteLine("Project Name :" + timelogs[0].Task.Project.Name);
                        Debug.WriteLine("Task Name :" + timelogs[0].Task.FullName);

                        ((MainActivity) Activity).TimeLogEditCallBack(timelogs[0].Task.Project.Name,
                            timelogs[0].Task.FullName, timelogs[0].Task.Id, null);
                    }
                    else
                    {
                        ((MainActivity) Activity).TimeLogEditCallBack(ProjectName, TaskName, _id, null);
                    }
                };
                fab.Show();
                foreach (var timelog in timelogs)

                {
                    Debug.WriteLine(timelog);
                }

                var listAdapter = new TaskTimeLogAdapter(Activity, Resource.Layout.TimeLogEntryListItem,
                    timelogs.ToArray());
                ListAdapter = listAdapter;

                if (timelogs.Count > 0)
                {
                    pn.Text = timelogs[0].Task.Project.Name;
                    tn.Text = timelogs[0].Task.FullName;
                }
                pb.Dismiss();


                ListView.ItemClick += (sender, args) =>
                {
                    var i = args.Position;
                    Debug.WriteLine("I position :" + i);
                    ((MainActivity) Activity).TimeLogEditCallBack(timelogs[i].Task.Project.Name,
                        timelogs[i].Task.FullName,
                        timelogs[i].Task.Id, timelogs[i]);
                };
            }
            catch (CannotReachServerException)
            {
                if(pb.IsShowing)
                    pb.Dismiss();
                
                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetTitle("Unable to Connect")
                    .SetMessage("Please check your network connection and try again")
                      .SetNeutralButton("Okay", (sender2, args2) =>
                      {
                          builder.Dispose();
                          ((MainActivity)Activity).FragmentManager.PopBackStack();
                      })
                    .SetCancelable(false);
                AlertDialog alert = builder.Create();
                alert.Show();




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

                                if (pb.IsShowing)
                                    pb.Dismiss();
                                Toast.MakeText(Activity, "Username and password error.", ToastLength.Long).Show();
                                Debug.WriteLine("We are about to logout");
                                AccountStorage.ClearStorage();
                                Debug.WriteLine("Main Activity is :" + Activity == null);
                                Debug.WriteLine("Items in the backstack :" + Activity.FragmentManager.BackStackEntryCount);
                                Debug.WriteLine("Main Activity is :" + Activity == null);
                                Activity.FragmentManager.PopBackStack(null, PopBackStackFlags.Inclusive);
                                Debug.WriteLine("Items in the backstack 2 :" + Activity.FragmentManager.BackStackEntryCount);
                                ((MainActivity)(Activity)).SetDrawerState(false);
                                ((MainActivity)(Activity)).SwitchToFragment(MainActivity.FragmentTypes.Login);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("We encountered an error :" + e.Message);
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
                    // no http status code availableToast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
                }
            }
            catch (StatusNotOkayException se)
            {
                if (pb.IsShowing)
                    pb.Dismiss();

                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetTitle("An Error has occured")
                    .SetMessage("Error :" + se.GetMessage())
                    .SetNeutralButton("Okay", (sender2, args2) =>
                    {
                        builder.Dispose();
                    })
                    .SetCancelable(false);
                AlertDialog alert = builder.Create();
                alert.Show();


            }
            catch (Exception e)
            {
                // For any other weird exceptions

                if (pb.IsShowing)
                    pb.Dismiss();

                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetTitle("An Error has occured")
                      .SetNeutralButton("Okay", (sender2, args2) =>
                      {
                          builder.Dispose();
                      })
                    .SetMessage("Error :" + e.Message)
                    .SetCancelable(false);
                AlertDialog alert = builder.Create();
                alert.Show();

            }
        }

    }

    public class TaskTimeLogAdapter : BaseAdapter
    {
        private readonly TimeLogEntry[] _values;
        private readonly Activity context;
        private readonly int resource;

        public TaskTimeLogAdapter(Activity context, int resource, TimeLogEntry[] objects)
        {
            _values = objects;
            this.context = context;
            this.resource = resource;
            
        }


        public override Object GetItem(int position)
        {
            return "";
        }
        public override long GetItemId(int position)
        {
            return 0;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var v = convertView ?? context.LayoutInflater.Inflate(resource, null);

            var name = Util.GetInstance().GetLocalTime(_values[position].StartDate).ToShortDateString();
            var value = TimeSpan.FromMinutes(_values[position].LoggedTime).ToString(@"hh\:mm");

            Debug.WriteLine("Inside name :"+name);
            Debug.WriteLine("Inside Value :"+value);

            var tv = v.FindViewById<TextView>(Resource.Id.Entry_name);
            var sv = v.FindViewById<TextView>(Resource.Id.Entry_value);

            tv.Text = name;
            sv.Text = "" + value;
            
            return v;
        }
        public override int Count {
            get { return _values.Length; } 
        }


     
        public TimeLogEntry GetTimeLog(int position)
        {
            return _values[position];
        }
    }
}