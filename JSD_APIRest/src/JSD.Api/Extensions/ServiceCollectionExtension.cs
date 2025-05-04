using Microsoft.Extensions.Options;

using JSD.Common.Settings;

namespace JSD.Api.Extensions;

// Eextension methods for mapping endpoints to the application where we define the endpoints and their handlers

// NOTE: Try and make this more tidy / easy to read, seems like a mess at the moment
//  I bet theres a better way to do this
public static class ServiceCollectionExtensions
{
  public static IServiceCollection DocuWareHttpClient<TClient, TImpl>(this IServiceCollection services)
      where TClient : class
      where TImpl : class, TClient
  {
    // Register the HttpClient 
    services.AddHttpClient<TClient, TImpl>((sp, client) =>
    {
      var cfg = sp.GetRequiredService<IOptions<AppSettings>>().Value;
      client.BaseAddress = new Uri(cfg.DocuWareBaseUrl);
    });

    return services;
  }
}
