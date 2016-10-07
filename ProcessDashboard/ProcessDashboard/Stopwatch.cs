#region
using System;
#endregion

namespace ProcessDashboard
{
	public class Stopwatch
	{
		private const double Minutes = 60000.0;
		private DateTime _firstStartTime = new DateTime(0, DateTimeKind.Utc);
		private long _interruptMillis;
		private long _loggedMillis;
		private DateTime _startTime = new DateTime(0, DateTimeKind.Utc);
		private DateTime _stopTime = new DateTime(0, DateTimeKind.Utc);

		public DateTime Get_first_startTime()
		{
			return _firstStartTime;
		}

		public void Start()
		{
			if (_startTime.Ticks == 0)
			{
				_startTime = DateTime.UtcNow;
				if (_firstStartTime.Ticks == 0)
				{
					_firstStartTime = _startTime;
				}
				if (_stopTime.Ticks != 0)
				{
					_interruptMillis += (long)(_startTime - _stopTime).TotalMilliseconds;
				}
			}
		}

		public void Stop()
		{
			StopAsOf(DateTime.UtcNow);
		}

		private void StopAsOf(DateTime when)
		{
			if (_startTime.Ticks != 0)
			{
				_stopTime = when;
				_loggedMillis += (long)(_stopTime - _startTime).TotalMilliseconds;
				Console.WriteLine("**** new logged millis: " + _loggedMillis);
				_startTime = new DateTime(0, DateTimeKind.Utc);
			}
		}

		public bool IsRunning()
		{
			return _startTime.Ticks != 0;
		}

		public bool IsPaused()
		{
			return _startTime.Ticks == 0;
		}

		public void Reset()
		{
			_firstStartTime = new DateTime(0, DateTimeKind.Utc);
			_startTime = new DateTime(0, DateTimeKind.Utc);
			_stopTime = new DateTime(0, DateTimeKind.Utc);
			_loggedMillis = 0;
			_interruptMillis = 0;
		}

		public void SetLoggedMinutes(double minutes)
		{
			if (IsRunning())
			{
				Stop();
				_loggedMillis = (long)(minutes * Minutes);
				Start();
			}
			else
			{
				_loggedMillis = (long)(minutes * Minutes);
			}
		}

		public double GetLoggedMinutes()
		{
			Console.WriteLine("stopwatch-getLoggedMinutes: _loggedMillis=" + _loggedMillis + "; isRunning=" + IsRunning());
			long time = (long)(_loggedMillis / Minutes);
			if (IsRunning())
			{
				time += (long)(DateTime.UtcNow - _startTime).TotalMinutes;
			}
			Console.WriteLine("return: " + time);
			return time;
		}

		public void SetInterruptMinutes(double minutes)
		{
			_interruptMillis = (long)(minutes * Minutes);
		}

		public double GetInterruptMinutes()
		{
			return _interruptMillis / Minutes;
		}

		public double GetTrailingLoggedMinutes()
		{
			if (IsRunning())
			{
				return (DateTime.UtcNow - _startTime).TotalMinutes;
			}
			else
			{
				return 0;
			}
		}

		public double GetTrailingInterruptMinutes()
		{
		    if (IsPaused() && _stopTime.Ticks != 0)
			{
				return (DateTime.UtcNow - _stopTime).TotalMinutes;
			}
		    return 0;
		}

	    public void MaybeCancelRunawayTimer(double maxTrailingLoggedMinutes)
		{
			double trailingLoggedMinutes = GetTrailingLoggedMinutes();
			if (trailingLoggedMinutes < maxTrailingLoggedMinutes)
			{
				return;
			}
			double minutesToCancel = trailingLoggedMinutes - maxTrailingLoggedMinutes;
			long millisToCancel = (long)(minutesToCancel * Minutes);
			DateTime cancelTime = DateTime.UtcNow.AddMilliseconds(-millisToCancel);
			CancelTimingAsOf(cancelTime);
			TimeLoggingController.GetInstance().OnTimeLoggingStateChanged(TimeLoggingControllerStates.TimeLogStopped, "Timer stopped automatically.");
		}

		public void CancelTimingAsOf(DateTime cancellationTime)
		{
			Console.WriteLine(" *** Cancelation Time: " + cancellationTime.ToLocalTime().ToString());
			DateTime now = DateTime.UtcNow;
			cancellationTime = Util.GetInstance().GetServerTime(cancellationTime);
			if (cancellationTime > now)
			{
				cancellationTime = now;
			}

			if (_startTime.Ticks != 0)
			{
				if (_startTime < cancellationTime)
				{
					StopAsOf(cancellationTime);
					return;
				}
				else
				{
					if (_stopTime.Ticks != 0)
					{
						long interrupToDiscard = (long)(_startTime - _stopTime).TotalMilliseconds;
						_interruptMillis -= interrupToDiscard;
						if (_interruptMillis < 0)
						{
							_interruptMillis = 0;
						}
					}
					_startTime = new DateTime(0, DateTimeKind.Utc);
				}
			}

			if (_stopTime.Ticks != 0 && cancellationTime < _stopTime)
			{
				long overlapMillis = (long)(_stopTime - cancellationTime).TotalMilliseconds;
				if (overlapMillis > _loggedMillis)
				{
					Reset();
				}
				else
				{
					_loggedMillis = _loggedMillis - overlapMillis;
					_stopTime = cancellationTime;
				}
			}
		}
	}
}