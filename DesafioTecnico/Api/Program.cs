using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using DesafioTecnico.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Os appsettings ficam em Api/ mas o content root é a raiz do projeto (.csproj).
// Adicionamos explicitamente para que JWT e connection string funcionem com dotnet run.
builder.Configuration
    .AddJsonFile(Path.Combine("Api", "appsettings.json"), optional: true, reloadOnChange: false)
    .AddJsonFile(Path.Combine("Api", $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<DesafioTecnico.Api.Filters.ValidateRouteGuidsFilter>();
});
builder.Services.AddAutoMapper(typeof(DesafioTecnico.Api.Mapping.AutoMapperProfile));
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Token JWT obtido em POST /api/auth/login. Cole apenas o valor do token, sem o prefixo 'Bearer'."
        };
        return Task.CompletedTask;
    });
    options.AddOperationTransformer((operation, context, ct) =>
    {
        var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
            .Any(m => m is Microsoft.AspNetCore.Authorization.AuthorizeAttribute);
        var isAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);
        if (hasAuthorize && !isAnonymous)
        {
            operation.Security = [new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = []
            }];
        }
        return Task.CompletedTask;
    });
});

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
// SQLite for local dev and integration tests; SQL Server for staging/prod (docker).
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("IntegrationTests"))
{
    // Usa caminho absoluto em Development para evitar ambiguidade de working directory
    // entre o startup (seed) e as requisições em runtime.
    var sqliteConn = builder.Environment.IsDevelopment()
        ? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "desafio_dev.db")}"
        : connection;
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqliteConn));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
}
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Data.IUnitOfWork, DesafioTecnico.Infrastructure.Data.UnitOfWork>();
builder.Services.AddScoped<DesafioTecnico.Infrastructure.Services.Interfaces.IClienteService, DesafioTecnico.Infrastructure.Services.ClienteService>();
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

var jwtKey = builder.Configuration["Jwt:Key"] ?? "Dev_FallbackKey_NotForProduction_MinLength32chars!";
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DesafioTecnicoApi",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DesafioTecnicoApiUsers",
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});

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
    if (app.Environment.IsDevelopment())
    {
        // Deleta o arquivo físico do SQLite para garantir esquema limpo.
        // EnsureDeleted() apenas dropa tabelas sem apagar o arquivo, causando
        // problemas de WAL quando uma nova conexão é aberta pelo SeedData.
        // OpenConnection() mantém o mesmo handle aberto para EnsureCreated e
        // SeedData compartilharem — necessário para SQLite file-based.
        var dbPath = Path.Combine(app.Environment.ContentRootPath, "desafio_dev.db");
        if (File.Exists(dbPath))
            File.Delete(dbPath);

        db.Database.OpenConnection();
        db.Database.EnsureCreated();
    }
    SeedData.EnsureSeedData(db);
}

app.UseMiddleware<DesafioTecnico.Api.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
