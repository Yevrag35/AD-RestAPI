using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using AD.Api.Components;
using AD.Api.Services;

namespace AD.Api
{
    [SupportedOSPlatform("windows")]
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
            IConfigurationSection domainsSection = this.Configuration.GetSection("Domains");
            var domains = new SearchDomains(GetSearchDomains(domainsSection.GetChildren()));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));

            services.AddControllers().AddNewtonsoftJson((json) =>
            {
                json.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy(), false));
                json.SerializerSettings.Formatting = Formatting.Indented;
                json.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                json.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AD.Api", Version = "v1" });
            });

            services.AddCors();

            services.AddScoped<IADCreateService, ADCreateService>();
            services.AddScoped<IADEditService, ADEditService>();
            services.AddScoped<IADQueryService, ADQueryService>(service =>
            {
                return new ADQueryService(
                    service.GetService<IMapper>(),
                    domains
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseSwagger()
                    .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AD.Api v1"));
            }

            //app.UseHttpsRedirection();

            app
                .UseRouting()
                .UseCors(x =>
                    x
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

            app.UseAuthentication()
               .UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IEnumerable<SearchDomain> GetSearchDomains(IEnumerable<IConfigurationSection> sections)
        {
            foreach (IConfigurationSection section in sections)
            {
                var dom = section.Get<SearchDomain>();
                dom.FQDN = section.Key;
                yield return dom;
            }
        }
    }
}
