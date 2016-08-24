﻿#region
using System;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using ProcessDashboard.Droid.Adapter;
using ProcessDashboard.SyncLogic;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    public class ListProjectTasks : ListFragment
    {
        private string _projectId;
        private string _projectName = "";
        private ListOfProjects.Scrollinput _si;
        private TextView empty;

        public ListProjectTasks(string projectId)
        {
            _projectId = projectId;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            // Create your fragment here
            ((MainActivity) Activity).SetTitle("List of Tasks");
            // Create your fragment here
        }

        public override void OnResume()
        {
            base.OnResume();
            ((MainActivity) Activity).SetTitle("List of Tasks");
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            SetId(_projectId,_projectName);
          
        }

        public void SetId(string projectId,string projectName)
        {
            _projectId = projectId;
            _projectName = projectName;
        }

        private async Task AddData(Controller ctrl, string projectId)
        {
            try
            {

                var output = await ctrl.GetTasks(Settings.GetInstance().Dataset, projectId);

                System.Diagnostics.Debug.WriteLine(output.Count);

                var listAdapter = new TaskAdapter(Activity, Android.Resource.Layout.SimpleListItem1, output.ToArray());
                ListView.Adapter = listAdapter;
                
                SetListShown(true);
                ListView.SetSelection(listAdapter.Count - 1);
                //ListView.SmoothScrollByOffset(listAdapter.Count -1);
               // ListView.SmoothScrollToPosition(listAdapter.Count - 1);
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var listView = ListView;
            listView.SetOnScrollListener(_si);
            listView.ItemClick += (sender, e) =>
            {
                var ta = (TaskAdapter)listView.Adapter;
                var p = ta.GetTask(e.Position);
                var taskId = p.Id;
                //  ((MainActivity)this.Activity).switchToFragment(MainActivity.fragmentTypes.taskdetails);
                ((MainActivity)Activity).PassTaskDetailsInfo(taskId, p.FullName, p.Project.Name, p.CompletionDate,
                    p.EstimatedTime, p.ActualTime);
                //Project p = listView.GetItemAtPosition(e.Position);
            };

            //listView.EmptyView = empty;
            ListView.EmptyView = empty;
            #pragma warning disable 4014
            AddData(((MainActivity)Activity).Ctrl, _projectId);
#pragma warning restore 4014

         
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            ViewGroup listContainer = (ViewGroup)base.OnCreateView(inflater, container, savedInstanceState);
           
            TextView proj = new TextView(inflater.Context);
            proj.Gravity = GravityFlags.Center;

            ComplexUnitType ct = ComplexUnitType.Dip;

            proj.SetTextSize(ct,20);
            proj.Text = _projectName;
            
            SwipeRefreshLayout srl = new SwipeRefreshLayout(inflater.Context);
            //srl.AddView(listContainer, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            srl.LayoutParameters = (
                new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent));


            LinearLayout ll = new LinearLayout(inflater.Context);
            ll.Orientation = Orientation.Vertical;

            LinearLayout.LayoutParams lp2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            lp2.Gravity= GravityFlags.Center;
            proj.LayoutParameters = lp2;


            View ruler = new View(inflater.Context);
            ruler.SetBackgroundColor(Color.Gray);

            ll.AddView(proj);
            ll.AddView(ruler,
             new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, 2));
            ll.AddView(listContainer);
            //ll.AddView(srl);


            empty = new TextView(this.Activity)
            {
                Text = "No Content Found",
                Visibility = ViewStates.Gone,
                LayoutParameters = (
                    new ViewGroup.LayoutParams(
                        ViewGroup.LayoutParams.WrapContent,
                        ViewGroup.LayoutParams.WrapContent))
            };

            empty.SetTextSize(ct, 25);

            ll.AddView(empty);

            _si = new ListOfProjects.Scrollinput(srl);
            
            srl.Refresh += async delegate
                  {
                      try
                      {
                          await AddData(((MainActivity) Activity).Ctrl, _projectId);
                          srl.Refreshing = false;
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
                          alert.Show();


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

                  };
            return ll;
              } 

      

        /*
        public void LoadDummyData()
        {
            string[] values = {"Sample Task", "Component 1 / Component 2 / Code", "... / head truncation"};
            ListAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleExpandableListItem1, values);
        }
        */

       
    }

  

}