using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Emerald.Utils
{
    public sealed class HttpRouter
    {
        private readonly string _routeTo;

        public HttpRouter(string routeTo)
        {
            _routeTo = routeTo;
        }

        public async Task<HttpResponseMessage> Route(HttpRequestMessage httpRequestMessage)
        {
            using (var httpClient = new HttpClient())
            {
                var requestUrl = $"{_routeTo}{httpRequestMessage.RequestUri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped)}";
                httpRequestMessage.RequestUri = new Uri(requestUrl);
                return await httpClient.SendAsync(httpRequestMessage);
            }
        }
    }
}