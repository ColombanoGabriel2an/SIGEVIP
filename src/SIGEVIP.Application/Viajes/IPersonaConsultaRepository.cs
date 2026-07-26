using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Viajes
{
    public interface IPersonaConsultaRepository
    {
        IReadOnlyCollection<PersonaSeleccionDto>
            ListarActivas();

        IReadOnlyCollection<Persona>
            ObtenerPorIds(
                IReadOnlyCollection<int> idsPersona);
    }
}
