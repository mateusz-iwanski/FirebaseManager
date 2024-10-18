using FirebaseManager.Firebase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FirebaseManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var serviceProvider = host.Services;

            using var scope = serviceProvider.CreateScope();
            var fs = scope.ServiceProvider.GetRequiredService<IFirestoreService>();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
              .ConfigureServices((context, services) =>
              {
                  var startup = new Startup();
                  startup.ConfigureServices(context, services);
              });
    }
}
