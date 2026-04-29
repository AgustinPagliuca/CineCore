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
        public static readonly TimeSpan PausaEntreFunciones = TimeSpan.FromMinutes(15);

        private readonly ApplicationDbContext _context;

        public FuncionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.Now;
            var esEmpleado = User.IsInRole(Roles.Empleado);

            var query = _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                    .ThenInclude(s => s!.TipoSala)
                .AsQueryable();

            if (!esEmpleado)
            {
                query = query.Where(f => f.FechaHora >= ahora);
            }

            var funciones = await query
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
            CargarSelectLists();
            return View();
        }

        [Authorize(Roles = Roles.Empleado)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FechaHora,Precio,PeliculaId,SalaId")] Funcion funcion)
        {
            IActionResult result;

            await ValidarSolapamiento(funcion);

            if (!ModelState.IsValid)
            {
                CargarSelectLists(funcion.PeliculaId, funcion.SalaId);
                result = View(funcion);
            }
            else
            {
                _context.Add(funcion);
                await _context.SaveChangesAsync();
                TempData[TempKeys.Exito] = "Función creada con éxito.";
                result = RedirectToAction(nameof(Index));
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
                else if (funcion.FechaHora < DateTime.Now)
                {
                    TempData[TempKeys.Error] = "No se pueden editar funciones que ya pasaron.";
                    result = RedirectToAction(nameof(Index));
                }
                else
                {
                    CargarSelectLists(funcion.PeliculaId, funcion.SalaId);
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
            else
            {
                var original = await _context.Funciones.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

                if (original == null)
                {
                    result = NotFound();
                }
                else if (original.FechaHora < DateTime.Now)
                {
                    TempData[TempKeys.Error] = "No se pueden editar funciones que ya pasaron.";
                    result = RedirectToAction(nameof(Index));
                }
                else
                {
                    await ValidarSolapamiento(funcion);

                    if (!ModelState.IsValid)
                    {
                        CargarSelectLists(funcion.PeliculaId, funcion.SalaId);
                        result = View(funcion);
                    }
                    else
                    {
                        try
                        {
                            _context.Update(funcion);
                            await _context.SaveChangesAsync();
                            TempData[TempKeys.Exito] = "Función actualizada.";
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
                else if (funcion.FechaHora < DateTime.Now)
                {
                    TempData[TempKeys.Error] = "No se pueden eliminar funciones que ya pasaron.";
                    result = RedirectToAction(nameof(Index));
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
            IActionResult result;

            var funcion = await _context.Funciones
                .Include(f => f.Reservas)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (funcion == null)
            {
                result = RedirectToAction(nameof(Index));
            }
            else if (funcion.FechaHora < DateTime.Now)
            {
                TempData[TempKeys.Error] = "No se pueden eliminar funciones que ya pasaron.";
                result = RedirectToAction(nameof(Index));
            }
            else if (funcion.Reservas.Any(r => r.Estado != EstadoReserva.Cancelada))
            {
                TempData[TempKeys.Error] = "No se puede eliminar una función con reservas activas.";
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                _context.Funciones.Remove(funcion);
                await _context.SaveChangesAsync();
                TempData[TempKeys.Exito] = "Función eliminada.";
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        private async Task ValidarSolapamiento(Funcion funcion)
        {
            var pelicula = await _context.Peliculas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == funcion.PeliculaId);

            if (pelicula != null)
            {
                var inicioNueva = funcion.FechaHora;
                var finNueva = inicioNueva.AddMinutes(pelicula.Duracion).Add(PausaEntreFunciones);

                var funcionesEnLaSala = await _context.Funciones
                    .Include(f => f.Pelicula)
                    .AsNoTracking()
                    .Where(f => f.SalaId == funcion.SalaId && f.Id != funcion.Id)
                    .ToListAsync();

                foreach (var existente in funcionesEnLaSala)
                {
                    var inicioExistente = existente.FechaHora;
                    var finExistente = inicioExistente
                        .AddMinutes(existente.Pelicula?.Duracion ?? 0)
                        .Add(PausaEntreFunciones);

                    var seSolapan = inicioNueva < finExistente && inicioExistente < finNueva;

                    if (seSolapan)
                    {
                        ModelState.AddModelError(
                            nameof(Funcion.FechaHora),
                            $"La función se solapa con \"{existente.Pelicula?.Titulo}\" del " +
                            $"{inicioExistente:dd/MM/yyyy HH:mm} en la misma sala.");
                    }
                }
            }
        }

        private void CargarSelectLists(int? peliculaId = null, int? salaId = null)
        {
            ViewData["PeliculaId"] = new SelectList(_context.Peliculas.OrderBy(p => p.Titulo), "Id", "Titulo", peliculaId);
            ViewData["SalaId"] = new SelectList(_context.Salas.OrderBy(s => s.Numero), "Id", "Numero", salaId);
        }

        private static class TempKeys
        {
            public const string Exito = "Exito";
            public const string Error = "Error";
        }
    }
}