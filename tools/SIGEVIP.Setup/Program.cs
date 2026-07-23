using System;
using System.Configuration;
using System.Text;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.Infrastructure.Security;

namespace SIGEVIP.Setup
{
    internal static class Program
    {
        private const int CodigoExito = 0;
        private const int CodigoErrorConfiguracion = 1;
        private const int CodigoErrorValidacion = 2;
        private const int CodigoUsuarioExistente = 3;
        private const int CodigoErrorPersistencia = 4;
        private const int CodigoErrorInesperado = 5;

        private static int Main()
        {
            Console.Title =
                "SIGEVIP - Configuración inicial";

            MostrarEncabezado();

            try
            {
                string connectionString =
                    ObtenerConnectionString();

                InicializacionSeguridadService service =
                    CrearServicio(
                        connectionString);

                DatosAdministrador datos =
                    SolicitarDatos();

                ResultadoInicializacionSeguridad resultado =
                    service.CrearAdministradorInicial(
                        datos.Nombre,
                        datos.Apellido,
                        datos.Email,
                        datos.NombreUsuario,
                        datos.Password,
                        datos.ConfirmacionPassword);

                if (resultado.Exitoso)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        resultado.Mensaje);

                    Console.WriteLine(
                        "Usuario creado: " +
                        resultado.NombreUsuario);

                    Console.WriteLine(
                        "Grupo asignado: ADMINISTRADOR_GENERAL");

                    return CodigoExito;
                }

                Console.WriteLine();
                Console.WriteLine(
                    resultado.Mensaje);

                if (resultado.UsuarioExistente)
                {
                    Console.WriteLine(
                        "No se creó una nueva Persona ni se modificó la cuenta existente.");

                    return CodigoUsuarioExistente;
                }

                return CodigoErrorValidacion;
            }
            catch (ConfigurationErrorsException exception)
            {
                MostrarError(
                    "La configuración de la aplicación es inválida.",
                    exception);

                return CodigoErrorConfiguracion;
            }
            catch (PersistenciaException exception)
            {
                MostrarError(
                    "No fue posible completar la inicialización en SQL Server.",
                    exception);

                return CodigoErrorPersistencia;
            }
            catch (Exception exception)
            {
                MostrarError(
                    "Ocurrió un error inesperado durante la inicialización.",
                    exception);

                return CodigoErrorInesperado;
            }
        }

        private static InicializacionSeguridadService CrearServicio(
            string connectionString)
        {
            SqlConnectionFactory connectionFactory =
                new SqlConnectionFactory(
                    connectionString);

            InicializacionSeguridadRepository repository =
                new InicializacionSeguridadRepository(
                    connectionFactory);

            Pbkdf2PasswordHasher passwordHasher =
                new Pbkdf2PasswordHasher();

            return new InicializacionSeguridadService(
                repository,
                passwordHasher);
        }

        private static string ObtenerConnectionString()
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

            return settings.ConnectionString;
        }

        private static DatosAdministrador SolicitarDatos()
        {
            Console.WriteLine(
                "Ingrese los datos del administrador inicial.");

            Console.WriteLine();

            string nombre =
                SolicitarTexto(
                    "Nombre");

            string apellido =
                SolicitarTexto(
                    "Apellido");

            string email =
                SolicitarTexto(
                    "Email");

            string nombreUsuario =
                SolicitarTexto(
                    "Nombre de usuario",
                    "admin");

            string password =
                SolicitarPassword(
                    "Contraseña");

            string confirmacionPassword =
                SolicitarPassword(
                    "Confirmación de contraseña");

            return new DatosAdministrador(
                nombre,
                apellido,
                email,
                nombreUsuario,
                password,
                confirmacionPassword);
        }

        private static string SolicitarTexto(
            string etiqueta)
        {
            return SolicitarTexto(
                etiqueta,
                null);
        }

        private static string SolicitarTexto(
            string etiqueta,
            string sugerencia)
        {
            if (string.IsNullOrWhiteSpace(
                sugerencia))
            {
                Console.Write(
                    etiqueta +
                    ": ");
            }
            else
            {
                Console.Write(
                    etiqueta +
                    " [" +
                    sugerencia +
                    "]: ");
            }

            string valor =
                Console.ReadLine();

            if (string.IsNullOrWhiteSpace(valor) &&
                !string.IsNullOrWhiteSpace(sugerencia))
            {
                return sugerencia;
            }

            return valor;
        }

        private static string SolicitarPassword(
            string etiqueta)
        {
            Console.Write(
                etiqueta +
                ": ");

            StringBuilder password =
                new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo tecla =
                    Console.ReadKey(true);

                if (tecla.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return password.ToString();
                }

                if (tecla.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Length--;

                        Console.Write(
                            "\b \b");
                    }

                    continue;
                }

                if (char.IsControl(
                    tecla.KeyChar))
                {
                    continue;
                }

                password.Append(
                    tecla.KeyChar);

                Console.Write("*");
            }
        }

        private static void MostrarEncabezado()
        {
            Console.WriteLine(
                "SIGEVIP - Configuración inicial de seguridad");

            Console.WriteLine(
                "==========================================");

            Console.WriteLine();

            Console.WriteLine(
                "Esta utilidad crea el primer administrador del sistema.");

            Console.WriteLine(
                "La contraseña no se mostrará ni se almacenará en texto plano.");

            Console.WriteLine();
        }

        private static void MostrarError(
            string mensaje,
            Exception exception)
        {
            Console.WriteLine();
            Console.WriteLine(
                mensaje);

            if (exception != null &&
                !string.IsNullOrWhiteSpace(
                    exception.Message))
            {
                Console.WriteLine(
                    "Detalle técnico: " +
                    exception.Message);
            }

            Console.WriteLine(
                "No se mostraron credenciales, hash ni salt.");
        }

        private sealed class DatosAdministrador
        {
            public DatosAdministrador(
                string nombre,
                string apellido,
                string email,
                string nombreUsuario,
                string password,
                string confirmacionPassword)
            {
                Nombre = nombre;
                Apellido = apellido;
                Email = email;
                NombreUsuario = nombreUsuario;
                Password = password;
                ConfirmacionPassword =
                    confirmacionPassword;
            }

            public string Nombre { get; private set; }

            public string Apellido { get; private set; }

            public string Email { get; private set; }

            public string NombreUsuario { get; private set; }

            public string Password { get; private set; }

            public string ConfirmacionPassword
            {
                get;
                private set;
            }
        }
    }
}
