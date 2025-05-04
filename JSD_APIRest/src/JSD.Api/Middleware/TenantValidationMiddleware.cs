using Microsoft.Extensions.Options;

using JSD.Common.Settings;

namespace JSD.Api.Middleware;

// Middlewares to validate tenant ID
public static class TenantValidationMiddleware
{

  // We verify if the tenant ID is present in the request headers
  public static IApplicationBuilder UseTenantExistence(this IApplicationBuilder app)
    => app.Use(async (ctx, next) =>
    {
      if (!ctx.Request.Headers.TryGetValue("X-Tenant-ID", out var t))
      {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsync("Missing X-Tenant-ID header, please provide a tenant ID in the request headers.");
        return;
      }
      await next();
    });

  // Weverify if the tenant ID is not in the blocked tenants list
  public static IApplicationBuilder UseTenantValidation(this IApplicationBuilder app)
      => app.Use(async (ctx, next) =>
      {
        var tenantId = ctx.Request.Headers["X-Tenant-ID"].ToString();
        var blockedTenants = ctx.RequestServices
              .GetRequiredService<IOptions<AppSettings>>()
              .Value.BlockedTenants;

        // Check if the tenant ID is in the blocked tenants list
        // NOTE: This is a simple string comparison, but we could use a more complex validation if needed
        if (blockedTenants.Contains(tenantId))
        {
          ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
          await ctx.Response.WriteAsync("Forbidden tenant ID.");
          return;
        }

        // Store the tenant ID in the context for later use
        ctx.Items["TenantId"] = tenantId.ToString();
        await next();
      });
}
