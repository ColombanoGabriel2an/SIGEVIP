using System;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class CambiarClaveForm : Form
    {
        private readonly CambiarClaveService
            _cambiarClaveService;

        private TextBox _txtClaveActual;
        private TextBox _txtClaveNueva;
        private TextBox _txtConfirmacion;
        private Button _btnGuardar;
        private Button _btnCancelar;

        public CambiarClaveForm(
            CambiarClaveService cambiarClaveService)
        {
            _cambiarClaveService =
                cambiarClaveService
                ?? throw new ArgumentNullException(
                    nameof(cambiarClaveService));

            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            Text =
                "Cambiar clave";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox =
                false;

            MinimizeBox =
                false;

            ShowInTaskbar =
                false;

            ClientSize =
                new Size(
                    470,
                    330);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            var panelPrincipal =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 6,
                    Padding =
                        new Padding(24),
                    BackColor =
                        Color.WhiteSmoke
                };

            panelPrincipal.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    165F));

            panelPrincipal.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            panelPrincipal.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    58F));

            panelPrincipal.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    48F));

            panelPrincipal.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    48F));

            panelPrincipal.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    48F));

            panelPrincipal.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            panelPrincipal.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    48F));

            var lblTitulo =
                new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            15F,
                            FontStyle.Bold),
                    Text =
                        "Cambiar contraseña",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            panelPrincipal.Controls.Add(
                lblTitulo,
                0,
                0);

            panelPrincipal.SetColumnSpan(
                lblTitulo,
                2);

            panelPrincipal.Controls.Add(
                CrearEtiqueta(
                    "Contraseña actual:"),
                0,
                1);

            _txtClaveActual =
                CrearCampoClave();

            panelPrincipal.Controls.Add(
                _txtClaveActual,
                1,
                1);

            panelPrincipal.Controls.Add(
                CrearEtiqueta(
                    "Contraseña nueva:"),
                0,
                2);

            _txtClaveNueva =
                CrearCampoClave();

            panelPrincipal.Controls.Add(
                _txtClaveNueva,
                1,
                2);

            panelPrincipal.Controls.Add(
                CrearEtiqueta(
                    "Confirmar contraseña:"),
                0,
                3);

            _txtConfirmacion =
                CrearCampoClave();

            panelPrincipal.Controls.Add(
                _txtConfirmacion,
                1,
                3);

            var lblAyuda =
                new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    ForeColor =
                        Color.DimGray,
                    Text =
                        "La nueva contraseña debe contener " +
                        "al menos 8 caracteres. " +
                        "Después del cambio deberá iniciar sesión nuevamente.",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            panelPrincipal.Controls.Add(
                lblAyuda,
                0,
                4);

            panelPrincipal.SetColumnSpan(
                lblAyuda,
                2);

            var panelBotones =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection =
                        FlowDirection.RightToLeft,
                    WrapContents =
                        false
                };

            _btnGuardar =
                new Button
                {
                    AutoSize =
                        true,
                    MinimumSize =
                        new Size(
                            110,
                            34),
                    Text =
                        "Guardar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnCancelar =
                new Button
                {
                    AutoSize =
                        true,
                    MinimumSize =
                        new Size(
                            110,
                            34),
                    Text =
                        "Cancelar",
                    DialogResult =
                        DialogResult.Cancel,
                    UseVisualStyleBackColor =
                        true
                };

            _btnGuardar.Click +=
                BtnGuardar_Click;

            panelBotones.Controls.Add(
                _btnGuardar);

            panelBotones.Controls.Add(
                _btnCancelar);

            panelPrincipal.Controls.Add(
                panelBotones,
                0,
                5);

            panelPrincipal.SetColumnSpan(
                panelBotones,
                2);

            Controls.Add(
                panelPrincipal);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private static Label CrearEtiqueta(
            string texto)
        {
            return new Label
            {
                AutoSize =
                    false,
                Dock =
                    DockStyle.Fill,
                Text =
                    texto,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private static TextBox CrearCampoClave()
        {
            return new TextBox
            {
                Dock =
                    DockStyle.Fill,
                Margin =
                    new Padding(
                        4,
                        10,
                        4,
                        8),
                UseSystemPasswordChar =
                    true,
                MaxLength =
                    200
            };
        }

        private void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                CambiarClaveCommand command =
                    new CambiarClaveCommand(
                        _txtClaveActual.Text,
                        _txtClaveNueva.Text,
                        _txtConfirmacion.Text);

                _cambiarClaveService.Cambiar(
                    command);

                MessageBox.Show(
                    this,
                    "La contraseña fue modificada correctamente. " +
                    "Debe iniciar sesión nuevamente.",
                    "Cambio de contraseña",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (AccesoDenegadoException exception)
            {
                MostrarAdvertencia(
                    exception.Message);
            }
            catch (ReglaNegocioException exception)
            {
                MostrarAdvertencia(
                    exception.Message);
            }
            catch (PersistenciaException)
            {
                MostrarError(
                    "No fue posible actualizar la contraseña. " +
                    "Intente nuevamente.");
            }
            catch (Exception)
            {
                MostrarError(
                    "Ocurrió un error inesperado al cambiar la contraseña.");
            }
        }

        private void MostrarAdvertencia(
            string mensaje)
        {
            MessageBox.Show(
                this,
                mensaje,
                "Validación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void MostrarError(
            string mensaje)
        {
            MessageBox.Show(
                this,
                mensaje,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
