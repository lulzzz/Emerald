using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Emerald.Queue
{
    public sealed class QueueDbAccessManager
    {
        private readonly string _applicationName;
        private readonly string _connectionString;

        private const string CreateDbQuery = "IF (SELECT COUNT(*) FROM [dbo].[sysdatabases] WHERE [name] = '{0}') = 0 CREATE DATABASE [{0}]";
        private const string CreateEventTableQuery = "IF OBJECT_ID('Events') IS NULL CREATE TABLE [dbo].[Events] ([Id] INT IDENTITY(1,1) PRIMARY KEY, [Type] NVARCHAR(128) NOT NULL, [Body] NVARCHAR(MAX) NOT NULL, [Source] NVARCHAR(64) NOT NULL, [PublishedAt] DATETIME2(7) NOT NULL)";
        private const string CreateSubscriberTableQuery = "IF OBJECT_ID('Subscribers') IS NULL CREATE TABLE [dbo].[Subscribers] ([Name] NVARCHAR(64) PRIMARY KEY, [LastReadAt] DATETIME2(7) NOT NULL, [LastReadEventId] INT NOT NULL)";
        private const string CreateLogTableQuery = "IF OBJECT_ID('Logs') IS NULL CREATE TABLE [dbo].[Logs] ([EventId] INT NOT NULL, [SubscriberName] NVARCHAR(64) NOT NULL, [ProcessedAt] DATETIME2(7) NOT NULL, [Result] NVARCHAR(8) NOT NULL, [Message] NVARCHAR(1024) NOT NULL, PRIMARY KEY ([EventId], [SubscriberName]))";
        private const string RegisterSubscriberQuery = "IF (SELECT COUNT(*) FROM [dbo].[Subscribers] WHERE [Name] = @Name) = 0 INSERT INTO [dbo].[Subscribers] ([Name], [LastReadAt], [LastReadEventId]) VALUES (@Name, GETUTCDATE(), (SELECT COALESCE(MAX([Id]), 0) FROM [dbo].[Events]))";
        private const string LastEventIdQuery = "SELECT [LastReadEventId] FROM [dbo].[Subscribers] WHERE [Name] = @Name";
        private const string EventListQuery = "SELECT [Id], [Type], [Body], [Source], [PublishedAt] FROM [dbo].[Events] WHERE [Id] > @Id ORDER BY [PublishedAt]";
        private const string UpdateLastEventIdQuery = "UPDATE [dbo].[Subscribers] SET [LastReadEventId] = @LastReadEventId WHERE [Name] = @Name";
        private const string UpdateLastReadAtQuery = "UPDATE [dbo].[Subscribers] SET [LastReadAt] = @LastReadAt WHERE [Name] = @Name";
        private const string InsertEventQuery = "INSERT INTO [dbo].[Events] ([Type], [Body], [Source], [PublishedAt]) VALUES (@Type, @Body, @Source, @PublishedAt)";
        private const string InsertLogQuery = "INSERT INTO [dbo].[Logs] ([EventId], [SubscriberName], [ProcessedAt], [Result], [Message]) VALUES (@EventId, @SubscriberName, @ProcessedAt, @Result, @Message)";

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
            using (var command = new SqlCommand(RegisterSubscriberQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", _applicationName);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
        public async Task AddEvent(string type, string body)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(InsertEventQuery, connection))
            {
                command.Parameters.AddWithValue("@Type", type);
                command.Parameters.AddWithValue("@Body", body);
                command.Parameters.AddWithValue("@Source", _applicationName);
                command.Parameters.AddWithValue("@PublishedAt", DateTime.UtcNow);
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
                                    var source = eventListReader.GetString(3);
                                    var publishedAt = eventListReader.GetDateTime(4);
                                    eventList.Add(new Event(id, type, body, source, publishedAt));
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
        }
        public async Task AddLog(long eventId, string result, string message)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(InsertLogQuery, connection))
            {
                command.Parameters.AddWithValue("@EventId", eventId);
                command.Parameters.AddWithValue("@SubscriberName", _applicationName);
                command.Parameters.AddWithValue("@ProcessedAt", DateTime.UtcNow);
                command.Parameters.AddWithValue("@Result", result);
                command.Parameters.AddWithValue("@Message", message);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}