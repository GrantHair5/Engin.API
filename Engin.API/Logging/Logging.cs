using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Engin.API.Logging
{
    public static class Logging
    {
        public static IServiceCollection AddSerilogLogger(this IServiceCollection services, IConfiguration configuration)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddSingleton<ILogger>(logger);

            return services;
        }
    }
}