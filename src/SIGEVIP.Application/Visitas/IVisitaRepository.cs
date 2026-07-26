using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Visitas
{
    public interface IVisitaRepository
    {
        int Insertar(
            Visita visita);

        IReadOnlyCollection<VisitaListadoDto>
            ListarPorViaje(
                int idViaje);

        IReadOnlyCollection<VisitaListadoDto>
            ListarPorCliente(
                int idCliente);
    }
}