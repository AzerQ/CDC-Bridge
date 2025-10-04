using CdcBridge.Configuration.Converters;
using CdcBridge.Configuration.Models;
using CdcBridge.Configuration.Validators;
using FluentValidation;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CdcBridge.Configuration;

public class ConfigurationLoader : IConfigurationLoader
{
	private readonly IDeserializer _deserializer;
	
	private readonly AbstractValidator<CdcSettings> _cdcSettingsValidator;

	public ConfigurationLoader()
	{
		_deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.WithTypeConverter(new JsonElementConverter())
			.IgnoreUnmatchedProperties()
			.Build();
		
		_cdcSettingsValidator = new CdcSettingsValidator();
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
			var settings = _deserializer.Deserialize<CdcSettings>(yamlContent);
			ValidateConfiguration(settings);
			return settings;
	}

	private void ValidateConfiguration(CdcSettings settings)
	{
		var validationResult = _cdcSettingsValidator.Validate(settings);
		if (!validationResult.IsValid)
		{
			throw new CdcConfigurationLoadException(validationResult);
		}
	}
	
}