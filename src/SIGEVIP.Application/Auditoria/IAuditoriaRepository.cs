using System.Collections.Generic;

namespace SIGEVIP.Application.Auditoria
{
    public interface IAuditoriaRepository
    {
        IReadOnlyCollection<AuditoriaListadoDto>
            Listar(
                AuditoriaFiltro filtro);

        IReadOnlyCollection<AuditoriaCambioDto>
            ObtenerCambios(
                long idAuditoria);

        IReadOnlyCollection<string>
            ListarModulos();

        IReadOnlyCollection<string>
            ListarAcciones(
                string modulo);
    }
}