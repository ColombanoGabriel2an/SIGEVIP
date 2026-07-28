using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class GrupoGestionDomainTests
    {
        [TestMethod]
        public void ActualizarDatos_ConDatosValidos_ActualizaNombreYDescripcion()
        {
            Grupo grupo =
                CrearGrupo();

            grupo.ActualizarDatos(
                " Grupo actualizado ",
                " Descripción actualizada ");

            Assert.AreEqual(
                "Grupo actualizado",
                grupo.Nombre);

            Assert.AreEqual(
                "Descripción actualizada",
                grupo.Descripcion);
        }

        [TestMethod]
        public void ActualizarDatos_ConNombreVacio_RechazaOperacion()
        {
            Grupo grupo =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.ActualizarDatos(
                    " ",
                    "Descripción válida"));
        }

        [TestMethod]
        public void ActualizarDatos_ConDescripcionVacia_RechazaOperacion()
        {
            Grupo grupo =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.ActualizarDatos(
                    "Nombre válido",
                    " "));
        }

        [TestMethod]
        public void ReemplazarPermisosDirectos_SinPermisos_RechazaOperacion()
        {
            Grupo grupo =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.ReemplazarPermisosDirectos(
                    new Permiso[0]));
        }

        [TestMethod]
        public void ReemplazarPermisosDirectos_ConPermisoNulo_RechazaOperacion()
        {
            Grupo grupo =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.ReemplazarPermisosDirectos(
                    new Permiso[]
                    {
                        null
                    }));
        }

        [TestMethod]
        public void ReemplazarPermisosDirectos_ConDuplicados_RechazaOperacion()
        {
            Grupo grupo =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.ReemplazarPermisosDirectos(
                    new[]
                    {
                        CrearPermiso(
                            10,
                            "CLIENTE_CONSULTAR"),
                        CrearPermiso(
                            10,
                            "VIAJE_CONSULTAR")
                    }));
        }

        [TestMethod]
        public void ReemplazarPermisosDirectos_ReemplazaPermisosAnteriores()
        {
            Grupo grupo =
                CrearGrupo();

            grupo.AgregarComponente(
                CrearPermiso(
                    1,
                    "CLIENTE_CONSULTAR"));

            grupo.ReemplazarPermisosDirectos(
                new[]
                {
                    CrearPermiso(
                        2,
                        "VIAJE_CONSULTAR"),
                    CrearPermiso(
                        3,
                        "VISITA_REGISTRAR")
                });

            string[] codigos =
                grupo.Componentes
                    .OfType<Permiso>()
                    .Select(
                        permiso => permiso.Codigo)
                    .ToArray();

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "VIAJE_CONSULTAR",
                    "VISITA_REGISTRAR"
                },
                codigos);
        }

        [TestMethod]
        public void ReemplazarPermisosDirectos_ConservaGruposHijos()
        {
            Grupo padre =
                CrearGrupo();

            Grupo hijo =
                new Grupo(
                    2,
                    "GRUPO_HIJO",
                    "Grupo hijo",
                    "Descripción del grupo hijo");

            padre.AgregarComponente(
                hijo);

            padre.AgregarComponente(
                CrearPermiso(
                    1,
                    "CLIENTE_CONSULTAR"));

            padre.ReemplazarPermisosDirectos(
                new[]
                {
                    CrearPermiso(
                        2,
                        "VIAJE_CONSULTAR")
                });

            Assert.AreEqual(
                1,
                padre.Componentes
                    .OfType<Grupo>()
                    .Count());

            Assert.AreSame(
                hijo,
                padre.Componentes
                    .OfType<Grupo>()
                    .Single());
        }

        [TestMethod]
        public void ReemplazarPermisosDirectos_NoModificaGrupoGrupo()
        {
            Grupo padre =
                CrearGrupo();

            Grupo hijo =
                new Grupo(
                    2,
                    "GRUPO_HIJO",
                    "Grupo hijo",
                    "Descripción del grupo hijo");

            Grupo nieto =
                new Grupo(
                    3,
                    "GRUPO_NIETO",
                    "Grupo nieto",
                    "Descripción del grupo nieto");

            hijo.AgregarComponente(
                nieto);

            padre.AgregarComponente(
                hijo);

            padre.ReemplazarPermisosDirectos(
                new[]
                {
                    CrearPermiso(
                        5,
                        "GRUPO_GESTIONAR")
                });

            Assert.AreSame(
                nieto,
                hijo.Componentes
                    .OfType<Grupo>()
                    .Single());
        }

        private static Grupo CrearGrupo()
        {
            return new Grupo(
                1,
                "GRUPO_PRUEBA",
                "Grupo de prueba",
                "Descripción inicial");
        }

        private static Permiso CrearPermiso(
            int idPermiso,
            string codigo)
        {
            return new Permiso(
                idPermiso,
                codigo,
                "Permiso " + codigo,
                "Descripción " + codigo);
        }
    }
}
