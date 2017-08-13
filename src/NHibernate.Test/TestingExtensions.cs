using NHibernate.Driver;

namespace NHibernate.Test
{
	public static class TestingExtensions
	{
		public static bool IsOdbcDriver(this IDriver driver)
		{
#if NETCOREAPP2_0
			return false;
#else
			return (driver is OdbcDriver);
#endif
		}

		public static bool IsOdbcDriver(this System.Type driverClass)
		{
#if NETCOREAPP2_0
			return false;
#else
			return typeof(OdbcDriver).IsAssignableFrom(driverClass);
#endif
		}

		public static bool IsOleDbDriver(this IDriver driver)
		{
#if NETCOREAPP2_0
			return false;
#else
			return (driver is OleDbDriver);
#endif
		}

		public static bool IsOleDbDriver(this System.Type driverClass)
		{
#if NETCOREAPP2_0
			return false;
#else
		return typeof(OleDbDriver).IsAssignableFrom(driverClass);
#endif
		}

		public static bool IsOracleDataClientDriver(this IDriver driver)
		{
#if NETCOREAPP2_0
			return false;
#else
			return (driver is OracleDataClientDriver);
#endif
		}

		public static bool IsOracleLiteDataClientDriver(this IDriver driver)
		{
#if NETCOREAPP2_0
			return false;
#else
			return (driver is OracleLiteDataClientDriver);
#endif
		}

		public static bool IsOracleManagedDataAccessDriver(this IDriver driver)
		{
#if NETCOREAPP2_0
			return false;
#else
            //return (driver is OracleManagedDataAccessDriver);
            return false;
#endif
		}
	}
}
