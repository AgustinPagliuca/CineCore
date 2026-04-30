namespace CineCore.Helpers
{
    public static class ReglasNegocio
    {
        public const int MaximoButacasPorReserva = 8;
        
        public static readonly TimeSpan MargenMinimoCreacionFuncion = TimeSpan.FromMinutes(30);
        
        public static readonly TimeSpan PausaEntreFunciones = TimeSpan.FromMinutes(15);

        public const int FilasPorSala = 6;

        public static readonly string[] EtiquetasFilas = { "A", "B", "C", "D", "E", "F" };
    }
}