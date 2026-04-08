namespace CineCore.Models
{
    public class Genero
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public ICollection<Pelicula> Peliculas { get; set; } = new List<Pelicula>();
    }
}
