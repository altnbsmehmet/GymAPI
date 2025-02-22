using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("/api/employee")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllEmployees()
    {
        var employees = await _employeeService.GetAllAsync();
        return Ok(employees);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        var employee =  await _employeeService.GetByIdAsync(id);
        return Ok(employee);
    }

}
