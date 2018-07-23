using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Emerald.AspNetCore.Filters
{
    internal sealed class LoggerActionFilter : IActionFilter
    {
        private readonly ICommandExecutor _commandExecutor;
        private readonly IApplicationConfiguration _configuration;
        private readonly ILogger<LoggerActionFilter> _logger;
        private DateTime _startedAt;

        public LoggerActionFilter(ICommandExecutor commandExecutor, IApplicationConfiguration configuration, ILogger<LoggerActionFilter> logger)
        {
            _commandExecutor = commandExecutor;
            _configuration = configuration;
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
            var isError = context.Exception != null && !context.ExceptionHandled;

            var log = new
            {
                message = isError ? "Request handled with error." : "Request handled.",
                request = CreateRequestLogObject(context),
                response = CreateResponseLogObject(context),
                commands = LoggerHelper.CreateLogObject(_commandExecutor.GetCommands())
            };

            _logger.Log(isError ? LogLevel.Error : LogLevel.Information, log.ToJson(Formatting.Indented));
        }

        private object CreateRequestLogObject(ActionExecutedContext context)
        {
            var method = context.HttpContext.Request.Method;
            var uri = $"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";

            string content = null;

            if (context.HttpContext.Request.Method != HttpMethod.Get.ToString() && (context.HttpContext.Request.ContentType?.Contains("application/json") ?? false))
            {
                context.HttpContext.Request.Body.Position = 0;
                content = $"{new StreamReader(context.HttpContext.Request.Body).ReadToEnd()}";
            }

            return new { method, uri, content };
        }
        private object CreateResponseLogObject(ActionExecutedContext context)
        {
            int? statusCode = null;
            object content = null;

            if (context.Result is ObjectResult objectResult)
            {
                statusCode = objectResult.StatusCode;
                if (objectResult.StatusCode < 200 || objectResult.StatusCode >= 300 && objectResult.Value != null) content = objectResult.Value;
                if (string.Equals(_configuration.Environment.Name, EnvironmentName.Development, StringComparison.InvariantCultureIgnoreCase)) content = objectResult.Value;
            }
            else if (context.Result is StatusCodeResult statusCodeResult)
            {
                statusCode = statusCodeResult.StatusCode;
            }

            var responseTime = $"{(DateTime.UtcNow - _startedAt).TotalMilliseconds}ms";

            return content == null ? (object)new { statusCode, responseTime } : new { statusCode, content, responseTime };
        }
    }
}