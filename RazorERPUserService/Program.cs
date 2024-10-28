using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RazorERPUserService.Data;
using RazorERPUserService.MiddleWare;
using RazorERPUserService.Repositories;
using RazorERPUserService.Services;
using RazorERPUserService.Mappings;
using System.Security.Claims;
using System.Text;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(UserMapping).Assembly);

//AddScoped is preferred, especially if DapperContext will be managing active connections
builder.Services.AddScoped<DapperContext>();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Reads settings from appsettings.json
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// IdentityServer - If implementing IdentityServer to handle JWT authentication
// it involves configuring IdentityServer to handle client credentials, API resources, and scopes,
// then modifying your API to validate tokens issued by IdentityServer.
//https://identityserver4.readthedocs.io/en/latest/topics/apis.html
//builder.Services.AddIdentityServer()
//    .AddDeveloperSigningCredential()
//    .AddInMemoryApiScopes
//    .AddInMemoryClients

//JWT Authentication Setup
builder.Services.AddAuthentication(x =>
{
x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    //x.Authority = "https://localhost:<port of id server>"; // The base URL of the IdentityServer if using Indentity server
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
        RoleClaimType = ClaimTypes.Role
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Register your repositories and services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Autorize section to add JWT bearer on requests
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RazorERP User Service", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RazorERP User Service");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Add ThrottlingMiddleware before Mapcontrollers
app.UseMiddleware<ThrottlingMiddleware>();

app.MapControllers();

app.Run();
