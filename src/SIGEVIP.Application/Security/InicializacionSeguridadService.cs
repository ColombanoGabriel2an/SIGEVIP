using System;

namespace SIGEVIP.Application.Security
{
    public sealed class InicializacionSeguridadService
    {
        public const string MensajeCamposObligatorios =
            "Todos los datos del administrador son obligatorios.";

        public const string MensajePasswordsDiferentes =
            "La contraseña y su confirmación no coinciden.";

        private readonly IInicializacionSeguridadRepository _repository;
        private readonly IPasswordHasher _passwordHasher;

        public InicializacionSeguridadService(
            IInicializacionSeguridadRepository repository,
            IPasswordHasher passwordHasher)
        {
            _repository = repository
                ?? throw new ArgumentNullException(
                    nameof(repository));

            _passwordHasher = passwordHasher
                ?? throw new ArgumentNullException(
                    nameof(passwordHasher));
        }

        public ResultadoInicializacionSeguridad CrearAdministradorInicial(
            string nombre,
            string apellido,
            string email,
            string nombreUsuario,
            string password,
            string confirmacionPassword)
        {
            if (HayDatosObligatoriosVacios(
                nombre,
                apellido,
                email,
                nombreUsuario,
                password,
                confirmacionPassword))
            {
                return ResultadoInicializacionSeguridad.CrearFallido(
                    MensajeCamposObligatorios);
            }

            if (!string.Equals(
                password,
                confirmacionPassword,
                StringComparison.Ordinal))
            {
                return ResultadoInicializacionSeguridad.CrearFallido(
                    MensajePasswordsDiferentes);
            }

            string nombreNormalizado =
                NormalizarNombreUsuario(
                    nombreUsuario);

            if (_repository.ExisteUsuario(
                nombreNormalizado))
            {
                return ResultadoInicializacionSeguridad
                    .CrearUsuarioExistente(
                        nombreNormalizado);
            }

            PasswordHashResult passwordHash =
                _passwordHasher.CrearHash(
                    password);

            bool creado =
                _repository.CrearAdministradorInicial(
                    nombre.Trim(),
                    apellido.Trim(),
                    email.Trim(),
                    nombreNormalizado,
                    passwordHash);

            if (!creado)
            {
                return ResultadoInicializacionSeguridad
                    .CrearUsuarioExistente(
                        nombreNormalizado);
            }

            return ResultadoInicializacionSeguridad.CrearExitoso(
                nombreNormalizado);
        }

        private static bool HayDatosObligatoriosVacios(
            string nombre,
            string apellido,
            string email,
            string nombreUsuario,
            string password,
            string confirmacionPassword)
        {
            return
                string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmacionPassword);
        }

        private static string NormalizarNombreUsuario(
            string nombreUsuario)
        {
            return nombreUsuario
                .Trim()
                .ToLowerInvariant();
        }
    }
}
