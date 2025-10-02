using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;

namespace Test.Mocks
{
    public class VeiculoServicoMock : IVeiculoServico
    {
        private static List<Veiculo> veiculos = new List<Veiculo>();

        public void CadastrarVeiculo(Veiculo veiculo)
        {
            veiculo.ID = veiculos.Count > 0 ? veiculos.Max(v => v.ID) + 1 : 1;
            veiculos.Add(veiculo);
        }

        public void AtualizarVeiculo(Veiculo veiculo)
        {
            var existente = veiculos.FirstOrDefault(v => v.ID == veiculo.ID);
            if (existente != null)
            {
                existente.Nome = veiculo.Nome;
                existente.Marca = veiculo.Marca;
                existente.Ano = veiculo.Ano;
            }
        }

        public void ApagarVeiculo(Veiculo veiculo)
        {
            veiculos.RemoveAll(v => v.ID == veiculo.ID);
        }

        public Veiculo? BuscaPorId(int id) => veiculos.FirstOrDefault(v => v.ID == id);

        public List<Veiculo>? Todos(int? page = 1, string? nome = null, string? marca = null) => veiculos.ToList();

        public static void LimparDados() => veiculos.Clear();
    }
}