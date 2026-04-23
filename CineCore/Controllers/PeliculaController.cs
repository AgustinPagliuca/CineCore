using CineCore.Data;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineCore.Controllers
{
    public class PeliculaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PeliculaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Público
        public async Task<IActionResult> Index()
        {
            var peliculas = await _context.Peliculas
                .Include(p => p.Generos)
                .ToListAsync();
            return View(peliculas);
        }

        // Público
        public async Task<IActionResult> Details(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var pelicula = await _context.Peliculas
                    .Include(p => p.Generos)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (pelicula == null)
                {
                    result = NotFound();
                }
                else
                {
                    result = View(pelicula);
                }
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = Roles.Empleado)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Duracion,Clasificacion,Sinopsis,ImagenUrl")] Pelicula pelicula)
        {
            IActionResult result;

            if (ModelState.IsValid)
            {
                _context.Add(pelicula);
                await _context.SaveChangesAsync();
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                result = View(pelicula);
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        public async Task<IActionResult> Edit(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var pelicula = await _context.Peliculas.FindAsync(id);
                if (pelicula == null)
                {
                    result = NotFound();
                }
                else
                {
                    result = View(pelicula);
                }
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Duracion,Clasificacion,Sinopsis,ImagenUrl")] Pelicula pelicula)
        {
            IActionResult result;

            if (id != pelicula.Id)
            {
                result = NotFound();
            }
            else if (!ModelState.IsValid)
            {
                result = View(pelicula);
            }
            else
            {
                try
                {
                    _context.Update(pelicula);
                    await _context.SaveChangesAsync();
                    result = RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (PeliculaExists(pelicula.Id))
                    {
                        throw;
                    }
                    result = NotFound();
                }
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        public async Task<IActionResult> Delete(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var pelicula = await _context.Peliculas
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (pelicula == null)
                {
                    result = NotFound();
                }
                else
                {
                    result = View(pelicula);
                }
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pelicula = await _context.Peliculas.FindAsync(id);

            if (pelicula != null)
            {
                _context.Peliculas.Remove(pelicula);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PeliculaExists(int id)
        {
            return _context.Peliculas.Any(e => e.Id == id);
        }
    }
}