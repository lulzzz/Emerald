using Emerald.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Emerald.Utils
{
    public static class LoggerHelper
    {
        public static string CreateLog(string message)
        {
            return new { message }.ToJson(Formatting.Indented);
        }

        public static async Task<string> CreateLog(HttpResponseMessage responseMessage)
        {
            var log = new
            {
                statusCode = responseMessage.StatusCode,
                content = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : string.Empty,
                headers = BuildHeaderLogObject(responseMessage.Headers)
            };

            return log.ToJson(Formatting.Indented);
        }
        public static async Task<string> CreateLog(HttpRequestMessage requestMessage)
        {
            var log = new
            {
                method = requestMessage.Method.ToString(),
                uri = requestMessage.RequestUri.ToString(),
                content = requestMessage.Content != null ? await requestMessage.Content.ReadAsStringAsync() : string.Empty,
                headers = BuildHeaderLogObject(requestMessage.Headers)
            };

            return log.ToJson(Formatting.Indented);
        }

        public static object CreateLogObject(ICommandInfo[] commands)
        {
            var log = commands.Select(c => new
            {
                name = c.GetType().Name,
                startedAt = c.StartedAt,
                result = c.Result,
                consistentHashKey = c.ConsistentHashKey,
                executionTime = c.ExecutionTime
            });

            return log;
        }

        public static async Task<string> CreateLogContent(string message, object parameters, HttpResponseMessage responseMessage)
        {
            return JsonHelper.Serialize(new
            {
                message,
                parameters,
                request = new
                {
                    method = responseMessage.RequestMessage.Method.ToString(),
                    uri = responseMessage.RequestMessage.RequestUri.ToString(),
                    content = responseMessage.RequestMessage.Content != null ? await responseMessage.RequestMessage.Content.ReadAsStringAsync() : string.Empty,
                    headers = BuildHeaderLogObject(responseMessage.RequestMessage.Headers)
                },
                response = new
                {
                    statusCode = responseMessage.StatusCode,
                    content = responseMessage.Content != null ? await responseMessage.Content.ReadAsStringAsync() : string.Empty,
                    headers = BuildHeaderLogObject(responseMessage.Headers)
                }
            });
        }

        private static object BuildHeaderLogObject(HttpHeaders headers)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                if (dictionary.ContainsKey(header.Key))
                {
                    dictionary[header.Key] = $"{dictionary[header.Key]}{(ValidationHelper.IsNullOrEmptyOrWhiteSpace(dictionary[header.Key]) ? "" : " ")}{string.Join(" ", header.Value)}";
                }
                else
                {
                    dictionary.Add(header.Key, string.Join(" ", header.Value));
                }
            }

            return dictionary;
        }
    }
}