using FamLedger.Application.Interfaces;
using FamLedger.Application.Options;
using FamLedger.Application.Profiles;
using FamLedger.Application.Services;
using FamLedger.Infrastructure.Data;
using FamLedger.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    // Fail fast during boot so Fly marks the machine unhealthy instead of
    // letting the API come up and 500 on every request.
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured. " +
        "Set it via appsettings, environment variable, or `fly secrets set`.");
}

builder.Services.AddDbContext<FamLedgerDbContext>(options => options.UseNpgsql(connectionString));

var jwtSettings = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? string.Empty))
        };
    });

//Common configuration binding.
builder.Services.Configure<CommonOptions>(builder.Configuration.GetSection("Configuration"));

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy
            .WithOrigins(allowedOrigins!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContextService>();

builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();

builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();

builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
builder.Services.AddScoped<IFamilyInviteRepository, FamilyInviteRepository>();
builder.Services.AddScoped<IFamilyService, FamilyService>();

// Add services to the container.
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FamLedger API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Paste only JWT token (without 'Bearer ').",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

app.UseCors("FrontendCors");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In production (Fly), the edge proxy terminates TLS and forwards plain HTTP
// to the container. Forcing HTTPS redirect inside would bounce every request
// and break health checks. Only redirect in local dev.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Liveness probe for Fly / UptimeRobot. Public, no auth. Deliberately
// does NOT check the database — a DB blip shouldn't kill the machine.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

app.MapControllers();

app.Run();
