using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


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
        var employeesResponse = await _employeeService.GetAllAsync();
        return Ok(employeesResponse);
    }

    [HttpGet("getall/{position}")]
    public async Task<IActionResult> GetAllEmployeesByPosition(string position)
    {
        var employees = await _employeeService.GetAllByPositionAsync(position);
        if (!employees.IsSuccess) return BadRequest(employees);
        return Ok(employees);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        var employee =  await _employeeService.GetByIdAsync(id);
        if (!employee.IsSuccess) return BadRequest(employee);
        return Ok(employee);
    }

}