using System.Text.Json;
using CdcBridge.Configuration.Extensions;
using CdcBridge.Configuration.Models;
using FluentValidation;

namespace CdcBridge.Configuration.Validators;

public class ReceiverValidator : AbstractValidator<Receiver>
{
    public ReceiverValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Receiver name is required");
        RuleFor(x => x.TrackingInstance).NotEmpty().WithMessage("Receiver tracking instance is required");
        RuleFor(x => x.Type).NotEmpty().WithMessage("Receiver type is required");
        RuleFor(x => x.RetryCount).GreaterThanOrEqualTo(0).WithMessage("Retry count must be greater than or equal to 0");
        
        // Проверка параметров в зависимости от типа ресивера
        RuleFor(x => x).Custom((receiver, context) =>
        {
            switch (receiver.Type)
            {
                case "webhook":
                    // Currently no specific validation for webhook receiver
                    break;
                
                default:
                    context.AddFailure($"Unknown receiver type: {receiver.Type} for receiver '{receiver.Name}'");
                    break;
            }
        });
        
        // Валидация опциональных ссылок на filter и transformer
        When(x => !string.IsNullOrEmpty(x.Filter), () =>
        {
            RuleFor(x => x.Filter).NotEmpty().WithMessage("Filter reference cannot be empty if provided");
        });
        
        When(x => !string.IsNullOrEmpty(x.Transformer), () =>
        {
            RuleFor(x => x.Transformer).NotEmpty().WithMessage("Transformer reference cannot be empty if provided");
        });
    }
    
    private void ValidateWebhookReceiver(Receiver receiver, ValidationContext<Receiver> context)
    {
        if (!receiver.Parameters.HasProperty("webhookUrl"))
        {
            context.AddFailure($"WebhookReceiver '{receiver.Name}' requires 'webhookUrl' parameter");
        }
        else
        {
            var webhookUrl = receiver.Parameters.GetStringProperty("webhookUrl");
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                context.AddFailure($"WebhookReceiver '{receiver.Name}' has empty 'webhookUrl' parameter");
            }
            else if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _))
            {
                context.AddFailure($"WebhookReceiver '{receiver.Name}' has invalid URL in 'webhookUrl' parameter: {webhookUrl}");
            }
        }
        
        if (!receiver.Parameters.HasProperty("httpMethod"))
        {
            context.AddFailure($"WebhookReceiver '{receiver.Name}' requires 'httpMethod' parameter");
        }
        else
        {
            var httpMethod = receiver.Parameters.GetStringProperty("httpMethod");
            var validMethods = new[] { "POST", "PUT", "PATCH", "GET", "DELETE" };
            if (!validMethods.Contains(httpMethod?.ToUpper()))
            {
                context.AddFailure($"WebhookReceiver '{receiver.Name}' has invalid HTTP method: {httpMethod}. Valid methods are: {string.Join(", ", validMethods)}");
            }
        }
        
        // Опциональная проверка headers
        if (receiver.Parameters.HasProperty("headers"))
        {
            var headers = receiver.Parameters.GetProperty("headers").Deserialize<Dictionary<string, string>>();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (string.IsNullOrWhiteSpace(header.Key) || string.IsNullOrWhiteSpace(header.Value))
                    {
                        context.AddFailure($"WebhookReceiver '{receiver.Name}' has invalid header: '{header.Key}'");
                    }
                }
            }
        }
    }
    
}