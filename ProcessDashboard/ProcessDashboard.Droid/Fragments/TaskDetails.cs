#region
using System;
using System.Globalization;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using ProcessDashboard.Droid.Adapter;
using ProcessDashboard.DTO;
using Debug = System.Diagnostics.Debug;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    public class TaskDetails : Fragment
    {
        private double? _actualTime;
        private DateTime? _completionDate;
        private double? _estimatedTime;
        private string _projectName;
        private string _taskId;
        private string _taskName;
        private Activity _mActivity;
        private Home.myBroadCastReceiver _onNotice;
        private IntentFilter _iff;

        private Button _play, _pause;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            _mActivity = (MainActivity)Activity;
            // Create your fragment here
            ((MainActivity)Activity).SetTitle("Task Details");
            // Create your fragment here
        }

        public override void OnPause()
        {
            base.OnPause();
            LocalBroadcastManager.GetInstance(Activity).UnregisterReceiver(_onNotice);

        }


        public override void OnResume()
        {
            base.OnResume();
            ((MainActivity)Activity).SetTitle("Task Details");
            _iff = new IntentFilter("processdashboard.timelogger");
            _onNotice = new Home.myBroadCastReceiver((MainActivity)Activity);
            LocalBroadcastManager.GetInstance(Activity).RegisterReceiver(_onNotice, _iff);
        }

        public void SetId(string id, string taskName, string projectName, DateTime? completionDate,
            double? estimatedTime, double? actualTime)
        {
            _taskId = id;
            _taskName = taskName;
            _projectName = projectName;
            _completionDate = completionDate;
            _estimatedTime = estimatedTime;
            _actualTime = actualTime;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var v = inflater.Inflate(Resource.Layout.TaskDetail, container, false);
            Debug.WriteLine("On create view called");
            AddData(v);
            return v;
        }

        public void ModifyPlayPauseState(bool isPlaying)
        {
            if (isPlaying)
            {
                _play.SetBackgroundResource(Resource.Drawable.play_activated);
                _pause.SetBackgroundResource(Resource.Drawable.pause_deactivated);
            }
            else
            {
                _play.SetBackgroundResource(Resource.Drawable.play_deactivated);
                _pause.SetBackgroundResource(Resource.Drawable.pause_activated);
            }
        }


        private async void AddData(View view)
        {
            var projectName = view.FindViewById<TextView>(Resource.Id.TaskDetails_ProjectName);
            var taskName = view.FindViewById<TextView>(Resource.Id.TaskDetails_TaskName);
            var notes = view.FindViewById<EditText>(Resource.Id.TaskDetails_Notes);
            var timeinfo = view.FindViewById<ListView>(Resource.Id.TaskDetails_TimeInfo);
            var taskComplete = view.FindViewById<CheckBox>(Resource.Id.TaskDetails_TaskComplete);

            Debug.WriteLine("Completion date :"+_completionDate.HasValue);
            Debug.WriteLine("Completion date :" + _completionDate.GetValueOrDefault());
            Debug.WriteLine("State to set :" + (_completionDate.HasValue && _completionDate.Value != DateTime.MinValue));
            Debug.WriteLine("State before :" + taskComplete.Checked);

            taskComplete.Checked = _completionDate.HasValue && _completionDate.Value != DateTime.MinValue;

            //taskComplete.Checked = true;

            Debug.WriteLine("State after :" + taskComplete.Checked);
            _play = view.FindViewById<Button>(Resource.Id.TaskDetails_Play);
            _pause = view.FindViewById<Button>(Resource.Id.TaskDetails_Pause);
             
            Debug.WriteLine("We have set the checkbox values");
            if (TimeLoggingController.GetInstance().IsTimerRunning() && TimeLoggingController.GetInstance().GetTimingTaskId().Equals(_taskId))
            {
                ModifyPlayPauseState(true);
            }
            var pb = new ProgressDialog(_mActivity) { Indeterminate = true };
            pb.SetTitle("Loading");
            pb.SetCanceledOnTouchOutside(false);
            if (_taskName != null)
                taskName.Text = _taskName;

            if (_projectName != null)
                projectName.Text = _projectName;

            Debug.WriteLine(" 0 :"+ taskComplete.Checked);
            Entry[] output = new Entry[3];

            output[0] = new Entry();
            output[1] = new Entry();
            output[2] = new Entry();

            output[0].name = "Planned Time";

            if (_estimatedTime.HasValue)
            {
                Debug.WriteLine("We have a value in estimated time");
                output[0].value = "" + TimeSpan.FromMinutes(_estimatedTime.Value).ToString(@"hh\:mm");
            }
            else
            {
                Debug.WriteLine("No value in estimated time");
                output[0].value = "";
            }
            Debug.WriteLine(" 1 ");


            output[1].name = "Actual Time";
            if (_actualTime.HasValue)
                output[1].value = "" + TimeSpan.FromMinutes(_actualTime.Value).ToString(@"hh\:mm");
            else
                output[1].value = "";

            output[2].name = "Completion Date";
            Debug.WriteLine(" 2 ");
            if (_completionDate.HasValue)

                output[2].value = Util.GetInstance().GetLocalTime(_completionDate.Value).ToShortDateString();
            else
                output[2].value = "-";
            Debug.WriteLine("output set :" + taskComplete.Checked);
            var listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem,
                   output);


            Debug.WriteLine("State pre-end :" + taskComplete.Checked);

            timeinfo.Adapter = listAdapter;
            Debug.WriteLine("State pre-end 1 :" + taskComplete.Checked);
            pb.Show();
            Task taskDetail = null;
            try

            {
                // Get data from server
                Debug.WriteLine("State pre-end 1.5 :" + taskComplete.Checked);
                taskDetail = await ((MainActivity)Activity).Ctrl.GetTask(AccountStorage.DataSet, _taskId);
                Debug.WriteLine("State pre-end 2 :" + taskComplete.Checked);
                _play.Click += (sender, args) =>
                {
                    Debug.WriteLine("Play Clicked");

                    //var timerServiceIntent = new Intent("com.tumasolutions.processdashboard.TimerService");

                    //var timerServiceConnection = new TimerServiceConnection((MainActivity)this.Activity);

                    //Activity.ApplicationContext.BindService(timerServiceIntent, timerServiceConnection, Bind.AutoCreate);
                    Intent intent = new Intent(Activity, typeof(TimerService));
                    intent.PutExtra("taskId", taskDetail.Id);
                    Activity.StartService(intent);

                };
                
                _pause.Click += (sender, args) =>
                {
                    Debug.WriteLine("Pause Clicked");
                    Activity.StopService(new Intent(Activity, typeof(TimerService)));
                    Toast.MakeText(Activity, "Time Log Entry Saved", ToastLength.Short).Show();

                };

                projectName.Click += (obj, args) =>
                {
                    var projectId = taskDetail.Project.Id;
                    var projectname = taskDetail.Project.Name;

                    ((MainActivity)Activity).ListOfProjectsCallback(projectId, projectname);

                };
                Debug.WriteLine("State pre-end 3 :" + taskComplete.Checked);


            }
            catch (CannotReachServerException)
            {
                if(pb.IsShowing)
                    pb.Dismiss();
                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetTitle("Unable to Connect")
                    .SetMessage("Please check your network connection and try again")
                      .SetNeutralButton("Okay", (sender, args) =>
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
                                AccountStorage.ClearStorage();
                                Activity.FragmentManager.PopBackStack(null, PopBackStackFlags.Inclusive);
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
                    .SetNeutralButton("Okay", (sender, args) =>
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
                      .SetNeutralButton("Okay", (sender, args) =>
                      {
                          builder.Dispose();
                      })
                    .SetMessage("Error :" + e.Message)
                    .SetCancelable(false);
                AlertDialog alert = builder.Create();
                alert.Show();

            }
            Debug.WriteLine("State pre-end 4 :" + taskComplete.Checked);
            if (taskDetail == null)
            {
                Debug.WriteLine("T is null");
            }

            if (pb.IsShowing)
                pb.Dismiss();

            // Dismiss Dialog
            Debug.WriteLine("State end :" + taskComplete.Checked);



        }

     
    }
}