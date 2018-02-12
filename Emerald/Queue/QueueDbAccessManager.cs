using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private const string RegisterSubscriberQuery = "IF (SELECT COUNT(*) FROM [dbo].[Subscribers] WHERE [Name] = '{0}') = 0 INSERT INTO [dbo].[Subscribers] ([Name], [LastReadAt], [LastReadEventId]) VALUES ('{0}', GETUTCDATE(), (SELECT COALESCE(MAX([Id]), 0) FROM [dbo].[Events]))";
        private const string LastEventIdQuery = "SELECT [LastReadEventId] FROM [dbo].[Subscribers] WHERE [Name] = '{0}'";
        private const string EventListQuery = "SELECT [Id], [Type], [Body] FROM [dbo].[Events] WHERE [Id] > {0} ORDER BY [Id]";
        private const string UpdateLastEventIdQuery = "UPDATE [dbo].[Subscribers] SET [LastReadEventId] = {0} WHERE [Name] = '{1}'";
        private const string UpdateLastReadAtQuery = "UPDATE [dbo].[Subscribers] SET [LastReadAt] = '{0}' WHERE [Name] = '{1}'";
        private const string InsertEventQuery = "INSERT INTO [dbo].[Events] ([Type], [Body], [Source], [PublishedAt]) VALUES ('{0}', '{1}', '{2}', '{3}')";

        public QueueDbAccessManager(string applicationName, string connectionString)
        {
            _applicationName = applicationName;
            _connectionString = connectionString;
        }

        public void CreateQueueDbIfNeeded()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            var dbName = connectionStringBuilder.InitialCatalog;
            var query = string.Format(CreateDbQuery, dbName);

            connectionStringBuilder.InitialCatalog = "master";

            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var createEventTableCommand = new SqlCommand(CreateEventTableQuery, connection))
            using (var createSubscriberTableCommand = new SqlCommand(CreateSubscriberTableQuery, connection))
            {
                connection.Open();
                createEventTableCommand.ExecuteNonQuery();
                createSubscriberTableCommand.ExecuteNonQuery();
            }
        }

        public void RegisterSubscriberIfNeeded()
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(string.Format(RegisterSubscriberQuery, _applicationName), connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
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
        public IEnumerable<object> GetEvents()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();

                try
                {
                    var lastEventIdCommand = new SqlCommand(string.Format(LastEventIdQuery, _applicationName), connection, transaction);
                    var lastEventIdCommandResult = lastEventIdCommand.ExecuteScalar();
                    var lastEventId = Convert.ToInt64(lastEventIdCommandResult);

                    var eventListCommand = new SqlCommand(string.Format(EventListQuery, lastEventId), connection, transaction);
                    var eventListReader = eventListCommand.ExecuteReader();
                    var eventList = new List<object>();

                    while (eventListReader.Read())
                    {
                        var id = eventListReader.GetInt32(0);
                        var typeName = eventListReader.GetString(1);
                        var body = eventListReader.GetString(2);

                        lastEventId = id;
                        var type = Type.GetType(typeName);
                        var @event = JsonConvert.DeserializeObject(body, type);

                        eventList.Add(@event);
                    }

                    if (eventList.Count > 0)
                    {
                        var updateLastEventIdCommand = new SqlCommand(string.Format(UpdateLastEventIdQuery, lastEventId, _applicationName), connection, transaction);
                        updateLastEventIdCommand.ExecuteNonQuery();
                    }

                    var updateLastReadAtCommand = new SqlCommand(string.Format(UpdateLastReadAtQuery, $"{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss tt}", _applicationName), connection, transaction);
                    updateLastReadAtCommand.ExecuteNonQuery();

                    transaction.Commit();

                    return eventList;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}