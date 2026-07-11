using Microsoft.VisualStudio.TestTools.UnitTesting;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.Tests.Application
{
    [TestClass]
    public class AutenticacionServiceTests
    {
        [TestMethod]
        public void Autenticar_ConNombreUsuarioVacio_RechazaAutenticacion()
        {
            AutenticacionService servicio =
                CrearServicio(null, "secreta");

            ResultadoAutenticacion resultado =
                servicio.Autenticar(" ", "secreta");

            Assert.IsFalse(resultado.Exitoso);
            Assert.IsNull(resultado.Usuario);
        }

        [TestMethod]
        public void Autenticar_ConPasswordVacio_RechazaAutenticacion()
        {
            AutenticacionService servicio =
                CrearServicio(CrearUsuario(), "secreta");

            ResultadoAutenticacion resultado =
                servicio.Autenticar("aperez", " ");

            Assert.IsFalse(resultado.Exitoso);
        }

        [TestMethod]
        public void Autenticar_ConUsuarioInexistente_RechazaAutenticacion()
        {
            AutenticacionService servicio =
                CrearServicio(null, "secreta");

            ResultadoAutenticacion resultado =
                servicio.Autenticar("aperez", "secreta");

            Assert.IsFalse(resultado.Exitoso);
            Assert.IsNull(resultado.Usuario);
        }

        [TestMethod]
        public void Autenticar_ConUsuarioInactivo_RechazaAutenticacion()
        {
            Usuario usuario = CrearUsuario();
            usuario.Desactivar();

            AutenticacionService servicio =
                CrearServicio(usuario, "secreta");

            ResultadoAutenticacion resultado =
                servicio.Autenticar("aperez", "secreta");

            Assert.IsFalse(resultado.Exitoso);
        }

        [TestMethod]
        public void Autenticar_ConPasswordIncorrecto_RechazaAutenticacion()
        {
            AutenticacionService servicio =
                CrearServicio(CrearUsuario(), "secreta");

            ResultadoAutenticacion resultado =
                servicio.Autenticar(
                    "aperez",
                    "incorrecta");

            Assert.IsFalse(resultado.Exitoso);
        }

        [TestMethod]
        public void Autenticar_ConCredencialesCorrectas_DevuelveUsuario()
        {
            Usuario usuario = CrearUsuario();

            AutenticacionService servicio =
                CrearServicio(usuario, "secreta");

            ResultadoAutenticacion resultado =
                servicio.Autenticar(
                    "  APEREZ  ",
                    "secreta");

            Assert.IsTrue(resultado.Exitoso);
            Assert.AreSame(usuario, resultado.Usuario);
        }

        [TestMethod]
        public void Autenticar_FallosDeUsuarioYPassword_UsanMismoMensajePublico()
        {
            AutenticacionService sinUsuario =
                CrearServicio(null, "secreta");

            AutenticacionService passwordIncorrecto =
                CrearServicio(CrearUsuario(), "secreta");

            ResultadoAutenticacion primerResultado =
                sinUsuario.Autenticar(
                    "aperez",
                    "secreta");

            ResultadoAutenticacion segundoResultado =
                passwordIncorrecto.Autenticar(
                    "aperez",
                    "incorrecta");

            Assert.AreEqual(
                primerResultado.Mensaje,
                segundoResultado.Mensaje);

            Assert.AreEqual(
                AutenticacionService.MensajeCredencialesInvalidas,
                primerResultado.Mensaje);
        }

        private static AutenticacionService CrearServicio(
            Usuario usuario,
            string passwordCorrecto)
        {
            return new AutenticacionService(
                new UsuarioRepositoryFalso(usuario),
                new PasswordHasherFalso(passwordCorrecto));
        }

        private static Usuario CrearUsuario()
        {
            return new Usuario(
                1,
                1,
                "aperez",
                new byte[] { 10, 20, 30, 40 },
                new byte[] { 50, 60, 70, 80 },
                100000);
        }

        private sealed class UsuarioRepositoryFalso
            : IUsuarioAutenticacionRepository
        {
            private readonly Usuario _usuario;

            public UsuarioRepositoryFalso(
                Usuario usuario)
            {
                _usuario = usuario;
            }

            public Usuario BuscarPorNombreUsuario(
                string nombreUsuario)
            {
                if (_usuario == null)
                {
                    return null;
                }

                return _usuario.NombreUsuario ==
                       nombreUsuario
                    ? _usuario
                    : null;
            }
        }

        private sealed class PasswordHasherFalso
            : IPasswordHasher
        {
            private readonly string _passwordCorrecto;

            public PasswordHasherFalso(
                string passwordCorrecto)
            {
                _passwordCorrecto = passwordCorrecto;
            }

            public PasswordHashResult CrearHash(
                string password)
            {
                return new PasswordHashResult(
                    new byte[] { 1 },
                    new byte[] { 2 },
                    1);
            }

            public bool Verificar(
                string password,
                byte[] hashEsperado,
                byte[] salt,
                int iteraciones)
            {
                return password == _passwordCorrecto;
            }
        }
    }
}
