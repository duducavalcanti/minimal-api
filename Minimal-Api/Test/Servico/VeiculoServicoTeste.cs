using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Servicos;
using MinimalAPI.Infraestrutura.DB;

namespace Teste.Servico
{
    [TestClass]
    public class VeiculoServicoTeste
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
        public void TestandoSalvarVeiculo()
        {
            // Arrange
            var context = CriarContextoDeTeste();
            context.Veiculos.RemoveRange(context.Veiculos);
            context.SaveChanges();

            var veic = new Veiculo
            {
                Nome = "Duster",
                Marca = "Renault",
                Ano = 2025
            };

            var veiculoServico = new VeiculoServico(context);

            // Act
            veiculoServico.CadastrarVeiculo(veic);

            // Assert
            Assert.AreEqual(1, veiculoServico.Todos(1)?.Count() ?? 0);
            Assert.AreEqual("Duster", veic.Nome);
            Assert.AreEqual("Renault", veic.Marca);
            Assert.AreEqual(2025, veic.Ano);

            // Act
            var veicPorId = veiculoServico.BuscaPorId(veic.ID);

            // Assert
            Assert.IsNotNull(veicPorId);
            Assert.AreEqual(veic.ID, veicPorId.ID);
            Assert.AreEqual(veic.Nome, veicPorId.Nome);
            Assert.AreEqual(veic.Marca, veicPorId.Marca);
            Assert.AreEqual(veic.Ano, veicPorId.Ano);

            // Act
            var veicTodos = veiculoServico.Todos(1)?.ToList() ?? new List<Veiculo>();

            // Assert
            Assert.AreEqual(1, veicTodos.Count);

            // Act
            var veicExistente = veiculoServico.BuscaPorId(veic.ID);
            veicExistente!.Nome = "Fusca";
            veicExistente.Marca = "Volkswagen";
            veicExistente.Ano = 1980;
            veiculoServico.AtualizarVeiculo(veicExistente);

            // Assert
            var veicAtualizado = veiculoServico.BuscaPorId(veic.ID);
            Assert.IsNotNull(veicExistente, "Veículo não encontrado para atualização");
            Assert.IsNotNull(veicAtualizado);
            Assert.AreEqual(veic.ID, veicAtualizado!.ID);
            Assert.AreEqual("Fusca", veicAtualizado.Nome);
            Assert.AreEqual("Volkswagen", veicAtualizado.Marca);
            Assert.AreEqual(1980, veicAtualizado.Ano);

            // Act
            veiculoServico.ApagarVeiculo(veicAtualizado);

            // Assert
            var veicApagado = veiculoServico.BuscaPorId(veicAtualizado.ID);
            Assert.IsNull(veicApagado);
        }
    }
}