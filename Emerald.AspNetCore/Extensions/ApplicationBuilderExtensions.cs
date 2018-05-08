using Emerald.AspNetCore.Common;
using Emerald.AspNetCore.Configuration;
using Emerald.AspNetCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Emerald.AspNetCore.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseEmerald(this IApplicationBuilder app)
        {
            var applicationLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            applicationLifetime.ApplicationStopped.Register(() => Registry.EmeraldSystem.Terminate().Wait());

            var configuration = app.ApplicationServices.GetService<IApplicationConfiguration>();
            var options = Registry.EmeraldOptions;

            if (options.AuthenticationEnabled)
            {
                var authenticationService = app.ApplicationServices.GetService(options.AuthenticationServiceType);
                app.UseMiddleware<AuthenticationMiddleware>(authenticationService, configuration);
                app.UseAuthentication();
            }

            app.UseMvc();

            if (options.SwaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(opt => { opt.SwaggerEndpoint(options.SwaggerEndpoint, options.SwaggerApiName); });
            }

            return app;
        }
    }
}