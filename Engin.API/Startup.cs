﻿using Engin.API.Configuration;
using Engin.API.Helpers;
using Engin.API.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

namespace Engin.API
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
            services.AddMvc();
            services.AddCors(options => options
                .AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()));
            services.Configure<EnginSettings>(Configuration.GetSection(nameof(EnginSettings)));
            services.AddSingleton<AlprServiceHelper>();
            services.AddSingleton<HpiServiceHelper>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Engin", Version = "v1" });
            });
            services.AddSerilogLogger(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var log = app.ApplicationServices.GetService<Serilog.ILogger>();
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();

            log.Debug("App has started");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Engin V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseCors("AllowAll");

            app.UseMvc();
        }
    }
}