using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System;
using System.ComponentModel;
using System.Data.SqlClient;

namespace Emerald.AspNetCore.EntityFrameworkCore
{
    public sealed class SqlAzureCommandExecutionStrategy : Emerald.System.CommandExecutionStrategy
    {
        protected override bool ShouldRetryOn(Exception exception)
        {
            return IsSqlAzureException(exception);
        }

        private static bool IsSqlAzureException(Exception exception)
        {
            if (exception == null) return false;

            if (exception is SqlException)
            {
                return SqlServerTransientExceptionDetector.ShouldRetryOn(exception);
            }

            if (exception is Win32Exception && exception.Message == "Unknown error 258")
            {
                return true;
            }

            return IsSqlAzureException(exception.InnerException);
        }
    }
}