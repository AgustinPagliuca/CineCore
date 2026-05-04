using System.ComponentModel.DataAnnotations;

namespace CineCore.Models
{
    public class TipoSala
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, 1_000_000, ErrorMessage = "El precio extra debe estar entre {1} y {2}.")]
        [Display(Name = "Precio extra")]
        public decimal PrecioExtra { get; set; }

        public ICollection<Sala> Salas { get; set; } = new List<Sala>();
    }
}