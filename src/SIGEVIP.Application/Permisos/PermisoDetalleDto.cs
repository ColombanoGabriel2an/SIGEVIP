namespace SIGEVIP.Application.Permisos
{
    public sealed class PermisoDetalleDto
    {
        public PermisoDetalleDto(
            int idPermiso,
            string codigo,
            string nombre,
            string descripcion,
            int cantidadGrupos,
            bool activo)
        {
            IdPermiso =
                idPermiso;

            Codigo =
                codigo;

            Nombre =
                nombre;

            Descripcion =
                descripcion;

            CantidadGrupos =
                cantidadGrupos;

            Activo =
                activo;
        }

        public int IdPermiso
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

        public int CantidadGrupos
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