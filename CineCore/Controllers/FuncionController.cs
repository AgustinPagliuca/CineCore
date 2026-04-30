using CineCore.Data;
using CineCore.Helpers;
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

                result = funcion == null ? NotFound() : View(funcion);
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
                TempData[TempKeys.Exito] = Mensajes.Funcion.Creada;
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
                else if (funcion.YaPaso())
                {
                    TempData[TempKeys.Error] = Mensajes.Funcion.PasadaNoEditable;
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
                else if (original.YaPaso())
                {
                    TempData[TempKeys.Error] = Mensajes.Funcion.PasadaNoEditable;
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
                            TempData[TempKeys.Exito] = Mensajes.Funcion.Actualizada;
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
                else if (funcion.YaPaso())
                {
                    TempData[TempKeys.Error] = Mensajes.Funcion.PasadaNoEliminable;
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
            else if (funcion.YaPaso())
            {
                TempData[TempKeys.Error] = Mensajes.Funcion.PasadaNoEliminable;
                result = RedirectToAction(nameof(Index));
            }
            else if (funcion.Reservas.Any(r => r.Estado != EstadoReserva.Cancelada))
            {
                TempData[TempKeys.Error] = Mensajes.Funcion.ConReservasNoEliminable;
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                _context.Funciones.Remove(funcion);
                await _context.SaveChangesAsync();
                TempData[TempKeys.Exito] = Mensajes.Funcion.Eliminada;
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
                var finNueva = inicioNueva
                    .AddMinutes(pelicula.Duracion)
                    .Add(ReglasNegocio.PausaEntreFunciones);

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
                        .Add(ReglasNegocio.PausaEntreFunciones);

                    var seSolapan = inicioNueva < finExistente && inicioExistente < finNueva;

                    if (seSolapan)
                    {
                        var titulo = existente.Pelicula?.Titulo ?? "(sin título)";
                        ModelState.AddModelError(
                            nameof(Funcion.FechaHora),
                            Mensajes.Funcion.Solapada(titulo, inicioExistente));
                    }
                }
            }
        }

        private void CargarSelectLists(int? peliculaId = null, int? salaId = null)
        {
            ViewData["PeliculaId"] = new SelectList(
                _context.Peliculas.OrderBy(p => p.Titulo), "Id", "Titulo", peliculaId);
            ViewData["SalaId"] = new SelectList(
                _context.Salas.OrderBy(s => s.Numero), "Id", "Numero", salaId);
        }
    }
}