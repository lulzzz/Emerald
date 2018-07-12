using Emerald.Utils;
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

        private const string InitializeDbQuery =
            "IF OBJECT_ID('Events') IS NULL CREATE TABLE [dbo].[Events] ([Id] INT IDENTITY(1,1) PRIMARY KEY, [Type] NVARCHAR(128) NOT NULL, [Body] NVARCHAR(MAX) NOT NULL, [Source] NVARCHAR(64) NOT NULL, [PublishedAt] DATETIME2(7) NOT NULL) " +
            "IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Events]') AND [name] = 'ConsistentHashKey') ALTER TABLE [dbo].[Events] ADD [ConsistentHashKey] NVARCHAR(64) NULL " +
            "IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Events]') AND [name] = 'CreatedAt') ALTER TABLE [dbo].[Events] ADD [CreatedAt] DATETIME2(7) NULL " +
            "IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE [name] = 'IX_Events_PublishedAt' AND object_id = OBJECT_ID('Events')) CREATE INDEX [IX_Events_PublishedAt] ON [dbo].[Events] ([PublishedAt]) INCLUDE ([Source]) " +
            "IF OBJECT_ID('Subscribers') IS NULL CREATE TABLE [dbo].[Subscribers] ([Name] NVARCHAR(64) PRIMARY KEY, [LastReadAt] DATETIME2(7) NOT NULL, [LastReadEventId] INT NOT NULL) " +
            "IF OBJECT_ID('Logs') IS NULL CREATE TABLE [dbo].[Logs] ([EventId] INT NOT NULL, [SubscriberName] NVARCHAR(64) NOT NULL, [ProcessedAt] DATETIME2(7) NOT NULL, [Result] NVARCHAR(8) NOT NULL, [Message] NVARCHAR(1024) NOT NULL, PRIMARY KEY ([EventId], [SubscriberName])) " +
            "IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Logs]') AND [name] = 'ReadAt') ALTER TABLE [dbo].[Logs] ADD [ReadAt] DATETIME2(7) NULL " +
            "IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Logs]') AND [name] = 'ReceivedAt') ALTER TABLE [dbo].[Logs] ADD [ReceivedAt] DATETIME2(7) NULL " +
            "IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Logs]') AND [name] = 'HandledAt') ALTER TABLE [dbo].[Logs] ADD [HandledAt] DATETIME2(7) NULL " +
            "IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE [name] = 'IX_Logs_Result' AND object_id = OBJECT_ID('Logs')) CREATE INDEX [IX_Logs_Result] ON [dbo].[Logs] ([Result]) ";

        private const string RegisterSubscriberQuery = "IF (SELECT COUNT(*) FROM [dbo].[Subscribers] WHERE [Name] = @Name) = 0 INSERT INTO [dbo].[Subscribers] ([Name], [LastReadAt], [LastReadEventId]) VALUES (@Name, GETUTCDATE(), (SELECT COALESCE(MAX([Id]), 0) FROM [dbo].[Events]))";
        private const string LastEventIdQuery = "SELECT [LastReadEventId] FROM [dbo].[Subscribers] WHERE [Name] = @Name";
        private const string EventListQuery = "SELECT [Id], [Type], [Body], [ConsistentHashKey] FROM [dbo].[Events] WHERE [Id] > @Id ORDER BY [PublishedAt]";
        private const string UpdateLastEventIdQuery = "UPDATE [dbo].[Subscribers] SET [LastReadEventId] = @LastReadEventId WHERE [Name] = @Name";
        private const string UpdateLastReadAtQuery = "UPDATE [dbo].[Subscribers] SET [LastReadAt] = @LastReadAt WHERE [Name] = @Name";
        private const string InsertEventQuery = "INSERT INTO [dbo].[Events] ([Type], [Body], [Source], [PublishedAt], [ConsistentHashKey], [CreatedAt]) VALUES (@Type, @Body, @Source, GETUTCDATE(), @ConsistentHashKey, @CreatedAt)";
        private const string InsertLogQuery = "INSERT INTO [dbo].[Logs] ([EventId], [SubscriberName], [ProcessedAt], [Result], [Message], [ReadAt], [ReceivedAt], [HandledAt]) VALUES (@EventId, @SubscriberName, GETUTCDATE(), @Result, @Message, @ReadAt, @ReceivedAt, @HandledAt)";

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

            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
                using (var createDbCommand = new SqlCommand(createDbQuery, connection))
                {
                    await connection.OpenAsync();
                    await createDbCommand.ExecuteNonQueryAsync();
                }
            });

            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(InitializeDbQuery, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            });
        }
        public async Task RegisterSubscriberIfNeeded()
        {
            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(RegisterSubscriberQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", _applicationName);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            });
        }
        public async Task AddEvent(string type, string body, string consistentHashKey)
        {
            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(InsertEventQuery, connection))
                {
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@Body", body);
                    command.Parameters.AddWithValue("@Source", _applicationName);
                    command.Parameters.AddWithValue("@ConsistentHashKey", consistentHashKey);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            });
        }
        public async Task<Event[]> GetEvents()
        {
            return await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            long lastEventId;
                            using (var lastEventIdCommand = new SqlCommand(LastEventIdQuery, connection, transaction))
                            {
                                lastEventIdCommand.Parameters.AddWithValue("@Name", _applicationName);
                                var lastEventIdCommandResult = await lastEventIdCommand.ExecuteScalarAsync();
                                lastEventId = Convert.ToInt64(lastEventIdCommandResult);
                            }

                            List<Event> eventList;
                            using (var eventListCommand = new SqlCommand(EventListQuery, connection, transaction))
                            {
                                eventListCommand.Parameters.AddWithValue("@Id", lastEventId);
                                using (var eventListReader = await eventListCommand.ExecuteReaderAsync())
                                {
                                    eventList = new List<Event>();
                                    while (await eventListReader.ReadAsync())
                                    {
                                        var id = eventListReader.GetInt32(0);
                                        var type = eventListReader.GetString(1);
                                        var body = eventListReader.GetString(2);
                                        var consistentHashKey = await eventListReader.IsDBNullAsync(3) ? null : eventListReader.GetString(3);
                                        eventList.Add(new Event(id, type, body, consistentHashKey, DateTime.UtcNow));
                                    }
                                }
                            }

                            if (eventList.Count > 0)
                            {
                                lastEventId = eventList.Max(i => i.Id);
                                using (var updateLastEventIdCommand = new SqlCommand(UpdateLastEventIdQuery, connection, transaction))
                                {
                                    updateLastEventIdCommand.Parameters.AddWithValue("@LastReadEventId", lastEventId);
                                    updateLastEventIdCommand.Parameters.AddWithValue("@Name", _applicationName);
                                    await updateLastEventIdCommand.ExecuteNonQueryAsync();
                                }
                            }

                            using (var updateLastReadAtCommand = new SqlCommand(UpdateLastReadAtQuery, connection, transaction))
                            {
                                updateLastReadAtCommand.Parameters.AddWithValue("@LastReadAt", DateTime.UtcNow);
                                updateLastReadAtCommand.Parameters.AddWithValue("@Name", _applicationName);
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
            });
        }
        public async Task AddLog(long eventId, string result, string message, DateTime readAt, DateTime receivedAt, DateTime handledAt)
        {
            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(InsertLogQuery, connection))
                {
                    command.Parameters.AddWithValue("@EventId", eventId);
                    command.Parameters.AddWithValue("@SubscriberName", _applicationName);
                    command.Parameters.AddWithValue("@Result", result);
                    command.Parameters.AddWithValue("@Message", message);
                    command.Parameters.AddWithValue("@ReadAt", readAt);
                    command.Parameters.AddWithValue("@ReceivedAt", receivedAt);
                    command.Parameters.AddWithValue("@HandledAt", handledAt);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            });
        }
    }
}