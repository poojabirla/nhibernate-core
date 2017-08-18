using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionSqlServer
	{
		public static IConnectionConfiguration BySqlServerDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SqlServerDriver>();
		}

		public static IConnectionConfiguration BySql2008ServerDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SqlServer2008Driver>();
		}
	}
}
