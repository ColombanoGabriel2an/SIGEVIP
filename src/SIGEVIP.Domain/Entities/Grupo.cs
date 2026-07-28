using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Domain.Security;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Grupo : IPermisoComponente
    {
        private readonly List<IPermisoComponente> _componentes;

        public Grupo(
            int idGrupo,
            string codigo,
            string nombre,
            string descripcion)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ReglaNegocioException(
                    "El código del grupo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ReglaNegocioException(
                    "El nombre del grupo es obligatorio.");
            }

            IdGrupo = idGrupo;
            Codigo = NormalizarCodigo(codigo);
            Nombre = nombre.Trim();
            Descripcion = NormalizarTextoOpcional(descripcion);
            Activo = true;

            _componentes =
                new List<IPermisoComponente>();
        }

        public int IdGrupo
        {
            get;
            private set;
        }

        public int IdComponente
        {
            get
            {
                return IdGrupo;
            }
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

        public IReadOnlyCollection<IPermisoComponente>
            Componentes
        {
            get
            {
                return new ReadOnlyCollection
                    <IPermisoComponente>(
                        _componentes);
            }
        }

        public void ActualizarDatos(
            string nombre,
            string descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ReglaNegocioException(
                    "El nombre del grupo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                throw new ReglaNegocioException(
                    "La descripción del grupo es obligatoria.");
            }

            Nombre = nombre.Trim();
            Descripcion = descripcion.Trim();
        }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public void AgregarComponente(
            IPermisoComponente componente)
        {
            if (componente == null)
            {
                throw new ReglaNegocioException(
                    "Debe indicar un componente de seguridad válido.");
            }

            Grupo grupoCandidato =
                componente as Grupo;

            if (grupoCandidato != null)
            {
                ValidarAusenciaDeCiclo(
                    grupoCandidato);
            }

            if (_componentes.Any(
                existente =>
                    SonElMismoComponente(
                        existente,
                        componente)))
            {
                throw new ReglaNegocioException(
                    "El componente ya pertenece al grupo.");
            }

            _componentes.Add(
                componente);
        }

        public void ReemplazarPermisosDirectos(
            IEnumerable<Permiso> permisos)
        {
            if (permisos == null)
            {
                throw new ReglaNegocioException(
                    "Debe asignar al menos un permiso al grupo.");
            }

            List<Permiso> permisosNuevos =
                permisos.ToList();

            if (permisosNuevos.Count == 0)
            {
                throw new ReglaNegocioException(
                    "Debe asignar al menos un permiso al grupo.");
            }

            foreach (Permiso permiso in permisosNuevos)
            {
                if (permiso == null)
                {
                    throw new ReglaNegocioException(
                        "Los permisos asignados contienen un elemento inválido.");
                }
            }

            ValidarComponentesDuplicados(
                permisosNuevos
                    .Cast<IPermisoComponente>()
                    .ToList(),
                "No se pueden asignar permisos duplicados al grupo.");

            List<IPermisoComponente> gruposHijos =
                _componentes
                    .Where(
                        componente =>
                            componente is Grupo)
                    .ToList();

            _componentes.Clear();

            _componentes.AddRange(
                gruposHijos);

            foreach (Permiso permiso in permisosNuevos)
            {
                _componentes.Add(
                    permiso);
            }
        }

        public void ReemplazarGruposHijos(
            IEnumerable<Grupo> gruposHijos)
        {
            if (gruposHijos == null)
            {
                throw new ReglaNegocioException(
                    "La colección de grupos hijos es obligatoria.");
            }

            List<Grupo> gruposNuevos =
                gruposHijos.ToList();

            foreach (Grupo grupoHijo in gruposNuevos)
            {
                if (grupoHijo == null)
                {
                    throw new ReglaNegocioException(
                        "Los grupos hijos contienen un elemento inválido.");
                }
            }

            ValidarComponentesDuplicados(
                gruposNuevos
                    .Cast<IPermisoComponente>()
                    .ToList(),
                "No se pueden asignar grupos hijos duplicados.");

            foreach (Grupo grupoHijo in gruposNuevos)
            {
                ValidarAusenciaDeCiclo(
                    grupoHijo);
            }

            List<IPermisoComponente> permisosDirectos =
                _componentes
                    .Where(
                        componente =>
                            componente is Permiso)
                    .ToList();

            _componentes.Clear();

            _componentes.AddRange(
                permisosDirectos);

            foreach (Grupo grupoHijo in gruposNuevos)
            {
                _componentes.Add(
                    grupoHijo);
            }
        }

        public IReadOnlyCollection<Permiso>
            ObtenerPermisosEfectivos()
        {
            Dictionary<string, Permiso> permisos =
                new Dictionary<string, Permiso>(
                    StringComparer.OrdinalIgnoreCase);

            if (!Activo)
            {
                return new ReadOnlyCollection<Permiso>(
                    permisos.Values.ToList());
            }

            foreach (
                IPermisoComponente componente
                in _componentes)
            {
                if (!componente.Activo)
                {
                    continue;
                }

                foreach (
                    Permiso permiso
                    in componente
                        .ObtenerPermisosEfectivos())
                {
                    if (!permiso.Activo)
                    {
                        continue;
                    }

                    string codigo =
                        Permiso.NormalizarCodigo(
                            permiso.Codigo);

                    if (!permisos.ContainsKey(
                        codigo))
                    {
                        permisos.Add(
                            codigo,
                            permiso);
                    }
                }
            }

            return new ReadOnlyCollection<Permiso>(
                permisos.Values.ToList());
        }

        internal static string NormalizarCodigo(
            string codigo)
        {
            return string.IsNullOrWhiteSpace(codigo)
                ? string.Empty
                : codigo
                    .Trim()
                    .ToUpperInvariant();
        }

        private void ValidarAusenciaDeCiclo(
            Grupo grupoCandidato)
        {
            if (ReferenceEquals(
                this,
                grupoCandidato))
            {
                throw new ReglaNegocioException(
                    "Un grupo no puede agregarse a sí mismo.");
            }

            HashSet<Grupo> visitados =
                new HashSet<Grupo>();

            if (grupoCandidato.ContieneGrupo(
                this,
                visitados))
            {
                throw new ReglaNegocioException(
                    "La asociación produciría un ciclo entre grupos.");
            }
        }

        private bool ContieneGrupo(
            Grupo grupoBuscado,
            HashSet<Grupo> visitados)
        {
            if (!visitados.Add(
                this))
            {
                return false;
            }

            if (ReferenceEquals(
                this,
                grupoBuscado))
            {
                return true;
            }

            foreach (
                Grupo grupoHijo
                in _componentes.OfType<Grupo>())
            {
                if (grupoHijo.ContieneGrupo(
                    grupoBuscado,
                    visitados))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidarComponentesDuplicados(
            IList<IPermisoComponente> componentes,
            string mensaje)
        {
            for (
                int indiceActual = 0;
                indiceActual < componentes.Count;
                indiceActual++)
            {
                for (
                    int indiceCandidato =
                        indiceActual + 1;
                    indiceCandidato <
                        componentes.Count;
                    indiceCandidato++)
                {
                    if (SonElMismoComponente(
                        componentes[indiceActual],
                        componentes[indiceCandidato]))
                    {
                        throw new ReglaNegocioException(
                            mensaje);
                    }
                }
            }
        }

        private static bool SonElMismoComponente(
            IPermisoComponente existente,
            IPermisoComponente candidato)
        {
            if (ReferenceEquals(
                existente,
                candidato))
            {
                return true;
            }

            if (existente.GetType() !=
                candidato.GetType())
            {
                return false;
            }

            if (existente.IdComponente > 0 &&
                candidato.IdComponente > 0 &&
                existente.IdComponente ==
                    candidato.IdComponente)
            {
                return true;
            }

            return string.Equals(
                NormalizarCodigo(
                    existente.Codigo),
                NormalizarCodigo(
                    candidato.Codigo),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizarTextoOpcional(
            string valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}
