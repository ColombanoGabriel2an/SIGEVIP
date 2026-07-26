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

        private readonly PerfilSesionService
            _perfilSesionService;

        private readonly ISesionActual
            _sesionActual;

        private LoginForm _loginForm;
        private MainForm _mainForm;
        private bool _finalizandoAplicacion;

        public SigevipApplicationContext(
            AutenticacionService autenticacionService,
            AutorizacionService autorizacionService,
            PerfilSesionService perfilSesionService,
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

            _perfilSesionService =
                perfilSesionService
                ?? throw new ArgumentNullException(
                    nameof(perfilSesionService));

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

            PerfilSesion perfil;

            try
            {
                perfil =
                    _perfilSesionService
                        .ObtenerPorIdPersona(
                            _sesionActual
                                .UsuarioActual
                                .IdPersona);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "No fue posible recuperar los datos " +
                    "de la persona autenticada.",
                    "Error de sesión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _sesionActual.Cerrar();
                return;
            }

            if (perfil == null)
            {
                MessageBox.Show(
                    "No se encontró la persona asociada " +
                    "al usuario autenticado.",
                    "Error de sesión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                _sesionActual.Cerrar();
                return;
            }

            CerrarLoginSinFinalizarAplicacion();
            MostrarMenuPrincipal(perfil);
        }

        private void LoginForm_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            if (_finalizandoAplicacion)
            {
                return;
            }

            FinalizarAplicacion();
        }

        private void MostrarMenuPrincipal(
            PerfilSesion perfil)
        {
            _mainForm =
                new MainForm(
                    _sesionActual,
                    _autorizacionService,
                    perfil);

            _mainForm.CerrarSesionSolicitada +=
                MainForm_CerrarSesionSolicitada;

            _mainForm.SalirSolicitado +=
                MainForm_SalirSolicitado;

            _mainForm.FormClosed +=
                MainForm_FormClosed;

            _mainForm.Show();
        }

        private void MainForm_CerrarSesionSolicitada(
            object sender,
            EventArgs e)
        {
            CerrarMenuSinFinalizarAplicacion();

            _sesionActual.Cerrar();

            MostrarLogin();
        }

        private void MainForm_SalirSolicitado(
            object sender,
            EventArgs e)
        {
            FinalizarAplicacion();
        }

        private void MainForm_FormClosed(
            object sender,
            FormClosedEventArgs e)
        {
            if (_finalizandoAplicacion)
            {
                return;
            }

            FinalizarAplicacion();
        }

        private void CerrarLoginSinFinalizarAplicacion()
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
        }

        private void CerrarMenuSinFinalizarAplicacion()
        {
            if (_mainForm == null)
            {
                return;
            }

            _mainForm.CerrarSesionSolicitada -=
                MainForm_CerrarSesionSolicitada;

            _mainForm.SalirSolicitado -=
                MainForm_SalirSolicitado;

            _mainForm.FormClosed -=
                MainForm_FormClosed;

            _mainForm.Close();
            _mainForm.Dispose();
            _mainForm = null;
        }

        private void FinalizarAplicacion()
        {
            if (_finalizandoAplicacion)
            {
                return;
            }

            _finalizandoAplicacion = true;

            CerrarLoginSinFinalizarAplicacion();
            CerrarMenuSinFinalizarAplicacion();

            _sesionActual.Cerrar();

            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            _sesionActual.Cerrar();

            base.ExitThreadCore();
        }
    }
}
