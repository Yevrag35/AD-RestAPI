using AD.Api.Domains;
using AD.Api.Extensions;
using AD.Api.Middleware;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddEnvironmentVariables()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(options =>
    {
        options.Path = "appsettings.json";
        options.Optional = false;
    });

// Add services to the container.

// Add Authentication
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

builder.Services.AddAuthentication()
    //.AddJwtBearer("Auth0", options =>
    //{
    //    options.Audience = "myCustomJwt";
    //    options.Authority = "https://doesnotexist.com";
    //})
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAD"));

builder.Services.AddAuthorization(options =>
{
    var policyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme); // Auth0
    policyBuilder = policyBuilder.RequireAuthenticatedUser();
    options.DefaultPolicy = policyBuilder.Build();
});

builder.Services.AddOperationRestrictions(builder.Configuration.GetSection("Settings").GetSection("Restrictions"));
builder.Services.AddSearchDomains(builder.Configuration.GetSection("Domains"));
builder.Services.AddSearchDefaultSettings(builder.Configuration.GetSection("Settings").GetSection("SearchDefaults"));
builder.Services.AddTextSettingOptions(builder.Configuration, out ITextSettings textSettings);

builder.Services.AddADApiServices();

Assembly[] appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
builder.Services
    .AddAutoMapper(appDomainAssemblies)
    .AddLdapEnumTypes(appDomainAssemblies);

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.AddADApiConfiguration(textSettings);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithOptions(builder.Configuration.GetSection("SwaggerInfo"));

#if DEBUG
//_ = builder.Services.BuildServiceProvider(true);
IdentityModelEventSource.ShowPII = true;
#endif

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = new ErrorHandlingMiddleware().Invoke
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

//app.UseMultipleSchemaAuthenticationMiddleware();

app.MapControllers();

app.Run();
