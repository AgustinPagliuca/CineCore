namespace CineCore.Models
{
    public class Funcion
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal Precio { get; set; }

        public int PeliculaId { get; set; }
        public Pelicula? Pelicula { get; set; }

        public int SalaId { get; set; }
        public Sala? Sala { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        public int ReservasActivas =>
            Reservas.Count(r => r.Estado != EstadoReserva.Cancelada);

        public int LugaresDisponibles =>
            (Sala?.Capacidad ?? 0) - ReservasActivas;
    }
}
