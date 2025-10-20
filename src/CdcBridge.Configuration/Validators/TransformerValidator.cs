using CdcBridge.Configuration.Models;
using FluentValidation;

namespace CdcBridge.Configuration.Validators;

public class TransformerValidator : AbstractValidator<Transformer>
{
    public TransformerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Transformer name is required");
        RuleFor(x => x.Type).NotEmpty().WithMessage("Transformer type is required");
        
        // Проверка параметров в зависимости от типа трансформера
        RuleFor(x => x).Custom((transformer, context) =>
        {
            
            // Можно добавить другие типы трансформеров
            switch (transformer.Type)
            {
                case "JSONataTransformer":
                    bool hasTransformationProperty = transformer.Parameters.TryGetProperty("transformation", out var transformationProperty);
                    if (!hasTransformationProperty)
                    {
                        context.AddFailure($"JSONataTransformer '{transformer.Name}' requires 'transformation' parameter");
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(transformationProperty.GetString()))
                        {
                            context.AddFailure($"JSONataTransformer '{transformer.Name}' has empty 'transformation' parameter");
                        }
                    }
                    break;
            }
        });
        
        // Проверка, что trackingInstance существует (будет проверяться в cross-reference validation)
        When(x => !string.IsNullOrEmpty(x.TrackingInstance), () =>
        {
            RuleFor(x => x.TrackingInstance).NotEmpty().WithMessage("Tracking instance reference cannot be empty if provided");
        });
    }
}