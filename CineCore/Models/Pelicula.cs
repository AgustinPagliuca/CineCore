using System.ComponentModel.DataAnnotations;
using CineCore.Helpers;

namespace CineCore.Models
{
    public class Pelicula
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, ErrorMessage = "El título no puede superar los {1} caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La duración es obligatoria.")]
        [Range(ReglasNegocio.DuracionMinimaPelicula, ReglasNegocio.DuracionMaximaPelicula,
            ErrorMessage = "La duración debe estar entre {1} y {2} minutos.")]
        [Display(Name = "Duración (minutos)")]
        public int Duracion { get; set; }

        [Required(ErrorMessage = "La clasificación es obligatoria.")]
        [StringLength(20, ErrorMessage = "La clasificación no puede superar los {1} caracteres.")]
        [Display(Name = "Clasificación")]
        public string Clasificacion { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "La sinopsis no puede superar los {1} caracteres.")]
        [DataType(DataType.MultilineText)]
        public string? Sinopsis { get; set; }

        [Url(ErrorMessage = "Ingresá una URL válida (ej: https://...).")]
        [StringLength(500, ErrorMessage = "La URL no puede superar los {1} caracteres.")]
        [Display(Name = "URL del poster")]
        public string? ImagenUrl { get; set; }

        public ICollection<Genero> Generos { get; set; } = new List<Genero>();
        public ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
    }
}