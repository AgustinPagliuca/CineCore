using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CineCore.Models
{
    public class Sala
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Capacidad { get; set; }

        public int TipoSalaId { get; set; }
        public TipoSala? TipoSala { get; set; }

        public ICollection<Butaca> Butacas { get; set; } = new List<Butaca>();
        public ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
    }
}
