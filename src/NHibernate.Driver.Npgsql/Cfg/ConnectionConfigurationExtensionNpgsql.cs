using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionNpgsql
	{
		public static IConnectionConfiguration ByNpgsqlDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<NpgsqlDriver>();
		}
	}
}
