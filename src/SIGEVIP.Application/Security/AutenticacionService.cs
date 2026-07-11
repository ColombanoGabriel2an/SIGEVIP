using System;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Security
{
    public sealed class AutenticacionService
    {
        public const string MensajeCredencialesInvalidas =
            "Credenciales inválidas.";

        private readonly IUsuarioAutenticacionRepository _repository;
        private readonly IPasswordHasher _passwordHasher;

        public AutenticacionService(
            IUsuarioAutenticacionRepository repository,
            IPasswordHasher passwordHasher)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));

            _passwordHasher = passwordHasher
                ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public ResultadoAutenticacion Autenticar(
            string nombreUsuario,
            string password)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return CrearResultadoFallido();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return CrearResultadoFallido();
            }

            string nombreNormalizado =
                NormalizarNombreUsuario(nombreUsuario);

            Usuario usuario =
                _repository.BuscarPorNombreUsuario(
                    nombreNormalizado);

            if (usuario == null || !usuario.Activo)
            {
                return CrearResultadoFallido();
            }

            bool passwordValido =
                _passwordHasher.Verificar(
                    password,
                    usuario.PasswordHash,
                    usuario.PasswordSalt,
                    usuario.IteracionesPassword);

            if (!passwordValido)
            {
                return CrearResultadoFallido();
            }

            return ResultadoAutenticacion.CrearExitoso(
                usuario);
        }

        private static ResultadoAutenticacion CrearResultadoFallido()
        {
            return ResultadoAutenticacion.CrearFallido(
                MensajeCredencialesInvalidas);
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
