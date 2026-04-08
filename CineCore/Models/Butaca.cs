namespace CineCore.Models
{
    public class Butaca
    {
        public int Id { get; set; }
        public string Fila { get; set; } = string.Empty;
        public int Numero { get; set; }

        public int SalaId { get; set; }
        public Sala Sala { get; set; } = null!;

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
