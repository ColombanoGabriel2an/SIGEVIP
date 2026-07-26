using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class PerfilSesionServiceTests
    {
        [TestMethod]
        public void Constructor_ConRepositorioNulo_LanzaExcepcion()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new PerfilSesionService(null));
        }

        [TestMethod]
        public void ObtenerPorIdPersona_ConIdInvalido_LanzaExcepcion()
        {
            PerfilSesionService servicio =
                CrearServicio(null);

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => servicio.ObtenerPorIdPersona(0));
        }

        [TestMethod]
        public void ObtenerPorIdPersona_ConPersonaExistente_DevuelvePerfil()
        {
            PerfilSesion perfil =
                new PerfilSesion(
                    15,
                    "Ana",
                    "Perez",
                    "ana@example.com");

            PerfilSesionService servicio =
                CrearServicio(perfil);

            PerfilSesion resultado =
                servicio.ObtenerPorIdPersona(15);

            Assert.AreSame(
                perfil,
                resultado);

            Assert.AreEqual(
                "Ana Perez",
                resultado.NombreCompleto);
        }

        [TestMethod]
        public void ObtenerPorIdPersona_ConPersonaInexistente_DevuelveNulo()
        {
            PerfilSesionService servicio =
                CrearServicio(null);

            PerfilSesion resultado =
                servicio.ObtenerPorIdPersona(99);

            Assert.IsNull(resultado);
        }

        private static PerfilSesionService CrearServicio(
            PerfilSesion perfil)
        {
            return new PerfilSesionService(
                new PerfilSesionRepositoryFalso(
                    perfil));
        }

        private sealed class PerfilSesionRepositoryFalso
            : IPerfilSesionRepository
        {
            private readonly PerfilSesion _perfil;

            public PerfilSesionRepositoryFalso(
                PerfilSesion perfil)
            {
                _perfil = perfil;
            }

            public PerfilSesion BuscarPorIdPersona(
                int idPersona)
            {
                if (_perfil == null)
                {
                    return null;
                }

                return _perfil.IdPersona == idPersona
                    ? _perfil
                    : null;
            }
        }
    }
}
