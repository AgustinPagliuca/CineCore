using System.ComponentModel.DataAnnotations;

namespace CineCore.Models
{
    public class Funcion : IValidatableObject
    {
        public static readonly TimeSpan MargenMinimoCreacion = TimeSpan.FromMinutes(30);

        public int Id { get; set; }

        [Display(Name = "Fecha y hora")]
        [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
        public DateTime FechaHora { get; set; }

        [Display(Name = "Precio base")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Range(0.01, 1_000_000, ErrorMessage = "El precio debe estar entre {1} y {2}.")]
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errores = new List<ValidationResult>();

            var minimo = DateTime.Now.Add(MargenMinimoCreacion);
            if (FechaHora < minimo)
            {
                var minutos = (int)MargenMinimoCreacion.TotalMinutes;
                errores.Add(new ValidationResult(
                    $"La función debe estar al menos {minutos} minutos en el futuro.",
                    new[] { nameof(FechaHora) }));
            }

            return errores;
        }
    }
}