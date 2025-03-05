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

    public async Task<GetUserResponse> GetCurrentUserAsync()
    {
        try {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            user.ProfilePhoto = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/{user.ProfilePhotoPath}";

            var employeeResult = await _employeeService.GetByUserIdAsync(user.Id);
            if (employeeResult.IsSuccess) {
                return new GetUserResponse { IsSuccess = true, Message = "User read.", User = user, Employee = employeeResult.Employee };
            }

            var memberResult = await _memberService.GetByUserIdAsync(user.Id);
            if (memberResult.IsSuccess) {
                return new GetUserResponse { IsSuccess = true, Message = "User read.", User = user, Member = memberResult.Member };                
            }

            return new GetUserResponse { IsSuccess = false, Message = "User couldn't find." };
        } catch (Exception e) {
            return new GetUserResponse { IsSuccess = false, Message = $"Exception --> {e.Message}" };
        }
    }

    public async Task<SignInResponse> SignInAsync(SignInDto signInDto)
    {
        try {
            var signInDomain = _mapper.Map<SignInDto, SignInDomain>(signInDto);
            var user = await _userManager.FindByNameAsync(signInDomain.UserName);
            if (user == null) return new SignInResponse { IsSuccess = false, Message = "UserName not found." };

            var result = await _signInManager.CheckPasswordSignInAsync(user, signInDomain.Password, false);
            if (!result.Succeeded) return new SignInResponse { IsSuccess = false, Message = "Password is invalid." };

            var token = GenerateJwtToken(user);
            return new SignInResponse { IsSuccess = true, Message = $"Login Successfull for user {user.UserName}.", Token = token };
        } catch (Exception e) {
            return new SignInResponse { IsSuccess = false, Message = $"Exception --> {e.Message}" };
        }
    }

    public async Task<ResponseBase> SignUpAsync(SignUpDto signUpDto)
    {
        var userDomain = _mapper.Map<SignUpDto, UserDomain>(signUpDto);

        var user = new ApplicationUser {
            UserName = userDomain.UserName,
            FirstName = userDomain.FirstName,
            LastName = userDomain.LastName,
            Role = userDomain.Role,
        };

        var userResult = await _userManager.CreateAsync(user, userDomain.Password);
        if (!userResult.Succeeded) {
            return new ResponseBase { IsSuccess = false, Message = $"Error signing up: {string.Join(", ", userResult.Errors.Select(e => e.Description))}" };
        }

        if (signUpDto.ProfilePhoto != null && signUpDto.ProfilePhoto.Length > 0) {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads/ProfilePhoto");
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(signUpDto.ProfilePhoto.FileName)}";
            var filePath = Path.Combine(folder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create)) {
                await signUpDto.ProfilePhoto.CopyToAsync(stream);
            }
            user.ProfilePhotoPath = $"Uploads/ProfilePhoto/{fileName}";
            await _userManager.UpdateAsync(user);
        }

        if (userDomain.Role == "Employee") {
            var employeeResult = await _employeeService.CreateAsync(new EmployeeDomain {
                UserId = user.Id,
                Position = userDomain.Position,
                Salary = (int)userDomain.Salary
            });
            return new ResponseBase { IsSuccess = true, Message = "Employee signed up." };
        } else if (userDomain.Role == "Member") {
            var memberResult = await _memberService.CreateAsync(new MemberDomain {
                UserId = user.Id
            });
            return new ResponseBase { IsSuccess = true, Message = "Member signed up." };
        }
        
        return new ResponseBase { IsSuccess = false, Message = "Wrong role." };
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<GetUsersResponse> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();
        if (users == null) return new GetUsersResponse { IsSuccess = false, Message = "Users couldn't read." };
        return new GetUsersResponse { IsSuccess = true, Message = "Users read.", Users = users };
    }

    public async Task<GetUserResponse> GetByIdAsync(string userId)
    {
        var user =  await _context.Users.FindAsync(userId);
        if (user == null) new GetUserResponse { IsSuccess = false, Message = "User coudln't read.", User = user };
        return new GetUserResponse { IsSuccess = true, Message = "User read.", User = user };
    }

    public async Task<ResponseBase> UpdateAsync(SignUpDto signUpDto, string id)
    {
        try {
            var userDomain = _mapper.Map<SignUpDto, UserDomain>(signUpDto);

            if (userDomain.Role == "Employee") {
                var employeeUpdate = await _employeeService.UpdateAsync(userDomain, id);
                return employeeUpdate;
            } else if (userDomain.Role == "Member") {
                var memberUpdate = await _memberService.UpdateAsync(userDomain, id);
                return memberUpdate;
            }
            var user = await _userManager.FindByIdAsync(id);
            user.FirstName = userDomain.FirstName;
            user.LastName = userDomain.LastName;
            user.Role = userDomain.Role;
            await _userManager.UpdateAsync(user);
            return new ResponseBase { IsSuccess = true, Message = "User updated" };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Error --> {e.Message}" };
        }
    }

    public async Task<ResponseBase> DeleteAsnyc(string id)
    {
        try {
            var user = await _userManager.FindByIdAsync(id);
            if (user.Role == "Employee") await _employeeService.DeleteAsync(id);
            if (user.Role == "Member") await _memberService.DeleteAsync(id);
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return new ResponseBase { IsSuccess = false, Message = $"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}" };
            return new ResponseBase { IsSuccess = true, Message = $"User deleted." };
        } catch (Exception e) {
            return new ResponseBase { IsSuccess = false, Message = $"Exception --> {e.Message}" };
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
