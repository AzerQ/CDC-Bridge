using FluentValidation.Results;

namespace CdcBridge.Configuration;

public class CdcConfigurationLoadException : Exception
{
	
    private readonly ValidationResult? _validationResult;
	
    public CdcConfigurationLoadException(ValidationResult validationResult): base("Failed to load configuration file: " +
        $"{string.Join(",", validationResult.Errors.Select(err => $"(FieldName={err.PropertyName}, Message={err.ErrorMessage})"))}")
    {
        _validationResult = validationResult;
    }

    public CdcConfigurationLoadException(string message) : base(message)
    {
    }

    public CdcConfigurationLoadException(string message, Exception inner) : base(message, inner)
    {
    }
}