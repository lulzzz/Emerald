using Emerald.AspNetCore.Common;
using Emerald.Common;
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
            var logger = app.ApplicationServices.GetService<ILogger<EmeraldSystem>>();
            var serviceScopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
            var emeraldSystem = Registry.EmeraldSystemBuilder.Build(logger, serviceScopeFactory, transactionScopeFactory);

            var applicationLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            applicationLifetime.ApplicationStopped.Register(() => emeraldSystem.Terminate().Wait());

            return app;
        }
    }
}