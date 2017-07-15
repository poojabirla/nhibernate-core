using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Example.Web.Models;
using NHibernate.Mapping.ByCode;

namespace NHibernate.Example.Web.Infrastructure
{
	public class AppSessionFactory
	{
		public Configuration Configuration { get; }
		public ISessionFactory SessionFactory { get; }

		public AppSessionFactory(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
		{
			NHibernate.LoggerProvider.SetLoggersFactory(new NHibernateToMicrosoftLoggerFactory(loggerFactory));

			var mapper = new ModelMapper();
			mapper.AddMapping<ItemMap>();
			var domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
			
			var cfg = new Configuration();
			cfg.SessionFactory()
				.GenerateStatistics()
				.Integrate
					.Using<MsSql2008Dialect>()
					.Connected
						.BySqlClientDriver()
						.Using(@"Server=(local)\SQLEXPRESS;initial catalog=nhibernate;Integrated Security=true");

			cfg.AddMapping(domainMapping);

			Configuration = cfg;
			SessionFactory = cfg.BuildSessionFactory();
		}

		public ISession OpenSession()
		{
			return SessionFactory.OpenSession();
		}
	}
}
