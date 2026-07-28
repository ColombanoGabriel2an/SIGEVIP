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

        private TabControl _tabConfiguracion;
        private TabPage _tabPermisos;
        private TabPage _tabGruposHijos;
        private TabPage _tabPermisosEfectivos;

        private CheckedListBox _lstPermisos;
        private CheckedListBox _lstGruposHijos;
        private ListBox _lstPermisosEfectivos;

        private Label _lblCantidadPermisos;
        private Label _lblCantidadGruposHijos;
        private Label _lblCantidadPermisosEfectivos;

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
                    820,
                    760);

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
                            750,
                            48),
                    Text =
                        EsEdicion
                            ? "El código es estable y no puede modificarse. " +
                              "Los cambios de permisos y jerarquías se aplicarán " +
                              "a los usuarios después de una nueva autenticación."
                            : "El código se genera automáticamente desde el nombre. " +
                              "Debe asignar al menos un permiso directo. " +
                              "La selección de grupos hijos es opcional."
                };

            _txtCodigo =
                CrearCampoTexto(
                    "Código",
                    124,
                    100);

            _txtCodigo.ReadOnly =
                true;

            _txtCodigo.BackColor =
                Color.White;

            _txtNombre =
                CrearCampoTexto(
                    "Nombre",
                    180,
                    150);

            CrearCampoDescripcion();

            CrearConfiguracion();

            _btnGuardar =
                new Button
                {
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            536,
                            694),
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
                            662,
                            694),
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

            _lstPermisos.ItemCheck +=
                Lista_ItemCheck;

            _lstGruposHijos.ItemCheck +=
                Lista_ItemCheck;

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
                            573,
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
                            236),
                    Text =
                        "Descripción"
                };

            _txtDescripcion =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            232),
                    MaxLength = 500,
                    Multiline = true,
                    ScrollBars =
                        ScrollBars.Vertical,
                    Size =
                        new Size(
                            573,
                            88)
                };

            Controls.Add(
                label);

            Controls.Add(
                _txtDescripcion);
        }

        private void CrearConfiguracion()
        {
            var lblConfiguracion =
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
                            346),
                    Text =
                        "Configuración de seguridad"
                };

            _tabConfiguracion =
                new TabControl
                {
                    Location =
                        new Point(
                            205,
                            342),
                    Size =
                        new Size(
                            573,
                            326)
                };

            CrearPestanaPermisos();
            CrearPestanaGruposHijos();
            CrearPestanaPermisosEfectivos();

            _tabConfiguracion.TabPages.Add(
                _tabPermisos);

            _tabConfiguracion.TabPages.Add(
                _tabGruposHijos);

            _tabConfiguracion.TabPages.Add(
                _tabPermisosEfectivos);

            Controls.Add(
                lblConfiguracion);

            Controls.Add(
                _tabConfiguracion);
        }

        private void CrearPestanaPermisos()
        {
            _tabPermisos =
                new TabPage
                {
                    BackColor =
                        Color.White,
                    Padding =
                        new Padding(
                            12),
                    Text =
                        "Permisos directos"
                };

            var lblAclaracion =
                new Label
                {
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            14,
                            14),
                    Size =
                        new Size(
                            520,
                            38),
                    Text =
                        "Marque uno o más permisos directos. " +
                        "Los permisos heredados desde grupos hijos " +
                        "no se muestran en esta lista."
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
                            14,
                            58),
                    Size =
                        new Size(
                            532,
                            202)
                };

            _lblCantidadPermisos =
                new Label
                {
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            14,
                            270),
                    Text =
                        "Seleccionados: 0"
                };

            _tabPermisos.Controls.Add(
                lblAclaracion);

            _tabPermisos.Controls.Add(
                _lstPermisos);

            _tabPermisos.Controls.Add(
                _lblCantidadPermisos);
        }

        private void CrearPestanaGruposHijos()
        {
            _tabGruposHijos =
                new TabPage
                {
                    BackColor =
                        Color.White,
                    Padding =
                        new Padding(
                            12),
                    Text =
                        "Grupos hijos"
                };

            var lblAclaracion =
                new Label
                {
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            14,
                            14),
                    Size =
                        new Size(
                            520,
                            52),
                    Text =
                        "La selección es opcional. El grupo heredará los " +
                        "permisos efectivos de los grupos hijos. " +
                        "Los grupos inactivos previamente asociados aparecen " +
                        "marcados como “Inactivo”."
                };

            _lstGruposHijos =
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
                            14,
                            72),
                    Size =
                        new Size(
                            532,
                            188)
                };

            _lblCantidadGruposHijos =
                new Label
                {
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            14,
                            270),
                    Text =
                        "Seleccionados: 0"
                };

            _tabGruposHijos.Controls.Add(
                lblAclaracion);

            _tabGruposHijos.Controls.Add(
                _lstGruposHijos);

            _tabGruposHijos.Controls.Add(
                _lblCantidadGruposHijos);
        }

        private void CrearPestanaPermisosEfectivos()
        {
            _tabPermisosEfectivos =
                new TabPage
                {
                    BackColor =
                        Color.White,
                    Padding =
                        new Padding(
                            12),
                    Text =
                        "Permisos efectivos"
                };

            var lblAclaracion =
                new Label
                {
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            14,
                            14),
                    Size =
                        new Size(
                            520,
                            52),
                    Text =
                        "Vista previa informativa. Combina los permisos " +
                        "directos con los heredados desde grupos hijos " +
                        "activos y elimina los duplicados."
                };

            _lstPermisosEfectivos =
                new ListBox
                {
                    DisplayMember =
                        "Texto",
                    HorizontalScrollbar =
                        true,
                    IntegralHeight =
                        false,
                    Location =
                        new Point(
                            14,
                            72),
                    SelectionMode =
                        SelectionMode.One,
                    Size =
                        new Size(
                            532,
                            188)
                };

            _lblCantidadPermisosEfectivos =
                new Label
                {
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            14,
                            270),
                    Text =
                        "Permisos efectivos: 0"
                };

            _tabPermisosEfectivos.Controls.Add(
                lblAclaracion);

            _tabPermisosEfectivos.Controls.Add(
                _lstPermisosEfectivos);

            _tabPermisosEfectivos.Controls.Add(
                _lblCantidadPermisosEfectivos);
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
                CargarGruposHijos();

                ActualizarContadores();
                ActualizarPermisosEfectivos();

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

        private void CargarGruposHijos()
        {
            int? idGrupoPadre =
                EsEdicion
                    ? (int?)_grupo.IdGrupo
                    : null;

            IReadOnlyCollection<int> seleccionados =
                EsEdicion
                    ? _grupo.IdsGruposHijos
                    : new List<int>()
                        .AsReadOnly();

            IReadOnlyCollection<GrupoSeleccionGrupoDto>
                grupos =
                    _grupoService.ListarGruposHijos(
                        idGrupoPadre,
                        seleccionados);

            _lstGruposHijos.Items.Clear();

            foreach (
                GrupoSeleccionGrupoDto grupo
                in grupos)
            {
                _lstGruposHijos.Items.Add(
                    grupo,
                    grupo.Seleccionado);
            }
        }

        private void TxtNombre_TextChanged(
            object sender,
            EventArgs e)
        {
            ActualizarCodigoVisible();
        }

        private void Lista_ItemCheck(
            object sender,
            ItemCheckEventArgs e)
        {
            BeginInvoke(
                new Action(
                    ActualizarSeleccionYVistaPrevia));
        }

        private void ActualizarSeleccionYVistaPrevia()
        {
            ActualizarContadores();
            ActualizarPermisosEfectivos();
        }

        private void ActualizarContadores()
        {
            _lblCantidadPermisos.Text =
                "Seleccionados: " +
                _lstPermisos.CheckedItems.Count;

            _lblCantidadGruposHijos.Text =
                "Seleccionados: " +
                _lstGruposHijos.CheckedItems.Count;
        }

        private void ActualizarPermisosEfectivos()
        {
            List<int> idsPermisos =
                ObtenerIdsPermisosSeleccionados();

            List<int> idsGruposHijos =
                ObtenerIdsGruposHijosSeleccionados();

            IReadOnlyCollection<PermisoEfectivoGrupoDto>
                permisos =
                    _grupoService
                        .ObtenerVistaPreviaPermisosEfectivos(
                            idsPermisos,
                            idsGruposHijos);

            _lstPermisosEfectivos.Items.Clear();

            foreach (
                PermisoEfectivoGrupoDto permiso
                in permisos)
            {
                _lstPermisosEfectivos.Items.Add(
                    permiso);
            }

            _lblCantidadPermisosEfectivos.Text =
                "Permisos efectivos: " +
                _lstPermisosEfectivos.Items.Count;
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

                List<int> idsGruposHijos =
                    ObtenerIdsGruposHijosSeleccionados();

                if (EsEdicion)
                {
                    _grupoService.Modificar(
                        new ModificarGrupoCommand(
                            _grupo.IdGrupo,
                            _txtNombre.Text,
                            _txtDescripcion.Text,
                            idsPermisos,
                            idsGruposHijos));
                }
                else
                {
                    _grupoService.Registrar(
                        new RegistrarGrupoCommand(
                            _txtNombre.Text,
                            _txtDescripcion.Text,
                            idsPermisos,
                            idsGruposHijos));
                }

                MessageBox.Show(
                    EsEdicion
                        ? "El grupo y su configuración fueron modificados correctamente."
                        : "El grupo y su configuración fueron registrados correctamente.",
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
                    "No fue posible guardar el grupo y su configuración. " +
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
                    _tabConfiguracion,
                    "Debe seleccionar al menos un permiso directo.");

                _tabConfiguracion.SelectedTab =
                    _tabPermisos;

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

        private List<int>
            ObtenerIdsGruposHijosSeleccionados()
        {
            List<int> ids =
                new List<int>();

            foreach (
                object elemento
                in _lstGruposHijos.CheckedItems)
            {
                GrupoSeleccionGrupoDto grupo =
                    elemento
                        as GrupoSeleccionGrupoDto;

                if (grupo != null)
                {
                    ids.Add(
                        grupo.IdGrupo);
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

            _tabConfiguracion.Enabled =
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
