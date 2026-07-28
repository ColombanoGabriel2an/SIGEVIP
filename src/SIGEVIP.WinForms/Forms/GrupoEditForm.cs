using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Grupos;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class GrupoEditForm : Form
    {
        private readonly GrupoGestionService
            _grupoService;

        private readonly GrupoDetalleDto
            _grupo;

        private readonly ErrorProvider
            _errorProvider;

        private TextBox _txtCodigo;
        private TextBox _txtNombre;
        private TextBox _txtDescripcion;

        private CheckedListBox _lstPermisos;

        private Button _btnGuardar;
        private Button _btnCancelar;

        private bool EsEdicion
        {
            get
            {
                return _grupo != null;
            }
        }

        public GrupoEditForm(
            GrupoGestionService grupoService,
            GrupoDetalleDto grupo)
        {
            _grupoService =
                grupoService
                ?? throw new ArgumentNullException(
                    nameof(grupoService));

            _grupo =
                grupo;

            _errorProvider =
                new ErrorProvider();

            InicializarFormulario();

            Load +=
                GrupoEditForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                EsEdicion
                    ? "Modificar grupo"
                    : "Nuevo grupo";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(
                    760,
                    680);

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
                            ? "Modificar grupo"
                            : "Nuevo grupo"
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
                            42),
                    Text =
                        EsEdicion
                            ? "El código es estable y no puede modificarse. " +
                              "Los cambios de permisos se aplicarán a los usuarios " +
                              "después de una nueva autenticación."
                            : "El código se genera automáticamente desde el nombre. " +
                              "Debe asignar al menos un permiso directo activo."
                };

            _txtCodigo =
                CrearCampoTexto(
                    "Código",
                    120,
                    100);

            _txtCodigo.ReadOnly =
                true;

            _txtCodigo.BackColor =
                Color.White;

            _txtNombre =
                CrearCampoTexto(
                    "Nombre",
                    176,
                    150);

            CrearCampoDescripcion();

            CrearListaPermisos();

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
                            614),
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
                            614),
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

            _txtNombre.TextChanged +=
                TxtNombre_TextChanged;

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
                            232),
                    Text =
                        "Descripción"
                };

            _txtDescripcion =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            228),
                    MaxLength = 500,
                    Multiline = true,
                    ScrollBars =
                        ScrollBars.Vertical,
                    Size =
                        new Size(
                            513,
                            88)
                };

            Controls.Add(
                label);

            Controls.Add(
                _txtDescripcion);
        }

        private void CrearListaPermisos()
        {
            var lblPermisos =
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
                            348),
                    Text =
                        "Permisos directos"
                };

            var lblAclaracionPermisos =
                new Label
                {
                    AutoSize = true,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            205,
                            348),
                    Text =
                        "Marque uno o más permisos activos."
                };

            _lstPermisos =
                new CheckedListBox
                {
                    CheckOnClick = true,
                    DisplayMember =
                        "Texto",
                    HorizontalScrollbar =
                        true,
                    IntegralHeight =
                        false,
                    Location =
                        new Point(
                            205,
                            374),
                    Size =
                        new Size(
                            513,
                            210)
                };

            Controls.Add(
                lblPermisos);

            Controls.Add(
                lblAclaracionPermisos);

            Controls.Add(
                _lstPermisos);
        }

        private void GrupoEditForm_Load(
            object sender,
            EventArgs e)
        {
            CambiarEstado(
                true);

            try
            {
                CargarDatos();
                CargarPermisos();

                _txtNombre.Focus();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);

                DialogResult =
                    DialogResult.Cancel;

                Close();
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void CargarDatos()
        {
            if (!EsEdicion)
            {
                _txtCodigo.Clear();
                return;
            }

            _txtCodigo.Text =
                _grupo.Codigo;

            _txtNombre.Text =
                _grupo.Nombre;

            _txtDescripcion.Text =
                _grupo.Descripcion;
        }

        private void CargarPermisos()
        {
            IReadOnlyCollection<int> seleccionados =
                EsEdicion
                    ? _grupo.IdsPermisosDirectos
                    : new List<int>()
                        .AsReadOnly();

            IReadOnlyCollection<PermisoSeleccionGrupoDto>
                permisos =
                    _grupoService.ListarPermisos(
                        seleccionados);

            _lstPermisos.Items.Clear();

            foreach (
                PermisoSeleccionGrupoDto permiso
                in permisos)
            {
                _lstPermisos.Items.Add(
                    permiso,
                    permiso.Seleccionado);
            }
        }

        private void TxtNombre_TextChanged(
            object sender,
            EventArgs e)
        {
            ActualizarCodigoVisible();
        }

        private void ActualizarCodigoVisible()
        {
            if (EsEdicion)
            {
                _txtCodigo.Text =
                    _grupo.Codigo;

                return;
            }

            if (string.IsNullOrWhiteSpace(
                _txtNombre.Text))
            {
                _txtCodigo.Clear();
                return;
            }

            try
            {
                _txtCodigo.Text =
                    GrupoGestionService
                        .GenerarCodigo(
                            _txtNombre.Text);
            }
            catch (ReglaNegocioException)
            {
                _txtCodigo.Clear();
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
                List<int> idsPermisos =
                    ObtenerIdsPermisosSeleccionados();

                if (EsEdicion)
                {
                    _grupoService.Modificar(
                        new ModificarGrupoCommand(
                            _grupo.IdGrupo,
                            _txtNombre.Text,
                            _txtDescripcion.Text,
                            idsPermisos));
                }
                else
                {
                    _grupoService.Registrar(
                        new RegistrarGrupoCommand(
                            _txtNombre.Text,
                            _txtDescripcion.Text,
                            idsPermisos));
                }

                MessageBox.Show(
                    EsEdicion
                        ? "El grupo fue modificado correctamente."
                        : "El grupo fue registrado correctamente.",
                    "Grupos",
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
                    "No fue posible guardar el grupo. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MostrarError(
                    "Ocurrió un error inesperado al guardar el grupo.",
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
                _txtNombre.Text))
            {
                _errorProvider.SetError(
                    _txtNombre,
                    "El nombre del grupo es obligatorio.");

                valido = false;
            }

            if (string.IsNullOrWhiteSpace(
                _txtDescripcion.Text))
            {
                _errorProvider.SetError(
                    _txtDescripcion,
                    "La descripción del grupo es obligatoria.");

                valido = false;
            }

            if (_lstPermisos.CheckedItems.Count == 0)
            {
                _errorProvider.SetError(
                    _lstPermisos,
                    "Debe seleccionar al menos un permiso.");

                valido = false;
            }

            if (!EsEdicion &&
                string.IsNullOrWhiteSpace(
                    _txtCodigo.Text))
            {
                _errorProvider.SetError(
                    _txtNombre,
                    "El nombre no permite generar un código válido.");

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

        private List<int>
            ObtenerIdsPermisosSeleccionados()
        {
            List<int> ids =
                new List<int>();

            foreach (
                object elemento
                in _lstPermisos.CheckedItems)
            {
                PermisoSeleccionGrupoDto permiso =
                    elemento
                        as PermisoSeleccionGrupoDto;

                if (permiso != null)
                {
                    ids.Add(
                        permiso.IdPermiso);
                }
            }

            return ids;
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

            _txtNombre.Enabled =
                !procesando;

            _txtDescripcion.Enabled =
                !procesando;

            _lstPermisos.Enabled =
                !procesando;

            UseWaitCursor =
                procesando;
        }

        private static void MostrarErrorControlado(
            Exception exception)
        {
            if (exception is ReglaNegocioException ||
                exception is AccesoDenegadoException)
            {
                MostrarError(
                    exception.Message,
                    "Grupos",
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MostrarError(
                    "No fue posible obtener los datos del grupo. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxIcon.Error);

                return;
            }

            MostrarError(
                "Ocurrió un error inesperado al cargar el formulario.",
                "Error",
                MessageBoxIcon.Error);
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