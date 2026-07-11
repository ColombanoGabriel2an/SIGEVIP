using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.States
{
    public sealed class EstadoViajeAbierto : IEstadoViaje
    {
        public EstadoViaje Estado
        {
            get { return EstadoViaje.Abierto; }
        }

        public void ValidarModificacion()
        {
        }

        public IEstadoViaje EnviarARendicion()
        {
            return new EstadoViajeEnRendicion();
        }

        public IEstadoViaje Aprobar()
        {
            throw new ReglaNegocioException(
                "No se puede aprobar un viaje que todavía se encuentra Abierto.");
        }

        public IEstadoViaje Cancelar()
        {
            return new EstadoViajeCancelado();
        }
    }
}
