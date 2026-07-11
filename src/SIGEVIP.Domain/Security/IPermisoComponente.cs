using System.Collections.Generic;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Domain.Security
{
    public interface IPermisoComponente
    {
        int IdComponente { get; }

        string Codigo { get; }

        bool Activo { get; }

        IReadOnlyCollection<Permiso> ObtenerPermisosEfectivos();
    }
}
