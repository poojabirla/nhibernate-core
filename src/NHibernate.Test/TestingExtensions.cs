using NHibernate.Driver;

namespace NHibernate.Test
{
	public static class TestingExtensions
	{
		public static bool IsOdbcDriver(this IDriver driver)
		{
			return (driver is OdbcDriver);
		}

		public static bool IsOdbcDriver(this System.Type driverClass)
		{
			return typeof(OdbcDriver).IsAssignableFrom(driverClass);
		}

		public static bool IsOleDbDriver(this IDriver driver)
		{
			return (driver is OleDbDriver);
		}

		public static bool IsOleDbDriver(this System.Type driverClass)
		{
			return typeof(OleDbDriver).IsAssignableFrom(driverClass);
		}

		/// <summary>
		/// Matches both SQL Server 2000 and 2008 drivers
		/// </summary>
		public static bool IsSqlServerDriver(this IDriver driver)
		{
#pragma warning disable 618
			return (driver is SqlClientDriver) 
#pragma warning restore 618
				|| (driver is SqlServer2000Driver);
		}

		/// <summary>
		/// Matches both SQL Server 2000 and 2008 drivers
		/// </summary>
		public static bool IsSqlServerDriver(this System.Type driverClass)
		{
#pragma warning disable 618
			return typeof(SqlClientDriver).IsAssignableFrom(driverClass)
#pragma warning restore 618
				|| typeof(SqlServer2000Driver).IsAssignableFrom(driverClass);
		}

		public static bool IsSqlServer2008Driver(this IDriver driver)
		{
#pragma warning disable 618
			return (driver is Sql2008ClientDriver)
#pragma warning restore 618
				|| (driver is SqlServer2008Driver);
		}

		public static bool IsMySqlDriver(this System.Type driverClass)
		{
#pragma warning disable 618
			return typeof(MySqlDataDriver).IsAssignableFrom(driverClass)
#pragma warning restore 618
				|| typeof(MySqlDriver).IsAssignableFrom(driverClass);
		}


		public static bool IsFirebirdDriver(this IDriver driver)
		{
#pragma warning disable 618
			return (driver is FirebirdClientDriver)
#pragma warning restore 618
				|| (driver is FirebirdDriver);
		}

		/// <summary>
		/// If driver is Firebird, clear the pool.
		/// Firebird will pool each connection created during the test and will marked as used any table
		/// referenced by queries. It will at best delays those tables drop until connections are actually
		/// closed, or immediately fail dropping them.
		/// This results in other tests failing when they try to create tables with same name.
		/// By clearing the connection pool the tables will get dropped. This is done by the following code.
		/// Moved from NH1908 test case, contributed by Amro El-Fakharany.
		/// </summary>
		public static void ClearPoolForFirebirdDriver(this IDriver driver)
		{
			switch (driver)
			{
#pragma warning disable 618
				case FirebirdClientDriver fbDriver:
					fbDriver.ClearPool(null);
					break;
#pragma warning restore 618
				case FirebirdDriver fbDriver2:
					fbDriver2.ClearPool(null);
					break;
			}
		}

		public static bool IsOracleDataClientDriver(this IDriver driver)
		{
			return (driver is OracleDataClientDriver);
		}

		public static bool IsOracleDataClientDriver(this System.Type driverClass)
		{
			return typeof(OracleDataClientDriver).IsAssignableFrom(driverClass);
		}

		public static bool IsOracleLiteDataClientDriver(this IDriver driver)
		{
			return (driver is OracleLiteDataClientDriver);
		}

		public static bool IsOracleManagedDriver(this IDriver driver)
		{
#pragma warning disable 618
            return (driver is OracleManagedDataClientDriver)
#pragma warning restore 618
				|| (driver is OracleManagedDriver);
		}
	}
}
