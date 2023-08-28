using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
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

            // Apply EF migrations
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
