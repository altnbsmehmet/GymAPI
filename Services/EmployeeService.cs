using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;


public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    public EmployeeService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<string> CreateAsync(EmployeeDomain employeeDomain)
    {
        try {
            var employee = new Employee {
                UserId = employeeDomain.UserId,
                Position = employeeDomain.Position,
                Salary = employeeDomain.Salary
            };
            _context.Employee.Add(employee);
            await _context.SaveChangesAsync();
            return "Employee signed up.";
        } catch (Exception e) {
            return $"Error --> {e.Message}";
        }
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        var employees = await _context.Employee
        .Include(e => e.User)
        .ToListAsync();
        return employees;
    }

    public async Task<Employee> GetByIdAsync(int id)
    {
        var employee = await _context.Employee
            .Include(e => e.User)
            .FirstOrDefaultAsync(employee => employee.Id == id);
        return employee;
    }

    public async Task<string> UpdateAsync(UserDomain userDomain, string id)
    {
        try {
            var employee = await _context.Employee.FirstOrDefaultAsync(employee => employee.UserId == id);
            employee.Position = userDomain.Position;
            employee.Salary = (int)userDomain.Salary;
            await _context.SaveChangesAsync();
            return $"Employee successfully updated.";
        } catch (Exception e) {
            return $"Exception --> {e}";
        }
    }

    public async Task<string> DeleteAsync(string id)
    {
        var employee = await _context.Employee.FirstOrDefaultAsync(e => e.UserId == id);
        _context.Remove(employee);
        await _context.SaveChangesAsync();
        return $"Employee successfully deleted.";
    }

}
