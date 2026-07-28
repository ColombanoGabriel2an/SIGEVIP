using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class CambiarClaveServiceTests
    {
        [TestMethod]
        public void Cambiar_SinSesion_RechazaOperacion()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    false,
                    true);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => contexto.Service.Cambiar(
                    CrearCommandValido()));

            Assert.IsFalse(
                contexto.Repository.FueInvocado);
        }

        [TestMethod]
        public void Cambiar_UsuarioInactivo_RechazaOperacion()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    false);

            Assert.ThrowsException<AccesoDenegadoException>(
                () => contexto.Service.Cambiar(
                    CrearCommandValido()));

            Assert.IsFalse(
                contexto.Repository.FueInvocado);
        }

        [TestMethod]
        public void Cambiar_CommandNulo_RechazaOperacion()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    true);

            ReglaNegocioException exception =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => contexto.Service.Cambiar(
                        null));

            Assert.AreEqual(
                CambiarClaveService
                    .MensajeCamposObligatorios,
                exception.Message);
        }

        [TestMethod]
        public void Cambiar_CamposObligatoriosIncompletos_RechazaOperacion()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    true);

            CambiarClaveCommand command =
                new CambiarClaveCommand(
                    " ",
                    "NuevaClave123",
                    "NuevaClave123");

            ReglaNegocioException exception =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => contexto.Service.Cambiar(
                        command));

            Assert.AreEqual(
                CambiarClaveService
                    .MensajeCamposObligatorios,
                exception.Message);

            Assert.IsFalse(
                contexto.Repository.FueInvocado);
        }

        [TestMethod]
        public void Cambiar_ClaveNuevaCorta_RechazaOperacion()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    true);

            CambiarClaveCommand command =
                new CambiarClaveCommand(
                    "ClaveActual123",
                    "1234567",
                    "1234567");

            ReglaNegocioException exception =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => contexto.Service.Cambiar(
                        command));

            Assert.AreEqual(
                CambiarClaveService
                    .MensajeClaveNuevaCorta,
                exception.Message);
        }

        [TestMethod]
        public void Cambiar_ConfirmacionDistinta_RechazaOperacion()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    true);

            CambiarClaveCommand command =
                new CambiarClaveCommand(
                    "ClaveActual123",
                    "NuevaClave123",
                    "OtraClave123");

            ReglaNegocioException exception =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => contexto.Service.Cambiar(
                        command));

            Assert.AreEqual(
                CambiarClaveService
                    .MensajeConfirmacionInvalida,
                exception.Message);
        }

        [TestMethod]
        public void Cambiar_ClaveActualInvalida_RechazaOperacion()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    true);

            contexto.PasswordHasher
                .ClaveActualValida =
                    false;

            ReglaNegocioException exception =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => contexto.Service.Cambiar(
                        CrearCommandValido()));

            Assert.AreEqual(
                CambiarClaveService
                    .MensajeClaveActualInvalida,
                exception.Message);

            Assert.IsFalse(
                contexto.Repository.FueInvocado);

            Assert.AreEqual(
                0,
                contexto.PasswordHasher
                    .CantidadCreaciones);
        }

        [TestMethod]
        public void Cambiar_UsuarioYaNoDisponible_RechazaYConservaCredenciales()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    true);

            byte[] hashAnterior =
                contexto.Usuario.PasswordHash;

            byte[] saltAnterior =
                contexto.Usuario.PasswordSalt;

            contexto.Repository
                .ResultadoActualizacion =
                    false;

            ReglaNegocioException exception =
                Assert.ThrowsException<ReglaNegocioException>(
                    () => contexto.Service.Cambiar(
                        CrearCommandValido()));

            Assert.AreEqual(
                CambiarClaveService
                    .MensajeUsuarioNoDisponible,
                exception.Message);

            CollectionAssert.AreEqual(
                hashAnterior,
                contexto.Usuario.PasswordHash);

            CollectionAssert.AreEqual(
                saltAnterior,
                contexto.Usuario.PasswordSalt);

            Assert.AreEqual(
                100000,
                contexto.Usuario
                    .IteracionesPassword);
        }

        [TestMethod]
        public void Cambiar_DatosValidos_ActualizaRepositorioYUsuario()
        {
            ContextoPrueba contexto =
                CrearContexto(
                    true,
                    true);

            CambiarClaveCommand command =
                CrearCommandValido();

            contexto.Service.Cambiar(
                command);

            Assert.IsTrue(
                contexto.Repository.FueInvocado);

            Assert.AreEqual(
                contexto.Usuario.IdUsuario,
                contexto.Repository.IdUsuarioRecibido);

            Assert.AreEqual(
                "ClaveActual123",
                contexto.PasswordHasher
                    .ClaveVerificada);

            Assert.AreEqual(
                "NuevaClave123",
                contexto.PasswordHasher
                    .ClaveHasheada);

            Assert.AreEqual(
                1,
                contexto.PasswordHasher
                    .CantidadCreaciones);

            CollectionAssert.AreEqual(
                contexto.PasswordHasher
                    .Resultado.Hash,
                contexto.Usuario.PasswordHash);

            CollectionAssert.AreEqual(
                contexto.PasswordHasher
                    .Resultado.Salt,
                contexto.Usuario.PasswordSalt);

            Assert.AreEqual(
                contexto.PasswordHasher
                    .Resultado.Iteraciones,
                contexto.Usuario
                    .IteracionesPassword);
        }

        private static CambiarClaveCommand
            CrearCommandValido()
        {
            return new CambiarClaveCommand(
                "ClaveActual123",
                "NuevaClave123",
                "NuevaClave123");
        }

        private static ContextoPrueba CrearContexto(
            bool haySesion,
            bool usuarioActivo)
        {
            Usuario usuario =
                new Usuario(
                    10,
                    20,
                    "usuario.prueba",
                    new byte[] { 1, 2, 3, 4 },
                    new byte[] { 5, 6, 7, 8 },
                    100000);

            if (!usuarioActivo)
            {
                usuario.Desactivar();
            }

            UsuarioClaveRepositoryFalso repository =
                new UsuarioClaveRepositoryFalso();

            PasswordHasherFalso passwordHasher =
                new PasswordHasherFalso();

            SesionActualFalsa sesion =
                new SesionActualFalsa(
                    haySesion,
                    usuario);

            CambiarClaveService service =
                new CambiarClaveService(
                    repository,
                    passwordHasher,
                    sesion);

            return new ContextoPrueba(
                service,
                repository,
                passwordHasher,
                usuario);
        }

        private sealed class ContextoPrueba
        {
            public ContextoPrueba(
                CambiarClaveService service,
                UsuarioClaveRepositoryFalso repository,
                PasswordHasherFalso passwordHasher,
                Usuario usuario)
            {
                Service = service;
                Repository = repository;
                PasswordHasher = passwordHasher;
                Usuario = usuario;
            }

            public CambiarClaveService Service
            {
                get;
                private set;
            }

            public UsuarioClaveRepositoryFalso Repository
            {
                get;
                private set;
            }

            public PasswordHasherFalso PasswordHasher
            {
                get;
                private set;
            }

            public Usuario Usuario
            {
                get;
                private set;
            }
        }

        private sealed class UsuarioClaveRepositoryFalso
            : IUsuarioClaveRepository
        {
            public UsuarioClaveRepositoryFalso()
            {
                ResultadoActualizacion =
                    true;
            }

            public bool ResultadoActualizacion
            {
                get;
                set;
            }

            public bool FueInvocado
            {
                get;
                private set;
            }

            public int IdUsuarioRecibido
            {
                get;
                private set;
            }

            public PasswordHashResult PasswordHashRecibido
            {
                get;
                private set;
            }

            public bool ActualizarCredenciales(
                int idUsuario,
                PasswordHashResult passwordHash)
            {
                FueInvocado =
                    true;

                IdUsuarioRecibido =
                    idUsuario;

                PasswordHashRecibido =
                    passwordHash;

                return ResultadoActualizacion;
            }
        }

        private sealed class PasswordHasherFalso
            : IPasswordHasher
        {
            public PasswordHasherFalso()
            {
                ClaveActualValida =
                    true;

                Resultado =
                    new PasswordHashResult(
                        new byte[] { 9, 8, 7, 6 },
                        new byte[] { 4, 3, 2, 1 },
                        200000);
            }

            public bool ClaveActualValida
            {
                get;
                set;
            }

            public string ClaveVerificada
            {
                get;
                private set;
            }

            public string ClaveHasheada
            {
                get;
                private set;
            }

            public int CantidadCreaciones
            {
                get;
                private set;
            }

            public PasswordHashResult Resultado
            {
                get;
                private set;
            }

            public PasswordHashResult CrearHash(
                string password)
            {
                ClaveHasheada =
                    password;

                CantidadCreaciones++;

                return Resultado;
            }

            public bool Verificar(
                string password,
                byte[] hashEsperado,
                byte[] salt,
                int iteraciones)
            {
                ClaveVerificada =
                    password;

                return ClaveActualValida;
            }
        }

        private sealed class SesionActualFalsa
            : ISesionActual
        {
            public SesionActualFalsa(
                bool hayUsuarioAutenticado,
                Usuario usuarioActual)
            {
                HayUsuarioAutenticado =
                    hayUsuarioAutenticado;

                UsuarioActual =
                    hayUsuarioAutenticado
                        ? usuarioActual
                        : null;
            }

            public bool HayUsuarioAutenticado
            {
                get;
                private set;
            }

            public Usuario UsuarioActual
            {
                get;
                private set;
            }

            public void Iniciar(
                Usuario usuario)
            {
                UsuarioActual =
                    usuario;

                HayUsuarioAutenticado =
                    usuario != null;
            }

            public void Cerrar()
            {
                UsuarioActual =
                    null;

                HayUsuarioAutenticado =
                    false;
            }
        }
    }
}
