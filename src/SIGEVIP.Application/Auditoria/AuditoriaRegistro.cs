using System;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Auditoria
{
    public sealed class AuditoriaRegistro
    {
        private const int LongitudMaximaNombreUsuario = 100;
        private const int LongitudMaximaModulo = 50;
        private const int LongitudMaximaAccion = 50;
        private const int LongitudMaximaEntidad = 100;
        private const int LongitudMaximaDescripcion = 1000;

        public AuditoriaRegistro(
            int idUsuario,
            string nombreUsuario,
            string modulo,
            string accion,
            string entidad,
            int? idEntidad,
            string descripcion)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idUsuario),
                    "El identificador del usuario de auditoría debe ser mayor que cero.");
            }

            if (idEntidad.HasValue &&
                idEntidad.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idEntidad),
                    "El identificador de la entidad auditada debe ser mayor que cero.");
            }

            IdUsuario =
                idUsuario;

            NombreUsuario =
                ValidarTexto(
                    nombreUsuario,
                    LongitudMaximaNombreUsuario,
                    "El nombre de usuario de auditoría es obligatorio.");

            Modulo =
                ValidarTexto(
                    modulo,
                    LongitudMaximaModulo,
                    "El módulo de auditoría es obligatorio.");

            Accion =
                ValidarTexto(
                    accion,
                    LongitudMaximaAccion,
                    "La acción de auditoría es obligatoria.");

            Entidad =
                ValidarTexto(
                    entidad,
                    LongitudMaximaEntidad,
                    "La entidad de auditoría es obligatoria.");

            IdEntidad =
                idEntidad;

            Descripcion =
                ValidarTexto(
                    descripcion,
                    LongitudMaximaDescripcion,
                    "La descripción de auditoría es obligatoria.");
        }

        public int IdUsuario
        {
            get;
            private set;
        }

        public string NombreUsuario
        {
            get;
            private set;
        }

        public string Modulo
        {
            get;
            private set;
        }

        public string Accion
        {
            get;
            private set;
        }

        public string Entidad
        {
            get;
            private set;
        }

        public int? IdEntidad
        {
            get;
            private set;
        }

        public string Descripcion
        {
            get;
            private set;
        }

        public AuditoriaRegistro ConIdEntidad(
            int idEntidad)
        {
            return new AuditoriaRegistro(
                IdUsuario,
                NombreUsuario,
                Modulo,
                Accion,
                Entidad,
                idEntidad,
                Descripcion);
        }

        private static string ValidarTexto(
            string valor,
            int longitudMaxima,
            string mensajeObligatorio)
        {
            if (string.IsNullOrWhiteSpace(
                valor))
            {
                throw new ReglaNegocioException(
                    mensajeObligatorio);
            }

            string normalizado =
                valor.Trim();

            if (normalizado.Length >
                longitudMaxima)
            {
                throw new ReglaNegocioException(
                    "El valor de auditoría supera la longitud máxima permitida de " +
                    longitudMaxima +
                    " caracteres.");
            }

            return normalizado;
        }
    }
}