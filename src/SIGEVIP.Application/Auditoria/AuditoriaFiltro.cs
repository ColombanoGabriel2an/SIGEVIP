using System;

namespace SIGEVIP.Application.Auditoria
{
    public sealed class AuditoriaFiltro
    {
        public AuditoriaFiltro(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string nombreUsuario,
            string modulo,
            string accion,
            string textoGeneral)
        {
            FechaDesde =
                fechaDesde.HasValue
                    ? fechaDesde.Value.Date
                    : (DateTime?)null;

            FechaHasta =
                fechaHasta.HasValue
                    ? fechaHasta.Value.Date
                    : (DateTime?)null;

            NombreUsuario =
                NormalizarTexto(
                    nombreUsuario);

            Modulo =
                NormalizarTexto(
                    modulo);

            Accion =
                NormalizarTexto(
                    accion);

            TextoGeneral =
                NormalizarTexto(
                    textoGeneral);
        }

        public DateTime? FechaDesde
        {
            get;
            private set;
        }

        public DateTime? FechaHasta
        {
            get;
            private set;
        }

        public string NombreUsuario
        {
            get;
            private set;
        }

        public string Modulo
        {
            get;
            private set;
        }

        public string Accion
        {
            get;
            private set;
        }

        public string TextoGeneral
        {
            get;
            private set;
        }

        public static AuditoriaFiltro CrearSinFiltros()
        {
            return new AuditoriaFiltro(
                null,
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        private static string NormalizarTexto(
            string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}