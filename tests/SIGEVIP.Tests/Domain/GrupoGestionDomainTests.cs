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
                CrearGrupo(
                    2,
                    "GRUPO_HIJO");

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
                CrearGrupo(
                    2,
                    "GRUPO_HIJO");

            Grupo nieto =
                CrearGrupo(
                    3,
                    "GRUPO_NIETO");

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

        [TestMethod]
        public void ReemplazarGruposHijos_ConGruposValidos_ReemplazaAnteriores()
        {
            Grupo padre =
                CrearGrupo();

            padre.AgregarComponente(
                CrearGrupo(
                    2,
                    "HIJO_ANTERIOR"));

            Grupo primero =
                CrearGrupo(
                    3,
                    "HIJO_PRIMERO");

            Grupo segundo =
                CrearGrupo(
                    4,
                    "HIJO_SEGUNDO");

            padre.ReemplazarGruposHijos(
                new[]
                {
                    primero,
                    segundo
                });

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "HIJO_PRIMERO",
                    "HIJO_SEGUNDO"
                },
                padre.Componentes
                    .OfType<Grupo>()
                    .Select(
                        grupo => grupo.Codigo)
                    .ToArray());
        }

        [TestMethod]
        public void ReemplazarGruposHijos_SinGrupos_EliminaHijos()
        {
            Grupo padre =
                CrearGrupo();

            Permiso permiso =
                CrearPermiso(
                    1,
                    "CLIENTE_CONSULTAR");

            padre.AgregarComponente(
                permiso);

            padre.AgregarComponente(
                CrearGrupo(
                    2,
                    "GRUPO_HIJO"));

            padre.ReemplazarGruposHijos(
                new Grupo[0]);

            Assert.AreEqual(
                0,
                padre.Componentes
                    .OfType<Grupo>()
                    .Count());

            Assert.AreSame(
                permiso,
                padre.Componentes
                    .OfType<Permiso>()
                    .Single());
        }

        [TestMethod]
        public void ReemplazarGruposHijos_ColeccionNula_RechazaOperacion()
        {
            Grupo padre =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => padre.ReemplazarGruposHijos(
                    null));
        }

        [TestMethod]
        public void ReemplazarGruposHijos_ConGrupoNulo_RechazaSinModificarEstado()
        {
            Grupo padre =
                CrearGrupo();

            Grupo anterior =
                CrearGrupo(
                    2,
                    "HIJO_ANTERIOR");

            padre.AgregarComponente(
                anterior);

            Assert.ThrowsException<ReglaNegocioException>(
                () => padre.ReemplazarGruposHijos(
                    new Grupo[]
                    {
                        null
                    }));

            Assert.AreSame(
                anterior,
                padre.Componentes
                    .OfType<Grupo>()
                    .Single());
        }

        [TestMethod]
        public void ReemplazarGruposHijos_ConDuplicados_RechazaOperacion()
        {
            Grupo padre =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => padre.ReemplazarGruposHijos(
                    new[]
                    {
                        CrearGrupo(
                            2,
                            "HIJO_UNO"),
                        CrearGrupo(
                            2,
                            "HIJO_DOS")
                    }));
        }

        [TestMethod]
        public void ReemplazarGruposHijos_ConElMismoGrupo_RechazaOperacion()
        {
            Grupo padre =
                CrearGrupo();

            Assert.ThrowsException<ReglaNegocioException>(
                () => padre.ReemplazarGruposHijos(
                    new[]
                    {
                        padre
                    }));
        }

        [TestMethod]
        public void ReemplazarGruposHijos_QueGeneraCicloIndirecto_RechazaOperacion()
        {
            Grupo padre =
                CrearGrupo();

            Grupo hijo =
                CrearGrupo(
                    2,
                    "GRUPO_HIJO");

            hijo.AgregarComponente(
                padre);

            Assert.ThrowsException<ReglaNegocioException>(
                () => padre.ReemplazarGruposHijos(
                    new[]
                    {
                        hijo
                    }));
        }

        [TestMethod]
        public void ReemplazarGruposHijos_ConservaPermisosDirectos()
        {
            Grupo padre =
                CrearGrupo();

            Permiso primero =
                CrearPermiso(
                    1,
                    "CLIENTE_CONSULTAR");

            Permiso segundo =
                CrearPermiso(
                    2,
                    "VIAJE_CONSULTAR");

            padre.AgregarComponente(
                primero);

            padre.AgregarComponente(
                segundo);

            padre.ReemplazarGruposHijos(
                new[]
                {
                    CrearGrupo(
                        3,
                        "GRUPO_HIJO")
                });

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "CLIENTE_CONSULTAR",
                    "VIAJE_CONSULTAR"
                },
                padre.Componentes
                    .OfType<Permiso>()
                    .Select(
                        permiso => permiso.Codigo)
                    .ToArray());
        }

        private static Grupo CrearGrupo()
        {
            return CrearGrupo(
                1,
                "GRUPO_PRUEBA");
        }

        private static Grupo CrearGrupo(
            int idGrupo,
            string codigo)
        {
            return new Grupo(
                idGrupo,
                codigo,
                "Grupo " + codigo,
                "Descripción " + codigo);
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
