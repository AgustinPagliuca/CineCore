using CineCore.Data;
using CineCore.Helpers;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineCore.Controllers
{
    [Authorize(Roles = Roles.Empleado)]
    public class TipoSalaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoSalaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tipos = await _context.TiposSala
                .OrderBy(t => t.Nombre)
                .ToListAsync();
            return View(tipos);
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
                var tipo = await _context.TiposSala.FirstOrDefaultAsync(m => m.Id == id);
                result = tipo == null ? NotFound() : View(tipo);
            }

            return result;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,PrecioExtra")] TipoSala tipoSala)
        {
            IActionResult result;

            var nombreDuplicado = await _context.TiposSala.AnyAsync(t => t.Nombre == tipoSala.Nombre);
            if (nombreDuplicado)
            {
                ModelState.AddModelError(nameof(TipoSala.Nombre), Mensajes.TipoSala.NombreDuplicado);
            }

            if (!ModelState.IsValid)
            {
                result = View(tipoSala);
            }
            else
            {
                _context.Add(tipoSala);
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
                var tipo = await _context.TiposSala.FindAsync(id);
                result = tipo == null ? NotFound() : View(tipo);
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,PrecioExtra")] TipoSala tipoSala)
        {
            IActionResult result;

            if (id != tipoSala.Id)
            {
                result = NotFound();
            }
            else
            {
                var nombreDuplicado = await _context.TiposSala.AnyAsync(t => t.Nombre == tipoSala.Nombre && t.Id != id);
                if (nombreDuplicado)
                {
                    ModelState.AddModelError(nameof(TipoSala.Nombre), Mensajes.TipoSala.NombreDuplicado);
                }

                if (!ModelState.IsValid)
                {
                    result = View(tipoSala);
                }
                else
                {
                    try
                    {
                        _context.Update(tipoSala);
                        await _context.SaveChangesAsync();
                        result = RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (_context.TiposSala.Any(e => e.Id == tipoSala.Id))
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
                var tipo = await _context.TiposSala
                    .Include(t => t.Salas)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (tipo == null)
                {
                    result = NotFound();
                }
                else
                {
                    ViewBag.SalasAsociadas = tipo.Salas.Count;
                    result = View(tipo);
                }
            }

            return result;
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            IActionResult result;

            var tipo = await _context.TiposSala
                .Include(t => t.Salas)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipo == null)
            {
                result = RedirectToAction(nameof(Index));
            }
            else if (tipo.Salas.Any())
            {
                TempData[TempKeys.Error] = Mensajes.TipoSala.ConSalasNoEliminable(tipo.Nombre, tipo.Salas.Count);
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                _context.TiposSala.Remove(tipo);
                await _context.SaveChangesAsync();
                TempData[TempKeys.Exito] = Mensajes.TipoSala.Eliminado(tipo.Nombre);
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }
    }
}