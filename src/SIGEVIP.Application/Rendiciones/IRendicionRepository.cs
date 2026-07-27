using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Rendiciones
{
    public interface IRendicionRepository
    {
        Viaje ObtenerPorId(
            int idViaje);

        IReadOnlyCollection<RendicionListadoDto>
            ListarPendientes();

        void Enviar(
            Viaje viaje);

        void ExcluirViatico(
            Viaje viaje,
            Viatico viatico);

        void ReactivarViatico(
            Viaje viaje,
            Viatico viatico);

        void AjustarMontoAnticipado(
            Viaje viaje);

        void Aprobar(
            Viaje viaje);

        void Cancelar(
            Viaje viaje);
    }
}