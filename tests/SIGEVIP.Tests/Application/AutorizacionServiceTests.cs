using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class AutorizacionServiceTests
    {
        [TestMethod]
        public void TienePermiso_ConUsuarioNulo_DevuelveFalse()
        {
            AutorizacionService servicio =
                new AutorizacionService();

            bool autorizado =
                servicio.TienePermiso(
                    null,
                    "VIAJE_CREAR");

            Assert.IsFalse(autorizado);
        }

        [TestMethod]
        public void TienePermiso_ConUsuarioInactivo_DevuelveFalse()
        {
            Usuario usuario = CrearUsuario();
            usuario.Desactivar();

            AutorizacionService servicio =
                new AutorizacionService();

            bool autorizado =
                servicio.TienePermiso(
                    usuario,
                    "VIAJE_CREAR");

            Assert.IsFalse(autorizado);
        }

        [TestMethod]
        public void TienePermiso_ConPermisoDirectoEnGrupo_DevuelveTrue()
        {
            Usuario usuario = CrearUsuario();
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            grupo.AgregarComponente(
                CrearPermiso(1, "VIAJE_CREAR"));

            usuario.AgregarGrupo(grupo);

            AutorizacionService servicio =
                new AutorizacionService();

            bool autorizado =
                servicio.TienePermiso(
                    usuario,
                    " viaje_crear ");

            Assert.IsTrue(autorizado);
        }

        [TestMethod]
        public void TienePermiso_ConPermisoEnGrupoAnidado_DevuelveTrue()
        {
            Usuario usuario = CrearUsuario();

            Grupo gerente =
                CrearGrupo(1, "GERENTE");

            Grupo comercial =
                CrearGrupo(2, "COMERCIAL");

            comercial.AgregarComponente(
                CrearPermiso(1, "VISITA_REGISTRAR"));

            gerente.AgregarComponente(comercial);
            usuario.AgregarGrupo(gerente);

            AutorizacionService servicio =
                new AutorizacionService();

            bool autorizado =
                servicio.TienePermiso(
                    usuario,
                    "VISITA_REGISTRAR");

            Assert.IsTrue(autorizado);
        }

        [TestMethod]
        public void TienePermiso_SinPermisoAsignado_DevuelveFalse()
        {
            Usuario usuario = CrearUsuario();

            usuario.AgregarGrupo(
                CrearGrupo(1, "COMERCIAL"));

            AutorizacionService servicio =
                new AutorizacionService();

            bool autorizado =
                servicio.TienePermiso(
                    usuario,
                    "VIAJE_APROBAR");

            Assert.IsFalse(autorizado);
        }

        [TestMethod]
        public void TienePermiso_ConCodigoVacio_LanzaExcepcion()
        {
            AutorizacionService servicio =
                new AutorizacionService();

            Assert.ThrowsException<ArgumentException>(
                () => servicio.TienePermiso(
                    CrearUsuario(),
                    " "));
        }

        private static Usuario CrearUsuario()
        {
            return new Usuario(
                1,
                1,
                "aperez",
                new byte[] { 10, 20, 30, 40 },
                new byte[] { 50, 60, 70, 80 },
                100000);
        }

        private static Grupo CrearGrupo(
            int idGrupo,
            string codigo)
        {
            return new Grupo(
                idGrupo,
                codigo,
                "Grupo " + codigo,
                string.Empty);
        }

        private static Permiso CrearPermiso(
            int idPermiso,
            string codigo)
        {
            return new Permiso(
                idPermiso,
                codigo,
                "Permiso " + codigo,
                string.Empty);
        }
    }
}
