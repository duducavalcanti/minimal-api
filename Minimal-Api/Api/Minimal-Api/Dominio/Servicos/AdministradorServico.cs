using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Infraestrutura.DB;

namespace MinimalAPI.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DBContexto _contexto;
        public AdministradorServico(DBContexto contexto)
        {
            _contexto = contexto;
        }
        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }

        public void ApagarAdministrador(Administrador administrador)
        {
            _contexto.Administradores.Remove(administrador);
            _contexto.SaveChanges();
        }

        public void AtualizarAdministrador(Administrador administrador)
        {
            _contexto.Administradores.Update(administrador);
            _contexto.SaveChanges();
        }

        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Administradores.Where(a => a.ID == id).FirstOrDefault();
        }

        public void CadastrarAdministrador(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public List<Administrador>? Todos(int? pagina = 1, string? email = null, string? senha = null, string? perfil = null)
        {
            var query = _contexto.Administradores.AsQueryable();

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(a => EF.Functions.Like(a.Email.ToLower(), $"%{email}%"));
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