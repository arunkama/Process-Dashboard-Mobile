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

            Debug.WriteLine("Completion date :" + _completionDate.HasValue);
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

            Debug.WriteLine(" 0 ");
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

            var listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem,
                   output);


            Debug.WriteLine("We have reached the end ");

            timeinfo.Adapter = listAdapter;

            pb.Show();
            Task taskDetail = null;
            try

            {
                // Get data from server
                taskDetail = await ((MainActivity)Activity).Ctrl.GetTask(AccountStorage.DataSet, _taskId);

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



            }
            catch (CannotReachServerException)
            {
                if (pb.IsShowing)
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
            catch (Refit.ApiException we)
            {
                Debug.WriteLine("Web exception :" + we.StatusCode);
                pb.Dismiss();
                if (we.StatusCode == HttpStatusCode.Unauthorized)
                {
                    try
                    {
                        Toast.MakeText(Activity, "Username and password error.", ToastLength.Long).Show();
                        Debug.WriteLine("We are about to logout");
                        AccountStorage.ClearStorage();
                        Activity.StartActivity(typeof(LoginActivity));
                        Activity.Finish();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("We encountered an error :" + e.Message);
                    }
                }

                else
                {
                    // no http status code available
                    Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
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
            if (taskDetail == null)
            {
                Debug.WriteLine("T is null");
            }

            if (taskDetail != null)
            {
                projectName.Text = taskDetail.Project.Name;

                timeinfo.ItemClick += (sender, args) =>
                {

                    if (args.Position == 0)
                    {

                        LinearLayout ll = new LinearLayout(Activity);
                        ll.Orientation = (Orientation.Horizontal);

                        NumberPicker aNumberPicker = new NumberPicker(Activity)
                        {
                            MaxValue = (100),
                            MinValue = (0)
                        };

                        double temp;

                        temp = taskDetail.EstimatedTime;


                        aNumberPicker.Value = TimeSpan.FromMinutes(temp).Hours;

                        NumberPicker aNumberPickerA = new NumberPicker(Activity)
                        {
                            MaxValue = (59),
                            MinValue = (0),
                            Value = TimeSpan.FromMinutes(temp).Minutes
                        };


                        LinearLayout.LayoutParams parameters = new LinearLayout.LayoutParams(50, 50);
                        parameters.Gravity = GravityFlags.Center;

                        LinearLayout.LayoutParams numPicerParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                        numPicerParams.Weight = 1;

                        LinearLayout.LayoutParams qPicerParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                        qPicerParams.Weight = 1;

                        ll.LayoutParameters = parameters;
                        ll.AddView(aNumberPicker, numPicerParams);
                        ll.AddView(aNumberPickerA, qPicerParams);

                        //((TaskDetailsAdapter)(timeinfo.Adapter)).GetEntry()


                        //var ts = DateTime.ParseExact("", "HH.mm", CultureInfo.InvariantCulture);

                        AlertDialog.Builder np = new AlertDialog.Builder(Activity).SetView(ll);

                        np.SetTitle("Update Planned Time");
                        np.SetNegativeButton("Cancel", (s, a) =>
                        {
                            np.Dispose();
                        });
                        np.SetPositiveButton("Ok", (s, a) =>
                        {

                            //Update Planned Time
                            string number = aNumberPicker.Value.ToString("D2") + ":" + aNumberPickerA.Value.ToString("D2");
                            Debug.WriteLine(number);
                            double val = Convert.ToDouble(TimeSpan.ParseExact(number, @"hh\:mm", CultureInfo.InvariantCulture).TotalMinutes);
                            Debug.WriteLine("The updated val is :" + val);
                            try
                            {
                                ((MainActivity)(Activity)).Ctrl.UpdateATask(AccountStorage.DataSet,
                                    _taskId, val, null, false);
                            }
                            catch (CannotReachServerException)
                            {

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
                            catch (Refit.ApiException we)
                            {
                                Debug.WriteLine("Web exception :" + we.StatusCode);
                                pb.Dismiss();
                                if (we.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    try
                                    {
                                        Toast.MakeText(Activity, "Username and password error.", ToastLength.Long).Show();
                                        Debug.WriteLine("We are about to logout");
                                        AccountStorage.ClearStorage();
                                        Activity.StartActivity(typeof(LoginActivity));
                                        Activity.Finish();
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("We encountered an error :" + e.Message);
                                    }
                                }

                                else
                                {
                                    // no http status code available
                                    Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
                                }


                            }
                            catch (StatusNotOkayException se)
                            {

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
                            output[0].value = TimeSpan.FromMinutes(val).ToString(@"hh\:mm");

                            listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem,
                  output);
                            Debug.WriteLine("We have changed content ");
                            timeinfo.Adapter = listAdapter;

                            Toast.MakeText(_mActivity, "Planned Time Updated", ToastLength.Short).Show();
                            np.Dispose();

                        });
                        np.Show();
                        //Planned Time
                    }
                    else if (args.Position == 1)
                    {
                        //Actual Time
                        ((MainActivity)Activity).PassTimeLogInfo(taskDetail.Id, taskDetail.Project.Name,
                               taskDetail.FullName);
                    }
                    else if (args.Position == 2)
                    {
                        // Completion Date


                        DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                        {
                            Debug.WriteLine("The received date is :" + time.ToShortDateString());

                            output[2].value = time.ToShortDateString();

                            listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem,
                  output);
                            Debug.WriteLine("We have changed content ");
                            timeinfo.Adapter = listAdapter;

                            try
                            {
                                ((MainActivity)(Activity)).Ctrl.UpdateATask(AccountStorage.DataSet,
                                    _taskId, null, Util.GetInstance().GetServerTime(time), false);
                            }
                            catch (CannotReachServerException)
                            {

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
                            catch (Refit.ApiException we)
                            {
                                Debug.WriteLine("Web exception :" + we.StatusCode);
                                pb.Dismiss();
                                if (we.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    try
                                    {
                                        Toast.MakeText(Activity, "Username and password error.", ToastLength.Long).Show();
                                        Debug.WriteLine("We are about to logout");
                                        AccountStorage.ClearStorage();
                                        Activity.StartActivity(typeof(LoginActivity));
                                        Activity.Finish();
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("We encountered an error :" + e.Message);
                                    }
                                }

                                else
                                {
                                    // no http status code available
                                    Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
                                }


                            }
                            catch (StatusNotOkayException se)
                            {

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


                            Toast.MakeText(_mActivity, "Completion Date Updated", ToastLength.Short).Show();
                        }, taskDetail.CompletionDate.HasValue? Util.GetInstance().GetLocalTime(taskDetail.CompletionDate.Value):DateTime.Now);
                        //frag.StartTime = DateTime.SpecifyKind(DateTime.Parse(""+output[2].value), DateTimeKind.Local);

                        if (taskDetail.CompletionDate.HasValue)
                            frag.StartTime = Util.GetInstance().GetLocalTime(taskDetail.CompletionDate.Value);
                        Debug.WriteLine(frag.StartTime);
                        frag.Show(FragmentManager, DatePickerFragment.TAG);


                    }

                };

                taskName.Text = taskDetail.FullName;
                output[0].value = TimeSpan.FromMinutes(taskDetail.EstimatedTime).ToString(@"hh\:mm");
                output[1].value = TimeSpan.FromMinutes(taskDetail.ActualTime).ToString(@"hh\:mm");
                output[2].value = taskDetail.CompletionDate.HasValue ? Util.GetInstance().GetLocalTime(taskDetail.CompletionDate.Value).ToShortDateString() : "-";
                listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem,
                     output);


                Debug.WriteLine("We have changed content ");

                timeinfo.Adapter = listAdapter;
                if (string.IsNullOrEmpty(taskDetail.Note))
                {
                    notes.Text = "-";
                    notes.Gravity = GravityFlags.Center;

                }
                else
                    notes.Text = taskDetail.Note;

                var timeLogs = view.FindViewById<Button>(Resource.Id.TaskDetails_TimeLogButton);
                timeLogs.Click +=
                    (sender, args) =>
                    {
                        ((MainActivity)Activity).PassTimeLogInfo(taskDetail.Id, taskDetail.Project.Name,
                            taskDetail.FullName);
                    };

                
                
                    taskComplete.Checked = taskDetail.CompletionDate.HasValue &&
                                           taskDetail.CompletionDate.Value != DateTime.MinValue;
                    
                taskComplete.CheckedChange += (sender, args) =>
                {
                    Debug.WriteLine("Task Complete Changed");
                    string text;

                    if (args.IsChecked)
                    {
                        // Mark a task as complete
                        // Show current date and time
                        // Mark Complete Right button


                        DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                        {

                            Debug.WriteLine("The received date is :" + time.ToShortDateString());
                            output[2].value = time.ToShortDateString();

                            listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem, output);
                            Debug.WriteLine("We have changed content ");
                            timeinfo.Adapter = listAdapter;

                            try
                            {
                                ((MainActivity)(Activity)).Ctrl.UpdateATask(AccountStorage.DataSet, _taskId, null, Util.GetInstance().GetServerTime(time), false);
                            }
                            catch (CannotReachServerException)
                            {

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
                            catch (Refit.ApiException we)
                            {
                                Debug.WriteLine("Web exception :" + we.StatusCode);
                                pb.Dismiss();
                                if (we.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    try
                                    {
                                        Toast.MakeText(Activity, "Username and password error.", ToastLength.Long).Show();
                                        Debug.WriteLine("We are about to logout");
                                        AccountStorage.ClearStorage();
                                        Activity.StartActivity(typeof(LoginActivity));
                                        Activity.Finish();
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine("We encountered an error :" + e.Message);
                                    }
                                }

                                else
                                {
                                    // no http status code available
                                    Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
                                }


                            }
                            catch (StatusNotOkayException se)
                            {

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


                            Toast.MakeText(_mActivity, "Task Marked Complete", ToastLength.Short).Show();
                        }, taskDetail.CompletionDate.HasValue? Util.GetInstance().GetLocalTime(taskDetail.CompletionDate.Value):DateTime.Now);
                        //frag.StartTime = DateTime.SpecifyKind(DateTime.Parse(""+output[2].value), DateTimeKind.Local);

                        if (taskDetail.CompletionDate.HasValue)
                        {
                            frag.StartTime = Util.GetInstance().GetLocalTime(taskDetail.CompletionDate.Value);
                            frag.positiveText = "Mark Complete";
                        }
                        else
                        {
                            frag.StartTime = DateTime.Now;
                            frag.positiveText = "Mark Complete";
                        }

                        Debug.WriteLine(frag.StartTime);
                        frag.Show(FragmentManager, DatePickerFragment.TAG);

                        //-------------------------- ---------------------
                        //((MainActivity)(Activity)).Ctrl.UpdateATask(AccountStorage.DataSet,_taskId, null, convertedTime, false);
                        //  output[2].value = DateTime.Now.ToShortDateString();

                        text = "";
                    }
                    else
                    {

                        // Task is already complete. Default option is to mark the task incomplete
                        // If user begins to change the completion date, provide option to change completion date



                        DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                        {
                            if (!time.Equals(DateTime.MinValue))
                            {
                                Debug.WriteLine("The received date is :" + time.ToShortDateString());
                                output[2].value = time.ToShortDateString();

                                listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem,
                                    output);
                                Debug.WriteLine("We have changed content ");
                                timeinfo.Adapter = listAdapter;

                                try
                                {
                                    ((MainActivity)(Activity)).Ctrl.UpdateATask(AccountStorage.DataSet, _taskId, null,
                                        Util.GetInstance().GetServerTime(time), false);
                                    var previousValue = output[2].value;
                                    // Unmark the task 
                                    taskDetail.CompletionDate = null;

                                }
                                catch (CannotReachServerException)
                                {

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
                                catch (Refit.ApiException we)
                                {
                                    Debug.WriteLine("Web exception :" + we.StatusCode);
                                    pb.Dismiss();
                                    if (we.StatusCode == HttpStatusCode.Unauthorized)
                                    {
                                        try
                                        {
                                            Toast.MakeText(Activity, "Username and password error.", ToastLength.Long).Show();
                                            Debug.WriteLine("We are about to logout");
                                            AccountStorage.ClearStorage();
                                            Activity.StartActivity(typeof(LoginActivity));
                                            Activity.Finish();
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.WriteLine("We encountered an error :" + e.Message);
                                        }
                                    }

                                    else
                                    {
                                        // no http status code available
                                        Toast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
                                    }


                                }
                                catch (StatusNotOkayException se)
                                {

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


                                Toast.MakeText(_mActivity, "Task Marked Complete", ToastLength.Short).Show();
                            }
                        }, taskDetail.CompletionDate.HasValue ? taskDetail.CompletionDate.Value : DateTime.Now);
                        //frag.StartTime = DateTime.SpecifyKind(DateTime.Parse(""+output[2].value), DateTimeKind.Local);

                        if (taskDetail.CompletionDate.HasValue)
                        {

                            frag.StartTime = Util.GetInstance().GetLocalTime(taskDetail.CompletionDate.Value);
                            frag.positiveText = "Mark Complete";
                        }
                        else
                        {
                            frag.StartTime = DateTime.Now;
                            frag.positiveText = "Mark Complete";
                        }

                        Debug.WriteLine(frag.StartTime);
                        frag.Show(FragmentManager, DatePickerFragment.TAG);





                    }

                    listAdapter = new TaskDetailsAdapter(Activity, Resource.Layout.TimeLogEntryListItem, output);
                    Debug.WriteLine("We have changed content ");
                    timeinfo.Adapter = listAdapter;
                    // await (((MainActivity)(Activity)).Ctrl).UpdateTimeLog(Settings.GetInstance().Dataset,)
                };
            }


            if (pb.IsShowing)
                pb.Dismiss();

            // Dismiss Dialog




        }


    }
}