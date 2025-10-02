using MinimalAPI.Dominio.Entidades;

namespace Teste.Domain.Entidades
{
    [TestClass]
    public class AdministradorTeste
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            //Arrange
            var adm = new Administrador();

            //Act
            adm.ID = 1;
            adm.Email = "teste@teste.com";
            adm.Senha = "teste";
            adm.Perfil = "adm";

            //Assert
            Assert.AreEqual<int>(1, adm.ID);
            Assert.AreEqual<string>("teste@teste.com", adm.Email);
            Assert.AreEqual<string>("teste", adm.Senha);
            Assert.AreEqual<string>("adm", adm.Perfil);
        }
    }
}