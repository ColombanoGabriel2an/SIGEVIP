using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIGEVIP.Application.Usuarios
{
    public sealed class UsuarioDetalleDto
    {
        private readonly List<int> _idsGrupos;

        public UsuarioDetalleDto(
            int idUsuario,
            int idPersona,
            string nombreUsuario,
            string nombreCompleto,
            string email,
            bool activo,
            IEnumerable<int> idsGrupos)
        {
            if (idsGrupos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsGrupos));
            }

            IdUsuario = idUsuario;
            IdPersona = idPersona;
            NombreUsuario = nombreUsuario;
            NombreCompleto = nombreCompleto;
            Email = email;
            Activo = activo;

            _idsGrupos =
                idsGrupos
                    .Distinct()
                    .ToList();
        }

        public int IdUsuario
        {
            get;
            private set;
        }

        public int IdPersona
        {
            get;
            private set;
        }

        public string NombreUsuario
        {
            get;
            private set;
        }

        public string NombreCompleto
        {
            get;
            private set;
        }

        public string Email
        {
            get;
            private set;
        }

        public bool Activo
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
