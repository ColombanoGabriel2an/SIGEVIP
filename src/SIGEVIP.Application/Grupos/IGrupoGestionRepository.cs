using System.Collections.Generic;
using SIGEVIP.Application.Auditoria;
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

        IReadOnlyCollection<PermisoEfectivoGrupoDto>
            ObtenerPermisosEfectivosVistaPrevia(
                IReadOnlyCollection<int> idsPermisosDirectos,
                IReadOnlyCollection<int> idsGruposHijos);

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
            IReadOnlyCollection<int> idsPermisos,
            AuditoriaRegistro auditoria);

        int Insertar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos,
            IReadOnlyCollection<int> idsGruposHijos,
            AuditoriaRegistro auditoria);

        void Actualizar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos,
            AuditoriaRegistro auditoria);

        void Actualizar(
            Grupo grupo,
            IReadOnlyCollection<int> idsPermisos,
            IReadOnlyCollection<int> idsGruposHijos,
            AuditoriaRegistro auditoria);

        void Activar(
            int idGrupo,
            AuditoriaRegistro auditoria);

        void Desactivar(
            int idGrupo,
            AuditoriaRegistro auditoria);
    }
}