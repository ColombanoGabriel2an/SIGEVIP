using System.Collections.Generic;

namespace SIGEVIP.Application.Auditoria
{
    public interface IAuditoriaRepository
    {
        IReadOnlyCollection<AuditoriaListadoDto>
            Listar(
                AuditoriaFiltro filtro);
    }
}