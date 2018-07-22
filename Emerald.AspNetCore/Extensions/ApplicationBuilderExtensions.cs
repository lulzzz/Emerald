using Emerald.AspNetCore.System;
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

            var options = Registry.EmeraldOptions;

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