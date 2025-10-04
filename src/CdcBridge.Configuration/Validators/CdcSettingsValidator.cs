using CdcBridge.Configuration.Models;

namespace CdcBridge.Configuration.Validators;

using FluentValidation;

public class CdcSettingsValidator: AbstractValidator<CdcSettings>
{
    public CdcSettingsValidator()
    {
        RuleFor(x => x.Connections)
            .NotEmpty().WithMessage("At least one connection is required")
            .ForEach(connectionRule => connectionRule.SetValidator(new ConnectionValidator()))
            .Custom((elements, context) => ValidateUniqueNames(elements.Select(x => x.Name), "connections", context));

        RuleFor(x => x.TrackingInstances)
            .NotEmpty().WithMessage("At least one tracking instance is required")
            .ForEach(tiRule => tiRule.SetValidator(new TrackingInstanceValidator()))
            .Custom((elements, context) => ValidateUniqueNames(elements.Select(x => x.Name), "trackingInstances", context));

        // Добавляем валидацию для filters, transformers и receivers если они есть
        When(x => x.Filters.Any(), () =>
        {
            RuleFor(x => x.Filters)
                .ForEach(filterRule => filterRule.SetValidator(new FilterValidator()))
                .Custom((elements, context) => ValidateUniqueNames(elements.Select(x => x.Name), "filters", context));;
        });

        When(x => x.Transformers.Any(), () =>
        {
            RuleFor(x => x.Transformers)
                .ForEach(transformerRule => transformerRule.SetValidator(new TransformerValidator()))
                .Custom((elements, context) => ValidateUniqueNames(elements.Select(x => x.Name), "transformers", context));
        });

        When(x => x.Receivers.Any(), () =>
        {
            RuleFor(x => x.Receivers)
                .ForEach(receiverRule => receiverRule.SetValidator(new ReceiverValidator()))
                .Custom((elements, context) => ValidateUniqueNames(elements.Select(x => x.Name), "receivers", context));
        });

        RuleFor(x => x).Custom(ValidateCrossReferences);
    }

    private void ValidateUniqueNames(IEnumerable<string> names, string sectionName, ValidationContext<CdcSettings> context)
    {
        var duplicates = names
            .GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
        {
            context.AddFailure($"Duplicate names found in section {sectionName}: {string.Join(", ", duplicates)}");
        }
    }

    private void ValidateCrossReferences(CdcSettings settings, ValidationContext<CdcSettings> context)
    {
        var connectionNames = settings.Connections.Select(c => c.Name).ToHashSet();
        var trackingInstanceNames = settings.TrackingInstances.Select(t => t.Name).ToHashSet();
        var filterNames = settings.Filters?.Select(f => f.Name).ToHashSet() ?? new HashSet<string>();
        var transformerNames = settings.Transformers?.Select(t => t.Name).ToHashSet() ?? new HashSet<string>();

        // Проверка tracking instances ссылаются на существующие connections
        settings.TrackingInstances
            .Where(ti => !connectionNames.Contains(ti.Connection))
            .ToList()
            .ForEach(ti => context.AddFailure($"Tracking instance '{ti.Name}' references unknown connection: {ti.Connection}"));

        // Проверка filters ссылаются на существующие tracking instances
        settings.Filters?
            .Where(f => !trackingInstanceNames.Contains(f.TrackingInstance))
            .ToList()
            .ForEach(f => context.AddFailure($"Filter '{f.Name}' references unknown tracking instance: {f.TrackingInstance}"));

        // Проверка transformers ссылаются на существующие tracking instances
        settings.Transformers?
            .Where(t => !string.IsNullOrEmpty(t.TrackingInstance) && !trackingInstanceNames.Contains(t.TrackingInstance!))
            .ToList()
            .ForEach(t => context.AddFailure($"Transformer '{t.Name}' references unknown tracking instance: {t.TrackingInstance}"));

        // Проверка receivers ссылаются на существующие tracking instances, filters и transformers
        settings.Receivers?
            .Where(r => !trackingInstanceNames.Contains(r.TrackingInstance))
            .ToList()
            .ForEach(r => context.AddFailure($"Receiver '{r.Name}' references unknown tracking instance: {r.TrackingInstance}"));

        settings.Receivers?
            .Where(r => !string.IsNullOrEmpty(r.Filter) && !filterNames.Contains(r.Filter!))
            .ToList()
            .ForEach(r => context.AddFailure($"Receiver '{r.Name}' references unknown filter: {r.Filter}"));

        settings.Receivers?
            .Where(r => !string.IsNullOrEmpty(r.Transformer) && !transformerNames.Contains(r.Transformer!))
            .ToList()
            .ForEach(r => context.AddFailure($"Receiver '{r.Name}' references unknown transformer: {r.Transformer}"));
    }
}