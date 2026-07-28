using System;
using System.Configuration;
using System.Windows.Forms;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Usuarios;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Infrastructure.Clientes;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Rendiciones;
using SIGEVIP.Infrastructure.Security;
using SIGEVIP.Infrastructure.Usuarios;
using SIGEVIP.Infrastructure.Viajes;
using SIGEVIP.Infrastructure.Viaticos;
using SIGEVIP.Infrastructure.Visitas;
using SIGEVIP.WinForms.Navigation;

namespace SIGEVIP.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            System.Windows.Forms.Application
                .EnableVisualStyles();

            System.Windows.Forms.Application
                .SetCompatibleTextRenderingDefault(false);

            try
            {
                ConnectionStringSettings settings =
                    ConfigurationManager
                        .ConnectionStrings["SIGEVIP"];

                if (settings == null ||
                    string.IsNullOrWhiteSpace(
                        settings.ConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "No se encontró la cadena de conexión SIGEVIP.");
                }

                var connectionFactory =
                    new SqlConnectionFactory(
                        settings.ConnectionString);

                var passwordHasher =
                    new Pbkdf2PasswordHasher();

                var usuarioRepository =
                    new UsuarioAutenticacionRepository(
                        connectionFactory);

                var perfilSesionRepository =
                    new PerfilSesionRepository(
                        connectionFactory);

                var usuarioGestionRepository =
                    new UsuarioGestionRepository(
                        connectionFactory);

                var clienteRepository =
                    new ClienteRepository(
                        connectionFactory);

                var personaConsultaRepository =
                    new PersonaConsultaRepository(
                        connectionFactory);

                var viajeRepository =
                    new ViajeRepository(
                        connectionFactory);

                var visitaRepository =
                    new VisitaRepository(
                        connectionFactory);

                var clienteConsultaVisitaRepository =
                    new ClienteConsultaVisitaRepository(
                        connectionFactory);

                var personaConsultaViaticoRepository =
                    new PersonaConsultaViaticoRepository(
                        connectionFactory);

                var viaticoRepository =
                    new ViaticoRepository(
                        connectionFactory);

                var rendicionRepository =
                    new RendicionRepository(
                        connectionFactory);

                var autenticacionService =
                    new AutenticacionService(
                        usuarioRepository,
                        passwordHasher);

                var perfilSesionService =
                    new PerfilSesionService(
                        perfilSesionRepository);

                var autorizacionService =
                    new AutorizacionService();

                ISesionActual sesionActual =
                    new SesionActual();

                var usuarioGestionService =
                    new UsuarioGestionService(
                        usuarioGestionRepository,
                        passwordHasher,
                        sesionActual,
                        autorizacionService);

                var clienteService =
                    new ClienteService(
                        clienteRepository,
                        sesionActual,
                        autorizacionService);

                var viajeService =
                    new ViajeService(
                        viajeRepository,
                        personaConsultaRepository,
                        sesionActual,
                        autorizacionService);

                var visitaService =
                    new VisitaService(
                        visitaRepository,
                        clienteConsultaVisitaRepository,
                        viajeRepository,
                        sesionActual,
                        autorizacionService);

                var viaticoService =
                    new ViaticoService(
                        viaticoRepository,
                        viajeRepository,
                        personaConsultaViaticoRepository,
                        sesionActual,
                        autorizacionService);

                var rendicionService =
                    new RendicionService(
                        rendicionRepository,
                        sesionActual,
                        autorizacionService);

                var applicationContext =
                    new SigevipApplicationContext(
                        autenticacionService,
                        autorizacionService,
                        perfilSesionService,
                        clienteService,
                        viajeService,
                        visitaService,
                        viaticoService,
                        rendicionService,
                        usuarioGestionService,
                        sesionActual);

                System.Windows.Forms.Application.Run(
                    applicationContext);
            }
            catch (ConfigurationErrorsException exception)
            {
                MostrarErrorInicio(
                    exception.Message);
            }
            catch (Exception)
            {
                MostrarErrorInicio(
                    "No fue posible iniciar SIGEVIP. " +
                    "Verifique la configuración de la aplicación.");
            }
        }

        private static void MostrarErrorInicio(
            string mensaje)
        {
            MessageBox.Show(
                mensaje,
                "Error de inicio",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
