﻿using Emerald.Utils;
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
            "IF OBJECT_ID('Events') IS NULL CREATE TABLE [dbo].[Events] ([Id] INT IDENTITY(1,1) PRIMARY KEY, [Type] NVARCHAR(128) NOT NULL, [Body] NVARCHAR(MAX) NOT NULL, [Source] NVARCHAR(64) NOT NULL, [PublishedAt] DATETIME2(7) NOT NULL, [ConsistentHashKey] NVARCHAR(64) NULL) " +
            "IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Events]') AND [name] = 'ConsistentHashKey') ALTER TABLE [dbo].[Events] ADD [ConsistentHashKey] NVARCHAR(64) NULL " +
            "IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE [name] = 'IX_Events_PublishedAt' AND object_id = OBJECT_ID('Events')) CREATE INDEX [IX_Events_PublishedAt] ON [dbo].[Events] ([PublishedAt]) INCLUDE ([Source]) " +
            "IF OBJECT_ID('Subscribers') IS NULL CREATE TABLE [dbo].[Subscribers] ([Name] NVARCHAR(64) PRIMARY KEY, [LastReadAt] DATETIME2(7) NOT NULL, [LastReadEventId] INT NOT NULL) " +
            "IF NOT EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Subscribers]') AND [name] = 'StartFromEventId') ALTER TABLE [dbo].[Subscribers] ADD [StartFromEventId] INT NOT NULL DEFAULT(0) " +
            "IF OBJECT_ID('Logs') IS NULL CREATE TABLE [dbo].[Logs] ([EventId] INT NOT NULL, [SubscriberName] NVARCHAR(64) NOT NULL, [ProcessedAt] DATETIME2(7) NOT NULL, [Result] NVARCHAR(8) NOT NULL, PRIMARY KEY ([EventId], [SubscriberName])) " +
            "IF EXISTS (SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID(N'[dbo].[Logs]') AND [name] = 'Message') ALTER TABLE [dbo].[Logs] DROP COLUMN [Message] " +
            "IF NOT EXISTS(SELECT * FROM [sys].[indexes] WHERE [name] = 'IX_Logs_Result' AND object_id = OBJECT_ID('Logs')) CREATE INDEX [IX_Logs_Result] ON [dbo].[Logs] ([Result]) ";

        private const string RegisterSubscriberQuery = "IF (SELECT COUNT(*) FROM [dbo].[Subscribers] WHERE [Name] = @Name) = 0 INSERT INTO [dbo].[Subscribers] ([Name], [LastReadAt], [LastReadEventId], [StartFromEventId]) VALUES (@Name, GETUTCDATE(), (SELECT COALESCE(MAX([Id]), 0) FROM [dbo].[Events]), (SELECT COALESCE(MAX([Id]), 0) + 1 FROM [dbo].[Events]))";
        private const string LastEventIdQuery = "SELECT [LastReadEventId] FROM [dbo].[Subscribers] WHERE [Name] = @Name";
        private const string EventListQuery = "SELECT [E].[Id], [E].[Type], [E].[Body], [E].[ConsistentHashKey] FROM [dbo].[Events] AS [E] LEFT JOIN [dbo].[Logs] AS [L] ON [L].[EventId] = [E].[Id] AND [L].[SubscriberName] = @SubscriberName WHERE [E].[Id] > @Id AND [L].[EventId] IS NULL ORDER BY [Id]";
        private const string UpdateLastEventIdQuery = "UPDATE [dbo].[Subscribers] SET [LastReadEventId] = @LastReadEventId WHERE [Name] = @Name";
        private const string UpdateLastReadAtQuery = "UPDATE [dbo].[Subscribers] SET [LastReadAt] = @LastReadAt WHERE [Name] = @Name";
        private const string InsertEventQuery = "INSERT INTO [dbo].[Events] ([Type], [Body], [Source], [PublishedAt], [ConsistentHashKey]) VALUES (@Type, @Body, @Source, GETUTCDATE(), @ConsistentHashKey)";
        private const string InsertLogQuery = "INSERT INTO [dbo].[Logs] ([EventId], [SubscriberName], [ProcessedAt], [Result]) VALUES (@EventId, @SubscriberName, GETUTCDATE(), @Result)";

        public QueueDbAccessManager(string applicationName, string connectionString)
        {
            _applicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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
            }, IsConnectionResiliencyException);

            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(InitializeDbQuery, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }, IsConnectionResiliencyException);
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
            }, IsConnectionResiliencyException);
        }
        public async Task AddEvent(string type, string body, string consistentHashKey)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (body == null) throw new ArgumentNullException(nameof(body));

            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(InsertEventQuery, connection))
                {
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@Body", body);
                    command.Parameters.AddWithValue("@Source", _applicationName);
                    command.Parameters.AddWithValue("@ConsistentHashKey", (object)consistentHashKey ?? DBNull.Value);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }, IsConnectionResiliencyException);
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
                                if (lastEventIdCommandResult == null || lastEventIdCommandResult == DBNull.Value) return new Event[0];
                                lastEventId = Convert.ToInt64(lastEventIdCommandResult);
                            }

                            List<Event> eventList;

                            using (var eventListCommand = new SqlCommand(EventListQuery, connection, transaction))
                            {
                                eventListCommand.Parameters.AddWithValue("@Id", lastEventId);
                                eventListCommand.Parameters.AddWithValue("@SubscriberName", _applicationName);

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
                            try { transaction.Rollback(); } catch (InvalidOperationException) { }
                            throw;
                        }
                    }
                }
            }, IsConnectionResiliencyException);
        }
        public async Task AddLog(long eventId, string result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            await RetryHelper.Execute(async () =>
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(InsertLogQuery, connection))
                {
                    command.Parameters.AddWithValue("@EventId", eventId);
                    command.Parameters.AddWithValue("@SubscriberName", _applicationName);
                    command.Parameters.AddWithValue("@Result", result);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }, IsConnectionResiliencyException);
        }

        private bool IsConnectionResiliencyException(Exception exception)
        {
            if (exception is SqlException sqlException)
            {
                foreach (SqlError error in sqlException.Errors)
                {
                    switch (error.Number)
                    {
                        case 20:
                        case 64:
                        case 233:
                        case 10053:
                        case 10054:
                        case 10060:
                        case 10928:
                        case 10929:
                        case 40197:
                        case 40501:
                        case 40613:
                        case 41301:
                        case 41302:
                        case 41305:
                        case 41325:
                            return true;
                        default:
                            continue;
                    }
                }

                return false;
            }

            return exception is TimeoutException;
        }
    }
}