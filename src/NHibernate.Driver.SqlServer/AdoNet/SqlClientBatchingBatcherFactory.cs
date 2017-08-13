using NHibernate.Engine;

namespace NHibernate.AdoNet
{
	public class SqlClientBatchingBatcherFactory : IBatcherFactory
	{
		public virtual IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
		{
#if !NETSTANDARD2_0
			return new SqlClientBatchingBatcher(connectionManager, interceptor);
#else
			return new NonBatchingBatcher(connectionManager, interceptor);
#endif
		}
	}
}