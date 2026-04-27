using CineCore.Models;

namespace CineCore.Models.ViewModels
{
    public class TablaReservasContext
    {
        public IReadOnlyList<Reserva> Reservas { get; init; } = new List<Reserva>();
        public string MensajeVacio { get; init; } = string.Empty;
        public bool PermiteCancelar { get; init; }
    }
}