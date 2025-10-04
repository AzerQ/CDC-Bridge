using CdcBridge.Configuration.Converters;
using CdcBridge.Configuration.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CdcBridge.Configuration;

public class ConfigurationLoader : IConfigurationLoader
{
	private readonly IDeserializer _deserializer;

	public ConfigurationLoader()
	{
		_deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.WithTypeConverter(new JsonElementConverter())
			.IgnoreUnmatchedProperties()
			.Build();
	}

	public CdcSettings LoadConfiguration(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Configuration file not found: {filePath}");
		}

		var yamlContent = File.ReadAllText(filePath);
		return LoadConfigurationFromString(yamlContent);
	}

	public CdcSettings LoadConfigurationFromString(string yamlContent)
	{
		try
		{
			var settings = _deserializer.Deserialize<CdcSettings>(yamlContent);
			ValidateConfiguration(settings);
			return settings;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException("Failed to parse YAML configuration", ex);
		}
	}

	private void ValidateConfiguration(CdcSettings settings)
	{
		if (settings.Connections == null || !settings.Connections.Any())
			throw new InvalidOperationException("Configuration must contain at least one connection");

		if (settings.TrackingInstances == null || !settings.TrackingInstances.Any())
			throw new InvalidOperationException("Configuration must contain at least one tracking instance");

		// Проверка уникальности имен
		ValidateUniqueNames(settings.Connections.Select(c => c.Name), "connections");
		ValidateUniqueNames(settings.Filters?.Select(f => f.Name) ?? Enumerable.Empty<string>(), "filters");
		ValidateUniqueNames(settings.Transformers?.Select(t => t.Name) ?? Enumerable.Empty<string>(), "transformers");
		ValidateUniqueNames(settings.Receivers?.Select(r => r.Name) ?? Enumerable.Empty<string>(), "receivers");
	}

	private void ValidateUniqueNames(IEnumerable<string> names, string sectionName)
	{
		var duplicates = names
			.GroupBy(x => x)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		if (duplicates.Any())
		{
			throw new InvalidOperationException(
				$"Duplicate names found in {sectionName}: {string.Join(", ", duplicates)}");
		}
	}
}

