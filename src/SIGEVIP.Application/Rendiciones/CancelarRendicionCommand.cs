using System;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class CancelarRendicionCommand
    {
        public CancelarRendicionCommand(
            int idViaje,
            string motivo)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje));
            }

            if (string.IsNullOrWhiteSpace(
                motivo))
            {
                throw new ReglaNegocioException(
                    "El motivo de cancelación es obligatorio.");
            }

            IdViaje = idViaje;
            Motivo = motivo.Trim();
        }

        public int IdViaje { get; private set; }

        public string Motivo { get; private set; }
    }
}