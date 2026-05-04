using System.Diagnostics;
using CineCore.Data;
using CineCore.Models;
using CineCore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineCore.Controllers
{
    public class HomeController : Controller
    {
        private const int MaximoPeliculasCarrusel = 6;

        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.Now;

            var peliculasEnCartelera = await _context.Peliculas
                .Include(p => p.Generos)
                .Include(p => p.Funciones.Where(f => f.FechaHora >= ahora))
                .Where(p => p.Funciones.Any(f => f.FechaHora >= ahora))
                .ToListAsync();

            var ordenadas = peliculasEnCartelera
                .OrderBy(p => p.Funciones.Min(f => f.FechaHora))
                .ToList();

            var carrusel = ordenadas
                .Where(p => !string.IsNullOrWhiteSpace(p.ImagenUrl))
                .Take(MaximoPeliculasCarrusel)
                .ToList();

            var modelo = new HomeViewModel
            {
                PeliculasCarrusel = carrusel,
                PeliculasEnCartelera = ordenadas
            };

            return View(modelo);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}