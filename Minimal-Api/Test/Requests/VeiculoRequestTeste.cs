using System.Net.Http.Json;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.Entidades;
using Test.Helpers;
using Test.Mocks;

namespace Test.Requests
{
    [TestClass]
    public class VeiculoRequestTeste
    {
        private const string BASE_URL = "/veiculos";

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Setup.Inicializar();
        }

        [TestInitialize]
        public void LimparDados()
        {
            VeiculoServicoMock.LimparDados();
        }

        [TestMethod]
        public async Task TestarFluxoCompletoVeiculo()
        {
            var veiculoDTO = new VeiculoDTO
            {
                Nome = "Fusca",
                Marca = "Volkswagen",
                Ano = 1980
            };

            // ====================================
            // Criar Veículo
            // ====================================
            var responsePost = await Setup.Client.PostAsJsonAsync(BASE_URL, veiculoDTO);
            responsePost.EnsureSuccessStatusCode();
            var veiculoCriado = await responsePost.Content.ReadFromJsonAsync<Veiculo>();
            Assert.IsNotNull(veiculoCriado);
            Assert.AreEqual("Fusca", veiculoCriado.Nome);

            // ====================================
            // Listar Veículos
            // ====================================
            var responseGet = await Setup.Client.GetAsync(BASE_URL);
            responseGet.EnsureSuccessStatusCode();
            var veiculos = await responseGet.Content.ReadFromJsonAsync<List<Veiculo>>();
            Assert.IsTrue(veiculos!.Any(v => v.Nome == "Fusca"));

            // ====================================
            // Buscar por ID
            // ====================================
            var responseGetById = await Setup.Client.GetAsync($"{BASE_URL}/{veiculoCriado.ID}");
            responseGetById.EnsureSuccessStatusCode();
            var veiculoPorId = await responseGetById.Content.ReadFromJsonAsync<Veiculo>();
            Assert.AreEqual(veiculoCriado.ID, veiculoPorId!.ID);

            // ====================================
            // Atualizar Veículo
            // ====================================
            veiculoDTO.Nome = "Fusca Atualizado";
            var responsePut = await Setup.Client.PutAsJsonAsync($"{BASE_URL}/{veiculoCriado.ID}", veiculoDTO);
            responsePut.EnsureSuccessStatusCode();
            var veiculoAtualizado = await responsePut.Content.ReadFromJsonAsync<Veiculo>();
            Assert.AreEqual("Fusca Atualizado", veiculoAtualizado!.Nome);

            // ====================================
            // Apagar Administrador
            // ====================================
            var responseDelete = await Setup.Client.DeleteAsync($"{BASE_URL}/{veiculoCriado.ID}");
            Assert.AreEqual(System.Net.HttpStatusCode.NoContent, responseDelete.StatusCode);

            // ====================================
            // Confirma que foi deletado
            // ====================================
            var responseGetAfterDelete = await Setup.Client.GetAsync($"{BASE_URL}/{veiculoCriado.ID}");
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, responseGetAfterDelete.StatusCode);
        }
    }
}