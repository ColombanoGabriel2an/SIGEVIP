using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Visita
    {
        private readonly List<Cliente> _clientes;

        public Visita(
            int idVisita,
            DateTime fecha,
            string observacion,
            string localidadEncuentro)
        {
            if (string.IsNullOrWhiteSpace(observacion))
            {
                throw new ReglaNegocioException(
                    "La observación de la visita es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(localidadEncuentro))
            {
                throw new ReglaNegocioException(
                    "La localidad del encuentro es obligatoria.");
            }

            IdVisita = idVisita;
            Fecha = fecha.Date;
            Observacion = observacion.Trim();
            LocalidadEncuentro = localidadEncuentro.Trim();

            _clientes = new List<Cliente>();
        }

        public int IdVisita { get; private set; }

        public int IdViaje { get; private set; }

        public DateTime Fecha { get; private set; }

        public string Observacion { get; private set; }

        public string LocalidadEncuentro { get; private set; }

        public IReadOnlyCollection<Cliente> Clientes
        {
            get
            {
                return new ReadOnlyCollection<Cliente>(_clientes);
            }
        }

        public bool TieneClientes
        {
            get { return _clientes.Count > 0; }
        }

        public void AgregarCliente(Cliente cliente)
        {
            if (cliente == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un cliente válido.");
            }

            if (_clientes.Any(item => SonElMismoCliente(item, cliente)))
            {
                throw new ReglaNegocioException(
                    "El cliente ya se encuentra asociado a la visita.");
            }

            _clientes.Add(cliente);
        }

        internal void AsociarAViaje(int idViaje)
        {
            if (IdViaje != 0 && IdViaje != idViaje)
            {
                throw new ReglaNegocioException(
                    "La visita ya pertenece a otro viaje.");
            }

            IdViaje = idViaje;
        }

        private static bool SonElMismoCliente(
            Cliente existente,
            Cliente candidato)
        {
            if (ReferenceEquals(existente, candidato))
            {
                return true;
            }

            if (existente.IdCliente > 0 &&
                candidato.IdCliente > 0 &&
                existente.IdCliente == candidato.IdCliente)
            {
                return true;
            }

            string cuitExistente =
                Cliente.NormalizarCuit(existente.Cuit);

            string cuitCandidato =
                Cliente.NormalizarCuit(candidato.Cuit);

            return !string.IsNullOrWhiteSpace(cuitExistente) &&
                   string.Equals(
                       cuitExistente,
                       cuitCandidato,
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
