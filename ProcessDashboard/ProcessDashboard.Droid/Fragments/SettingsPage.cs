#region
using Android.App;
using Android.OS;
using Android.Preferences;
#endregion
namespace ProcessDashboard.Droid.Fragments
{
    // Use Preference Fragment
    public class SettingsPage : PreferenceFragment
    {
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            ((MainActivity)Activity).SetTitle("Settings");
            
            AccountStorage.SetContext(Activity);
            // Create your fragment here
            AddPreferencesFromResource(Resource.Layout.Settings);

            Preference pf = FindPreference("login_preference");
            pf.Summary = "Logged in as : " + AccountStorage.UserId;

            pf.PreferenceClick += (sender, args) =>
            {
                try
                {
                    AccountStorage.ClearStorage();
                    Activity.StartActivity(typeof(LoginActivity));
                    Activity.Finish();

                }
                catch (System.Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("We encountered an error :" + e.Message);
                }

            };



        }

        public override void OnResume()
        {
            base.OnResume();
            ((MainActivity) Activity).SetTitle("Settings");
        }

        public override bool OnPreferenceTreeClick(PreferenceScreen preferenceScreen, Preference preference)
        {

            System.Diagnostics.Debug.WriteLine("We are in the on preference portion");
            switch (preference.Key)
            {
                case "wifi_preferance":
                    SettingsData.WiFiOnly = ((CheckBoxPreference) preference).Checked;
                    break;
                case "runawaytimer_preference":
                    SettingsData.ForgottenTmrThsMin = int.Parse(((EditTextPreference) preference).Text);
                    break;
                case "interrupt_preference":
                    SettingsData.MaxContIntTimeMin = int.Parse(((EditTextPreference)preference).Text);
                    break;
            }
            return base.OnPreferenceTreeClick(preferenceScreen, preference);
        }

    

    }
}