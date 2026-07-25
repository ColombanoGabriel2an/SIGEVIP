using System;
using System.Windows.Forms;
using SIGEVIP.Application.Security;
using SIGEVIP.WinForms.Forms;

namespace SIGEVIP.WinForms.Navigation
{
    public sealed class SigevipApplicationContext
        : ApplicationContext
    {
        private readonly AutenticacionService
            _autenticacionService;

        private readonly AutorizacionService
            _autorizacionService;

        private readonly ISesionActual
            _sesionActual;

        private LoginForm _loginForm;

        public SigevipApplicationContext(
            AutenticacionService autenticacionService,
            AutorizacionService autorizacionService,
            ISesionActual sesionActual)
        {
            _autenticacionService =
                autenticacionService
                ?? throw new ArgumentNullException(
                    nameof(autenticacionService));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));

            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            MostrarLogin();
        }

        private void MostrarLogin()
        {
            _loginForm =
                new LoginForm(
                    _autenticacionService,
                    _sesionActual);

            _loginForm.AutenticacionExitosa +=
                LoginForm_AutenticacionExitosa;

            _loginForm.FormClosed +=
                LoginForm_FormClosed;

            MainForm = _loginForm;

            _loginForm.Show();
        }

        private void LoginForm_AutenticacionExitosa(
            object sender,
            EventArgs e)
        {
            if (!_sesionActual.HayUsuarioAutenticado)
            {
                MessageBox.Show(
                    "No fue posible establecer la sesión.",
                    "Error de sesión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Autenticación correcta. " +
                "El menú principal se incorporará " +
                "en el siguiente incremento.",
                "SIGEVIP",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CerrarLoginSinProcesarCierre();

            _sesionActual.Cerrar();

            ExitThread();
        }

        private void LoginForm_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            _sesionActual.Cerrar();

            ExitThread();
        }

        private void CerrarLoginSinProcesarCierre()
        {
            if (_loginForm == null)
            {
                return;
            }

            _loginForm.AutenticacionExitosa -=
                LoginForm_AutenticacionExitosa;

            _loginForm.FormClosed -=
                LoginForm_FormClosed;

            _loginForm.Close();
            _loginForm.Dispose();
            _loginForm = null;
            MainForm = null;
        }

        protected override void ExitThreadCore()
        {
            _sesionActual.Cerrar();

            base.ExitThreadCore();
        }
    }
}