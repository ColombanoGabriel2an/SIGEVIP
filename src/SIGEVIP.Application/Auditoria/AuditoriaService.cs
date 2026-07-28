using System;
using System.Collections.Generic;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Auditoria
{
    public sealed class AuditoriaService
    {
        public const string PermisoConsultar =
            "AUDITORIA_CONSULTAR";

        private readonly IAuditoriaRepository
            _auditoriaRepository;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        public AuditoriaService(
            IAuditoriaRepository auditoriaRepository,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _auditoriaRepository =
                auditoriaRepository
                ?? throw new ArgumentNullException(
                    nameof(auditoriaRepository));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));
        }

        public IReadOnlyCollection<AuditoriaListadoDto>
            Listar(
                AuditoriaFiltro filtro)
        {
            ExigirPermisoConsultar();

            AuditoriaFiltro filtroEfectivo =
                filtro
                ?? AuditoriaFiltro
                    .CrearSinFiltros();

            ValidarFiltro(
                filtroEfectivo);

            return _auditoriaRepository
                .Listar(
                    filtroEfectivo);
        }

        private void ExigirPermisoConsultar()
        {
            if (!_sesionActual
                    .HayUsuarioAutenticado ||
                _sesionActual
                    .UsuarioActual == null)
            {
                throw new AccesoDenegadoException(
                    "Debe iniciar sesión para consultar la auditoría.");
            }

            if (!_sesionActual
                    .UsuarioActual
                    .Activo)
            {
                throw new AccesoDenegadoException(
                    "El usuario autenticado se encuentra inactivo.");
            }

            if (!_autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        PermisoConsultar))
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para consultar la auditoría.");
            }
        }

        private static void ValidarFiltro(
            AuditoriaFiltro filtro)
        {
            if (filtro.FechaDesde.HasValue &&
                filtro.FechaHasta.HasValue &&
                filtro.FechaDesde.Value >
                    filtro.FechaHasta.Value)
            {
                throw new ReglaNegocioException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }
        }
    }
}