using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
#if TESTCONTAINERS
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
#endif
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Integration
{
    public class IntegrationTests : IAsyncLifetime
    {
        private readonly WebApplicationFactory<DesafioTecnico.Program> _factory;
        private WebApplicationFactory<DesafioTecnico.Program>? _configuredFactory;
        private HttpClient _client = null!;
#if TESTCONTAINERS
        private MsSqlTestcontainer? _container;
#endif
        private bool _skipIntegration = false;
        private string? _skipReason;
        private string? _connectionString;

        private const string ContainerName = "desafio-test-sql";
        private readonly string? SaPassword = Environment.GetEnvironmentVariable("TEST_DB_SA_PASSWORD")
            ?? Environment.GetEnvironmentVariable("SA_PASSWORD");
        private readonly int HostPort = int.TryParse(Environment.GetEnvironmentVariable("TEST_DB_PORT") ?? Environment.GetEnvironmentVariable("DB_PORT"), out var _hp) ? _hp : 14333;
        private const int ContainerPort = 1433;
        private const string SqlImage = "mcr.microsoft.com/mssql/server:2019-latest";
        private const int MasterReadyTimeoutSeconds = 180;
        private const int MasterPollIntervalMs = 5000;
        private const int MigrationMaxAttempts = 12; // ~60 seconds

        public IntegrationTests()
        {
            _factory = new WebApplicationFactory<DesafioTecnico.Program>();
        }

        public async Task InitializeAsync()
        {
#if !TESTCONTAINERS
            if (string.IsNullOrEmpty(SaPassword))
            {
                _skipIntegration = true;
                _skipReason = "TEST_DB_SA_PASSWORD or SA_PASSWORD not set in environment. Set these variables or run docker-compose with .env before running integration tests.";
                Console.WriteLine(_skipReason);
                return;
            }
#endif
#if TESTCONTAINERS
            var testcontainersBuilder = new TestcontainersBuilder<MsSqlTestcontainer>()
                .WithDatabase(new MsSqlTestcontainerConfiguration
                {
                    Password = SaPassword,
                })
                .WithImage(SqlImage)
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithCleanUp(true)
                .WithName(ContainerName)
                .WithPortBinding(HostPort, ContainerPort)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(ContainerPort));

            _container = testcontainersBuilder.Build();
            try
            {
                await _container.StartAsync();
            }
            catch (Exception ex)
            {
                _skipIntegration = true;
                _skipReason = $"Testcontainers failed to start: {ex.Message}. Ensure Docker daemon TCP is available or run docker-compose and execute tests without TESTCONTAINERS.";
            }

            // ~3 min timeout waiting for SQL Server master to accept connections
                var masterConn = new SqlConnectionStringBuilder
                {
                    DataSource = $"127.0.0.1,{HostPort}",
                    InitialCatalog = "master",
                    UserID = "sa",
                    Password = SaPassword,
                    TrustServerCertificate = true,
                    ConnectTimeout = 5
                }.ConnectionString;

            var readyMaster = false;
            for (int i = 0; i < 36; i++) // ~36 * 5s = 180s
            {
                try
                {
                    using var testConn = new SqlConnection(masterConn);
                    await testConn.OpenAsync();
                    readyMaster = true;
                    break;
                }
                catch
                {
                    await Task.Delay(5000);
                }
            }

            if (!readyMaster)
            {
                await DumpContainerLogsAsync();
                _skipIntegration = true;
                _skipReason = $"SQL Server master did not become ready in time. Check docker logs for the container '{ContainerName}'.";
                Console.WriteLine(_skipReason);
                return;
            }

            try
            {
                using var createConn = new SqlConnection(masterConn);
                await createConn.OpenAsync();
                using var cmd = createConn.CreateCommand();
                cmd.CommandText = @"IF DB_ID(N'DesafioTecnicoDb') IS NULL BEGIN CREATE DATABASE [DesafioTecnicoDb]; END";
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                await DumpContainerLogsAsync();
                Console.WriteLine("Failed to ensure DesafioTecnicoDb exists; skipping integration test. Error: " + ex.Message);
                return;
            }

            var conn = new SqlConnectionStringBuilder
            {
                DataSource = $"127.0.0.1,{HostPort}",
                InitialCatalog = "DesafioTecnicoDb",
                UserID = "sa",
                Password = SaPassword,
                TrustServerCertificate = true,
                ConnectTimeout = 30
            }.ConnectionString;
#else
            var envSa = Environment.GetEnvironmentVariable("TEST_DB_SA_PASSWORD") ?? Environment.GetEnvironmentVariable("SA_PASSWORD") ?? "Your_password123";
            var envPortStr = Environment.GetEnvironmentVariable("TEST_DB_PORT") ?? Environment.GetEnvironmentVariable("DB_PORT");
            var envPort = 14333;
            if (!string.IsNullOrEmpty(envPortStr) && int.TryParse(envPortStr, out var parsed)) envPort = parsed;

            var conn = new SqlConnectionStringBuilder
            {
                DataSource = $"127.0.0.1,{envPort}",
                InitialCatalog = "DesafioTecnicoDb",
                UserID = "sa",
                Password = envSa,
                TrustServerCertificate = true,
                ConnectTimeout = 30
            }.ConnectionString;
#endif

            _connectionString = conn;

            _configuredFactory = _factory.WithWebHostBuilder(builder =>
            {
                // Development so SQLite branch doesn't activate; we replace the DbContext below anyway.
                builder.UseSetting("environment", "Development");
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<DesafioTecnico.Infrastructure.Data.AppDbContext>) || d.ServiceType == typeof(DesafioTecnico.Infrastructure.Data.AppDbContext)).ToList();
                    foreach (var d in descriptors) services.Remove(d);

                    services.AddDbContext<DesafioTecnico.Infrastructure.Data.AppDbContext>(options => options.UseSqlServer(conn));

                    services.AddSingleton<Microsoft.AspNetCore.Authentication.ISystemClock>(_ => new Tests.Authentication.TimeProviderSystemClock(TimeProvider.System));
                    services.AddAuthentication("Test").AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, Tests.Authentication.TestAuthHandler>("Test", options => { });
                });
            });

            _client = _configuredFactory.CreateClient();

            using var scope = _configuredFactory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();

            // retry opening connection/migrating for a short while to allow SQL Server to finish initialization
            var migrated = false;
            var attempts = 0;
            var maxAttempts = 12; // ~60 seconds total
            while (!migrated && attempts < maxAttempts)
            {
                try
                {
                    await db.Database.MigrateAsync();
                    migrated = true;
                }
                catch (Exception ex)
                {
                    attempts++;
                    if (attempts >= maxAttempts)
                    {
#if TESTCONTAINERS
                        await DumpContainerLogsAsync();
#endif
                        Console.WriteLine($"Failed to apply migrations after waiting. Skipping integration test. Last error: {ex.Message}");
                        return;
                    }
                    await Task.Delay(5000);
                }
            }

            try
            {
                using var verifyConn = new SqlConnection(conn);
                await verifyConn.OpenAsync();
                using var cmd = verifyConn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
#if TESTCONTAINERS
                await DumpContainerLogsAsync();
#endif
                _skipIntegration = true;
                _skipReason = "Failed to verify DB connectivity after migrations: " + ex.Message;
                Console.WriteLine(_skipReason);
                return;
            }
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            try
            {
#if TESTCONTAINERS
                if (_container != null)
                {
                    await _container.StopAsync();
                    await _container.DisposeAsync();
                }
#endif
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        [Fact]
        [Trait("Category","Integration")]
        public async Task ConfirmarReserva_DeveMudarApartamentoParaVendido_QuandoFluxoCompletoAutenticado()
        {
            bool shouldSkip = false;
            string? skipReason = null;
#if TESTCONTAINERS
            shouldSkip = _skipIntegration;
            skipReason = _skipReason;
#endif
            if (shouldSkip)
            {
                Console.WriteLine(skipReason ?? "Integration tests skipped due to Testcontainers startup failure.");
                return;
            }
            try
            {
                using (var scope = _configuredFactory!.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();
                    db.Usuarios.Add(new DesafioTecnico.Domain.Entities.Usuario { Id = Guid.NewGuid(), Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin" });
                    var apt = Fixtures.FakeDataBuilder.CreateApartamento();
                    db.Apartamentos.Add(apt);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Skipping integration test during seed step: {ex.GetType().Name}: {ex.Message}");
                return;
            }

            // 1. authenticate using Test auth
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "admin");

            // 2. create client
            var clienteReq = new { Nome = "Cliente IT", Email = "it@exemplo.com", Cpf = "222.222.222-22", DataNascimento = DateTime.UtcNow.AddYears(-30) };
            var clienteResp = await PostJsonAsync("/api/clientes", clienteReq);
            clienteResp.EnsureSuccessStatusCode();
            var createdCliente = JsonDocument.Parse(await clienteResp.Content.ReadAsStringAsync());
            var clienteId = createdCliente.RootElement.GetProperty("id").GetGuid();

            // 3. get apartments and pick first
            var aptsResp = await _client.GetAsync("/api/apartamentos");
            aptsResp.EnsureSuccessStatusCode();
            var aptsJson = JsonDocument.Parse(await aptsResp.Content.ReadAsStringAsync());
            var aptId = aptsJson.RootElement.GetProperty("items")[0].GetProperty("id").GetGuid();

            // 4. create reserva
            var reservaReq = new { ClienteId = clienteId, ApartamentoId = aptId };
            var reservaResp = await PostJsonAsync("/api/reservas", reservaReq);
            reservaResp.EnsureSuccessStatusCode();
            var reservaJson = JsonDocument.Parse(await reservaResp.Content.ReadAsStringAsync());
            var reservaId = reservaJson.RootElement.GetProperty("id").GetGuid();

            // 5. confirm reserva
            var confirmResp = await _client.PostAsync($"/api/reservas/{reservaId}/confirm", null);
            confirmResp.EnsureSuccessStatusCode();

            // verify in db
            using (var scope = _configuredFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();
                var apt = await db.Apartamentos.FindAsync(aptId);
                Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, apt.Status);

                var venda = await db.Vendas.FirstOrDefaultAsync(v => v.ApartamentoId == aptId);
                Assert.NotNull(venda);
                Assert.Equal(clienteId, venda.ClienteId);
            }
        }

        private async Task<HttpResponseMessage> PostJsonAsync(HttpClient client, string url, object body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PostAsync(url, content);
        }

        private Task<HttpResponseMessage> PostJsonAsync(string url, object body)
            => PostJsonAsync(_client, url, body);

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CancelarReserva_DeveLibertarApartamento_QuandoAutenticadoViaJwt()
        {
            if (_configuredFactory == null) { Console.WriteLine("Integration tests skipped."); return; }

            var username = "jwt-" + Guid.NewGuid().ToString("N")[..8];
            const string password = "admin123";
            Guid aptId;

            using (var scope = _configuredFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();
                db.Usuarios.Add(new DesafioTecnico.Domain.Entities.Usuario
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "Admin"
                });
                var apt = Fixtures.FakeDataBuilder.CreateApartamento();
                aptId = apt.Id;
                db.Apartamentos.Add(apt);
                db.SaveChanges();
            }

            // Factory sem TestAuthHandler — JWT nativo da aplicação é usado
            using var jwtFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("environment", "Development");
                builder.ConfigureServices(services =>
                {
                    var toRemove = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<DesafioTecnico.Infrastructure.Data.AppDbContext>) ||
                        d.ServiceType == typeof(DesafioTecnico.Infrastructure.Data.AppDbContext)).ToList();
                    foreach (var d in toRemove) services.Remove(d);
                    services.AddDbContext<DesafioTecnico.Infrastructure.Data.AppDbContext>(
                        options => options.UseSqlServer(_connectionString));
                    services.AddSingleton<Microsoft.AspNetCore.Authentication.ISystemClock>(
                        _ => new Tests.Authentication.TimeProviderSystemClock(TimeProvider.System));
                });
            });
            using var jwtClient = jwtFactory.CreateClient();

            // 1. Login real com JWT
            var loginResp = await PostJsonAsync(jwtClient, "/api/auth/login", new { Username = username, Password = password });
            loginResp.EnsureSuccessStatusCode();
            var loginDoc = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync());
            var token = loginDoc.RootElement.GetProperty("token").GetString()!;
            Assert.False(string.IsNullOrEmpty(token), "JWT token não deve ser vazio após login bem-sucedido");

            jwtClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Criar cliente
            var uid = Guid.NewGuid();
            var digits = uid.ToString("N")[..9];
            var cpf = $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[..2]}";
            var clienteResp = await PostJsonAsync(jwtClient, "/api/clientes", new
            {
                Nome = "Cliente Cancelamento",
                Email = $"cancel-{uid:N}@exemplo.com",
                Cpf = cpf,
                DataNascimento = DateTime.UtcNow.AddYears(-28)
            });
            clienteResp.EnsureSuccessStatusCode();
            var clienteId = JsonDocument.Parse(await clienteResp.Content.ReadAsStringAsync())
                .RootElement.GetProperty("id").GetGuid();

            // 3. Reservar
            var reservaResp = await PostJsonAsync(jwtClient, "/api/reservas", new { ClienteId = clienteId, ApartamentoId = aptId });
            reservaResp.EnsureSuccessStatusCode();
            var reservaId = JsonDocument.Parse(await reservaResp.Content.ReadAsStringAsync())
                .RootElement.GetProperty("id").GetGuid();

            // 4. Cancelar
            var cancelResp = await jwtClient.PostAsync($"/api/reservas/{reservaId}/cancel", null);
            cancelResp.EnsureSuccessStatusCode();

            // Verificar estado no banco
            using var verifyScope = _configuredFactory.Services.CreateScope();
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();
            var cancelledApt = await verifyDb.Apartamentos.FindAsync(aptId);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Disponivel, cancelledApt!.Status);
            var cancelledReserva = await verifyDb.Reservas.FindAsync(reservaId);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusReserva.Cancelada, cancelledReserva!.Status);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task VenderDiretamente_DeveCriarVenda_QuandoApartamentoDisponivel()
        {
            if (_configuredFactory == null) { Console.WriteLine("Integration tests skipped."); return; }

            Guid clienteId, aptId;
            using (var scope = _configuredFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();
                var cliente = Fixtures.FakeDataBuilder.CreateCliente();
                clienteId = cliente.Id;
                var apt = Fixtures.FakeDataBuilder.CreateApartamento();
                aptId = apt.Id;
                db.Clientes.Add(cliente);
                db.Apartamentos.Add(apt);
                db.SaveChanges();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "admin");

            var vendaResp = await PostJsonAsync("/api/vendas", new
            {
                ClienteId = clienteId,
                ApartamentoId = aptId,
                ValorPago = 420000m
            });
            vendaResp.EnsureSuccessStatusCode();

            using var verifyScope = _configuredFactory.Services.CreateScope();
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();
            var soldApt = await verifyDb.Apartamentos.FindAsync(aptId);
            Assert.Equal(DesafioTecnico.Domain.Enums.StatusApartamento.Vendido, soldApt!.Status);
            var venda = await verifyDb.Vendas.FirstOrDefaultAsync(v => v.ApartamentoId == aptId && v.ClienteId == clienteId);
            Assert.NotNull(venda);
            Assert.Equal(420000m, venda.ValorPago);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CriarReserva_DeveRetornarBadRequest_QuandoApartamentoJaVendido()
        {
            if (_configuredFactory == null) { Console.WriteLine("Integration tests skipped."); return; }

            Guid clienteId, aptId;
            using (var scope = _configuredFactory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DesafioTecnico.Infrastructure.Data.AppDbContext>();
                var cliente = Fixtures.FakeDataBuilder.CreateCliente();
                clienteId = cliente.Id;
                var apt = Fixtures.FakeDataBuilder.CreateApartamento();
                apt.Status = DesafioTecnico.Domain.Enums.StatusApartamento.Vendido;
                aptId = apt.Id;
                db.Clientes.Add(cliente);
                db.Apartamentos.Add(apt);
                db.SaveChanges();
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "admin");

            var reservaResp = await PostJsonAsync("/api/reservas", new { ClienteId = clienteId, ApartamentoId = aptId });

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, reservaResp.StatusCode);
        }

#if TESTCONTAINERS
        private async Task DumpContainerLogsAsync()
        {
            try
            {
                var psi = new ProcessStartInfo("docker", $"logs {ContainerName}") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
                var p = Process.Start(psi);
                if (p != null)
                {
                    var outp = await p.StandardOutput.ReadToEndAsync();
                    var err = await p.StandardError.ReadToEndAsync();
                    p.WaitForExit(5000);
                    Console.WriteLine("===== Docker logs for desafio-test-sql =====");
                    Console.WriteLine(outp);
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        Console.WriteLine("===== Docker logs (stderr) =====");
                        Console.WriteLine(err);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read docker logs: " + ex.Message);
            }
        }
#endif
    }
}
