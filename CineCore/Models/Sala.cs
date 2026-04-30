using System.ComponentModel.DataAnnotations;
using CineCore.Helpers;

namespace CineCore.Models
{
    public class Sala : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Número de sala")]
        [Range(1, 100, ErrorMessage = "El número de sala debe estar entre {1} y {2}.")]
        public int Numero { get; set; }

        [Range(6, 300, ErrorMessage = "La capacidad debe estar entre {1} y {2} butacas.")]
        public int Capacidad { get; set; }

        public int TipoSalaId { get; set; }
        public TipoSala? TipoSala { get; set; }

        public ICollection<Butaca> Butacas { get; set; } = new List<Butaca>();
        public ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errores = new List<ValidationResult>();

            if (Capacidad % ReglasNegocio.FilasPorSala != 0)
            {
                errores.Add(new ValidationResult(
                    $"La capacidad debe ser múltiplo de {ReglasNegocio.FilasPorSala} (la sala se distribuye en {ReglasNegocio.FilasPorSala} filas).",
                    new[] { nameof(Capacidad) }));
            }

            return errores;
        }
    }
}