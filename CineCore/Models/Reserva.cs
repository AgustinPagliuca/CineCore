namespace CineCore.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public DateTime FechaReserva { get; set; } = DateTime.Now;
        public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

        public string ClienteId { get; set; } = string.Empty;
        public ApplicationUser? Cliente { get; set; }

        public int FuncionId { get; set; }
        public Funcion? Funcion { get; set; }

        public int ButacaId { get; set; }
        public Butaca? Butaca { get; set; }
    }
}
