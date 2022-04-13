using AD.Api.Domains;
using AD.Api.Extensions;
using AD.Api.Ldap.Converters;
using AD.Api.Middleware;
using AD.Api.Schema;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

// Add services to the container.

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddSearchDomains(builder.Configuration.GetSection("Domains"));
builder.Services.AddSearchDefaultSettings(builder.Configuration.GetSection("Settings").GetSection("SearchDefaults"));
builder.Services.AddTextSettingOptions(builder.Configuration);

builder.Services.AddADApiServices();

Assembly[] appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
builder.Services
    .AddAutoMapper(appDomainAssemblies)
    .AddLdapEnumTypes(appDomainAssemblies);

builder.Services.AddControllers().AddNewtonsoftJson(options => options.AddADApiConfiguration());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
