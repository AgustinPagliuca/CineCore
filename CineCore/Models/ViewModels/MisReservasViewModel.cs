using CineCore.Models;

namespace CineCore.Models.ViewModels
{
    public class MisReservasViewModel
    {
        public IReadOnlyList<Reserva> Reservas { get; init; } = new List<Reserva>();
    }
}