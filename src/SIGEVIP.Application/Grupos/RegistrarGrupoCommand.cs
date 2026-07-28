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

        private readonly List<int>
            _idsGruposHijos;

        public RegistrarGrupoCommand(
            string nombre,
            string descripcion,
            IEnumerable<int> idsPermisos)
            : this(
                nombre,
                descripcion,
                idsPermisos,
                Enumerable.Empty<int>())
        {
        }

        public RegistrarGrupoCommand(
            string nombre,
            string descripcion,
            IEnumerable<int> idsPermisos,
            IEnumerable<int> idsGruposHijos)
        {
            if (idsPermisos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsPermisos));
            }

            if (idsGruposHijos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsGruposHijos));
            }

            Nombre = nombre;
            Descripcion = descripcion;

            _idsPermisos =
                idsPermisos.ToList();

            _idsGruposHijos =
                idsGruposHijos.ToList();
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

        public IReadOnlyCollection<int>
            IdsGruposHijos
        {
            get
            {
                return new ReadOnlyCollection<int>(
                    _idsGruposHijos);
            }
        }
    }
}
