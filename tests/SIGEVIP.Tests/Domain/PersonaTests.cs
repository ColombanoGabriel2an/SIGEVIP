using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class PersonaTests
    {
        [TestMethod]
        public void CrearPersona_ConDatosValidos_NaceActiva()
        {
            Persona persona = CrearPersona();

            Assert.IsTrue(persona.Activo);
        }

        [TestMethod]
        public void CrearPersona_ConNombreVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Persona(
                        1,
                        " ",
                        "Pérez",
                        "persona@prueba.com"));

            StringAssert.Contains(
                excepcion.Message,
                "nombre");
        }

        [TestMethod]
        public void CrearPersona_ConApellidoVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Persona(
                        1,
                        "Ana",
                        " ",
                        "persona@prueba.com"));

            StringAssert.Contains(
                excepcion.Message,
                "apellido");
        }

        [TestMethod]
        public void CrearPersona_ConEmailVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Persona(
                        1,
                        "Ana",
                        "Pérez",
                        " "));

            StringAssert.Contains(
                excepcion.Message,
                "email");
        }

        [TestMethod]
        public void DesactivarPersona_CambiaEstadoAInactiva()
        {
            Persona persona = CrearPersona();

            persona.Desactivar();

            Assert.IsFalse(persona.Activo);
        }

        [TestMethod]
        public void ActivarPersona_DespuesDeDesactivarla_CambiaEstadoAActiva()
        {
            Persona persona = CrearPersona();
            persona.Desactivar();

            persona.Activar();

            Assert.IsTrue(persona.Activo);
        }

        private static Persona CrearPersona()
        {
            return new Persona(
                1,
                "Ana",
                "Pérez",
                "persona@prueba.com");
        }
    }
}
