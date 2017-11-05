using System;
using System.Runtime.Serialization;
using System.Security;

namespace NHibernate.Util
{
	[Serializable]
	internal sealed class SerializableSystemType : ISerializable
	{
		[NonSerialized]
		private readonly System.Type _type;

		[NonSerialized]
		private readonly Lazy<System.Type> _lazyType;

		private AssemblyQualifiedTypeName _typeName;

		/// <summary>
		/// Creates a new instance of <see cref="SerializableSystemType"/> if 
		/// <paramref name="type"/> is not null, otherwise returns <c>null</c>.
		/// </summary>
		/// <param name="type">The <see cref="System.Type"/> being wrapped for serialization.</param>
		/// <returns>New instance of <see cref="SerializableSystemType"/> or <c>null</c>.</returns>
		public static SerializableSystemType Wrap(System.Type type)
		{
			return type == null ? null : new SerializableSystemType(type);
		}

		/// <summary>
		/// Creates a new <see cref="SerializableSystemType"/>
		/// </summary>
		/// <param name="type">The <see cref="System.Type"/> being wrapped for serialization.</param>
		private SerializableSystemType(System.Type type)
		{
			_type = type ?? throw new ArgumentNullException(nameof(type));
			_typeName = null;
			_lazyType = null;
		}

		private SerializableSystemType(SerializationInfo info, StreamingContext context)
		{
			_type = null;
			_typeName = info.GetValue<AssemblyQualifiedTypeName>("_typeName");
			_lazyType = new Lazy<System.Type>(() => _typeName?.TypeFromAssembly(false));
		}

		/// <summary>
		/// Returns the type, using reflection if necessary to load.  Will throw if unable to load.
		/// </summary>
		/// <returns>The type that this class was initialized with or initialized after deserialization.</returns>
		public new System.Type GetType() => _type ?? _lazyType.Value ?? throw new TypeLoadException("Could not load type " + _typeName + ".");

		/// <summary>
		/// Returns the type, using reflection if necessary to load.  Will return null if unable to load.
		/// </summary>
		/// <returns>The type that this class was initialized with, the type initialized after deserialization, or null if unable to load.</returns>
		public System.Type TryGetType() => _type ?? _lazyType.Value;

		public string FullName => _type?.FullName ?? _typeName.Type;

		public string AssemblyQualifiedName => _type?.AssemblyQualifiedName ?? _typeName.ToString();

		[SecurityCritical]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (_typeName == null)
			{
				_typeName = new AssemblyQualifiedTypeName(_type.FullName, _type.Assembly.FullName);
			}

			info.AddValue("_typeName", _typeName);
		}

		private bool Equals(SerializableSystemType other)
		{
			return TryGetType() == null || other.TryGetType() == null
				? Equals(_typeName, other._typeName)
				: Equals(GetType(), other.GetType());
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is SerializableSystemType && Equals((SerializableSystemType) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (FullName.GetHashCode() * 397) ^ (AssemblyQualifiedName?.GetHashCode() ?? 0);
			}
		}

		public static bool operator ==(SerializableSystemType left, SerializableSystemType right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(SerializableSystemType left, SerializableSystemType right)
		{
			return !Equals(left, right);
		}

		public static explicit operator System.Type(SerializableSystemType serializableType)
		{
			return serializableType?.GetType();
		}

		public static implicit operator SerializableSystemType(System.Type type)
		{
			return Wrap(type);
		}
	}
}
