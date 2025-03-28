using Microsoft.EntityFrameworkCore;
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.WebHost.UseUrls("http://0.0.0.0:5410");

EnvironmentVariables.IsDevelopment = builder.Environment.IsDevelopment();
EnvironmentVariables.ApiDomainUrl = Environment.GetEnvironmentVariable("API_DOMAIN_URL") ?? builder.Configuration["ApiDomainUrl"] ?? "";
EnvironmentVariables.FrontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? builder.Configuration["FrontendUrl"] ?? "";
EnvironmentVariables.SeqUrl = Environment.GetEnvironmentVariable("SEQ_URL") ?? builder.Configuration["SeqUrl"] ?? "";
EnvironmentVariables.DbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? builder.Configuration["ConnectionStrings:DefaultConnection"] ?? "";
EnvironmentVariables.JwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["Jwt:Key"] ?? "";
EnvironmentVariables.JwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"] ?? "";
EnvironmentVariables.JwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"] ?? "";
Console.WriteLine($"\n\n\tEnvironmental Variables\n" +
    $"IsDevelopment --> {EnvironmentVariables.IsDevelopment}\n" +
    $"ApiUrl --> {EnvironmentVariables.ApiDomainUrl}\n" +
    $"FrontendUrl --> {EnvironmentVariables.FrontendUrl}\n" +
    $"SeqUrl --> {EnvironmentVariables.SeqUrl}\n" +
    $"DbConnectionString --> {EnvironmentVariables.DbConnectionString}\n" +
    $"JwtSecret --> {EnvironmentVariables.JwtSecret}\n" +
    $"JwtIssuer --> {EnvironmentVariables.JwtIssuer}\n" +
    $"JwtAudience --> {EnvironmentVariables.JwtAudience}\n\n");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(EnvironmentVariables.SeqUrl)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(EnvironmentVariables.DbConnectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = EnvironmentVariables.IsDevelopment ? CookieSecurePolicy.None : CookieSecurePolicy.Always; // support http in local
    options.Cookie.SameSite = EnvironmentVariables.IsDevelopment ? SameSiteMode.Lax : SameSiteMode.Lax; // Lax in local, None in production
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // 401 means unauthorized
        return Task.CompletedTask;
    };
});

// jwt authentication
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
        ValidIssuer = EnvironmentVariables.JwtIssuer,
        ValidAudience = EnvironmentVariables.JwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(EnvironmentVariables.JwtSecret)),
    };

    // to get jwt from cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // taking jwt from cookie
            var jwt = context.HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(jwt))
            {
                context.Token = jwt;
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder => builder
        .WithOrigins(EnvironmentVariables.FrontendUrl)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

builder.Services.AddAuthorization();

// Seq Testing
Log.Information("Test Information.");
Log.Warning("Test Warning.");
Log.Error("Test Error.");

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();