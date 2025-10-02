namespace MinimalAPI.Dominio.DTOs
{
    public record AdministradorDTO
    {
        public string Email { get; set; } = default!;

        public string Senha { get; set; } = default!;

        public string Perfil { get; set; } = default!;

    }
}

