using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Domain
{
    [TestClass]
    public class GrupoPermisoTests
    {
        [TestMethod]
        public void CrearPermiso_ConDatosValidos_NaceActivo()
        {
            Permiso permiso = CrearPermiso(
                1,
                "VIAJE_CREAR");

            Assert.IsTrue(permiso.Activo);
        }

        [TestMethod]
        public void CrearPermiso_ConCodigoVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Permiso(
                        1,
                        " ",
                        "Crear viajes",
                        string.Empty));

            StringAssert.Contains(excepcion.Message, "código");
        }

        [TestMethod]
        public void CrearPermiso_ConNombreVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Permiso(
                        1,
                        "VIAJE_CREAR",
                        " ",
                        string.Empty));

            StringAssert.Contains(excepcion.Message, "nombre");
        }

        [TestMethod]
        public void PermisoInactivo_NoDevuelvePermisosEfectivos()
        {
            Permiso permiso = CrearPermiso(
                1,
                "VIAJE_CREAR");

            permiso.Desactivar();

            Assert.AreEqual(
                0,
                permiso.ObtenerPermisosEfectivos().Count);
        }

        [TestMethod]
        public void CrearGrupo_ConDatosValidos_NaceActivo()
        {
            Grupo grupo = CrearGrupo(
                1,
                "COMERCIAL");

            Assert.IsTrue(grupo.Activo);
        }

        [TestMethod]
        public void CrearGrupo_ConCodigoVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Grupo(
                        1,
                        " ",
                        "Comercial",
                        string.Empty));

            StringAssert.Contains(excepcion.Message, "código");
        }

        [TestMethod]
        public void CrearGrupo_ConNombreVacio_LanzaExcepcion()
        {
            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => new Grupo(
                        1,
                        "COMERCIAL",
                        " ",
                        string.Empty));

            StringAssert.Contains(excepcion.Message, "nombre");
        }

        [TestMethod]
        public void AgregarComponente_ConPermisoValido_AgregaPermiso()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            grupo.AgregarComponente(
                CrearPermiso(1, "VIAJE_CREAR"));

            Assert.AreEqual(1, grupo.Componentes.Count);
        }

        [TestMethod]
        public void AgregarComponente_ConGrupoHijoValido_AgregaGrupo()
        {
            Grupo padre = CrearGrupo(1, "GERENTE");
            Grupo hijo = CrearGrupo(2, "COMERCIAL");

            padre.AgregarComponente(hijo);

            Assert.AreEqual(1, padre.Componentes.Count);
        }

        [TestMethod]
        public void AgregarComponente_Nulo_LanzaExcepcion()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => grupo.AgregarComponente(null));

            StringAssert.Contains(
                excepcion.Message,
                "componente");
        }

        [TestMethod]
        public void AgregarComponente_MismaReferencia_LanzaExcepcion()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");
            Permiso permiso =
                CrearPermiso(1, "VIAJE_CREAR");

            grupo.AgregarComponente(permiso);

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.AgregarComponente(permiso));
        }

        [TestMethod]
        public void AgregarComponente_MismoIdPersistido_LanzaExcepcion()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            grupo.AgregarComponente(
                CrearPermiso(10, "VIAJE_CREAR"));

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.AgregarComponente(
                    CrearPermiso(
                        10,
                        "VISITA_REGISTRAR")));
        }

        [TestMethod]
        public void AgregarComponente_MismoCodigoNormalizado_LanzaExcepcion()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            grupo.AgregarComponente(
                CrearPermiso(0, "VIAJE_CREAR"));

            Assert.ThrowsException<ReglaNegocioException>(
                () => grupo.AgregarComponente(
                    CrearPermiso(
                        0,
                        " viaje_crear ")));
        }

        [TestMethod]
        public void AgregarGrupo_AElMismoGrupo_LanzaExcepcion()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => grupo.AgregarComponente(grupo));

            StringAssert.Contains(
                excepcion.Message,
                "sí mismo");
        }

        [TestMethod]
        public void AgregarGrupo_QueProduceCicloIndirecto_LanzaExcepcion()
        {
            Grupo primero = CrearGrupo(1, "PRIMERO");
            Grupo segundo = CrearGrupo(2, "SEGUNDO");
            Grupo tercero = CrearGrupo(3, "TERCERO");

            primero.AgregarComponente(segundo);
            segundo.AgregarComponente(tercero);

            ReglaNegocioException excepcion =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => tercero.AgregarComponente(primero));

            StringAssert.Contains(
                excepcion.Message,
                "ciclo");
        }

        [TestMethod]
        public void ObtenerPermisosEfectivos_DevuelvePermisosAnidados()
        {
            Grupo padre = CrearGrupo(1, "GERENTE");
            Grupo hijo = CrearGrupo(2, "COMERCIAL");

            hijo.AgregarComponente(
                CrearPermiso(1, "VIAJE_CREAR"));

            padre.AgregarComponente(hijo);

            Assert.AreEqual(
                1,
                padre.ObtenerPermisosEfectivos().Count);
        }

        [TestMethod]
        public void ObtenerPermisosEfectivos_EliminaDuplicados()
        {
            Grupo padre = CrearGrupo(1, "GERENTE");
            Grupo primero = CrearGrupo(2, "PRIMERO");
            Grupo segundo = CrearGrupo(3, "SEGUNDO");

            primero.AgregarComponente(
                CrearPermiso(0, "VIAJE_CREAR"));

            segundo.AgregarComponente(
                CrearPermiso(0, " viaje_crear "));

            padre.AgregarComponente(primero);
            padre.AgregarComponente(segundo);

            Assert.AreEqual(
                1,
                padre.ObtenerPermisosEfectivos().Count);
        }

        [TestMethod]
        public void GrupoInactivo_NoAportaPermisos()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            grupo.AgregarComponente(
                CrearPermiso(1, "VIAJE_CREAR"));

            grupo.Desactivar();

            Assert.AreEqual(
                0,
                grupo.ObtenerPermisosEfectivos().Count);
        }

        [TestMethod]
        public void GrupoHijoInactivo_NoAportaPermisos()
        {
            Grupo padre = CrearGrupo(1, "GERENTE");
            Grupo hijo = CrearGrupo(2, "COMERCIAL");

            hijo.AgregarComponente(
                CrearPermiso(1, "VIAJE_CREAR"));

            hijo.Desactivar();
            padre.AgregarComponente(hijo);

            Assert.AreEqual(
                0,
                padre.ObtenerPermisosEfectivos().Count);
        }

        [TestMethod]
        public void PermisoInactivo_NoAportaPermisosAlGrupo()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");
            Permiso permiso =
                CrearPermiso(1, "VIAJE_CREAR");

            permiso.Desactivar();
            grupo.AgregarComponente(permiso);

            Assert.AreEqual(
                0,
                grupo.ObtenerPermisosEfectivos().Count);
        }

        [TestMethod]
        public void Componentes_NoPermiteModificarColeccionDesdeElExterior()
        {
            Grupo grupo = CrearGrupo(1, "COMERCIAL");

            Assert.IsFalse(
                grupo.Componentes
                    is List<SIGEVIP.Domain.Security.IPermisoComponente>);
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
