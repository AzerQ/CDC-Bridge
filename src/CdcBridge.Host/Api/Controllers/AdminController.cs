using CdcBridge.Core.Models;
using CdcBridge.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;

namespace CdcBridge.Host.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IDbContextFactory<CdcBridgeDbContext> _contextFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

  public AdminController(
 IDbContextFactory<CdcBridgeDbContext> contextFactory,
        IConfiguration configuration,
        ILogger<AdminController> logger)
    {
    _contextFactory = contextFactory;
   _configuration = configuration;
        _logger = logger;
  }

    /// <summary>
 /// �������� ������ API ����� (������ � localhost � � ������-�������)
    /// </summary>
    [HttpPost("apikeys")]
    public async Task<ActionResult<ApiKeyResponse>> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        // ��������, ��� ������ ���� � localhost
     if (!IsLocalRequest())
        {
   _logger.LogWarning("Attempt to create API key from non-localhost IP: {RemoteIp}", HttpContext.Connection.RemoteIpAddress);
     return StatusCode(403, new { error = "API key creation is only allowed from localhost" });
      }

        // �������� ������-������
   var masterPassword = _configuration["ApiKeys:MasterPassword"];
 if (string.IsNullOrEmpty(masterPassword))
        {
   _logger.LogError("Master password is not configured");
       return StatusCode(500, new { error = "Master password is not configured" });
        }

        if (request.MasterPassword != masterPassword)
     {
 _logger.LogWarning("Invalid master password attempt from localhost");
     return Unauthorized(new { error = "Invalid master password" });
        }

        // ��������� ������
   if (string.IsNullOrWhiteSpace(request.Name))
        {
     return BadRequest(new { error = "Name is required" });
     }

        if (request.ExpiresInDays.HasValue && request.ExpiresInDays.Value <= 0)
  {
       return BadRequest(new { error = "ExpiresInDays must be greater than 0" });
 }

      await using var context = await _contextFactory.CreateDbContextAsync();

        // �������� ������������ �����
     if (await context.ApiKeys.AnyAsync(k => k.Name == request.Name))
     {
       return BadRequest(new { error = $"API key with name '{request.Name}' already exists" });
    }

        var apiKey = new ApiKey
   {
  Key = GenerateApiKey(),
            Name = request.Name,
    Owner = request.Owner,
  Description = request.Description,
    Permission = request.Permission,
     CreatedAt = DateTime.UtcNow,
    ExpiresAt = request.ExpiresInDays.HasValue
        ? DateTime.UtcNow.AddDays(request.ExpiresInDays.Value)
              : null,
            IsActive = true
        };

 context.ApiKeys.Add(apiKey);
        await context.SaveChangesAsync();

    _logger.LogInformation("New API key created: {Name} ({Permission}) by admin from localhost", 
            apiKey.Name, apiKey.Permission);

  return Ok(new ApiKeyResponse
   {
    Id = apiKey.Id,
     Key = apiKey.Key, // ���������� ���� ������ ��� ��������!
            Name = apiKey.Name,
      Owner = apiKey.Owner,
     Description = apiKey.Description,
            Permission = apiKey.Permission,
     CreatedAt = apiKey.CreatedAt,
     ExpiresAt = apiKey.ExpiresAt,
            IsActive = apiKey.IsActive
 });
    }

  /// <summary>
    /// ��������� ������ ���� API ������ (��� ������ �����, ������ � localhost)
    /// </summary>
    [HttpGet("apikeys")]
    public async Task<ActionResult<IEnumerable<ApiKeyInfo>>> GetAllApiKeys([FromQuery] string? masterPassword)
    {
     if (!IsLocalRequest())
        {
  return StatusCode(403, new { error = "Access denied. Only localhost is allowed." });
 }

        var configuredPassword = _configuration["ApiKeys:MasterPassword"];
  if (masterPassword != configuredPassword)
        {
       return Unauthorized(new { error = "Invalid master password" });
    }

        await using var context = await _contextFactory.CreateDbContextAsync();
     var apiKeys = await context.ApiKeys
      .OrderByDescending(k => k.CreatedAt)
      .Select(k => new ApiKeyInfo
        {
       Id = k.Id,
         Name = k.Name,
       Owner = k.Owner,
      Description = k.Description,
       Permission = k.Permission,
       CreatedAt = k.CreatedAt,
 ExpiresAt = k.ExpiresAt,
   IsActive = k.IsActive,
       LastUsedAt = k.LastUsedAt,
KeyPrefix = k.Key.Substring(0, Math.Min(8, k.Key.Length)) + "..." // ���������� ������ �������
  })
       .ToListAsync();

        return Ok(apiKeys);
    }

    /// <summary>
 /// ����������� API �����
    /// </summary>
    [HttpPut("apikeys/{id}/deactivate")]
 public async Task<IActionResult> DeactivateApiKey(int id, [FromQuery] string? masterPassword)
   {
     if (!IsLocalRequest())
        {
            return StatusCode(403, new { error = "Access denied. Only localhost is allowed." });
        }

   var configuredPassword = _configuration["ApiKeys:MasterPassword"];
if (masterPassword != configuredPassword)
        {
 return Unauthorized(new { error = "Invalid master password" });
        }

  await using var context = await _contextFactory.CreateDbContextAsync();
   var apiKey = await context.ApiKeys.FindAsync(id);
        
        if (apiKey == null)
  {
            return NotFound(new { error = "API key not found" });
    }

 apiKey.IsActive = false;
        await context.SaveChangesAsync();

        _logger.LogInformation("API key deactivated: {Name} (ID: {Id})", apiKey.Name, apiKey.Id);

     return Ok(new { message = "API key deactivated successfully" });
 }

    /// <summary>
    /// ��������� API �����
    /// </summary>
    [HttpPut("apikeys/{id}/activate")]
    public async Task<IActionResult> ActivateApiKey(int id, [FromQuery] string? masterPassword)
    {
     if (!IsLocalRequest())
        {
       return StatusCode(403, new { error = "Access denied. Only localhost is allowed." });
      }

   var configuredPassword = _configuration["ApiKeys:MasterPassword"];
  if (masterPassword != configuredPassword)
        {
       return Unauthorized(new { error = "Invalid master password" });
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
     var apiKey = await context.ApiKeys.FindAsync(id);
  
   if (apiKey == null)
 {
       return NotFound(new { error = "API key not found" });
   }

   apiKey.IsActive = true;
        await context.SaveChangesAsync();

     _logger.LogInformation("API key activated: {Name} (ID: {Id})", apiKey.Name, apiKey.Id);

 return Ok(new { message = "API key activated successfully" });
    }

    /// <summary>
    /// �������� API �����
 /// </summary>
    [HttpDelete("apikeys/{id}")]
    public async Task<IActionResult> DeleteApiKey(int id, [FromQuery] string? masterPassword)
    {
        if (!IsLocalRequest())
     {
   return StatusCode(403, new { error = "Access denied. Only localhost is allowed." });
   }

        var configuredPassword = _configuration["ApiKeys:MasterPassword"];
        if (masterPassword != configuredPassword)
        {
 return Unauthorized(new { error = "Invalid master password" });
   }

   await using var context = await _contextFactory.CreateDbContextAsync();
        var apiKey = await context.ApiKeys.FindAsync(id);
      
 if (apiKey == null)
        {
    return NotFound(new { error = "API key not found" });
   }

        context.ApiKeys.Remove(apiKey);
     await context.SaveChangesAsync();

        _logger.LogInformation("API key deleted: {Name} (ID: {Id})", apiKey.Name, apiKey.Id);

        return Ok(new { message = "API key deleted successfully" });
    }

    private bool IsLocalRequest()
    {
     var remoteIp = HttpContext.Connection.RemoteIpAddress;
     var localIp = HttpContext.Connection.LocalIpAddress;

        // ��������� localhost
 if (remoteIp == null || IPAddress.IsLoopback(remoteIp))
        {
 return true;
 }

        // ���������, ��������� �� remote � local IP (������ � ���� �� ����������)
        if (remoteIp.Equals(localIp))
   {
     return true;
 }

        return false;
 }

    private static string GenerateApiKey()
  {
     var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
     rng.GetBytes(bytes);
   return Convert.ToBase64String(bytes)
            .Replace("+", "")
  .Replace("/", "")
     .Replace("=", "")
          [..32];
    }
}

public record CreateApiKeyRequest(
 string Name,
    string? Owner,
    string? Description,
    ApiKeyPermission Permission,
  int? ExpiresInDays,
    string MasterPassword
);

public record ApiKeyResponse
{
    public int Id { get; init; }
    public required string Key { get; init; }
    public required string Name { get; init; }
    public string? Owner { get; init; }
    public string? Description { get; init; }
    public ApiKeyPermission Permission { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
  public bool IsActive { get; init; }
}

public record ApiKeyInfo
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? Owner { get; init; }
    public string? Description { get; init; }
    public ApiKeyPermission Permission { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastUsedAt { get; init; }
    public required string KeyPrefix { get; init; }
}
