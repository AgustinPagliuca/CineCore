namespace CineCore.Models.ViewModels
{
    public class MisReservasViewModel
    {
        public IReadOnlyList<Reserva> Proximas { get; init; } = new List<Reserva>();
        public IReadOnlyList<Reserva> Anteriores { get; init; } = new List<Reserva>();
    }
}