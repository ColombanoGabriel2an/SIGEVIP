using System;
using System.Collections.Generic;
using System.Linq;

namespace SIGEVIP.Application.Rendiciones
{
    public sealed class RendicionVisitaDto
    {
        public RendicionVisitaDto(
            int idVisita,
            DateTime fecha,
            string observacion,
            string localidadEncuentro,
            IEnumerable<RendicionClienteDto> clientes)
        {
            IdVisita = idVisita;
            Fecha = fecha.Date;
            Observacion =
                observacion ?? string.Empty;
            LocalidadEncuentro =
                localidadEncuentro ?? string.Empty;

            Clientes =
                (clientes ??
                    Enumerable.Empty<RendicionClienteDto>())
                .ToList()
                .AsReadOnly();
        }

        public int IdVisita { get; private set; }

        public DateTime Fecha { get; private set; }

        public string Observacion
        {
            get;
            private set;
        }

        public string LocalidadEncuentro
        {
            get;
            private set;
        }

        public IReadOnlyCollection<RendicionClienteDto>
            Clientes
        {
            get;
            private set;
        }
    }
}