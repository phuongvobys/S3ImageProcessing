﻿using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using S3ImageProcessing.S3Bucket;
using S3ImageProcessing.Services.Implementations;
using S3ImageProcessing.Services.Interfaces;

namespace S3ImageProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Path.Combine(AppContext.BaseDirectory)).AddJsonFile("appsettings.json", true, true);

            var configuration = builder.Build();

            var serviceCollection = new ServiceCollection();

            var serviceProvider = serviceCollection
                .AddOptions()
                .AddLogging(opt => opt.AddConsole())
                .Configure<S3ClientOption>(configuration.GetSection(nameof(S3ClientOption)))
                .AddSingleton<S3CBucketClient>()
                .AddScoped<IImageStorageProvider, S3ImageStorageProvider>()

                //.AddScoped<IExchangeService, ExchangeService>()
                //.AddScoped<IRegressionEquationService, RegressionEquationService>()
                //.AddScoped<IPredictionService, PredictionService>()
                //.AddScoped<IAutoCompleteHandler, AutoCompleteHandler>()
                .AddScoped<S3ImageProcessingApp>()
                .BuildServiceProvider();

            serviceProvider.GetService<S3ImageProcessingApp>().Start().GetAwaiter().GetResult();
        }
    }
}