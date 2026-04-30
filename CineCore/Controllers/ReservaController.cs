using CineCore.Data;
using CineCore.Helpers;
using CineCore.Models;
using CineCore.Models.ViewModels;
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
            var ahora = DateTime.Now;

            var reservas = await _context.Reservas
                .Include(r => r.Funcion)
                    .ThenInclude(f => f!.Pelicula)
                .Include(r => r.Funcion)
                    .ThenInclude(f => f!.Sala)
                        .ThenInclude(s => s!.TipoSala)
                .Include(r => r.Butaca)
                .Where(r => r.ClienteId == userId)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            var modelo = new MisReservasViewModel
            {
                Proximas = reservas
                    .Where(r => r.Funcion!.FechaHora >= ahora)
                    .ToList(),
                Anteriores = reservas
                    .Where(r => r.Funcion!.FechaHora < ahora)
                    .ToList()
            };

            return View(modelo);
        }

        public async Task<IActionResult> Crear(int funcionId)
        {
            IActionResult result;

            var funcion = await _context.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                    .ThenInclude(s => s!.TipoSala)
                .Include(f => f.Sala)
                    .ThenInclude(s => s!.Butacas)
                .Include(f => f.Reservas)
                .FirstOrDefaultAsync(f => f.Id == funcionId);

            if (funcion == null)
            {
                result = NotFound();
            }
            else if (funcion.YaPaso())
            {
                TempData[TempKeys.Error] = Mensajes.Reserva.FuncionYaPaso;
                result = RedirectToAction("Index", "Funcion");
            }
            else
            {
                var butacasOcupadas = funcion.Reservas
                    .Where(r => r.Estado != EstadoReserva.Cancelada)
                    .Select(r => r.ButacaId)
                    .ToHashSet();

                var butacasDisponibles = funcion.Sala!.Butacas
                    .Where(b => !butacasOcupadas.Contains(b.Id))
                    .ToList();

                ViewBag.Funcion = funcion;
                ViewBag.ButacasDisponibles = butacasDisponibles;
                ViewBag.MaximoButacasPorReserva = ReglasNegocio.MaximoButacasPorReserva;
                result = View();
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int funcionId, int[] butacaIds)
        {
            var userId = _userManager.GetUserId(User);
            IActionResult result;

            var cantidad = butacaIds?.Length ?? 0;
            var cantidadInvalida = cantidad < 1 || cantidad > ReglasNegocio.MaximoButacasPorReserva;

            var funcion = await _context.Funciones
                .Include(f => f.Sala)
                    .ThenInclude(s => s!.TipoSala)
                .FirstOrDefaultAsync(f => f.Id == funcionId);

            if (funcion == null)
            {
                result = NotFound();
            }
            else if (funcion.YaPaso())
            {
                TempData[TempKeys.Error] = Mensajes.Reserva.FuncionYaPaso;
                result = RedirectToAction("Index", "Funcion");
            }
            else if (cantidadInvalida)
            {
                TempData[TempKeys.Error] = Mensajes.Reserva.CantidadInvalida(ReglasNegocio.MaximoButacasPorReserva);
                result = RedirectToAction(nameof(Crear), new { funcionId });
            }
            else
            {
                result = await IntentarCrearReservas(funcion, butacaIds!, userId!);
            }

            return result;
        }

        private async Task<IActionResult> IntentarCrearReservas(Funcion funcion, int[] butacaIds, string userId)
        {
            IActionResult result;

            var butacasPertenecenALaSala = await _context.Butacas
                .Where(b => butacaIds.Contains(b.Id) && b.SalaId == funcion.SalaId)
                .CountAsync();

            var hayAlgunaOcupada = await _context.Reservas.AnyAsync(r =>
                r.FuncionId == funcion.Id
                && butacaIds.Contains(r.ButacaId)
                && r.Estado != EstadoReserva.Cancelada);

            if (butacasPertenecenALaSala != butacaIds.Length)
            {
                TempData[TempKeys.Error] = Mensajes.Reserva.ButacaNoPertenece;
                result = RedirectToAction(nameof(Crear), new { funcionId = funcion.Id });
            }
            else if (hayAlgunaOcupada)
            {
                TempData[TempKeys.Error] = Mensajes.Reserva.ButacaTomadaPorOtro;
                result = RedirectToAction(nameof(Crear), new { funcionId = funcion.Id });
            }
            else
            {
                var momento = DateTime.Now;
                var precioUnitario = funcion.PrecioFinal;

                using var transaccion = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var butacaId in butacaIds)
                    {
                        var reserva = new Reserva
                        {
                            ClienteId = userId,
                            FuncionId = funcion.Id,
                            ButacaId = butacaId,
                            FechaReserva = momento,
                            Estado = EstadoReserva.Confirmada,
                            PrecioPagado = precioUnitario
                        };
                        _context.Reservas.Add(reserva);
                    }

                    await _context.SaveChangesAsync();
                    await transaccion.CommitAsync();

                    TempData[TempKeys.Exito] = butacaIds.Length == 1
                        ? Mensajes.Reserva.Exito
                        : Mensajes.Reserva.ExitoMultiple(butacaIds.Length);
                    result = RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    await transaccion.RollbackAsync();
                    TempData[TempKeys.Error] = Mensajes.Reserva.ErrorAlCrear;
                    result = RedirectToAction(nameof(Crear), new { funcionId = funcion.Id });
                }
            }

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarGrupo(int funcionId, DateTime fecha)
        {
            var userId = _userManager.GetUserId(User);
            IActionResult result;

            var reservasDelGrupo = await _context.Reservas
                .Include(r => r.Funcion)
                .Where(r => r.ClienteId == userId
                         && r.FuncionId == funcionId
                         && r.FechaReserva == fecha
                         && r.Estado != EstadoReserva.Cancelada)
                .ToListAsync();

            var primera = reservasDelGrupo.FirstOrDefault();
            var funcionYaPaso = primera?.Funcion?.YaPaso() ?? false;

            if (!reservasDelGrupo.Any())
            {
                result = NotFound();
            }
            else if (funcionYaPaso)
            {
                TempData[TempKeys.Error] = Mensajes.Reserva.PasadaNoCancelable;
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var reserva in reservasDelGrupo)
                {
                    reserva.Estado = EstadoReserva.Cancelada;
                }
                await _context.SaveChangesAsync();

                TempData[TempKeys.Exito] = reservasDelGrupo.Count == 1
                    ? Mensajes.Reserva.Cancelada
                    : Mensajes.Reserva.CanceladasMultiples(reservasDelGrupo.Count);
                result = RedirectToAction(nameof(Index));
            }

            return result;
        }
    }
}