using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class InicializacionSeguridadServiceTests
    {
        [TestMethod]
        public void CrearAdministradorInicial_ConNombreVacio_Rechaza()
        {
            RepositorioFalso repository =
                new RepositorioFalso();

            PasswordHasherFalso hasher =
                new PasswordHasherFalso();

            InicializacionSeguridadService service =
                new InicializacionSeguridadService(
                    repository,
                    hasher);

            ResultadoInicializacionSeguridad resultado =
                service.CrearAdministradorInicial(
                    " ",
                    "Pérez",
                    "admin@sigevip.test",
                    "admin",
                    "Clave123",
                    "Clave123");

            Assert.IsFalse(resultado.Exitoso);
            Assert.AreEqual(
                InicializacionSeguridadService
                    .MensajeCamposObligatorios,
                resultado.Mensaje);
            Assert.IsFalse(hasher.CrearHashFueInvocado);
            Assert.IsFalse(repository.CrearFueInvocado);
        }

        [TestMethod]
        public void CrearAdministradorInicial_ConPasswordsDiferentes_Rechaza()
        {
            InicializacionSeguridadService service =
                new InicializacionSeguridadService(
                    new RepositorioFalso(),
                    new PasswordHasherFalso());

            ResultadoInicializacionSeguridad resultado =
                service.CrearAdministradorInicial(
                    "Ana",
                    "Pérez",
                    "admin@sigevip.test",
                    "admin",
                    "Clave123",
                    "OtraClave");

            Assert.IsFalse(resultado.Exitoso);
            Assert.AreEqual(
                InicializacionSeguridadService
                    .MensajePasswordsDiferentes,
                resultado.Mensaje);
        }

        [TestMethod]
        public void CrearAdministradorInicial_UsuarioExistente_NoGeneraHash()
        {
            RepositorioFalso repository =
                new RepositorioFalso();

            repository.UsuarioExiste = true;

            PasswordHasherFalso hasher =
                new PasswordHasherFalso();

            InicializacionSeguridadService service =
                new InicializacionSeguridadService(
                    repository,
                    hasher);

            ResultadoInicializacionSeguridad resultado =
                service.CrearAdministradorInicial(
                    "Ana",
                    "Pérez",
                    "admin@sigevip.test",
                    " ADMIN ",
                    "Clave123",
                    "Clave123");

            Assert.IsFalse(resultado.Exitoso);
            Assert.IsTrue(resultado.UsuarioExistente);
            Assert.AreEqual(
                "admin",
                resultado.NombreUsuario);
            Assert.IsFalse(hasher.CrearHashFueInvocado);
            Assert.IsFalse(repository.CrearFueInvocado);
        }

        [TestMethod]
        public void CrearAdministradorInicial_DatosValidos_NormalizaUsuario()
        {
            RepositorioFalso repository =
                new RepositorioFalso();

            InicializacionSeguridadService service =
                new InicializacionSeguridadService(
                    repository,
                    new PasswordHasherFalso());

            ResultadoInicializacionSeguridad resultado =
                service.CrearAdministradorInicial(
                    " Ana ",
                    " Pérez ",
                    " admin@sigevip.test ",
                    " ADMIN ",
                    "Clave123",
                    "Clave123");

            Assert.IsTrue(resultado.Exitoso);
            Assert.AreEqual(
                "admin",
                resultado.NombreUsuario);
            Assert.AreEqual(
                "admin",
                repository.NombreUsuarioRecibido);
            Assert.AreEqual(
                "Ana",
                repository.NombreRecibido);
            Assert.AreEqual(
                "Pérez",
                repository.ApellidoRecibido);
            Assert.AreEqual(
                "admin@sigevip.test",
                repository.EmailRecibido);
        }

        [TestMethod]
        public void CrearAdministradorInicial_DatosValidos_UsaPasswordHasher()
        {
            RepositorioFalso repository =
                new RepositorioFalso();

            PasswordHasherFalso hasher =
                new PasswordHasherFalso();

            InicializacionSeguridadService service =
                new InicializacionSeguridadService(
                    repository,
                    hasher);

            ResultadoInicializacionSeguridad resultado =
                service.CrearAdministradorInicial(
                    "Ana",
                    "Pérez",
                    "admin@sigevip.test",
                    "admin",
                    "Clave123",
                    "Clave123");

            Assert.IsTrue(resultado.Exitoso);
            Assert.IsTrue(hasher.CrearHashFueInvocado);
            Assert.AreEqual(
                "Clave123",
                hasher.PasswordRecibido);
            Assert.AreSame(
                hasher.Resultado,
                repository.PasswordHashRecibido);
        }

        [TestMethod]
        public void CrearAdministradorInicial_ConflictoDuranteInsercion_InformaExistente()
        {
            RepositorioFalso repository =
                new RepositorioFalso();

            repository.CreacionExitosa = false;

            InicializacionSeguridadService service =
                new InicializacionSeguridadService(
                    repository,
                    new PasswordHasherFalso());

            ResultadoInicializacionSeguridad resultado =
                service.CrearAdministradorInicial(
                    "Ana",
                    "Pérez",
                    "admin@sigevip.test",
                    "admin",
                    "Clave123",
                    "Clave123");

            Assert.IsFalse(resultado.Exitoso);
            Assert.IsTrue(resultado.UsuarioExistente);
        }

        private sealed class RepositorioFalso
            : IInicializacionSeguridadRepository
        {
            public RepositorioFalso()
            {
                CreacionExitosa = true;
            }

            public bool UsuarioExiste { get; set; }

            public bool CreacionExitosa { get; set; }

            public bool CrearFueInvocado { get; private set; }

            public string NombreRecibido { get; private set; }

            public string ApellidoRecibido { get; private set; }

            public string EmailRecibido { get; private set; }

            public string NombreUsuarioRecibido { get; private set; }

            public PasswordHashResult PasswordHashRecibido
            {
                get;
                private set;
            }

            public bool ExisteUsuario(
                string nombreUsuario)
            {
                NombreUsuarioRecibido =
                    nombreUsuario;

                return UsuarioExiste;
            }

            public bool CrearAdministradorInicial(
                string nombre,
                string apellido,
                string email,
                string nombreUsuario,
                PasswordHashResult passwordHash)
            {
                CrearFueInvocado = true;
                NombreRecibido = nombre;
                ApellidoRecibido = apellido;
                EmailRecibido = email;
                NombreUsuarioRecibido = nombreUsuario;
                PasswordHashRecibido = passwordHash;

                return CreacionExitosa;
            }
        }

        private sealed class PasswordHasherFalso
            : IPasswordHasher
        {
            public PasswordHasherFalso()
            {
                Resultado =
                    new PasswordHashResult(
                        new byte[] { 1, 2, 3, 4 },
                        new byte[] { 5, 6, 7, 8 },
                        100);
            }

            public bool CrearHashFueInvocado { get; private set; }

            public string PasswordRecibido { get; private set; }

            public PasswordHashResult Resultado { get; private set; }

            public PasswordHashResult CrearHash(
                string password)
            {
                CrearHashFueInvocado = true;
                PasswordRecibido = password;

                return Resultado;
            }

            public bool Verificar(
                string password,
                byte[] hashEsperado,
                byte[] salt,
                int iteraciones)
            {
                return false;
            }
        }
    }
}
