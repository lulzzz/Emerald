using Emerald.Application;
using System;

namespace Emerald.AspNetCore.Infrastructure
{
    public interface IAuthenticationService
    {
        OperationResult<AuthenticationServiceResult> Authenticate(string userName, string password);
    }

    public class AuthenticationServiceResult
    {
        public AuthenticationServiceResult(object context, Guid userId)
        {
            Context = context;
            UserId = userId;
        }

        public object Context { get; }
        public Guid UserId { get; }
    }
}