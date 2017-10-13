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
using System.Data;
using System.Data.Common;

using NHibernate.Engine;
using NHibernate.Impl;

namespace NHibernate.Transaction
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class AdoTransaction : ITransaction
	{

		private async Task AfterTransactionCompletionAsync(bool successful, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			using (new SessionIdLoggingContext(sessionId))
			{
				session.ConnectionManager.AfterTransaction();
				await (session.AfterTransactionCompletionAsync(successful, this, cancellationToken)).ConfigureAwait(false);
				NotifyLocalSynchsAfterTransactionCompletion(successful);
				foreach (var dependentSession in session.ConnectionManager.DependentSessions)
					await (dependentSession.AfterTransactionCompletionAsync(successful, this, cancellationToken)).ConfigureAwait(false);

				session = null;
				begun = false;
			}
		}

		/// <summary>
		/// Commits the <see cref="ITransaction"/> by flushing asynchronously the <see cref="ISession"/>
		/// then committing synchronously the <see cref="DbTransaction"/>.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <exception cref="TransactionException">
		/// Thrown if there is any exception while trying to call <c>Commit()</c> on 
		/// the underlying <see cref="DbTransaction"/>.
		/// </exception>
		public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			using (new SessionIdLoggingContext(sessionId))
			{
				CheckNotDisposed();
				CheckBegun();
				CheckNotZombied();

				log.Debug("Start Commit");

				await (session.BeforeTransactionCompletionAsync(this, cancellationToken)).ConfigureAwait(false);
				NotifyLocalSynchsBeforeTransactionCompletion();
				foreach (var dependentSession in session.ConnectionManager.DependentSessions)
					await (dependentSession.BeforeTransactionCompletionAsync(this, cancellationToken)).ConfigureAwait(false);

				try
				{
					trans.Commit();
					log.Debug("DbTransaction Committed");

					committed = true;
					await (AfterTransactionCompletionAsync(true, cancellationToken)).ConfigureAwait(false);
					Dispose();
				}
				catch (HibernateException e)
				{
					log.Error(e, "Commit failed");
					await (AfterTransactionCompletionAsync(false, cancellationToken)).ConfigureAwait(false);
					commitFailed = true;
					// Don't wrap HibernateExceptions
					throw;
				}
				catch (Exception e)
				{
					log.Error(e, "Commit failed");
					await (AfterTransactionCompletionAsync(false, cancellationToken)).ConfigureAwait(false);
					commitFailed = true;
					throw new TransactionException("Commit failed with SQL exception", e);
				}
				finally
				{
					CloseIfRequired();
				}
			}
		}

		/// <summary>
		/// Rolls back the <see cref="ITransaction"/> by calling the method <c>Rollback</c> 
		/// on the underlying <see cref="DbTransaction"/>.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <exception cref="TransactionException">
		/// Thrown if there is any exception while trying to call <c>Rollback()</c> on 
		/// the underlying <see cref="DbTransaction"/>.
		/// </exception>
		public async Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			using (new SessionIdLoggingContext(sessionId))
			{
				CheckNotDisposed();
				CheckBegun();
				CheckNotZombied();

				log.Debug("Rollback");

				if (!commitFailed)
				{
					try
					{
						trans.Rollback();
						log.Debug("DbTransaction RolledBack");
						rolledBack = true;
						Dispose();
					}
					catch (HibernateException e)
					{
						log.Error(e, "Rollback failed");
						// Don't wrap HibernateExceptions
						throw;
					}
					catch (Exception e)
					{
						log.Error(e, "Rollback failed");
						throw new TransactionException("Rollback failed with SQL Exception", e);
					}
					finally
					{
						await (AfterTransactionCompletionAsync(false, cancellationToken)).ConfigureAwait(false);
						CloseIfRequired();
					}
				}
			}
		}

		#region System.IDisposable Members

		/// <summary>
		/// Takes care of freeing the managed and unmanaged resources that 
		/// this class is responsible for.
		/// </summary>
		/// <param name="isDisposing">Indicates if this AdoTransaction is being Disposed of or Finalized.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <remarks>
		/// If this AdoTransaction is being Finalized (<c>isDisposing==false</c>) then make sure not
		/// to call any methods that could potentially bring this AdoTransaction back to life.
		/// </remarks>
		protected virtual async Task DisposeAsync(bool isDisposing, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			using (new SessionIdLoggingContext(sessionId))
			{
				if (_isAlreadyDisposed)
				{
					// don't dispose of multiple times.
					return;
				}

				// free managed resources that are being managed by the AdoTransaction if we
				// know this call came through Dispose()
				if (isDisposing)
				{
					if (trans != null)
					{
						trans.Dispose();
						trans = null;
						log.Debug("DbTransaction disposed.");
					}

					if (IsActive && session != null)
					{
						// Assume we are rolled back
						await (AfterTransactionCompletionAsync(false, cancellationToken)).ConfigureAwait(false);
					}
				}

				// free unmanaged resources here

				_isAlreadyDisposed = true;
				// nothing for Finalizer to do - so tell the GC to ignore it
				GC.SuppressFinalize(this);
			}
		}

		#endregion
	}
}
