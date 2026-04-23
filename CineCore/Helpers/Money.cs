using System.Globalization;

namespace CineCore.Helpers
{
    public static class Money
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        public static string Format(decimal monto)
        {
            return monto.ToString("C2", Cultura);
        }
    }
}