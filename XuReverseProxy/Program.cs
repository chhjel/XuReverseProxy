using XuReverseProxy.Extensions;

namespace XuReverseProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Add3rdPartyServices(builder.Configuration, builder.Environment);
            builder.Services.AddReverseProxy(builder.Configuration, builder.Environment);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseReverseProxy();
            app.Use3rdPartyServices();

            app.Run();
        }
    }
}
