using System;
using SQLite;

namespace ProcessDashboard.iOS
{
	public interface ISQLite
	{
		SQLiteConnection GetConnection();
	}
}

