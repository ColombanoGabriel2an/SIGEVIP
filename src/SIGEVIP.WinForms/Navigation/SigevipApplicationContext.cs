using System;
using System.Windows.Forms;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Application.Visitas;
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

        private readonly ClienteService
            _clienteService;

        private readonly ViajeService
            _viajeService;

        private readonly VisitaService
            _visitaService;

        private readonly ViaticoService
            _viaticoService;

        private readonly RendicionService
            _rendicionService;

        private readonly ISesionActual
            _sesionActual;

        private LoginForm _loginForm;
        private MainForm _mainForm;
        private bool _finalizandoAplicacion;

        public SigevipApplicationContext(
            AutenticacionService autenticacionService,
            AutorizacionService autorizacionService,
            PerfilSesionService perfilSesionService,
            ClienteService clienteService,
            ViajeService viajeService,
            VisitaService visitaService,
            ViaticoService viaticoService,
            RendicionService rendicionService,
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

            _clienteService =
                clienteService
                ?? throw new ArgumentNullException(
                    nameof(clienteService));

            _viajeService =
                viajeService
                ?? throw new ArgumentNullException(
                    nameof(viajeService));

            _visitaService =
                visitaService
                ?? throw new ArgumentNullException(
                    nameof(visitaService));

            _viaticoService =
                viaticoService
                ?? throw new ArgumentNullException(
                    nameof(viaticoService));

            _rendicionService =
                rendicionService
                ?? throw new ArgumentNullException(
                    nameof(rendicionService));

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

            _mainForm.ClientesSolicitados +=
                MainForm_ClientesSolicitados;

            _mainForm.ViajesSolicitados +=
                MainForm_ViajesSolicitados;

            _mainForm.VisitasSolicitadas +=
                MainForm_VisitasSolicitadas;

            _mainForm.ViaticosSolicitados +=
                MainForm_ViaticosSolicitados;

            _mainForm.CerrarSesionSolicitada +=
                MainForm_CerrarSesionSolicitada;

            _mainForm.SalirSolicitado +=
                MainForm_SalirSolicitado;

            _mainForm.FormClosed +=
                MainForm_FormClosed;

            _mainForm.Show();
        }

        private void MainForm_ClientesSolicitados(
            object sender,
            EventArgs e)
        {
            if (_mainForm == null)
            {
                return;
            }

            using (
                var clientesForm =
                    new ClientesForm(
                        _clienteService,
                        _sesionActual,
                        _autorizacionService))
            {
                clientesForm.ShowDialog(
                    _mainForm);
            }
        }

        private void MainForm_ViajesSolicitados(
            object sender,
            EventArgs e)
        {
            if (_mainForm == null)
            {
                return;
            }

            using (
                var viajesForm =
                    new ViajesForm(
                        _viajeService,
                        _sesionActual,
                        _autorizacionService))
            {
                viajesForm.ShowDialog(
                    _mainForm);
            }
        }

        private void MainForm_VisitasSolicitadas(
            object sender,
            EventArgs e)
        {
            if (_mainForm == null)
            {
                return;
            }

            using (
                var visitasForm =
                    new VisitasForm(
                        _visitaService,
                        _viajeService,
                        _sesionActual,
                        _autorizacionService))
            {
                visitasForm.ShowDialog(
                    _mainForm);
            }
        }

        private void MainForm_ViaticosSolicitados(
            object sender,
            EventArgs e)
        {
            if (_mainForm == null)
            {
                return;
            }

            using (
                var formulario =
                    new ViaticosRendicionesForm(
                        _viaticoService,
                        _rendicionService,
                        _viajeService,
                        _sesionActual,
                        _autorizacionService))
            {
                formulario.ShowDialog(
                    _mainForm);
            }
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

            _mainForm.ClientesSolicitados -=
                MainForm_ClientesSolicitados;

            _mainForm.ViajesSolicitados -=
                MainForm_ViajesSolicitados;

            _mainForm.VisitasSolicitadas -=
                MainForm_VisitasSolicitadas;

            _mainForm.ViaticosSolicitados -=
                MainForm_ViaticosSolicitados;

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
