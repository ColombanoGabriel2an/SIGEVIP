namespace SIGEVIP.Application.Security
{
    public interface IInicializacionSeguridadRepository
    {
        bool ExisteUsuario(
            string nombreUsuario);

        bool CrearAdministradorInicial(
            string nombre,
            string apellido,
            string email,
            string nombreUsuario,
            PasswordHashResult passwordHash);
    }
}
