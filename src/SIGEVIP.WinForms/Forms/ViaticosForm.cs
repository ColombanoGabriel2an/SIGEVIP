using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ViaticosForm : Form
    {
        private readonly ViaticoService
            _viaticoService;

        private readonly RendicionService
            _rendicionService;

        private readonly ViajeService
            _viajeService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private ComboBox _cmbViaje;
        private DateTimePicker _dtpFechaDesde;
        private DateTimePicker _dtpFechaHasta;
        private ComboBox _cmbCategoria;
        private ComboBox _cmbEstado;

        private Button _btnConsultar;
        private Button _btnLimpiar;
        private Button _btnActualizar;
        private Button _btnNuevo;
        private Button _btnModificar;
        private Button _btnDetalle;
        private Button _btnEnviarRendicion;
        private Button _btnCerrar;

        private DataGridView _grilla;

        private Label _lblViaje;
        private Label _lblResultados;
        private Label _lblMontoAnticipado;
        private Label _lblTotalGastado;
        private Label _lblSaldo;

        private ViajeListadoDto
            ViajeSeleccionado
        {
            get
            {
                return _cmbViaje.SelectedItem
                    as ViajeListadoDto;
            }
        }

        public ViaticosForm(
            ViaticoService viaticoService,
            RendicionService rendicionService,
            ViajeService viajeService,
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

            _viajeService =
                viajeService
                ?? throw new ArgumentNullException(
                    nameof(viajeService));

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
                ViaticosForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Gestión de viáticos";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(
                    1180,
                    700);

            Size =
                new Size(
                    1320,
                    790);

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
                            24,
                            18),
                    Text =
                        "Gestión de viáticos"
                });

            Controls.Add(
                CrearPanelSeleccionViaje());

            Controls.Add(
                CrearPanelFiltros());

            _lblViaje =
                new Label
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    AutoEllipsis = true,
                    BackColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            24,
                            205),
                    Padding =
                        new Padding(10),
                    Size =
                        new Size(
                            1250,
                            45),
                    Text =
                        "Seleccione un viaje abierto.",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            Controls.Add(
                _lblViaje);

            _grilla =
                CrearGrilla();

            Controls.Add(
                _grilla);

            Controls.Add(
                CrearPanelResumen());

            Controls.Add(
                CrearPanelAcciones());

            _lblResultados =
                new Label
                {
                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Bottom,
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            25,
                            704),
                    Text =
                        "Resultados: 0"
                };

            Controls.Add(
                _lblResultados);
        }

        private Panel CrearPanelSeleccionViaje()
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
                            62),
                    Size =
                        new Size(
                            1250,
                            62)
                };

            panel.Controls.Add(
                CrearEtiqueta(
                    "Viaje abierto",
                    16,
                    9));

            _cmbViaje =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            19,
                            31),
                    Size =
                        new Size(
                            870,
                            24)
                };

            _cmbViaje.SelectedIndexChanged +=
                CmbViaje_SelectedIndexChanged;

            _btnConsultar =
                CrearBoton(
                    "Consultar",
                    910,
                    24,
                    120);

            _btnActualizar =
                CrearBoton(
                    "Actualizar viajes",
                    1040,
                    24,
                    180);

            _btnConsultar.Click +=
                BtnConsultar_Click;

            _btnActualizar.Click +=
                BtnActualizar_Click;

            panel.Controls.Add(
                _cmbViaje);

            panel.Controls.Add(
                _btnConsultar);

            panel.Controls.Add(
                _btnActualizar);

            return panel;
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
                            134),
                    Size =
                        new Size(
                            1250,
                            61)
                };

            _dtpFechaDesde =
                CrearFechaFiltro(
                    panel,
                    "Fecha desde",
                    16);

            _dtpFechaHasta =
                CrearFechaFiltro(
                    panel,
                    "Fecha hasta",
                    215);

            panel.Controls.Add(
                CrearEtiqueta(
                    "Categoría",
                    414,
                    8));

            _cmbCategoria =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            414,
                            30),
                    Size =
                        new Size(
                            190,
                            24)
                };

            panel.Controls.Add(
                CrearEtiqueta(
                    "Estado",
                    624,
                    8));

            _cmbEstado =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            624,
                            30),
                    Size =
                        new Size(
                            180,
                            24)
                };

            _btnLimpiar =
                CrearBoton(
                    "Limpiar filtros",
                    824,
                    22,
                    150);

            _btnLimpiar.Click +=
                BtnLimpiar_Click;

            panel.Controls.Add(
                _cmbCategoria);

            panel.Controls.Add(
                _cmbEstado);

            panel.Controls.Add(
                _btnLimpiar);

            return panel;
        }

        private static DateTimePicker CrearFechaFiltro(
            Control contenedor,
            string texto,
            int posicionX)
        {
            contenedor.Controls.Add(
                CrearEtiqueta(
                    texto,
                    posicionX,
                    8));

            var control =
                new DateTimePicker
                {
                    Checked = false,
                    Format =
                        DateTimePickerFormat.Short,
                    Location =
                        new Point(
                            posicionX,
                            30),
                    ShowCheckBox = true,
                    Size =
                        new Size(
                            180,
                            24)
                };

            contenedor.Controls.Add(
                control);

            return control;
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
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    AutoGenerateColumns = false,
                    BackgroundColor =
                        Color.White,
                    Location =
                        new Point(
                            24,
                            260),
                    MultiSelect = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    SelectionMode =
                        DataGridViewSelectionMode
                            .FullRowSelect,
                    Size =
                        new Size(
                            1250,
                            310)
                };

            AgregarColumna(
                grilla,
                "IdViatico",
                "Id",
                55);

            AgregarColumna(
                grilla,
                "Fecha",
                "Fecha",
                90,
                "dd/MM/yyyy");

            AgregarColumna(
                grilla,
                "Categoria",
                "Categoría",
                120);

            AgregarColumna(
                grilla,
                "MetodoPago",
                "Método de pago",
                145);

            AgregarColumna(
                grilla,
                "PagadoPor",
                "Pagado por",
                190);

            AgregarColumna(
                grilla,
                "Monto",
                "Monto",
                110,
                "N2");

            AgregarColumna(
                grilla,
                "Descripcion",
                "Descripción",
                255);

            AgregarColumna(
                grilla,
                "Estado",
                "Estado",
                95);

            AgregarColumna(
                grilla,
                "Comprobante",
                "Comprobante",
                100);

            return grilla;
        }

        private static void AgregarColumna(
            DataGridView grilla,
            string propiedad,
            string titulo,
            int ancho,
            string formato = null)
        {
            var columna =
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
                };

            if (!string.IsNullOrWhiteSpace(
                formato))
            {
                columna.DefaultCellStyle.Format =
                    formato;
            }

            grilla.Columns.Add(
                columna);
        }

        private Panel CrearPanelResumen()
        {
            var panel =
                new TableLayoutPanel
                {
                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,
                    BackColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    ColumnCount = 3,
                    Location =
                        new Point(
                            24,
                            582),
                    RowCount = 1,
                    Size =
                        new Size(
                            1250,
                            62)
                };

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.33F));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.33F));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.34F));

            _lblMontoAnticipado =
                CrearResumen(
                    "Monto anticipado: 0,00");

            _lblTotalGastado =
                CrearResumen(
                    "Total gastado vigente: 0,00");

            _lblSaldo =
                CrearResumen(
                    "Saldo: 0,00");

            panel.Controls.Add(
                _lblMontoAnticipado,
                0,
                0);

            panel.Controls.Add(
                _lblTotalGastado,
                1,
                0);

            panel.Controls.Add(
                _lblSaldo,
                2,
                0);

            return panel;
        }

        private Panel CrearPanelAcciones()
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
                            656),
                    Size =
                        new Size(
                            1250,
                            42),
                    WrapContents = false
                };

            _btnNuevo =
                CrearBotonAccion(
                    "Nuevo viático");

            _btnModificar =
                CrearBotonAccion(
                    "Modificar");

            _btnDetalle =
                CrearBotonAccion(
                    "Ver detalle");

            _btnEnviarRendicion =
                CrearBotonAccion(
                    "Enviar a rendición");

            _btnCerrar =
                CrearBotonAccion(
                    "Cerrar");

            _btnNuevo.Click +=
                BtnNuevo_Click;

            _btnModificar.Click +=
                BtnModificar_Click;

            _btnDetalle.Click +=
                BtnDetalle_Click;

            _btnEnviarRendicion.Click +=
                BtnEnviarRendicion_Click;

            _btnCerrar.Click +=
                delegate
                {
                    Close();
                };

            panel.Controls.Add(
                _btnNuevo);

            panel.Controls.Add(
                _btnModificar);

            panel.Controls.Add(
                _btnDetalle);

            panel.Controls.Add(
                _btnEnviarRendicion);

            panel.Controls.Add(
                _btnCerrar);

            return panel;
        }

        private static Label CrearEtiqueta(
            string texto,
            int posicionX,
            int posicionY)
        {
            return new Label
            {
                AutoSize = true,
                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                Location =
                    new Point(
                        posicionX,
                        posicionY),
                Text =
                    texto
            };
        }

        private static Button CrearBoton(
            string texto,
            int posicionX,
            int posicionY,
            int ancho)
        {
            return new Button
            {
                Location =
                    new Point(
                        posicionX,
                        posicionY),
                Size =
                    new Size(
                        ancho,
                        31),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private static Button CrearBotonAccion(
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
                        160,
                        36),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private static Label CrearResumen(
            string texto)
        {
            return new Label
            {
                Dock =
                    DockStyle.Fill,
                Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold),
                Padding =
                    new Padding(12),
                Text =
                    texto,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private void ConfigurarPermisos()
        {
            _btnNuevo.Visible =
                TienePermiso(
                    ViaticoService
                        .PermisoRegistrar);

            _btnModificar.Visible =
                TienePermiso(
                    ViaticoService
                        .PermisoModificar);

            _btnDetalle.Visible =
                TienePermiso(
                    ViaticoService
                        .PermisoConsultar);

            _btnEnviarRendicion.Visible =
                TienePermiso(
                    RendicionService
                        .PermisoEnviar);
        }

        private bool TienePermiso(
            string codigo)
        {
            return _sesionActual
                .HayUsuarioAutenticado
                && _sesionActual.UsuarioActual != null
                && _autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        codigo);
        }

        private void ViaticosForm_Load(
            object sender,
            EventArgs e)
        {
            CargarFiltros();
            CargarViajes();
        }

        private void CargarFiltros()
        {
            _cmbCategoria.Items.Clear();

            _cmbCategoria.Items.Add(
                new CategoriaFiltroItem(
                    "Todas",
                    null));

            foreach (
                CategoriaGasto categoria
                in Enum.GetValues(
                    typeof(CategoriaGasto)))
            {
                _cmbCategoria.Items.Add(
                    new CategoriaFiltroItem(
                        categoria.ToString(),
                        categoria));
            }

            _cmbCategoria.DisplayMember =
                "Texto";

            _cmbCategoria.SelectedIndex = 0;

            _cmbEstado.Items.Clear();

            _cmbEstado.Items.Add(
                new EstadoFiltroItem(
                    "Todos",
                    null));

            foreach (
                EstadoViatico estado
                in Enum.GetValues(
                    typeof(EstadoViatico)))
            {
                _cmbEstado.Items.Add(
                    new EstadoFiltroItem(
                        estado.ToString(),
                        estado));
            }

            _cmbEstado.DisplayMember =
                "Texto";

            _cmbEstado.SelectedIndex = 0;
        }

        private void CargarViajes()
        {
            CambiarEstadoCarga(
                true);

            try
            {
                List<ViajeListadoDto> viajes =
                    _viajeService
                        .Listar(
                            new ViajeFiltro(
                                null,
                                null,
                                EstadoViaje.Abierto,
                                null))
                        .OrderBy(
                            viaje =>
                                viaje.FechaInicio)
                        .ThenBy(
                            viaje =>
                                viaje.IdViaje)
                        .ToList();

                _cmbViaje.DataSource =
                    null;

                _cmbViaje.DataSource =
                    viajes;

                _cmbViaje.DisplayMember =
                    "Descripcion";

                if (viajes.Count == 0)
                {
                    LimpiarResultados();

                    _lblViaje.Text =
                        "No existen viajes abiertos disponibles.";

                    MessageBox.Show(
                        "No existen viajes abiertos disponibles para gestionar viáticos.",
                        "Viáticos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    _cmbViaje.SelectedIndex = 0;
                    CargarViaticos();
                }
            }
            catch (Exception exception)
            {
                LimpiarResultados();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void CmbViaje_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ActualizarDescripcionViaje();
        }

        private void BtnConsultar_Click(
            object sender,
            EventArgs e)
        {
            CargarViaticos();
        }

        private void BtnActualizar_Click(
            object sender,
            EventArgs e)
        {
            CargarViajes();
        }

        private void BtnLimpiar_Click(
            object sender,
            EventArgs e)
        {
            _dtpFechaDesde.Checked = false;
            _dtpFechaHasta.Checked = false;
            _cmbCategoria.SelectedIndex = 0;
            _cmbEstado.SelectedIndex = 0;

            CargarViaticos();
        }

        private void CargarViaticos()
        {
            ViajeListadoDto viaje =
                ViajeSeleccionado;

            if (viaje == null)
            {
                LimpiarResultados();
                return;
            }

            CambiarEstadoCarga(
                true);

            try
            {
                var filtro =
                    new ViaticoFiltro(
                        _dtpFechaDesde.Checked
                            ? _dtpFechaDesde
                                .Value
                                .Date
                            : (DateTime?)null,
                        _dtpFechaHasta.Checked
                            ? _dtpFechaHasta
                                .Value
                                .Date
                            : (DateTime?)null,
                        ObtenerCategoriaFiltro(),
                        ObtenerEstadoFiltro());

                List<ViaticoListadoDto> viaticos =
                    _viaticoService
                        .ListarPorViaje(
                            viaje.IdViaje,
                            filtro)
                        .ToList();

                _grilla.DataSource = null;
                _grilla.DataSource =
                    viaticos;

                _lblResultados.Text =
                    "Resultados: " +
                    viaticos.Count;

                CargarResumenViaje(
                    viaje.IdViaje);

                ActualizarDescripcionViaje();
            }
            catch (Exception exception)
            {
                LimpiarResultados();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void CargarResumenViaje(
            int idViaje)
        {
            Viaje viaje =
                _viajeService.Obtener(
                    idViaje);

            _lblMontoAnticipado.Text =
                "Monto anticipado: " +
                viaje.MontoAnticipado
                    .ToString("N2");

            _lblTotalGastado.Text =
                "Total gastado vigente: " +
                viaje.TotalGastado
                    .ToString("N2");

            _lblSaldo.Text =
                "Saldo: " +
                viaje.SaldoPendiente
                    .ToString("N2");
        }

        private void ActualizarDescripcionViaje()
        {
            ViajeListadoDto viaje =
                ViajeSeleccionado;

            if (viaje == null)
            {
                _lblViaje.Text =
                    "Seleccione un viaje abierto.";

                return;
            }

            _lblViaje.Text =
                "Viaje " +
                viaje.IdViaje +
                " | " +
                viaje.FechaInicio
                    .ToString("dd/MM/yyyy") +
                " al " +
                viaje.FechaFin
                    .ToString("dd/MM/yyyy") +
                " | " +
                viaje.Descripcion +
                " | Participantes: " +
                viaje.ParticipantesResumen;
        }

        private CategoriaGasto?
            ObtenerCategoriaFiltro()
        {
            CategoriaFiltroItem item =
                _cmbCategoria.SelectedItem
                    as CategoriaFiltroItem;

            return item == null
                ? null
                : item.Categoria;
        }

        private EstadoViatico?
            ObtenerEstadoFiltro()
        {
            EstadoFiltroItem item =
                _cmbEstado.SelectedItem
                    as EstadoFiltroItem;

            return item == null
                ? null
                : item.Estado;
        }

        private ViaticoListadoDto
            ObtenerViaticoSeleccionado()
        {
            if (_grilla.CurrentRow == null)
            {
                return null;
            }

            return _grilla.CurrentRow
                .DataBoundItem
                as ViaticoListadoDto;
        }

        private void BtnNuevo_Click(
            object sender,
            EventArgs e)
        {
            ViajeListadoDto viaje =
                ViajeSeleccionado;

            if (viaje == null)
            {
                MessageBox.Show(
                    "Seleccione un viaje abierto.",
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (
                var formulario =
                    new ViaticoEditForm(
                        _viaticoService,
                        viaje.IdViaje,
                        viaje.FechaInicio,
                        viaje.FechaFin,
                        viaje.Descripcion,
                        null))
            {
                if (formulario.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    CargarViaticos();
                }
            }
        }

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            ViajeListadoDto viaje =
                ViajeSeleccionado;

            ViaticoListadoDto seleccionado =
                ObtenerViaticoSeleccionado();

            if (viaje == null)
            {
                MessageBox.Show(
                    "Seleccione un viaje abierto.",
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (seleccionado == null)
            {
                MessageBox.Show(
                    "Seleccione un viático de la lista.",
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (seleccionado.Estado !=
                EstadoViatico.Vigente)
            {
                MessageBox.Show(
                    "Solo pueden modificarse viáticos vigentes.",
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                Viatico viatico =
                    _viaticoService.Obtener(
                        seleccionado.IdViatico);

                using (
                    var formulario =
                        new ViaticoEditForm(
                            _viaticoService,
                            viaje.IdViaje,
                            viaje.FechaInicio,
                            viaje.FechaFin,
                            viaje.Descripcion,
                            viatico))
                {
                    if (formulario.ShowDialog(this) ==
                        DialogResult.OK)
                    {
                        CargarViaticos();
                    }
                }
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnDetalle_Click(
            object sender,
            EventArgs e)
        {
            ViaticoListadoDto seleccionado =
                ObtenerViaticoSeleccionado();

            if (seleccionado == null)
            {
                MessageBox.Show(
                    "Seleccione un viático de la lista.",
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                Viatico viatico =
                    _viaticoService.Obtener(
                        seleccionado.IdViatico);

                MessageBox.Show(
                    CrearDetalleViatico(
                        viatico),
                    "Detalle del viático",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnEnviarRendicion_Click(
            object sender,
            EventArgs e)
        {
            ViajeListadoDto viaje =
                ViajeSeleccionado;

            if (viaje == null)
            {
                MessageBox.Show(
                    "Seleccione un viaje.",
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult confirmacion =
                MessageBox.Show(
                    "¿Desea enviar el viaje seleccionado a rendición?" +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Luego del envío no podrán registrarse ni modificarse viáticos.",
                    "Confirmar envío",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (confirmacion !=
                DialogResult.Yes)
            {
                return;
            }

            CambiarEstadoCarga(
                true);

            try
            {
                _rendicionService.Enviar(
                    new EnviarRendicionCommand(
                        viaje.IdViaje));

                MessageBox.Show(
                    "El viaje fue enviado a rendición correctamente.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarViajes();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private static string CrearDetalleViatico(
            Viatico viatico)
        {
            var detalle =
                new StringBuilder();

            detalle.AppendLine(
                "Identificador: " +
                viatico.IdViatico);

            detalle.AppendLine(
                "Fecha: " +
                viatico.Fecha
                    .ToString("dd/MM/yyyy"));

            detalle.AppendLine(
                "Categoría: " +
                viatico.Categoria);

            detalle.AppendLine(
                "Método de pago: " +
                viatico.MetodoPago);

            detalle.AppendLine(
                "Pagado por: " +
                (
                    viatico.PagadoPor == null
                        ? "No corresponde"
                        : viatico.PagadoPor
                            .Apellido +
                          ", " +
                          viatico.PagadoPor
                            .Nombre
                ));

            detalle.AppendLine(
                "Monto: " +
                viatico.Monto
                    .ToString("N2"));

            detalle.AppendLine(
                "Estado: " +
                viatico.Estado);

            detalle.AppendLine(
                "Descripción: " +
                viatico.Descripcion);

            detalle.AppendLine(
                "Comprobante: " +
                (
                    viatico.Comprobante == null
                        ? "No"
                        : "Sí"
                ));

            if (viatico.Comprobante != null)
            {
                detalle.AppendLine();
                detalle.AppendLine(
                    "Tipo: " +
                    viatico.Comprobante.Tipo);

                detalle.AppendLine(
                    "Proveedor: " +
                    viatico.Comprobante
                        .RazonSocialProveedor);

                detalle.AppendLine(
                    "CUIT: " +
                    viatico.Comprobante
                        .CuitProveedor);

                detalle.AppendLine(
                    "Número: " +
                    viatico.Comprobante
                        .Sucursal +
                    "-" +
                    viatico.Comprobante
                        .Numero);

                detalle.AppendLine(
                    "Total comprobante: " +
                    viatico.Comprobante
                        .Total
                        .ToString("N2"));
            }

            return detalle.ToString();
        }

        private void LimpiarResultados()
        {
            _grilla.DataSource = null;

            _lblResultados.Text =
                "Resultados: 0";

            _lblMontoAnticipado.Text =
                "Monto anticipado: 0,00";

            _lblTotalGastado.Text =
                "Total gastado vigente: 0,00";

            _lblSaldo.Text =
                "Saldo: 0,00";
        }

        private void CambiarEstadoCarga(
            bool cargando)
        {
            _btnConsultar.Enabled =
                !cargando;

            _btnActualizar.Enabled =
                !cargando;

            _btnLimpiar.Enabled =
                !cargando;

            _btnNuevo.Enabled =
                !cargando;

            _btnModificar.Enabled =
                !cargando;

            _btnDetalle.Enabled =
                !cargando;

            _btnEnviarRendicion.Enabled =
                !cargando;

            _cmbViaje.Enabled =
                !cargando;

            UseWaitCursor =
                cargando;
        }

        private static void MostrarErrorControlado(
            Exception exception)
        {
            if (exception is
                    ReglaNegocioException ||
                exception is
                    AccesoDenegadoException)
            {
                MessageBox.Show(
                    exception.Message,
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is
                PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible acceder a los datos de viáticos. " +
                    "Verifique la conexión con SQL Server e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado en el módulo de viáticos.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private sealed class
            CategoriaFiltroItem
        {
            public CategoriaFiltroItem(
                string texto,
                CategoriaGasto? categoria)
            {
                Texto = texto;
                Categoria = categoria;
            }

            public string Texto
            {
                get;
                private set;
            }

            public CategoriaGasto?
                Categoria
            {
                get;
                private set;
            }
        }

        private sealed class EstadoFiltroItem
        {
            public EstadoFiltroItem(
                string texto,
                EstadoViatico? estado)
            {
                Texto = texto;
                Estado = estado;
            }

            public string Texto
            {
                get;
                private set;
            }

            public EstadoViatico? Estado
            {
                get;
                private set;
            }
        }
    }
}
