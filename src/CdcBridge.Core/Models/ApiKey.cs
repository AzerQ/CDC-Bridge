namespace CdcBridge.Core.Models;

public class ApiKey
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public ApiKeyPermission Permission { get; set; } = ApiKeyPermission.ReadOnly;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }
}

public enum ApiKeyPermission
{
    ReadOnly = 0,
    ReadWrite = 1
}
