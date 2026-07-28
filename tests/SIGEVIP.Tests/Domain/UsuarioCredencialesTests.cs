using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class UsuarioCredencialesTests
    {
        [TestMethod]
        public void ActualizarCredenciales_DatosValidos_ReemplazaCredenciales()
        {
            Usuario usuario =
                CrearUsuario();

            byte[] nuevoHash =
                new byte[] { 10, 20, 30, 40 };

            byte[] nuevoSalt =
                new byte[] { 50, 60, 70, 80 };

            usuario.ActualizarCredenciales(
                nuevoHash,
                nuevoSalt,
                200000);

            CollectionAssert.AreEqual(
                nuevoHash,
                usuario.PasswordHash);

            CollectionAssert.AreEqual(
                nuevoSalt,
                usuario.PasswordSalt);

            Assert.AreEqual(
                200000,
                usuario.IteracionesPassword);
        }

        [TestMethod]
        public void ActualizarCredenciales_CopiaLosArreglosRecibidos()
        {
            Usuario usuario =
                CrearUsuario();

            byte[] nuevoHash =
                new byte[] { 10, 20, 30, 40 };

            byte[] nuevoSalt =
                new byte[] { 50, 60, 70, 80 };

            usuario.ActualizarCredenciales(
                nuevoHash,
                nuevoSalt,
                200000);

            nuevoHash[0] = 99;
            nuevoSalt[0] = 99;

            Assert.AreEqual(
                10,
                usuario.PasswordHash[0]);

            Assert.AreEqual(
                50,
                usuario.PasswordSalt[0]);
        }

        [TestMethod]
        public void ActualizarCredenciales_HashVacio_RechazaYConservaEstado()
        {
            Usuario usuario =
                CrearUsuario();

            byte[] hashAnterior =
                usuario.PasswordHash;

            byte[] saltAnterior =
                usuario.PasswordSalt;

            int iteracionesAnteriores =
                usuario.IteracionesPassword;

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ActualizarCredenciales(
                    new byte[0],
                    new byte[] { 9, 8, 7, 6 },
                    200000));

            CollectionAssert.AreEqual(
                hashAnterior,
                usuario.PasswordHash);

            CollectionAssert.AreEqual(
                saltAnterior,
                usuario.PasswordSalt);

            Assert.AreEqual(
                iteracionesAnteriores,
                usuario.IteracionesPassword);
        }

        [TestMethod]
        public void ActualizarCredenciales_SaltVacio_RechazaYConservaEstado()
        {
            Usuario usuario =
                CrearUsuario();

            byte[] hashAnterior =
                usuario.PasswordHash;

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ActualizarCredenciales(
                    new byte[] { 9, 8, 7, 6 },
                    null,
                    200000));

            CollectionAssert.AreEqual(
                hashAnterior,
                usuario.PasswordHash);

            Assert.AreEqual(
                100000,
                usuario.IteracionesPassword);
        }

        [TestMethod]
        public void ActualizarCredenciales_IteracionesInvalidas_RechazaYConservaEstado()
        {
            Usuario usuario =
                CrearUsuario();

            byte[] hashAnterior =
                usuario.PasswordHash;

            byte[] saltAnterior =
                usuario.PasswordSalt;

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ActualizarCredenciales(
                    new byte[] { 9, 8, 7, 6 },
                    new byte[] { 5, 4, 3, 2 },
                    0));

            CollectionAssert.AreEqual(
                hashAnterior,
                usuario.PasswordHash);

            CollectionAssert.AreEqual(
                saltAnterior,
                usuario.PasswordSalt);

            Assert.AreEqual(
                100000,
                usuario.IteracionesPassword);
        }

        private static Usuario CrearUsuario()
        {
            return new Usuario(
                10,
                20,
                "usuario.prueba",
                new byte[] { 1, 2, 3, 4 },
                new byte[] { 5, 6, 7, 8 },
                100000);
        }
    }
}
