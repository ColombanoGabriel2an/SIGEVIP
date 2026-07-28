using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class PermisoGestionDomainTests
    {
        [TestMethod]
        public void ActualizarDatos_ConDatosValidos_ActualizaNombreYDescripcion()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.ActualizarDatos(
                " Nuevo nombre ",
                " Nueva descripción ");

            Assert.AreEqual(
                "Nuevo nombre",
                permiso.Nombre);

            Assert.AreEqual(
                "Nueva descripción",
                permiso.Descripcion);
        }

        [TestMethod]
        public void ActualizarDatos_ConNombreVacio_RechazaOperacion()
        {
            Permiso permiso =
                CrearPermiso();

            Assert.ThrowsException<ReglaNegocioException>(
                () => permiso.ActualizarDatos(
                    " ",
                    "Descripción válida"));
        }

        [TestMethod]
        public void ActualizarDatos_ConDescripcionNula_UsaTextoVacio()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.ActualizarDatos(
                "Nombre válido",
                null);

            Assert.AreEqual(
                string.Empty,
                permiso.Descripcion);
        }

        [TestMethod]
        public void ActualizarDatos_ConDescripcionVacia_UsaTextoVacio()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.ActualizarDatos(
                "Nombre válido",
                " ");

            Assert.AreEqual(
                string.Empty,
                permiso.Descripcion);
        }

        [TestMethod]
        public void ActualizarDatos_NoModificaCodigo()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.ActualizarDatos(
                "Nombre modificado",
                "Descripción modificada");

            Assert.AreEqual(
                "CLIENTE_CONSULTAR",
                permiso.Codigo);
        }

        [TestMethod]
        public void ActualizarDatos_NoModificaIdentificador()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.ActualizarDatos(
                "Nombre modificado",
                "Descripción modificada");

            Assert.AreEqual(
                10,
                permiso.IdPermiso);
        }

        [TestMethod]
        public void ActualizarDatos_NoModificaEstado()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.Desactivar();

            permiso.ActualizarDatos(
                "Nombre modificado",
                "Descripción modificada");

            Assert.IsFalse(
                permiso.Activo);
        }

        [TestMethod]
        public void ActualizarDatos_Invalido_ConservaDatosAnteriores()
        {
            Permiso permiso =
                CrearPermiso();

            Assert.ThrowsException<ReglaNegocioException>(
                () => permiso.ActualizarDatos(
                    " ",
                    "Descripción modificada"));

            Assert.AreEqual(
                "Consultar clientes",
                permiso.Nombre);

            Assert.AreEqual(
                "Permite consultar clientes.",
                permiso.Descripcion);
        }

        [TestMethod]
        public void Activar_PermisoInactivo_CambiaEstado()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.Desactivar();
            permiso.Activar();

            Assert.IsTrue(
                permiso.Activo);
        }

        [TestMethod]
        public void Desactivar_PermisoActivo_CambiaEstado()
        {
            Permiso permiso =
                CrearPermiso();

            permiso.Desactivar();

            Assert.IsFalse(
                permiso.Activo);
        }

        private static Permiso CrearPermiso()
        {
            return new Permiso(
                10,
                " cliente_consultar ",
                "Consultar clientes",
                "Permite consultar clientes.");
        }
    }
}