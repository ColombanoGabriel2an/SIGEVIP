namespace SIGEVIP.Application.Grupos
{
    public sealed class PermisoEfectivoGrupoDto
    {
        public PermisoEfectivoGrupoDto(
            int idPermiso,
            string codigo,
            string nombre,
            string descripcion,
            bool directo)
        {
            IdPermiso = idPermiso;
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Directo = directo;
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

        public bool Directo
        {
            get;
            private set;
        }

        public string Origen
        {
            get
            {
                return Directo
                    ? "Directo"
                    : "Heredado";
            }
        }

        public string Texto
        {
            get
            {
                return "[" +
                    Origen +
                    "] " +
                    Codigo +
                    " - " +
                    Nombre;
            }
        }
    }
}
