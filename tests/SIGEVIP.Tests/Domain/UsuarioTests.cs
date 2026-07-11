using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class UsuarioTests
    {
        [TestMethod]
        public void CrearUsuario_ConDatosValidos_NaceActivo()
        {
            Usuario usuario = CrearUsuario();

            Assert.IsTrue(usuario.Activo);
        }

        [TestMethod]
        public void CrearUsuario_ConPersonaInvalida_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Usuario(
                        1,
                        0,
                        "aperez",
                        CrearHash(),
                        CrearSalt(),
                        100000));

            StringAssert.Contains(
                excepcion.Message,
                "persona válida");
        }

        [TestMethod]
        public void CrearUsuario_ConNombreUsuarioVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Usuario(
                        1,
                        1,
                        " ",
                        CrearHash(),
                        CrearSalt(),
                        100000));

            StringAssert.Contains(
                excepcion.Message,
                "nombre de usuario");
        }

        [TestMethod]
        public void CrearUsuario_NormalizaNombreUsuario()
        {
            Usuario usuario = new Usuario(
                1,
                1,
                "  APerez  ",
                CrearHash(),
                CrearSalt(),
                100000);

            Assert.AreEqual(
                "aperez",
                usuario.NombreUsuario);
        }

        [TestMethod]
        public void CrearUsuario_ConHashNulo_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Usuario(
                        1,
                        1,
                        "aperez",
                        null,
                        CrearSalt(),
                        100000));

            StringAssert.Contains(
                excepcion.Message,
                "hash");
        }

        [TestMethod]
        public void CrearUsuario_ConSaltVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Usuario(
                        1,
                        1,
                        "aperez",
                        CrearHash(),
                        new byte[0],
                        100000));

            StringAssert.Contains(
                excepcion.Message,
                "salt");
        }

        [TestMethod]
        public void CrearUsuario_ConIteracionesInvalidas_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Usuario(
                        1,
                        1,
                        "aperez",
                        CrearHash(),
                        CrearSalt(),
                        0));

            StringAssert.Contains(
                excepcion.Message,
                "iteraciones");
        }

        [TestMethod]
        public void DesactivarUsuario_CambiaEstadoAInactivo()
        {
            Usuario usuario = CrearUsuario();

            usuario.Desactivar();

            Assert.IsFalse(usuario.Activo);
        }

        [TestMethod]
        public void ActivarUsuario_DespuesDeDesactivarlo_CambiaEstadoAActivo()
        {
            Usuario usuario = CrearUsuario();
            usuario.Desactivar();

            usuario.Activar();

            Assert.IsTrue(usuario.Activo);
        }

        [TestMethod]
        public void PasswordHash_NoPermiteModificarElValorInterno()
        {
            Usuario usuario = CrearUsuario();

            byte[] hashExpuesto = usuario.PasswordHash;
            hashExpuesto[0] = 255;

            Assert.AreNotEqual(
                255,
                usuario.PasswordHash[0]);
        }

        [TestMethod]
        public void PasswordSalt_NoPermiteModificarElValorInterno()
        {
            Usuario usuario = CrearUsuario();

            byte[] saltExpuesto = usuario.PasswordSalt;
            saltExpuesto[0] = 255;

            Assert.AreNotEqual(
                255,
                usuario.PasswordSalt[0]);
        }

        private static Usuario CrearUsuario()
        {
            return new Usuario(
                1,
                1,
                "aperez",
                CrearHash(),
                CrearSalt(),
                100000);
        }

        private static byte[] CrearHash()
        {
            return new byte[]
            {
                10,
                20,
                30,
                40
            };
        }

        private static byte[] CrearSalt()
        {
            return new byte[]
            {
                50,
                60,
                70,
                80
            };
        }
    }
}
