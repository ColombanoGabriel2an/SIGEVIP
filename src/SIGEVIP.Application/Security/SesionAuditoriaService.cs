using System;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Security
{
    public sealed class SesionAuditoriaService
    {
        public const string ModuloAuditoria =
            "Seguridad";

        public const string EntidadAuditoria =
            "Sesion";

        public const string AccionInicioSesion =
            "InicioSesion";

        public const string AccionCierreSesion =
            "CierreSesion";

        private readonly ISesionAuditoriaRepository
            _repository;

        private readonly ISesionActual
            _sesionActual;

        public SesionAuditoriaService(
            ISesionAuditoriaRepository repository,
            ISesionActual sesionActual)
        {
            _repository =
                repository
                ?? throw new ArgumentNullException(
                    nameof(repository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));
        }

        public void RegistrarInicioSesion()
        {
            Usuario usuario =
                ObtenerUsuarioAutenticado();

            _repository.Registrar(
                CrearRegistro(
                    usuario,
                    AccionInicioSesion,
                    "El usuario " +
                    usuario.NombreUsuario +
                    " inició sesión."));
        }

        public void RegistrarCierreSesion()
        {
            Usuario usuario =
                ObtenerUsuarioAutenticado();

            _repository.Registrar(
                CrearRegistro(
                    usuario,
                    AccionCierreSesion,
                    "El usuario " +
                    usuario.NombreUsuario +
                    " cerró sesión."));
        }

        private Usuario ObtenerUsuarioAutenticado()
        {
            if (!_sesionActual.HayUsuarioAutenticado ||
                _sesionActual.UsuarioActual == null)
            {
                throw new InvalidOperationException(
                    "No existe una sesión autenticada para auditar.");
            }

            return _sesionActual.UsuarioActual;
        }

        private static AuditoriaRegistro CrearRegistro(
            Usuario usuario,
            string accion,
            string descripcion)
        {
            return new AuditoriaRegistro(
                usuario.IdUsuario,
                usuario.NombreUsuario,
                ModuloAuditoria,
                accion,
                EntidadAuditoria,
                usuario.IdUsuario,
                descripcion);
        }
    }
}