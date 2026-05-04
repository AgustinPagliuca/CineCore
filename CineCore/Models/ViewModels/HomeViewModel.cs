using CineCore.Models;

namespace CineCore.Models.ViewModels
{
    public class HomeViewModel
    {
        public IReadOnlyList<Pelicula> PeliculasCarrusel { get; init; } = new List<Pelicula>();
        public IReadOnlyList<Pelicula> PeliculasEnCartelera { get; init; } = new List<Pelicula>();
    }
}