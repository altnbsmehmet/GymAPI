using Data;

public interface IEmployeeService
{
    
    Task<GetEmployeeResponse> CreateAsync(EmployeeDomain employeeDomain);

    Task<GetEmployeesResponse> GetAllAsync();

    Task<GetEmployeesResponse> GetAllByPositionAsync(string position);

    Task<GetEmployeeResponse> GetByIdAsync(int id);

    Task<GetEmployeeResponse> GetByUserIdAsync (string id);

    Task<ResponseBase> UpdateAsync(UserDomain userDomain, string id);

    Task<ResponseBase> DeleteAsync(string id);
}