using System.Text.Json;
using CdcBridge.Application.CdcSources;
using CdcBridge.Configuration.Models;
using CdcBridge.Core.Abstractions;
using CdcBridge.Core.Models;

namespace CdcBridge.Example.WorkerService.services;

public class Consumer(ILogger<Consumer> logger, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Database Data Consumer Simulator");


        string connectionString = configuration.GetConnectionString("default") ??
                                  throw new Exception("Default connection string is null");
        
        int secondsPollingInterval = configuration.GetValue<int>("Intervals:ChangesPollingIntervalInSeconds");

        var connection = new Connection
        {
            ConnectionString = connectionString,
            Name = "ExampleConnection",
            Type = "SqlServerCdcSource"
        };
        
        
        var trackingInstance = new TrackingInstance
        {
            Name = "captureEmployees",
            Connection = string.Empty,
            SourceTable = "employee",
            SourceSchema = "dbo"
        };
        
        var trackingInstanceInfo = new TrackingInstanceInfo(trackingInstance, connection);
        
        ICdcSource cdcSource = new SqlServerCdcSource();

        await cdcSource.EnableTrackingInstance(trackingInstanceInfo);
        CdcRequest? cdcRequest = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            var changes = await cdcSource.GetChanges(trackingInstanceInfo, cdcRequest);
            var trackedChanges = changes.ToList();

            if (trackedChanges.Count != 0)
            {
                logger.LogInformation("Found {changesCount} changes", trackedChanges.Count);
                logger.LogInformation("Founded changes {changes}", JsonSerializer.Serialize(trackedChanges));
                cdcRequest = new()
                {
                    LastRowFlag = trackedChanges.Last().RowLabel
                };
            }

            else
            {
                logger.LogInformation("No changes");
            }

            await Task.Delay(secondsPollingInterval * 1_000, stoppingToken);
        }
    }
}