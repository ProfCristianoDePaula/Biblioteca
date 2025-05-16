using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;


namespace Biblioteca.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reservas.Include(r => r.Livro).Include(r => r.Usuario);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Livro)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(m => m.ReservaId == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            ViewData["LivroId"] = new SelectList(_context.Livros, "LivroId", "LivroId");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View();
        }

        // POST: Reservas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     int LivroId,
     string? searchTerm,
     string? sortOrder,
     int page = 1)
        {
            // Obtém o usuário logado (Identity)
            var userName = User.Identity?.Name;

            // Busca o usuário no banco de dados
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUser != null && u.IdentityUser.UserName == userName);

            if (usuario == null)
            {
                return Unauthorized();
            }

            // Verifica se o livro tem quantidade disponivel para reserva
            var livro = await _context.Livros.FindAsync(LivroId);

            if (livro == null || livro.Quantidade <= 0)
            {
                // Livro não encontrado ou sem quantidade disponível
                var livrosQuery = _context.Livros.Include(l => l.Genero).AsQueryable();

                // Busca
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    livrosQuery = livrosQuery.Where(l =>
                        l.Titulo.Contains(searchTerm) ||
                        l.Autor.Contains(searchTerm)
                    );
                }

                // Ordenação
                switch (sortOrder)
                {
                    case "titulo_desc":
                        livrosQuery = livrosQuery.OrderByDescending(l => l.Titulo);
                        break;
                    case "titulo_asc":
                        livrosQuery = livrosQuery.OrderBy(l => l.Titulo);
                        break;
                    case "autor_desc":
                        livrosQuery = livrosQuery.OrderByDescending(l => l.Autor);
                        break;
                    case "autor_asc":
                        livrosQuery = livrosQuery.OrderBy(l => l.Autor);
                        break;
                    case "editora_desc":
                        livrosQuery = livrosQuery.OrderByDescending(l => l.Editora);
                        break;
                    case "editora_asc":
                        livrosQuery = livrosQuery.OrderBy(l => l.Editora);
                        break;
                    case "genero_desc":
                        livrosQuery = livrosQuery.OrderByDescending(l => l.Genero.Nome);
                        break;
                    case "genero_asc":
                        livrosQuery = livrosQuery.OrderBy(l => l.Genero.Nome);
                        break;
                    default:
                        livrosQuery = livrosQuery.OrderBy(l => l.Titulo);
                        break;
                }

                // Paginação
                int pageSize = 5;
                int totalCount = await livrosQuery.CountAsync();
                var livros = await livrosQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var viewModel = new LivroListViewModel
                {
                    Livros = livros,
                    PageIndex = page,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    SearchTerm = searchTerm,
                    SortOrder = sortOrder,
                    ErrorMessage = "Livro não disponível para reserva."
                };

                // Retorna a view de livros com paginação, ordenação e busca
                return View("~/Views/Livros/Index.cshtml", viewModel);
            }

            // Atualiza a quantidade do livro
            livro.Quantidade -= 1;
            _context.Update(livro);

            // Cria a reserva
            var reserva = new Reserva
            {
                LivroId = LivroId,
                UsuarioId = usuario.UsuarioId,
                DataReserva = DateTime.Now,
                LivroRetirado = false,
                Cancelada = false
            };

            _context.Add(reserva);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            ViewData["LivroId"] = new SelectList(_context.Livros, "LivroId", "LivroId", reserva.LivroId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservaId,DataReserva,UsuarioId,LivroId,LivroRetirado,Cancelada")] Reserva reserva)
        {
            if (id != reserva.ReservaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.ReservaId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LivroId"] = new SelectList(_context.Livros, "LivroId", "LivroId", reserva.LivroId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", reserva.UsuarioId);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Livro)
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(m => m.ReservaId == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.ReservaId == id);
        }
    }
}
