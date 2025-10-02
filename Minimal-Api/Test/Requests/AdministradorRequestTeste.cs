using System.Net.Http.Json;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.Enuns;
using MinimalAPI.Dominio.ModelViews;
using Test.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using Test.Mocks;
using MinimalAPI.Dominio.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Requests
{
    [TestClass]
    public class AdministradorRequestTeste
    {
        private const string BASE_URL = "/administradores";

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Setup.Inicializar();
        }

        [TestInitialize]
        public void LimparDados()
        {
            AdministradorServicoMock.LimparDados();
        }

        [TestMethod]
        public async Task TestarFluxoCompletoAdministrador()
        {
            // ====================================
            // Criar Administrador
            // ====================================
            var administradorDTO = new AdministradorDTO
            {
                Email = "teste_criado@teste.com",
                Senha = "123456",
                Perfil = "ADM"
            };

            var responsePost = await Setup.Client!.PostAsJsonAsync(BASE_URL, administradorDTO);
            responsePost.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var administradorCriado = await responsePost.Content.ReadFromJsonAsync<AdministradorModelView>(options);

            Assert.IsNotNull(administradorCriado);
            Assert.AreEqual("teste_criado@teste.com", administradorCriado.Email);
            Assert.AreEqual(PerfilAdministrador.Adm, administradorCriado.Perfil);

            // ====================================
            // Listar Administradores
            // ====================================
            var responseGet = await Setup.Client!.GetAsync(BASE_URL);
            responseGet.EnsureSuccessStatusCode();

            var administradores = await responseGet.Content.ReadFromJsonAsync<List<AdministradorModelView>>(options);
            Assert.IsNotNull(administradores);
            Assert.IsTrue(administradores!.Any(a => a.Email == "teste_criado@teste.com"));

            // ====================================
            // Buscar por ID
            // ====================================
            var responseGetById = await Setup.Client.GetAsync($"{BASE_URL}/{administradorCriado.ID}");
            responseGetById.EnsureSuccessStatusCode();

            var administradorPorId = await responseGetById.Content.ReadFromJsonAsync<AdministradorModelView>(options);
            Assert.IsNotNull(administradorPorId);
            Assert.AreEqual(administradorCriado.ID, administradorPorId.ID);

            // ====================================
            // Atualizar Administrador
            // ====================================
            var administradorUpdateDTO = new AdministradorDTO
            {
                Email = "teste_atualizado@teste.com",
                Senha = "654321",
                Perfil = "EDITOR"
            };

            var responsePut = await Setup.Client.PutAsJsonAsync($"{BASE_URL}/{administradorCriado.ID}", administradorUpdateDTO);
            responsePut.EnsureSuccessStatusCode();

            var administradorAtualizado = await responsePut.Content.ReadFromJsonAsync<AdministradorModelView>(options);
            Assert.IsNotNull(administradorAtualizado);
            Assert.AreEqual("teste_atualizado@teste.com", administradorAtualizado.Email);
            Assert.AreEqual(PerfilAdministrador.Editor, administradorAtualizado.Perfil);

            // ====================================
            // Apagar Administrador
            // ====================================
            var responseDelete = await Setup.Client.DeleteAsync($"{BASE_URL}/{administradorCriado.ID}");
            Assert.AreEqual(HttpStatusCode.NoContent, responseDelete.StatusCode);

            // ====================================
            // Confirma que foi deletado
            // ====================================
            var responseGetAfterDelete = await Setup.Client.GetAsync($"{BASE_URL}/{administradorCriado.ID}");
            Assert.AreEqual(HttpStatusCode.NotFound, responseGetAfterDelete.StatusCode);
        }
    }
}