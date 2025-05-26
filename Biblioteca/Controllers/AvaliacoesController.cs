using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Biblioteca.Controllers
{
    public class AvaliacoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AvaliacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        // GET: Avaliacoes
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.AppUserId.ToString() == userId);
            if (usuario == null)
                return RedirectToAction("Index", "Home");

            var avaliacoes = await _context.Avaliacoes
                .Include(a => a.Livro)
                .Where(a => a.UsuarioId == usuario.UsuarioId)
                .OrderByDescending(a => a.DataAvaliacao)
                .ToListAsync();

            return View(avaliacoes);
        }

        // GET: Avaliacoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Livro)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.AvaliacaoId == id);
            if (avaliacao == null)
            {
                return NotFound();
            }

            return View(avaliacao);
        }

        // GET: Avaliacoes/Create
        public IActionResult Create()
        {
            ViewData["LivroId"] = new SelectList(_context.Livros, "LivroId", "LivroId");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId");
            return View();
        }

        // POST: Avaliacoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int LivroId, int Nota, string? Comentario)
        {
            var userName = User.Identity?.Name;
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUser != null && u.IdentityUser.UserName == userName);

            if (usuario == null || Nota < 1 || Nota > 5)
                return Unauthorized();

            var avaliacao = new Avaliacao
            {
                LivroId = LivroId,
                UsuarioId = usuario.UsuarioId,
                Nota = Nota,
                Comentario = Comentario,
                DataAvaliacao = DateTime.Now
            };

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Avaliação registrada com sucesso!";
            return RedirectToAction("Index", "Reservas");
        }


        // GET: Avaliacoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao == null)
            {
                return NotFound();
            }
            ViewData["LivroId"] = new SelectList(_context.Livros, "LivroId", "LivroId", avaliacao.LivroId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "UsuarioId", "UsuarioId", avaliacao.UsuarioId);
            return View(avaliacao);
        }

        // POST: Avaliacoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int AvaliacaoId, int LivroId, int Nota, string? Comentario)
        {
            var userName = User.Identity?.Name;
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdentityUser != null && u.IdentityUser.UserName == userName);

            if (usuario == null || Nota < 1 || Nota > 5)
                return Unauthorized();

            var avaliacao = await _context.Avaliacoes
                .FirstOrDefaultAsync(a => a.AvaliacaoId == AvaliacaoId && a.UsuarioId == usuario.UsuarioId);

            if (avaliacao == null)
                return NotFound();

            avaliacao.Nota = Nota;
            avaliacao.Comentario = Comentario;
            avaliacao.DataAvaliacao = DateTime.Now;

            _context.Avaliacoes.Update(avaliacao);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Avaliação atualizada com sucesso!";
            return RedirectToAction("Index", "Reservas");
        }

        // GET: Avaliacoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Livro)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.AvaliacaoId == id);
            if (avaliacao == null)
            {
                return NotFound();
            }

            return View(avaliacao);
        }

        // POST: Avaliacoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);
            if (avaliacao != null)
            {
                _context.Avaliacoes.Remove(avaliacao);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AvaliacaoExists(int id)
        {
            return _context.Avaliacoes.Any(e => e.AvaliacaoId == id);
        }
    }
}
