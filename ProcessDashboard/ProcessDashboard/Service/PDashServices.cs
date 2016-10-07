#region
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fusillade;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Polly;
using ProcessDashboard.DTO;
using ProcessDashboard.Service.Interface;
using Task = ProcessDashboard.DTO.Task;
#endregion
namespace ProcessDashboard.Service_Access_Layer
{
    /*
     * 
     * Name: PDashServices.cs
     * 
     * Purpose: This class is a concerete implementation for IPDashServices interface.
     * 
     * Description:
     * This class provides concrete implemntation for getting values either from remote service or from local database.
     * The remote service will inturn make use of a concrete implementation of IApiTypes interface.
     * The local service use of the Database Manager for connecting to SQlite Database.

     */

    public class PDashServices : IPDashServices
    {
        // Api Service for making the request using Fusilade
        private readonly IApiTypes _apiService;
        private string AuthHeader
        {
            get
            {
                var authData = $"{AccountStorage.UserId}:{AccountStorage.Password}";
                return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authData));
            }
        }
        private readonly Policy _globalPolicy = Policy
            .Handle<WebException>(ex => ex.Status==WebExceptionStatus.ProtocolError && ((HttpWebResponse)ex.Response).StatusCode!=HttpStatusCode.Unauthorized)
            .Or<Exception>()
            .WaitAndRetryAsync
            (
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );


        // DB Manager to manage Database operations
        // private readonly DbManager _dbm;

        
        private readonly Util _util;

        public PDashServices(IApiTypes apiService)
        {
            _apiService = apiService;
            _util = Util.GetInstance();
            Policy.Handle<Exception>()
                .WaitAndRetryAsync
            (
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );


            //_dbm = DbManager.GetInstance();
        }

        /*
         * List of projects 
         * 
         */

        public async Task<List<Project>> GetProjectsListRemote(Priority priority, string dataset)
        {
            CheckConnection();
            try
            {
                var projectsDtoTask = _apiService.GetApi(priority).GetProjectsList(dataset, AuthHeader);
                var projects = await _globalPolicy
                    .ExecuteAsync(async () => await projectsDtoTask);

                if (!projects.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(projects.Err.Msg, projects.Err.Code);
                }

                return projects.Projects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Task>> GetTasksListRemote(Priority priority, string dataset, string projectId)
        {
            CheckConnection();
            try
            {
                var getTasksDtoTask = _apiService.GetApi(priority)
                    .GetTasksList(dataset, projectId, AuthHeader);

                var tasks = await _globalPolicy
                    .ExecuteAsync(async () => await getTasksDtoTask);

                // Adding the project value to each of the tasks
                var p = tasks.ForProject;

                for (var i = 0; i < tasks.ProjectTasks.Count; i++)
                {
                    var t = tasks.ProjectTasks[i];
                    t.Project = p;
                    // TODO: Check whether the line below can be removed
                    tasks.ProjectTasks[i] = t;
                }

                if (!tasks.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(tasks.Err.Msg, tasks.Err.Code);
                }

                return tasks.ProjectTasks;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Task> GetTaskDetailsRemote(Priority priority, string dataset, string projecttaskId)
        {
            CheckConnection();
            try
            {
                var getTaskDetailsDtoTask = _apiService.GetApi(priority)
                    .GetTaskDetails(dataset, projecttaskId, AuthHeader);

                var task = await _globalPolicy
                    .ExecuteAsync(async () => await getTaskDetailsDtoTask);

                if (!task.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(task.Err.Msg, task.Err.Code);
                }

                return task.Task;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Task>> GetRecentTasksRemote(Priority priority, string dataset)
        {
            CheckConnection();
            try
            {
                var getRecentTasksDtoTask = _apiService.GetApi(priority).GetRecentTasks(dataset, AuthHeader);
                var recenttask = await _globalPolicy
                    .ExecuteAsync(async () => await getRecentTasksDtoTask);

                if (!recenttask.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(recenttask.Err.Msg, recenttask.Err.Code);
                }
                return recenttask.RecentTasks;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /* 
         * List of Time Logs/ Global Time Log
         */

        public async Task<List<TimeLogEntry>> GetTimeLogsRemote(Priority priority, string dataset, int? maxResults,
            DateTime? startDateFrom, DateTime? startDateTo, string taskId, string projectId)
        {
            CheckConnection();
            try
            {
                var getTimeLogsDtoTask = _apiService.GetApi(priority)
                    .GetTimeLogs(dataset, maxResults, _util.GetServerTimeString(startDateFrom),
                        _util.GetServerTimeString(startDateTo), taskId, projectId, AuthHeader);

                var timelogs = await _globalPolicy
                    .ExecuteAsync(async () => await getTimeLogsDtoTask);

                if (!timelogs.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(timelogs.Err.Msg, timelogs.Err.Code);
                }

                return timelogs.TimeLogEntries;
            }
            catch (Exception)
            {
                throw;
            }
        }
        // TODO: This might not be needed. Can be removed if so
        public async Task<TimeLogEntry> GetATimeLogRemote(Priority priority, string dataset, string timelogId)
        {
            CheckConnection();
            try
            {
                var getTimeLogDtoTask = _apiService.GetApi(priority)
                    .GetTimeLog(dataset, timelogId, AuthHeader);

                var timelog = await _globalPolicy
                    .ExecuteAsync(async () => await getTimeLogDtoTask);

                if (!timelog.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(timelog.Err.Msg, timelog.Err.Code);
                }

                return timelog.TimeLogEntry;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Task> UpdateTaskDetails(Priority priority, string dataset, string projecttaskId,
            double? estimatedTime,
            DateTime? completionDate, bool markIncomplete)
        {
            CheckConnection();
            try
            {
                var value = new Dictionary<string, object>();

                if (estimatedTime.HasValue)
                    value.Add("estimatedTime", estimatedTime.Value);
                if (markIncomplete)
                    value.Add("completionDate", null);
                else if (completionDate.HasValue)
                    value.Add("completionDate", _util.GetServerTimeString(completionDate));

                var updateTaskDtoTask = _apiService.GetApi(priority)
                    .UpdateTaskDetails(AuthHeader, dataset, projecttaskId, _util.GetEditTimeStamp(), value);

                var task = await _globalPolicy
                    .ExecuteAsync(async () => await updateTaskDtoTask);

                if (!task.Stat.Equals("ok"))
                {
                    //TODO: Remove before production
                    Debug.WriteLine("Msg :" + task.Err.Msg + " Code :" + task.Err.Code);
                    throw new StatusNotOkayException(task.Err.Msg, task.Err.Code);
                }

                return task.Task;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /*
         * Adding/Updating/Deleting Time Log entries
         */

        public async Task<TimeLogEntry> AddTimeLog(Priority priority, string dataset, string comment, DateTime startDate,
            string taskId, double loggedTime, double interruptTime, bool open)
        {
            CheckConnection();
            try
            {
                Debug.WriteLine("1");
                var value = new Dictionary<string, object>
                {
                    {"comment", comment},
                    {"startDate", _util.GetServerTimeString(startDate)},
                    {"taskId", taskId},
                    {"loggedTime", loggedTime},
                    {"editTimestamp", _util.GetEditTimeStamp()},
                    {"open", open ? "true" : "false"},
                    {"interruptTime", interruptTime}
                };

                var addTimeLog = _apiService.GetApi(priority).AddTimeLog(AuthHeader, dataset, value);
                Debug.WriteLine("2");

                var timelogged = await addTimeLog;
                Debug.WriteLine("3");
                if (timelogged.Stat.Equals("fail") && timelogged.Err.Code.Equals("stopTimeLogging"))
                {
                    throw new CancelTimeLoggingException(timelogged.Err.StopTime);
                }
                else if (!timelogged.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(timelogged.Err.Msg, timelogged.Err.Code);
                }
                
                return timelogged.TimeLogEntry;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TimeLogEntry> UpdateTimeLog(Priority priority, string dataset, string timeLogId,
            string comment, DateTime? startDate, string taskId,
             double? loggedTimeDelta, double? interruptTimeDelta, bool open)
        {
            CheckConnection();

            try
            {

                var value = new Dictionary<string, object>();
                if (comment != null)
                    value.Add("comment", comment);
                if (taskId != null)
                    value.Add("taskId", taskId);
                if (startDate.HasValue)
                    value.Add("startDate", _util.GetServerTimeString(startDate));
                if (loggedTimeDelta.HasValue)
                    value.Add("loggedTimeDelta", loggedTimeDelta.Value);
                value.Add("open", open ? "true" : "false");
                if (interruptTimeDelta.HasValue)
                    value.Add("interruptTimeDelta", interruptTimeDelta.Value);
                value.Add("editTimestamp", _util.GetEditTimeStamp());

                var updateTimeLog = _apiService.GetApi(priority)
                    .UpdateTimeLog(AuthHeader, dataset, timeLogId, value);

                var updatedTimeLog = await _globalPolicy
                    .ExecuteAsync(async () => await updateTimeLog);
                if (updatedTimeLog.Stat.Equals("fail") && updatedTimeLog.Err.Code.Equals("stopTimeLogging"))
                {
                    throw new CancelTimeLoggingException(updatedTimeLog.Err.StopTime);
                }
                if (!updatedTimeLog.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(updatedTimeLog.Err.Msg, updatedTimeLog.Err.Code);
                }

                return updatedTimeLog.TimeLogEntry;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DeleteRoot> DeleteTimeLog(Priority priority, string dataset, string timelogId)
        {
            CheckConnection();
            try
            {
                var editTimeStamp = _util.GetEditTimeStamp();

                var deleteTimeLog = _apiService.GetApi(priority)
                    .DeleteTimeLog(AuthHeader, dataset, timelogId, editTimeStamp);

                var deletestatus = await _globalPolicy
                    .ExecuteAsync(async () => await deleteTimeLog);
                if (deletestatus.Stat.Equals("fail") && deletestatus.Err.Code.Equals("stopTimeLogging"))
                {
                    throw new CancelTimeLoggingException(deletestatus.Err.StopTime);
                }
                if (!deletestatus.Stat.Equals("ok"))
                {
                    throw new StatusNotOkayException(deletestatus.Err.Msg, deletestatus.Err.Code);
                }

                return deletestatus;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /*
         * Check Wifi
         */

        public bool IsWifiConnected()
        {
            var ct = ConnectionType.WiFi;
            return CrossConnectivity.Current.ConnectionTypes.Contains(ct);
        }

        /*
         * Check Network Connectivity
         */

        public void CheckConnection()
        {
            if (SettingsData.WiFiOnly)
            {
                if (!IsWifiConnected())
                {
					throw new CannotReachServerException("App running in WiFi only mode, but WiFi is unavailable.");
                }
            }
            if (!CrossConnectivity.Current.IsConnected)
            {
                throw new CannotReachServerException("Server connection unavailable.");
            }
        }
    }
}