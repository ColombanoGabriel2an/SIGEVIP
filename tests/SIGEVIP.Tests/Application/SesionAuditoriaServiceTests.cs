using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class SesionAuditoriaServiceTests
    {
        [TestMethod]
        public void RegistrarInicioSesion_ConSesionActiva_RegistraEvento()
        {
            SesionAuditoriaRepositoryFalso repository =
                new SesionAuditoriaRepositoryFalso();

            SesionAuditoriaService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario());

            servicio.RegistrarInicioSesion();

            Assert.AreEqual(
                1,
                repository.CantidadRegistros);

            Assert.IsNotNull(
                repository.UltimoRegistro);

            Assert.AreEqual(
                "Seguridad",
                repository.UltimoRegistro.Modulo);

            Assert.AreEqual(
                "InicioSesion",
                repository.UltimoRegistro.Accion);

            Assert.AreEqual(
                "Sesion",
                repository.UltimoRegistro.Entidad);

            Assert.AreEqual(
                1,
                repository.UltimoRegistro.IdUsuario);

            Assert.AreEqual(
                1,
                repository.UltimoRegistro.IdEntidad);

            Assert.AreEqual(
                "aperez",
                repository.UltimoRegistro.NombreUsuario);
        }

        [TestMethod]
        public void RegistrarCierreSesion_ConSesionActiva_RegistraEvento()
        {
            SesionAuditoriaRepositoryFalso repository =
                new SesionAuditoriaRepositoryFalso();

            SesionAuditoriaService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario());

            servicio.RegistrarCierreSesion();

            Assert.AreEqual(
                1,
                repository.CantidadRegistros);

            Assert.AreEqual(
                "CierreSesion",
                repository.UltimoRegistro.Accion);

            StringAssert.Contains(
                repository.UltimoRegistro.Descripcion,
                "aperez");
        }

        [TestMethod]
        public void RegistrarInicioSesion_NoExponeCredenciales()
        {
            SesionAuditoriaRepositoryFalso repository =
                new SesionAuditoriaRepositoryFalso();

            SesionAuditoriaService servicio =
                CrearServicio(
                    repository,
                    CrearUsuario());

            servicio.RegistrarInicioSesion();

            string descripcion =
                repository
                    .UltimoRegistro
                    .Descripcion
                    .ToLowerInvariant();

            Assert.IsFalse(
                descripcion.Contains(
                    "password"));

            Assert.IsFalse(
                descripcion.Contains(
                    "contraseña"));

            Assert.IsFalse(
                descripcion.Contains(
                    "hash"));

            Assert.IsFalse(
                descripcion.Contains(
                    "salt"));
        }

        [TestMethod]
        public void RegistrarInicioSesion_SinSesion_RechazaOperacion()
        {
            SesionAuditoriaRepositoryFalso repository =
                new SesionAuditoriaRepositoryFalso();

            SesionAuditoriaService servicio =
                new SesionAuditoriaService(
                    repository,
                    new SesionActual());

            Assert.ThrowsException
                <InvalidOperationException>(
                    () =>
                        servicio
                            .RegistrarInicioSesion());

            Assert.AreEqual(
                0,
                repository.CantidadRegistros);
        }

        [TestMethod]
        public void RegistrarCierreSesion_SinSesion_RechazaOperacion()
        {
            SesionAuditoriaRepositoryFalso repository =
                new SesionAuditoriaRepositoryFalso();

            SesionAuditoriaService servicio =
                new SesionAuditoriaService(
                    repository,
                    new SesionActual());

            Assert.ThrowsException
                <InvalidOperationException>(
                    () =>
                        servicio
                            .RegistrarCierreSesion());

            Assert.AreEqual(
                0,
                repository.CantidadRegistros);
        }

        private static SesionAuditoriaService CrearServicio(
            SesionAuditoriaRepositoryFalso repository,
            Usuario usuario)
        {
            SesionActual sesion =
                new SesionActual();

            sesion.Iniciar(
                usuario);

            return new SesionAuditoriaService(
                repository,
                sesion);
        }

        private static Usuario CrearUsuario()
        {
            return new Usuario(
                1,
                1,
                "aperez",
                new byte[32],
                new byte[32],
                1000);
        }

        private sealed class SesionAuditoriaRepositoryFalso
            : ISesionAuditoriaRepository
        {
            public int CantidadRegistros
            {
                get;
                private set;
            }

            public AuditoriaRegistro UltimoRegistro
            {
                get;
                private set;
            }

            public void Registrar(
                AuditoriaRegistro registro)
            {
                UltimoRegistro =
                    registro;

                CantidadRegistros++;
            }
        }
    }
}