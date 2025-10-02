using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;

namespace MinimalAPI.Infraestrutura.DB;

public class DBContexto : DbContext
{
    public DBContexto(DbContextOptions<DBContexto> options) : base(options)
    {
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;
    public DbSet<Veiculo> Veiculos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador
            {
                ID = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "adm"
            }
        );
    }
}