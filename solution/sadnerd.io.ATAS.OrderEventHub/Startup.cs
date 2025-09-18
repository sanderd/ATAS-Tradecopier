using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Services;
using sadnerd.io.ATAS.OrderEventHub.Factories;
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
        services.AddDbContext<OrderEventHubDbContext>();

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
        app.UseCors(builder =>
        {
            builder.WithOrigins("https://www.topstepx.com", "https://topstepx.com").AllowAnyMethod().AllowAnyHeader()
                .AllowCredentials();
        });

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