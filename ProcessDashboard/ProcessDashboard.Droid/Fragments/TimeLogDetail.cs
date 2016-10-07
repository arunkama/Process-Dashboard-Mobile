#region
using System;
using System.Globalization;
using System.Net;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ProcessDashboard.DTO;
using Debug = System.Diagnostics.Debug;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    public class TimeLogDetail : Fragment
    {
        private EditText _interruptTime, _comment, _startTime, _deltaTime;
        private string _projectName;
        private string _taskId;
        private string _taskName;
        private TimeLogEntry _timeLog;

        private double _oldLoggedTime;
        private double _oldInterruptTime;

        private TextView _tn, _pn;

        public void SetData(string projectName, string taskName, string taskId, TimeLogEntry timeLog)
        {
            _projectName = projectName;
            _taskName = taskName;
            _timeLog = timeLog;
            _taskId = taskId;

            Debug.WriteLine("PN :" + _projectName);
            Debug.WriteLine("TN :" + _taskName);

            Debug.WriteLine("Time log ; " + _timeLog);
            Debug.WriteLine("Task ID :" + _taskId);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Debug.WriteLine("Time Log Detail : " + "We are in On create");
            //  RetainInstance = true;
            SetHasOptionsMenu(true);
            // Create your fragment here
            ((MainActivity)Activity).SetTitle("Time Log Detail");
            // Create your fragment here
        }

        public override void OnResume()
        {
            base.OnResume();
            Debug.WriteLine("Time Log Detail : " + "We are in On Resume");
            ((MainActivity)Activity).SetTitle("Time Log Detail ");
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);
            inflater.Inflate(Resource.Menu.TimelogMenu, menu);
          //  menu.FindItem(Resource.Id.TimeLog_Delete).Icon = Android.Resource.Drawable.IcMenuSave;

            if (_timeLog == null)
            {
                menu.FindItem(Resource.Id.TimeLog_Delete).SetVisible(false);
            }
            else
            {
                menu.FindItem(Resource.Id.TimeLog_Save).SetVisible(false);
            }
            
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
          
            switch (item.ItemId)
            {
                case Resource.Id.TimeLog_Save:
                    ProcessEntries();
                    return true;
                case Resource.Id.TimeLog_Delete:
                    AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                    builder.SetTitle("Delete")
                          .SetPositiveButton("Okay", (sender, args) =>
                          {
                              DeleteCurrentEntry();
                              builder.Dispose();
                          })
                         .SetNegativeButton("Cancel",(sender,args)=> { builder.Dispose(); })
                        .SetMessage("Are you sure you want to delete this entry?")
                        .SetCancelable(false);
                    AlertDialog alert = builder.Create();
                    alert.Show();
                    
                    return true;
                default:
                    return false;
            }
        }

        public async void DeleteCurrentEntry()
        {
            ProgressDialog pd = new ProgressDialog(Activity);
            pd.Indeterminate = true;
            pd.SetCancelable(false);
            pd.SetMessage("Deleting");
            try
            {
              
                await ((MainActivity)Activity).Ctrl.DeleteTimeLog(AccountStorage.DataSet, "" + _timeLog.Id);
                pd.Dismiss();
                Toast.MakeText(Activity, "Deleted the timelog entry", ToastLength.Short).Show();
                Debug.WriteLine("We are going to pop backstack");
                ((MainActivity)Activity).FragmentManager.PopBackStack();
            }
            catch (CannotReachServerException)
            {
                if (pd.IsShowing)
                    pd.Dismiss();
                Toast.MakeText(Activity, "Please check your internet connection and try again.", ToastLength.Long).Show();
                Debug.WriteLine("We are going to pop backstack");
                ((MainActivity)Activity).FragmentManager.PopBackStack();
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

                                if (pd.IsShowing)
                                    pd.Dismiss();
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
            catch (StatusNotOkayException)
            {
                if (pd.IsShowing)
                    pd.Dismiss();
                Toast.MakeText(Activity, "An error occured. Please try again.", ToastLength.Short).Show();
                Debug.WriteLine("We are going to pop backstack");
                ((MainActivity)Activity).FragmentManager.PopBackStack();
            }
            catch (Exception)
            {
                if (pd.IsShowing)
                    pd.Dismiss();
                // For any other weird exceptions
                Toast.MakeText(Activity, "Invalid operation. Please try again.", ToastLength.Short).Show();
                Debug.WriteLine("We are going to pop backstack");
                ((MainActivity)Activity).FragmentManager.PopBackStack();
            }
        }

        public async void ProcessEntries()
        {
            ProgressDialog pd = new ProgressDialog(Activity);
            pd.SetMessage("Saving..");
            pd.Indeterminate = true;

            if (_timeLog == null)
            {
                Debug.WriteLine("This is a new entry");
                try
                {
                    var newDate = DateTime.SpecifyKind(DateTime.Parse(_startTime.Text), DateTimeKind.Local);
                    
                    double val =Convert.ToDouble(TimeSpan.ParseExact(_deltaTime.Text, @"hh\:mm", CultureInfo.InvariantCulture).TotalMinutes);
                    double val2 =Convert.ToDouble(TimeSpan.ParseExact(_interruptTime.Text, @"hh\:mm", CultureInfo.InvariantCulture).TotalMinutes);
                    pd.Show();
                    await
                        ((MainActivity)Activity).Ctrl.AddATimeLog(AccountStorage.DataSet, _comment.Text,
                            newDate, _taskId, val, val2, false);
                    pd.Dismiss();
                    Toast.MakeText(Activity, "Added new Timelog Entry", ToastLength.Short).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
                }
                catch (CannotReachServerException)
                {
                    if (pd.IsShowing)
                        pd.Dismiss();
                    Toast.MakeText(Activity, "Please check your internet connection and try again.", ToastLength.Long).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
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

                                    if (pd.IsShowing)
                                        pd.Dismiss();
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
                catch (StatusNotOkayException)
                {
                    if (pd.IsShowing)
                        pd.Dismiss();
                    Toast.MakeText(Activity, "An error occured. Please try again.", ToastLength.Short).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
                }
                catch (Exception ea)
                {
                    if (pd.IsShowing)
                        pd.Dismiss();
                    // For any other weird exceptions
                    Toast.MakeText(Activity, "Invalid values. Please try again."+ea.Message, ToastLength.Short).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
                }

            }
            else
            {
                Debug.WriteLine("This is an update operation");
                try
                {

                    var newDate = DateTime.SpecifyKind(DateTime.Parse(_startTime.Text), DateTimeKind.Local);
                    double val = Convert.ToDouble(TimeSpan.ParseExact(_deltaTime.Text, @"hh\:mm", CultureInfo.InvariantCulture).TotalMinutes);
                    var updatedLoggedTime = val - _oldLoggedTime;
                    val = Convert.ToDouble(TimeSpan.ParseExact(_interruptTime.Text, @"hh\:mm", CultureInfo.InvariantCulture).TotalMinutes);
                    var updatedInterruptTime = val - _oldInterruptTime;

                    pd.Show();
                    await
                        ((MainActivity)Activity).Ctrl.UpdateTimeLog(AccountStorage.DataSet,
                            "" + _timeLog.Id,
                            _comment.Text,
                            newDate, _taskId, updatedLoggedTime, updatedInterruptTime, false);


                    pd.Dismiss();
                    Toast.MakeText(Activity, "Updated the Timelog Entry", ToastLength.Short).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
                }
                catch (CannotReachServerException)
                {
                    if(pd.IsShowing)
                        pd.Dismiss();
                    Toast.MakeText(Activity, "Please check your internet connection and try again.", ToastLength.Long).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
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

                                    if (pd.IsShowing)
                                        pd.Dismiss();
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
                catch (StatusNotOkayException)
                {
                    if (pd.IsShowing)
                        pd.Dismiss();
                    Toast.MakeText(Activity, "An error occured. Please try again.", ToastLength.Short).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
                }
                catch (Exception)
                {
                    if (pd.IsShowing)
                        pd.Dismiss();
                    // For any other weird exceptions
                    Toast.MakeText(Activity, "Invalid values. Please try again.", ToastLength.Short).Show();
                    Debug.WriteLine("We are going to pop backstack");
                    ((MainActivity)Activity).FragmentManager.PopBackStack();
                }
            }

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Debug.WriteLine("Time Log Detail : " + "We are in On create view");

            // Use this to return your custom view for this Fragment
            var v = inflater.Inflate(Resource.Layout.TimeLogEntry, container, false);

            _tn = v.FindViewById<TextView>(Resource.Id.TimeLogEdit_TaskName);
            _pn = v.FindViewById<TextView>(Resource.Id.TimeLogEdit_ProjectName);
            _interruptTime = v.FindViewById<EditText>(Resource.Id.TimeLogEdit_InterruptTime);
            _comment = v.FindViewById<EditText>(Resource.Id.TimeLogEdit_Comment);
            _startTime = v.FindViewById<EditText>(Resource.Id.TimeLogEdit_StartTime);
            _deltaTime = v.FindViewById<EditText>(Resource.Id.TimeLogEdit_DeltaTime);

            _startTime.Focusable = false;
            _deltaTime.Focusable = false;
            _interruptTime.Focusable = false;
            _comment.Focusable = false;

            _startTime.Clickable = true;
            _deltaTime.Clickable = true;
            _interruptTime.Clickable = true;
            _comment.Clickable = true;

            if (_timeLog != null)
            {
                _oldLoggedTime = _timeLog.LoggedTime;
                _oldInterruptTime = _timeLog.InterruptTime;
            }
            else
            {
                _oldLoggedTime = 0;
                _oldInterruptTime = 0;
                _deltaTime.Text = "" + TimeSpan.FromMinutes(0).ToString(@"hh\:mm");
                _interruptTime.Text = "" + TimeSpan.FromMinutes(0).ToString(@"hh\:mm");
                _startTime.Text = DateTime.Now.ToString("g");
            }


            _deltaTime.Click += (sender, args) =>
            {

                LinearLayout ll = new LinearLayout(Activity);
                ll.Orientation = (Orientation.Horizontal);

                NumberPicker aNumberPicker = new NumberPicker(Activity);
                // TODO: Should there be a maximum for the hour column ?
                aNumberPicker.MaxValue = (100);
                aNumberPicker.MinValue = (0);

                double temp = 0;
                if(!_deltaTime.Text.Equals(""))
                temp = Convert.ToDouble(TimeSpan.ParseExact(_deltaTime.Text, @"hh\:mm", CultureInfo.InvariantCulture).TotalMinutes);

                //temp = Double.Parse(_deltaTime.Text);

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

                
                AlertDialog.Builder np = new AlertDialog.Builder(Activity).SetView(ll);

                np.SetTitle("Update Delta Time");
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
                    var updatedLoggedTime = val - _oldLoggedTime;
                    try
                    {
                        if(_timeLog!=null)
                        ((MainActivity)Activity).Ctrl.UpdateTimeLog(AccountStorage.DataSet,"" + _timeLog.Id,null,null, _taskId, updatedLoggedTime, null, false);
                        
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


                    _deltaTime.Text = "" + TimeSpan.FromMinutes(val).ToString(@"hh\:mm");
                    Toast.MakeText(Activity, "Delta Time Updated", ToastLength.Short).Show();
                    np.Dispose();

                });
                np.Show();



            };

            _startTime.Click += (sender, args) =>
            {
                DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                {
                    //TODO: DEBUG: Check whether this is causing any issues.
                    _startTime.Text = time.ToString(CultureInfo.InvariantCulture);
                    Toast.MakeText(Activity, "Start Date Updated", ToastLength.Short).Show();
                    TimePickerFragment frag2 = TimePickerFragment.NewInstance(delegate (int hour, int min)
                    {
                        Toast.MakeText(Activity, "Start Time Updated", ToastLength.Short).Show();

                        DateTime output = new DateTime(time.Year, time.Month, time.Day, hour, min, 0);
                        _startTime.Text = output.ToString("g");

                        try
                        {
                            if (_timeLog != null)
                                ((MainActivity)Activity).Ctrl.UpdateTimeLog(AccountStorage.DataSet, "" + _timeLog.Id, null, output, _taskId, null, null, false);

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


                    });

                    frag2.chosenDate = time;
                    if (_timeLog != null)
                    {
                        frag2.StartHour = _timeLog.StartDate.Hour;
                        frag2.StartMinute = _timeLog.StartDate.Minute;
                    }
                    else
                    {
                        frag2.StartHour = time.Hour;
                        frag2.StartMinute = time.Minute;
                    }

                    frag2.Show(FragmentManager, TimePickerFragment.TAG);

                });
                //frag.StartTime = DateTime.SpecifyKind(DateTime.Parse(""+output[2].value), DateTimeKind.Local);

                frag.StartTime = _timeLog != null ? _timeLog.StartDate : DateTime.Now;

                Debug.WriteLine(frag.StartTime);
                frag.Show(FragmentManager, DatePickerFragment.TAG);

                   
            };

            _interruptTime.Click += (sender, args) =>
            {
                LinearLayout ll = new LinearLayout(Activity);
                ll.Orientation = (Orientation.Horizontal);

                NumberPicker aNumberPicker = new NumberPicker(Activity);
                // TODO: Should there be a maximum for the hour column ?
                aNumberPicker.MaxValue = (100);
                aNumberPicker.MinValue = (0);

                double temp = 0;
                if (!_interruptTime.Text.Equals(""))
                    temp = Convert.ToDouble(TimeSpan.ParseExact(_interruptTime.Text, @"hh\:mm", CultureInfo.InvariantCulture).TotalMinutes);

                //temp = Double.Parse(_deltaTime.Text);

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


                AlertDialog.Builder np = new AlertDialog.Builder(Activity).SetView(ll);

                np.SetTitle("Update Interrupt Time");
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
                    var updatedInterruptTime = val - _oldInterruptTime;
                    if (_timeLog != null)
                    {
                        // This is an update operation
                        try
                        {
                            if (_timeLog != null)
                                ((MainActivity) Activity).Ctrl.UpdateTimeLog(AccountStorage.DataSet, "" + _timeLog.Id, null,
                                null, _taskId, null, updatedInterruptTime, false);
                            
                        }
                        catch (CannotReachServerException)
                        {

                            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                            builder.SetTitle("Unable to Connect")
                                .SetMessage("Please check your network connection and try again")
                                .SetNeutralButton("Okay", (sender2, args2) =>
                                {
                                    builder.Dispose();
                                    ((MainActivity) Activity).FragmentManager.PopBackStack();
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
                                    Console.WriteLine("HTTP Status Code: " + (int) response.StatusCode);
                                    if (response.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                        try
                                        {

                                            Toast.MakeText(Activity, "Username and password error.",
                                                ToastLength.Long).Show();
                                            Debug.WriteLine("We are about to logout");
                                            AccountStorage.ClearStorage();
                                            Debug.WriteLine("Main Activity is :" + Activity == null);
                                            Debug.WriteLine("Items in the backstack :" +
                                                                               Activity.FragmentManager
                                                                                   .BackStackEntryCount);
                                            Debug.WriteLine("Main Activity is :" + Activity == null);
                                            Activity.FragmentManager.PopBackStack(null, PopBackStackFlags.Inclusive);
                                            Debug.WriteLine("Items in the backstack 2 :" +
                                                                               Activity.FragmentManager
                                                                                   .BackStackEntryCount);
                                            ((MainActivity) (Activity)).SetDrawerState(false);
                                            ((MainActivity) (Activity)).SwitchToFragment(
                                                MainActivity.FragmentTypes.Login);
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
                                    Toast.MakeText(Activity, "Unable to load the data. Please restart the application.",
                                        ToastLength.Short).Show();
                                }
                            }
                            else
                            {
                                // no http status code availableToast.MakeText(Activity, "Unable to load the data. Please restart the application.", ToastLength.Short).Show();
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
                    }
                    _interruptTime.Text = "" + TimeSpan.FromMinutes(val).ToString(@"hh\:mm");
                    Toast.MakeText(Activity, "Interrupt Time Updated", ToastLength.Short).Show();
                    np.Dispose();

                });
                np.Show();


            };

            _comment.Click += (sender, args) =>
            {
                LinearLayout ll = new LinearLayout(Activity);
                ll.Orientation = (Orientation.Horizontal);

                EditText et = new EditText(Activity);
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                      ViewGroup.LayoutParams.MatchParent,
                      ViewGroup.LayoutParams.WrapContent);
                et.LayoutParameters = lp;
                //et.LayoutParams = (lp);
                ll.AddView(et);

                et.Focusable=(true);
                

                if(!_comment.Text.Equals("-"))
                et.Text = _comment.Text;

                AlertDialog.Builder np = new AlertDialog.Builder(Activity).SetView(ll);

                np.SetTitle("Update Comments");
                np.SetNegativeButton("Cancel", (s, a) =>
                {
                    np.Dispose();
                });

                np.SetPositiveButton("Ok", (s, a) =>
                {

                    //Update Comments
                    string value = et.Text;
                    if (_timeLog != null)
                    {
                        // This is an update operation
                        try
                        {
                            if (_timeLog != null)
                                ((MainActivity)Activity).Ctrl.UpdateTimeLog(AccountStorage.DataSet, "" + _timeLog.Id, value,
                                null, _taskId, null, null, false);
                            
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

                                            Toast.MakeText(Activity, "Username and password error.",
                                                ToastLength.Long).Show();
                                            AccountStorage.ClearStorage();
                                            Activity.FragmentManager.PopBackStack(null, PopBackStackFlags.Inclusive);
                                            ((MainActivity)(Activity)).SetDrawerState(false);
                                            ((MainActivity)(Activity)).SwitchToFragment(
                                                MainActivity.FragmentTypes.Login);
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
                                    Toast.MakeText(Activity, "Unable to load the data. Please restart the application.",
                                        ToastLength.Short).Show();
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
                    }

                    _comment.Text = value;
                    Toast.MakeText(Activity, "Comment Updated", ToastLength.Short).Show();
                    np.Dispose();

                });
                np.Show();

            };

            if (_taskName != null && _projectName != null)
            {
                _tn.Text = _taskName;
                _pn.Text = _projectName;
            }

            if (_timeLog == null) return v;

            string c = Util.GetInstance().GetLocalTime(_timeLog.StartDate).ToString("g");
            _startTime.Text = (c);
            _interruptTime.Text = "" + TimeSpan.FromMinutes(_timeLog.InterruptTime).ToString(@"hh\:mm");
            _comment.Text = _timeLog.Comment ?? "-";
            _deltaTime.Text = "" + TimeSpan.FromMinutes(_timeLog.LoggedTime).ToString(@"hh\:mm");

            return v;
        }
    }
}