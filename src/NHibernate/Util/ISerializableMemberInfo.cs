using System.Reflection;

namespace NHibernate.Util
{
	internal interface ISerializableMemberInfo
	{
		MemberInfo Value { get; }
	}
}