namespace CineCore.Helpers
{
    public static class Mensajes
    {
        public static class Reserva
        {
            public const string Exito = "Reserva realizada con éxito.";
            public const string Cancelada = "Reserva cancelada.";
            public const string ButacaYaReservada = "Esa butaca ya fue reservada.";
            public const string ButacaNoPertenece = "Alguna de las butacas seleccionadas no pertenece a esta sala.";
            public const string ButacaTomadaPorOtro = "Alguna de las butacas ya fue reservada por otro cliente. Elegí nuevamente.";
            public const string FuncionYaPaso = "Esta función ya pasó. Elegí otra de la cartelera.";
            public const string PasadaNoCancelable = "No se pueden cancelar reservas de funciones que ya pasaron.";
            public const string ErrorAlCrear = "No se pudo completar la reserva. Intentá nuevamente.";

            public static string ExitoMultiple(int cantidad) =>
                $"Se reservaron {cantidad} butacas con éxito.";

            public static string CanceladasMultiples(int cantidad) =>
                $"Se cancelaron {cantidad} butacas.";

            public static string CantidadInvalida(int maximo) =>
                $"Tenés que seleccionar entre 1 y {maximo} butacas.";
        }

        public static class Funcion
        {
            public const string Creada = "Función creada con éxito.";
            public const string Actualizada = "Función actualizada.";
            public const string Eliminada = "Función eliminada.";
            public const string PasadaNoEditable = "No se pueden editar funciones que ya pasaron.";
            public const string PasadaNoEliminable = "No se pueden eliminar funciones que ya pasaron.";
            public const string ConReservasNoEliminable = "No se puede eliminar una función con reservas activas.";

            public static string Solapada(string titulo, DateTime fechaExistente) =>
                $"La función se solapa con \"{titulo}\" del {fechaExistente:dd/MM/yyyy HH:mm} en la misma sala.";
        }

        public static class Pelicula
        {
            public static string Creada(string titulo) => $"Película \"{titulo}\" creada.";
            public static string Eliminada(string titulo) => $"Película \"{titulo}\" eliminada.";
            public const string Actualizada = "Película actualizada.";

            public static string ConFuncionesNoEliminable(string titulo, int cantidad) =>
                $"No se puede eliminar \"{titulo}\" porque tiene {cantidad} función/es asociada/s. Eliminá las funciones primero.";
        }

        public static class Sala
        {
            public static string Creada(int numero, int capacidad) =>
                $"Sala {numero} creada con {capacidad} butacas.";

            public static string Eliminada(int numero) => $"Sala {numero} eliminada.";
            public const string Actualizada = "Sala actualizada.";

            public const string CapacidadInmutable =
                "No se puede modificar la capacidad de una sala existente porque las butacas ya fueron generadas.";

            public static string NumeroDuplicado(int numero) =>
                $"Ya existe una sala con el número {numero}.";

            public static string ConFuncionesNoEliminable(int numero, int cantidad) =>
                $"No se puede eliminar la sala {numero} porque tiene {cantidad} función/es asociada/s.";
        }

        public static class Genero
        {
            public static string Eliminado(string nombre) => $"Género \"{nombre}\" eliminado.";
            public const string NombreDuplicado = "Ya existe un género con ese nombre.";

            public static string ConPeliculasNoEliminable(string nombre, int cantidad) =>
                $"No se puede eliminar el género \"{nombre}\" porque tiene {cantidad} película/s asociada/s.";
        }

        public static class TipoSala
        {
            public static string Eliminado(string nombre) => $"Tipo de sala \"{nombre}\" eliminado.";
            public const string NombreDuplicado = "Ya existe un tipo de sala con ese nombre.";

            public static string ConSalasNoEliminable(string nombre, int cantidad) =>
                $"No se puede eliminar el tipo \"{nombre}\" porque tiene {cantidad} sala/s asociada/s.";
        }
    }
}