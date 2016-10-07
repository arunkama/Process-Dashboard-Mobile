#region
using System;
using System.Text;
#endregion
namespace ProcessDashboard
{
    internal class Settings
    {
        private static Settings _instance;

        public bool CheckWifi { get; set; }

        public static Settings GetInstance()
        {
            return _instance ?? (_instance = new Settings());
        }
    }
}