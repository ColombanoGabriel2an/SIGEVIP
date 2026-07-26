using System.Collections.Generic;
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
            Viaje viaje);

        void Actualizar(
            Viaje viaje);

        void Cancelar(
            Viaje viaje);
    }
}
