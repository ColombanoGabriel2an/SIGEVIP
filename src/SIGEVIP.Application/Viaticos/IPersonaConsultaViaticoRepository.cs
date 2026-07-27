using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Viaticos
{
    public interface
        IPersonaConsultaViaticoRepository
    {
        IReadOnlyCollection<PagadorSeleccionDto>
            ListarActivas();

        Persona ObtenerPorId(
            int idPersona);
    }
}
