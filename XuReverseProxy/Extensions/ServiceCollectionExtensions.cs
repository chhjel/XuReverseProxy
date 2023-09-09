using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QoDL.Toolkit.Core.Util;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication;
using XuReverseProxy.Core.ScheduledTasks;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Core.Systems.ScheduledTasks;
using XuReverseProxy.Core.Utils;
using XuReverseProxy.Middleware;

namespace XuReverseProxy.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReverseProxy(this IServiceCollection services, ConfigurationManager configuration, IWebHostEnvironment _)
    {
        // Config
        services.Configure<ServerConfig>(configuration.GetSection("ServerConfig"));
        services.AddScoped<RuntimeServerConfig>();

        // Services
        services.AddScoped<IDevDataSeeder, DevDataSeederService>();
        services.AddScoped<IProxyClientIdentityService, ProxyClientIdentityService>();
        services.AddScoped<IProxyChallengeService, ProxyChallengeService>();
        services.AddScoped<IProxyAuthenticationChallengeFactory, ProxyAuthenticationChallengeFactory>();
        services.AddScoped<IProxyAuthenticationConditionChecker, ProxyAuthenticationConditionChecker>();
        services.AddScoped<IIPBlockService, IPBlockService>();
        services.AddSingleton<IIPLookupService, IPWhoIsIPLookupService>();

        // Scheduled tasks
        services.AddSingleton<IHostedService, SchedulerHostedService>();
        services.AddSingleton<IScheduledTask, ClientIdentityCleanupTask>();

        return services;
    }

    public static WebApplication UseReverseProxy(this WebApplication app)
    {
        app.UseMiddleware<ReverseProxyMiddleware>();

        using (var scope = app.Services.CreateScope())
        {
            // Apply EF migrations
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            // Ensure we have rows for all settings
            scope.ServiceProvider.GetService<RuntimeServerConfig>()?.EnsureDatabaseRows();

            // todo: not here
            TKAsyncUtils.RunSync(() => scope.ServiceProvider.GetService<IDevDataSeeder>()?.EnsureDemoDataAsync());
        }
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

    public const string IdentityCookieName = "___xurp_identity";
    public const string AuthCookieName = "___xurp_auth";
    public const string AntiForgeryCookieName = "___xurp_antiforgery";
    public static void Add3rdPartyServices(this IServiceCollection services, ConfigurationManager _, IWebHostEnvironment environment)
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
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Stores.MaxLengthForKeys = 128;
#if DEBUG
                // Allow simple passwords for localdev
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
#endif
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            // Validate security timestamps for when we invalidate admin sessions due to ip change (if enabled).
            options.ValidationInterval = TimeSpan.FromSeconds(1);
        });
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = IdentityCookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
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
                    options.Cookie = new CookieBuilder() { Name = AuthCookieName };
                    options.LoginPath = new PathString("/auth/login");
                    options.AccessDeniedPath = new PathString("/auth/denied");
                    options.ReturnUrlParameter = "return";
                    //options.EventsType = typeof(CustomCookieAuthenticationEvents);
                });

        // Misc
        services.AddMemoryCache();
        services.AddControllersWithViews(options =>
        {
            // Supressed since we send EF entities to frontend and some refer to parents that are null.
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        }).AddJsonOptions(options => JsonConfig.ApplyDefaultOptions(options.JsonSerializerOptions));
        services.AddMvc();
        services.AddAntiforgery(opts => opts.Cookie.Name = AuthCookieName);
        services.AddHttpClient();
        services.AddHttpForwarder();
        services.AddHttpContextAccessor();
    }
}
