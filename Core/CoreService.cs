// Название задачи: Реализация Core Service
// Описание задачи: Класс, управляющий загрузкой плагинов, расписанием опроса, обработкой событий и логированием.
// Чек-лист выполнения задачи:
// - [x] Загрузка плагинов
// - [ ] Расписание опроса
// - [ ] Обработка событий (фильтрация, трансформация)
// - [ ] Отправка событий
// - [ ] Логирование (будет в отдельной задаче)

using Plugin.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Core;

public class CoreService
{
    private readonly PluginLoader _pluginLoader;
    private readonly System.Timers.Timer _timer;
    private IEnumerable<ISourcePlugin> _sources;
    private IEnumerable<ISinkPlugin> _sinks;
    private readonly EventLogger _logger;

    public CoreService()
    {
        _pluginLoader = new PluginLoader();
        _timer = new System.Timers.Timer(15000); // Каждые 15 секунд
        _timer.Elapsed += OnTimerElapsed;
        _logger = new EventLogger();
    }

    public CoreService(PluginLoader pluginLoader)
    {
        _pluginLoader = pluginLoader;
        _timer = new System.Timers.Timer(15000); // Каждые 15 секунд
        _timer.Elapsed += OnTimerElapsed;
        _logger = new EventLogger();
    }

    public async Task StartAsync()
    {
        _sources = await _pluginLoader.LoadSourcePluginsAsync();
        _sinks = await _pluginLoader.LoadSinkPluginsAsync();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        foreach (var source in _sources)
        {
            var changes = await source.GetChangesAsync(CancellationToken.None);
            foreach (var change in changes)
            {
                _logger.LogEvent(change);
                // TODO: Фильтрация и трансформация
                foreach (var sink in _sinks)
                {
                    await sink.SendAsync(change, CancellationToken.None);
                }
            }
        }
    }

    public IEnumerable<string> GetLoadedSources() => _sources.Select(s => s.Name);

    public async Task LoadSourcePluginAsync(string pluginName)
    {
        var plugin = await _pluginLoader.LoadSourcePluginAsync(pluginName);
        if (plugin != null)
        {
            _sources = _sources.Append(plugin);
        }
    }

    public IEnumerable<string> GetLoadedSinks() => _sinks.Select(s => s.Name);

    public async Task LoadSinkPluginAsync(string pluginName)
    {
        var plugin = await _pluginLoader.LoadSinkPluginAsync(pluginName);
        if (plugin != null)
        {
            _sinks = _sinks.Append(plugin);
        }
    }
}