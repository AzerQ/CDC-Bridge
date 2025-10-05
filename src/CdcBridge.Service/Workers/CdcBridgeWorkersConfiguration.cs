namespace CdcBridge.Service.Workers;

public class CleanupWorkerConfiguration {
    public int CleanupIntervalHours { get; init; } 
    public int BufferTimeToLiveHours { get; init; } 

}

public class ReceiverWorkerConfiguration
{
    public int PollingIntervalMs { get; init; }
    public int BatchSize { get; init; }
    
}

