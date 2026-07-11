using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class SesionActualTests
    {
        [TestMethod]
        public void NuevaSesion_ComienzaSinUsuario()
        {
            SesionActual sesion =
                new SesionActual();

            Assert.IsFalse(
                sesion.HayUsuarioAutenticado);

            Assert.IsNull(
                sesion.UsuarioActual);
        }

        [TestMethod]
        public void Iniciar_ConUsuarioActivo_IniciaSesion()
        {
            SesionActual sesion =
                new SesionActual();

            sesion.Iniciar(CrearUsuario());

            Assert.IsTrue(
                sesion.HayUsuarioAutenticado);
        }

        [TestMethod]
        public void Iniciar_ConUsuarioActivo_ExponeUsuarioActual()
        {
            SesionActual sesion =
                new SesionActual();

            Usuario usuario =
                CrearUsuario();

            sesion.Iniciar(usuario);

            Assert.AreSame(
                usuario,
                sesion.UsuarioActual);
        }

        [TestMethod]
        public void Cerrar_ConSesionIniciada_EliminaUsuarioActual()
        {
            SesionActual sesion =
                new SesionActual();

            sesion.Iniciar(CrearUsuario());
            sesion.Cerrar();

            Assert.IsFalse(
                sesion.HayUsuarioAutenticado);

            Assert.IsNull(
                sesion.UsuarioActual);
        }

        [TestMethod]
        public void Iniciar_ConUsuarioNulo_LanzaExcepcion()
        {
            SesionActual sesion =
                new SesionActual();

            Assert.ThrowsException<ArgumentNullException>(
                () => sesion.Iniciar(null));
        }

        [TestMethod]
        public void Iniciar_ConUsuarioInactivo_LanzaExcepcion()
        {
            SesionActual sesion =
                new SesionActual();

            Usuario usuario =
                CrearUsuario();

            usuario.Desactivar();

            Assert.ThrowsException<InvalidOperationException>(
                () => sesion.Iniciar(usuario));
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
    }
}
