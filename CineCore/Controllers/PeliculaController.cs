using CineCore.Data;
using CineCore.Helpers;
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

        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.Now;

            var peliculas = await _context.Peliculas
                .Include(p => p.Generos)
                .Include(p => p.Funciones)
                .OrderBy(p => p.Titulo)
                .ToListAsync();

            var idsConFuncionesFuturas = peliculas
                .Where(p => p.Funciones.Any(f => f.FechaHora >= ahora))
                .Select(p => p.Id)
                .ToHashSet();

            ViewBag.IdsConFuncionesFuturas = idsConFuncionesFuturas;

            return View(peliculas);
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
                var pelicula = await _context.Peliculas
                    .Include(p => p.Generos)
                    .Include(p => p.Funciones.Where(f => f.FechaHora >= DateTime.Now))
                        .ThenInclude(f => f.Sala!)
                            .ThenInclude(s => s.TipoSala)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (pelicula == null)
                {
                    result = NotFound();
                }
                else
                {
                    pelicula.Funciones = pelicula.Funciones
                        .OrderBy(f => f.FechaHora)
                        .ToList();
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
                TempData[TempKeys.Exito] = Mensajes.Pelicula.Creada(pelicula.Titulo);
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
                result = pelicula == null ? NotFound() : View(pelicula);
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
                    TempData[TempKeys.Exito] = Mensajes.Pelicula.Actualizada;
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
                    .Include(p => p.Funciones)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (pelicula == null)
                {
                    result = NotFound();
                }
                else
                {
                    ViewBag.FuncionesAsociadas = pelicula.Funciones.Count;
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
            IActionResult result;

            var pelicula = await _context.Peliculas
                .Include(p => p.Funciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pelicula == null)
            {
                result = RedirectToAction(nameof(Index));
            }
            else if (pelicula.Funciones.Any())
            {
                TempData[TempKeys.Error] = Mensajes.Pelicula.ConFuncionesNoEliminable(
                    pelicula.Titulo, pelicula.Funciones.Count);
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                _context.Peliculas.Remove(pelicula);
                await _context.SaveChangesAsync();
                TempData[TempKeys.Exito] = Mensajes.Pelicula.Eliminada(pelicula.Titulo);
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        private bool PeliculaExists(int id)
        {
            return _context.Peliculas.Any(e => e.Id == id);
        }
    }
}