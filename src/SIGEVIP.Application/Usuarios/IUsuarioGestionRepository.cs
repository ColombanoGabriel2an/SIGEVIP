using System.Collections.Generic;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Usuarios
{
    public interface IUsuarioGestionRepository
    {
        IReadOnlyCollection<UsuarioListadoDto> Listar(
            UsuarioFiltro filtro);

        UsuarioDetalleDto ObtenerDetallePorId(
            int idUsuario);

        Usuario ObtenerPorId(
            int idUsuario);

        Persona ObtenerPersonaPorId(
            int idPersona);

        IReadOnlyCollection<PersonaSeleccionUsuarioDto>
            ListarPersonasDisponibles();

        IReadOnlyCollection<GrupoSeleccionUsuarioDto>
            ListarGruposActivos(
                IReadOnlyCollection<int> idsSeleccionados);

        IReadOnlyCollection<Grupo> ObtenerGruposPorIds(
            IReadOnlyCollection<int> idsGrupos);

        bool ExisteNombreUsuario(
            string nombreUsuario,
            int? idUsuarioExcluido);

        bool PersonaTieneUsuario(
            int idPersona);

        bool ExisteOtroAdministradorActivo(
            int idUsuarioExcluido);

        int Insertar(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos,
            AuditoriaRegistro auditoria);

        void Actualizar(
            Usuario usuario,
            IReadOnlyCollection<int> idsGrupos,
            AuditoriaRegistro auditoria);

        void Activar(
            int idUsuario,
            AuditoriaRegistro auditoria);

        void Desactivar(
            int idUsuario,
            AuditoriaRegistro auditoria);
    }
}