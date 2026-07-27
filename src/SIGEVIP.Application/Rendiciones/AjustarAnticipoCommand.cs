using System;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class AjustarAnticipoCommand
    {
        public AjustarAnticipoCommand(
            int idViaje,
            decimal montoAnticipado)
        {
            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje));
            }

            if (montoAnticipado < 0m)
            {
                throw new ReglaNegocioException(
                    "El monto anticipado no puede ser negativo.");
            }

            IdViaje = idViaje;
            MontoAnticipado = montoAnticipado;
        }

        public int IdViaje { get; private set; }

        public decimal MontoAnticipado
        {
            get;
            private set;
        }
    }
}