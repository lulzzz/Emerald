using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;

namespace Emerald.AspNetCore.Filters
{
    internal sealed class EmeraldActionFilter : IActionFilter
    {
        private readonly ILogger<EmeraldActionFilter> _logger;

        public EmeraldActionFilter(ILogger<EmeraldActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Request.EnableRewind();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var request = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            var requestContent = string.Empty;

            if (context.HttpContext.Request.Method != HttpMethod.Get.ToString())
            {
                try
                {
                    context.HttpContext.Request.Body.Position = 0;
                    requestContent = $"{context.HttpContext.Request.ContentType} {new StreamReader(context.HttpContext.Request.Body).ReadToEnd()}";
                }
                catch (NotSupportedException)
                {
                }
            }

            var response = $"{context.HttpContext.Response.StatusCode}";

            if (context.Exception != null && !context.ExceptionHandled)
            {
                _logger.LogError(context.Exception, $"New request handled with error. Request: {request}, RequestContent: {requestContent}, Response: {response}");
            }
            else
            {
                _logger.LogInformation($"New request handled. Request: {request}, RequestContent: {requestContent}, Response: {response}");
            }
        }
    }
}