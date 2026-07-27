using System;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viaticos;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ViaticosRendicionesForm
        : Form
    {
        private readonly ViaticoService
            _viaticoService;

        private readonly RendicionService
            _rendicionService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private Button _btnGestionViaticos;
        private Button _btnRendiciones;
        private Label _lblEstado;

        public ViaticosRendicionesForm(
            ViaticoService viaticoService,
            RendicionService rendicionService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
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

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));

            InicializarFormulario();
            ConfigurarPermisos();
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Viáticos y rendiciones";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(
                    760,
                    480);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        18F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            28,
                            24),
                    Text =
                        "Viáticos y rendiciones"
                });

            Controls.Add(
                new Label
                {
                    AutoSize = false,
                    Location =
                        new Point(
                            31,
                            70),
                    Size =
                        new Size(
                            690,
                            42),
                    Text =
                        "Seleccione una opción. Las funciones disponibles dependen de los permisos del usuario autenticado."
                });

            _btnGestionViaticos =
                CrearBotonModulo(
                    "Gestión de viáticos",
                    "Consultar, registrar y modificar gastos asociados a un viaje.",
                    32);

            _btnRendiciones =
                CrearBotonModulo(
                    "Rendiciones pendientes",
                    "Revisar gastos, ajustar anticipos, aprobar o cancelar rendiciones.",
                    382);

            _btnGestionViaticos.Click +=
                BtnGestionViaticos_Click;

            _btnRendiciones.Click +=
                BtnRendiciones_Click;

            _lblEstado =
                new Label
                {
                    AutoSize = false,
                    BackColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Location =
                        new Point(
                            32,
                            315),
                    Padding =
                        new Padding(12),
                    Size =
                        new Size(
                            690,
                            70),
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            var btnCerrar =
                new Button
                {
                    DialogResult =
                        DialogResult.Cancel,
                    Location =
                        new Point(
                            592,
                            408),
                    Size =
                        new Size(
                            130,
                            36),
                    Text =
                        "Cerrar",
                    UseVisualStyleBackColor =
                        true
                };

            Controls.Add(
                _btnGestionViaticos);

            Controls.Add(
                _btnRendiciones);

            Controls.Add(
                _lblEstado);

            Controls.Add(
                btnCerrar);

            CancelButton =
                btnCerrar;
        }

        private static Button CrearBotonModulo(
            string titulo,
            string descripcion,
            int posicionX)
        {
            return new Button
            {
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold),
                Location =
                    new Point(
                        posicionX,
                        130),
                Size =
                    new Size(
                        340,
                        150),
                Text =
                    titulo +
                    Environment.NewLine +
                    Environment.NewLine +
                    descripcion,
                TextAlign =
                    ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor =
                    true
            };
        }

        private void ConfigurarPermisos()
        {
            bool puedeGestionarViaticos =
                TieneAlgunPermiso(
                    ViaticoService.PermisoConsultar,
                    ViaticoService.PermisoRegistrar,
                    ViaticoService.PermisoModificar,
                    RendicionService.PermisoEnviar);

            bool puedeRevisarRendiciones =
                TieneAlgunPermiso(
                    RendicionService.PermisoRevisar,
                    RendicionService.PermisoExcluirViatico,
                    RendicionService.PermisoReactivarViatico,
                    RendicionService.PermisoAjustarAnticipo,
                    RendicionService.PermisoAprobar,
                    RendicionService.PermisoCancelar);

            _btnGestionViaticos.Visible =
                puedeGestionarViaticos;

            _btnGestionViaticos.Enabled =
                puedeGestionarViaticos;

            _btnRendiciones.Visible =
                puedeRevisarRendiciones;

            _btnRendiciones.Enabled =
                puedeRevisarRendiciones;

            int cantidad =
                (puedeGestionarViaticos ? 1 : 0) +
                (puedeRevisarRendiciones ? 1 : 0);

            _lblEstado.Text =
                cantidad == 0
                    ? "El usuario no posee permisos operativos para este módulo."
                    : "Opciones disponibles según permisos: " +
                      cantidad +
                      ".";
        }

        private bool TieneAlgunPermiso(
            params string[] codigos)
        {
            if (!_sesionActual.HayUsuarioAutenticado ||
                _sesionActual.UsuarioActual == null)
            {
                return false;
            }

            foreach (
                string codigo
                in codigos)
            {
                if (_autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        codigo))
                {
                    return true;
                }
            }

            return false;
        }

        private void BtnGestionViaticos_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "La composición del servicio de viáticos está activa. " +
                "La pantalla de consulta y carga se incorporará en el siguiente incremento.",
                "Gestión de viáticos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnRendiciones_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "La composición del servicio de rendiciones está activa. " +
                "La pantalla de revisión se incorporará en el siguiente incremento.",
                "Rendiciones",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
