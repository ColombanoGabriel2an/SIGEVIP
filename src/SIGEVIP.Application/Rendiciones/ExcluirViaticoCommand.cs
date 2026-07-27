using System;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class ExcluirViaticoCommand
    {
        public ExcluirViaticoCommand(
            int idViaje,
            int idViatico,
            string motivo)
        {
            ValidarIds(
                idViaje,
                idViatico);

            if (string.IsNullOrWhiteSpace(
                motivo))
            {
                throw new ReglaNegocioException(
                    "El motivo de exclusión es obligatorio.");
            }

            IdViaje = idViaje;
            IdViatico = idViatico;
            Motivo = motivo.Trim();
        }

        public int IdViaje { get; private set; }

        public int IdViatico { get; private set; }

        public string Motivo { get; private set; }

        private static void ValidarIds(
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
        }
    }
}