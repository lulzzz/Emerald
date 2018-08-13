using Emerald.AspNetCore.Configuration;
using Emerald.Core;
using Emerald.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
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
        private readonly string _correlationId = Guid.NewGuid().ToString();
        private DateTime _startedAt;

        public LoggerActionFilter(ICommandExecutor commandExecutor, IApplicationConfiguration configuration)
        {
            _commandExecutor = commandExecutor;
            _configuration = configuration;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _startedAt = DateTime.UtcNow;

            context.HttpContext.Request.EnableRewind();

            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(string.Join(Environment.NewLine, context.ModelState.Values.SelectMany(i => i.Errors).Select(i => i.ErrorMessage)));
            }

            _commandExecutor.SetCorrelationId(_correlationId);
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var isError = context.Exception != null && !context.ExceptionHandled;
            var level = isError ? LogEventLevel.Error : LogEventLevel.Information;

            var logContent = new
            {
                message = isError ? "Request handled with errors" : "Request handled successfully.",
                correlationId = _correlationId,
                handlingTime = $"{(DateTime.UtcNow - _startedAt).TotalMilliseconds}ms",
                request = CreateLogContent(context.HttpContext.Request),
                response = CreateLogContent(context.Result),
                commands = _commandExecutor.GetCommands().Select(c => new
                {
                    name = c.GetType().Name,
                    startedAt = c.StartedAt,
                    result = c.Result,
                    consistentHashKey = c.ConsistentHashKey,
                    executionTime = c.ExecutionTime
                })
            }.ToJson(Formatting.Indented);

            Log.Logger.Write(level, context.Exception, "content: {content}, correlationId: {correlationId}", logContent, _correlationId);
        }

        private object CreateLogContent(HttpRequest request)
        {
            var method = request.Method;
            var uri = $"{request.Path}{request.QueryString}";

            object content = null;

            if (request.Method != HttpMethod.Get.ToString() && (request.ContentType?.Contains("application/json") ?? false))
            {
                request.Body.Position = 0;

                try
                {
                    content = JsonConvert.DeserializeObject(new StreamReader(request.Body).ReadToEnd());
                }
                catch
                {
                    content = null;
                }
            }

            return new { method, uri, content };
        }
        private object CreateLogContent(IActionResult actionResult)
        {
            int? statusCode = null;
            object content = null;

            if (actionResult is ObjectResult objectResult)
            {
                statusCode = objectResult.StatusCode;
                if (objectResult.StatusCode < 200 || objectResult.StatusCode >= 300 && objectResult.Value != null) content = objectResult.Value;
                if (string.Equals(_configuration.Environment.Name, EnvironmentName.Development, StringComparison.InvariantCultureIgnoreCase)) content = objectResult.Value;
            }
            else if (actionResult is StatusCodeResult statusCodeResult)
            {
                statusCode = statusCodeResult.StatusCode;
            }

            return content == null ? (object)new { statusCode } : new { statusCode, content };
        }
    }
}