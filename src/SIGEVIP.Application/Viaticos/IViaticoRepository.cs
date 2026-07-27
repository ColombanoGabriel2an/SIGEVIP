using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Application.Viaticos
{
    public interface IViaticoRepository
    {
        Viatico ObtenerPorId(
            int idViatico);

        IReadOnlyCollection<ViaticoListadoDto>
            ListarPorViaje(
                int idViaje,
                ViaticoFiltro filtro);

        int Insertar(
            Viatico viatico);

        void Actualizar(
            Viatico viatico);
    }
}