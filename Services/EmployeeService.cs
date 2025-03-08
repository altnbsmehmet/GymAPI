using Microsoft.EntityFrameworkCore;
using Data;
using AutoMapper;


public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public EmployeeService(AppDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetEmployeeResponse> CreateAsync(EmployeeDomain employeeDomain)
    {
        if (employeeDomain == null) return new GetEmployeeResponse { IsSuccess = false, Message = "Employee data is null." };
        try {
            var employee = new Employee {
                UserId = employeeDomain.UserId,
                Position = employeeDomain.Position,
                Salary = employeeDomain.Salary
            };
            _context.Employee.Add(employee);
            await _context.SaveChangesAsync();
            return new GetEmployeeResponse { IsSuccess = true, Message = "Employee signed up.", Employee = employee };
        } catch (Exception e) {
            return new GetEmployeeResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetEmployeesResponse> GetAllAsync()
    {
        try {
            var employees = await _context.Employee
            .Include(e => e.User)
            .ToListAsync();
            return new GetEmployeesResponse { IsSuccess = true, Message = "Employees read.", Employees = employees };
        } catch (Exception e) {
            return new GetEmployeesResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetEmployeesResponse> GetAllByPositionAsync(string position)
    {
        try {
            var employees = await _context.Employee
                .Include(e => e.User)
                .Where(e => e.Position == position)
                .ToListAsync();
            foreach (var employee in employees) {
                employee.User.ProfilePhoto = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/{employee.User.ProfilePhotoPath}";
            }
            return new GetEmployeesResponse { IsSuccess = true, Message = "Employees read.", Employees = employees };
        } catch (Exception e) {
            return new GetEmployeesResponse { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<GetEmployeeResponse> GetByIdAsync(int id)
    {
        try {
            var employee = await _context.Employee
                .Include(e => e.User)
                .FirstOrDefaultAsync(employee => employee.Id == id);
            if (employee == null) return new GetEmployeeResponse { IsSuccess = false, Message = $"No employee associated with given userId." };
            return new GetEmployeeResponse { IsSuccess = true, Message = "Employee read.", Employee = employee};
        } catch (Exception e) {
            return new GetEmployeeResponse { IsSuccess = false, Message = $"Exception --> {e.Message}" };
        }
    }

    public async Task<GetEmployeeResponse> GetByUserIdAsync (string id)
    {
        try {
            var employee = await _context.Employee
                .Include(e => e.User)
                .FirstOrDefaultAsync(employee => employee.UserId == id);
            if (employee == null) return new GetEmployeeResponse { IsSuccess = false, Message = "No employee associated with given UserId." };
            return new GetEmployeeResponse { IsSuccess = true, Message = "Employee read.", Employee = employee};
        } catch (Exception e) {
            return new GetEmployeeResponse { IsSuccess = false, Message = $"Exception --> {e.Message}" };
        }
    }

    public async Task<ResponseBase> UpdateAsync(UserDomain userDomain, string id)
    {
        try {
            var employee = await _context.Employee.FirstOrDefaultAsync(employee => employee.UserId == id);
            employee.Position = userDomain.Position;
            employee.Salary = (int)userDomain.Salary;
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = true, Message = $"Employee updated." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Exception --> {e}" };
        }
    }

    public async Task<ResponseBase> DeleteAsync(string id)
    {
        try {
            var employee = await _context.Employee.FirstOrDefaultAsync(e => e.UserId == id);
            _context.Remove(employee);
            await _context.SaveChangesAsync();
            return new ResponseBase { IsSuccess = true, Message = $"Employee deleted." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Exception --> {e}" };
        }
    }

}