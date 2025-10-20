using CdcBridge.Configuration.Models;
using FluentValidation;

namespace CdcBridge.Configuration.Validators;

public class TrackingInstanceValidator : AbstractValidator<TrackingInstance>
{
    public TrackingInstanceValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.SourceTable).NotEmpty();
        RuleFor(x => x.CapturedColumns).NotEmpty();
        RuleFor(x => x.Connection).NotEmpty();
        RuleFor(x => x.CheckIntervalInSeconds).GreaterThan(0);
    }
}