using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Security
{
    public interface ISesionActual
    {
        bool HayUsuarioAutenticado { get; }

        Usuario UsuarioActual { get; }

        void Iniciar(Usuario usuario);

        void Cerrar();
    }
}
