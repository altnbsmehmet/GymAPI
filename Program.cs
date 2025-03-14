using Microsoft.EntityFrameworkCore;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.WebHost.UseUrls("http://0.0.0.0:5410");

var _isDevelopment = builder.Environment.IsDevelopment();
var _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? builder.Configuration["FrontendUrl"];
var _dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
var _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["Jwt:Key"];
var _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
Console.WriteLine($"\n\n\tEnvironmental Variables\nIsDevelopment --> {_isDevelopment}\nFrontendUrl --> {_frontendUrl}\nDbConnectionString --> {_dbConnectionString}\nJwtSecret --> {_jwtSecret}\nJwtIssuer --> {_jwtIssuer}\nJwtAudience --> {_jwtAudience}\n\n");

builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = _jwtSecret;
    options.Issuer = _jwtIssuer;
    options.Audience = _jwtAudience;
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(_dbConnectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie settings (for cookie authentication)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = _isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always; // Yerelde HTTP destekle
    options.Cookie.SameSite = _isDevelopment ? SameSiteMode.Lax : SameSiteMode.Lax; // Yerelde Lax, production'da None
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // Unauthorized
        return Task.CompletedTask;
    };
});

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _jwtIssuer,
        ValidAudience = _jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSecret)),
    };

    // Cookie'den JWT'yi almak için:
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Cookie'den JWT'yi al
            var jwt = context.HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(jwt))
            {
                context.Token = jwt;  // Cookie'deki token'ı alıp, context'e atıyoruz
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"\nAuthentication failed: {context.Exception.Message} \n");
            return Task.CompletedTask;
        }
    };
});

// CORS (Cross-Origin Resource Sharing) Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder => builder
        .WithOrigins(_frontendUrl)  // Replace with your frontend URL
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Static file and routing setup
app.UseStaticFiles();
app.UseRouting();

// Apply CORS policy
app.UseCors("AllowFrontend");

app.UseAuthentication();  // Authentication middleware
app.UseAuthorization();   // Authorization middleware

app.MapControllers();

app.Run();