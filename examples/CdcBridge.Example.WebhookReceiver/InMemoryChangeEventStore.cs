using System.Collections.Concurrent;
using System.Text.Json;

namespace CdcBridge.Example.WebhookReceiver;

// Простой сервис-Singleton для хранения полученных событий в потокобезопасной коллекции
public class InMemoryChangeEventStore
{
    private readonly ConcurrentBag<(string Entity, JsonElement Change)> _events = new();

    public void AddChange(string entity, JsonElement change)
    {
        _events.Add((entity, change));
    }

    public IEnumerable<(string Entity, JsonElement Change)> GetAllChanges()
    {
        return _events.ToList();
    }
}