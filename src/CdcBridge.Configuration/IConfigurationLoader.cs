using CdcBridge.Configuration.Models;

namespace CdcBridge.Configuration;

public interface IConfigurationLoader
{
    CdcSettings LoadConfiguration(string filePath);
    CdcSettings LoadConfigurationFromString(string yamlContent);
}