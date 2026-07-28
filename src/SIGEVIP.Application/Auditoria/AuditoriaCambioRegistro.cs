using System;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Application.Auditoria
{
    public sealed class AuditoriaCambioRegistro
    {
        private const int LongitudMaximaCampo = 100;

        public AuditoriaCambioRegistro(
            string campo,
            string valorAnterior,
            string valorNuevo)
        {
            if (string.IsNullOrWhiteSpace(
                campo))
            {
                throw new ReglaNegocioException(
                    "El campo del cambio de auditoría es obligatorio.");
            }

            string campoNormalizado =
                campo.Trim();

            if (campoNormalizado.Length >
                LongitudMaximaCampo)
            {
                throw new ReglaNegocioException(
                    "El campo del cambio de auditoría supera la longitud máxima permitida de " +
                    LongitudMaximaCampo +
                    " caracteres.");
            }

            if (string.Equals(
                valorAnterior,
                valorNuevo,
                StringComparison.Ordinal))
            {
                throw new ReglaNegocioException(
                    "El valor anterior y el valor nuevo deben ser diferentes.");
            }

            Campo = campoNormalizado;
            ValorAnterior = valorAnterior;
            ValorNuevo = valorNuevo;
        }

        public string Campo { get; private set; }

        public string ValorAnterior { get; private set; }

        public string ValorNuevo { get; private set; }
    }
}
