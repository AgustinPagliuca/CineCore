using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CineCore.Data;
using CineCore.Models;

namespace CineCore.Controllers
{
    [Authorize]
    public class ReservaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var reservas = await _context.Reservas
                .Include(r => r.Funcion)
                    .ThenInclude(f => f.Pelicula)
                .Include(r => r.Funcion)
                    .ThenInclude(f => f.Sala)
                .Include(r => r.Butaca)
                .Where(r => r.ClienteId == userId)
                .ToListAsync();

            return View(reservas);
        }

        public async Task<IActionResult> Crear(int funcionId)
        {
            var funcion = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                    .ThenInclude(s => s.Butacas)
                .Include(f => f.Reservas)
                .FirstOrDefaultAsync(f => f.Id == funcionId);

            if (funcion == null)
                return NotFound();

            var butacasOcupadas = funcion.Reservas.Select(r => r.ButacaId).ToList();
            var butacasDisponibles = funcion.Sala.Butacas
                .Where(b => !butacasOcupadas.Contains(b.Id))
                .ToList();

            ViewBag.Funcion = funcion;
            ViewBag.ButacasDisponibles = butacasDisponibles;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int funcionId, int butacaId)
        {
            var userId = _userManager.GetUserId(User);

            var yaReservada = await _context.Reservas
                .AnyAsync(r => r.FuncionId == funcionId && r.ButacaId == butacaId);

            if (yaReservada)
            {
                TempData["Error"] = "Esa butaca ya fue reservada.";
                return RedirectToAction("Crear", new { funcionId });
            }

            var reserva = new Reserva
            {
                ClienteId = userId!,
                FuncionId = funcionId,
                ButacaId = butacaId,
                FechaReserva = DateTime.Now,
                Estado = "Confirmada"
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Reserva realizada con éxito.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = _userManager.GetUserId(User);
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id && r.ClienteId == userId);

            if (reserva == null)
                return NotFound();

            reserva.Estado = "Cancelada";
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Reserva cancelada.";
            return RedirectToAction("Index");
        }
    }
}