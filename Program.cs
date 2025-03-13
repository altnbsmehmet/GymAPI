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

var isDevelopment = builder.Environment.IsDevelopment();
string GetConfigValue(string envVarName, string appSettingsKey){
    var envValue = Environment.GetEnvironmentVariable(envVarName);
    return !string.IsNullOrEmpty(envValue) ? envValue : builder.Configuration[appSettingsKey];
}
var frontendUrl = GetConfigValue("FRONTEND_URL", "FrontendUrl");
var dbConnectionString = GetConfigValue("DB_CONNECTION_STRING", "ConnectionStrings:DefaultConnection");
var jwtSecret = GetConfigValue("JWT_SECRET", "Jwt:Key");
var jwtIssuer = GetConfigValue("JWT_ISSUER", "Jwt:Issuer");
var jwtAudience = GetConfigValue("JWT_AUDIENCE", "Jwt:Audience");
builder.Services.Configure<JwtSettings>(options =>
{
    options.Key = jwtSecret;
    options.Issuer = jwtIssuer;
    options.Audience = jwtAudience;
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
    options.UseNpgsql(dbConnectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie settings (for cookie authentication)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always; // Yerelde HTTP destekle
    options.Cookie.SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None; // Yerelde Lax, prod'da None
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret)),
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
        .WithOrigins(frontendUrl)  // Replace with your frontend URL
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