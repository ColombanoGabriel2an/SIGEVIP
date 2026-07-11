using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.States
{
    public sealed class EstadoViajeAprobado : IEstadoViaje
    {
        public EstadoViaje Estado
        {
            get { return EstadoViaje.Aprobado; }
        }

        public void ValidarModificacion()
        {
            throw new ReglaNegocioException(
                "El viaje se encuentra Aprobado y no admite modificaciones.");
        }

        public IEstadoViaje EnviarARendicion()
        {
            throw new ReglaNegocioException(
                "Un viaje Aprobado no puede volver a EnRendicion.");
        }

        public IEstadoViaje Aprobar()
        {
            throw new ReglaNegocioException(
                "El viaje ya se encuentra Aprobado.");
        }

        public IEstadoViaje Cancelar()
        {
            throw new ReglaNegocioException(
                "Un viaje Aprobado no puede ser cancelado.");
        }
    }
}
