using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Services;
using sadnerd.io.ATAS.OrderEventHub.Factories;
using sadnerd.io.ATAS.OrderEventHub.Identity;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.AtasEventHub;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.FeatureFlags;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications.SignalR;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.Notifications.Sinks;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.CopyManager;
using sadnerd.io.ATAS.OrderEventHub.ProjectXIntegration.SignalR;
using sadnerd.io.ATAS.OrderEventHub.Services;
using sadnerd.io.ATAS.ProjectXApiClient;
using Serilog;

namespace sadnerd.io.ATAS.OrderEventHub;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalR();
        services.AddHostedService<ServiceWireWorker>();
        services.AddHostedService<CopyStrategyInitializationService>();
        services.AddCors();

        // Configure Entity Framework
        services.AddDbContext<OrderEventHubDbContext>(options =>
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Join(path, "sadnerd.tradecopy.db");
            options.UseSqlite($"Data Source={dbPath}");
        });

        // Configure Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Relaxed password settings - require only a password
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 1;
            options.Password.RequiredUniqueChars = 0;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        })
        .AddEntityFrameworkStores<OrderEventHubDbContext>()
        .AddDefaultTokenProviders();

        // Configure application cookie
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/Login";
            options.SlidingExpiration = true;
        });

        // Add account service
        services.AddScoped<IAccountService, AccountService>();

        // Feature Flags
        services.AddSingleton<IFeatureFlagService, ConfigurationFeatureFlagService>();

        // Notification Infrastructure
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<INotificationSink, ConsoleLoggingSink>();
        services.AddSingleton<INotificationSink, SignalRNotificationSink>();

        services.AddSingleton<IOrderEventHubDispatchService, EventBusPassthroughEventHubDispatchService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());

        // Add integration event bus
        services.AddHostedService<IntegrationEventProcessorJob>();
        services.AddSingleton<InMemoryMessageQueue>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddTransient<IProjectXBrowserAutomationClient, ProjectXBrowserAutomationClient>();

        // Add ProjectX vendor configuration service
        services.AddSingleton<IProjectXVendorConfigurationService, ProjectXVendorConfigurationService>();
        services.AddSingleton<IProjectXClientFactory, ProjectXClientFactory>();

        // Add ProjectX token cache service (singleton to share tokens across all instances)
        services.AddSingleton<IProjectXTokenCacheService, ProjectXTokenCacheService>();

        services.AddSingleton<ProjectXTradeCopyManagerProvider>(sp =>
        {
            var manager = new ProjectXTradeCopyManagerProvider(sp.CreateScope().ServiceProvider);
            return manager;
        });

        // NOTE: No longer in use
        services.AddSingleton<TopstepBrowserConnectionManager>();

        // Legacy configuration for backward compatibility - now handled by vendor service
        services.Configure<ProjectXClientOptions>(options =>
        {
            options.ApiKey = "6p9C6d/G5QMR7UZ/Bfsf2TjzKLLvJQtPqmTt/sVRqZM=";
            options.ApiUrl = "https://api.topstepx.com";
            options.UserApiUrl = "https://userapi.topstepx.com";
            options.ApiUser = "sanderd";
        });
        services.AddHttpClient<IProjectXClient, ProjectXClient>();

        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddScoped<CopyStrategyService>();

        services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            //.ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Allow CORS from TopstepX for SignalR connections (used in case of Browser Automation)
        // Also allow localhost for local development
        app.UseCors(builder =>
        {
            if (env.IsDevelopment())
            {
                builder.WithOrigins("https://www.topstepx.com", "https://topstepx.com", "http://localhost:5000", "https://localhost:5001")
                    .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            }
            else
            {
                builder.WithOrigins("https://www.topstepx.com", "https://topstepx.com")
                    .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            }
        });

        // Add authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<SignalRProjectXAutomationHub>("/topstepxhub");
            endpoints.MapHub<NotificationHub>("/notificationhub");

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapRazorPages();
        });

        // Initialize notification sinks
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var sinks = scope.ServiceProvider.GetServices<INotificationSink>();
            
            foreach (var sink in sinks)
            {
                notificationService.AddSink(sink);
            }
        }

        // Apply migrations and ensure database is created
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderEventHubDbContext>();
            dbContext.Database.Migrate();
        }
    }
}