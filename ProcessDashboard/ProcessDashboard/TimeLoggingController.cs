#region
using System;
using ProcessDashboard.Service;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;

#endregion
namespace ProcessDashboard
{

    // delegate used for state changes of Process Dashboard.
    // use this delegate to trigger UI updates
    public delegate void StateChangedEventHandler(object sender, StateChangedEventArgs e);

    public class TimeLoggingController
    {
        public bool isReadyForNewTimeLog = true;
        public bool WasNetworkAvailable = true;

        private Controller _controller;

        private OsTimerService _osTimerService;
        private int _savedLoggedTime;
        private int _savedInterruptTime;
        private Stopwatch _stopwatch = new Stopwatch();
        private string _taskId;
        private string _timeLogEntryId;



        // Events --------------------------------------------------------------------------------------------------------------------

        // Fired when network connection to the PDES changed from available to unavailable, or from unavailable to available
        // not real-time. We do network connections once per minute in TimeLoggingController
        // not represents global network availablility. Fired by and only by this TimeLoggingController.
        public static event StateChangedEventHandler NetworkAvailabilityChanged;
        public void OnNetworkAvailabilityChanged(NetworkAvailabilityStates e, String message)
        {
            if (NetworkAvailabilityChanged != null)
            {
                NetworkAvailabilityChanged(this, new StateChangedEventArgs(e, message));
            }
        }

        // Fired when a new time log is created successfully, updated successfully/failed, or canceled by PDES.
        public static event StateChangedEventHandler TimeLoggingStateChanged;
        public void OnTimeLoggingStateChanged(TimeLoggingControllerStates e, String message)
        {
            if (TimeLoggingStateChanged != null)
            {
                TimeLoggingStateChanged(this, new StateChangedEventArgs(e, message));
            }
        }

        // -------------------------------------------------------------------------------------------------------------------- Events

        // this class is a singleton
        private static TimeLoggingController _instance;
        public static TimeLoggingController GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TimeLoggingController();
            }
            return _instance;
        }

        public TimeLoggingController()
        {
            var apiService = new ApiTypes(null);
            var service = new PDashServices(apiService);
            _controller = new Controller(service);
            _osTimerService = new OsTimerService(this);
        }

        public async System.Threading.Tasks.Task StopTiming()
        {
            _stopwatch.Stop();
            await Save();
        }


        public async System.Threading.Tasks.Task StartTiming(string _taskId)
        {

            Console.WriteLine("Trying to start timer for task: " + _taskId);

            await SetTaskId(_taskId);
            if (_stopwatch.GetTrailingInterruptMinutes() > SettingsData.MaxContIntTimeMin)
            {
                await SaveIfNeeded();
                ReleaseTimeLogEntry(true);
            }

            _stopwatch.Start();
            await Save();

            if (_stopwatch.IsPaused())
            {
                _stopwatch.Start();
                await Save();
            }
        }

        private async System.Threading.Tasks.Task SetTaskId(string newTaskId)
        {
            if (newTaskId.Equals(_taskId))
            {
                return;
            }
            _stopwatch.Stop();
            await SaveIfNeeded();

            _taskId = newTaskId;
            ReleaseTimeLogEntry(true);
        }

        public string GetTimingTaskId()
        {
            return IsTimerRunning() ? _taskId : null;
        }

        public bool IsTimerRunning()
        {
            return _stopwatch.IsRunning();
        }

        public string GetActiveTimeLogEntryId()
        {
            return _timeLogEntryId;
        }

        public async System.Threading.Tasks.Task SetLoggedTime(int minutes)
        {
            _stopwatch.SetLoggedMinutes(minutes);
            await Save();
        }

        public async System.Threading.Tasks.Task SetInterruptTime(int minutes)
        {
            _stopwatch.SetInterruptMinutes(minutes);
            await Save();
        }

        public async void Ping(object stateInfo)
        {
            _stopwatch.MaybeCancelRunawayTimer(SettingsData.ForgottenTmrThsMin);
            await Save();
        }

        private async System.Threading.Tasks.Task Save()
        {
            try
            {
                await SaveIfNeeded();
                _osTimerService.SetBackgroundPingsEnabled(_stopwatch.IsRunning());

                if (!WasNetworkAvailable)
                {
                    OnNetworkAvailabilityChanged(NetworkAvailabilityStates.BecameAvailable, String.Empty);

                    WasNetworkAvailable = true;
                }
            }
            catch (CannotReachServerException e)
            {
                isReadyForNewTimeLog = false;

                _osTimerService.SetBackgroundPingsEnabled(true);

                OnTimeLoggingStateChanged(TimeLoggingControllerStates.TimeLogUpdateFailed, String.Empty);

                if (WasNetworkAvailable)
                {
                    OnNetworkAvailabilityChanged(NetworkAvailabilityStates.BecameUnavailable, String.Empty);
                    WasNetworkAvailable = false;
                }
            }
        }

        private async System.Threading.Tasks.Task SaveIfNeeded()
        {
            if (_timeLogEntryId == null)
            {
                if (_stopwatch.IsRunning() || TimeIsDiscrepant())
                {
                    try
                    {
                        int logged = Round(_stopwatch.GetLoggedMinutes());
                        int interrupt = Round(_stopwatch.GetInterruptMinutes());

                        var value = await _controller.AddATimeLog(AccountStorage.DataSet, null, _stopwatch.Get_first_startTime(), _taskId, logged, interrupt, _stopwatch.IsRunning());

                        _timeLogEntryId = "" + value.Id;

                        _savedLoggedTime = logged;
                        _savedInterruptTime = interrupt;

                        OnTimeLoggingStateChanged(TimeLoggingControllerStates.TimeLogCreated, String.Empty);
                    }
                    catch (CancelTimeLoggingException e)
                    {
                        await HandleCancelTimeLoggingException(e);
                        OnTimeLoggingStateChanged(TimeLoggingControllerStates.TimeLogCanceled, String.Empty);
                    }
                }
            }
            else
            {
                if (_stopwatch.IsPaused() && _stopwatch.GetLoggedMinutes() < 0.5)
                {
                    Console.WriteLine("delete time log in saveifneeded, loggedMinutes: " + _stopwatch.GetLoggedMinutes());
                    await _controller.DeleteTimeLog(AccountStorage.DataSet, _timeLogEntryId);
                    ReleaseTimeLogEntry(false);

                    OnTimeLoggingStateChanged(TimeLoggingControllerStates.TimeLogUpdated, "Time log deleted because it is too short.");
                }
                else if (TimeIsDiscrepant())
                {
                    try
                    {
                        int logged = Round(_stopwatch.GetLoggedMinutes());
                        int interrupt = Round(_stopwatch.GetInterruptMinutes());

                        await _controller.UpdateTimeLog(AccountStorage.DataSet, _timeLogEntryId, null, null, _taskId,
                                                       LoggedTimeDelta(), interruptTimeDelta(), _stopwatch.IsRunning());

                        _savedLoggedTime = logged;
                        _savedInterruptTime = interrupt;
                        OnTimeLoggingStateChanged(TimeLoggingControllerStates.TimeLogUpdated, "Time log updated");
                    }
                    catch (CancelTimeLoggingException e)
                    {
                        await HandleCancelTimeLoggingException(e);
                        OnTimeLoggingStateChanged(TimeLoggingControllerStates.TimeLogCanceled, String.Empty);
                    }
                }
            }
            isReadyForNewTimeLog = true;
        }

        private async System.Threading.Tasks.Task HandleCancelTimeLoggingException(CancelTimeLoggingException e)
        {
            Console.WriteLine("handleCancelTimeLoggingException() called");

            _stopwatch.CancelTimingAsOf(e.GetStopTime());

            Console.WriteLine("canceltimingasof called");

            await SaveIfNeeded();
            ReleaseTimeLogEntry(true);

            Console.WriteLine("handle finished");
        }

        private void ReleaseTimeLogEntry(Boolean resetStopwatch)
        {
            _timeLogEntryId = null;
            _savedLoggedTime = 0;
            _savedInterruptTime = 0;
            if (resetStopwatch)
            {
                _stopwatch.Reset();
            }
        }

        private Boolean TimeIsDiscrepant()
        {
            return LoggedTimeDelta() != 0 || interruptTimeDelta() != 0;
        }

        private int LoggedTimeDelta()
        {
            int ret = Round(_stopwatch.GetLoggedMinutes() - _savedLoggedTime);
            Console.WriteLine("loggedTimeDelta: " + ret);
            return ret;
        }

        private int interruptTimeDelta()
        {
            return Round(_stopwatch.GetInterruptMinutes() - _savedInterruptTime);
        }

        private int Round(double d)
        {
            return (int)Math.Round(d);
        }
    }
}