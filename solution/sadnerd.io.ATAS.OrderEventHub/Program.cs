using Microsoft.Extensions.Options;
using sadnerd.io.ATAS.OrderEventHub.Configuration;
using sadnerd.io.ATAS.OrderEventHub.Infrastructure;

namespace sadnerd.io.ATAS.OrderEventHub
{
    class Program
    {
        private static SingleInstanceManager? _singleInstanceManager;

        static int Main(string[] args)
        {
            // Build a temporary configuration to read the AllowMultipleInstances setting
            var tempConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var applicationOptions = new ApplicationOptions();
            tempConfig.GetSection(ApplicationOptions.SectionName).Bind(applicationOptions);

            // Check for single instance if not allowing multiple instances
            if (!applicationOptions.AllowMultipleInstances)
            {
                _singleInstanceManager = new SingleInstanceManager("sadnerd.io.ATAS.OrderEventHub");
                
                if (!_singleInstanceManager.TryAcquireLock(TimeSpan.FromSeconds(1)))
                {
                    Console.WriteLine("Another instance of the application is already running.");
                    Console.WriteLine("Set 'Application:AllowMultipleInstances' to true in appsettings.json to allow multiple instances.");
                    
                    _singleInstanceManager.Dispose();
                    return 1; // Exit with error code
                }

                // Ensure mutex is released when application exits
                AppDomain.CurrentDomain.ProcessExit += (sender, e) => _singleInstanceManager?.Dispose();
                Console.CancelKeyPress += (sender, e) => _singleInstanceManager?.Dispose();
            }

            try
            {
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application startup failed: {ex.Message}");
                return 1;
            }
            finally
            {
                _singleInstanceManager?.Dispose();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}