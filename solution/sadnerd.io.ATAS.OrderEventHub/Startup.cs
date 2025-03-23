using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using sadnerd.io.ATAS.BroadcastOrderEvents.Contracts.Services;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure.AtasEventHub;

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
        services.AddCors();

        services.AddSingleton<IOrderEventHubDispatchService, EventBusPassthroughEventHubDispatchService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());

        // Add integration event bus
        services.AddHostedService<IntegrationEventProcessorJob>();
        services.AddSingleton<InMemoryMessageQueue>();
        services.AddSingleton<IEventBus, EventBus>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseCors(builder =>
        {
            builder.WithOrigins("https://www.youtube.com").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
            builder.WithOrigins("https://www.topstepx.com").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<SignalRTopstepAutomationHub>("/topstepxhub");
        });
    }
}