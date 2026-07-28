using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIGEVIP.Application.Grupos
{
    public sealed class GrupoDetalleDto
    {
        private readonly List<int>
            _idsPermisosDirectos;

        public GrupoDetalleDto(
            int idGrupo,
            string codigo,
            string nombre,
            string descripcion,
            bool activo,
            IEnumerable<int> idsPermisosDirectos)
        {
            if (idsPermisosDirectos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsPermisosDirectos));
            }

            IdGrupo = idGrupo;
            Codigo = codigo;
            Nombre = nombre;
            Descripcion = descripcion;
            Activo = activo;

            _idsPermisosDirectos =
                idsPermisosDirectos
                    .Distinct()
                    .ToList();
        }

        public int IdGrupo
        {
            get;
            private set;
        }

        public string Codigo
        {
            get;
            private set;
        }

        public string Nombre
        {
            get;
            private set;
        }

        public string Descripcion
        {
            get;
            private set;
        }

        public bool Activo
        {
            get;
            private set;
        }

        public IReadOnlyCollection<int>
            IdsPermisosDirectos
        {
            get
            {
                return new ReadOnlyCollection<int>(
                    _idsPermisosDirectos);
            }
        }
    }
}
