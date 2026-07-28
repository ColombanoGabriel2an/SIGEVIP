using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIGEVIP.Application.Grupos
{
    public sealed class ModificarGrupoCommand
    {
        private readonly List<int>
            _idsPermisos;

        private readonly List<int>
            _idsGruposHijos;

        public ModificarGrupoCommand(
            int idGrupo,
            string nombre,
            string descripcion,
            IEnumerable<int> idsPermisos)
            : this(
                idGrupo,
                nombre,
                descripcion,
                idsPermisos,
                Enumerable.Empty<int>(),
                false)
        {
        }

        public ModificarGrupoCommand(
            int idGrupo,
            string nombre,
            string descripcion,
            IEnumerable<int> idsPermisos,
            IEnumerable<int> idsGruposHijos)
            : this(
                idGrupo,
                nombre,
                descripcion,
                idsPermisos,
                idsGruposHijos,
                true)
        {
        }

        private ModificarGrupoCommand(
            int idGrupo,
            string nombre,
            string descripcion,
            IEnumerable<int> idsPermisos,
            IEnumerable<int> idsGruposHijos,
            bool reemplazarGruposHijos)
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

            IdGrupo = idGrupo;
            Nombre = nombre;
            Descripcion = descripcion;
            ReemplazarGruposHijos =
                reemplazarGruposHijos;

            _idsPermisos =
                idsPermisos.ToList();

            _idsGruposHijos =
                idsGruposHijos.ToList();
        }

        public int IdGrupo
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

        public bool ReemplazarGruposHijos
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
