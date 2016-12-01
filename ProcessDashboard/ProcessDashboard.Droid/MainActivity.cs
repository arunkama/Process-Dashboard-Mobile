#region
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using HockeyApp.Android;
using ProcessDashboard.Droid.Fragments;
using ProcessDashboard.Droid.Interfaces;
using ProcessDashboard.DTO;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
#endregion
namespace ProcessDashboard.Droid
{
    [Activity(Label = "Process Dashboard Companion", MainLauncher = true, Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AppCompatActivity, IListOfProjectsInterface, IListOfTaksInterface, ITimeLogsInterface,
        ITimeLogEditInterface
        //,IOnBackStackChangedListener
    {
        public enum FragmentTypes
        {
            Login,
            Home,
            Settings,
            Listofprojects,
            Listoftasks,
            Taskdetails,
            Tasktimelogdetails,
            Globaltimelog,
            Globaltimelogdetails
        }

        private static bool _CrashHandlerRegistered = false;

        public bool isBound = false; 
    	public bool isConfigurationChange = false;

        public TimerService.TimerServiceBinder binder;
        public TimerServiceConnection timerServiceConnection;


        
            
        private Fragment _currentFragment;
        private DrawerLayout _drawerLayout;
        private GlobalTimeLogList _globalTimeLogFragment;
        private Home _homeFragment;
        private ListOfProjects _listOfProjectFragment;
        private ListProjectTasks _listOfTasksFragment;
        private Login _loginFragment;
        private SettingsPage _settingsFragment;
        private TaskDetails _taskDetailFragment;
        private TaskTimeLogList _taskTimeLogDetailFragment;
        private ActionBarDrawerToggle drawerToggle;
        private TestFragment _testFragment;
        private TimeLogDetail _timeLogDetailFragment;
        private Toolbar _toolbar;

        private string APP_ID = "168ed05dd48a4d32b8bbcc25fec3d3a9";

        public Controller Ctrl;

        public void ListOfProjectsCallback(string projectid,string projectName)
        {
            _listOfTasksFragment.SetId(projectid,projectName);
            SwitchToFragment(FragmentTypes.Listoftasks);
        }

        public void PassTaskDetailsInfo(string id, string taskName, string projectName, DateTime? completionDate,
            double? estimatedTime, double? actualTime)
        {
            _taskDetailFragment.SetId(id, taskName, projectName, completionDate, estimatedTime, actualTime);
            SwitchToFragment(FragmentTypes.Taskdetails);
        }

        public void TimeLogEditCallBack(string projectname, string taskName, string taskId, TimeLogEntry timelogId)
        {
            //  throw new NotImplementedException();
            _timeLogDetailFragment = new TimeLogDetail();
            _timeLogDetailFragment.SetData(projectname, taskName, taskId, timelogId);
            SwitchToFragment(FragmentTypes.Globaltimelogdetails);
        }

        public void PassTimeLogInfo(string timelogId, string projectName, string taskName)
        {
            _taskTimeLogDetailFragment.SetData(timelogId, projectName, taskName);
            SwitchToFragment(FragmentTypes.Tasktimelogdetails);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            AccountStorage.SetContext(this);
            //ase.Set("testing","testing","testing","mock");
            //System.Diagnostics.Debug.WriteLine(ase.UserId);
            
            //System.Diagnostics.Debug.WriteLine("User id: "+ase.UserId);

            // Create UI
            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            // Init toolbar
            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            _toolbar.Title = Resources.GetString(Resource.String.app_name);
            SetSupportActionBar(_toolbar);
            //_toolbar.

            // Attach item selected handler to navigation view
            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetCheckedItem(0);
            navigationView.Menu.GetItem(0).SetChecked(true);

            navigationView.NavigationItemSelected += NavigationView_NavigationItemSelected;

            // Create ActionBarDrawerToggle button and add it to the toolbar
            drawerToggle = new ActionBarDrawerToggle(this, _drawerLayout, _toolbar, Resource.String.open_drawer,
                Resource.String.close_drawer);
            _drawerLayout.SetDrawerListener(drawerToggle);

            //SetDrawerState(true);
            //  drawerToggle.SetHomeAsUpIndicator(null);
            //drawerToggle.SyncState();

            _loginFragment = new Login();
            _homeFragment = new Home();
            _settingsFragment = new SettingsPage();
            _globalTimeLogFragment = new GlobalTimeLogList();
            _timeLogDetailFragment = new TimeLogDetail();
            _listOfProjectFragment = new ListOfProjects();
            _taskDetailFragment = new TaskDetails();
            _taskTimeLogDetailFragment = new TaskTimeLogList();
            _listOfTasksFragment = new ListProjectTasks("");
            _testFragment = new TestFragment();
       try{
                if (AccountStorage.UserId != null)
                {
                    System.Diagnostics.Debug.WriteLine("User id is not null");
                    SetDrawerState(true);
                    _currentFragment = _homeFragment;

                    //for testing
                    //_currentFragment = _testFragment;
                    // if logged in
                    System.Diagnostics.Debug.WriteLine("Moving Forward");
                    // else 
                    //CurrentFragment = ListOfProjectFragment;

                    var fragmentTx = FragmentManager.BeginTransaction();
                    // The fragment will have the ID of Resource.Id.fragment_container.
                    fragmentTx.Replace(Resource.Id.fragmentContainer, _currentFragment);
                    // Commit the transaction.
                    fragmentTx.Commit();

                    var apiService = new ApiTypes();
                    var service = new PDashServices(apiService);
                    Ctrl = new Controller(service);

                    // ...
                    CheckForCrashes();
                    //  checkForUpdates();

                    // FragmentManager.AddOnBackStackChangedListener(this);
                    // shouldDisplayHomeUp();


                }
                else
                {
                    //SetDrawerState(false);
                    //_currentFragment = _loginFragment;
                    System.Diagnostics.Debug.WriteLine("Goto Login");
                    StartActivity(typeof(LoginActivity));
                    Finish();


                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("We had an exception");
                StartActivity(typeof(LoginActivity));
                Finish();
            }
         
         
        }

        public void SetTitle(string title)
        {
            try
            {
                _toolbar.Title = title;
            }
            catch (Exception e)
            {
                
            }
        }

        private void NavigationView_NavigationItemSelected(object sender,
            NavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.MenuItem.ItemId)
            {
                case Resource.Id.nav_home:
                    // React on 'Home' selection
                    ShowFragment(_homeFragment);

                    break;
                case Resource.Id.nav_projects:
                    // React on 'Projects' selection
                    ShowFragment(_listOfProjectFragment);
                   //ShowFragment(_listO)
                    break;
                case Resource.Id.nav_timelogs:
                    // React on 'Time Logs' selection
                    ShowFragment(_globalTimeLogFragment);
                    break;
                case Resource.Id.nav_settings:
                    // React on 'Settings' selection
                    ShowFragment(_settingsFragment);
                    break;
            }

            // Close drawer
            _drawerLayout.CloseDrawers();
        }

        private void ShowFragment(Fragment fragment)
        {
            if (fragment.IsVisible)
            {
                return;
            }
            var fragmentTx = FragmentManager.BeginTransaction();
            // The fragment will have the ID of Resource.Id.fragment_container.
            fragmentTx.Replace(Resource.Id.fragmentContainer, fragment);
            // Commit the transaction.
            fragmentTx.AddToBackStack("hello" + new Random().Next());

            fragmentTx.Commit();
        }

        public void logoutUser()
        {
            AccountStorage.ClearStorage();
            _loginFragment.token.Text = "";
            _loginFragment.username.Text = "";
            _loginFragment.password.Text = "";

        }
        public override void OnBackPressed()
        {
            if (FragmentManager.BackStackEntryCount > 0)
            {
                FragmentManager.PopBackStack();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public void ModifyPlayPauseState(bool isPlaying)
        {


            if (_homeFragment.IsVisible)
            {
                _homeFragment.ModifyPlayPauseState(isPlaying);
            }
            else if (_taskDetailFragment.IsVisible)
            {
                _taskDetailFragment.ModifyPlayPauseState(isPlaying);
            }



        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (isBound)
            {
                UnbindService(timerServiceConnection);
                isBound = false;
            }
            unregisterManagers();
        }
        public void SwitchToFragment(FragmentTypes fragmentType)
        {
            switch (fragmentType)
            {
                case FragmentTypes.Home:
                    ShowFragment(_homeFragment);

                    break;
                case FragmentTypes.Login:
                    ShowFragment(_loginFragment);
                    break;
                case FragmentTypes.Settings:
                    ShowFragment(_settingsFragment);
                    break;
                case FragmentTypes.Listoftasks:
                    ShowFragment(_listOfTasksFragment);

                    break;
                case FragmentTypes.Globaltimelog:
                    ShowFragment(_globalTimeLogFragment);
                    break;
                case FragmentTypes.Globaltimelogdetails:
                    ShowFragment(_timeLogDetailFragment);
                    break;
                case FragmentTypes.Listofprojects:
                    ShowFragment(_listOfProjectFragment);

                    break;
                case FragmentTypes.Taskdetails:
                    ShowFragment(_taskDetailFragment);
                    break;
                case FragmentTypes.Tasktimelogdetails:
                    ShowFragment(_taskTimeLogDetailFragment);
                    break;
            }
        }


        public void SetDrawerState(bool isEnabled)
        {
            try
            {
                if (isEnabled)
                {
                    _drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
                    drawerToggle.OnDrawerStateChanged(DrawerLayout.LockModeUnlocked);
                    drawerToggle.DrawerIndicatorEnabled = (true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    drawerToggle.SyncState();

                }
                else
                {
                    _drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
                    drawerToggle.OnDrawerStateChanged(DrawerLayout.LockModeUnlocked);
                    drawerToggle.DrawerIndicatorEnabled = (false);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                    SupportActionBar.SetHomeButtonEnabled(false);

                    drawerToggle.SyncState();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Set Drawer State : "+e.Message);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            CheckForCrashes();
            if (!_CrashHandlerRegistered)
            {
                /*
                _CrashHandlerRegistered = true;
                CrashManager.Register(this, PlatformUtils.HockeyApplicationId);
                //copy build properties into c# land so that the handler won't crash accessing java
                TraceWriter.InitializeConstants();
                //TraceWriter.Initialize(listener); // instead of InitializeConstants: if you want things like user/contact/description

                AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
                {
                    TraceWriter.WriteTrace(args.Exception);
                    args.Handled = true;
                };
                AppDomain.CurrentDomain.UnhandledException +=
                    (sender, args) => TraceWriter.WriteTrace(args.ExceptionObject);
                TaskScheduler.UnobservedTaskException += (sender, args) => TraceWriter.WriteTrace(args.Exception);
                ExceptionSupport.UncaughtTaskExceptionHandler = TraceWriter.WriteTrace;
                */
            }
        }

        /*
	    public void OnBackStackChanged()
	    {
	        shouldDisplayHomeUp();
	    }

        public  void shouldDisplayHomeUp()
        {
            //Enable Up button only  if there are entries in the back stack
            bool canback = FragmentManager.BackStackEntryCount > 0;
            this.SupportActionBar?.SetDisplayHomeAsUpEnabled(canback);
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
        public override bool OnSupportNavigateUp()
        {
            //This method is called when the up button is pressed. Just the pop back stack.
            FragmentManager.PopBackStack();
            return true;
        }
        */

     //HockeyApp

        private void CheckForCrashes()
        {
            CrashManager.Register(this, APP_ID);
        }

        // ReSharper disable once UnusedMember.Local
        private void CheckForUpdates()
        {
            // Remove this for store builds!
            UpdateManager.Register(this, APP_ID);
        }

        private void unregisterManagers()
        {
            UpdateManager.Unregister();

            // unregister other managers if necessary...
        }

       
    }


    public class TimerServiceConnection : Java.Lang.Object, IServiceConnection
    {
        MainActivity activity;
        private TimerService.TimerServiceBinder binder;

        public TimerServiceConnection(MainActivity activity)
        {
            this.activity = activity;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var timerServiceBinder = service as TimerService.TimerServiceBinder;

            if (timerServiceBinder != null)
            {
                activity.binder = timerServiceBinder;
                activity.isBound = true;
                binder = (TimerService.TimerServiceBinder) service;
            }

        }

        public void OnServiceDisconnected(ComponentName name)
        {
           
            activity.isBound = false;
        }
    }

}