namespace SIGEVIP.Application.Permisos
{
    public sealed class RegistrarPermisoCommand
    {
        public RegistrarPermisoCommand(
            string codigo,
            string nombre,
            string descripcion)
        {
            Codigo =
                codigo;

            Nombre =
                nombre;

            Descripcion =
                descripcion;
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
    }
}