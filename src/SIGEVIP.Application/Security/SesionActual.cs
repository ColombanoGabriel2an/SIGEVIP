using System;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Security
{
    public sealed class SesionActual : ISesionActual
    {
        public bool HayUsuarioAutenticado
        {
            get { return UsuarioActual != null; }
        }

        public Usuario UsuarioActual { get; private set; }

        public void Iniciar(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new ArgumentNullException(nameof(usuario));
            }

            if (!usuario.Activo)
            {
                throw new InvalidOperationException(
                    "No se puede iniciar sesión con un usuario inactivo.");
            }

            UsuarioActual = usuario;
        }

        public void Cerrar()
        {
            UsuarioActual = null;
        }
    }
}
