using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Emerald.AspNetCore.Routing
{
    internal sealed class HttpRouter
    {
        private readonly Dictionary<string, string> _routeDictionary;

        public HttpRouter(Dictionary<string, string> routeDictionary)
        {
            _routeDictionary = routeDictionary;
        }

        public async Task Route(HttpContext httpContext)
        {
            var path = httpContext.Request.GetUri().GetComponents(UriComponents.Path, UriFormat.UriEscaped);

            if (!_routeDictionary.ContainsKey(path))
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var host = _routeDictionary[path];
            var routePath = $"{host}{httpContext.Request.GetUri().GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped)}";

            using (var httpClient = new HttpClient())
            {
                var httpMethod = new HttpMethod(httpContext.Request.Method);
                var requestMessage = new HttpRequestMessage(httpMethod, routePath);

                if (httpMethod != HttpMethod.Get)
                {
                    var content = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
                    requestMessage.Content = new StringContent(content);
                }

                var responseMessage = await httpClient.SendAsync(requestMessage);

                httpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                httpContext.Response.Body = await responseMessage.Content.ReadAsStreamAsync();
            }
        }
    }
}