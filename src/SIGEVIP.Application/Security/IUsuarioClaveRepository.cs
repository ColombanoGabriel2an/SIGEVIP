namespace SIGEVIP.Application.Security
{
    public interface IUsuarioClaveRepository
    {
        bool ActualizarCredenciales(
            int idUsuario,
            PasswordHashResult passwordHash);
    }
}
