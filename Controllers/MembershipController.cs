using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


[ApiController]
[Route("/api/membership")]
public class MembershipController : ControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateMembership([FromBody] MembershipDto membershipDto)
    {
        Console.WriteLine($"\n\n\tmembershipDto\n{JsonConvert.SerializeObject(membershipDto, Formatting.Indented)}\n\n");
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
        var membershipResponse =  await _membershipService.GetByIdAsync(id);
        return Ok(membershipResponse);
    }

    [Authorize]
    [HttpGet("toggleactivation/{id}")]
    public async Task<IActionResult> ToggleMembershipActivationById(int id)
    {
        Console.WriteLine($"\n\nid from endpoint: {id}\n\n");
        var membersipDeactivationResponse = await _membershipService.ToggleActivationByIdAsync(id);
        return Ok(membersipDeactivationResponse);
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
