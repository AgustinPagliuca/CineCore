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
    public class ReservaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reserva
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reservas.Include(r => r.Butaca).Include(r => r.Cliente).Include(r => r.Funcion);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reserva/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Butaca)
                .Include(r => r.Cliente)
                .Include(r => r.Funcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reserva/Create
        public IActionResult Create()
        {
            ViewData["ButacaId"] = new SelectList(_context.Butacas, "Id", "Id");
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["FuncionId"] = new SelectList(_context.Funciones, "Id", "Id");
            return View();
        }

        // POST: Reserva/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FechaReserva,Estado,ClienteId,FuncionId,ButacaId")] Reserva reserva)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ButacaId"] = new SelectList(_context.Butacas, "Id", "Id", reserva.ButacaId);
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Id", reserva.ClienteId);
            ViewData["FuncionId"] = new SelectList(_context.Funciones, "Id", "Id", reserva.FuncionId);
            return View(reserva);
        }

        // GET: Reserva/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            ViewData["ButacaId"] = new SelectList(_context.Butacas, "Id", "Id", reserva.ButacaId);
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Id", reserva.ClienteId);
            ViewData["FuncionId"] = new SelectList(_context.Funciones, "Id", "Id", reserva.FuncionId);
            return View(reserva);
        }

        // POST: Reserva/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaReserva,Estado,ClienteId,FuncionId,ButacaId")] Reserva reserva)
        {
            if (id != reserva.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.Id))
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
            ViewData["ButacaId"] = new SelectList(_context.Butacas, "Id", "Id", reserva.ButacaId);
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Id", reserva.ClienteId);
            ViewData["FuncionId"] = new SelectList(_context.Funciones, "Id", "Id", reserva.FuncionId);
            return View(reserva);
        }

        // GET: Reserva/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Butaca)
                .Include(r => r.Cliente)
                .Include(r => r.Funcion)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reserva/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.Id == id);
        }
    }
}
