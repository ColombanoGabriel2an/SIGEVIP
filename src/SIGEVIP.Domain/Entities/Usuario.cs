using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Usuario
    {
        private readonly byte[] _passwordHash;
        private readonly byte[] _passwordSalt;
        private readonly List<Grupo> _grupos;

        public Usuario(
            int idUsuario,
            int idPersona,
            string nombreUsuario,
            byte[] passwordHash,
            byte[] passwordSalt,
            int iteracionesPassword)
        {
            if (idPersona <= 0)
            {
                throw new ReglaNegocioException(
                    "El usuario debe estar asociado a una persona válida.");
            }

            ValidarNombreUsuario(
                nombreUsuario);

            if (passwordHash == null ||
                passwordHash.Length == 0)
            {
                throw new ReglaNegocioException(
                    "El hash de contraseña es obligatorio.");
            }

            if (passwordSalt == null ||
                passwordSalt.Length == 0)
            {
                throw new ReglaNegocioException(
                    "El salt de contraseña es obligatorio.");
            }

            if (iteracionesPassword <= 0)
            {
                throw new ReglaNegocioException(
                    "La cantidad de iteraciones de contraseña debe ser mayor que cero.");
            }

            IdUsuario = idUsuario;
            IdPersona = idPersona;
            NombreUsuario =
                NormalizarNombreUsuario(
                    nombreUsuario);

            IteracionesPassword =
                iteracionesPassword;

            Activo = true;

            _passwordHash =
                CopiarArreglo(
                    passwordHash);

            _passwordSalt =
                CopiarArreglo(
                    passwordSalt);

            _grupos =
                new List<Grupo>();
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

        public int IteracionesPassword
        {
            get;
            private set;
        }

        public bool Activo
        {
            get;
            private set;
        }

        public byte[] PasswordHash
        {
            get
            {
                return CopiarArreglo(
                    _passwordHash);
            }
        }

        public byte[] PasswordSalt
        {
            get
            {
                return CopiarArreglo(
                    _passwordSalt);
            }
        }

        public IReadOnlyCollection<Grupo> Grupos
        {
            get
            {
                return new ReadOnlyCollection<Grupo>(
                    _grupos);
            }
        }

        public void ActualizarNombreUsuario(
            string nombreUsuario)
        {
            ValidarNombreUsuario(
                nombreUsuario);

            NombreUsuario =
                NormalizarNombreUsuario(
                    nombreUsuario);
        }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public void AgregarGrupo(
            Grupo grupo)
        {
            ValidarGrupo(
                grupo);

            if (_grupos.Any(
                existente =>
                    SonElMismoGrupo(
                        existente,
                        grupo)))
            {
                throw new ReglaNegocioException(
                    "El grupo ya se encuentra asignado al usuario.");
            }

            _grupos.Add(
                grupo);
        }

        public void ReemplazarGrupos(
            IEnumerable<Grupo> grupos)
        {
            if (grupos == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar los grupos asignados al usuario.");
            }

            List<Grupo> gruposValidados =
                new List<Grupo>();

            foreach (Grupo grupo in grupos)
            {
                ValidarGrupo(
                    grupo);

                if (gruposValidados.Any(
                    existente =>
                        SonElMismoGrupo(
                            existente,
                            grupo)))
                {
                    throw new ReglaNegocioException(
                        "No se pueden asignar grupos duplicados al usuario.");
                }

                gruposValidados.Add(
                    grupo);
            }

            if (gruposValidados.Count == 0)
            {
                throw new ReglaNegocioException(
                    "El usuario debe pertenecer al menos a un grupo.");
            }

            _grupos.Clear();

            _grupos.AddRange(
                gruposValidados);
        }

        internal static string NormalizarNombreUsuario(
            string nombreUsuario)
        {
            return string.IsNullOrWhiteSpace(
                nombreUsuario)
                ? string.Empty
                : nombreUsuario
                    .Trim()
                    .ToLowerInvariant();
        }

        private static void ValidarNombreUsuario(
            string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(
                nombreUsuario))
            {
                throw new ReglaNegocioException(
                    "El nombre de usuario es obligatorio.");
            }
        }

        private static void ValidarGrupo(
            Grupo grupo)
        {
            if (grupo == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un grupo válido.");
            }
        }

        private static bool SonElMismoGrupo(
            Grupo existente,
            Grupo candidato)
        {
            if (ReferenceEquals(
                existente,
                candidato))
            {
                return true;
            }

            if (existente.IdGrupo > 0 &&
                candidato.IdGrupo > 0 &&
                existente.IdGrupo ==
                    candidato.IdGrupo)
            {
                return true;
            }

            return string.Equals(
                Grupo.NormalizarCodigo(
                    existente.Codigo),
                Grupo.NormalizarCodigo(
                    candidato.Codigo),
                StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] CopiarArreglo(
            byte[] origen)
        {
            byte[] copia =
                new byte[origen.Length];

            Array.Copy(
                origen,
                copia,
                origen.Length);

            return copia;
        }
    }
}
