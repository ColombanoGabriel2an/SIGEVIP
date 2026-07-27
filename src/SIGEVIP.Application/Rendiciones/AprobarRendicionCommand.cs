using System;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class AprobarRendicionCommand
    {
        public AprobarRendicionCommand(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje));
            }

            IdViaje = idViaje;
        }

        public int IdViaje { get; private set; }
    }
}