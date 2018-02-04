using System;
using System.Net.Http;
using System.Text;
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
                var requestMessage = new HttpRequestMessage(httpRequestMessage.Method, requestUrl);

                if (httpRequestMessage.Method != HttpMethod.Get)
                {
                    requestMessage.Content = new StringContent(await httpRequestMessage.Content.ReadAsStringAsync(), Encoding.UTF8, "application/json");
                }

                return await httpClient.SendAsync(requestMessage);
            }
        }
    }
}