namespace CineCore.Helpers
{
    public static class Duracion
    {
        public static string Format(int minutos)
        {
            var horas = minutos / 60;
            var restantes = minutos % 60;

            string formato;

            if (horas == 0)
            {
                formato = $"{restantes} min";
            }
            else if (restantes == 0)
            {
                formato = $"{horas} h";
            }
            else
            {
                formato = $"{horas} h {restantes} min";
            }

            return formato;
        }
    }
}