using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    /// <summary>
    /// Interceptor to audit SQL commands executed by Entity Framework
    /// </summary>
    public class SqlCommandAuditInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<SqlCommandAuditInterceptor>? _logger;
        private static readonly AsyncLocal<bool> _suppressAudit = new AsyncLocal<bool>();

        public SqlCommandAuditInterceptor(ILogger<SqlCommandAuditInterceptor>? logger = null)
        {
            _logger = logger;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            LogCommand(command, eventData);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, eventData);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            LogCommand(command, eventData);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, eventData);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
        {
            LogCommand(command, eventData);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, eventData);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void LogCommand(DbCommand command, CommandEventData eventData)
        {
            // Prevent infinite recursion when saving audit logs
            if (_suppressAudit.Value)
            {
                return;
            }

            try
            {
                var commandText = command.CommandText;
                
                // Skip auditing of audit log inserts to prevent infinite recursion
                if (commandText.Contains("\"AuditLogs\"", StringComparison.OrdinalIgnoreCase) ||
                    commandText.Contains("audit_logs", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Collect parameters
                var parameters = new Dictionary<string, object?>();
                foreach (DbParameter param in command.Parameters)
                {
                    parameters[param.ParameterName] = param.Value;
                }

                var parametersJson = parameters.Count > 0 ? JsonSerializer.Serialize(parameters) : null;

                // Log to standard logger for immediate visibility
                // Database persistence of SQL audits should be implemented using a background
                // service with a queue to avoid recursion and performance issues
                _logger?.LogInformation("SQL Command: {Command}, Parameters: {Parameters}", 
                    commandText, parametersJson ?? "None");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error logging SQL command");
            }
        }

        /// <summary>
        /// Suppress audit logging temporarily (used when inserting audit logs).
        /// This is for future use when SQL audit logs are persisted to the database.
        /// Usage: using (SqlCommandAuditInterceptor.SuppressAudit()) { ... }
        /// </summary>
        public static IDisposable SuppressAudit()
        {
            _suppressAudit.Value = true;
            return new SuppressAuditScope();
        }

        private class SuppressAuditScope : IDisposable
        {
            public void Dispose()
            {
                _suppressAudit.Value = false;
            }
        }
    }
}
