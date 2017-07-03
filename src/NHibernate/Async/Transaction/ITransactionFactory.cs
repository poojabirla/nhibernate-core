﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Engine.Transaction;

namespace NHibernate.Transaction
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial interface ITransactionFactory
	{

		/// <summary>
		/// Execute a work outside of the current transaction (if any).
		/// </summary>
		/// <param name="session">The session for which an isolated work has to be executed.</param>
		/// <param name="work">The work to execute.</param>
		/// <param name="transacted"><see langword="true" /> for encapsulating the work in a dedicated
		/// transaction, <see langword="false" /> for not transacting it.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task ExecuteWorkInIsolationAsync(ISessionImplementor session, IIsolatedWork work, bool transacted, CancellationToken cancellationToken);
	}
}