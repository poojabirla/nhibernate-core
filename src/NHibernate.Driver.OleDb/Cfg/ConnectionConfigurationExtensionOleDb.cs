using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionOleDb
	{
		public static IConnectionConfiguration ByOleDbDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<OleDbDriver>();
		}
	}
}
