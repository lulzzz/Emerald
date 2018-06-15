using Emerald.Application;
using Emerald.AspNetCore.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Emerald.AspNetCore.Infrastructure
{
    internal sealed class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuthenticationService _authenticationService;
        private readonly IApplicationConfiguration _configuration;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(
            RequestDelegate next,
            IAuthenticationService authenticationService,
            IApplicationConfiguration configuration,
            ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _authenticationService = authenticationService;
            _configuration = configuration;
            _logger = logger;
        }

        public Task Invoke(HttpContext context)
        {
            var startedAt = DateTime.UtcNow;

            if (context.Request.Path.Value != "/api/tokens") return _next(context);
            if (context.Request.Method != HttpMethods.Post) return WriteBadRequest(context, null, startedAt);
            if (context.Request.ContentType != "application/json") return WriteBadRequest(context, null, startedAt);

            context.Request.EnableRewind();
            context.Request.Body.Position = 0;
            var requestBody = $" {new StreamReader(context.Request.Body).ReadToEnd()}";

            var authenticationRequestBody = JsonConvert.DeserializeObject<AuthenticationRequestBody>(requestBody);
            if (authenticationRequestBody == null) return WriteBadRequest(context, null, startedAt);
            if (authenticationRequestBody.UserName == null) return WriteBadRequest(context, "'userName' value is required.", startedAt);
            if (authenticationRequestBody.Password == null) return WriteBadRequest(context, "'password' value is required.", startedAt);

            var operationResult = _authenticationService.Authenticate(authenticationRequestBody.UserName, authenticationRequestBody.Password);
            if (operationResult.IsError) return WriteBadRequest(context, JsonConvert.SerializeObject(operationResult.GetError()), startedAt);
            if (operationResult.IsNotFound) return WriteUnauthorized(context, startedAt);

            var symmetricSecurityKeyFilePath = _configuration.Environment.Jwt.Key;
            var symmetricSecurityKeyFileContent = File.ReadAllText(symmetricSecurityKeyFilePath);
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricSecurityKeyFileContent));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userId", operationResult.GetOutput().UserId.ToString()),
                new Claim("context", JsonConvert.SerializeObject(operationResult.GetOutput().Context))
            };

            var token = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials);
            var tokenObject = new { value = new JwtSecurityTokenHandler().WriteToken(token) };

            return WriteOk(context, tokenObject, startedAt);
        }

        private async Task WriteBadRequest(HttpContext context, string message, DateTime startedAt)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(message);
            Log(context, message, startedAt);
        }
        private Task WriteUnauthorized(HttpContext context, DateTime startedAt)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            Log(context, null, startedAt);
            return Task.CompletedTask;
        }
        private async Task WriteOk(HttpContext context, object token, DateTime startedAt)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(token));
            Log(context, null, startedAt);
        }

        private void Log(HttpContext context, string responseBody, DateTime startedAt)
        {
            var requestInfo = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
            context.Request.Body.Position = 0;
            requestInfo += $" {new StreamReader(context.Request.Body).ReadToEnd()}";
            requestInfo = "Request: " + requestInfo;

            var responseInfo = $"{context.Response.StatusCode}{(ValidationHelper.IsNullOrEmptyOrWhiteSpace(responseBody) ? "" : $", '{responseBody}'")}";
            responseInfo += $"{responseInfo}, {(DateTime.UtcNow - startedAt).TotalMilliseconds}ms";
            responseInfo = "Response: " + responseInfo;

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("New request handled.");
            messageBuilder.AppendLine(requestInfo);
            messageBuilder.AppendLine(responseInfo);
            _logger.LogInformation(messageBuilder.ToString());
        }
    }

    internal sealed class AuthenticationRequestBody
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}