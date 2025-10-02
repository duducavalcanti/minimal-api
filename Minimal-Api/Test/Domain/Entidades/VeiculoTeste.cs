using MinimalAPI.Dominio.Entidades;

namespace Teste.Domain.Entidades
{
    [TestClass]
    public class VeiculoTeste
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            //Arrange
            var vec = new Veiculo();

            //Act
            vec.ID = 1;
            vec.Nome = "Duster";
            vec.Marca = "Renault";
            vec.Ano = 2025;

            //Assert
            Assert.AreEqual<int>(1, vec.ID);
            Assert.AreEqual<string>("Duster", vec.Nome);
            Assert.AreEqual<string>("Renault", vec.Marca);
            Assert.AreEqual<int>(2025, vec.Ano);
        }
    }
}