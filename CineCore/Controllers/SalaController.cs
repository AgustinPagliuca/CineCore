using CineCore.Data;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CineCore.Controllers
{
    [Authorize(Roles = Roles.Empleado)]
    public class SalaController : Controller
    {
        private static readonly string[] EtiquetasFilas = { "A", "B", "C", "D", "E", "F" };

        private readonly ApplicationDbContext _context;

        public SalaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var salas = await _context.Salas
                .Include(s => s.TipoSala)
                .OrderBy(s => s.Numero)
                .ToListAsync();

            return View(salas);
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
                var sala = await _context.Salas
                    .Include(s => s.TipoSala)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (sala == null)
                {
                    result = NotFound();
                }
                else
                {
                    result = View(sala);
                }
            }

            return result;
        }

        public IActionResult Create()
        {
            CargarSelectListTipos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Numero,Capacidad,TipoSalaId")] Sala sala)
        {
            IActionResult result;

            var numeroDuplicado = await _context.Salas.AnyAsync(s => s.Numero == sala.Numero);
            if (numeroDuplicado)
            {
                ModelState.AddModelError(nameof(Sala.Numero), $"Ya existe una sala con el número {sala.Numero}.");
            }

            if (!ModelState.IsValid)
            {
                CargarSelectListTipos(sala.TipoSalaId);
                result = View(sala);
            }
            else
            {
                _context.Add(sala);
                await _context.SaveChangesAsync();

                GenerarButacas(sala);
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = $"Sala {sala.Numero} creada con {sala.Capacidad} butacas.";
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
                var sala = await _context.Salas.FindAsync(id);

                if (sala == null)
                {
                    result = NotFound();
                }
                else
                {
                    CargarSelectListTipos(sala.TipoSalaId);
                    result = View(sala);
                }
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numero,Capacidad,TipoSalaId")] Sala sala)
        {
            IActionResult result;

            if (id != sala.Id)
            {
                result = NotFound();
            }
            else
            {
                var original = await _context.Salas.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

                if (original == null)
                {
                    result = NotFound();
                }
                else
                {
                    if (sala.Capacidad != original.Capacidad)
                    {
                        ModelState.AddModelError(nameof(Sala.Capacidad),
                            "No se puede modificar la capacidad de una sala existente porque las butacas ya fueron generadas.");
                    }

                    var numeroDuplicado = await _context.Salas.AnyAsync(s => s.Numero == sala.Numero && s.Id != id);
                    if (numeroDuplicado)
                    {
                        ModelState.AddModelError(nameof(Sala.Numero), $"Ya existe una sala con el número {sala.Numero}.");
                    }

                    if (!ModelState.IsValid)
                    {
                        CargarSelectListTipos(sala.TipoSalaId);
                        result = View(sala);
                    }
                    else
                    {
                        try
                        {
                            _context.Update(sala);
                            await _context.SaveChangesAsync();
                            TempData[TempKeys.Exito] = "Sala actualizada.";
                            result = RedirectToAction(nameof(Index));
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (_context.Salas.Any(e => e.Id == sala.Id))
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

        public async Task<IActionResult> Delete(int? id)
        {
            IActionResult result;

            if (id == null)
            {
                result = NotFound();
            }
            else
            {
                var sala = await _context.Salas
                    .Include(s => s.TipoSala)
                    .Include(s => s.Funciones)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (sala == null)
                {
                    result = NotFound();
                }
                else
                {
                    ViewBag.FuncionesAsociadas = sala.Funciones.Count;
                    result = View(sala);
                }
            }

            return result;
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            IActionResult result;

            var sala = await _context.Salas
                .Include(s => s.Funciones)
                .Include(s => s.Butacas)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sala == null)
            {
                result = RedirectToAction(nameof(Index));
            }
            else if (sala.Funciones.Any())
            {
                TempData[TempKeys.Error] =
                    $"No se puede eliminar la sala {sala.Numero} porque tiene {sala.Funciones.Count} función/es asociada/s.";
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                _context.Butacas.RemoveRange(sala.Butacas);
                _context.Salas.Remove(sala);
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = $"Sala {sala.Numero} eliminada.";
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        private void GenerarButacas(Sala sala)
        {
            var butacasPorFila = sala.Capacidad / EtiquetasFilas.Length;

            foreach (var fila in EtiquetasFilas)
            {
                for (var numero = 1; numero <= butacasPorFila; numero++)
                {
                    _context.Butacas.Add(new Butaca
                    {
                        Fila = fila,
                        Numero = numero,
                        SalaId = sala.Id
                    });
                }
            }
        }

        private void CargarSelectListTipos(int? tipoSalaId = null)
        {
            ViewData["TipoSalaId"] = new SelectList(_context.TiposSala, "Id", "Nombre", tipoSalaId);
        }

        private static class TempKeys
        {
            public const string Exito = "Exito";
            public const string Error = "Error";
        }
    }
}