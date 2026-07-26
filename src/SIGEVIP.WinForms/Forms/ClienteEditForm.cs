using System;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ClienteEditForm : Form
    {
        private readonly ClienteService
            _clienteService;

        private readonly Cliente
            _cliente;

        private readonly ErrorProvider
            _errorProvider;

        private TextBox _txtRazonSocial;
        private TextBox _txtCuit;
        private TextBox _txtEmail;
        private TextBox _txtTelefono;
        private TextBox _txtLocalidad;
        private TextBox _txtProvincia;

        private Button _btnGuardar;
        private Button _btnCancelar;

        private bool EsEdicion
        {
            get
            {
                return _cliente != null;
            }
        }

        public ClienteEditForm(
            ClienteService clienteService,
            Cliente cliente)
        {
            _clienteService =
                clienteService
                ?? throw new ArgumentNullException(
                    nameof(clienteService));

            _cliente = cliente;

            _errorProvider =
                new ErrorProvider();

            InicializarFormulario();
            CargarDatos();
        }

        private void InicializarFormulario()
        {
            Text =
                EsEdicion
                    ? "Modificar cliente"
                    : "Nuevo cliente";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(560, 490);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            _errorProvider.ContainerControl =
                this;

            var lblTitulo =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        16F,
                        FontStyle.Bold),
                    Location =
                        new Point(28, 22),
                    Text =
                        EsEdicion
                            ? "Modificar cliente"
                            : "Nuevo cliente"
                };

            _txtRazonSocial =
                CrearCampo(
                    "Razón social",
                    82,
                    150);

            _txtCuit =
                CrearCampo(
                    "CUIT",
                    138,
                    20);

            _txtEmail =
                CrearCampo(
                    "Email",
                    194,
                    150);

            _txtTelefono =
                CrearCampo(
                    "Teléfono",
                    250,
                    50);

            _txtLocalidad =
                CrearCampo(
                    "Localidad",
                    306,
                    100);

            _txtProvincia =
                CrearCampo(
                    "Provincia",
                    362,
                    100);

            _btnGuardar =
                new Button
                {
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(285, 424),
                    Size =
                        new Size(116, 36),
                    Text =
                        "Guardar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnCancelar =
                new Button
                {
                    Location =
                        new Point(411, 424),
                    Size =
                        new Size(116, 36),
                    Text =
                        "Cancelar",
                    DialogResult =
                        DialogResult.Cancel,
                    UseVisualStyleBackColor =
                        true
                };

            _btnGuardar.Click +=
                BtnGuardar_Click;

            Controls.Add(lblTitulo);
            Controls.Add(_btnGuardar);
            Controls.Add(_btnCancelar);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private TextBox CrearCampo(
            string etiqueta,
            int posicionY,
            int longitudMaxima)
        {
            var label =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(30, posicionY),
                    Text =
                        etiqueta
                };

            var textBox =
                new TextBox
                {
                    Font = new Font(
                        "Segoe UI",
                        10F),
                    Location =
                        new Point(165, posicionY - 4),
                    MaxLength =
                        longitudMaxima,
                    Size =
                        new Size(362, 25)
                };

            Controls.Add(label);
            Controls.Add(textBox);

            return textBox;
        }

        private void CargarDatos()
        {
            if (!EsEdicion)
            {
                return;
            }

            _txtRazonSocial.Text =
                _cliente.RazonSocial;

            _txtCuit.Text =
                _cliente.Cuit;

            _txtEmail.Text =
                _cliente.Email;

            _txtTelefono.Text =
                _cliente.Telefono;

            _txtLocalidad.Text =
                _cliente.Localidad;

            _txtProvincia.Text =
                _cliente.Provincia;
        }

        private void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            LimpiarErrores();

            if (!ValidarCampos())
            {
                return;
            }

            CambiarEstado(true);

            try
            {
                if (EsEdicion)
                {
                    _clienteService.Modificar(
                        _cliente.IdCliente,
                        _txtRazonSocial.Text,
                        _txtCuit.Text,
                        _txtEmail.Text,
                        _txtTelefono.Text,
                        _txtLocalidad.Text,
                        _txtProvincia.Text);
                }
                else
                {
                    _clienteService.Registrar(
                        _txtRazonSocial.Text,
                        _txtCuit.Text,
                        _txtEmail.Text,
                        _txtTelefono.Text,
                        _txtLocalidad.Text,
                        _txtProvincia.Text);
                }

                MessageBox.Show(
                    EsEdicion
                        ? "El cliente fue modificado correctamente."
                        : "El cliente fue registrado correctamente.",
                    "Clientes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (ReglaNegocioException exception)
            {
                MostrarError(
                    exception.Message,
                    "Validación");
            }
            catch (AccesoDenegadoException exception)
            {
                MostrarError(
                    exception.Message,
                    "Acceso denegado");
            }
            catch (PersistenciaException)
            {
                MostrarError(
                    "No fue posible guardar el cliente. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia");
            }
            catch (Exception)
            {
                MostrarError(
                    "Ocurrió un error inesperado al guardar el cliente.",
                    "Error");
            }
            finally
            {
                CambiarEstado(false);
            }
        }

        private bool ValidarCampos()
        {
            bool valido = true;

            if (string.IsNullOrWhiteSpace(
                _txtRazonSocial.Text))
            {
                _errorProvider.SetError(
                    _txtRazonSocial,
                    "La razón social es obligatoria.");

                valido = false;
            }

            if (string.IsNullOrWhiteSpace(
                _txtCuit.Text))
            {
                _errorProvider.SetError(
                    _txtCuit,
                    "El CUIT es obligatorio.");

                valido = false;
            }

            if (!valido)
            {
                MessageBox.Show(
                    "Complete los campos obligatorios.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return valido;
        }

        private void LimpiarErrores()
        {
            _errorProvider.Clear();
        }

        private void CambiarEstado(
            bool guardando)
        {
            _btnGuardar.Enabled =
                !guardando;

            _btnCancelar.Enabled =
                !guardando;

            UseWaitCursor =
                guardando;
        }

        private static void MostrarError(
            string mensaje,
            string titulo)
        {
            MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _errorProvider.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
