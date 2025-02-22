using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/member")]
public class MemberController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MemberController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllMembers()
    {
        var members = await _memberService.GetAllAsync();
        return Ok(members);
    }

    [HttpGet("getbyId/{id}")]
    public async Task<IActionResult> GetMemberById(int id)
    {
        var member =  await _memberService.GetByIdAsync(id);
        return Ok(member);
    }

}
