using Microsoft.EntityFrameworkCore;
using DesafioTecnico.Infraestructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(DesafioTecnico.Api.Mapping.AutoMapperProfile));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
// Repositórios e serviços
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IClienteRepository, DesafioTecnico.Infraestructure.Repositories.ClienteRepository>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.Interfaces.IClienteService, DesafioTecnico.Infraestructure.Services.ClienteService>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IApartamentoRepository, DesafioTecnico.Infraestructure.Repositories.ApartamentoRepository>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IReservaRepository, DesafioTecnico.Infraestructure.Repositories.ReservaRepository>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Repositories.Interfaces.IVendaRepository, DesafioTecnico.Infraestructure.Repositories.VendaRepository>();
// Auth and token generator
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Security.JwtTokenGenerator>();
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.AuthService>();
// Venda service
builder.Services.AddScoped<DesafioTecnico.Infraestructure.Services.Interfaces.IVendaService, DesafioTecnico.Infraestructure.Services.VendaService>();

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
}

// Apply seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.EnsureSeedData(db);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
