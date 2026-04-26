using System.ComponentModel.DataAnnotations;

namespace CineCore.Models
{
    public class TipoSala
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Precio extra")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Range(0, 1_000_000, ErrorMessage = "El precio extra no puede ser negativo.")]
        public decimal PrecioExtra { get; set; }

        public ICollection<Sala> Salas { get; set; } = new List<Sala>();
    }
}