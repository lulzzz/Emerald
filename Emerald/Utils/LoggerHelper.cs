using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Emerald.Utils
{
    public static class LoggerHelper
    {
        public static string CreateLogContent(string message)
        {
            return JsonConvert.SerializeObject(new { message }, Formatting.Indented);
        }
        public static async Task<string> CreateLogContent(string message, object parameters, HttpResponseMessage responseMessage)
        {
            return JsonConvert.SerializeObject(new
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
            }, Formatting.Indented);
        }

        private static object BuildHeaderLogObject(HttpHeaders headers)
        {
            var dictionary = new Dictionary<string, List<string>>();

            foreach (var header in headers)
            {
                if (dictionary.ContainsKey(header.Key))
                {
                    dictionary[header.Key].AddRange(header.Value);
                }
                else
                {
                    dictionary.Add(header.Key, new List<string>(header.Value));
                }
            }

            return dictionary;
        }
    }
}