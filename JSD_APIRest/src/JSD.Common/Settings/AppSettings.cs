namespace JSD.Common.Settings;

// General settings for the application
public class AppSettings
{
  // The path to the JSON file where tasks are stored
  // NOTE: Run the app from JSD_APIRest folder to use relative path
  // NOTE: All these should be provided via ENV vars in production, dockerfile or secrets manager, etc
  public string TasksFilePath { get; set; } = "";

  // Not Allowed tenants for the application
  public string[] BlockedTenants { get; set; } = Array.Empty<string>();

  // The base URL for the DocuWare API
  public string DocuWareBaseUrl { get; set; } = "";

  // Credentials for the DocuWare API ( couldn't find them in swagger, but i found them on another candidate's fork )
  public string DocuWareUser { get; set; } = "";
  public string DocuWarePassword { get; set; } = "";
  public string DocuWareCabinetId { get; set; } = "";
}