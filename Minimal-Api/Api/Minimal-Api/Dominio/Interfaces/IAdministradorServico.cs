using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.DTOs;

namespace MinimalAPI.Dominio.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);

        List<Administrador>? Todos(int? page = 1, string? email = null, string? senha = null, string? perfil = null);

        Administrador? BuscaPorId(int id);

        void CadastrarAdministrador(Administrador administrador);

        void AtualizarAdministrador(Administrador administrador);

        void ApagarAdministrador(Administrador administrador);
    }
}