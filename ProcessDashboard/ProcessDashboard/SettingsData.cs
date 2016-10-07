using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace ProcessDashboard
{
	public static class SettingsData
	{
		private static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		private const string WiFiOnlyKey = "WiFiOnly";
		private static readonly bool WiFiOnlyDefault = true;

		public static bool WiFiOnly
		{
			get
			{
				return AppSettings.GetValueOrDefault<bool>(WiFiOnlyKey, WiFiOnlyDefault);
			}
			set
			{
				AppSettings.AddOrUpdateValue<bool>(WiFiOnlyKey, value);
			}
		}

		private const string MaxContIntTimeMinKey = "MaxContIntTimeMin";
		private static readonly int MaxContIntTimeMinDefault = 10;

		public static int MaxContIntTimeMin
		{
			get
			{
				return AppSettings.GetValueOrDefault<int>(MaxContIntTimeMinKey, MaxContIntTimeMinDefault);
			}
			set
			{
				AppSettings.AddOrUpdateValue<int>(MaxContIntTimeMinKey, value);
			}
		}

		private const string ForgottenTmrThsMinKey = "ForgottenTmrThsMin";
		private static readonly int ForgottenTmrThsMinDefault = 60;

		public static int ForgottenTmrThsMin
		{
			get
			{
				return AppSettings.GetValueOrDefault<int>(ForgottenTmrThsMinKey, ForgottenTmrThsMinDefault);
			}
			set
			{
				AppSettings.AddOrUpdateValue<int>(ForgottenTmrThsMinKey, value);
			}
		}
	}
}
