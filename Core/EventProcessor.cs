// Название задачи: Обработка событий (фильтрация, трансформация)
// Описание задачи: Класс для фильтрации и трансформации событий изменений перед отправкой в стоки.
// Чек-лист выполнения задачи:
// - [x] Фильтрация событий
// - [x] Трансформация событий
// - [x] Документация

using Plugin.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core;

/// <summary>
/// Processes change events by filtering and transforming them before sending to sinks.
/// </summary>
public class EventProcessor
{
    private readonly List<Func<ChangeEvent, bool>> _filters = new();
    private readonly List<Func<ChangeEvent, ChangeEvent>> _transformers = new();

    /// <summary>
    /// Adds a filter to the processing pipeline.
    /// </summary>
    /// <param name="filter">A function that returns true if the event should be processed, false if it should be filtered out.</param>
    public void AddFilter(Func<ChangeEvent, bool> filter)
    {
        _filters.Add(filter);
    }

    /// <summary>
    /// Adds a transformer to the processing pipeline.
    /// </summary>
    /// <param name="transformer">A function that transforms a change event into another change event.</param>
    public void AddTransformer(Func<ChangeEvent, ChangeEvent> transformer)
    {
        _transformers.Add(transformer);
    }

    /// <summary>
    /// Processes a collection of change events by applying filters and transformers.
    /// </summary>
    /// <param name="events">The collection of change events to process.</param>
    /// <returns>The processed collection of change events.</returns>
    public IEnumerable<ChangeEvent> ProcessEvents(IEnumerable<ChangeEvent> events)
    {
        var result = events;

        // Apply filters
        foreach (var filter in _filters)
        {
            result = result.Where(filter);
        }

        // Apply transformers
        foreach (var transformer in _transformers)
        {
            result = result.Select(transformer);
        }

        return result;
    }

    /// <summary>
    /// Processes a single change event by applying filters and transformers.
    /// </summary>
    /// <param name="changeEvent">The change event to process.</param>
    /// <returns>The processed change event, or null if it was filtered out.</returns>
    public ChangeEvent ProcessEvent(ChangeEvent changeEvent)
    {
        // Apply filters
        foreach (var filter in _filters)
        {
            if (!filter(changeEvent))
            {
                return null; // Event filtered out
            }
        }

        // Apply transformers
        var result = changeEvent;
        foreach (var transformer in _transformers)
        {
            result = transformer(result);
        }

        return result;
    }
}

// Documentation:
// EventProcessor class provides filtering and transformation of ChangeEvent instances.
// Usage example:
// var processor = new EventProcessor();
// processor.AddFilter(e => e.Type != ChangeType.Delete); // Filter out delete events
// processor.AddTransformer(e => { e.Table = e.Table.ToLower(); return e; }); // Transform table names to lowercase
// var processedEvents = processor.ProcessEvents(sourceEvents);