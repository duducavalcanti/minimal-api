using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Infraestrutura.DB;

namespace MinimalAPI.Dominio.Servicos
{
    public class VeiculoServico : IVeiculoServico
    {
        private readonly DBContexto _contexto;
        public VeiculoServico(DBContexto contexto)
        {
            _contexto = contexto;
        }

        public void ApagarVeiculo(Veiculo veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public void AtualizarVeiculo(Veiculo veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _contexto.Veiculos.Where(v => v.ID == id).FirstOrDefault();
        }

        public void CadastrarVeiculo(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public List<Veiculo>? Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _contexto.Veiculos.AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{nome}%"));
            }

            int itensPorPagina = 10;

            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
            }


            return query.ToList();
        }
    }
}