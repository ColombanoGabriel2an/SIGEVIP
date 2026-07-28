namespace SIGEVIP.Application.Grupos
{
    public sealed class GrupoSeleccionGrupoDto
    {
        public GrupoSeleccionGrupoDto(
            int idGrupo,
            string codigo,
            string nombre,
            string descripcion,
            bool activo,
            bool seleccionado)
        {
            IdGrupo = idGrupo;
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Activo = activo;
            Seleccionado = seleccionado;
        }

        public int IdGrupo
        {
            get;
            private set;
        }

        public string Codigo
        {
            get;
            private set;
        }

        public string Nombre
        {
            get;
            private set;
        }

        public string Descripcion
        {
            get;
            private set;
        }

        public bool Activo
        {
            get;
            private set;
        }

        public bool Seleccionado
        {
            get;
            private set;
        }

        public string Texto
        {
            get
            {
                return Codigo +
                    " - " +
                    Nombre +
                    (
                        Activo
                            ? string.Empty
                            : " (Inactivo)"
                    );
            }
        }
    }
}
