// Название задачи: Реализация Core Service
// Описание задачи: Класс, управляющий загрузкой плагинов, расписанием опроса, обработкой событий и логированием.
// Чек-лист выполнения задачи:
// - [x] Загрузка плагинов
// - [x] Расписание опроса (с использованием Quartz.NET)
// - [x] Обработка событий (фильтрация, трансформация)
// - [x] Отправка событий
// - [x] Логирование

using Plugin.Contracts;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Core;

/// <summary>
/// Core service that manages plugin loading, polling schedule, event processing and logging.
/// </summary>
public class CoreService
{
    private readonly PluginLoader _pluginLoader;
    private IEnumerable<ISourcePlugin> _sources;
    private IEnumerable<ISinkPlugin> _sinks;
    private readonly EventLogger _logger;
    private readonly EventProcessor _eventProcessor;
    private IScheduler _scheduler;

    /// <summary>
    /// Initializes a new instance of the CoreService class.
    /// </summary>
    public CoreService()
    {
        _pluginLoader = new PluginLoader();
        _logger = new EventLogger();
        _eventProcessor = new EventProcessor();
    }

    /// <summary>
    /// Initializes a new instance of the CoreService class with the specified plugin loader.
    /// </summary>
    /// <param name="pluginLoader">The plugin loader to use.</param>
    public CoreService(PluginLoader pluginLoader)
    {
        _pluginLoader = pluginLoader;
        _logger = new EventLogger();
        _eventProcessor = new EventProcessor();
    }

    /// <summary>
    /// Starts the service, loads plugins and schedules polling.
    /// </summary>
    public async Task StartAsync()
    {
        _sources = await _pluginLoader.LoadSourcePluginsAsync();
        _sinks = await _pluginLoader.LoadSinkPluginsAsync();
        
        // Initialize Quartz scheduler
        var schedulerFactory = new StdSchedulerFactory();
        _scheduler = await schedulerFactory.GetScheduler();
        
        // Define the job and tie it to our PollSourcesJob class
        var jobDetail = JobBuilder.Create<PollSourcesJob>()
            .WithIdentity("pollSourcesJob", "cdcGroup")
            .Build();
        
        // Pass the required services to the job
        jobDetail.JobDataMap["sources"] = _sources;
        jobDetail.JobDataMap["sinks"] = _sinks;
        jobDetail.JobDataMap["logger"] = _logger;
        jobDetail.JobDataMap["eventProcessor"] = _eventProcessor;
        
        // Create a trigger that fires every 15 seconds
        var trigger = TriggerBuilder.Create()
            .WithIdentity("pollTrigger", "cdcGroup")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(15)
                .RepeatForever())
            .Build();
        
        // Schedule the job with the trigger
        await _scheduler.ScheduleJob(jobDetail, trigger);
        
        // Start the scheduler
        await _scheduler.Start();
    }

    /// <summary>
    /// Stops the service and shuts down the scheduler.
    /// </summary>
    public async Task StopAsync()
    {
        if (_scheduler != null)
        {
            await _scheduler.Shutdown();
        }
    }


    /// <summary>
    /// Gets the names of all loaded source plugins.
    /// </summary>
    /// <returns>A collection of source plugin names.</returns>
    public IEnumerable<string> GetLoadedSources() => _sources.Select(s => s.Name);

    /// <summary>
    /// Loads a source plugin by name and adds it to the collection of sources.
    /// </summary>
    /// <param name="pluginName">The name of the plugin to load.</param>
    public async Task LoadSourcePluginAsync(string pluginName)
    {
        var plugin = await _pluginLoader.LoadSourcePluginAsync(pluginName);
        if (plugin != null)
        {
            _sources = _sources.Append(plugin);
            
            // Update the job data map with the new sources collection
            if (_scheduler != null && _scheduler.IsStarted)
            {
                var jobDetail = await _scheduler.GetJobDetail(new JobKey("pollSourcesJob", "cdcGroup"));
                if (jobDetail != null)
                {
                    jobDetail.JobDataMap["sources"] = _sources;
                    jobDetail.JobDataMap["eventProcessor"] = _eventProcessor;
                    await _scheduler.AddJob(jobDetail, true);
                }
            }
        }
    }

    /// <summary>
    /// Gets the names of all loaded sink plugins.
    /// </summary>
    /// <returns>A collection of sink plugin names.</returns>
    public IEnumerable<string> GetLoadedSinks() => _sinks.Select(s => s.Name);

    /// <summary>
    /// Loads a sink plugin by name and adds it to the collection of sinks.
    /// </summary>
    /// <param name="pluginName">The name of the plugin to load.</param>
    public async Task LoadSinkPluginAsync(string pluginName)
    {
        var plugin = await _pluginLoader.LoadSinkPluginAsync(pluginName);
        if (plugin != null)
        {
            _sinks = _sinks.Append(plugin);
            
            // Update the job data map with the new sinks collection
            if (_scheduler != null && _scheduler.IsStarted)
            {
                var jobDetail = await _scheduler.GetJobDetail(new JobKey("pollSourcesJob", "cdcGroup"));
                if (jobDetail != null)
                {
                    jobDetail.JobDataMap["sinks"] = _sinks;
                    jobDetail.JobDataMap["eventProcessor"] = _eventProcessor;
                    await _scheduler.AddJob(jobDetail, true);
                }
            }
        }
    }
    
    /// <summary>
    /// Adds a filter to the event processor.
    /// </summary>
    /// <param name="filter">A function that returns true if the event should be processed, false if it should be filtered out.</param>
    public void AddFilter(Func<ChangeEvent, bool> filter)
    {
        _eventProcessor.AddFilter(filter);
    }
    
    /// <summary>
    /// Adds a transformer to the event processor.
    /// </summary>
    /// <param name="transformer">A function that transforms a change event into another change event.</param>
    public void AddTransformer(Func<ChangeEvent, ChangeEvent> transformer)
    {
        _eventProcessor.AddTransformer(transformer);
    }
}