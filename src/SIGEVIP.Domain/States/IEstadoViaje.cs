using SIGEVIP.Domain.Enums;

namespace SIGEVIP.Domain.States
{
    public interface IEstadoViaje
    {
        EstadoViaje Estado { get; }

        void ValidarModificacion();

        IEstadoViaje EnviarARendicion();

        IEstadoViaje Aprobar();

        IEstadoViaje Cancelar();
    }
}
