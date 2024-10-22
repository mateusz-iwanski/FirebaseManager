using FirebaseManager.Firebase;
using FirebaseManager.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace FirebaseManager
{
    /// <summary>
    /// Represents a builder for configuring services.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the ConfigureServices class.
        /// </summary>
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

            services.AddSingleton<IConfiguration>((ConfigurationBuilder) =>
            {
                return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            });

            // Add NLog as the logging provider
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders(); // Clear any existing logging providers
                loggingBuilder.SetMinimumLevel(LogLevel.Trace); // Set the minimum log level

                // Add NLog
                loggingBuilder.AddNLog();
            });
            // Register NLog's ILogger
            services.AddSingleton<NLog.ILogger>(provider => NLog.LogManager.GetCurrentClassLogger());

            // Add options to bind to the configuration instance
            services.Configure<FirebaseSettings>(context.Configuration.GetSection("Firebase").GetSection("Firestore"));

            // Add Firestore services
            services.AddScoped<IFirestoreConnector, FirestoreConnector>(); 
            services.AddScoped<IFirestoreService, FirestoreService>();
        }
    }
}
