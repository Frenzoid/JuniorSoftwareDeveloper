namespace JSD.Common.Settings;

// General settings for the application
public class AppSettings
{
  // The path to the JSON file where tasks are stored
  // NOTE: Run the app from JSD_APIRest folder to use relative path
  // NOTE: All these should be provided via ENV vars in production, dockerfile or secrets manager, etc
  public string TasksFilePath { get; set; } = "../../data/tasks.json";

  // Not Allowed tenants for the application
  public string[] BlockedTenants { get; set; } = new[] { "NotAllowed" };

  // The base URL for the DocuWare API
  public string DocuWareBaseUrl { get; set; } = "https://services.paloalto.swiss:10443";

  // Credentials for the DocuWare API ( couldn't find them in swagger, but i found them on another candidate's fork )
  public string DocuWareUser { get; set; } = "23";
  public string DocuWarePassword { get; set; } = "1234";
  public string DocuWareCabinetId { get; set; } = "804dfcb0-cf00-49c7-bb23-ec68bc3a6097";
}