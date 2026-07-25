using System;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AutenticacionService
            _autenticacionService;

        private readonly ISesionActual
            _sesionActual;

        public LoginForm(
            AutenticacionService autenticacionService,
            ISesionActual sesionActual)
        {
            _autenticacionService =
                autenticacionService
                ?? throw new ArgumentNullException(
                    nameof(autenticacionService));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            InitializeComponent();
        }

        public event EventHandler AutenticacionExitosa;

        private void LoginForm_Load(
            object sender,
            EventArgs e)
        {
            txtUsuario.Focus();
        }

        private void BtnIniciarSesion_Click(
            object sender,
            EventArgs e)
        {
            Autenticar();
        }

        private void BtnSalir_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        private void ChkMostrarPassword_CheckedChanged(
            object sender,
            EventArgs e)
        {
            txtPassword.UseSystemPasswordChar =
                !chkMostrarPassword.Checked;
        }

        private void Autenticar()
        {
            LimpiarEstado();

            string nombreUsuario =
                txtUsuario.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                nombreUsuario))
            {
                MostrarValidacion(
                    "Ingrese el nombre de usuario.");

                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(
                txtPassword.Text))
            {
                MostrarValidacion(
                    "Ingrese la contraseña.");

                txtPassword.Focus();
                return;
            }

            string password =
                txtPassword.Text;

            txtPassword.Clear();

            CambiarEstadoAutenticacion(true);

            try
            {
                ResultadoAutenticacion resultado =
                    _autenticacionService.Autenticar(
                        nombreUsuario,
                        password);

                if (!resultado.Exitoso)
                {
                    MostrarCredencialesInvalidas();
                    return;
                }

                _sesionActual.Iniciar(
                    resultado.Usuario);

                lblEstado.ForeColor =
                    Color.DarkGreen;

                lblEstado.Text =
                    "Autenticación correcta.";

                OnAutenticacionExitosa();
            }
            catch (PersistenciaException)
            {
                MostrarErrorTecnico(
                    "No fue posible conectar con SQL Server. " +
                    "Verifique que el servidor esté disponible " +
                    "e intente nuevamente.");
            }
            catch (Exception)
            {
                MostrarErrorTecnico(
                    "Ocurrió un error inesperado durante " +
                    "la autenticación. Intente nuevamente.");
            }
            finally
            {
                password = null;

                CambiarEstadoAutenticacion(false);
            }
        }

        private void MostrarValidacion(
            string mensaje)
        {
            lblEstado.ForeColor =
                Color.DarkRed;

            lblEstado.Text = mensaje;
        }

        private void MostrarCredencialesInvalidas()
        {
            lblEstado.ForeColor =
                Color.DarkRed;

            lblEstado.Text =
                AutenticacionService
                    .MensajeCredencialesInvalidas;

            txtPassword.Focus();
        }

        private void MostrarErrorTecnico(
            string mensaje)
        {
            lblEstado.ForeColor =
                Color.DarkRed;

            lblEstado.Text = mensaje;

            MessageBox.Show(
                mensaje,
                "Error de autenticación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            txtPassword.Focus();
        }

        private void LimpiarEstado()
        {
            lblEstado.ForeColor =
                SystemColors.ControlText;

            lblEstado.Text = string.Empty;
        }

        private void CambiarEstadoAutenticacion(
            bool autenticando)
        {
            btnIniciarSesion.Enabled =
                !autenticando;

            btnSalir.Enabled =
                !autenticando;

            txtUsuario.Enabled =
                !autenticando;

            txtPassword.Enabled =
                !autenticando;

            chkMostrarPassword.Enabled =
                !autenticando;

            UseWaitCursor = autenticando;
        }

        private void OnAutenticacionExitosa()
        {
            EventHandler handler =
                AutenticacionExitosa;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }
    }
}