using Refit;

namespace CdcBridge.ApiClient.Interfaces;

/// <summary>
/// Refit interface для работы с Admin API.
/// </summary>
public interface IAdminApi
{
    /// <summary>
    /// Создает новый API ключ.
    /// </summary>
    [Post("/api/admin/apikeys")]
    Task<ApiKeyResponse> CreateApiKeyAsync([Body] CreateApiKeyRequest request);

    /// <summary>
    /// Получает список всех API ключей.
    /// </summary>
    [Get("/api/admin/apikeys")]
    Task<List<ApiKeyInfo>> GetAllApiKeysAsync([Query] string? masterPassword);

    /// <summary>
    /// Деактивирует API ключ.
    /// </summary>
    [Put("/api/admin/apikeys/{id}/deactivate")]
    Task DeactivateApiKeyAsync(int id, [Query] string? masterPassword);

    /// <summary>
    /// Активирует API ключ.
    /// </summary>
    [Put("/api/admin/apikeys/{id}/activate")]
    Task ActivateApiKeyAsync(int id, [Query] string? masterPassword);

    /// <summary>
    /// Удаляет API ключ.
    /// </summary>
    [Delete("/api/admin/apikeys/{id}")]
    Task DeleteApiKeyAsync(int id, [Query] string? masterPassword);
}

/// <summary>
/// Запрос на создание API ключа.
/// </summary>
public record CreateApiKeyRequest(
    string Name,
    string? Owner,
    string? Description,
    ApiKeyPermission Permission,
    int? ExpiresInDays,
    string MasterPassword
);

/// <summary>
/// Ответ при создании API ключа.
/// </summary>
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

/// <summary>
/// Информация об API ключе.
/// </summary>
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

/// <summary>
/// Права доступа API ключа.
/// </summary>
public enum ApiKeyPermission
{
    ReadOnly = 0,
    ReadWrite = 1,
    Admin = 2
}
