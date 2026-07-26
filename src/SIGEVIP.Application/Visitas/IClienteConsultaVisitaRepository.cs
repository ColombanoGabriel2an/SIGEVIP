using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Visitas
{
    public interface IClienteConsultaVisitaRepository
    {
        IReadOnlyCollection<ClienteSeleccionVisitaDto>
            ListarActivos();

        IReadOnlyCollection<Cliente>
            ObtenerPorIds(
                IReadOnlyCollection<int> idsClientes);
    }
}