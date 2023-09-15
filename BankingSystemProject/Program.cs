using BankingSystem.Data;
using BankingSystem.Services.Implementations;
using BankingSystem.Services.Interfaces;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Models;
using BankingSystemProject.Services.Implementations;
using BankingSystemProject.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// DBContext-ის კონფიგურაცია
builder.Services.AddDbContext<BankingDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("LocalServer"));
});

//JWT Identity -ის კონფიგურიაცია
builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequiredLength = 5;
})  
    .AddEntityFrameworkStores<BankingDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Value,
        ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value))
    };
});

//Swagger Authentication -ის კონფიგურაცია
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Banking System API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
        new string[]{ }
        }
    });
}); 

//AutoMapper-ის კონფიგურირება
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Serilog-ის კონფიგურირება ფაილსა და ბაზაში ლოგების შესანახად
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File("Logs/log.xml", rollingInterval: RollingInterval.Day) // ფაილში ლოგირება
    .WriteTo.MSSqlServer(builder.Configuration.GetConnectionString("LocalServer"), "ErrorLog") // ბაზაში ლოგირება
    .CreateLogger();
builder.Host.UseSerilog();

//Adding Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IATMService, ATMService>();
builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<INetBankService, NetBankService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<ExchangeRateUpdateService>();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Authentication -ის Pipeline-ის კონფიგურირება
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseSerilogRequestLogging();

app.Run();
