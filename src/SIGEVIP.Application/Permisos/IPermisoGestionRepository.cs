using System.Collections.Generic;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Permisos
{
    public interface IPermisoGestionRepository
    {
        IReadOnlyCollection<PermisoListadoDto>
            Listar(
                PermisoFiltro filtro);

        PermisoDetalleDto ObtenerDetallePorId(
            int idPermiso);

        Permiso ObtenerPorId(
            int idPermiso);

        bool ExisteCodigo(
            string codigo,
            int? idPermisoExcluido);

        int Insertar(
            Permiso permiso,
            AuditoriaRegistro auditoria);

        void Actualizar(
            Permiso permiso,
            AuditoriaRegistro auditoria);

        void Activar(
            int idPermiso,
            AuditoriaRegistro auditoria);

        void Desactivar(
            int idPermiso,
            AuditoriaRegistro auditoria);
    }
}