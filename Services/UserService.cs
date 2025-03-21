using Microsoft.EntityFrameworkCore;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using AutoMapper;
using Newtonsoft.Json;


public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    public UserService(AppDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }
    public IEmployeeService GetEmployeeService()
    {
        return _serviceProvider.GetRequiredService<IEmployeeService>();
    }
    public IMemberService GetMemberService()
    {
        return _serviceProvider.GetRequiredService<IMemberService>();
    }

    public async Task<ResponseBase> Test()
    {
        await _context.Employee.AddAsync(null);
        return new ResponseBase { IsSuccess = true, Message = "success"};
    }

    public async Task<GetUserResponse> GetCurrentUserAsync()
    {
        var _employeeService = GetEmployeeService();
        var _memberService = GetMemberService();
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

        var userDto = _mapper.Map<ApplicationUser, UserDto>(user);

        var employeeResult = await _employeeService.GetByUserIdAsync(user.Id);
        if (employeeResult.IsSuccess) {
            return new GetUserResponse { IsSuccess = true, Message = "Employee read.", User = userDto, Employee = employeeResult.Employee };
        }

        var memberResult = await _memberService.GetByUserIdAsync(user.Id);
        if (memberResult.IsSuccess) {
            return new GetUserResponse { IsSuccess = true, Message = "Member read.", User = userDto, Member = memberResult.Member };                
        }

        if (user.Role == "Admin") return new GetUserResponse { IsSuccess = true, Message = "User read.", User = userDto };

        return new GetUserResponse { IsSuccess = false, Message = "User couldn't find." };
    }

    public async Task<UserAuthorizationResponse> AuthorizeUserAsync()
    {
        var token = GetTokenFromRequest();
        if (string.IsNullOrEmpty(token))return new UserAuthorizationResponse { IsSuccess = false, Message = "Token is missing" };

        var principal = GetUserClaimsFormToken(token);
        if (principal == null) return new UserAuthorizationResponse { IsSuccess = false, Message = "Invalid token" };

        var user =  await _context.Users.FindAsync(principal.UserId);
        if (user == null) new GetUserResponse { IsSuccess = false, Message = "User coudln't read." };

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        return new UserAuthorizationResponse {
            IsSuccess = true,
            Message = "User is authorized",
            Role = role
        };
    }

    public async Task<SignInResponse> SignInAsync(SignInDto signInDto)
    {
        var signInDomain = _mapper.Map<SignInDto, SignInDomain>(signInDto);
        var user = await _userManager.FindByNameAsync(signInDomain.UserName);
        if (user == null) return new SignInResponse { IsSuccess = false, Message = "UserName not found." };

        var result = await _signInManager.CheckPasswordSignInAsync(user, signInDomain.Password, false);
        if (!result.Succeeded) return new SignInResponse { IsSuccess = false, Message = "Password is invalid." };

        var token = GenerateJwtToken(user);
        return new SignInResponse { IsSuccess = true, Message = $"Login Successfull for user {user.UserName}.", Token = token, Role = user.Role };
    }

    public async Task<ResponseBase> SignUpAsync(SignUpDto signUpDto)
    {
        if (signUpDto.Role != "Employee" && signUpDto.Role != "Member") {
            return new ResponseBase { IsSuccess = false, Message = "Signup is only allowed for Employee and Member roles." };
        }

        var _employeeService = GetEmployeeService();
        var _memberService = GetMemberService();

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

        var roleResult = await _userManager.AddToRoleAsync(user, userDomain.Role);
        if (!roleResult.Succeeded)
        {
            return new ResponseBase { IsSuccess = false, Message = $"Error assigning role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}" };
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
            if (!employeeResult.IsSuccess) {
                await _userManager.DeleteAsync(user);
                return new ResponseBase { IsSuccess = false, Message = "Error creating Employee." };
            }
            return new ResponseBase { IsSuccess = true, Message = "Employee signed up." };
        } else if (userDomain.Role == "Member") {
            var memberResult = await _memberService.CreateAsync(new MemberDomain {
                UserId = user.Id
            });
            if (!memberResult.IsSuccess) {
                await _userManager.DeleteAsync(user);
                return new ResponseBase { IsSuccess = false, Message = "Error creating Member." };
            }
            return new ResponseBase { IsSuccess = true, Message = "Member signed up." };
        }
        return new ResponseBase { IsSuccess = false, Message = "Unexpected Errorr." };
    }

    public async Task<ResponseBase> SignOutAsync()
    {
        await _signInManager.SignOutAsync();
        return new ResponseBase { IsSuccess = true, Message = $"Signed out." };
    }

    public async Task<GetUsersResponse> GetAllAsync()
    {
        var users = await _context.Users.ToListAsync();
        if (users == null) return new GetUsersResponse { IsSuccess = false, Message = "Users couldn't read." };
        var usersDto = _mapper.Map<List<ApplicationUser>, List<UserDto>>(users);
        return new GetUsersResponse { IsSuccess = true, Message = "Users read.", Users = usersDto };
    }

    public async Task<GetUserResponse> GetByIdAsync(string userId)
    {
        var user =  await _context.Users.FindAsync(userId);
        var userDto = _mapper.Map<ApplicationUser, UserDto>(user);
        if (user == null) new GetUserResponse { IsSuccess = false, Message = "User coudln't read." };
        return new GetUserResponse { IsSuccess = true, Message = "User read.", User = userDto };
    }

    public async Task<GetUserResponse> GetByUserNameAsync(string userName)
    {
        var user =  await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (user == null) return new GetUserResponse { IsSuccess = false, Message = "User couldn't read." };
        var userDto = _mapper.Map<ApplicationUser, UserDto>(user);
        return new GetUserResponse { IsSuccess = true, Message = "User read.", User = userDto };
    }

    public async Task<ResponseBase> UpdateAsync(SignUpDto signUpDto, string userId)
    {
        var _employeeService = GetEmployeeService();
        var _memberService = GetMemberService();
        var userDomain = _mapper.Map<SignUpDto, UserDomain>(signUpDto);

        var user = await _userManager.FindByIdAsync(userId);
        user.FirstName = userDomain.FirstName;
        user.LastName = userDomain.LastName;
        user.UserName = userDomain.UserName;
        await _userManager.UpdateAsync(user);

        if (userDomain.Role == "Employee") {
            var employeeUpdate = await _employeeService.UpdateAsync(userDomain, userId);
            Console.WriteLine($"\n\n{JsonConvert.SerializeObject(employeeUpdate, Formatting.Indented)}\n\n");
            return employeeUpdate;
        } else if (userDomain.Role == "Member") {
            var memberUpdateResponse = await _memberService.UpdateAsync(userDomain, userId);
            return memberUpdateResponse;
        } else if (userDomain.Role == "Admin") {
            return new ResponseBase { IsSuccess = false, Message = "Admin Updated." };
        }

        return new ResponseBase { IsSuccess = false, Message = "Wrong Role" };
    }

    public async Task<ResponseBase> DeleteAsync(string id)
    {
        var _employeeService = GetEmployeeService();
        var _memberService = GetMemberService();
        var user = await _userManager.FindByIdAsync(id);
        if (user.Role == "Employee") await _employeeService.DeleteAsync(id);
        if (user.Role == "Member") await _memberService.DeleteAsync(id);
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) return new ResponseBase { IsSuccess = false, Message = $"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}" };
        return new ResponseBase { IsSuccess = true, Message = $"User deleted." };
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EnvironmentVariables.JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(user.Id))
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));

        if (!string.IsNullOrEmpty(user.Id))
            claims.Add(new Claim("user_id", user.Id));

        if (!string.IsNullOrEmpty(user.UserName))
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName));

        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

        var roles = _userManager.GetRolesAsync(user).Result;
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // Unique Token ID>

        var token = new JwtSecurityToken(
            issuer: EnvironmentVariables.JwtIssuer,
            audience: EnvironmentVariables.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // Token expires in 2 hours
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserClaims GetUserClaimsFormToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(EnvironmentVariables.JwtSecret);

        var parameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = EnvironmentVariables.JwtIssuer,
            ValidAudience = EnvironmentVariables.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Token'ı doğrulama
        var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

        // JWT geçerli ise kullanıcı bilgilerini al
        if (validatedToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) {
            var userInfo = new UserClaims {
                UserId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                        ?? principal.FindFirst("user_id")?.Value,
                UserName = principal.FindFirst(ClaimTypes.Name)?.Value,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            };

            return userInfo;
        }

        return null; // Token geçersiz
    }

    private string GetTokenFromRequest()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token)) token = httpContext.Request.Cookies["jwt"];

        return token;
    }

}