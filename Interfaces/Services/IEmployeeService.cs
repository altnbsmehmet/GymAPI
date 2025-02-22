using Data;

public interface IEmployeeService
{
    
    Task<string> CreateAsync(EmployeeDomain employeeDomain);

    Task<List<Employee>> GetAllAsync();

    Task<Employee> GetByIdAsync(int id);

    Task<string> UpdateAsync(UserDomain userDomain, string id);

    Task<string> DeleteAsync(string id);
}