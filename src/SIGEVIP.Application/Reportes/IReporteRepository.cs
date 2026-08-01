using System.Collections.Generic;

namespace SIGEVIP.Application.Reportes
{
    public interface IReporteRepository
    {
        IReadOnlyCollection<ReporteViajeSeleccionDto>
            ListarViajes();

        IReadOnlyCollection<ReportePersonaSeleccionDto>
            ListarPersonasPagadoras();

        ReporteViajeDatosDto ObtenerDatosViaje(
            int idViaje);

        IReadOnlyCollection<ReporteViaticoFilaDto>
            ListarViaticos(
                ReporteViaticoFiltro filtro);

        IReadOnlyCollection<ReporteAnalisisItemDto>
            ConsultarAnalisis(
                ReporteAnalisisFiltro filtro);
    }
}
