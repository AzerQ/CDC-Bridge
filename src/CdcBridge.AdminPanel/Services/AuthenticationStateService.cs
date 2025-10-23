using Blazored.LocalStorage;

namespace CdcBridge.AdminPanel.Services;

/// <summary>
/// Service for managing authentication state (API key storage).
/// </summary>
public class AuthenticationStateService
{
    private const string ApiKeyStorageKey = "cdc_bridge_api_key";
    private const string ApiBaseUrlStorageKey = "cdc_bridge_api_base_url";
    private const string MasterPasswordStorageKey = "cdc_bridge_master_password";

    private readonly ILocalStorageService _localStorage;

    public AuthenticationStateService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<string?> GetApiKeyAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<string>(ApiKeyStorageKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetApiKeyAsync(string apiKey)
    {
        await _localStorage.SetItemAsync(ApiKeyStorageKey, apiKey);
    }

    public async Task RemoveApiKeyAsync()
    {
        await _localStorage.RemoveItemAsync(ApiKeyStorageKey);
    }

    public async Task<string?> GetApiBaseUrlAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<string>(ApiBaseUrlStorageKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetApiBaseUrlAsync(string baseUrl)
    {
        await _localStorage.SetItemAsync(ApiBaseUrlStorageKey, baseUrl);
    }

    public async Task<string?> GetMasterPasswordAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<string>(MasterPasswordStorageKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetMasterPasswordAsync(string password)
    {
        await _localStorage.SetItemAsync(MasterPasswordStorageKey, password);
    }

    public async Task RemoveMasterPasswordAsync()
    {
        await _localStorage.RemoveItemAsync(MasterPasswordStorageKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var apiKey = await GetApiKeyAsync();
        return !string.IsNullOrEmpty(apiKey);
    }
}
