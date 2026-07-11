using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.States
{
    public static class EstadoViajeFactory
    {
        public static IEstadoViaje Crear(EstadoViaje estado)
        {
            switch (estado)
            {
                case EstadoViaje.Abierto:
                    return new EstadoViajeAbierto();

                case EstadoViaje.EnRendicion:
                    return new EstadoViajeEnRendicion();

                case EstadoViaje.Aprobado:
                    return new EstadoViajeAprobado();

                case EstadoViaje.Cancelado:
                    return new EstadoViajeCancelado();

                default:
                    throw new ReglaNegocioException(
                        "El estado indicado para el viaje no es válido.");
            }
        }
    }
}
