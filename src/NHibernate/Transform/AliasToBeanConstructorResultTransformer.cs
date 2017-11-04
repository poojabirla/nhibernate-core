using System;
using System.Collections;
using System.Reflection;
using NHibernate.Util;

namespace NHibernate.Transform
{
	[Serializable]
	public class AliasToBeanConstructorResultTransformer : IResultTransformer
	{
		private readonly SerializableConstructorInfo _constructor;

		public AliasToBeanConstructorResultTransformer(ConstructorInfo constructor)
		{
			if (constructor == null) throw new ArgumentNullException(nameof(constructor));
			this._constructor = SerializableConstructorInfo.Wrap(constructor);
		}

		public object TransformTuple(object[] tuple, string[] aliases)
		{
			try
			{
				return _constructor.Value.Invoke(tuple);
			}
			catch (Exception e)
			{
				throw new QueryException(
					string.Format("could not instantiate: {0}", _constructor.Value.DeclaringType?.FullName),
					e);
			}
		}

		public IList TransformList(IList collection)
		{
			return collection;
		}

		public bool Equals(AliasToBeanConstructorResultTransformer other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other._constructor, _constructor);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AliasToBeanConstructorResultTransformer);
		}

		public override int GetHashCode()
		{
			return _constructor.GetHashCode();
		}
	}
}
