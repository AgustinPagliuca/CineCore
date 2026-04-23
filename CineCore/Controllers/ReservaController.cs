using CineCore.Data;
using CineCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineCore.Controllers
{
    [Authorize(Roles = Roles.Cliente)]
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
                    .ThenInclude(f => f!.Pelicula)
                .Include(r => r.Funcion)
                    .ThenInclude(f => f!.Sala)
                .Include(r => r.Butaca)
                .Where(r => r.ClienteId == userId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            return View(reservas);
        }

        public async Task<IActionResult> Crear(int funcionId)
        {
            IActionResult result;

            var funcion = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                    .ThenInclude(s => s!.Butacas)
                .Include(f => f.Reservas)
                .FirstOrDefaultAsync(f => f.Id == funcionId);

            if (funcion == null)
            {
                result = NotFound();
            }
            else
            {
                var butacasOcupadas = funcion.Reservas
                    .Where(r => r.Estado != EstadoReserva.Cancelada)
                    .Select(r => r.ButacaId)
                    .ToHashSet();

                var butacasDisponibles = funcion.Sala!.Butacas
                    .Where(b => !butacasOcupadas.Contains(b.Id))
                    .OrderBy(b => b.Fila)
                    .ThenBy(b => b.Numero)
                    .ToList();

                ViewBag.Funcion = funcion;
                ViewBag.ButacasDisponibles = butacasDisponibles;
                result = View();
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int funcionId, int butacaId)
        {
            var userId = _userManager.GetUserId(User);
            IActionResult result;

            var butacaOcupada = await _context.Reservas.AnyAsync(r =>
                r.FuncionId == funcionId
                && r.ButacaId == butacaId
                && r.Estado != EstadoReserva.Cancelada);

            if (butacaOcupada)
            {
                TempData[TempKeys.Error] = "Esa butaca ya fue reservada.";
                result = RedirectToAction(nameof(Crear), new { funcionId });
            }
            else
            {
                var reserva = new Reserva
                {
                    ClienteId = userId!,
                    FuncionId = funcionId,
                    ButacaId = butacaId,
                    FechaReserva = DateTime.Now,
                    Estado = EstadoReserva.Confirmada
                };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = "Reserva realizada con éxito.";
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = _userManager.GetUserId(User);
            IActionResult result;

            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.Id == id && r.ClienteId == userId);

            if (reserva == null)
            {
                result = NotFound();
            }
            else
            {
                reserva.Estado = EstadoReserva.Cancelada;
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = "Reserva cancelada.";
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }

        private static class TempKeys
        {
            public const string Exito = "Exito";
            public const string Error = "Error";
        }
    }
}