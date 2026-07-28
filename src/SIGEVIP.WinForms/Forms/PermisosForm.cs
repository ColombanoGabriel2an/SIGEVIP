using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Permisos;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class PermisosForm : Form
    {
        private readonly PermisoGestionService
            _permisoService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private TextBox _txtBusqueda;
        private ComboBox _cmbEstado;

        private Button _btnBuscar;
        private Button _btnLimpiar;
        private Button _btnNuevo;
        private Button _btnModificar;
        private Button _btnActivar;
        private Button _btnDesactivar;
        private Button _btnCerrar;

        private DataGridView _grilla;

        private Label _lblCantidad;
        private Label _lblUsuarioActual;

        public PermisosForm(
            PermisoGestionService permisoService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _permisoService =
                permisoService
                ?? throw new ArgumentNullException(
                    nameof(permisoService));

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

            Load +=
                PermisosForm_Load;

            _grilla.SelectionChanged +=
                Grilla_SelectionChanged;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Permisos";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(
                    1080,
                    650);

            Size =
                new Size(
                    1220,
                    740);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            var lblTitulo =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        18F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            24,
                            20),
                    Text =
                        "Gestión de permisos"
                };

            _lblUsuarioActual =
                new Label
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Right,
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            745,
                            27),
                    Size =
                        new Size(
                            434,
                            24),
                    TextAlign =
                        ContentAlignment.MiddleRight
                };

            Panel panelFiltros =
                CrearPanelFiltros();

            _grilla =
                CrearGrilla();

            FlowLayoutPanel panelAcciones =
                CrearPanelAcciones();

            _lblCantidad =
                new Label
                {
                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Bottom,
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            25,
                            655),
                    Text =
                        "Resultados: 0"
                };

            Controls.Add(
                lblTitulo);

            Controls.Add(
                _lblUsuarioActual);

            Controls.Add(
                panelFiltros);

            Controls.Add(
                _grilla);

            Controls.Add(
                panelAcciones);

            Controls.Add(
                _lblCantidad);
        }

        private Panel CrearPanelFiltros()
        {
            var panel =
                new Panel
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    BackColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Location =
                        new Point(
                            24,
                            67),
                    Size =
                        new Size(
                            1155,
                            125)
                };

            var lblBusqueda =
                CrearEtiquetaFiltro(
                    "Búsqueda general",
                    16);

            _txtBusqueda =
                new TextBox
                {
                    Location =
                        new Point(
                            16,
                            42),
                    MaxLength = 500,
                    Size =
                        new Size(
                            550,
                            23)
                };

            var lblEstado =
                CrearEtiquetaFiltro(
                    "Estado",
                    590);

            _cmbEstado =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            590,
                            42),
                    Size =
                        new Size(
                            170,
                            23)
                };

            _cmbEstado.Items.Add(
                "Todos");

            _cmbEstado.Items.Add(
                "Activos");

            _cmbEstado.Items.Add(
                "Inactivos");

            _cmbEstado.SelectedIndex = 0;

            _btnBuscar =
                new Button
                {
                    Location =
                        new Point(
                            878,
                            78),
                    Size =
                        new Size(
                            120,
                            32),
                    Text =
                        "Buscar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnLimpiar =
                new Button
                {
                    Location =
                        new Point(
                            1008,
                            78),
                    Size =
                        new Size(
                            120,
                            32),
                    Text =
                        "Limpiar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnBuscar.Click +=
                BtnBuscar_Click;

            _btnLimpiar.Click +=
                BtnLimpiar_Click;

            panel.Controls.Add(
                lblBusqueda);

            panel.Controls.Add(
                _txtBusqueda);

            panel.Controls.Add(
                lblEstado);

            panel.Controls.Add(
                _cmbEstado);

            panel.Controls.Add(
                _btnBuscar);

            panel.Controls.Add(
                _btnLimpiar);

            return panel;
        }

        private static Label CrearEtiquetaFiltro(
            string texto,
            int posicionX)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold),
                Location =
                    new Point(
                        posicionX,
                        17),
                Text =
                    texto
            };
        }

        private DataGridView CrearGrilla()
        {
            var grilla =
                new DataGridView
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    AutoGenerateColumns =
                        false,
                    AllowUserToAddRows =
                        false,
                    AllowUserToDeleteRows =
                        false,
                    AllowUserToResizeRows =
                        false,
                    BackgroundColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.Fixed3D,
                    ColumnHeadersHeightSizeMode =
                        DataGridViewColumnHeadersHeightSizeMode
                            .AutoSize,
                    Location =
                        new Point(
                            24,
                            208),
                    MultiSelect =
                        false,
                    ReadOnly =
                        true,
                    RowHeadersVisible =
                        false,
                    SelectionMode =
                        DataGridViewSelectionMode
                            .FullRowSelect,
                    Size =
                        new Size(
                            1155,
                            375)
                };

            AgregarColumna(
                grilla,
                "Codigo",
                "Código",
                230);

            AgregarColumna(
                grilla,
                "Nombre",
                "Nombre",
                240);

            AgregarColumna(
                grilla,
                "Descripcion",
                "Descripción",
                405);

            AgregarColumna(
                grilla,
                "CantidadGrupos",
                "Grupos",
                90);

            AgregarColumna(
                grilla,
                "Estado",
                "Estado",
                100);

            return grilla;
        }

        private static void AgregarColumna(
            DataGridView grilla,
            string propiedad,
            string titulo,
            int ancho)
        {
            grilla.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        propiedad,
                    HeaderText =
                        titulo,
                    Name =
                        propiedad,
                    Width =
                        ancho
                });
        }

        private FlowLayoutPanel CrearPanelAcciones()
        {
            var panel =
                new FlowLayoutPanel
                {
                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,
                    FlowDirection =
                        FlowDirection.LeftToRight,
                    Location =
                        new Point(
                            24,
                            595),
                    Size =
                        new Size(
                            1155,
                            48),
                    WrapContents =
                        false
                };

            _btnNuevo =
                CrearBoton(
                    "Nuevo");

            _btnModificar =
                CrearBoton(
                    "Modificar");

            _btnActivar =
                CrearBoton(
                    "Activar");

            _btnDesactivar =
                CrearBoton(
                    "Desactivar");

            _btnCerrar =
                CrearBoton(
                    "Cerrar");

            _btnNuevo.Click +=
                BtnNuevo_Click;

            _btnModificar.Click +=
                BtnModificar_Click;

            _btnActivar.Click +=
                BtnActivar_Click;

            _btnDesactivar.Click +=
                BtnDesactivar_Click;

            _btnCerrar.Click +=
                BtnCerrar_Click;

            panel.Controls.Add(
                _btnNuevo);

            panel.Controls.Add(
                _btnModificar);

            panel.Controls.Add(
                _btnActivar);

            panel.Controls.Add(
                _btnDesactivar);

            panel.Controls.Add(
                _btnCerrar);

            return panel;
        }

        private static Button CrearBoton(
            string texto)
        {
            return new Button
            {
                Margin =
                    new Padding(
                        0,
                        0,
                        10,
                        0),
                Size =
                    new Size(
                        125,
                        36),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private void ConfigurarPermisos()
        {
            bool puedeGestionar =
                _sesionActual.HayUsuarioAutenticado
                && _autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        PermisoGestionService
                            .PermisoGestionar);

            _btnNuevo.Visible =
                puedeGestionar;

            _btnModificar.Visible =
                puedeGestionar;

            _btnActivar.Visible =
                puedeGestionar;

            _btnDesactivar.Visible =
                puedeGestionar;

            if (_sesionActual.HayUsuarioAutenticado)
            {
                _lblUsuarioActual.Text =
                    "Usuario autenticado: " +
                    _sesionActual
                        .UsuarioActual
                        .NombreUsuario;
            }
        }

        private void PermisosForm_Load(
            object sender,
            EventArgs e)
        {
            CargarPermisos();
        }

        private void BtnBuscar_Click(
            object sender,
            EventArgs e)
        {
            CargarPermisos();
        }

        private void BtnLimpiar_Click(
            object sender,
            EventArgs e)
        {
            _txtBusqueda.Clear();
            _cmbEstado.SelectedIndex = 0;

            CargarPermisos();
        }

        private void BtnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using (
                var formulario =
                    new PermisoEditForm(
                        _permisoService,
                        null))
            {
                if (formulario.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    CargarPermisos();
                }
            }
        }

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            PermisoListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            try
            {
                PermisoDetalleDto detalle =
                    _permisoService.Obtener(
                        seleccionado.IdPermiso);

                using (
                    var formulario =
                        new PermisoEditForm(
                            _permisoService,
                            detalle))
                {
                    if (formulario.ShowDialog(this) ==
                        DialogResult.OK)
                    {
                        CargarPermisos();
                    }
                }
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnActivar_Click(
            object sender,
            EventArgs e)
        {
            CambiarEstadoSeleccionado(
                true);
        }

        private void BtnDesactivar_Click(
            object sender,
            EventArgs e)
        {
            CambiarEstadoSeleccionado(
                false);
        }

        private void CambiarEstadoSeleccionado(
            bool activar)
        {
            PermisoListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            if (activar &&
                seleccionado.Activo)
            {
                MessageBox.Show(
                    "El permiso seleccionado ya se encuentra activo.",
                    "Permisos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!activar &&
                !seleccionado.Activo)
            {
                MessageBox.Show(
                    "El permiso seleccionado ya se encuentra inactivo.",
                    "Permisos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string mensaje =
                activar
                    ? "¿Desea activar el permiso seleccionado?"
                    : "¿Desea desactivar el permiso seleccionado? " +
                      "Las asociaciones con grupos se conservarán, " +
                      "pero el permiso dejará de otorgar autorización " +
                      "hasta ser reactivado.";

            DialogResult confirmacion =
                MessageBox.Show(
                    mensaje,
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (confirmacion !=
                DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (activar)
                {
                    _permisoService.Activar(
                        seleccionado.IdPermiso);
                }
                else
                {
                    _permisoService.Desactivar(
                        seleccionado.IdPermiso);
                }

                CargarPermisos();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnCerrar_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        private void CargarPermisos()
        {
            CambiarEstadoCarga(
                true);

            try
            {
                PermisoFiltro filtro =
                    new PermisoFiltro(
                        _txtBusqueda.Text,
                        ObtenerEstadoSeleccionado());

                IReadOnlyCollection<PermisoListadoDto>
                    permisos =
                        _permisoService.Listar(
                            filtro);

                List<PermisoListadoDto> lista =
                    permisos.ToList();

                _grilla.DataSource =
                    null;

                _grilla.DataSource =
                    lista;

                _lblCantidad.Text =
                    "Resultados: " +
                    lista.Count;

                ConfigurarAccionesSeleccion();
            }
            catch (Exception exception)
            {
                _grilla.DataSource =
                    null;

                _lblCantidad.Text =
                    "Resultados: 0";

                ConfigurarAccionesSeleccion();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void Grilla_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ConfigurarAccionesSeleccion();
        }

        private void ConfigurarAccionesSeleccion()
        {
            PermisoListadoDto seleccionado =
                ObtenerSeleccionado();

            bool haySeleccion =
                seleccionado != null;

            _btnModificar.Enabled =
                haySeleccion;

            _btnActivar.Enabled =
                haySeleccion &&
                !seleccionado.Activo;

            _btnDesactivar.Enabled =
                haySeleccion &&
                seleccionado.Activo;
        }

        private void CambiarEstadoCarga(
            bool cargando)
        {
            _btnBuscar.Enabled =
                !cargando;

            _btnLimpiar.Enabled =
                !cargando;

            _btnNuevo.Enabled =
                !cargando;

            _cmbEstado.Enabled =
                !cargando;

            _txtBusqueda.Enabled =
                !cargando;

            if (cargando)
            {
                _btnModificar.Enabled =
                    false;

                _btnActivar.Enabled =
                    false;

                _btnDesactivar.Enabled =
                    false;
            }
            else
            {
                ConfigurarAccionesSeleccion();
            }

            UseWaitCursor =
                cargando;
        }

        private bool? ObtenerEstadoSeleccionado()
        {
            switch (
                _cmbEstado.SelectedIndex)
            {
                case 1:
                    return true;

                case 2:
                    return false;

                default:
                    return null;
            }
        }

        private PermisoListadoDto ObtenerSeleccionado()
        {
            if (_grilla.CurrentRow == null)
            {
                return null;
            }

            return _grilla
                .CurrentRow
                .DataBoundItem
                as PermisoListadoDto;
        }

        private static void MostrarSeleccionRequerida()
        {
            MessageBox.Show(
                "Seleccione un permiso de la lista.",
                "Permisos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static void MostrarErrorControlado(
            Exception exception)
        {
            if (exception is ReglaNegocioException ||
                exception is AccesoDenegadoException)
            {
                MessageBox.Show(
                    exception.Message,
                    "Permisos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible acceder a los permisos. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado en el módulo de permisos.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}