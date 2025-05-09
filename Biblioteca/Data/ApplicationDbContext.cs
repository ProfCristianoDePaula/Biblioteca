using Biblioteca.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Livro>().ToTable("Livros");
            builder.Entity<Genero>().ToTable("Generos");
            builder.Entity<Movimentacao>().ToTable("Movimentacoes");
            builder.Entity<Reserva>().ToTable("Reservas");
            builder.Entity<Avaliacao>().ToTable("Avaliacoes");
            builder.Entity<Usuario>().ToTable("Usuarios");
        }
    }
}
