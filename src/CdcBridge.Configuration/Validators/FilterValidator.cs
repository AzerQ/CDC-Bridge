using CdcBridge.Configuration.Extensions;
using CdcBridge.Configuration.Models;
using FluentValidation;

namespace CdcBridge.Configuration.Validators;

public class FilterValidator : AbstractValidator<Filter>
{
    public FilterValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.TrackingInstance).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
        
        RuleFor(x => x).Custom((filter, context) =>
        {
            switch (filter.Type)
            {
                case "JsonPathFilter":
                    if (!filter.Parameters.HasProperty("expression"))
                        context.AddFailure("JsonPathFilter requires 'expression' parameter");
                    break;
            }
        });
    }
}