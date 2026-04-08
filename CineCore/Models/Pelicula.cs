using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CineCore.Models
{
    public class Pelicula
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int Duracion { get; set; }
        public string Clasificacion { get; set; } = string.Empty;
        public string? Sinopsis { get; set; }
        public string? ImagenUrl { get; set; }

        public ICollection<Genero> Generos { get; set; } = new List<Genero>();
        public ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
    }
}
