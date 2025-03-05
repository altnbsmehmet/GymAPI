using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;


[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInDto signInDto)
    {
        var result = await _userService.SignInAsync(signInDto);
        if (!result.IsSuccess) return BadRequest(result);

        var token = result.Token;
        Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(1),
            });
        return Ok(result);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromForm] SignUpDto signUpDto)
    {
        var result = await _userService.SignUpAsync(signUpDto);
        return Ok(result);
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        if (!users.IsSuccess) return BadRequest(users);
        return Ok(users);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (!user.IsSuccess) return BadRequest(user);
        return Ok(user);
    }

    [Authorize]
    [HttpGet("getcurrentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync();
        if (!user.IsSuccess) return BadRequest(user);
        return Ok(user);
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateUser([FromBody] SignUpDto signUpDto, string id)
    {
        var result = await _userService.UpdateAsync(signUpDto, id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteAsnyc(id);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

}