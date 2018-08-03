using Emerald.Utils;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Emerald.AspNetCore.EntityFrameworkCore
{
    public sealed class SqlAzureCommandExecutionStrategy : Emerald.System.CommandExecutionStrategy
    {
        protected override bool ShouldRetryOn(Exception exception)
        {
            var exceptionList = new List<Exception>();
            var isSqlAzureException = IsSqlAzureException(exception, exceptionList);

            if (!isSqlAzureException)
            {
                Log.Logger.Error(JsonHelper.Serialize(new
                {
                    message = "Exception occured on command execution.",
                    exceptions = exceptionList.Select(ex => new { Type = ex.GetType().FullName, ex.Message }).ToArray()
                }));
            }

            return isSqlAzureException;
        }

        private static bool IsSqlAzureException(Exception exception, List<Exception> exceptions)
        {
            if (exception == null) return false;

            exceptions.Add(exception);

            if (exception is SqlException)
            {
                return SqlServerTransientExceptionDetector.ShouldRetryOn(exception);
            }

            return IsSqlAzureException(exception.InnerException, exceptions);
        }
    }
}