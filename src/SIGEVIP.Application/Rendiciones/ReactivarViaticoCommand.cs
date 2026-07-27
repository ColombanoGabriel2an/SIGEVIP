using System;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class ReactivarViaticoCommand
    {
        public ReactivarViaticoCommand(
            int idViaje,
            int idViatico)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje));
            }

            if (idViatico <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViatico));
            }

            IdViaje = idViaje;
            IdViatico = idViatico;
        }

        public int IdViaje { get; private set; }

        public int IdViatico { get; private set; }
    }
}