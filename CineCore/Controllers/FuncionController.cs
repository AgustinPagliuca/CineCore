using CineCore.Data;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineCore.Controllers
{
    public class FuncionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FuncionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Público - clientes y empleados pueden ver funciones
        public async Task<IActionResult> Index()
        {
            var funciones = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                    .ThenInclude(s => s!.TipoSala)
                .OrderBy(f => f.FechaHora)
                .ToListAsync();

            return View(funciones);
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
                var funcion = await _context.Funciones
                    .Include(f => f.Pelicula)
                    .Include(f => f.Sala)
                        .ThenInclude(s => s!.TipoSala)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (funcion == null)
                {
                    result = NotFound();
                }
                else
                {
                    result = View(funcion);
                }
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        public IActionResult Create()
        {
            ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id", "Titulo");
            ViewData["SalaId"] = new SelectList(_context.Salas, "Id", "Numero");
            return View();
        }

        [Authorize(Roles = Roles.Empleado)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FechaHora,Precio,PeliculaId,SalaId")] Funcion funcion)
        {
            IActionResult result;

            if (ModelState.IsValid)
            {
                _context.Add(funcion);
                await _context.SaveChangesAsync();
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id", "Titulo", funcion.PeliculaId);
                ViewData["SalaId"] = new SelectList(_context.Salas, "Id", "Numero", funcion.SalaId);
                result = View(funcion);
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
                var funcion = await _context.Funciones.FindAsync(id);
                if (funcion == null)
                {
                    result = NotFound();
                }
                else
                {
                    ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id", "Titulo", funcion.PeliculaId);
                    ViewData["SalaId"] = new SelectList(_context.Salas, "Id", "Numero", funcion.SalaId);
                    result = View(funcion);
                }
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaHora,Precio,PeliculaId,SalaId")] Funcion funcion)
        {
            IActionResult result;

            if (id != funcion.Id)
            {
                result = NotFound();
            }
            else if (!ModelState.IsValid)
            {
                ViewData["PeliculaId"] = new SelectList(_context.Peliculas, "Id", "Titulo", funcion.PeliculaId);
                ViewData["SalaId"] = new SelectList(_context.Salas, "Id", "Numero", funcion.SalaId);
                result = View(funcion);
            }
            else
            {
                try
                {
                    _context.Update(funcion);
                    await _context.SaveChangesAsync();
                    result = RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (_context.Funciones.Any(e => e.Id == funcion.Id))
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
                var funcion = await _context.Funciones
                    .Include(f => f.Pelicula)
                    .Include(f => f.Sala)
                        .ThenInclude(s => s!.TipoSala)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (funcion == null)
                {
                    result = NotFound();
                }
                else
                {
                    result = View(funcion);
                }
            }

            return result;
        }

        [Authorize(Roles = Roles.Empleado)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var funcion = await _context.Funciones.FindAsync(id);

            if (funcion != null)
            {
                _context.Funciones.Remove(funcion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}