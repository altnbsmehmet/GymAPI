using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("/api/membership")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test()
    {
        EmployeeDto dto = new EmployeeDto { Position = "Employee", Salary = 299, UserId = "5fglöş"};
        Type type = dto.GetType();
        var properties = type.GetProperties();
        return Ok(properties);
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateMembership([FromBody] MembershipDto membershipDto)
    {
        var message =  await _membershipService.CreateAsync(membershipDto);
        return Ok(message);
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllMemberships()
    {
        var memberships = await _membershipService.GetAllAsync();
        return Ok(memberships);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetMembershipById(int id)
    {
        var membership =  await _membershipService.GetByIdAsync(id);
        return Ok(membership);
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateMembership([FromBody] MembershipDto membershipDto, int id)
    {
        var message = await _membershipService.UpdateAsync(membershipDto, id);
        return Ok(message);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteMembership(int id)
    {
        var message = await _membershipService.DeleteAsync(id);
        return Ok(message);
    }

}
