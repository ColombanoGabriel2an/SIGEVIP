using System;

namespace SIGEVIP.Application.Auditoria
{
    public sealed class AuditoriaCambioDto
    {
        public AuditoriaCambioDto(
            long idAuditoriaCambio,
            long idAuditoria,
            string campo,
            string valorAnterior,
            string valorNuevo)
        {
            if (idAuditoriaCambio <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idAuditoriaCambio));
            }

            if (idAuditoria <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idAuditoria));
            }

            if (string.IsNullOrWhiteSpace(campo))
            {
                throw new ArgumentException(
                    "El campo del cambio es obligatorio.",
                    nameof(campo));
            }

            IdAuditoriaCambio = idAuditoriaCambio;
            IdAuditoria = idAuditoria;
            Campo = campo.Trim();
            ValorAnterior = valorAnterior;
            ValorNuevo = valorNuevo;
        }

        public long IdAuditoriaCambio { get; private set; }

        public long IdAuditoria { get; private set; }

        public string Campo { get; private set; }

        public string ValorAnterior { get; private set; }

        public string ValorNuevo { get; private set; }
    }
}
