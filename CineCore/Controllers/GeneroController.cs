using CineCore.Data;
using CineCore.Helpers;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineCore.Controllers
{
    [Authorize(Roles = Roles.Empleado)]
    public class GeneroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GeneroController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var generos = await _context.Generos
                .OrderBy(g => g.Nombre)
                .ToListAsync();
            return View(generos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var genero = await _context.Generos.FirstOrDefaultAsync(m => m.Id == id);
                result = genero == null ? NotFound() : View(genero);
            }

            return result;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] Genero genero)
        {
            IActionResult result;

            var nombreDuplicado = await _context.Generos.AnyAsync(g => g.Nombre == genero.Nombre);
            if (nombreDuplicado)
            {
                ModelState.AddModelError(nameof(Genero.Nombre), Mensajes.Genero.NombreDuplicado);
            }

            if (!ModelState.IsValid)
            {
                result = View(genero);
            }
            else
            {
                _context.Add(genero);
                await _context.SaveChangesAsync();
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        public async Task<IActionResult> Edit(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var genero = await _context.Generos.FindAsync(id);
                result = genero == null ? NotFound() : View(genero);
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Genero genero)
        {
            IActionResult result;

            if (id != genero.Id)
            {
                result = NotFound();
            }
            else
            {
                var nombreDuplicado = await _context.Generos.AnyAsync(g => g.Nombre == genero.Nombre && g.Id != id);
                if (nombreDuplicado)
                {
                    ModelState.AddModelError(nameof(Genero.Nombre), Mensajes.Genero.NombreDuplicado);
                }

                if (!ModelState.IsValid)
                {
                    result = View(genero);
                }
                else
                {
                    try
                    {
                        _context.Update(genero);
                        await _context.SaveChangesAsync();
                        result = RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (_context.Generos.Any(e => e.Id == genero.Id))
                        {
                            throw;
                        }
                        result = NotFound();
                    }
                }
            }

            return result;
        }

        public async Task<IActionResult> Delete(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var genero = await _context.Generos
                    .Include(g => g.Peliculas)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (genero == null)
                {
                    result = NotFound();
                }
                else
                {
                    ViewBag.PeliculasAsociadas = genero.Peliculas.Count;
                    result = View(genero);
                }
            }

            return result;
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            IActionResult result;

            var genero = await _context.Generos
                .Include(g => g.Peliculas)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (genero == null)
            {
                result = RedirectToAction(nameof(Index));
            }
            else if (genero.Peliculas.Any())
            {
                TempData[TempKeys.Error] = Mensajes.Genero.ConPeliculasNoEliminable(genero.Nombre, genero.Peliculas.Count);
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                _context.Generos.Remove(genero);
                await _context.SaveChangesAsync();
                TempData[TempKeys.Exito] = Mensajes.Genero.Eliminado(genero.Nombre);
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }
    }
}