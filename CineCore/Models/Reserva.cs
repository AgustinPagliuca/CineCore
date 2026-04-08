namespace CineCore.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public DateTime FechaReserva { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";

        public string ClienteId { get; set; } = string.Empty;
        public ApplicationUser Cliente { get; set; } = null!;

        public int FuncionId { get; set; }
        public Funcion Funcion { get; set; } = null!;

        public int ButacaId { get; set; }
        public Butaca Butaca { get; set; } = null!;
    }
}
