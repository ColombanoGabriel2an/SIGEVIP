using System.Collections.Generic;
using SIGEVIP.Application.Auditoria;
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
            Viaje viaje,
            AuditoriaRegistro auditoria);

        void ExcluirViatico(
            Viaje viaje,
            Viatico viatico,
            AuditoriaRegistro auditoria);

        void ReactivarViatico(
            Viaje viaje,
            Viatico viatico,
            AuditoriaRegistro auditoria);

        void AjustarMontoAnticipado(
            Viaje viaje,
            AuditoriaRegistro auditoria);

        void Aprobar(
            Viaje viaje);

        void Cancelar(
            Viaje viaje);
    }
}