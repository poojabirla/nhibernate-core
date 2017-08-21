using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using NHibernate.Exceptions;

namespace NHibernate.Test.ExceptionsTest
{
	public class FbExceptionConverterExample : ISQLExceptionConverter
	{
		#region ISQLExceptionConverter Members

		public Exception Convert(AdoExceptionContextInfo adoExceptionContextInfo)
		{
			var sqle = ADOExceptionHelper.ExtractDbException(adoExceptionContextInfo.SqlException) as DbException;
			if (sqle != null)
			{
#if NETCOREAPP2_0
				// As of v5.9.0.0, for netstandard 1.6, the FbException.ErrorCode is not an override, so we need to directly get it.
				var propertyInfo = sqle.GetType().GetProperty("ErrorCode");
				int sqleErrorCode = (int)(propertyInfo?.GetValue(sqle, null) ?? 0);
#else
				var sqleErrorCode = sqle.ErrorCode;
#endif

				if (sqleErrorCode == 335544466)
				{
					return new ConstraintViolationException(adoExceptionContextInfo.Message, sqle.InnerException, adoExceptionContextInfo.Sql, null);
				}
				if (sqleErrorCode == 335544569)
				{
					return new SQLGrammarException(adoExceptionContextInfo.Message, sqle.InnerException, adoExceptionContextInfo.Sql);
				}
			}
			return SQLStateConverter.HandledNonSpecificException(adoExceptionContextInfo.SqlException, adoExceptionContextInfo.Message, adoExceptionContextInfo.Sql);
		}

		#endregion
	}
}
