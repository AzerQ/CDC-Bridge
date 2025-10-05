namespace CdcBridge.Service.Workers;

public record CleanupWorkerConfiguration(int CleanupIntervalHours, int BufferTimeToLiveHours);

public record ReceiverWorkerConfiguration(int PollingIntervalMs, int BatchSize);

