namespace SIGEVIP.Application.Grupos
{
    public sealed class PermisoSeleccionGrupoDto
    {
        public PermisoSeleccionGrupoDto(
            int idPermiso,
            string codigo,
            string nombre,
            string descripcion,
            bool seleccionado)
        {
            IdPermiso = idPermiso;
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Seleccionado = seleccionado;
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
                    Nombre;
            }
        }
    }
}
