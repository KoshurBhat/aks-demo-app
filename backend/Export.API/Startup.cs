﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Export.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Export.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            HostingEnvironment = environment;
        }

        private IHostingEnvironment HostingEnvironment { get; }
        private IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(options =>
            {
                // add just console logger now
                options.AddConsole();
            });
            services.AddMvc();
            services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });
            services.Configure<GzipCompressionProviderOptions>(compressionOptions =>
            {
                compressionOptions.Level = CompressionLevel.Optimal;
            });
            services.AddCors();
            services.AddTransient<JobDispatcherService>();
            if (HostingEnvironment.IsDevelopment())
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info
                    {
                        Title = "ExportAPI",
                        Contact = new Contact
                        {
                            Name = "Thorsten Hans",
                            Email = "thorsten.hans@gmail.com",
                            Url = "https://thorsten-hans.com"

                        },
                        License = new License
                        {
                            Name = "MIT"
                        },
                        Version = "v1"
                    });
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //todo: provide real cors policy for prod
            app.UseCors(config =>
            {
                config.AllowAnyHeader();
                config.AllowAnyMethod();
                config.AllowAnyOrigin();
            });
            app.UseResponseCompression();
            app.UseMvc();
            if (HostingEnvironment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExportAPI v1");
                    c.RoutePrefix = string.Empty;
                });
            }
        }
    }
}
