using System.Globalization;

namespace Biblioteca.Models
{
    public class Livro
    {
        public int LivroId { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Descricao { get; set; }
        public string Editora { get; set; }
        public DateOnly DataPublicacao { get; set; }
        public int NumeroPaginas { get; set; }
        public int Quantidade { get; set; }
        public string? UrlCapa { get; set; }
        public string? ISBN10 { get; set; }
        public string? ISBN13 { get; set; }

        public int GeneroId { get; set; }
        public Genero? Genero { get; set; }
    }
}
