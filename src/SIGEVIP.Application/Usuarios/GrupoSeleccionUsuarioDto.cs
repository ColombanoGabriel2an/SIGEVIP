namespace SIGEVIP.Application.Usuarios
{
    public sealed class GrupoSeleccionUsuarioDto
    {
        public GrupoSeleccionUsuarioDto(
            int idGrupo,
            string codigo,
            string nombre,
            bool activo,
            bool seleccionado)
        {
            IdGrupo = idGrupo;
            Codigo = codigo;
            Nombre = nombre;
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

        public string Descripcion
        {
            get
            {
                return string.IsNullOrWhiteSpace(Codigo)
                    ? Nombre
                    : Nombre + " (" + Codigo + ")";
            }
        }
    }
}
