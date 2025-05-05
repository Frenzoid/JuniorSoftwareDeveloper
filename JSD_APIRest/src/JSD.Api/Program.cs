using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using System.Text.Json;

using JSD.Api.Middleware;
using JSD.Api.Extensions;
using JSD.Infrastructure.Repositories;
using JSD.Infrastructure.Services;
using JSD.Common.Settings;
using JSD.Common.Interfaces;

// Builder for the web application
var builder = WebApplication.CreateBuilder(args);


// Configs ( default harcoded in JSON, should load now on env vars )
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
       optional: true,
       reloadOnChange: true)
    .AddEnvironmentVariables();

// Bind settings to the AppSettings class
//   this is to load the settings from the appsettings.json file into the AppSettings class
builder.Services
    .AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection("AppSettings"));



// Service: JSON Repository for tasks
builder.Services.AddSingleton<ITaskRepository, JsonTaskRepository>();

// Service: DocuWare service with HttpClient
//   our custom service to interact with the DocuWare API
builder.Services.DocuWareHttpClient<IDocuWareService, DocuWareService>();

// Body serializer options
//   this is to ignore null values in the JSON serialization
builder.Services.Configure<JsonOptions>(opts =>
{
    opts.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opts.SerializerOptions.AllowTrailingCommas = true;
    opts.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
});


// Builddd
var app = builder.Build();

// DEBUG: Show loaded settings
app.MapGet("/_config", (IOptions<AppSettings> settings) => settings.Value);

// Middlewares
//   Order: Force https -> Verify if tenant ID is present -> Verify if tenant ID is not in the blocked tenants list.
app.UseHttpsRedirection();
app.UseTenantExistence();
app.UseTenantValidation();

// Endpoints ( Routers )
app.MapTaskEndpoints();

// Lets goo
app.Run();



// This is just to make the compiler happy ( enables integration testing ), we could remove it
public partial class Program { }
