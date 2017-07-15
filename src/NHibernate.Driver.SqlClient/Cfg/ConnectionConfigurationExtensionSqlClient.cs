using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionSqlClient
	{
		public static IConnectionConfiguration BySqlClientDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<SqlClientDriver>();
		}

		public static IConnectionConfiguration BySql2008ClientDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<Sql2008ClientDriver>();
		}
	}
}
