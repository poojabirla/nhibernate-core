using NHibernate.Cfg.Loquacious;
using NHibernate.Driver;

namespace NHibernate.Cfg
{
	public static class ConnectionConfigurationExtensionFirebirdClient
	{
		public static IConnectionConfiguration ByFirebirdClientDriver(this IConnectionConfiguration cfg)
		{
			return cfg.By<FirebirdClientDriver>();
		}
	}
}
