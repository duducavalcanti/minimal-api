using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;

namespace Test.Mocks
{
    public class AdministradorServicoMock : IAdministradorServico
    {
        private static List<Administrador> administradores = new List<Administrador>();

        public List<Administrador> ObterTodos() => administradores;

        public Administrador? Login(LoginDTO loginDTO)
        {
            return administradores
                .FirstOrDefault(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
        }

        public void ApagarAdministrador(Administrador administrador)
        {
            var existente = administradores.FirstOrDefault(a => a.ID == administrador.ID);
            if (existente != null)
            {
                administradores.Remove(existente);
            }
        }

        public void AtualizarAdministrador(Administrador administrador)
        {
            var existente = administradores.FirstOrDefault(a => a.ID == administrador.ID);
            if (existente != null)
            {
                existente.Email = administrador.Email;
                existente.Senha = administrador.Senha;
                existente.Perfil = administrador.Perfil;
            }
        }

        public Administrador? BuscaPorId(int id)
        {
            return administradores.FirstOrDefault(a => a.ID == id);
        }

        public void CadastrarAdministrador(Administrador administrador)
        {
            administrador.ID = administradores.Count > 0
                ? administradores.Max(a => a.ID) + 1
                : 1;

            administradores.Add(administrador);
        }

        public List<Administrador>? Todos(int? pagina = 1, string? email = null, string? senha = null, string? perfil = null)
        {
            return administradores.ToList();
        }

        public static void LimparDados() => administradores.Clear();
    }
}