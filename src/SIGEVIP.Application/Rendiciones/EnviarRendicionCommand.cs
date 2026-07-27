using System;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class EnviarRendicionCommand
    {
        public EnviarRendicionCommand(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje),
                    "El identificador del viaje debe ser mayor que cero.");
            }

            IdViaje = idViaje;
        }

        public int IdViaje { get; private set; }
    }
}