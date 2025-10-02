using MinimalAPI.Dominio.Entidades;

namespace MinimalAPI.Dominio.Interfaces
{
    public interface IVeiculoServico
    {
        List<Veiculo>? Todos(int? page = 1, string? nome = null, string? marca = null);
        Veiculo? BuscaPorId(int id);
        void CadastrarVeiculo(Veiculo veiculo);
        void AtualizarVeiculo(Veiculo veiculo);
        void ApagarVeiculo(Veiculo veiculo);
    }
}