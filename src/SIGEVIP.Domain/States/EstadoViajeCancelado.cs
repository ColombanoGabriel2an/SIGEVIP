using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.States
{
    public sealed class EstadoViajeCancelado : IEstadoViaje
    {
        public EstadoViaje Estado
        {
            get { return EstadoViaje.Cancelado; }
        }

        public void ValidarModificacion()
        {
            throw new ReglaNegocioException(
                "El viaje se encuentra Cancelado y no admite modificaciones.");
        }

        public IEstadoViaje EnviarARendicion()
        {
            throw new ReglaNegocioException(
                "Un viaje Cancelado no puede enviarse a rendición.");
        }

        public IEstadoViaje Aprobar()
        {
            throw new ReglaNegocioException(
                "Un viaje Cancelado no puede ser aprobado.");
        }

        public IEstadoViaje Cancelar()
        {
            throw new ReglaNegocioException(
                "El viaje ya se encuentra Cancelado.");
        }
    }
}
