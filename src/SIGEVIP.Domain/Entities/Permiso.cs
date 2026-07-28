using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Domain.Security;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Permiso : IPermisoComponente
    {
        public Permiso(
            int idPermiso,
            string codigo,
            string nombre,
            string descripcion)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ReglaNegocioException(
                    "El código del permiso es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ReglaNegocioException(
                    "El nombre del permiso es obligatorio.");
            }

            IdPermiso = idPermiso;
            Codigo = NormalizarCodigo(codigo);
            Nombre = nombre.Trim();
            Descripcion =
                NormalizarTextoOpcional(
                    descripcion);
            Activo = true;
        }

        public int IdPermiso
        {
            get;
            private set;
        }

        public int IdComponente
        {
            get
            {
                return IdPermiso;
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

        public void ActualizarDatos(
            string nombre,
            string descripcion)
        {
            if (string.IsNullOrWhiteSpace(
                nombre))
            {
                throw new ReglaNegocioException(
                    "El nombre del permiso es obligatorio.");
            }

            Nombre =
                nombre.Trim();

            Descripcion =
                NormalizarTextoOpcional(
                    descripcion);
        }

        public void Activar()
        {
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public IReadOnlyCollection<Permiso>
            ObtenerPermisosEfectivos()
        {
            List<Permiso> permisos =
                new List<Permiso>();

            if (Activo)
            {
                permisos.Add(
                    this);
            }

            return new ReadOnlyCollection<Permiso>(
                permisos);
        }

        internal static string NormalizarCodigo(
            string codigo)
        {
            return string.IsNullOrWhiteSpace(
                codigo)
                ? string.Empty
                : codigo
                    .Trim()
                    .ToUpperInvariant();
        }

        private static string
            NormalizarTextoOpcional(
                string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                ? string.Empty
                : valor.Trim();
        }
    }
}