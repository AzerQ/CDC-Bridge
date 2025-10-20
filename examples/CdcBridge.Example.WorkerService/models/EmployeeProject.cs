namespace CdcBridge.Example.WorkerService.models;

public class EmployeeProject
{
    public Guid EmployeeId { get; set; }
    public Guid ProjectId { get; set; }
    public string Role { get; set; }
    public DateTime AssignedDate { get; set; }
    public int? HoursPerWeek { get; set; }
}