using Microsoft.AspNetCore.Mvc;
using Data;
using Microsoft.AspNetCore.Authorization;


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
        var message = await _userService.SignInAsync(signInDto);
        return Ok(message);
    }

    [HttpPost("signup")]
    public async Task<string> SignUp([FromBody] UserDto userDto)
    {
        var result = await _userService.SignUpAsync(userDto);
        return result;
    }

    [HttpGet("getall")]
    public async Task<List<ApplicationUser>> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return users;
    }

    [HttpGet("getbyid/{id}")]
    public async Task<ApplicationUser> GetUserById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user;
    }

    [Authorize]
    [HttpGet("getcurrentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync();
        return Ok(user);
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateUser([FromBody] UserDto userDto, string id)
    {
        var result = await _userService.UpdateAsync(userDto, id);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteAsnyc(id);
        return Content(result);
    }

}
