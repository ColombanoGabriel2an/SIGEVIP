using System;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Security
{
    public sealed class CambiarClaveService
    {
        public const int LongitudMinimaClave = 8;

        public const string MensajeSesionRequerida =
            "Debe iniciar sesión para cambiar la clave.";

        public const string MensajeCamposObligatorios =
            "Debe completar la clave actual, la clave nueva y la confirmación.";

        public const string MensajeConfirmacionInvalida =
            "La confirmación es inválida.";

        public const string MensajeClaveActualInvalida =
            "La clave actual es inválida.";

        public const string MensajeClaveNuevaCorta =
            "La clave nueva debe contener al menos 8 caracteres.";

        public const string MensajeUsuarioNoDisponible =
            "No fue posible cambiar la clave porque el usuario ya no se encuentra activo.";

        private readonly IUsuarioClaveRepository
            _repository;

        private readonly IPasswordHasher
            _passwordHasher;

        private readonly ISesionActual
            _sesionActual;

        public CambiarClaveService(
            IUsuarioClaveRepository repository,
            IPasswordHasher passwordHasher,
            ISesionActual sesionActual)
        {
            _repository =
                repository
                ?? throw new ArgumentNullException(
                    nameof(repository));

            _passwordHasher =
                passwordHasher
                ?? throw new ArgumentNullException(
                    nameof(passwordHasher));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));
        }

        public void Cambiar(
            CambiarClaveCommand command)
        {
            Usuario usuario =
                ObtenerUsuarioAutenticado();

            ValidarCommand(
                command);

            bool claveActualValida =
                _passwordHasher.Verificar(
                    command.ClaveActual,
                    usuario.PasswordHash,
                    usuario.PasswordSalt,
                    usuario.IteracionesPassword);

            if (!claveActualValida)
            {
                throw new ReglaNegocioException(
                    MensajeClaveActualInvalida);
            }

            PasswordHashResult nuevoPasswordHash =
                _passwordHasher.CrearHash(
                    command.ClaveNueva);

            bool actualizado =
                _repository.ActualizarCredenciales(
                    usuario.IdUsuario,
                    nuevoPasswordHash);

            if (!actualizado)
            {
                throw new ReglaNegocioException(
                    MensajeUsuarioNoDisponible);
            }

            usuario.ActualizarCredenciales(
                nuevoPasswordHash.Hash,
                nuevoPasswordHash.Salt,
                nuevoPasswordHash.Iteraciones);
        }

        private Usuario ObtenerUsuarioAutenticado()
        {
            if (!_sesionActual.HayUsuarioAutenticado ||
                _sesionActual.UsuarioActual == null)
            {
                throw new AccesoDenegadoException(
                    MensajeSesionRequerida);
            }

            Usuario usuario =
                _sesionActual.UsuarioActual;

            if (!usuario.Activo)
            {
                throw new AccesoDenegadoException(
                    MensajeSesionRequerida);
            }

            return usuario;
        }

        private static void ValidarCommand(
            CambiarClaveCommand command)
        {
            if (command == null)
            {
                throw new ReglaNegocioException(
                    MensajeCamposObligatorios);
            }

            if (string.IsNullOrWhiteSpace(
                    command.ClaveActual) ||
                string.IsNullOrWhiteSpace(
                    command.ClaveNueva) ||
                string.IsNullOrWhiteSpace(
                    command.ConfirmacionClaveNueva))
            {
                throw new ReglaNegocioException(
                    MensajeCamposObligatorios);
            }

            if (command.ClaveNueva.Length <
                LongitudMinimaClave)
            {
                throw new ReglaNegocioException(
                    MensajeClaveNuevaCorta);
            }

            if (!string.Equals(
                command.ClaveNueva,
                command.ConfirmacionClaveNueva,
                StringComparison.Ordinal))
            {
                throw new ReglaNegocioException(
                    MensajeConfirmacionInvalida);
            }
        }
    }
}
