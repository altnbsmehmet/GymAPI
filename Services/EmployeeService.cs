using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;
using Newtonsoft.Json;


public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    public EmployeeService(AppDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    public async Task<GetEmployeeResponse> CreateAsync(EmployeeDomain employeeDomain)
    {
        if (employeeDomain == null) return new GetEmployeeResponse { IsSuccess = false, Message = "Employee data is null." };
        var employee = new Employee {
            UserId = employeeDomain.UserId,
            Position = employeeDomain.Position,
            Salary = employeeDomain.Salary
        };
        _context.Employee.Add(employee);
        var employeeDto = _mapper.Map<Employee, EmployeeDto>(employee);
        await _context.SaveChangesAsync();
        return new GetEmployeeResponse { IsSuccess = true, Message = "Employee signed up.", Employee = employeeDto };
    }

    public async Task<GetEmployeesResponse> GetAllAsync()
    {
        var employees = await _context.Employee
            .Include(e => e.User)
            .ToListAsync();
        var employeeDto = _mapper.Map<List<Employee>, List<EmployeeDto>>(employees);
        return new GetEmployeesResponse { IsSuccess = true, Message = "Employees read.", Employees = employeeDto };
    }

    public async Task<GetEmployeesResponse> GetAllByPositionAsync(string position)
    {
        var employees = await _context.Employee
            .Include(e => e.User)
            .Where(e => e.Position == position)
            .ToListAsync();
        var employeeDto = _mapper.Map<List<Employee>, List<EmployeeDto>>(employees);
        return new GetEmployeesResponse { IsSuccess = true, Message = "Employees read.", Employees = employeeDto };
    }

    public async Task<GetEmployeeResponse> GetByIdAsync(int id)
    {
        var employee = await _context.Employee
            .Include(e => e.User)
            .FirstOrDefaultAsync(employee => employee.Id == id);
        if (employee == null) return new GetEmployeeResponse { IsSuccess = false, Message = $"No employee associated with given userId." };
        var employeeDto = _mapper.Map<Employee, EmployeeDto>(employee);
        return new GetEmployeeResponse { IsSuccess = true, Message = "Employee read.", Employee = employeeDto};
    }

    public async Task<GetEmployeeResponse> GetByUserIdAsync (string userId)
    {
        var employee = await _context.Employee
            .Include(e => e.User)
            .FirstOrDefaultAsync(employee => employee.UserId == userId);
        if (employee == null) return new GetEmployeeResponse { IsSuccess = false, Message = "No employee associated with given UserId." };
        var employeeDto = _mapper.Map<Employee, EmployeeDto>(employee);
        return new GetEmployeeResponse { IsSuccess = true, Message = "Employee read.", Employee = employeeDto};
    }

    public async Task<ResponseBase> UpdateAsync(UserDomain userDomain, string userId)
    {
        var employee = await _context.Employee.FirstOrDefaultAsync(employee => employee.UserId == userId);
        employee.Position = userDomain.Position;
        employee.Salary = (int)userDomain.Salary;
        await _context.SaveChangesAsync();
        return new ResponseBase { IsSuccess = true, Message = $"Employee updated." };
    }

    public async Task<ResponseBase> DeleteAsync(string id)
    {
        var employee = await _context.Employee.FirstOrDefaultAsync(e => e.UserId == id);
        _context.Remove(employee);
        await _context.SaveChangesAsync();
        return new ResponseBase { IsSuccess = true, Message = $"Employee deleted." };
    }

}