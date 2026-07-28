using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIGEVIP.Application.Usuarios
{
    public sealed class RegistrarUsuarioCommand
    {
        private readonly List<int> _idsGrupos;

        public RegistrarUsuarioCommand(
            int idPersona,
            string nombreUsuario,
            string password,
            string confirmacionPassword,
            IEnumerable<int> idsGrupos)
        {
            if (idsGrupos == null)
            {
                throw new ArgumentNullException(
                    nameof(idsGrupos));
            }

            IdPersona = idPersona;
            NombreUsuario = nombreUsuario;
            Password = password;
            ConfirmacionPassword = confirmacionPassword;

            _idsGrupos =
                idsGrupos.ToList();
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

        public string Password
        {
            get;
            private set;
        }

        public string ConfirmacionPassword
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
