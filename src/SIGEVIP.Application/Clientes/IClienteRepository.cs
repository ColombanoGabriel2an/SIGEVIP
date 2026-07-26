using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Clientes
{
    public interface IClienteRepository
    {
        Cliente ObtenerPorId(
            int idCliente);

        bool ExisteCuit(
            string cuit,
            int? idClienteExcluido);

        IReadOnlyCollection<ClienteListadoDto> Listar(
            ClienteFiltro filtro);

        int Insertar(
            Cliente cliente);

        void Actualizar(
            Cliente cliente);

        void Activar(
            int idCliente);

        void Desactivar(
            int idCliente);
    }
}
