using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infraestructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(DesafioTecnico.Api.Mapping.AutoMapperProfile));
builder.Services.AddOpenApi();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
// Allow using Sqlite for integration tests environment
if (builder.Environment.IsEnvironment("IntegrationTests"))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
}
// Repositorios e services
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IClienteRepository, DesafioTecnico.Infraestructure.Repositories.ClienteRepository>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.Interfaces.IClienteService, DesafioTecnico.Infraestructure.Services.ClienteService>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IApartamentoRepository, DesafioTecnico.Infraestructure.Repositories.ApartamentoRepository>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IReservaRepository, DesafioTecnico.Infraestructure.Repositories.ReservaRepository>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IVendaRepository, DesafioTecnico.Infraestructure.Repositories.VendaRepository>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.Interfaces.IReservaService, DesafioTecnico.Infraestructure.Services.ReservaService>();
// Auth and token generator
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Security.JwtTokenGenerator>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.Interfaces.IAuthService, DesafioTecnico.Infraestructure.Services.AuthService>();
// Venda service
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.Interfaces.IVendaService, DesafioTecnico.Infraestructure.Services.VendaService>();
// Apartamento service
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.Interfaces.IApartamentoService, DesafioTecnico.Infraestructure.Services.ApartamentoService>();

// JWT Authentication
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

// Configure the HTTP request pipeline.
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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
