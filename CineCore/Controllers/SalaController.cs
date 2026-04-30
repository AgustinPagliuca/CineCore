using CineCore.Data;
using CineCore.Helpers;
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
                result = sala == null ? NotFound() : View(sala);
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
                ModelState.AddModelError(nameof(Sala.Numero), Mensajes.Sala.NumeroDuplicado(sala.Numero));
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

                TempData[TempKeys.Exito] = Mensajes.Sala.Creada(sala.Numero, sala.Capacidad);
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
                        ModelState.AddModelError(nameof(Sala.Capacidad), Mensajes.Sala.CapacidadInmutable);
                    }

                    var numeroDuplicado = await _context.Salas.AnyAsync(s => s.Numero == sala.Numero && s.Id != id);
                    if (numeroDuplicado)
                    {
                        ModelState.AddModelError(nameof(Sala.Numero), Mensajes.Sala.NumeroDuplicado(sala.Numero));
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
                            TempData[TempKeys.Exito] = Mensajes.Sala.Actualizada;
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
                TempData[TempKeys.Error] = Mensajes.Sala.ConFuncionesNoEliminable(sala.Numero, sala.Funciones.Count);
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                _context.Butacas.RemoveRange(sala.Butacas);
                _context.Salas.Remove(sala);
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = Mensajes.Sala.Eliminada(sala.Numero);
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        private void GenerarButacas(Sala sala)
        {
            var butacasPorFila = sala.Capacidad / ReglasNegocio.EtiquetasFilas.Length;

            foreach (var fila in ReglasNegocio.EtiquetasFilas)
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
    }
}