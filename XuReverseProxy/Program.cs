using XuReverseProxy.Core.Logging;
using XuReverseProxy.Extensions;

namespace XuReverseProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;

            var enableSentry = !string.IsNullOrWhiteSpace(configuration["Sentry:Dsn"]);
            if (enableSentry) builder.WebHost.UseSentry();

            builder.WebHost.UseKestrel(options =>
            {
                options.AddServerHeader = false;
                options.Limits.MaxRequestBodySize = long.MaxValue;
            });
            builder.Logging.AddMemoryLogger();

            // Add services to the container.
            var services = builder.Services;
            if (enableSentry) services.AddSentry();
            services.AddCoreServices(configuration, builder.Environment);
            services.AddReverseProxy(configuration, builder.Environment);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseReverseProxy();
            app.UseCore(enableSentry);

            app.Run();
        }
    }
}
