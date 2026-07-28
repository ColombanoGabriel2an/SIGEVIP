using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIGEVIP.Application.Usuarios
{
    public sealed class ModificarUsuarioCommand
    {
        private readonly List<int> _idsGrupos;

        public ModificarUsuarioCommand(
            int idUsuario,
            string nombreUsuario,
            IEnumerable<int> idsGrupos)
        {
            if (idsGrupos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsGrupos));
            }

            IdUsuario = idUsuario;
            NombreUsuario = nombreUsuario;

            _idsGrupos =
                idsGrupos.ToList();
        }

        public int IdUsuario
        {
            get;
            private set;
        }

        public string NombreUsuario
        {
            get;
            private set;
        }

        public IReadOnlyCollection<int> IdsGrupos
        {
            get
            {
                return new ReadOnlyCollection<int>(
                    _idsGrupos);
            }
        }
    }
}
