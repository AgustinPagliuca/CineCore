using CineCore.Data;
using CineCore.Helpers;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // GET: Pelicula
        // Visible para todos. Si es cliente, ve solo el listado. Si es empleado, ve botones de editar/eliminar.
        public async Task<IActionResult> Index()
        {
            var peliculas = await _context.Peliculas
                .Include(p => p.Generos)
                .OrderBy(p => p.Titulo)
                .ToListAsync();

            var ahora = DateTime.Now;
            var idsConFuncionesFuturas = await _context.Funciones
                .Where(f => f.FechaHora >= ahora)
                .Select(f => f.PeliculaId)
                .Distinct()
                .ToListAsync();

            ViewBag.IdsConFuncionesFuturas = idsConFuncionesFuturas.ToHashSet();

            return View(peliculas);
        }

        // GET: Pelicula/Details/5
        // Visible para todos.
        public async Task<IActionResult> Details(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var ahora = DateTime.Now;
                var pelicula = await _context.Peliculas
                    .Include(p => p.Generos)
                    .Include(p => p.Funciones.Where(f => f.FechaHora >= ahora))
                        .ThenInclude(f => f.Sala!)
                            .ThenInclude(s => s.TipoSala)
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

        // GET: Pelicula/Create
        [Authorize(Roles = Roles.Empleado)]
        public async Task<IActionResult> Create()
        {
            await CargarGenerosViewBagAsync(generosSeleccionados: null);
            return View();
        }

        // POST: Pelicula/Create
        [Authorize(Roles = Roles.Empleado)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Titulo,Duracion,Clasificacion,Sinopsis,ImagenUrl")] Pelicula pelicula,
            int[] generosSeleccionados)
        {
            IActionResult result;

            if (!ModelState.IsValid)
            {
                await CargarGenerosViewBagAsync(generosSeleccionados);
                result = View(pelicula);
            }
            else
            {
                if (generosSeleccionados != null && generosSeleccionados.Length > 0)
                {
                    var generos = await _context.Generos
                        .Where(g => generosSeleccionados.Contains(g.Id))
                        .ToListAsync();
                    pelicula.Generos = generos;
                }

                _context.Add(pelicula);
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = Mensajes.Pelicula.Creada(pelicula.Titulo);
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        // GET: Pelicula/Edit/5
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
                var pelicula = await _context.Peliculas
                    .Include(p => p.Generos)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pelicula == null)
                {
                    result = NotFound();
                }
                else
                {
                    var generosSeleccionados = pelicula.Generos.Select(g => g.Id).ToArray();
                    await CargarGenerosViewBagAsync(generosSeleccionados);
                    result = View(pelicula);
                }
            }

            return result;
        }

        // POST: Pelicula/Edit/5
        [Authorize(Roles = Roles.Empleado)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Titulo,Duracion,Clasificacion,Sinopsis,ImagenUrl")] Pelicula peliculaForm,
            int[] generosSeleccionados)
        {
            IActionResult result;

            if (id != peliculaForm.Id)
            {
                result = NotFound();
            }
            else if (!ModelState.IsValid)
            {
                await CargarGenerosViewBagAsync(generosSeleccionados);
                result = View(peliculaForm);
            }
            else
            {
                var peliculaDb = await _context.Peliculas
                    .Include(p => p.Generos)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (peliculaDb == null)
                {
                    result = NotFound();
                }
                else
                {
                    peliculaDb.Titulo = peliculaForm.Titulo;
                    peliculaDb.Duracion = peliculaForm.Duracion;
                    peliculaDb.Clasificacion = peliculaForm.Clasificacion;
                    peliculaDb.Sinopsis = peliculaForm.Sinopsis;
                    peliculaDb.ImagenUrl = peliculaForm.ImagenUrl;

                    peliculaDb.Generos.Clear();
                    if (generosSeleccionados != null && generosSeleccionados.Length > 0)
                    {
                        var generos = await _context.Generos
                            .Where(g => generosSeleccionados.Contains(g.Id))
                            .ToListAsync();
                        foreach (var genero in generos)
                        {
                            peliculaDb.Generos.Add(genero);
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        TempData[TempKeys.Exito] = Mensajes.Pelicula.Actualizada;
                        result = RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException) when (!PeliculaExists(peliculaForm.Id))
                    {
                        result = NotFound();
                    }
                }
            }

            return result;
        }

        // GET: Pelicula/Delete/5
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
                    result = View(pelicula);
                }
            }

            return result;
        }

        // POST: Pelicula/Delete/5
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
                result = NotFound();
            }
            else if (pelicula.Funciones.Any())
            {
                TempData[TempKeys.Error] = Mensajes.Pelicula.ConFuncionesNoEliminable(pelicula.Titulo, pelicula.Funciones.Count);
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                var titulo = pelicula.Titulo;
                _context.Peliculas.Remove(pelicula);
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = Mensajes.Pelicula.Eliminada(titulo);
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        // ------------------------------------------------------------
        // Helpers privados
        // ------------------------------------------------------------

        private async Task CargarGenerosViewBagAsync(int[]? generosSeleccionados)
        {
            var generos = await _context.Generos
                .OrderBy(g => g.Nombre)
                .ToListAsync();

            ViewBag.Generos = new MultiSelectList(
                generos,
                nameof(Genero.Id),
                nameof(Genero.Nombre),
                generosSeleccionados ?? Array.Empty<int>()
            );
        }

        private bool PeliculaExists(int id)
        {
            return _context.Peliculas.Any(e => e.Id == id);
        }
    }
}