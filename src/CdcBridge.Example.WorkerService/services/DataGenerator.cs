using Bogus;
using CdcBridge.Example.WorkerService.models;

namespace CdcBridge.Example.WorkerService.services;

public class DataGenerator
{
    private readonly Faker<Employee> _employeeFaker;
    private readonly Faker<Department> _departmentFaker;
    private readonly Faker<Project> _projectFaker;
    private readonly List<string> _roles = ["Developer", "Manager", "Analyst", "Tester", "Designer", "Architect"];
    private readonly List<string> _projectStatuses = ["Active", "Completed", "On Hold", "Cancelled"];

    public DataGenerator()
    {
        Randomizer.Seed = new Random();

        _employeeFaker = new Faker<Employee>()
            .RuleFor(e => e.Id, f => f.Random.Guid())
            .RuleFor(e => e.FirstName, f => f.Name.FirstName())
            .RuleFor(e => e.LastName, f => f.Name.LastName())
            .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.FirstName, e.LastName))
            .RuleFor(e => e.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(e => e.DepartmentId, f => f.Random.Int(1, 4))
            .RuleFor(e => e.Salary, f => f.Random.Decimal(40000, 120000))
            .RuleFor(e => e.HireDate, f => f.Date.Past(5))
            .RuleFor(e => e.IsActive, f => f.Random.Bool(0.8f))
            .RuleFor(e => e.CreatedAt, f => DateTime.Now)
            .RuleFor(e => e.UpdatedAt, f => DateTime.Now);

        _departmentFaker = new Faker<Department>()
            .RuleFor(d => d.Name, f => f.Commerce.Department())
            .RuleFor(d => d.Budget, f => f.Random.Decimal(100000, 500000))
            .RuleFor(d => d.Location, f => f.Address.City())
            .RuleFor(d => d.CreatedAt, f => DateTime.Now);

        _projectFaker = new Faker<Project>()
            .RuleFor(p => p.Id, f => f.Random.Guid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName() + " Project")
            .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
            .RuleFor(p => p.StartDate, f => f.Date.Soon(10))
            .RuleFor(p => p.EndDate, f => f.Date.Future(1))
            .RuleFor(p => p.Budget, f => f.Random.Decimal(50000, 300000))
            .RuleFor(p => p.Status, f => f.PickRandom(_projectStatuses))
            .RuleFor(p => p.DepartmentId, f => f.Random.Int(1, 4))
            .RuleFor(p => p.CreatedAt, f => DateTime.Now);
    }

    public Employee GenerateEmployee() => _employeeFaker.Generate();
    public Department GenerateDepartment() => _departmentFaker.Generate();
    public Project GenerateProject() => _projectFaker.Generate();

    public EmployeeProject GenerateEmployeeProjectAssignment(Guid employeeId, Guid projectId)
    {
        var faker = new Faker();
        return new EmployeeProject
        {
            EmployeeId = employeeId,
            ProjectId = projectId,
            Role = faker.PickRandom(_roles),
            AssignedDate = DateTime.Now,
            HoursPerWeek = faker.Random.Int(10, 40)
        };
    }

    public Employee GenerateEmployeeUpdate(Employee existingEmployee)
    {
        var faker = new Faker();
        existingEmployee.FirstName = faker.Name.FirstName();
        existingEmployee.LastName = faker.Name.LastName();
        existingEmployee.Email = faker.Internet.Email(existingEmployee.FirstName, existingEmployee.LastName);
        existingEmployee.Phone = faker.Phone.PhoneNumber();
        existingEmployee.Salary = faker.Random.Decimal(40000, 120000);
        existingEmployee.UpdatedAt = DateTime.Now;
        return existingEmployee;
    }
}