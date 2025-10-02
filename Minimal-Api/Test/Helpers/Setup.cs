using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MinimalAPI;
using MinimalAPI.Dominio.Interfaces;
using Test.Mocks;
using System.Text.Encodings.Web;
using System.Security.Claims;

namespace Test.Helpers
{
    public static class Setup
    {
        public static HttpClient Client { get; private set; } = default!;
        public static WebApplicationFactory<Startup> Factory { get; private set; } = default!;


        public static void Inicializar()
        {
            Factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    // Substitui os serviços reais pelos mocks
                    services.AddScoped<IAdministradorServico, AdministradorServicoMock>();
                    services.AddScoped<IVeiculoServico, VeiculoServicoMock>();

                    // Remove autenticação real e adiciona fake
                    services.AddAuthentication("Test")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                    // Define "Test" como esquema padrão
                    services.PostConfigure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    });
                });
            });

            Client = Factory.CreateClient();
        }

        // Fake auth handler
        public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public TestAuthHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder,
                ISystemClock clock)
                : base(options, logger, encoder, clock) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var claims = new[] {
                    new Claim(ClaimTypes.Name, "admin@teste.com"),
                    new Claim(ClaimTypes.Role, "ADM")
                };

                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Test");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}