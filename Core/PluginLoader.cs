using Plugin.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

namespace Core;

/// <summary>
/// Loads and manages plugins using Prise.
/// </summary>
public class PluginLoader
{
    private readonly string _pluginPath;

    public PluginLoader()
    {
        _pluginPath = Path.Combine(AppContext.BaseDirectory, "plugins");
        Directory.CreateDirectory(_pluginPath); // Ensure plugins directory exists
    }

    public async Task<IEnumerable<ISourcePlugin>> LoadSourcePluginsAsync()
    {
        var plugins = new List<ISourcePlugin>();
        var pluginFiles = Directory.GetFiles(_pluginPath, "*.dll");
        
        foreach (var file in pluginFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                var types = assembly.GetTypes()
                    .Where(t => typeof(ISourcePlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                
                foreach (var type in types)
                {
                    var plugin = Activator.CreateInstance(type) as ISourcePlugin;
                    if (plugin != null)
                    {
                        plugins.Add(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception but continue with other plugins
                Console.WriteLine($"Error loading plugin from {file}: {ex.Message}");
            }
        }
        
        return plugins;
    }

    public async Task<IEnumerable<ISinkPlugin>> LoadSinkPluginsAsync()
    {
        var plugins = new List<ISinkPlugin>();
        var pluginFiles = Directory.GetFiles(_pluginPath, "*.dll");
        
        foreach (var file in pluginFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                var types = assembly.GetTypes()
                    .Where(t => typeof(ISinkPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                
                foreach (var type in types)
                {
                    var plugin = Activator.CreateInstance(type) as ISinkPlugin;
                    if (plugin != null)
                    {
                        plugins.Add(plugin);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception but continue with other plugins
                Console.WriteLine($"Error loading plugin from {file}: {ex.Message}");
            }
        }
        
        return plugins;
    }

    public async Task<ISourcePlugin> LoadSourcePluginAsync(string pluginAssemblyName)
    {
        var pluginPath = Path.Combine(_pluginPath, $"{pluginAssemblyName}.dll");
        if (!File.Exists(pluginPath))
        {
            throw new FileNotFoundException($"Plugin {pluginAssemblyName} not found");
        }
        
        try
        {
            var assembly = Assembly.LoadFrom(pluginPath);
            var type = assembly.GetTypes()
                .FirstOrDefault(t => typeof(ISourcePlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            
            if (type == null)
            {
                throw new InvalidOperationException($"No ISourcePlugin implementation found in {pluginAssemblyName}");
            }
            
            return Activator.CreateInstance(type) as ISourcePlugin;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading plugin {pluginAssemblyName}: {ex.Message}", ex);
        }
    }

    public async Task<ISinkPlugin> LoadSinkPluginAsync(string pluginAssemblyName)
    {
        var pluginPath = Path.Combine(_pluginPath, $"{pluginAssemblyName}.dll");
        if (!File.Exists(pluginPath))
        {
            throw new FileNotFoundException($"Plugin {pluginAssemblyName} not found");
        }
        
        try
        {
            var assembly = Assembly.LoadFrom(pluginPath);
            var type = assembly.GetTypes()
                .FirstOrDefault(t => typeof(ISinkPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            
            if (type == null)
            {
                throw new InvalidOperationException($"No ISinkPlugin implementation found in {pluginAssemblyName}");
            }
            
            return Activator.CreateInstance(type) as ISinkPlugin;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading plugin {pluginAssemblyName}: {ex.Message}", ex);
        }
    }
}