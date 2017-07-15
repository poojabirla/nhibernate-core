using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionMySql
	{
		public static IConnectionConfiguration ByMySqlDataDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<MySqlDataDriver>();
		}
	}
}
