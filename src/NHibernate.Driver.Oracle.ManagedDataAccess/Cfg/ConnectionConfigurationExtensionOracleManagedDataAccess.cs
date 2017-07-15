using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionOracleManagedDataAccess
	{
		public static IConnectionConfiguration ByOracleManagedDataAccessDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<OracleManagedDataAccessDriver>();
		}
	}
}
