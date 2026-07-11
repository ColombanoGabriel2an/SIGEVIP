using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.States
{
    public sealed class EstadoViajeEnRendicion : IEstadoViaje
    {
        public EstadoViaje Estado
        {
            get { return EstadoViaje.EnRendicion; }
        }

        public void ValidarModificacion()
        {
            throw new ReglaNegocioException(
                "El viaje se encuentra EnRendicion y no admite modificaciones administrativas.");
        }

        public IEstadoViaje EnviarARendicion()
        {
            throw new ReglaNegocioException(
                "El viaje ya se encuentra EnRendicion.");
        }

        public IEstadoViaje Aprobar()
        {
            return new EstadoViajeAprobado();
        }

        public IEstadoViaje Cancelar()
        {
            return new EstadoViajeCancelado();
        }
    }
}
