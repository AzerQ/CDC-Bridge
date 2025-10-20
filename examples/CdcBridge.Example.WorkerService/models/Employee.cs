namespace CdcBridge.Example.WorkerService.models;

public class Employee
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public int? DepartmentId { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}