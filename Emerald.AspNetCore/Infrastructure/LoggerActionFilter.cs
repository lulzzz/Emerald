using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class LoggerActionFilter : IActionFilter
    {
        private readonly ILogger<LoggerActionFilter> _logger;
        private DateTime _startedAt;

        public LoggerActionFilter(ILogger<LoggerActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _startedAt = DateTime.UtcNow;

            context.HttpContext.Request.EnableRewind();

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(string.Join(Environment.NewLine, context.ModelState.Values.SelectMany(i => i.Errors).Select(i => i.ErrorMessage)));
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var requestInfo = CreateRequestInfo(context);
            var responseInfo = CreateResponseInfo(context);
            var isError = context.Exception != null && !context.ExceptionHandled;

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(isError ? "New request handled with error." : "New request handled.");
            messageBuilder.AppendLine(requestInfo);
            messageBuilder.AppendLine(responseInfo);

            var message = messageBuilder.ToString();

            if (isError)
            {
                _logger.LogError(context.Exception, message);
            }
            else
            {
                _logger.LogInformation(message);
            }
        }

        private string CreateRequestInfo(ActionExecutedContext context)
        {
            var info = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";

            if (context.HttpContext.Request.Method != HttpMethod.Get.ToString() && (context.HttpContext.Request.ContentType?.Contains("application/json") ?? false))
            {
                context.HttpContext.Request.Body.Position = 0;
                info += $" {new StreamReader(context.HttpContext.Request.Body).ReadToEnd()}";
            }

            info = "Request: " + info;

            return info;
        }
        private string CreateResponseInfo(ActionExecutedContext context)
        {
            var info = string.Empty;

            if (context.Result is ObjectResult objectResult)
            {
                info += $"{objectResult.StatusCode}";
                if (objectResult.StatusCode < 200 || objectResult.StatusCode >= 300 && objectResult.Value != null) info += $", {JsonConvert.SerializeObject(objectResult.Value)}";
            }
            else if (context.Result is StatusCodeResult statusCodeResult)
            {
                info += $"{statusCodeResult.StatusCode}";
            }

            info += $"{(info == string.Empty ? string.Empty : ", ")}{(DateTime.UtcNow - _startedAt).TotalMilliseconds}ms";
            info = "Response: " + info;

            return info;
        }
    }
}