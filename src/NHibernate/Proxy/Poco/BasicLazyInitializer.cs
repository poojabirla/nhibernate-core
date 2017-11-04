using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Proxy.Poco
{
	/// <summary> Lazy initializer for POCOs</summary>
	[Serializable]
	public abstract class BasicLazyInitializer : AbstractLazyInitializer
	{
		private static readonly IEqualityComparer IdentityEqualityComparer = new IdentityEqualityComparer();

		// Since 5.1
		[Obsolete("This field has no inherited usages in NHibernate and will be removed.")]
		internal System.Type persistentClass;
		// Since 5.1
		[Obsolete("This field has no inherited usages in NHibernate and will be removed.")]
		protected internal MethodInfo getIdentifierMethod;
		// Since 5.1
		[Obsolete("This field has no inherited usages in NHibernate and will be removed.")]
		protected internal MethodInfo setIdentifierMethod;
		// Since 5.1
		[Obsolete("This field has no inherited usages in NHibernate and will be removed.")]
		protected internal bool overridesEquals;
		// Since 5.1
		[Obsolete("This field has no inherited usages in NHibernate and will be removed.")]
		protected internal IAbstractComponentType componentIdType;

		private readonly SerializableSystemType _persistentClass;
		private readonly SerializableMethodInfo _getIdentifierMethod;
		private readonly SerializableMethodInfo _setIdentifierMethod;
		private readonly bool _overridesEquals;
		private readonly IAbstractComponentType _componentIdType;

		protected internal BasicLazyInitializer(string entityName, System.Type persistentClass, object id, 
			MethodInfo getIdentifierMethod, MethodInfo setIdentifierMethod, 
			IAbstractComponentType componentIdType, ISessionImplementor session, bool overridesEquals)
			: base(entityName, id, session)
		{
			this._persistentClass = SerializableSystemType.Wrap(persistentClass);
			this._getIdentifierMethod = SerializableMethodInfo.Wrap(getIdentifierMethod);
			this._setIdentifierMethod = SerializableMethodInfo.Wrap(setIdentifierMethod);
			this._componentIdType = componentIdType;
			this._overridesEquals = overridesEquals;

#pragma warning disable 618
			this.persistentClass = persistentClass;
			this.getIdentifierMethod = getIdentifierMethod;
			this.setIdentifierMethod = setIdentifierMethod;
			this.componentIdType = componentIdType;
			this.overridesEquals = overridesEquals;
#pragma warning restore 618
		}

		/// <summary>
		/// Adds all of the information into the SerializationInfo that is needed to
		/// reconstruct the proxy during deserialization or to replace the proxy
		/// with the instantiated target.
		/// </summary>
		/// <remarks>
		/// This will only be called if the Dynamic Proxy generator does not handle serialization
		/// itself or delegates calls to the method GetObjectData to the LazyInitializer.
		/// </remarks>
		protected virtual void AddSerializationInfo(SerializationInfo info, StreamingContext context)
		{
		}

		public override System.Type PersistentClass
		{
			get { return _persistentClass.GetType(); }
		}

		/// <summary>
		/// Invokes the method if this is something that the LazyInitializer can handle
		/// without the underlying proxied object being instantiated.
		/// </summary>
		/// <param name="method">The name of the method/property to Invoke.</param>
		/// <param name="args">The arguments to pass the method/property.</param>
		/// <param name="proxy">The proxy object that the method is being invoked on.</param>
		/// <returns>
		/// The result of the Invoke if the underlying proxied object is not needed.  If the 
		/// underlying proxied object is needed then it returns the result <see cref="AbstractLazyInitializer.InvokeImplementation"/>
		/// which indicates that the Proxy will need to forward to the real implementation.
		/// </returns>
		public virtual object Invoke(MethodInfo method, object[] args, object proxy)
		{
			string methodName = method.Name;
			int paramCount = method.GetParameters().Length;

			if (paramCount == 0)
			{
				if (!_overridesEquals && methodName == "GetHashCode")
				{
					return IdentityEqualityComparer.GetHashCode(proxy);
				}
				else if (IsEqualToIdentifierMethod(method))
				{
					return Identifier;
				}
				else if (methodName == "Dispose")
				{
					return null;
				}
				else if ("get_HibernateLazyInitializer".Equals(methodName))
				{
					return this;
				}
			}
			else if (paramCount == 1)
			{
				if (!_overridesEquals && methodName == "Equals")
				{
					return IdentityEqualityComparer.Equals(args[0], proxy);
				}
				else if (_setIdentifierMethod != null && method.Equals(_setIdentifierMethod?.Value))
				{
					Initialize();
					Identifier = args[0];
					return InvokeImplementation;
				}
			}
			else if (paramCount == 2)
			{
				// if the Proxy Engine delegates the call of GetObjectData to the Initializer
				// then we need to handle it.  Castle.DynamicProxy takes care of serializing
				// proxies for us, but other providers might not.
				if (methodName == "GetObjectData")
				{
					SerializationInfo info = (SerializationInfo)args[0];
					StreamingContext context = (StreamingContext)args[1]; // not used !?!

					if (Target == null & Session != null)
					{
						EntityKey key = Session.GenerateEntityKey(Identifier, Session.Factory.GetEntityPersister(EntityName));
						object entity = Session.PersistenceContext.GetEntity(key);
						if (entity != null)
							SetImplementation(entity);
					}

					// let the specific ILazyInitializer write its requirements for deserialization 
					// into the stream.
					AddSerializationInfo(info, context);

					// don't need a return value for proxy.
					return null;
				}
			}

			//if it is a property of an embedded component, invoke on the "identifier"
			if (_componentIdType != null && _componentIdType.IsMethodOf(method))
			{
				return method.Invoke(Identifier, args);
			}

			return InvokeImplementation;
		}

		private bool IsEqualToIdentifierMethod(MethodInfo method)
		{
			if (_getIdentifierMethod != null)
			{
				// in the case of inherited identifier methods (from a base class or an iterface) the
				// passed in MethodBase object is not equal to the getIdentifierMethod instance that we
				// have... but if their names and return types are identical, then it is the correct 
				// identifier method
				return method.Name.Equals(_getIdentifierMethod.Value.Name) 
					&& method.ReturnType.Equals(_getIdentifierMethod.Value.ReturnType);
			}

			return false;
		}
	}
}
