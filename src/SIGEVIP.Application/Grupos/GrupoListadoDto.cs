namespace SIGEVIP.Application.Grupos
{
    public sealed class GrupoListadoDto
    {
        public GrupoListadoDto(
            int idGrupo,
            string codigo,
            string nombre,
            string descripcion,
            int cantidadPermisos,
            int cantidadUsuarios,
            bool activo)
        {
            IdGrupo = idGrupo;
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            CantidadPermisos = cantidadPermisos;
            CantidadUsuarios = cantidadUsuarios;
            Activo = activo;
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

        public int CantidadPermisos
        {
            get;
            private set;
        }

        public int CantidadUsuarios
        {
            get;
            private set;
        }

        public bool Activo
        {
            get;
            private set;
        }

        public string Estado
        {
            get
            {
                return Activo
                    ? "Activo"
                    : "Inactivo";
            }
        }
    }
}
