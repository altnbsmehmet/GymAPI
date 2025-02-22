using Microsoft.EntityFrameworkCore;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using AutoMapper;


public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _config;
    private readonly IEmployeeService _employeeService;
    private readonly IMemberService _memberService;

    public UserService(AppDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, IConfiguration config, IEmployeeService employeeService, IMemberService memberService)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
        _config = config;
        _employeeService = employeeService;
        _memberService = memberService;
    }

    public async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        return user;
    }

    public async Task<Object> SignInAsync(SignInDto signInDto)
    {   
        try {
           var signInDomain = _mapper.Map<SignInDto, SignInDomain>(signInDto);
            var user = await _userManager.FindByNameAsync(signInDomain.UserName);
            if (user == null) return "UserName not found.";

            var result = await _signInManager.CheckPasswordSignInAsync(user, signInDomain.Password, false);
            if (!result.Succeeded) return "Password is invalid.";

            var token = GenerateJwtToken(user);
            return new { message = $"Login Successfull for user {user.UserName}.", Token = token };
        } catch (Exception e) {
            return $"Exception --> {e.Message}";
        }
    }

    public async Task<string> SignUpAsync(UserDto userDto)
    {
        var userDomain = _mapper.Map<UserDto, UserDomain>(userDto);
        
        var user = new ApplicationUser {
            UserName = userDomain.UserName,
            FirstName = userDomain.FirstName,
            LastName = userDomain.LastName,
            Role = userDomain.Role
        };

        if (userDomain.Role == "Employee") {
            try {
                var userResult = await _userManager.CreateAsync(user, userDomain.Password);
                if (userResult.Succeeded) {
                    var employeeResult = await _employeeService.CreateAsync(new EmployeeDomain {
                        UserId = user.Id,
                        Position = userDomain.Position,
                        Salary = (int)userDomain.Salary
                    });
                    return employeeResult;
                } else {
                    return $"Error signing up an employee: {string.Join(", ", userResult.Errors.Select(e => e.Description))}";
                }
            } catch (Exception e) {
                if (e.InnerException != null) return $"Error --> {e.Message} Inner Exception --> {e.InnerException.Message}";
                else return $"Error --> {e.Message}";
            }
        } else if (userDomain.Role == "Member") {
            try {
                var userResult = await _userManager.CreateAsync(user, userDomain.Password);
                if (userResult.Succeeded) {
                    var memberResult = await _memberService.CreateAsync(new MemberDomain {
                        UserId = user.Id
                    });
                    return memberResult;
                } else {
                    return $"Error signing up a member: {string.Join(", ", userResult.Errors.Select(e => e.Description))}";
                }
            } catch (Exception e) {
                return $"Error --> {e.Message}";
            }
        }
        return "Wrong role.";
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<List<ApplicationUser>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<ApplicationUser> GetByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<string> UpdateAsync(UserDto userDto, string id)
    {
        try {
            var userDomain = _mapper.Map<UserDto, UserDomain>(userDto);

            if (userDomain.Role == "Employee") {
                await _employeeService.UpdateAsync(userDomain, id);
                return "";
            } else if (userDomain.Role == "Member") {
                await _memberService.UpdateAsync(userDomain, id);
                return "";
            }
            var user = await _userManager.FindByIdAsync(id);
            user.FirstName = userDomain.FirstName;
            user.LastName = userDomain.LastName;
            user.Role = userDomain.Role;
            await _userManager.UpdateAsync(user);
            return "";
        } catch (Exception e) {
            return $"Error --> {e.Message}";
        }
    }

    public async Task<string> DeleteAsnyc(string id)
    {
        try {
            var user = await _userManager.FindByIdAsync(id);
            if (user.Role == "Employee") await _employeeService.DeleteAsync(id);
            if (user.Role == "Member") await _memberService.DeleteAsync(id);
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return $"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            return $"User deleted successfully.";
        } catch (Exception e) {
            return $"Exception --> {e.Message}";
        }
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(user.Id))
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));

        if (!string.IsNullOrEmpty(user.UserName))
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName));

        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        var roles = _userManager.GetRolesAsync(user).Result;
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // Unique Token ID>


        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // Token expires in 2 hours
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
