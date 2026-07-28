using System;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Permisos;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class PermisoEditForm : Form
    {
        private readonly PermisoGestionService
            _permisoService;

        private readonly PermisoDetalleDto
            _permiso;

        private readonly ErrorProvider
            _errorProvider;

        private TextBox _txtCodigo;
        private TextBox _txtNombre;
        private TextBox _txtDescripcion;

        private Label _lblEstado;
        private Label _lblCantidadGrupos;

        private Button _btnGuardar;
        private Button _btnCancelar;

        private bool EsEdicion
        {
            get
            {
                return _permiso != null;
            }
        }

        public PermisoEditForm(
            PermisoGestionService permisoService,
            PermisoDetalleDto permiso)
        {
            _permisoService =
                permisoService
                ?? throw new ArgumentNullException(
                    nameof(permisoService));

            _permiso =
                permiso;

            _errorProvider =
                new ErrorProvider();

            InicializarFormulario();

            Load +=
                PermisoEditForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                EsEdicion
                    ? "Modificar permiso"
                    : "Nuevo permiso";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(
                    760,
                    500);

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
                        new Point(
                            28,
                            22),
                    Text =
                        EsEdicion
                            ? "Modificar permiso"
                            : "Nuevo permiso"
                };

            var lblAclaracion =
                new Label
                {
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            31,
                            60),
                    Size =
                        new Size(
                            690,
                            52),
                    Text =
                        EsEdicion
                            ? "El código identifica una autorización técnica " +
                              "y no puede modificarse. Los cambios se reflejarán " +
                              "en una nueva autenticación."
                            : "Ingrese un código técnico estable. Se normalizará " +
                              "a mayúsculas, sin acentos y con guiones bajos."
                };

            _txtCodigo =
                CrearCampoTexto(
                    "Código",
                    135,
                    100);

            _txtNombre =
                CrearCampoTexto(
                    "Nombre",
                    191,
                    150);

            CrearCampoDescripcion();

            _lblEstado =
                CrearEtiquetaInformativa(
                    "Estado:",
                    362);

            _lblCantidadGrupos =
                CrearEtiquetaInformativa(
                    "Grupos asociados:",
                    390);

            _btnGuardar =
                new Button
                {
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            476,
                            438),
                    Size =
                        new Size(
                            116,
                            36),
                    Text =
                        "Guardar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnCancelar =
                new Button
                {
                    DialogResult =
                        DialogResult.Cancel,
                    Location =
                        new Point(
                            602,
                            438),
                    Size =
                        new Size(
                            116,
                            36),
                    Text =
                        "Cancelar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnGuardar.Click +=
                BtnGuardar_Click;

            _txtCodigo.TextChanged +=
                TxtCodigo_TextChanged;

            Controls.Add(
                lblTitulo);

            Controls.Add(
                lblAclaracion);

            Controls.Add(
                _btnGuardar);

            Controls.Add(
                _btnCancelar);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private TextBox CrearCampoTexto(
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
                        new Point(
                            31,
                            posicionY),
                    Text =
                        etiqueta
                };

            var textBox =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            posicionY - 4),
                    MaxLength =
                        longitudMaxima,
                    Size =
                        new Size(
                            513,
                            25)
                };

            Controls.Add(
                label);

            Controls.Add(
                textBox);

            return textBox;
        }

        private void CrearCampoDescripcion()
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
                        new Point(
                            31,
                            247),
                    Text =
                        "Descripción"
                };

            _txtDescripcion =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            243),
                    MaxLength = 500,
                    Multiline = true,
                    ScrollBars =
                        ScrollBars.Vertical,
                    Size =
                        new Size(
                            513,
                            92)
                };

            Controls.Add(
                label);

            Controls.Add(
                _txtDescripcion);
        }

        private Label CrearEtiquetaInformativa(
            string texto,
            int posicionY)
        {
            var etiqueta =
                new Label
                {
                    AutoSize = true,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            205,
                            posicionY),
                    Text =
                        texto
                };

            Controls.Add(
                etiqueta);

            return etiqueta;
        }

        private void PermisoEditForm_Load(
            object sender,
            EventArgs e)
        {
            CargarDatos();

            _txtCodigo.ReadOnly =
                EsEdicion;

            _txtCodigo.BackColor =
                Color.White;

            if (EsEdicion)
            {
                _txtNombre.Focus();
            }
            else
            {
                _txtCodigo.Focus();
            }
        }

        private void CargarDatos()
        {
            if (!EsEdicion)
            {
                _txtCodigo.Clear();
                _txtNombre.Clear();
                _txtDescripcion.Clear();

                _lblEstado.Text =
                    "Estado: el permiso se registrará activo";

                _lblCantidadGrupos.Text =
                    "Grupos asociados: 0";

                return;
            }

            _txtCodigo.Text =
                _permiso.Codigo;

            _txtNombre.Text =
                _permiso.Nombre;

            _txtDescripcion.Text =
                _permiso.Descripcion;

            _lblEstado.Text =
                "Estado: " +
                _permiso.Estado;

            _lblCantidadGrupos.Text =
                "Grupos asociados: " +
                _permiso.CantidadGrupos;
        }

        private void TxtCodigo_TextChanged(
            object sender,
            EventArgs e)
        {
            if (EsEdicion ||
                string.IsNullOrWhiteSpace(
                    _txtCodigo.Text))
            {
                return;
            }

            int posicion =
                _txtCodigo.SelectionStart;

            try
            {
                string normalizado =
                    PermisoGestionService
                        .NormalizarCodigo(
                            _txtCodigo.Text);

                if (!string.Equals(
                    normalizado,
                    _txtCodigo.Text,
                    StringComparison.Ordinal))
                {
                    _txtCodigo.Text =
                        normalizado;

                    _txtCodigo.SelectionStart =
                        Math.Min(
                            posicion,
                            _txtCodigo.Text.Length);
                }
            }
            catch (ReglaNegocioException)
            {
                // La validación definitiva se realiza al guardar.
            }
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

            CambiarEstado(
                true);

            try
            {
                if (EsEdicion)
                {
                    _permisoService.Modificar(
                        new ModificarPermisoCommand(
                            _permiso.IdPermiso,
                            _txtNombre.Text,
                            _txtDescripcion.Text));
                }
                else
                {
                    _permisoService.Registrar(
                        new RegistrarPermisoCommand(
                            _txtCodigo.Text,
                            _txtNombre.Text,
                            _txtDescripcion.Text));
                }

                MessageBox.Show(
                    EsEdicion
                        ? "El permiso fue modificado correctamente."
                        : "El permiso fue registrado correctamente.",
                    "Permisos",
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
                    "Validación",
                    MessageBoxIcon.Warning);
            }
            catch (AccesoDenegadoException exception)
            {
                MostrarError(
                    exception.Message,
                    "Acceso denegado",
                    MessageBoxIcon.Warning);
            }
            catch (PersistenciaException)
            {
                MostrarError(
                    "No fue posible guardar el permiso. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MostrarError(
                    "Ocurrió un error inesperado al guardar el permiso.",
                    "Error",
                    MessageBoxIcon.Error);
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private bool ValidarCampos()
        {
            bool valido = true;

            if (string.IsNullOrWhiteSpace(
                _txtCodigo.Text))
            {
                _errorProvider.SetError(
                    _txtCodigo,
                    "El código del permiso es obligatorio.");

                valido = false;
            }

            if (string.IsNullOrWhiteSpace(
                _txtNombre.Text))
            {
                _errorProvider.SetError(
                    _txtNombre,
                    "El nombre del permiso es obligatorio.");

                valido = false;
            }

            if (!valido)
            {
                MessageBox.Show(
                    "Revise los campos indicados antes de continuar.",
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
            bool procesando)
        {
            _btnGuardar.Enabled =
                !procesando;

            _btnCancelar.Enabled =
                !procesando;

            _txtCodigo.Enabled =
                !procesando;

            _txtNombre.Enabled =
                !procesando;

            _txtDescripcion.Enabled =
                !procesando;

            UseWaitCursor =
                procesando;
        }

        private static void MostrarError(
            string mensaje,
            string titulo,
            MessageBoxIcon icono)
        {
            MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButtons.OK,
                icono);
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _errorProvider.Dispose();
            }

            base.Dispose(
                disposing);
        }
    }
}