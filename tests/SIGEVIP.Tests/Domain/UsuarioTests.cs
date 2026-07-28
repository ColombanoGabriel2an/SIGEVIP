using System.Collections.Generic;
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
            Usuario usuario =
                CrearUsuario();

            Assert.IsTrue(
                usuario.Activo);
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
            Usuario usuario =
                new Usuario(
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
            Assert.ThrowsException<ReglaNegocioException>(
                () => new Usuario(
                    1,
                    1,
                    "aperez",
                    null,
                    CrearSalt(),
                    100000));
        }

        [TestMethod]
        public void CrearUsuario_ConSaltVacio_LanzaExcepcion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new Usuario(
                    1,
                    1,
                    "aperez",
                    CrearHash(),
                    new byte[0],
                    100000));
        }

        [TestMethod]
        public void CrearUsuario_ConIteracionesInvalidas_LanzaExcepcion()
        {
            Assert.ThrowsException<ReglaNegocioException>(
                () => new Usuario(
                    1,
                    1,
                    "aperez",
                    CrearHash(),
                    CrearSalt(),
                    0));
        }

        [TestMethod]
        public void ActualizarNombreUsuario_ConNombreValido_Normaliza()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.ActualizarNombreUsuario(
                "  NUEVO.USUARIO  ");

            Assert.AreEqual(
                "nuevo.usuario",
                usuario.NombreUsuario);
        }

        [TestMethod]
        public void ActualizarNombreUsuario_ConNombreVacio_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ActualizarNombreUsuario(
                    " "));
        }

        [TestMethod]
        public void ActualizarNombreUsuario_NoModificaPersona()
        {
            Usuario usuario =
                CrearUsuario();

            int idPersonaOriginal =
                usuario.IdPersona;

            usuario.ActualizarNombreUsuario(
                "usuario.modificado");

            Assert.AreEqual(
                idPersonaOriginal,
                usuario.IdPersona);
        }

        [TestMethod]
        public void DesactivarUsuario_CambiaEstadoAInactivo()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.Desactivar();

            Assert.IsFalse(
                usuario.Activo);
        }

        [TestMethod]
        public void ActivarUsuario_DespuesDeDesactivarlo_CambiaEstadoAActivo()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.Desactivar();

            usuario.Activar();

            Assert.IsTrue(
                usuario.Activo);
        }

        [TestMethod]
        public void PasswordHash_NoPermiteModificarElValorInterno()
        {
            Usuario usuario =
                CrearUsuario();

            byte[] hashExpuesto =
                usuario.PasswordHash;

            hashExpuesto[0] = 255;

            Assert.AreNotEqual(
                255,
                usuario.PasswordHash[0]);
        }

        [TestMethod]
        public void PasswordSalt_NoPermiteModificarElValorInterno()
        {
            Usuario usuario =
                CrearUsuario();

            byte[] saltExpuesto =
                usuario.PasswordSalt;

            saltExpuesto[0] = 255;

            Assert.AreNotEqual(
                255,
                usuario.PasswordSalt[0]);
        }

        [TestMethod]
        public void AgregarGrupo_ConGrupoValido_AsignaGrupo()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.AgregarGrupo(
                CrearGrupo(
                    1,
                    "COMERCIAL"));

            Assert.AreEqual(
                1,
                usuario.Grupos.Count);
        }

        [TestMethod]
        public void AgregarGrupo_Nulo_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.AgregarGrupo(
                    null));
        }

        [TestMethod]
        public void AgregarGrupo_Duplicado_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.AgregarGrupo(
                CrearGrupo(
                    0,
                    "COMERCIAL"));

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.AgregarGrupo(
                    CrearGrupo(
                        0,
                        " comercial ")));
        }

        [TestMethod]
        public void AgregarGrupo_ConVariosGrupos_AsignaTodos()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.AgregarGrupo(
                CrearGrupo(
                    1,
                    "COMERCIAL"));

            usuario.AgregarGrupo(
                CrearGrupo(
                    2,
                    "GERENTE"));

            Assert.AreEqual(
                2,
                usuario.Grupos.Count);
        }

        [TestMethod]
        public void ReemplazarGrupos_ConVariosGrupos_SustituyeAsignaciones()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.AgregarGrupo(
                CrearGrupo(
                    1,
                    "COMERCIAL"));

            List<Grupo> nuevosGrupos =
                new List<Grupo>
                {
                    CrearGrupo(
                        2,
                        "ADMINISTRATIVO"),
                    CrearGrupo(
                        3,
                        "GERENTE")
                };

            usuario.ReemplazarGrupos(
                nuevosGrupos);

            Assert.AreEqual(
                2,
                usuario.Grupos.Count);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "ADMINISTRATIVO",
                    "GERENTE"
                },
                ObtenerCodigos(
                    usuario.Grupos));
        }

        [TestMethod]
        public void ReemplazarGrupos_ConColeccionNula_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ReemplazarGrupos(
                    null));
        }

        [TestMethod]
        public void ReemplazarGrupos_ConColeccionVacia_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ReemplazarGrupos(
                    new List<Grupo>()));
        }

        [TestMethod]
        public void ReemplazarGrupos_ConGrupoNulo_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ReemplazarGrupos(
                    new List<Grupo>
                    {
                        CrearGrupo(
                            1,
                            "COMERCIAL"),
                        null
                    }));
        }

        [TestMethod]
        public void ReemplazarGrupos_ConDuplicadosPorId_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ReemplazarGrupos(
                    new List<Grupo>
                    {
                        CrearGrupo(
                            1,
                            "COMERCIAL"),
                        CrearGrupo(
                            1,
                            "OTRO_CODIGO")
                    }));
        }

        [TestMethod]
        public void ReemplazarGrupos_ConDuplicadosPorCodigo_LanzaExcepcion()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ReemplazarGrupos(
                    new List<Grupo>
                    {
                        CrearGrupo(
                            0,
                            "COMERCIAL"),
                        CrearGrupo(
                            0,
                            " comercial ")
                    }));
        }

        [TestMethod]
        public void ReemplazarGrupos_ConDatosInvalidos_ConservaAsignacionesAnteriores()
        {
            Usuario usuario =
                CrearUsuario();

            usuario.AgregarGrupo(
                CrearGrupo(
                    1,
                    "COMERCIAL"));

            Assert.ThrowsException<ReglaNegocioException>(
                () => usuario.ReemplazarGrupos(
                    new List<Grupo>
                    {
                        CrearGrupo(
                            2,
                            "GERENTE"),
                        CrearGrupo(
                            2,
                            "ADMINISTRATIVO")
                    }));

            Assert.AreEqual(
                1,
                usuario.Grupos.Count);

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "COMERCIAL"
                },
                ObtenerCodigos(
                    usuario.Grupos));
        }

        [TestMethod]
        public void Grupos_NoPermiteModificarColeccionDesdeElExterior()
        {
            Usuario usuario =
                CrearUsuario();

            Assert.IsFalse(
                usuario.Grupos is List<Grupo>);
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

        private static string[] ObtenerCodigos(
            IEnumerable<Grupo> grupos)
        {
            List<string> codigos =
                new List<string>();

            foreach (Grupo grupo in grupos)
            {
                codigos.Add(
                    grupo.Codigo);
            }

            return codigos.ToArray();
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
