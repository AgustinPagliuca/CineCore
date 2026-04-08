using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CineCore.Data;
using CineCore.Models;

namespace CineCore.Controllers
{
    public class TipoSalaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoSalaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TipoSala
        public async Task<IActionResult> Index()
        {
            return View(await _context.TiposSala.ToListAsync());
        }

        // GET: TipoSala/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoSala = await _context.TiposSala
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoSala == null)
            {
                return NotFound();
            }

            return View(tipoSala);
        }

        // GET: TipoSala/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoSala/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,PrecioExtra")] TipoSala tipoSala)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoSala);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoSala);
        }

        // GET: TipoSala/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoSala = await _context.TiposSala.FindAsync(id);
            if (tipoSala == null)
            {
                return NotFound();
            }
            return View(tipoSala);
        }

        // POST: TipoSala/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,PrecioExtra")] TipoSala tipoSala)
        {
            if (id != tipoSala.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoSala);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoSalaExists(tipoSala.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tipoSala);
        }

        // GET: TipoSala/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoSala = await _context.TiposSala
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoSala == null)
            {
                return NotFound();
            }

            return View(tipoSala);
        }

        // POST: TipoSala/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoSala = await _context.TiposSala.FindAsync(id);
            if (tipoSala != null)
            {
                _context.TiposSala.Remove(tipoSala);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoSalaExists(int id)
        {
            return _context.TiposSala.Any(e => e.Id == id);
        }
    }
}
