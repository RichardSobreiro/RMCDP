using Business.ConstructiveHeuristics;
using Contracts.Interfaces.Business;
using Contracts.Interfaces.Repository.Instances;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Instances;
using System;
using System.IO;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var services = ConfigureServices();

                var serviceProvider = services.BuildServiceProvider();

                serviceProvider.GetService<ConsoleApplication>().Run();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                Console.Write(e.StackTrace);
            }
        }

        public class ConsoleApplication
        {
            public void Run()
            {
                int grandeCentroId = 12;
                DateTime begin = new DateTime(2019, 1, 2, 0, 0, 0);
                DateTime end = new DateTime(2019, 1, 3, 0, 0, 0);

                behrouzAlireza.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 2, 0, 0, 0);
                end = new DateTime(2019, 1, 4, 0, 0, 0);

                behrouzAlireza.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 7, 0, 0, 0);
                end = new DateTime(2019, 1, 10, 0, 0, 0);

                behrouzAlireza.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 7, 0, 0, 0);
                end = new DateTime(2019, 1, 11, 0, 0, 0);

                behrouzAlireza.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 7, 0, 0, 0);
                end = new DateTime(2019, 1, 14, 0, 0, 0);

                behrouzAlireza.Execute(grandeCentroId, begin, end);

                //-----------------------------------------------------

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 2, 0, 0, 0);
                end = new DateTime(2019, 1, 3, 0, 0, 0);

                bestLoadPlace.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 2, 0, 0, 0);
                end = new DateTime(2019, 1, 4, 0, 0, 0);

                bestLoadPlace.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 7, 0, 0, 0);
                end = new DateTime(2019, 1, 10, 0, 0, 0);

                bestLoadPlace.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 7, 0, 0, 0);
                end = new DateTime(2019, 1, 11, 0, 0, 0);

                bestLoadPlace.Execute(grandeCentroId, begin, end);

                grandeCentroId = 12;
                begin = new DateTime(2019, 1, 7, 0, 0, 0);
                end = new DateTime(2019, 1, 14, 0, 0, 0);

                bestLoadPlace.Execute(grandeCentroId, begin, end);
            }

            public ConsoleApplication(IBestLoadPlaceFit _bestLoadPlace, IBehrouzAlireza _behrouzAlireza)
            {
                bestLoadPlace = _bestLoadPlace;
                behrouzAlireza = _behrouzAlireza;
            }

            private IBestLoadPlaceFit bestLoadPlace;
            private IBehrouzAlireza behrouzAlireza;
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            var config = LoadConfiguration();
            services.AddSingleton(config);

            services.AddScoped<IDeliveryOrderRepository, DeliveryOrderRepository>();
            services.AddScoped<ILoadPlacesRepository, LoadPlacesRepository>();

            services.AddScoped<IBestLoadPlaceFit, BestLoadPlaceFit>();
            services.AddScoped<IBehrouzAlireza, BehrouzAlireza>();

            services.AddTransient<ConsoleApplication>();
            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }
    }
}
