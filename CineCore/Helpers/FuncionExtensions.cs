using CineCore.Models;

namespace CineCore.Helpers
{
    public static class FuncionExtensions
    {
        public static bool YaPaso(this Funcion funcion)
        {
            return funcion.FechaHora < DateTime.Now;
        }
        public static bool EstaAgotada(this Funcion funcion)
        {
            return funcion.LugaresDisponibles <= 0;
        }
    }
}