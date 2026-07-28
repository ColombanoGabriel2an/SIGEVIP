using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIGEVIP.Application.Grupos
{
    public sealed class RegistrarGrupoCommand
    {
        private readonly List<int>
            _idsPermisos;

        public RegistrarGrupoCommand(
            string nombre,
            string descripcion,
            IEnumerable<int> idsPermisos)
        {
            if (idsPermisos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsPermisos));
            }

            Nombre = nombre;
            Descripcion = descripcion;

            _idsPermisos =
                idsPermisos.ToList();
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

        public IReadOnlyCollection<int>
            IdsPermisos
        {
            get
            {
                return new ReadOnlyCollection<int>(
                    _idsPermisos);
            }
        }
    }
}
