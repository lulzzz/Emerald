using Emerald.Abstractions;
using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Emerald.AspNetCore.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseEmerald(this IApplicationBuilder app, ITransactionScopeFactory transactionScopeFactory)
        {
            var logger = new Logger(app.ApplicationServices.GetService<ILogger<EmeraldSystem>>());
            var serviceScopeFactory = new ServiceScopeFactory(app.ApplicationServices.GetService<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>());
            var emeraldSystem = Registry.EmeraldSystemBuilder.Build(logger, serviceScopeFactory, transactionScopeFactory);

            var applicationLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            applicationLifetime.ApplicationStopped.Register(() => emeraldSystem.Terminate().Wait());

            return app;
        }
    }
}