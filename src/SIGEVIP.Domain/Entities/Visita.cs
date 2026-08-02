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
            ValidarDatosGenerales(
                fecha,
                observacion,
                localidadEncuentro);

            IdVisita = idVisita;
            Fecha = fecha.Date;
            Observacion = observacion.Trim();
            LocalidadEncuentro =
                localidadEncuentro.Trim();

            _clientes =
                new List<Cliente>();
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
                return new ReadOnlyCollection<Cliente>(
                    _clientes);
            }
        }

        public bool TieneClientes
        {
            get
            {
                return _clientes.Count > 0;
            }
        }

        public static Visita Reconstruir(
            int idVisita,
            int idViaje,
            DateTime fecha,
            string observacion,
            string localidadEncuentro,
            IEnumerable<Cliente> clientes)
        {
            if (idVisita <= 0)
            {
                throw new ReglaNegocioException(
                    "El identificador persistido de la visita debe ser válido.");
            }

            if (idViaje <= 0)
            {
                throw new ReglaNegocioException(
                    "El identificador persistido del viaje debe ser válido.");
            }

            var visita =
                new Visita(
                    idVisita,
                    fecha,
                    observacion,
                    localidadEncuentro);

            visita.CargarClientes(
                clientes,
                true);

            visita.AsociarAViaje(
                idViaje);

            return visita;
        }

        public void Modificar(
            DateTime fecha,
            string observacion,
            string localidadEncuentro,
            IEnumerable<Cliente> clientes)
        {
            ValidarDatosGenerales(
                fecha,
                observacion,
                localidadEncuentro);

            List<Cliente> clientesValidados =
                ValidarYMaterializarClientes(
                    clientes,
                    true);

            Fecha = fecha.Date;
            Observacion = observacion.Trim();
            LocalidadEncuentro =
                localidadEncuentro.Trim();

            _clientes.Clear();
            _clientes.AddRange(
                clientesValidados);
        }

        public void AgregarCliente(
            Cliente cliente)
        {
            ValidarCliente(
                cliente);

            if (_clientes.Any(
                existente =>
                    SonElMismoCliente(
                        existente,
                        cliente)))
            {
                throw new ReglaNegocioException(
                    "El cliente ya se encuentra asociado a la visita.");
            }

            _clientes.Add(
                cliente);
        }

        internal void AsociarAViaje(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un viaje válido.");
            }

            if (IdViaje != 0 &&
                IdViaje != idViaje)
            {
                throw new ReglaNegocioException(
                    "La visita ya pertenece a otro viaje.");
            }

            IdViaje = idViaje;
        }

        private void CargarClientes(
            IEnumerable<Cliente> clientes,
            bool exigirAlMenosUno)
        {
            List<Cliente> materializados =
                ValidarYMaterializarClientes(
                    clientes,
                    exigirAlMenosUno);

            _clientes.Clear();
            _clientes.AddRange(
                materializados);
        }

        private static List<Cliente>
            ValidarYMaterializarClientes(
                IEnumerable<Cliente> clientes,
                bool exigirAlMenosUno)
        {
            if (clientes == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar los clientes de la visita.");
            }

            List<Cliente> materializados =
                clientes.ToList();

            if (exigirAlMenosUno &&
                materializados.Count == 0)
            {
                throw new ReglaNegocioException(
                    "La visita debe tener al menos un cliente asociado.");
            }

            var resultado =
                new List<Cliente>();

            foreach (
                Cliente cliente
                in materializados)
            {
                ValidarCliente(
                    cliente);

                if (resultado.Any(
                    existente =>
                        SonElMismoCliente(
                            existente,
                            cliente)))
                {
                    throw new ReglaNegocioException(
                        "No se permiten clientes duplicados en la visita.");
                }

                resultado.Add(
                    cliente);
            }

            return resultado;
        }

        private static void ValidarDatosGenerales(
            DateTime fecha,
            string observacion,
            string localidadEncuentro)
        {
            if (string.IsNullOrWhiteSpace(
                observacion))
            {
                throw new ReglaNegocioException(
                    "La observación de la visita es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(
                localidadEncuentro))
            {
                throw new ReglaNegocioException(
                    "La localidad del encuentro es obligatoria.");
            }

            if (fecha == DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La fecha de la visita debe ser válida.");
            }
        }

        private static void ValidarCliente(
            Cliente cliente)
        {
            if (cliente == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un cliente válido.");
            }
        }

        private static bool SonElMismoCliente(
            Cliente existente,
            Cliente candidato)
        {
            if (ReferenceEquals(
                existente,
                candidato))
            {
                return true;
            }

            if (existente.IdCliente > 0 &&
                candidato.IdCliente > 0 &&
                existente.IdCliente ==
                    candidato.IdCliente)
            {
                return true;
            }

            string cuitExistente =
                Cliente.NormalizarCuit(
                    existente.Cuit);

            string cuitCandidato =
                Cliente.NormalizarCuit(
                    candidato.Cuit);

            return
                !string.IsNullOrWhiteSpace(
                    cuitExistente)
                &&
                string.Equals(
                    cuitExistente,
                    cuitCandidato,
                    StringComparison.OrdinalIgnoreCase);
        }
    }
}