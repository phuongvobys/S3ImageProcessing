﻿using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MySql.Data.MySqlClient;

using S3ImageProcessing.Data;
using S3ImageProcessing.S3Bucket;
using S3ImageProcessing.Services.Implementations;
using S3ImageProcessing.Services.Interfaces;

namespace S3ImageProcessing
{
    class Program
    {
        private static IServiceProvider _serviceProvider;

        private static IConfiguration _configuration;

        static void Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder().SetBasePath(Path.Combine(AppContext.BaseDirectory)).AddJsonFile("appsettings.json", true, true);

            _configuration = configurationBuilder.Build();

            RegisterDbProvider();
            RegisterServices(_configuration);

            _serviceProvider.GetService<S3ImageProcessingApp>().Start();

            DisposeServices();
        }

        private static void RegisterDbProvider()
        {
            DbProviderFactories.RegisterFactory("MySql.Data.MySqlClient", MySqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
        }

        private static void RegisterServices(IConfiguration configuration)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddOptions()
                .AddLogging(opt => opt.AddConsole())
                .Configure<S3ClientOption>(configuration.GetSection(nameof(S3ClientOption)))
                .Configure<DatabaseOption>(configuration.GetSection(nameof(DatabaseOption)))
                .AddSingleton<S3CBucketClient>()
                .AddSingleton<DbAccess>()
                .AddSingleton<IImageStorageProvider, S3ImageStorageProvider>()
                .AddSingleton<IParsedImageStore, ParsedImageStore>()
                .AddSingleton<IImageHistogramService, ImageHistogramService>()
                .AddSingleton<S3ImageProcessingApp>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}