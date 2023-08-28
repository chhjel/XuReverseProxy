using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Middleware;

namespace XuReverseProxy.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReverseProxy(this IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        // Config
        services.Configure<ServerConfig>(configuration.GetSection("ServerConfig"));
        services.AddScoped<RuntimeServerConfig>();

        // Services
        services.AddScoped<IProxyConfigService, ProxyConfigService>();
        services.AddScoped<IProxyClientIdentityService, ProxyClientIdentityService>();
        services.AddScoped<IProxyAuthenticationChallengeFactory, ProxyAuthenticationChallengeFactory>();
        services.AddScoped<IProxyAuthenticationConditionChecker, ProxyAuthenticationConditionChecker>();

        return services;
    }

    public static WebApplication UseReverseProxy(this WebApplication app)
    {
        app.UseMiddleware<ReverseProxyMiddleware>();
        return app;
    }

    public static WebApplication Use3rdPartyServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/error/{0}");
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();
        return app;
    }

    public static void Add3rdPartyServices(this IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        // Inject service provider itself for use in some special places
        services.AddSingleton(x => x);

        // EF etc
        services.AddDbContext<ApplicationDbContext>();
        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options => options.Stores.MaxLengthForKeys = 128)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "_xurp_identity";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(365) * 100;
            options.SlidingExpiration = true;
            options.LoginPath = new PathString("/auth/login");
            options.LogoutPath = new PathString("/auth/logout");
            options.AccessDeniedPath = new PathString("/auth/denied");
            options.ReturnUrlParameter = "return";
            //options.EventsType = typeof(CustomCookieAuthenticationEvents);
        });

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                options =>
                {
                    options.Cookie = new CookieBuilder() { Name = "_xurp_auth" };
                    options.LoginPath = new PathString("/auth/login");
                    options.AccessDeniedPath = new PathString("/auth/denied");
                    options.ReturnUrlParameter = "return";
                    //options.EventsType = typeof(CustomCookieAuthenticationEvents);
                });

        // Misc
        //services.AddRazorPages();
        services.AddControllersWithViews();
        services.AddMvc();
        services.AddAntiforgery(opts => opts.Cookie.Name = "_xurp_antiforgery");
        services.AddHttpClient();
        services.AddHttpForwarder();
        services.AddHttpContextAccessor();
    }
}
