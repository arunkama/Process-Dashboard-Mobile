#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using HockeyApp.Android;
using ProcessDashboard.Droid.Adapter;
using ProcessDashboard.DTO;
using ProcessDashboard.SyncLogic;
using Debug = System.Diagnostics.Debug;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    public class Home : ListFragment,ExpandableListView.IOnGroupCollapseListener,ExpandableListView.IOnChildClickListener
    {
        private MainActivity _mActivity;
        private myBroadCastReceiver onNotice;
        private IntentFilter iff;

        private Dictionary<string, List<Task>> _headings = new Dictionary<string, List<Task>>();
       
        private ExpandableListView expandableList;
        private Button play, pause;
        
        private string _currenttaskid;


        public override void OnAttach(Context activity)
        {
            base.OnAttach(activity);
            _mActivity = (MainActivity)activity;
        }

        //Disabling compiler warnings

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            _mActivity = (MainActivity)activity;
            //
        }

        public override void OnDetach()
        {
            base.OnDetach();
            _mActivity = null;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            _mActivity.SetTitle("Process Dashboard");

            AccountStorage.SetContext(Activity);
        }

        public override void OnPause()
        {
            base.OnPause();
            LocalBroadcastManager.GetInstance(Activity).UnregisterReceiver(onNotice);
        }

        public override void OnResume()
        {
            base.OnResume();
            iff = new IntentFilter("processdashboard.timelogger");

            onNotice = new myBroadCastReceiver((MainActivity)Activity);
            LocalBroadcastManager.GetInstance(Activity).RegisterReceiver(onNotice, iff);
            _mActivity.SetTitle("Process Dashboard");
        }


        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);
            var ta = (TaskAdapter)l.Adapter;
            var pb = ta.GetTask(position);
            _mActivity.PassTaskDetailsInfo(pb.Id, null, null, null, null, null);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            var v = inflater.Inflate(Resource.Layout.Home, container, false);
            LoadData(v, _mActivity.Ctrl);
            return v;
        }

        private async void LoadData(View v, Controller ctrl)
        {
            play = v.FindViewById<Button>(Resource.Id.Home_Play);
            pause = v.FindViewById<Button>(Resource.Id.Home_Pause);
            var taskComplete = v.FindViewById<CheckBox>(Resource.Id.Home_TaskComplete);
            var recentTask = v.FindViewById<Button>(Resource.Id.Home_RecentTask);
            var recentProject = v.FindViewById<Button>(Resource.Id.Home_CurrentProject);
            expandableList = v.FindViewById<ExpandableListView>(Android.Resource.Id.List);


            var pb = new ProgressDialog(_mActivity) { Indeterminate = true };
            pb.SetTitle("Loading");

            //TODO: This is under the assumption that whatever task is on the top of the recent task list
            // is the currently logged timer task. Check whether this assumption is valid or not.

            if (TimeLoggingController.GetInstance().IsTimerRunning())
                ModifyPlayPauseState(true);

            play.Click += (sender, args) =>
            {
                Debug.WriteLine("Play Clicked");

                //var timerServiceIntent = new Intent("com.tumasolutions.processdashboard.TimerService");
                //var timerServiceConnection = new TimerServiceConnection((MainActivity)this.Activity);
                //Activity.ApplicationContext.BindService(timerServiceIntent, timerServiceConnection, Bind.AutoCreate);
                if (TimeLoggingController.GetInstance().WasNetworkAvailable)
                {
                
                    Intent intent = new Intent(Activity, typeof(TimerService));
                    intent.PutExtra("taskId", _currenttaskid);
                    Activity.StartService(intent);
                }
                else
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                    builder.SetTitle("Previous changes not saved")
                        .SetMessage("An earlier time log entry has not yet been synchronized with the server. Please try again later.")
                          .SetNeutralButton("Okay", (sender2, args2) =>
                          {
                              builder.Dispose();
                          })
                        .SetCancelable(false);
                    AlertDialog alert = builder.Create();
                    alert.Show();
                }

            };

            pause.Click += (sender, args) =>
            {
                Debug.WriteLine("Pause Clicked");
                Activity.StopService(new Intent(Activity, typeof(TimerService)));
                Toast.MakeText(Activity, "Time Log Entry Saved", ToastLength.Short).Show();

            };


            taskComplete.CheckedChange += (sender, args) =>
          {
              string text = "";
              if (args.IsChecked)
              {
                    // Mark a task as complete
                    DateTime convertedTime = Util.GetInstance().GetServerTime(DateTime.UtcNow);
                    //taskDetail.CompletionDate = convertedTime;

                    try
                  {
                      ((MainActivity)(Activity)).Ctrl.UpdateATask(AccountStorage.DataSet,
                          _currenttaskid, null, convertedTime, false);
                      text = "Task Marked Complete";

                  }
                  catch (CannotReachServerException)
                  {
                      if (pb.IsShowing)
                          pb.Dismiss();
                      Debug.WriteLine("We could not reach the server");
                      taskComplete.Checked = false;
                      text = "Please check your internet connection and try again.";
                      Toast.MakeText(Activity, text, ToastLength.Short).Show();

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
                  catch (StatusNotOkayException)
                  {
                      pb.Dismiss();
                      taskComplete.Checked = false;
                        //TODO: Should we report this ?
                        text = "An error has occured. Please try again.";
                      Toast.MakeText(Activity, text, ToastLength.Short).Show();

                  }
                  catch (Exception e)
                  {
                        // For any other weird exceptions
                        pb.Dismiss();

                      taskComplete.Checked = false;
                        // Sending to HockeyApp
                        ExceptionHandler.SaveException(Java.Lang.Throwable.FromException(e), null, null);
                      text = "Unable to make the change. Please try again.";
                      Toast.MakeText(Activity, text, ToastLength.Short).Show();
                  }


              }
              else
              {

                    // Unmark the task 

                    try
                  {

                      ((MainActivity)(Activity)).Ctrl.UpdateATask(AccountStorage.DataSet,
                          _currenttaskid, null, null, true);
                      text = "Task Marked Incomplete";

                  }
                  catch (CannotReachServerException)
                  {
                      if (pb.IsShowing)
                          pb.Dismiss();
                      Debug.WriteLine("We could not reach the server");
                      taskComplete.Checked = true;
                      text = "Please check your internet connection and try again.";
                      Toast.MakeText(Activity, text, ToastLength.Short).Show();


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
                  catch (StatusNotOkayException)
                  {
                      pb.Dismiss();
                      taskComplete.Checked = true;
                      text = "An error has occured. Please try again.";
                      Toast.MakeText(Activity, text, ToastLength.Short).Show();


                  }
                  catch (Exception e)
                  {
                        // For any other weird exceptions
                        taskComplete.Checked = true;
                      ExceptionHandler.SaveException(Java.Lang.Throwable.FromException(e), null, null);
                      text = "Unable to make the change. Please try again.";
                      Toast.MakeText(Activity, text, ToastLength.Short).Show();

                  }


              }
              Debug.WriteLine("We have changed content ");
              Toast.MakeText(Activity, text, ToastLength.Short).Show();

          };
            recentTask.Text = "Loading..";

            recentProject.Text = "Loading..";

            pb.SetCanceledOnTouchOutside(false);
            pb.Show();

            try
            {
                Debug.WriteLine("We are about to get the remote data");
                var output = await ctrl.GetRecentTasks(AccountStorage.DataSet);
                Debug.WriteLine("Proceeding");
                pb.Dismiss();

                var recent = output[0];

                recentTask.Text = recent.FullName;
                recentProject.Text = recent.Project.Name;
                _currenttaskid = recent.Id;
                _headings = new Dictionary<string, List<Task>>();
                _headings.Add(recent.Project.Name,new List<Task>());

                output.RemoveAt(0);

                foreach(Task t in output)
                {
                    if (_headings.ContainsKey(t.Project.Name))
                    {
                        List<Task> tt = _headings[t.Project.Name];
                        tt.Add(t);
                        _headings[t.Project.Name] = tt;
                    }
                    else
                    {
                        List<Task> tt = new List<Task>();
                        tt.Add(t);
                        _headings[t.Project.Name] = tt;
                    }

                }

                //var listAdapter = new TaskAdapter(_mActivity, Android.Resource.Layout.SimpleListItem1, output.ToArray());
                var listAdapter = new HomeListAdapter(Activity, _headings);
                expandableList.SetAdapter(listAdapter);
                //ListAdapter = listAdapter;
                int count = listAdapter.GroupCount;
                expandableList.SetOnGroupCollapseListener(this);
                expandableList.SetOnChildClickListener(this);
                for (int i = 0; i < count; i++)
                {
                    expandableList.ExpandGroup(i);
                }

                var refresher = v.FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);

                refresher.Refresh += async delegate
                {
                    try
                    {
                        output = await ctrl.GetRecentTasks(AccountStorage.DataSet);

                        recent = output[0];
                        _headings = new Dictionary<string, List<Task>>();
                        recentTask.Text = recent.FullName;
                        recentProject.Text = recent.Project.Name;
                        _headings.Add(recent.Project.Name, new List<Task>());
                        output.RemoveAt(0);
                        foreach (Task t in output)
                        {
                            if (_headings.ContainsKey(t.Project.Name))
                            {
                                List<Task> tt = _headings[t.Project.Name];
                                tt.Add(t);
                                _headings[t.Project.Name] = tt;
                            }
                            else
                            {
                                List<Task> tt = new List<Task>();
                                tt.Add(t);
                                _headings[t.Project.Name] = tt;
                            }

                        }
                        listAdapter = new HomeListAdapter(Activity, _headings);
                        expandableList.SetAdapter(listAdapter);
                        count = listAdapter.GroupCount;
                        expandableList.SetOnGroupCollapseListener(this);
                        expandableList.SetOnChildClickListener(this);
                        for (int i = 0; i < count; i++)
                        {
                            expandableList.ExpandGroup(i);
                        }
                        refresher.Refreshing = false;
                    }
                    catch (CannotReachServerException)
                    {
                        AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                        builder.SetTitle("Unable to Connect")
                            .SetMessage("Please check your network connection and try again")
                              .SetNeutralButton("Okay", (sender, args) =>
                              {
                                  builder.Dispose();
                              })
                            .SetCancelable(false);
                        AlertDialog alert = builder.Create();
                        refresher.Refreshing = false;
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
                            .SetNeutralButton("Okay", (sender, args) =>
                            {
                                builder.Dispose();
                            })
                            .SetCancelable(false);
                        AlertDialog alert = builder.Create();
                        refresher.Refreshing = false;
                        alert.Show();


                    }
                    catch (Exception e)
                    {
                        // For any other weird exceptions
                        AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                        builder.SetTitle("An Error has occured")
                              .SetNeutralButton("Okay", (sender, args) =>
                              {
                                  builder.Dispose();
                              })
                            .SetMessage("Error :" + e.Message)
                            .SetCancelable(false);
                        AlertDialog alert = builder.Create();
                        refresher.Refreshing = false;
                        alert.Show();

                    }

                };

                recentProject.Click += (sender, args) =>
                {
                    ((MainActivity)Activity).ListOfProjectsCallback(recent.Project.Id, recent.Project.Name);
                };

                recentTask.Click += (sender, args) =>
                {
                    ((MainActivity)Activity).PassTaskDetailsInfo(recent.Id, recent.FullName, recent.Project.Name,
                        recent.CompletionDate,
                        recent.EstimatedTime, recent.ActualTime);
                };
            }
            catch (CannotReachServerException)
            {
                Debug.WriteLine("Cannot reach Server");
                pb.Dismiss();
                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetTitle("Unable to Connect")
                    .SetMessage("Please check your network connection and try again")
                      .SetNeutralButton("Okay", (sender, args) =>
                      {
                          builder.Dispose();
                      })
                    .SetCancelable(false);
                AlertDialog alert = builder.Create();
                alert.Show();


            }
           
            catch (Refit.ApiException we)
            {
                Debug.WriteLine("Web exception :"+we.StatusCode);
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
                Debug.WriteLine("Status not okay");
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
                Debug.WriteLine("General exception :"+e.GetType());
                // For any other weird exceptions
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
            /*
                private void LoadDummyData(View v)
                {

                    ListView lv = v.FindViewById<ListView>(Android.Resource.Id.List);
                    var items = new[] { "Vegetables", "Fruits", "Flower Buds", "Legumes", "Bulbs", "Tubers" };
                    ArrayAdapter listAdapter = new ArrayAdapter<String>(_mActivity, Android.Resource.Layout.SimpleListItem1, items);
                    ListAdapter = listAdapter;

                    TextView recentTask = v.FindViewById<TextView>(Resource.Id.Home_RecentTask);
                    recentTask.Text = "Project / Mobile App l1 / Iteration 1 / View Skeletons / Create Android Skeletons / Home Screen ";

                }
        */
        }

        public void ModifyPlayPauseState(bool isPlaying)
        {
            if (isPlaying)
            {
                play.SetBackgroundResource(Resource.Drawable.play_activated);
                pause.SetBackgroundResource(Resource.Drawable.pause_deactivated);
            }
            else
            {
                play.SetBackgroundResource(Resource.Drawable.play_deactivated);
                pause.SetBackgroundResource(Resource.Drawable.pause_activated);
            }
        }


        [BroadcastReceiver(Enabled = true)]
        [IntentFilter(new[] { "processdashboard.timelogger" })]
        public class myBroadCastReceiver : BroadcastReceiver
        {

            private MainActivity home;

            public myBroadCastReceiver()
            {

            }

            public myBroadCastReceiver(MainActivity home)
            {
                this.home = home;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string value = intent.GetStringExtra("key");
                if (value == null)
                    Debug.WriteLine("Value is null");
                else
                {
                    Debug.WriteLine(value);
                    if (value.Equals("Time logging has been started by the server"))
                    {
                        home.ModifyPlayPauseState(true);
                    }
                    else if (value.Equals("Time logging has been cancelled by the server"))
                    {
                        home.ModifyPlayPauseState(false);
                    } else if (value.Equals("Time logging has been stopped"))
                    {
                        home.ModifyPlayPauseState(false);
                    }
                }


            }
        }

        public class HomeListAdapter : BaseExpandableListAdapter
        {
            private Activity _activity;
            private Dictionary<string, List<Task>> _dictGroup;
            private List<string> _lstGroupId;

            public HomeListAdapter(Activity activity, Dictionary<string, List<Task>> dictGroup)
            {
                _dictGroup = dictGroup;
                _activity = activity;
                _lstGroupId = dictGroup.Keys.ToList();
            }
            #region implemented abstract members of BaseExpandableListAdapter
            public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
            {
                var myObj = _dictGroup[_lstGroupId[groupPosition]][childPosition];
                return new JavaObjectWrapper<Task> { Obj = myObj };
            }
            public override long GetChildId(int groupPosition, int childPosition)
            {
                return childPosition;
            }
            public override int GetChildrenCount(int groupPosition)
            {
                return _dictGroup[_lstGroupId[groupPosition]].Count;
            }
            public override View GetChildView(int groupPosition,int childPosition,
                bool isLastChild,View convertView,
                ViewGroup parent)
            {
                var item = _dictGroup[_lstGroupId[groupPosition]][childPosition];

                if (convertView == null)
                    convertView = _activity.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);

                var tv = convertView.FindViewById<TextView>(Android.Resource.Id.Text1);
                var text = item.FullName;
                var spannable = new SpannableString(text);
                spannable.SetSpan(new LeadingMarginSpanStandard(0, 15), 0, text.Length, 0);

                if (item.CompletionDate.HasValue && !item.CompletionDate.Value.Equals(DateTime.MinValue))
                {
                    // Strike out the text
                    spannable.SetSpan(new StrikethroughSpan(), 0, text.Length, SpanTypes.InclusiveExclusive);

                }
                tv.TextFormatted = spannable;

                return convertView;
            }

            public override Java.Lang.Object GetGroup(int groupPosition)
            {
                return _lstGroupId[groupPosition];
            }
            public override long GetGroupId(int groupPosition)
            {
                return groupPosition;
            }
            public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
            {
                var item = _lstGroupId[groupPosition];

                if (convertView == null)
                    convertView = _activity.LayoutInflater.Inflate(Resource.Layout.ListControl_Group, null);

                var textBox = convertView.FindViewById<TextView>(Resource.Id.txtLarge);
                textBox.SetText(item, TextView.BufferType.Normal);

                return convertView;
            }
            public override bool IsChildSelectable(int groupPosition, int childPosition)
            {
                return true;
            }
            public override int GroupCount => _dictGroup.Count;
            public override bool HasStableIds => true;
            #endregion
        }

        public class JavaObjectWrapper<T> : Java.Lang.Object
        {
            public T Obj { get; set; }
        }

        public void OnGroupCollapse(int groupPosition)
        {
            // throw new NotImplementedException();
            expandableList.ExpandGroup(groupPosition);

        }
        public bool OnChildClick(ExpandableListView parent, View clickedView, int groupPosition, int childPosition, long id)
        {
            var itmGroup = (String)parent.ExpandableListAdapter.GetGroup(groupPosition);
            var itmChild = _headings[itmGroup][childPosition];
            _mActivity.PassTaskDetailsInfo(itmChild.Id, null, null, null, null, null);
            return true;

        }
    }
}