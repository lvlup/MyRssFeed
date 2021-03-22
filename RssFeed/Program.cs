using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RssFeed.Service;
using System;
using System.IO;

namespace RssFeed
{
    class Program
    {
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            RegisterServices();

            var rssService = _serviceProvider.GetService<IFeedScrapingService>();
            rssService.GetRssFeed();
            Console.WriteLine(rssService.GetStringRssFeed().Result);
            Console.ReadKey();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();

                IHostEnvironment env = hostingContext.HostingEnvironment;

                configuration
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

                IConfigurationRoot configurationRoot = configuration.Build();

                ConfigOptions options = new();
                configurationRoot.GetSection(nameof(ConfigOptions))
                                 .Bind(options);
            });

        private static void RegisterServices()
        {
            IConfiguration config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
              .Build();

            var collection = new ServiceCollection();
            collection.AddOptions();

            collection.Configure<ConfigOptions>(config.GetSection(nameof(ConfigOptions)));
            var builder = new ContainerBuilder();

            builder.RegisterType<FeedScrapingService>().As<IFeedScrapingService>();
            builder.Populate(collection);

            var appContainer = builder.Build();
            _serviceProvider = new AutofacServiceProvider(appContainer);
        }
    }
}
