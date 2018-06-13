using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    internal sealed class QueueDbAccessManager
    {
        private readonly string _applicationName;
        private readonly string _connectionString;

        private const string CreateDbQuery = "IF (SELECT COUNT(*) FROM [dbo].[sysdatabases] WHERE [name] = '{0}') = 0 CREATE DATABASE [{0}]";
        private const string CreateEventTableQuery = "IF OBJECT_ID('Events') IS NULL CREATE TABLE [dbo].[Events] ([Id] INT IDENTITY(1,1) PRIMARY KEY, [Type] NVARCHAR(128) NOT NULL, [Body] NVARCHAR(MAX) NOT NULL, [Source] NVARCHAR(64) NOT NULL, [PublishedAt] DATETIME2(7) NOT NULL)";
        private const string CreateSubscriberTableQuery = "IF OBJECT_ID('Subscribers') IS NULL CREATE TABLE [dbo].[Subscribers] ([Name] NVARCHAR(64) PRIMARY KEY, [LastReadAt] DATETIME2(7) NOT NULL, [LastReadEventId] INT NOT NULL)";
        private const string CreateLogTableQuery = "IF OBJECT_ID('Logs') IS NULL CREATE TABLE [dbo].[Logs] ([EventId] INT NOT NULL, [SubscriberName] NVARCHAR(64) NOT NULL, [ProcessedAt] DATETIME2(7) NOT NULL, [Result] NVARCHAR(8) NOT NULL, [Message] NVARCHAR(1024) NOT NULL, PRIMARY KEY ([EventId], [SubscriberName]))";
        private const string RegisterSubscriberQuery = "IF (SELECT COUNT(*) FROM [dbo].[Subscribers] WHERE [Name] = N'{0}') = 0 INSERT INTO [dbo].[Subscribers] ([Name], [LastReadAt], [LastReadEventId]) VALUES (N'{0}', GETUTCDATE(), (SELECT COALESCE(MAX([Id]), 0) FROM [dbo].[Events]))";
        private const string LastEventIdQuery = "SELECT [LastReadEventId] FROM [dbo].[Subscribers] WHERE [Name] = '{0}'";
        private const string EventListQuery = "SELECT [Id], [Type], [Body], [Source], [PublishedAt] FROM [dbo].[Events] WHERE [Id] > {0} ORDER BY [PublishedAt]";
        private const string UpdateLastEventIdQuery = "UPDATE [dbo].[Subscribers] SET [LastReadEventId] = {0} WHERE [Name] = '{1}'";
        private const string UpdateLastReadAtQuery = "UPDATE [dbo].[Subscribers] SET [LastReadAt] = '{0}' WHERE [Name] = '{1}'";
        private const string InsertEventQuery = "INSERT INTO [dbo].[Events] ([Type], [Body], [Source], [PublishedAt]) VALUES (N'{0}', N'{1}', N'{2}', '{3}')";
        private const string InsertLogQuery = "INSERT INTO [dbo].[Logs] ([EventId], [SubscriberName], [ProcessedAt], [Result], [Message]) VALUES ({0}, N'{1}', '{2}', N'{3}', N'{4}')";

        public QueueDbAccessManager(string applicationName, string connectionString)
        {
            _applicationName = applicationName;
            _connectionString = connectionString;
        }

        public async Task CreateQueueDbIfNeeded()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            var dbName = connectionStringBuilder.InitialCatalog;
            connectionStringBuilder.InitialCatalog = "master";
            var createDbQuery = string.Format(CreateDbQuery, dbName);

            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            using (var createDbCommand = new SqlCommand(createDbQuery, connection))
            {
                await connection.OpenAsync();
                await createDbCommand.ExecuteNonQueryAsync();
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var createEventTableCommand = new SqlCommand(CreateEventTableQuery, connection))
            using (var createSubscriberTableCommand = new SqlCommand(CreateSubscriberTableQuery, connection))
            using (var createLogTableCommand = new SqlCommand(CreateLogTableQuery, connection))
            {
                await connection.OpenAsync();
                await createEventTableCommand.ExecuteNonQueryAsync();
                await createSubscriberTableCommand.ExecuteNonQueryAsync();
                await createLogTableCommand.ExecuteNonQueryAsync();
            }
        }
        public async Task RegisterSubscriberIfNeeded()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(string.Format(RegisterSubscriberQuery, _applicationName), connection))
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task AddEvent(string type, string body)
        {
            var query = string.Format(InsertEventQuery, type, body, _applicationName, $"{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss tt}");

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task<Event[]> GetEvents()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        long lastEventId;
                        using (var lastEventIdCommand = new SqlCommand(string.Format(LastEventIdQuery, _applicationName), connection, transaction))
                        {
                            var lastEventIdCommandResult = await lastEventIdCommand.ExecuteScalarAsync();
                            lastEventId = Convert.ToInt64(lastEventIdCommandResult);
                        }

                        List<Event> eventList;
                        using (var eventListCommand = new SqlCommand(string.Format(EventListQuery, lastEventId), connection, transaction))
                        using (var eventListReader = await eventListCommand.ExecuteReaderAsync())
                        {
                            eventList = new List<Event>();
                            while (await eventListReader.ReadAsync())
                            {
                                eventList.Add(new Event
                                {
                                    Id = eventListReader.GetInt32(0),
                                    Type = eventListReader.GetString(1),
                                    Body = eventListReader.GetString(2),
                                    Source = eventListReader.GetString(3),
                                    PublishedAt = eventListReader.GetDateTime(4)
                                });
                            }
                        }

                        if (eventList.Count > 0)
                        {
                            lastEventId = eventList.Max(i => i.Id);
                            using (var updateLastEventIdCommand = new SqlCommand(string.Format(UpdateLastEventIdQuery, lastEventId, _applicationName), connection, transaction))
                            {
                                await updateLastEventIdCommand.ExecuteNonQueryAsync();
                            }
                        }

                        using (var updateLastReadAtCommand = new SqlCommand(string.Format(UpdateLastReadAtQuery, $"{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss tt}", _applicationName), connection, transaction))
                        {
                            await updateLastReadAtCommand.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();

                        return eventList.ToArray();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task AddLog(long eventId, string result, string message)
        {
            var query = string.Format(InsertLogQuery, eventId, _applicationName, $"{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss tt}", result, message);

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}