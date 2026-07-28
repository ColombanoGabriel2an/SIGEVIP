using System.Collections.Generic;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Viajes
{
    public interface IViajeRepository
    {
        Viaje ObtenerPorId(
            int idViaje);

        IReadOnlyCollection<ViajeListadoDto> Listar(
            ViajeFiltro filtro);

        int Insertar(
            Viaje viaje,
            AuditoriaRegistro auditoria);

        void Actualizar(
            Viaje viaje,
            AuditoriaRegistro auditoria);

        void Cancelar(
            Viaje viaje,
            AuditoriaRegistro auditoria);
    }
}
