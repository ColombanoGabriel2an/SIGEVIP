using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Security;

namespace SIGEVIP.Tests.Infrastructure
{
    [TestClass]
    public class Pbkdf2PasswordHasherTests
    {
        [TestMethod]
        public void CrearHash_ConPasswordValido_GeneraHash()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult resultado =
                hasher.CrearHash("ClaveSegura123");

            Assert.IsNotNull(resultado.Hash);
            Assert.AreEqual(
                Pbkdf2PasswordHasher.LongitudHashBytes,
                resultado.Hash.Length);
        }

        [TestMethod]
        public void CrearHash_ConPasswordValido_GeneraSalt()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult resultado =
                hasher.CrearHash("ClaveSegura123");

            Assert.IsNotNull(resultado.Salt);
            Assert.AreEqual(
                Pbkdf2PasswordHasher.LongitudSaltBytes,
                resultado.Salt.Length);
        }

        [TestMethod]
        public void CrearHash_UsaIteracionesPredeterminadas()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult resultado =
                hasher.CrearHash("ClaveSegura123");

            Assert.AreEqual(
                Pbkdf2PasswordHasher.IteracionesPredeterminadas,
                resultado.Iteraciones);
        }

        [TestMethod]
        public void CrearHash_DosVecesConMismoPassword_GeneraSaltDiferente()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult primero =
                hasher.CrearHash("ClaveSegura123");

            PasswordHashResult segundo =
                hasher.CrearHash("ClaveSegura123");

            CollectionAssert.AreNotEqual(
                primero.Salt,
                segundo.Salt);
        }

        [TestMethod]
        public void CrearHash_DosVecesConMismoPassword_GeneraHashDiferente()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult primero =
                hasher.CrearHash("ClaveSegura123");

            PasswordHashResult segundo =
                hasher.CrearHash("ClaveSegura123");

            CollectionAssert.AreNotEqual(
                primero.Hash,
                segundo.Hash);
        }

        [TestMethod]
        public void Verificar_ConPasswordCorrecto_DevuelveTrue()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult resultado =
                hasher.CrearHash("ClaveSegura123");

            bool valido =
                hasher.Verificar(
                    "ClaveSegura123",
                    resultado.Hash,
                    resultado.Salt,
                    resultado.Iteraciones);

            Assert.IsTrue(valido);
        }

        [TestMethod]
        public void Verificar_ConPasswordIncorrecto_DevuelveFalse()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult resultado =
                hasher.CrearHash("ClaveSegura123");

            bool valido =
                hasher.Verificar(
                    "ClaveIncorrecta",
                    resultado.Hash,
                    resultado.Salt,
                    resultado.Iteraciones);

            Assert.IsFalse(valido);
        }

        [TestMethod]
        public void Verificar_ConHashModificado_DevuelveFalse()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            PasswordHashResult resultado =
                hasher.CrearHash("ClaveSegura123");

            byte[] hashModificado =
                resultado.Hash;

            hashModificado[0] =
                (byte)(hashModificado[0] ^ 255);

            bool valido =
                hasher.Verificar(
                    "ClaveSegura123",
                    hashModificado,
                    resultado.Salt,
                    resultado.Iteraciones);

            Assert.IsFalse(valido);
        }

        [TestMethod]
        public void CrearHash_ConPasswordVacio_LanzaExcepcion()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            Assert.ThrowsException<ArgumentException>(
                () => hasher.CrearHash(" "));
        }

        [TestMethod]
        public void Verificar_ConHashNulo_LanzaExcepcion()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            Assert.ThrowsException<ArgumentException>(
                () => hasher.Verificar(
                    "ClaveSegura123",
                    null,
                    new byte[] { 1, 2, 3, 4 },
                    100000));
        }

        [TestMethod]
        public void Verificar_ConSaltVacio_LanzaExcepcion()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            Assert.ThrowsException<ArgumentException>(
                () => hasher.Verificar(
                    "ClaveSegura123",
                    new byte[] { 1, 2, 3, 4 },
                    new byte[0],
                    100000));
        }

        [TestMethod]
        public void Verificar_ConIteracionesInvalidas_LanzaExcepcion()
        {
            Pbkdf2PasswordHasher hasher =
                new Pbkdf2PasswordHasher();

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => hasher.Verificar(
                    "ClaveSegura123",
                    new byte[] { 1, 2, 3, 4 },
                    new byte[] { 5, 6, 7, 8 },
                    0));
        }
    }
}
