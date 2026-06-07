using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using DesafioTecnico.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<DesafioTecnico.Api.Filters.ValidateRouteGuidsFilter>();
});
builder.Services.AddAutoMapper(typeof(DesafioTecnico.Api.Mapping.AutoMapperProfile));
builder.Services.AddOpenApi();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
// SQLite for local dev and integration tests; SQL Server for staging/prod (docker).
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("IntegrationTests"))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
}
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Repositories.Interfaces.IClienteRepository, DesafioTecnico.Infrastructure.Repositories.ClienteRepository>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Services.Interfaces.IClienteService, DesafioTecnico.Infrastructure.Services.ClienteService>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Repositories.Interfaces.IApartamentoRepository, DesafioTecnico.Infrastructure.Repositories.ApartamentoRepository>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Repositories.Interfaces.IReservaRepository, DesafioTecnico.Infrastructure.Repositories.ReservaRepository>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Repositories.Interfaces.IVendaRepository, DesafioTecnico.Infrastructure.Repositories.VendaRepository>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Services.Interfaces.IReservaService, DesafioTecnico.Infrastructure.Services.ReservaService>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Security.JwtTokenGenerator>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Services.Interfaces.IAuthService, DesafioTecnico.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Services.Interfaces.IVendaService, DesafioTecnico.Infrastructure.Services.VendaService>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Services.Interfaces.IApartamentoService, DesafioTecnico.Infrastructure.Services.ApartamentoService>();

// Rate limiting — máx 5 tentativas de login por minuto por IP
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Seed initial data (admin user + demo apartment) so the system is usable right after migrations.
// Guards inside EnsureSeedData prevent duplicate inserts on subsequent startups.
if (!app.Environment.IsEnvironment("IntegrationTests"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.EnsureSeedData(db);
}

app.UseMiddleware<DesafioTecnico.Api.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
