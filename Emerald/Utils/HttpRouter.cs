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
                httpRequestMessage.RequestUri = new Uri($"{_routeTo}{httpRequestMessage.RequestUri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped)}");

                if (httpRequestMessage.Method == HttpMethod.Get && httpRequestMessage.Content != null)
                {
                    httpRequestMessage.Content.Dispose();
                    httpRequestMessage.Content = null;
                }

                return await httpClient.SendAsync(httpRequestMessage);
            }
        }
    }
}