using CdcBridge.Configuration.Converters;
using CdcBridge.Configuration.Models;
using CdcBridge.Configuration.Preprocessing;
using CdcBridge.Configuration.Validators;
using FluentValidation;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CdcBridge.Configuration;

public class CdcConfigurationContextBuilder : ICdcConfigurationContextBuilder
{
	private readonly IDeserializer _deserializer;
	
	private readonly AbstractValidator<CdcSettings> _cdcSettingsValidator;
	
	private readonly YamlProcessor? _yamlProcessor;
	
	private CdcSettings _cdcSettings = new CdcSettings {Connections = [], Receivers = [], TrackingInstances = []};

	public CdcConfigurationContextBuilder(YamlProcessor? yamlProcessor = null)
	{
		_yamlProcessor = yamlProcessor;
		_deserializer = new DeserializerBuilder()
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.WithTypeConverter(new JsonElementConverter())
			.IgnoreUnmatchedProperties()
			.Build();
		
		_cdcSettingsValidator = new CdcSettingsValidator();
	}

	public ICdcConfigurationContextBuilder AddConfigurationFromFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException($"Configuration file not found: {filePath}");
		}

		var yamlContent = File.ReadAllText(filePath);
		
		if (_yamlProcessor != null)
		{
			yamlContent = _yamlProcessor.Process(yamlContent, Path.GetFullPath(filePath));
		}
		
		return AddConfigurationFromString(yamlContent);
	}

	public ICdcConfigurationContextBuilder AddConfigurationFromString(string yamlContent)
	{ 
			var settings = _deserializer.Deserialize<CdcSettings>(yamlContent);
			ValidateConfiguration(settings);
			return AddConfiguration(settings);
	}

	public ICdcConfigurationContextBuilder AddConfiguration(CdcSettings newSettings)
	{
		ValidateConfiguration(newSettings);
		_cdcSettings = _cdcSettings.Merge(newSettings);
		return this;
	}

	public ICdcConfigurationContext Build()
	{
		return new CdcConfigurationContext(_cdcSettings);
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