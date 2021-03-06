﻿using Business.ConstructiveHeuristics;
using Contracts.Interfaces.Business;
using Contracts.Interfaces.Repository.Instances;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Instances;

namespace RMCDP.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddScoped<IDeliveryOrderRepository, DeliveryOrderRepository>();
            services.AddScoped<ILoadPlacesRepository, LoadPlacesRepository>();

            services.AddScoped<IBestLoadPlaceFit, BestLoadPlaceFit>();
            services.AddScoped<IBehrouzAlireza, BehrouzAlireza>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
        }
    }
}
