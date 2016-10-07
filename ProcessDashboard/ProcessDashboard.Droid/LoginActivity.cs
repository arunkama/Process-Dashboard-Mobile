using System;
using System.Text;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.Views.InputMethods;
using Plugin.Connectivity;
using Debug = System.Diagnostics.Debug;
namespace ProcessDashboard.Droid
{
    [Activity(Label = "Login")]
    public class LoginActivity : Activity
    {


        private string baseurl;
        private string dataset;

        public TextView token, username, password;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here 

            SetContentView(Resource.Layout.Login);
            Title = "Process Dashboard";

         // Use this to return your custom view for this Fragment
            var lf = this;
            var login = lf.FindViewById<Button>(Resource.Id.login_login);
            token = lf.FindViewById<TextView>(Resource.Id.login_token);
            username = lf.FindViewById<TextView>(Resource.Id.login_username);
            password = lf.FindViewById<TextView>(Resource.Id.login_password);
            try
            {
                Android.Support.V7.Widget.Toolbar tb = lf.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.login_toolbar);
                tb.Title = "Process Dashboard Companion";
            }
            catch (Exception e)
            {
                Debug.WriteLine("Message : "+e.Message);
            }

            token.Text = "GO.YN-HK1";
            username.Text = "test";
            password.Text = "test";

            /*
            token.Text = "";
            username.Text = "";
            password.Text = "";
            */
            login.Click += (sender, args) =>
            {
                if (token.Text.Equals("") || username.Text.Equals("") || password.Text.Equals(""))
                {
                    Toast.MakeText(this, "Please check the values you have entered", ToastLength.Short).Show();
                }
                else
                {
                    Debug.WriteLine("We are checking network connection");
                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        AlertDialog.Builder builder = new AlertDialog.Builder(this);
                        builder.SetTitle("Unable to connect")
                            .SetMessage("Please check your internet connection and try again")
                            .SetNeutralButton("Okay", (sender2, args2) => { builder.Dispose(); })
                            .SetCancelable(false);
                        AlertDialog alert = builder.Create();
                        alert.Show();
                    }
                    else
                    {
                        CheckCredentials(token.Text, username.Text, password.Text);
                    }
                }
            };
           
        }

        public async Task<int> CheckCredentials(string datatoken, string userid, string password2)
        {
            //Check username and password
            Debug.WriteLine("We are inside the outer task");
            ProgressDialog pd = new ProgressDialog(this);
            pd.SetMessage("Checking username and password");
            pd.SetCancelable(false);
            pd.Show();
            AlertDialog.Builder builder = new AlertDialog.Builder((this));
            await Task.Run(() =>
            {
                Debug.WriteLine("We are checking username");
                HttpWebResponse resp;
                try
                {
                    DataSetLocationResolver dslr = new DataSetLocationResolver();
                    dslr.ResolveFromToken(datatoken, out baseurl, out dataset);

                    Debug.WriteLine("Base url :" + baseurl);

                    AccountStorage.SetContext(this);
                    AccountStorage.Set(userid, password2, baseurl, dataset);


                    var req =
                        WebRequest.CreateHttp(AccountStorage.BaseUrl + "api/v1/datasets/" +
                                              AccountStorage.DataSet + "/");
                    req.Method = "GET";
                    req.AllowAutoRedirect = false;
                    string credential = userid + ":" + password2;
                    req.Headers.Add("Authorization",
                        "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credential)));
                    // req.Get

                    resp = (HttpWebResponse)req.GetResponse();

                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        if (resp.GetResponseStream().CanRead)
                        {
                            Stream data = resp.GetResponseStream();
                            var reader = new StreamReader(data);

                            string responseStr = reader.ReadToEnd();
                            Debug.WriteLine(responseStr);

                            if (responseStr.Contains("auth-required"))
                            {
                                Debug.WriteLine("Wrong credentials 2");
                                AccountStorage.ClearStorage();
                                RunOnUiThread(() =>
                                {
                                    if (pd.IsShowing)
                                        pd.Dismiss();

                                    builder.SetTitle("Wrong Credentials")
                                        .SetMessage("Please check your username and password and try again.")
                                        .SetNeutralButton("Okay", (sender2, args2) => { builder.Dispose(); })
                                        .SetCancelable(false);
                                    AlertDialog alert = builder.Create();
                                    alert.Show();
                                    Debug.WriteLine("We should have shown the dialog now");

                                });

                            }
                            else if (responseStr.Contains("permission-denied"))
                            {
                                Debug.WriteLine("permission issue");
                                AccountStorage.ClearStorage();
                                RunOnUiThread(() =>
                                {
                                    if (pd.IsShowing)
                                        pd.Dismiss();

                                    builder.SetTitle("Access Denied")
                                        .SetMessage("You donot have access to this dataset")
                                        .SetNeutralButton("Okay", (sender2, args2) => { builder.Dispose(); })
                                        .SetCancelable(false);
                                    AlertDialog alert = builder.Create();

                                    alert.Show();
                                });


                            }
                            else if (responseStr.Contains("dataset"))
                            {
                                Debug.WriteLine("Username and password was correct");
                                RunOnUiThread(() =>
                                {

                                    pd.SetMessage("Getting Account Info");
                                    pd.SetCancelable(false);
                                    if (!pd.IsShowing)
                                        pd.Show();

                                });
                                Task.Run(() =>
                                {
                                    //LOAD METHOD TO GET ACCOUNT INFO


                                    Debug.WriteLine("We are going to store the values");

                                    Debug.WriteLine("We have stored the values");
                                    Debug.WriteLine(AccountStorage.BaseUrl);
                                    Debug.WriteLine(AccountStorage.DataSet);
                                    Debug.WriteLine(AccountStorage.Password);
                                    Debug.WriteLine(AccountStorage.UserId);

                                    // Switch to next screen

                                    //HIDE PROGRESS DIALOG
                                    RunOnUiThread(() =>
                                    {

                                        if (pd.IsShowing)
                                            pd.Dismiss();
                                        InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);

                                        if (imm.IsAcceptingText)
                                            imm.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);

                                        Toast.MakeText(this, "Logged in", ToastLength.Short).Show();
                                       
                                        StartActivity(typeof(MainActivity));
                                        Finish();

                                    });
                                });
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    Debug.WriteLine("We have a problem");
                    RunOnUiThread(() =>
                    {

                        if (pd.IsShowing)
                        {
                            pd.Dismiss();
                        }

                    });
                    using (WebResponse response = e.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        Console.WriteLine("Error code: {0}", httpResponse.StatusCode);

                        if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Debug.WriteLine("Wrong credentials");
                            AccountStorage.ClearStorage();
                            RunOnUiThread(() =>
                            {
                                try
                                {

                                    builder.SetTitle("Unauthorized")
                                        .SetMessage(
                                            "Please check your username and password and data token and try again.")
                                        .SetNeutralButton("Okay", (sender2, args2) => { builder.Dispose(); })
                                        .SetCancelable(false);

                                    AlertDialog alert = builder.Create();

                                    alert.Show();

                                }
                                catch (Exception e2)
                                {
                                    Debug.WriteLine("We have hit an error while showing the dialog :" + e2.Message);
                                    AccountStorage.ClearStorage();
                                }
                            });



                        }
                    }
                }


                catch (Exception e)
                {
                    // Catching any generic exception
                    Debug.WriteLine("We have hit a generic exception :" + e.Message);
                    AccountStorage.ClearStorage();
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder builder2 = new AlertDialog.Builder(this);
                        builder2.SetTitle("Error occured")
                            .SetMessage(e.Message +
                                        ". Please report this error to the developers. We are sorry for the inconvenience.")
                            .SetNeutralButton("Okay", (sender2, args2) => { builder2.Dispose(); })
                            .SetCancelable(false);
                        AlertDialog alert2 = builder2.Create();
                        alert2.Show();
                    });
                }

                return true;
            });

            //    pd.Dismiss();

            Debug.WriteLine("We are done with the outer task");
            return 0;
        }





    }
}