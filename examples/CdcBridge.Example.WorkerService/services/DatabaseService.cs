using System.Data.SqlClient;
using CdcBridge.Example.WorkerService.models;
using Dapper;

#pragma warning disable CS0618 // Type or member is obsolete

namespace CdcBridge.Example.WorkerService.services;

public class DatabaseService(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("default") ?? throw new Exception("Default connection string not found");

    public async Task<int> InsertEmployeeAsync(Employee employee)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO employee (id, first_name, last_name, email, phone, department_id, salary, hire_date, is_active, created_at, updated_at)
                   VALUES (@Id, @FirstName, @LastName, @Email, @Phone, @DepartmentId, @Salary, @HireDate, @IsActive, @CreatedAt, @UpdatedAt)";
        return await connection.ExecuteAsync(sql, employee);
    }

    public async Task<int> UpdateEmployeeAsync(Employee employee)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"UPDATE employee SET 
                   first_name = @FirstName, 
                   last_name = @LastName, 
                   email = @Email, 
                   phone = @Phone, 
                   department_id = @DepartmentId, 
                   salary = @Salary, 
                   is_active = @IsActive,
                   updated_at = @UpdatedAt
                   WHERE id = @Id";
        return await connection.ExecuteAsync(sql, employee);
    }

    public async Task<int> DeleteEmployeeAsync(Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = "DELETE FROM employee WHERE id = @Id";
        return await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> InsertDepartmentAsync(Department department)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO department (name, budget, location, manager_id, created_at)
                   VALUES (@Name, @Budget, @Location, @ManagerId, @CreatedAt)";
        return await connection.ExecuteAsync(sql, department);
    }

    public async Task<int> UpdateDepartmentAsync(Department department)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"UPDATE department SET 
                   name = @Name, 
                   budget = @Budget, 
                   location = @Location, 
                   manager_id = @ManagerId
                   WHERE id = @Id";
        return await connection.ExecuteAsync(sql, department);
    }

    public async Task<int> InsertProjectAsync(Project project)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO project (id, name, description, start_date, end_date, budget, status, department_id, created_at)
                   VALUES (@Id, @Name, @Description, @StartDate, @EndDate, @Budget, @Status, @DepartmentId, @CreatedAt)";
        return await connection.ExecuteAsync(sql, project);
    }

    public async Task<int> UpdateProjectAsync(Project project)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"UPDATE project SET 
                   name = @Name, 
                   description = @Description, 
                   start_date = @StartDate, 
                   end_date = @EndDate, 
                   budget = @Budget, 
                   status = @Status, 
                   department_id = @DepartmentId
                   WHERE id = @Id";
        return await connection.ExecuteAsync(sql, project);
    }

    public async Task<int> AssignEmployeeToProjectAsync(EmployeeProject assignment)
    {
        await using var connection = new SqlConnection(_connectionString);
        var sql = @"INSERT INTO employee_project (employee_id, project_id, role, assigned_date, hours_per_week)
                   VALUES (@EmployeeId, @ProjectId, @Role, @AssignedDate, @HoursPerWeek)";
        return await connection.ExecuteAsync(sql, assignment);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Employee>("SELECT * FROM employee");
    }

    public async Task<IEnumerable<Department>> GetDepartmentsAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Department>("SELECT * FROM department");
    }

    public async Task<IEnumerable<Project>> GetProjectsAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Project>("SELECT * FROM project");
    }
}