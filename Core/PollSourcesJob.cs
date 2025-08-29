using Plugin.Contracts;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core;

/// <summary>
/// Quartz.NET job that polls source plugins for changes and sends them to sink plugins.
/// </summary>
public class PollSourcesJob : IJob
{
    /// <summary>
    /// Executes the job to poll sources and process changes.
    /// </summary>
    /// <param name="context">The execution context.</param>
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var dataMap = context.JobDetail.JobDataMap;
            var sources = (IEnumerable<ISourcePlugin>)dataMap["sources"];
            var sinks = (IEnumerable<ISinkPlugin>)dataMap["sinks"];
            var logger = (EventLogger)dataMap["logger"];
            var eventProcessor = (EventProcessor)dataMap["eventProcessor"];

            foreach (var source in sources)
            {
                var changes = await source.GetChangesAsync(CancellationToken.None);
                
                // Process events through the event processor (filtering and transformation)
                var processedChanges = eventProcessor.ProcessEvents(changes);
                
                foreach (var change in processedChanges)
                {
                    // Log the processed event
                    logger.LogEvent(change);
                    
                    // Send to all sinks
                    foreach (var sink in sinks)
                    {
                        await sink.SendAsync(change, CancellationToken.None);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception but don't rethrow to prevent the scheduler from marking the job as failed
            Console.WriteLine($"Error in PollSourcesJob: {ex.Message}");
        }
    }
}