namespace SIGEVIP.Application.Usuarios
{
    public sealed class UsuarioFiltro
    {
        public UsuarioFiltro(
            string textoGeneral,
            bool? activo,
            int? idGrupo)
        {
            if (idGrupo.HasValue &&
                idGrupo.Value <= 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(idGrupo),
                    "El identificador del grupo debe ser mayor que cero.");
            }

            TextoGeneral =
                NormalizarTextoOpcional(
                    textoGeneral);

            Activo = activo;
            IdGrupo = idGrupo;
        }

        public string TextoGeneral
        {
            get;
            private set;
        }

        public bool? Activo
        {
            get;
            private set;
        }

        public int? IdGrupo
        {
            get;
            private set;
        }

        public static UsuarioFiltro CrearSinFiltros()
        {
            return new UsuarioFiltro(
                string.Empty,
                null,
                null);
        }

        private static string NormalizarTextoOpcional(
            string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}
