using System.Collections.Generic;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Visitas
{
    public interface IVisitaRepository
    {
        int Insertar(
            Visita visita,
            AuditoriaRegistro auditoria);

        IReadOnlyCollection<VisitaListadoDto>
            ListarPorViaje(
                int idViaje);

        IReadOnlyCollection<VisitaListadoDto>
            ListarPorCliente(
                int idCliente);
    }
}