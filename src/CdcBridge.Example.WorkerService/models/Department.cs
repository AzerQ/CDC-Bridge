namespace CdcBridge.Example.WorkerService.models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal? Budget { get; set; }
    public string Location { get; set; }
    public Guid? ManagerId { get; set; }
    public DateTime CreatedAt { get; set; }
}