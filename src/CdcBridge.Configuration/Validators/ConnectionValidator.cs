using CdcBridge.Configuration.Models;
using FluentValidation;

namespace CdcBridge.Configuration.Validators;

public class ConnectionValidator : AbstractValidator<Connection>
{
    public ConnectionValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.ConnectionString).NotEmpty();
        RuleFor(x => x.Active).NotNull();
    }
}