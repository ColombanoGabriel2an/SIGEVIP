using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Grupos
{
    public interface IGrupoGestionRepository
    {
        IReadOnlyCollection<GrupoListadoDto> Listar(
            GrupoFiltro filtro);

        GrupoDetalleDto ObtenerDetallePorId(
            int idGrupo);

        Grupo ObtenerPorId(
            int idGrupo);

        IReadOnlyCollection<PermisoSeleccionGrupoDto>
            ListarPermisosActivos(
                IReadOnlyCollection<int> idsSeleccionados);

        IReadOnlyCollection<GrupoSeleccionGrupoDto>
            ListarGruposActivos(
                int? idGrupoPadre,
                IReadOnlyCollection<int> idsSeleccionados);

        IReadOnlyCollection<Permiso>
            ObtenerPermisosPorIds(
                IReadOnlyCollection<int> idsPermisos);

        IReadOnlyCollection<Grupo>
            ObtenerGruposPorIds(
                IReadOnlyCollection<int> idsGrupos);

        bool ExisteCodigo(
            string codigo,
            int? idGrupoExcluido);

        bool ExisteNombre(
            string nombre,
            int? idGrupoExcluido);

        bool ExistePermisoActivo(
            string codigoPermiso);

        int Insertar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos);

        int Insertar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos,
            IReadOnlyCollection<int> idsGruposHijos);

        void Actualizar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos);

        void Actualizar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos,
            IReadOnlyCollection<int> idsGruposHijos);

        void Activar(
            int idGrupo);

        void Desactivar(
            int idGrupo);
    }
}
