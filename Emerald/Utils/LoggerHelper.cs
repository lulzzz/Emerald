using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Emerald.Utils
{
    public static class LoggerHelper
    {
        public static async Task<object> CreateLogContent(HttpRequestMessage request, bool includeContent = true, bool includeHeaders = true)
        {
            return request == null ? null : new
            {
                method = request.Method.ToString(),
                uri = request.RequestUri.ToString(),
                content = request.Content != null && includeContent ? JsonHelper.TryParse(await request.Content.ReadAsStringAsync()) : null,
                headers = includeHeaders ? CreateLogContent(request.Headers) : null
            };
        }
        public static async Task<object> CreateLogContent(HttpResponseMessage response, bool includeContent = true, bool includeHeaders = true)
        {
            return response == null ? null : new
            {
                statusCode = response.StatusCode.ToString(),
                content = response.Content != null && includeContent ? JsonHelper.TryParse(await response.Content.ReadAsStringAsync()) : null,
                headers = includeHeaders ? CreateLogContent(response.Headers) : null
            };
        }
        public static object CreateLogContent(HttpHeaders headers)
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