﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NUnit.Framework;
using NHibernate.Test.Immutable.EntityWithMutableCollection;

namespace NHibernate.Test.Immutable.EntityWithMutableCollection.NonInverse
{
	using System.Threading.Tasks;
	[TestFixture]
	public class VersionedEntityWithNonInverseManyToManyTestAsync : AbstractEntityWithManyToManyTestAsync
	{
		protected override System.Collections.IList Mappings
		{
			get
			{
				return new string[] { "Immutable.EntityWithMutableCollection.NonInverse.ContractVariationVersioned.hbm.xml" };
			}
		}
	}
}