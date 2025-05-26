using System.Diagnostics;
using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var ultimosLivros = await _context.Livros
                .Include(l => l.Genero)
                .OrderByDescending(l => l.LivroId)
                .Take(4)
                .ToListAsync();

            var top5Livros = await _context.Reservas
                .GroupBy(r => r.LivroId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            var livrosMaisReservados = await _context.Livros
                .Include(l => l.Genero)
                .Where(l => top5Livros.Contains(l.LivroId))
                .ToListAsync();

            // Ordena conforme o ranking
            livrosMaisReservados = top5Livros
                .Select(id => livrosMaisReservados.First(l => l.LivroId == id))
                .ToList();

            // Calcula médias das avaliações dos últimos livros
            var mediasAvaliacoes = await _context.Avaliacoes
                .Where(a => ultimosLivros.Select(l => l.LivroId).Contains(a.LivroId))
                .GroupBy(a => a.LivroId)
                .ToDictionaryAsync(g => g.Key, g => g.Any() ? g.Average(a => a.Nota) : 0);

            // Top 4 livros mais bem avaliados
            var top4Avaliacoes = await _context.Avaliacoes
                .GroupBy(a => a.LivroId)
                .Select(g => new { LivroId = g.Key, Media = g.Average(a => a.Nota), Count = g.Count() })
                .OrderByDescending(g => g.Media)
                .ThenByDescending(g => g.Count) // desempate por quantidade de avaliações
                .Take(4)
                .ToListAsync();

            var livrosTopAvaliados = await _context.Livros
                .Include(l => l.Genero)
                .Where(l => top4Avaliacoes.Select(t => t.LivroId).Contains(l.LivroId))
                .ToListAsync();

            // Ordena conforme o ranking das médias
            livrosTopAvaliados = top4Avaliacoes
                .Select(t => livrosTopAvaliados.First(l => l.LivroId == t.LivroId))
                .ToList();

            // Dicionário de médias para os top avaliados
            var mediasTopAvaliados = top4Avaliacoes.ToDictionary(t => t.LivroId, t => t.Media);

            ViewBag.LivrosMaisReservados = livrosMaisReservados;
            ViewBag.MediasAvaliacoes = mediasAvaliacoes;
            ViewBag.LivrosTopAvaliados = livrosTopAvaliados;
            ViewBag.MediasTopAvaliados = mediasTopAvaliados;

            return View(ultimosLivros);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
