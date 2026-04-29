using System.ComponentModel.DataAnnotations;

namespace CineCore.Models
{
    public class Pelicula
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, ErrorMessage = "El título no puede superar los {1} caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Display(Name = "Duración (minutos)")]
        [Range(1, 500, ErrorMessage = "La duración debe estar entre {1} y {2} minutos.")]
        public int Duracion { get; set; }

        [Required(ErrorMessage = "La clasificación es obligatoria.")]
        [StringLength(20, ErrorMessage = "La clasificación no puede superar los {1} caracteres.")]
        public string Clasificacion { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "La sinopsis no puede superar los {1} caracteres.")]
        public string? Sinopsis { get; set; }

        [Display(Name = "URL de imagen")]
        [Url(ErrorMessage = "Ingresá una URL válida (debe empezar con http:// o https://).")]
        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        public ICollection<Genero> Generos { get; set; } = new List<Genero>();
        public ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
    }
}