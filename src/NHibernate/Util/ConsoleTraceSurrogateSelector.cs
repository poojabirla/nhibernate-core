using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NHibernate.Util
{
	public class ConsoleTraceSurrogateSelector : SurrogateSelector
	{
		private Dictionary<System.Type, ISerializationSurrogate> _typeSurrogates = new Dictionary<System.Type, ISerializationSurrogate>();
		private HashSet<string> _typeWarnings = new HashSet<string>();

		public ConsoleTraceSurrogateSelector()
		{
			ThrowOnType<System.Reflection.TypeInfo>();
			ThrowOnType<System.Type>();
			//ThrowOnType<System.RuntimeType>();
			ThrowOnType<System.Globalization.CultureInfo>();

			_typeWarnings.Add("System.RuntimeType");
		}

		private void ThrowOnType<T>()
		{
			var type = typeof(T);
			this._typeSurrogates.Add(
				type,
				new ThrowingSerializationSurrogate(type.AssemblyQualifiedName));
			this._typeWarnings.Add(type.FullName);
		}

		public override void AddSurrogate(System.Type type, StreamingContext context, ISerializationSurrogate surrogate)
		{
			Console.WriteLine($"AddSurrogate({type}, {context.State}, {surrogate})");
			base.AddSurrogate(type, context, surrogate);
		}

		public override void ChainSelector(ISurrogateSelector selector)
		{
			Console.WriteLine($"ChainSelector({selector})");
			base.ChainSelector(selector);
		}

		public override ISurrogateSelector GetNextSelector()
		{
			Console.WriteLine($"GetNextSelector()");
			return base.GetNextSelector();
		}

		public override ISerializationSurrogate GetSurrogate(System.Type type, StreamingContext context, out ISurrogateSelector selector)
		{
			ISerializationSurrogate serializationSurrogate;

			if (_typeWarnings.Contains(type.FullName))
			{
				Console.WriteLine($"WARN: GetSurrogate({type}, {{{context.State}, {context.Context ?? "<null>"}}}, out selector)");
			}

			if (_typeSurrogates.TryGetValue(type, out serializationSurrogate))
			{
				selector = this;
			}
			else
			{
				serializationSurrogate = base.GetSurrogate(type, context, out selector);
			}

			Console.WriteLine($"GetSurrogate({type}, {{{context.State}, {context.Context ?? "<null>"}}}, out selector) = {serializationSurrogate}");
			return serializationSurrogate;
		}

		public override void RemoveSurrogate(System.Type type, StreamingContext context)
		{
			Console.WriteLine($"RemoveSurrogate({type}, {context.State})");
			base.RemoveSurrogate(type, context);
		}
	}

	public class ThrowingSerializationSurrogate : ISerializationSurrogate
	{
		private readonly string _assemblyQualifiedName;

		public ThrowingSerializationSurrogate(string assemblyQualifiedName)
		{
			_assemblyQualifiedName = assemblyQualifiedName;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			throw new SerializationException($"Type {_assemblyQualifiedName} is not marked as serializable.  Has value of {obj}");
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			throw new SerializationException($"Type {_assemblyQualifiedName} is not marked as serializable.  Has value of {obj}");
		}
	}
}