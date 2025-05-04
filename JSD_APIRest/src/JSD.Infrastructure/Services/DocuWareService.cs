using System.Net.Http.Json;
using Microsoft.Extensions.Options;

using JSD.Domain.Models;
using JSD.Common.Settings;

namespace JSD.Infrastructure.Services
{
  public interface IDocuWareService
  {
    Task AddRecordAsync(TaskItem item);
  }

  public class DocuWareService : IDocuWareService
  {
    private readonly HttpClient _http;
    private readonly string _userId;
    private readonly string _passwordWS;
    private readonly string _cabinetId;

    public DocuWareService(HttpClient http, IOptions<AppSettings> opts)
    {
      _http = http;
      var cfg = opts.Value;
      _userId = cfg.DocuWareUser;
      _passwordWS = cfg.DocuWarePassword;
      _cabinetId = cfg.DocuWareCabinetId;
    }

    public async Task AddRecordAsync(TaskItem t)
    {
      // Build payload including credentials and cabinet ID from the swagger docs provided
      var payload = new
      {
        userId = _userId,
        passwordWS = _passwordWS,
        cabinetId = _cabinetId,
        indexFields = new[]
          {
            new { fieldName = "TASK_ID",           fieldValue = t.Id },
            new { fieldName = "TASK_DESCRIPTION",  fieldValue = t.Description },
            new { fieldName = "CREATION_DATE",     fieldValue = t.CreationDate.ToString("o") }
          }
      };

      var url_path = $"/api2/Docuware/add-record";
      var resp = await _http.PostAsJsonAsync(url_path, payload);
      resp.EnsureSuccessStatusCode();
    }
  }
}
