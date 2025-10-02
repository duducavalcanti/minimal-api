using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Infraestrutura.DB;

namespace Teste.Servico
{
    [TestClass]
    public class AdministradorServicoTeste
    {
        private DBContexto CriarContextoDeTeste()
        {
            // Carrega configuração do projeto de teste
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // garante que vai pegar o appsettings.json do projeto de teste
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            // Pega a connection string do banco de teste
            var connectionString = configuration.GetConnectionString("TestDataBase");

            var options = new DbContextOptionsBuilder<DBContexto>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .Options;

            return new DBContexto(options);
        }

        [TestMethod]
        public void TestandoSalvarAdministrador()
        {
            //Arrange
            var context = CriarContextoDeTeste();
            context.Administradores.RemoveRange(context.Administradores);
            context.SaveChanges();

            var adm = new Administrador
            {
                Email = "teste@teste.com",
                Senha = "teste",
                Perfil = "adm",
            };

            var administradorServico = new AdministradorServico(context);

            //Act
            administradorServico.CadastrarAdministrador(adm);

            //Assert
            Assert.AreEqual(1, administradorServico.Todos(1)?.Count() ?? 0);
            Assert.AreEqual("teste@teste.com", adm.Email);
            Assert.AreEqual("teste", adm.Senha);
            Assert.AreEqual("adm", adm.Perfil);

            //Act
            var admPorId = administradorServico.BuscaPorId(adm.ID);

            // Assert
            Assert.IsNotNull(admPorId);
            Assert.AreEqual(adm.ID, admPorId.ID);
            Assert.AreEqual(adm.Email, admPorId.Email);
            Assert.AreEqual(adm.Senha, admPorId.Senha);
            Assert.AreEqual(adm.Perfil, admPorId.Perfil);

            // Act
            var admTodos = administradorServico.Todos(1)?.ToList() ?? new List<Administrador>();

            // Assert
            Assert.AreEqual(1, admTodos.Count);

            //Act
            var admExistente = administradorServico.BuscaPorId(adm.ID);
            admExistente!.Email = "teste_teste@teste.com";
            admExistente.Senha = "teste_teste";
            admExistente.Perfil = "editor";
            administradorServico.AtualizarAdministrador(admExistente);

            // Assert
            var admAtualizado = administradorServico.BuscaPorId(adm.ID);
            Assert.IsNotNull(admExistente, "Administrador não encontrado para atualização");
            Assert.IsNotNull(admAtualizado);
            Assert.AreEqual(adm.ID, admAtualizado!.ID);
            Assert.AreEqual("teste_teste@teste.com", admAtualizado.Email);
            Assert.AreEqual("teste_teste", admAtualizado.Senha);
            Assert.AreEqual("editor", admAtualizado.Perfil);

            //Act
            administradorServico.ApagarAdministrador(admAtualizado);

            // Assert
            var admApagado = administradorServico.BuscaPorId(admAtualizado.ID);
            Assert.IsNull(admApagado);
        }
    }
}

