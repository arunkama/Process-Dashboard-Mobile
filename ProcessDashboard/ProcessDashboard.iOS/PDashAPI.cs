using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProcessDashboard.DTO;
using ProcessDashboard.Service;
using ProcessDashboard.Service.Interface;
using ProcessDashboard.Service_Access_Layer;
using ProcessDashboard.SyncLogic;
using UIKit;
using CoreFoundation;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;

namespace ProcessDashboard.iOS
{
	// use this wrapper to get different API types, instead of initializing them again and again
	public static class PDashAPI
	{
		public static readonly RobustController Controller;
		private static StateChangedEventHandler stateChangedHandler;
		public static event StateChangedEventHandler UIHandlerToDispatch;

		static PDashAPI()
		{
			var apiService = new ApiTypes(null);
			var service = new PDashServices(apiService);
			Controller = new RobustController(service);
			stateChangedHandler = new StateChangedEventHandler(iOSTimeLoggingStateChanged);
			TimeLoggingController.TimeLoggingStateChanged += stateChangedHandler;
		}

		private static void iOSTimeLoggingStateChanged(object sender, StateChangedEventArgs ea)
		{
			if (UIHandlerToDispatch != null)
			{
				UIApplication.SharedApplication.InvokeOnMainThread(() =>
				{
					UIHandlerToDispatch(sender, ea);
				});
			}
		}
	}

	// wrapper of Controller
	// hides "dataset"
	// handles exceptions
	public class RobustController : Controller
	{
		// dataset value is refreshed after each login
		private string dataset = AccountStorage.DataSet;
		public delegate void ControllerExceptionHandler(string message);

		public void RefreshDataset()
		{
			dataset = AccountStorage.DataSet;
		}

		public RobustController(IPDashServices pDash) : base(pDash) {}

		// Get list of projects

		public async Task<List<Project>> GetProjects()
		{
			try
			{
				return await base.GetProjects(dataset);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		// Get list of tasks
		public async Task<List<DTO.Task>> GetTasks(string projectId)
		{
			try
			{
				return await base.GetTasks(dataset, projectId);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		// Get Time Log entries with optional parameters.
		// Optional parameters should be specified as null
		// only dataset is required
		public async Task<List<TimeLogEntry>> GetTimeLogs(int? maxResults, DateTime? startDateFrom, DateTime? startDateTo, string taskId, string projectId)
		{
			try
			{
				return await base.GetTimeLogs(dataset, maxResults, startDateFrom, startDateTo, taskId, projectId);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		// Get the list of recent tasks
		public async Task<List<DTO.Task>> GetRecentTasks()
		{
			try
			{
				return await base.GetRecentTasks(dataset);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		// Get details about a single task
		public async Task<DTO.Task> GetTask(string taskId)
		{
			try
			{
				return await base.GetTask(dataset, taskId);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		// Add a new time log entry
		public async Task<TimeLogEntry> AddATimeLog(string comment, DateTime startDate, string taskId, double loggedTime, double interruptTime, bool open)
		{
			try
			{
				return await base.AddATimeLog(dataset, comment, startDate, taskId, loggedTime, interruptTime, open);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		//Update an existing timelog entry
		public async Task<TimeLogEntry> UpdateTimeLog(string timeLogId, string comment, DateTime? startDate, string taskId, double? loggedTime, double? interruptTime, bool open)
		{
			try
			{
				return await base.UpdateTimeLog(dataset, timeLogId, comment, startDate, taskId, loggedTime, interruptTime, open);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		//Delete a  timelog entry
		public async Task<Service.Interface.DeleteRoot> DeleteTimeLog(string timeLogId)
		{
			try
			{
				return await base.DeleteTimeLog(dataset, timeLogId);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		//Read a timelog entry
		public async Task<TimeLogEntry> GetTimeLog(string timeLogId)
		{
			try
			{
				return await base.GetTimeLog(dataset, timeLogId);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}

		// Update a particular task
		public async Task<DTO.Task> UpdateATask(string taskId, double? estimatedTime, DateTime? completionDate, bool markTaskIncomplete)
		{
			try
			{
				return await base.UpdateATask(dataset, taskId, estimatedTime, completionDate, markTaskIncomplete);
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("401"))
				{
					AccountStorage.ClearPassword();
					AppDelegate del = UIApplication.SharedApplication.Delegate as AppDelegate;
					del.BindLoginViewController();
				}
				throw ex;
			}
		}
	}
}

