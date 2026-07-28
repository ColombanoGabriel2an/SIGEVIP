namespace SIGEVIP.Application.Permisos
{
    public sealed class ModificarPermisoCommand
    {
        public ModificarPermisoCommand(
            int idPermiso,
            string nombre,
            string descripcion)
        {
            IdPermiso =
                idPermiso;

            Nombre =
                nombre;

            Descripcion =
                descripcion;
        }

        public int IdPermiso
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
    }
}