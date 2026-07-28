using System.Collections.Generic;
using SIGEVIP.Application.Auditoria;
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
            Cliente cliente,
            AuditoriaRegistro auditoria);

        void Actualizar(
            Cliente cliente,
            AuditoriaRegistro auditoria);

        void Activar(
            int idCliente,
            AuditoriaRegistro auditoria);

        void Desactivar(
            int idCliente,
            AuditoriaRegistro auditoria);
    }
}