using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace Biblioteca.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NomeCompleto { get; set; }
        public string CPF { get; set; }
        public string Celular { get; set; }
        public string DataNascimento { get; set; }
        public string? UrlFoto { get; set; }

        // Relacionamento com o IdentityUser
        public Guid? AppUserId { get; set; }
        public IdentityUser? IdentityUser { get; set; }
    }
}
