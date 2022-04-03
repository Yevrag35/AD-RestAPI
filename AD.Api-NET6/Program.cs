using AD.Api.Domains;
using AD.Api.Ldap.Converters.Json;
using AD.Api.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

static IEnumerable<SearchDomain> GetSearchDomains(IEnumerable<IConfigurationSection> sections)
{
    foreach (IConfigurationSection section in sections)
    {
        var dom = section.Get<SearchDomain>();
        dom.FQDN = section.Key;
        yield return dom;
    }
}

IConfigurationSection section = builder.Configuration.GetSection("Domains");
builder.Services.AddSingleton(new SearchDomains(GetSearchDomains(section.GetChildren())));

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new FilterConverter());
    options.SerializerSettings.Converters.Add(new PathValueJsonConverter());
    options.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));

    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionService, ConnectionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
