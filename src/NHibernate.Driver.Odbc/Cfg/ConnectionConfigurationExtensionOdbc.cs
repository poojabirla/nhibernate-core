using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionOdbc
	{
		public static IConnectionConfiguration ByOdbcDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<OdbcDriver>();
		}
	}
}
