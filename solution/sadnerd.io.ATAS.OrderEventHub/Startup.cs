using Microsoft.EntityFrameworkCore;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.OrderEventHub.Data;
using sadnerd.io.ATAS.OrderEventHub.Data.Services;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.AtasEventHub;
using sadnerd.io.ATAS.OrderEventHub.Services;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.ConnectionManagement;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.CopyManager;
using sadnerd.io.ATAS.OrderEventHub.TopstepIntegration.SignalR;
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

        services.AddSingleton<IOrderEventHubDispatchService, EventBusPassthroughEventHubDispatchService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());

        // Add integration event bus
        services.AddHostedService<IntegrationEventProcessorJob>();
        services.AddSingleton<InMemoryMessageQueue>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddTransient<ITopstepBrowserAutomationClient, TopstepBrowserAutomationClient>();

        services.AddSingleton<TopstepXTradeCopyManagerProvider>(sp =>
        {
            var manager = new TopstepXTradeCopyManagerProvider(sp.CreateScope().ServiceProvider);
            return manager;
        });
        services.AddSingleton<TopstepConnectionManager>();

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
        services.AddDbContext<TradeCopyContext>();

        services.AddScoped<CopyStrategyService>();

        services.AddHostedService<TopstepTest>();

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

        app.UseCors(builder =>
        {
            builder.WithOrigins("https://www.youtube.com").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            builder.WithOrigins("https://www.topstepx.com", "https://topstepx.com").AllowAnyMethod().AllowAnyHeader()
                .AllowCredentials();
        });

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<SignalRTopstepAutomationHub>("/topstepxhub");

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapRazorPages();
        });

        

        // Apply migrations and ensure database is created
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TradeCopyContext>();
            dbContext.Database.Migrate();
        }
    }
}