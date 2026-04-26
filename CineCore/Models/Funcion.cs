using System.ComponentModel.DataAnnotations;

namespace CineCore.Models
{
    public class Funcion
    {
        public int Id { get; set; }

        [Display(Name = "Fecha y hora")]
        public DateTime FechaHora { get; set; }

        [Display(Name = "Precio base")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
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

        public decimal ExtraTipoSala =>
            Sala?.TipoSala?.PrecioExtra ?? 0m;

        public decimal PrecioFinal =>
            Precio + ExtraTipoSala;
    }
}