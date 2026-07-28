using System.Collections.Generic;
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
            Permiso permiso);

        void Actualizar(
            Permiso permiso);

        void Activar(
            int idPermiso);

        void Desactivar(
            int idPermiso);
    }
}