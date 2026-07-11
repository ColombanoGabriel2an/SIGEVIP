using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Security
{
    public interface IUsuarioAutenticacionRepository
    {
        Usuario BuscarPorNombreUsuario(string nombreUsuario);
    }
}
