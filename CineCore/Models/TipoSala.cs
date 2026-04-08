namespace CineCore.Models
{
    public class TipoSala
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioExtra { get; set; }

        public ICollection<Sala> Salas { get; set; } = new List<Sala>();
    }
}
