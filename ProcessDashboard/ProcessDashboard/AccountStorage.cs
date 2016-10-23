using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Auth;

#if __ANDROID__
using Android.Content;
#endif

namespace ProcessDashboard
{
	/*
	 * https://developer.xamarin.com/recipes/cross-platform/xamarin-forms/general/store-credentials/
	 */
	public class AccountStorage
	{
		private const String AppName = "Process_Dashboard";

		#if __ANDROID__
	    private static Context _context;

	    public static void SetContext(Context context)
	    {
	        _context = context;
	    }
		#endif

		public static void Set(String userId, String password, String baseUrl, String dataSet, String dataToken)
		{
			ClearStorage();

			if (String.IsNullOrEmpty(userId) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(baseUrl) || String.IsNullOrEmpty(dataSet) || String.IsNullOrEmpty(dataToken))
			{
				throw new FormatException("input parameters contain null or empty strings");
			}
			else
			{
				Account account = new Account
				{
					Username = userId
				};
				account.Properties.Add("Password", password);
				account.Properties.Add("BaseUrl", baseUrl);
				account.Properties.Add("DataSet", dataSet);
				account.Properties.Add("DataToken", dataToken);

				#if __IOS__
				AccountStore.Create().Save(account, AppName);
				#else
				AccountStore.Create(_context).Save(account, AppName);
				#endif
			}
		}

		public static void ClearStorage()
		{
			while (true)
			{
				#if __ANDROID__
			    var accounts = AccountStore.Create(_context).FindAccountsForService(AppName);
				#else
				List<Account> accounts = (List<Account>)AccountStore.Create().FindAccountsForService(AppName);
				#endif

				if (accounts.Count() > 0)
				{
					#if __ANDROID__
                    AccountStore.Create(_context).Delete(accounts.First(), AppName);
					#else
					AccountStore.Create().Delete(accounts[0], AppName);
					#endif
				}
				else
				{
					break;
				}
			}
		}

		public static string AuthHeader
		{
			get
			{
				String _username = AccountStorage.UserId;
				String _password = AccountStorage.Password;
				var authData = $"{_username}:{_password}";
				return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authData));
			}
		}

		public static string UserId
		{
			get
			{
				#if __IOS__
				List<Account> accounts = (List<Account>) AccountStore.Create().FindAccountsForService(AppName);
				return (accounts.Count > 0) ? accounts[0].Username : null;
				#else
                Account account =  AccountStore.Create(_context).FindAccountsForService(AppName).ElementAtOrDefault(0);
                if (account == null)
			    {
			        System.Diagnostics.Debug.WriteLine("Account is null ");
			    }
			    else
			    {
			        System.Diagnostics.Debug.WriteLine("Account is not null :"+account.ToString());
                    System.Diagnostics.Debug.WriteLine("Account is not null 2:" + account.Username);
                }
                return account?.Username;
				#endif
			}
		}

		public static string Password
		{
			get
			{
				#if __IOS__
				List<Account> accounts = (List<Account>)AccountStore.Create().FindAccountsForService(AppName);
				return (accounts.Count > 0) ? accounts[0].Properties["Password"] : null;
				#else
				Account account = (Account)AccountStore.Create(_context).FindAccountsForService(AppName).ElementAtOrDefault(0);
				return account?.Properties["Password"];
				#endif
			}
		}

		public static void ClearPassword()
		{

            #if __IOS__
            List<Account> accounts = (List<Account>)AccountStore.Create().FindAccountsForService(AppName);
			if (accounts.Count > 0)
			{
				accounts[0].Properties["Password"] = "";
			}
            #endif
		}

		public static string BaseUrl
		{
			get
			{
#if __IOS__
				List<Account> accounts = (List<Account>)AccountStore.Create().FindAccountsForService(AppName);
				return (accounts.Count > 0) ? accounts[0].Properties["BaseUrl"] : null;
#else
				Account account = (Account)AccountStore.Create(_context).FindAccountsForService(AppName).ElementAtOrDefault(0);
				return account?.Properties["BaseUrl"];
#endif
			}
		}

		public static string DataSet
		{
			get
			{
#if __IOS__
				List<Account> accounts = (List<Account>)AccountStore.Create().FindAccountsForService(AppName);
				return (accounts.Count > 0) ? accounts[0].Properties["DataSet"] : null;
#else
				Account account = (Account)AccountStore.Create(_context).FindAccountsForService(AppName).ElementAtOrDefault(0);
				return account?.Properties["DataSet"];
#endif
			}
		}

		public static string DataToken
		{
			get
			{
#if __IOS__
				List<Account> accounts = (List<Account>)AccountStore.Create().FindAccountsForService(AppName);
				if (accounts.Count > 0 && accounts[0].Properties.ContainsKey("DataToken"))
				{
					return accounts[0].Properties["DataToken"];
				}
				return null;

#else
				Account account = (Account)AccountStore.Create(_context).FindAccountsForService(AppName).ElementAtOrDefault(0);
				return account?.Properties["DataSet"];
#endif
			}
		}

	}
}