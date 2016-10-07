#region
using System;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using ProcessDashboard.Droid.Adapter;
using ProcessDashboard.SyncLogic;
using Debug = System.Diagnostics.Debug;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    public class ListOfProjects : ListFragment
    {
        private Scrollinput _si;
       public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            // Create your fragment here
            ((MainActivity)Activity).SetTitle("List of Projects");
            // Create your fragment here
        }

        public override void OnResume()
        {
            base.OnResume();
            ((MainActivity)Activity).SetTitle("List of projects");
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            ViewGroup listContainer = (ViewGroup)base.OnCreateView(inflater, container, savedInstanceState);

            SwipeRefreshLayout srl = new SwipeRefreshLayout(inflater.Context);
           // srl.AddView(listContainer, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            srl.LayoutParameters = (
                new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent));
            _si = new Scrollinput(srl);
            srl.Refresh += async delegate
            {

                await GetData(ListView, ((MainActivity)Activity).Ctrl);
                srl.Refreshing = false;

            };

            //return srl;
            return listContainer;
        }


        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            var listView = ListView;
            listView.SetOnScrollListener(_si);
            
            listView.ItemClick += (sender, e) =>
            {
                // var p = e.Parent.SelectedItem.Class;
                var p = (ProjectsAdapter)listView.Adapter;
                Debug.WriteLine("Type:" + p.GetProject(e.Position));
                var projectId = p.GetProject(e.Position).Id;
                var projectName = p.GetProject(e.Position).Name;
                //  ((MainActivity)this.Activity).switchToFragment(MainActivity.fragmentTypes.listoftasks);
                ((MainActivity)Activity).ListOfProjectsCallback(projectId, projectName);
            };
            Debug.WriteLine("We are now assigning values to the list view");
#pragma warning disable 4014
            GetData(listView, ((MainActivity)Activity).Ctrl);
#pragma warning restore 4014


        }

        private async Task GetData(ListView listView, Controller ctrl)
        {
          try
            {
                var output = await ctrl.GetProjects(AccountStorage.DataSet);
                Debug.WriteLine("We are here");
                var listAdapter = new ProjectsAdapter(Activity, Android.Resource.Layout.SimpleListItem1,
                    output.ToArray());
                Debug.WriteLine("Setting the adapter");
                listView.Adapter = listAdapter;
                Debug.WriteLine("List will be displayed");
                SetListShown(true);
                ListView.SetSelection(listAdapter.Count - 1);
            }
            catch (CannotReachServerException)
            {
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
        }

        // ReSharper disable once UnusedMember.Local
        private void LoadDummyData(ListView listView)
        {
            string[] items = { "Sample Project", "Linux Kernel", "Windows 11 Ultimate", "Mobile Process Dashboard" };
            ArrayAdapter listAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItem1, items);
            listView.Adapter = listAdapter;
        }


        public class Scrollinput : AbsListView.IOnScrollListener
        {
            private SwipeRefreshLayout srl;
            public Scrollinput(SwipeRefreshLayout srl)
            {
                this.srl = srl;
            }

            public void Dispose()
            {
                // throw new NotImplementedException();
            }
            // ReSharper disable once UnassignedGetOnlyAutoProperty
            public IntPtr Handle { get; }
            public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
            {
                Debug.WriteLine("We are in on scroll");
                int topRowVerticalPosition = (view == null || view.ChildCount == 0) ? 0 : view.GetChildAt(0).Top;
                Debug.WriteLine("Top Row " + topRowVerticalPosition);
                srl.Enabled = (firstVisibleItem == 0 && topRowVerticalPosition >= 0);
                Debug.WriteLine("Enabled : " + srl.Enabled);

            }
            public void OnScrollStateChanged(AbsListView view, ScrollState scrollState)
            {


            }
        }
        // ReSharper disable once UnusedMember.Local
        private class ListFragmentSwipeRefreshLayout : SwipeRefreshLayout
        {
            private Context con;


            public ListFragmentSwipeRefreshLayout(Context context) : base(context)
            {
                con = context;
            }

            /**
         * As mentioned above, we need to override this method to properly signal when a
         * 'swipe-to-refresh' is possible.
         *
         * @return true if the {@link android.widget.ListView} is visible and can scroll up.
         */

            public override bool CanChildScrollUp()
            {
                ListView listview = new ListView(con);

                if (listview.Visibility == ViewStates.Visible)
                {
                    return CanListViewScrollUp(listview);
                }
                return false;
            }

            /**
     * Utility method to check whether a {@link ListView} can scroll up from it's current position.
     * Handles platform version differences, providing backwards compatible functionality where
     * needed.
     */
            private static bool CanListViewScrollUp(ListView listView)
            {

                return ViewCompat.CanScrollVertically(listView, -1);

            }

        }
    }
}