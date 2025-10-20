namespace CdcBridge.Example.WorkerService.models;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public string Status { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime CreatedAt { get; set; }
}