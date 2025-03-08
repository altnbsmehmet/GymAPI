using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.HttpResults;


[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("getcurrentuser")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync();
        return Ok(user);
    }

    [Authorize]
    [HttpGet("authorizeuser")]
    public async Task<IActionResult> AuthorizeUser()
    {
        Console.WriteLine("\n\nAuthorize endpoint Hit!\n\n");
        var result = await _userService.AuthorizeUserAsync();
        return Ok(result);
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInDto signInDto)
    {
        var result = await _userService.SignInAsync(signInDto);
        if (!result.IsSuccess) return Ok(result);

        return Ok(result);
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromForm] SignUpDto signUpDto)
    {
        var result = await _userService.SignUpAsync(signUpDto);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("signout")]
    public async Task<IActionResult> SignOut()
    {
        var result = await _userService.SignOutAsync();
        return Ok(result);
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("getbyid/{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(user);
    }

    [Authorize]
    [HttpPatch("update/{id}")]
    public async Task<IActionResult> UpdateUser([FromBody] SignUpDto signUpDto, string id)
    {
        Console.WriteLine($"\nUserResult\n{JsonConvert.SerializeObject(signUpDto)}\nAnd id: {id}\n");
        var result = await _userService.UpdateAsync(signUpDto, id);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteAsnyc(id);
        return Ok(result);
    }

}