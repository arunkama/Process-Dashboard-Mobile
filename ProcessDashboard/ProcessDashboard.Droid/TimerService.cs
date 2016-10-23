using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
namespace ProcessDashboard.Droid
{
    [Service(Label = "TimerService")]
    [IntentFilter(new String[] { "com.tumasolutions.processdashboard.TimerService" })]
    public class TimerService : Android.App.Service
    {
        private static TimeLoggingController _tlc;
        private readonly string eventName = "processdashboard.timelogger";
        public TimerService()
        {
            _tlc = TimeLoggingController.GetInstance();
        }

        private string _taskId { get; set; }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                _taskId = intent.GetStringExtra("taskId");
                var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), 0);

                Notification.Builder builder = new Notification.Builder(this)
                    .SetContentIntent(pendingIntent)
                    .SetContentText("Timer is running")
                    .SetContentTitle("Process Dashboard")
                    .SetSmallIcon(Resource.Drawable.Icon);

                var ongoing = builder.Build();

                StartForeground((int)NotificationFlags.ForegroundService, ongoing);

                Intent intent2 = new Intent(eventName);

                //TODO: THERE SEEMS TO BE SOMETHING WEIRD HERE!!

                TimeLoggingController.TimeLoggingStateChanged += _tlc_TimeLoggingStateChanged;
                _tlc.StartTiming(_taskId);
                intent2.PutExtra("key", "Time logging has been started by the server");
                LocalBroadcastManager.GetInstance(this).SendBroadcast(intent2);
                System.Diagnostics.Debug.WriteLine("This is a foreground Service");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            return StartCommandResult.RedeliverIntent;
        }

        private void _tlc_TimeLoggingStateChanged(object sender, StateChangedEventArgs e)
        {
            Intent intent = new Intent(eventName);
            // You can also include some extra data.
            System.Diagnostics.Debug.WriteLine("We have a change of state");

            if (e.NewState.Equals(TimeLoggingControllerStates.TimeLogCanceled))

            {

                System.Diagnostics.Debug.WriteLine("1");
                intent.PutExtra("key", "Time logging has been cancelled by the server");

                // Communicate with the activity to update the UI

            }
            if (e.NewState.Equals(TimeLoggingControllerStates.TimeLogUpdated))
            {
                System.Diagnostics.Debug.WriteLine("2");
                intent.PutExtra("key", "Time logging has been updated by the server");
            }

            if (e.NewState.Equals(TimeLoggingControllerStates.TimeLogCreated))
            {
                System.Diagnostics.Debug.WriteLine("3");
                intent.PutExtra("key", "Time logging has been created by the server");
            }


            if (e.NewState.Equals(TimeLoggingControllerStates.TimeLogUpdateFailed))
            {
                System.Diagnostics.Debug.WriteLine("4");
                intent.PutExtra("key", "Time logging Update has failed");
            }
            if (e.NewState.Equals(TimeLoggingControllerStates.TimeLogStopped))
            {
                System.Diagnostics.Debug.WriteLine("5");
                intent.PutExtra("key", "Time logging has been stopped");
            }

            LocalBroadcastManager.GetInstance(this).SendBroadcast(intent);

        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public async void StopTimer()
        {
            await _tlc.StopTiming();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_tlc.IsTimerRunning())
            {
                Intent intent2 = new Intent(eventName);
                intent2.PutExtra("key", "Time logging has been cancelled by the server");
                LocalBroadcastManager.GetInstance(this).SendBroadcast(intent2);
                _tlc.StopTiming();

            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;

        }


        class TimerHandler : Handler
        {
            public override void HandleMessage(Message msg)
            {

                System.Diagnostics.Debug.WriteLine(msg.Arg1);
                System.Diagnostics.Debug.WriteLine(msg.Arg2);

                //if(_tlc.IsReadyForNewTimeLog);
            }
        }


        public class TimerServiceBinder : Binder
        {
            TimerService service;

            public TimerServiceBinder(TimerService service)
            {
                this.service = service;
            }

            public TimerService GetTimerService()
            {
                return service;
            }

        }


    }
}