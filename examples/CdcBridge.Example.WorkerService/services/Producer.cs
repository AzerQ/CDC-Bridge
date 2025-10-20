namespace CdcBridge.Example.WorkerService.services;

public class Producer(ILogger<Producer> logger, IConfiguration configuration, DatabaseService dbService, DataGenerator dataGenerator)
    : BackgroundService
{
    private async Task RunAutoMode(CancellationToken stoppingToken)
    {
        logger.LogInformation("Running in AUTO mode. Press Ctrl+C to stop.");

        var actions = new Func<Task>[]
        {
            InsertRandomEmployee,
            UpdateRandomEmployee,
            InsertRandomProject,
            UpdateRandomProject,
            AssignRandomEmployeeToProject
        };

        var random = new Random();

        int secondsDelayInterval = configuration.GetValue<int>("Intervals:ChangesDelayIntervalInSeconds");
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var action = actions[random.Next(actions.Length)];
                await action();
                await Task.Delay(secondsDelayInterval * 1_000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Auto mode stopped.");
        }
    }

    private async Task InsertRandomEmployee()
    {
        var employee = dataGenerator.GenerateEmployee();
        var result = await dbService.InsertEmployeeAsync(employee);
        logger.LogInformation($"[INSERT] Employee: {employee.FirstName} {employee.LastName} ({employee.Email})");
    }

    private async Task UpdateRandomEmployee()
    {
        var employees = (await dbService.GetEmployeesAsync()).ToList();
        if (employees.Any())
        {
            var randomEmployee = employees[new Random().Next(employees.Count)];
            var updatedEmployee = dataGenerator.GenerateEmployeeUpdate(randomEmployee);
            var result = await dbService.UpdateEmployeeAsync(updatedEmployee);
            logger.LogInformation(
                $"[UPDATE] Employee: {updatedEmployee.FirstName} {updatedEmployee.LastName} (ID: {updatedEmployee.Id})");
        }
        else
        {
            logger.LogInformation("No employees found to update.");
        }
    }

    private async Task DeleteRandomEmployee()
    {
        var employees = (await dbService.GetEmployeesAsync()).ToList();
        if (employees.Any())
        {
            var randomEmployee = employees[new Random().Next(employees.Count)];
            var result = await dbService.DeleteEmployeeAsync(randomEmployee.Id);
            logger.LogInformation(
                $"[DELETE] Employee: {randomEmployee.FirstName} {randomEmployee.LastName} (ID: {randomEmployee.Id})");
        }
        else
        {
            logger.LogInformation("No employees found to delete.");
        }
    }

    private async Task InsertRandomDepartment()
    {
        var department = dataGenerator.GenerateDepartment();
        var result = await dbService.InsertDepartmentAsync(department);
        logger.LogInformation($"[INSERT] Department: {department.Name} in {department.Location}");
    }

    private async Task UpdateRandomDepartment()
    {
        var departments = (await dbService.GetDepartmentsAsync()).ToList();
        if (departments.Any())
        {
            var randomDept = departments[new Random().Next(departments.Count)];
            randomDept.Budget = randomDept.Budget * 1.1m; // Increase budget by 10%
            var result = await dbService.UpdateDepartmentAsync(randomDept);
            logger.LogInformation($"[UPDATE] Department: {randomDept.Name} (New Budget: {randomDept.Budget:C})");
        }
        else
        {
            logger.LogInformation("No departments found to update.");
        }
    }

    private async Task InsertRandomProject()
    {
        var project = dataGenerator.GenerateProject();
        var result = await dbService.InsertProjectAsync(project);
        logger.LogInformation($"[INSERT] Project: {project.Name} (Status: {project.Status})");
    }

    private async Task UpdateRandomProject()
    {
        var projects = (await dbService.GetProjectsAsync()).ToList();
        if (projects.Any())
        {
            var randomProject = projects[new Random().Next(projects.Count)];
            randomProject.Status = "Completed";
            randomProject.EndDate = DateTime.Now;
            var result = await dbService.UpdateProjectAsync(randomProject);
            logger.LogInformation($"[UPDATE] Project: {randomProject.Name} (Status: {randomProject.Status})");
        }
        else
        {
            logger.LogInformation("No projects found to update.");
        }
    }

    private async Task AssignRandomEmployeeToProject()
    {
        var employees = (await dbService.GetEmployeesAsync()).ToList();
        var projects = (await dbService.GetProjectsAsync()).ToList();

        if (employees.Any() && projects.Any())
        {
            var randomEmployee = employees[new Random().Next(employees.Count)];
            var randomProject = projects[new Random().Next(projects.Count)];

            var assignment = dataGenerator.GenerateEmployeeProjectAssignment(randomEmployee.Id, randomProject.Id);
            var result = await dbService.AssignEmployeeToProjectAsync(assignment);

            logger.LogInformation(
                $"[ASSIGN] Employee {randomEmployee.FirstName} {randomEmployee.LastName} to project '{randomProject.Name}' as {assignment.Role}");
        }
        else
        {
            logger.LogInformation("Need both employees and projects to create assignments.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Database Data Produce Simulator");

        await RunAutoMode(stoppingToken);
    }
}