using System;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Security
{
    public sealed class AutorizacionService
    {
        public bool TienePermiso(
            Usuario usuario,
            string codigoPermiso)
        {
            if (usuario == null || !usuario.Activo)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(codigoPermiso))
            {
                throw new ArgumentException(
                    "El código del permiso es obligatorio.",
                    nameof(codigoPermiso));
            }

            string codigoNormalizado =
                NormalizarCodigo(codigoPermiso);

            foreach (Grupo grupo in usuario.Grupos)
            {
                if (!grupo.Activo)
                {
                    continue;
                }

                foreach (
                    Permiso permiso
                    in grupo.ObtenerPermisosEfectivos())
                {
                    if (string.Equals(
                        permiso.Codigo,
                        codigoNormalizado,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string NormalizarCodigo(
            string codigo)
        {
            return codigo
                .Trim()
                .ToUpperInvariant();
        }
    }
}
