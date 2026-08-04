using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Reportes;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.WinForms.Controls;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ReportesForm : Form
    {
        private static readonly CultureInfo
            CulturaArgentina =
                CultureInfo.GetCultureInfo(
                    "es-AR");

        private static readonly Color
            ColorPrimario =
                Color.FromArgb(
                    37,
                    99,
                    235);

        private static readonly Color
            ColorFondo =
                Color.FromArgb(
                    245,
                    247,
                    250);

        private static readonly Color
            ColorTarjeta =
                Color.White;

        private static readonly Color
            ColorBorde =
                Color.FromArgb(
                    218,
                    223,
                    230);

        private static readonly Color
            ColorTextoSecundario =
                Color.FromArgb(
                    85,
                    94,
                    108);

        private readonly ReporteService
            _reporteService;

        private readonly ViajeService
            _viajeService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private readonly bool
            _puedeResumen;

        private readonly bool
            _puedeFinanciero;

        private TabControl
            _tabPrincipal;

        private Button
            _btnCerrar;

        private TextBox
            _txtResumenBuscar;

        private DateTimePicker
            _dtpResumenFechaDesde;

        private DateTimePicker
            _dtpResumenFechaHasta;

        private ComboBox
            _cmbResumenEstado;

        private Button
            _btnBuscarViajesResumen;

        private Button
            _btnLimpiarViajesResumen;

        private Button
            _btnConsultarResumen;

        private Label
            _lblViajeSeleccionado;

        private DataGridView
            _gridViajesResumen;

        private Label
            _lblResumenId;

        private Label
            _lblResumenDescripcion;

        private Label
            _lblResumenTipo;

        private Label
            _lblResumenEstado;

        private Label
            _lblResumenFechas;

        private Label
            _lblResumenEnvio;

        private Label
            _lblResumenAprobacion;

        private Label
            _lblResumenCancelacion;

        private Label
            _lblResumenMotivoCancelacion;

        private Label
            _lblCantidadParticipantes;

        private Label
            _lblCantidadVisitas;

        private Label
            _lblCantidadClientes;

        private Label
            _lblAnticipoResumen;

        private Label
            _lblTotalGastosResumen;

        private Label
            _lblSaldoResumen;

        private Label
            _lblReglaSaldoResumen;

        private DataGridView
            _gridParticipantes;

        private DataGridView
            _gridVisitas;

        private DataGridView
            _gridClientes;

        private DataGridView
            _gridViaticosResumen;

        private DateTimePicker
            _dtpFechaDesde;

        private DateTimePicker
            _dtpFechaHasta;

        private ComboBox
            _cmbViajeViatico;

        private ComboBox
            _cmbPersonaPagadora;

        private ComboBox
            _cmbCategoria;

        private ComboBox
            _cmbMetodoPago;

        private ComboBox
            _cmbEstadoViatico;

        private ComboBox
            _cmbEstadoViaje;

        private Button
            _btnConsultarViaticos;

        private Button
            _btnLimpiarViaticos;

        private Label
            _lblCantidadReporte;

        private Label
            _lblTotalRegistradoReporte;

        private Label
            _lblTotalVigenteReporte;

        private Label
            _lblTotalExcluidoReporte;

        private Label
            _lblPromedioReporte;

        private Label
            _lblCategoriaMayorReporte;

        private DataGridView
            _gridViaticosReporte;

        private DateTimePicker
            _dtpAnalisisDesde;

        private DateTimePicker
            _dtpAnalisisHasta;

        private ComboBox
            _cmbAreaAnalisis;

        private ComboBox
            _cmbIndicadorAnalisis;

        private ComboBox
            _cmbAgrupacionAnalisis;

        private ComboBox
            _cmbTopAnalisis;

        private Button
            _btnGenerarAnalisis;

        private Button
            _btnLimpiarAnalisis;

        private Label
            _lblAyudaAnalisis;

        private Label
            _lblTotalAnalisis;

        private Label
            _lblMaximoAnalisis;

        private Label
            _lblPromedioAnalisis;

        private Label
            _lblDestacadoAnalisis;

        private Label
            _lblEstadoAnalisis;

        private Chart
            _chartAnalisis;

        private DataGridView
            _gridRankingAnalisis;

        private bool
            _cargandoConfiguracionAnalisis;

        public ReportesForm(
            ReporteService reporteService,
            ViajeService viajeService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _reporteService =
                reporteService
                ?? throw new ArgumentNullException(
                    nameof(reporteService));

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

            if (!_sesionActual.HayUsuarioAutenticado ||
                _sesionActual.UsuarioActual == null)
            {
                throw new InvalidOperationException(
                    "No existe una sesión autenticada.");
            }

            _puedeResumen =
                TienePermiso(
                    ReporteService
                        .PermisoResumenViaje);

            _puedeFinanciero =
                TienePermiso(
                    ReporteService
                        .PermisoReporteViaticos)
                ||
                TienePermiso(
                    ReporteService
                        .PermisoReporteRendiciones);

            if (!_puedeResumen &&
                !_puedeFinanciero)
            {
                throw new AccesoDenegadoException(
                    "No posee permisos para consultar Reportes.");
            }

            InicializarFormulario();

            Load +=
                ReportesForm_Load;
        }

        private bool TienePermiso(
            string codigo)
        {
            return _autorizacionService
                .TienePermiso(
                    _sesionActual.UsuarioActual,
                    codigo);
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Reportes";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(
                    1100,
                    700);

            Size =
                new Size(
                    1400,
                    820);

            AutoScaleMode =
                AutoScaleMode.Dpi;

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                ColorFondo;

            var raiz =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding =
                        new Padding(12),
                    BackColor =
                        ColorFondo
                };

            raiz.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    72F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    50F));

            raiz.Controls.Add(
                CrearEncabezado(),
                0,
                0);

            _tabPrincipal =
                new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            9.5F)
                };

            if (_puedeResumen)
            {
                _tabPrincipal.TabPages.Add(
                    CrearTabResumen());
            }

            if (_puedeFinanciero)
            {
                _tabPrincipal.TabPages.Add(
                    CrearTabReporteViaticos());
            }

            _tabPrincipal.TabPages.Add(
                CrearTabAnalisis());

            raiz.Controls.Add(
                _tabPrincipal,
                0,
                1);

            raiz.Controls.Add(
                CrearPie(),
                0,
                2);

            Controls.Add(
                raiz);

            AcceptButton =
                _puedeResumen
                    ? _btnConsultarResumen
                    : null;

            CancelButton =
                _btnCerrar;
        }

        private Control CrearEncabezado()
        {
            var panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor =
                        ColorTarjeta,
                    Padding =
                        new Padding(
                            18,
                            8,
                            18,
                            8)
                };

            var titulo =
                new Label
                {
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            20F,
                            FontStyle.Bold),
                    ForeColor =
                        Color.FromArgb(
                            30,
                            41,
                            59),
                    Location =
                        new Point(
                            18,
                            7),
                    Text =
                        "Reportes"
                };

            var subtitulo =
                new Label
                {
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            9.5F),
                    ForeColor =
                        ColorTextoSecundario,
                    Location =
                        new Point(
                            20,
                            45),
                    Text =
                        "Resumen operativo y análisis de gastos y actividad comercial."
                };

            panel.Controls.Add(
                subtitulo);

            panel.Controls.Add(
                titulo);

            return panel;
        }

        private Control CrearPie()
        {
            var panel =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection =
                        FlowDirection.RightToLeft,
                    WrapContents = false,
                    Padding =
                        new Padding(
                            0,
                            8,
                            4,
                            0),
                    BackColor =
                        ColorFondo
                };

            _btnCerrar =
                CrearBotonSecundario(
                    "Cerrar",
                    100);

            _btnCerrar.DialogResult =
                DialogResult.Cancel;

            _btnCerrar.Click +=
                (sender, args) =>
                    Close();

            panel.Controls.Add(
                _btnCerrar);

            return panel;
        }

        private TabPage CrearTabResumen()
        {
            var tab =
                CrearTabBase(
                    "Resumen de viaje");

            var raiz =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding =
                        new Padding(10),
                    BackColor =
                        ColorFondo
                };

            raiz.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    250F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    210F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            raiz.Controls.Add(
                CrearSelectorResumen(),
                0,
                0);

            var bloqueCentral =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor =
                        ColorFondo
                };

            bloqueCentral.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    68F));

            bloqueCentral.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    32F));

            bloqueCentral.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            bloqueCentral.Controls.Add(
                CrearInformacionResumen(),
                0,
                0);

            bloqueCentral.Controls.Add(
                CrearIndicadoresResumen(),
                1,
                0);

            raiz.Controls.Add(
                bloqueCentral,
                0,
                1);

            raiz.Controls.Add(
                CrearDetalleResumen(),
                0,
                2);

            tab.Controls.Add(
                raiz);

            return tab;
        }

        private Control CrearSelectorResumen()
        {
            var grupo =
                CrearGrupo(
                    "Seleccionar viaje");

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 5,
                    RowCount = 4,
                    Padding =
                        new Padding(
                            10,
                            5,
                            10,
                            8),
                    BackColor =
                        ColorTarjeta
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    26F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    13F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    13F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    13F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    35F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    22F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    38F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    26F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Buscar por ID, descripción o participante"),
                0,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Fecha desde"),
                1,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Fecha hasta"),
                2,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Estado"),
                3,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Acciones"),
                4,
                0);

            _txtResumenBuscar =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(
                            4,
                            3,
                            6,
                            3)
                };

            _dtpResumenFechaDesde =
                CrearFechaFiltro();

            _dtpResumenFechaHasta =
                CrearFechaFiltro();

            _cmbResumenEstado =
                CrearCombo();

            tabla.Controls.Add(
                _txtResumenBuscar,
                0,
                1);

            tabla.Controls.Add(
                _dtpResumenFechaDesde,
                1,
                1);

            tabla.Controls.Add(
                _dtpResumenFechaHasta,
                2,
                1);

            tabla.Controls.Add(
                _cmbResumenEstado,
                3,
                1);

            var acciones =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 1,
                    Margin =
                        new Padding(
                            2,
                            1,
                            2,
                            1),
                    BackColor =
                        ColorTarjeta
                };

            acciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    29F));

            acciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    29F));

            acciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    42F));

            acciones.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            _btnBuscarViajesResumen =
                CrearBotonPrimario(
                    "Buscar",
                    90);

            _btnLimpiarViajesResumen =
                CrearBotonSecundario(
                    "Limpiar",
                    90);

            _btnConsultarResumen =
                CrearBotonPrimario(
                    "Consultar selección",
                    135);

            _btnBuscarViajesResumen.Dock =
                DockStyle.Fill;

            _btnLimpiarViajesResumen.Dock =
                DockStyle.Fill;

            _btnConsultarResumen.Dock =
                DockStyle.Fill;

            _btnBuscarViajesResumen.Margin =
                new Padding(2);

            _btnLimpiarViajesResumen.Margin =
                new Padding(2);

            _btnConsultarResumen.Margin =
                new Padding(2);

            _btnBuscarViajesResumen.Click +=
                BtnBuscarViajesResumen_Click;

            _btnLimpiarViajesResumen.Click +=
                BtnLimpiarViajesResumen_Click;

            _btnConsultarResumen.Click +=
                BtnConsultarResumen_Click;

            acciones.Controls.Add(
                _btnBuscarViajesResumen,
                0,
                0);

            acciones.Controls.Add(
                _btnLimpiarViajesResumen,
                1,
                0);

            acciones.Controls.Add(
                _btnConsultarResumen,
                2,
                0);

            tabla.Controls.Add(
                acciones,
                4,
                1);

            _lblViajeSeleccionado =
                new Label
                {
                    Dock = DockStyle.Fill,
                    ForeColor =
                        ColorTextoSecundario,
                    Text =
                        "Seleccione una fila para consultar su resumen.",
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    AutoEllipsis = true,
                    Padding =
                        new Padding(
                            4,
                            2,
                            0,
                            0)
                };

            tabla.Controls.Add(
                _lblViajeSeleccionado,
                0,
                2);

            tabla.SetColumnSpan(
                _lblViajeSeleccionado,
                5);

            _gridViajesResumen =
                CrearGridBase();

            AgregarColumnaTexto(
                _gridViajesResumen,
                "IdViaje",
                "ID",
                65);

            AgregarColumnaFecha(
                _gridViajesResumen,
                "FechaInicio",
                "Inicio",
                92);

            AgregarColumnaFecha(
                _gridViajesResumen,
                "FechaFin",
                "Fin",
                92);

            AgregarColumnaTexto(
                _gridViajesResumen,
                "Descripcion",
                "Descripción",
                285);

            AgregarColumnaTexto(
                _gridViajesResumen,
                "TipoViaje",
                "Tipo",
                120);

            AgregarColumnaTexto(
                _gridViajesResumen,
                "ParticipantesResumen",
                "Participantes",
                260);

            AgregarColumnaMoneda(
                _gridViajesResumen,
                "MontoAnticipado",
                "Anticipo",
                110);

            AgregarColumnaTexto(
                _gridViajesResumen,
                "Estado",
                "Estado",
                105);

            _gridViajesResumen.SelectionChanged +=
                GridViajesResumen_SelectionChanged;

            _gridViajesResumen.CellDoubleClick +=
                GridViajesResumen_CellDoubleClick;

            tabla.Controls.Add(
                _gridViajesResumen,
                0,
                3);

            tabla.SetColumnSpan(
                _gridViajesResumen,
                5);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearInformacionResumen()
        {
            var grupo =
                CrearGrupo(
                    "Información del viaje");

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = 5,
                    Padding =
                        new Padding(
                            12,
                            6,
                            12,
                            8),
                    BackColor =
                        ColorTarjeta
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    110F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    110F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            for (int indice = 0;
                indice < 5;
                indice++)
            {
                tabla.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        20F));
            }

            _lblResumenId =
                CrearEtiquetaValor();

            _lblResumenDescripcion =
                CrearEtiquetaValor();

            _lblResumenTipo =
                CrearEtiquetaValor();

            _lblResumenEstado =
                CrearEtiquetaValor();

            _lblResumenFechas =
                CrearEtiquetaValor();

            _lblResumenEnvio =
                CrearEtiquetaValor();

            _lblResumenAprobacion =
                CrearEtiquetaValor();

            _lblResumenCancelacion =
                CrearEtiquetaValor();

            _lblResumenMotivoCancelacion =
                CrearEtiquetaValor();

            AgregarDato(
                tabla,
                0,
                0,
                "ID:",
                _lblResumenId);

            AgregarDato(
                tabla,
                0,
                2,
                "Estado:",
                _lblResumenEstado);

            AgregarDato(
                tabla,
                1,
                0,
                "Descripción:",
                _lblResumenDescripcion);

            AgregarDato(
                tabla,
                1,
                2,
                "Tipo:",
                _lblResumenTipo);

            AgregarDato(
                tabla,
                2,
                0,
                "Fechas:",
                _lblResumenFechas);

            AgregarDato(
                tabla,
                2,
                2,
                "Envío:",
                _lblResumenEnvio);

            AgregarDato(
                tabla,
                3,
                0,
                "Aprobación:",
                _lblResumenAprobacion);

            AgregarDato(
                tabla,
                3,
                2,
                "Cancelación:",
                _lblResumenCancelacion);

            AgregarDato(
                tabla,
                4,
                0,
                "Motivo:",
                _lblResumenMotivoCancelacion);

            tabla.SetColumnSpan(
                _lblResumenMotivoCancelacion,
                3);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearIndicadoresResumen()
        {
            var grupo =
                CrearGrupo(
                    "Resumen ejecutivo");

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 5,
                    Padding =
                        new Padding(
                            6,
                            5,
                            6,
                            6),
                    BackColor =
                        ColorTarjeta
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            for (int indice = 0;
                indice < 4;
                indice++)
            {
                tabla.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        17F));
            }

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    32F));

            tabla.Controls.Add(
                CrearFilaIndicadorResumen(
                    "Participantes",
                    out _lblCantidadParticipantes),
                0,
                0);

            tabla.Controls.Add(
                CrearFilaIndicadorResumen(
                    "Visitas",
                    out _lblCantidadVisitas),
                0,
                1);

            tabla.Controls.Add(
                CrearFilaIndicadorResumen(
                    "Clientes distintos",
                    out _lblCantidadClientes),
                0,
                2);

            tabla.Controls.Add(
                CrearFilaFinancieraResumen(),
                0,
                3);

            tabla.Controls.Add(
                CrearTarjetaSaldo(),
                0,
                4);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearFilaFinancieraResumen()
        {
            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    Margin =
                        new Padding(0),
                    Padding =
                        new Padding(0),
                    BackColor =
                        ColorTarjeta
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            tabla.Controls.Add(
                CrearFilaIndicadorResumen(
                    "Anticipado",
                    out _lblAnticipoResumen),
                0,
                0);

            tabla.Controls.Add(
                CrearFilaIndicadorResumen(
                    "Total gastos",
                    out _lblTotalGastosResumen),
                1,
                0);

            return tabla;
        }

        private Control CrearTarjetaSaldo()
        {
            var panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(3),
                    Padding =
                        new Padding(
                            8,
                            3,
                            8,
                            3),
                    BackColor =
                        Color.FromArgb(
                            235,
                            244,
                            255),
                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            var contenido =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor =
                        Color.FromArgb(
                            235,
                            244,
                            255)
                };

            contenido.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            contenido.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    26F));

            contenido.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            var cabecera =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor =
                        Color.FromArgb(
                            235,
                            244,
                            255)
                };

            cabecera.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    28F));

            cabecera.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    72F));

            cabecera.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            cabecera.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F),
                    ForeColor =
                        Color.FromArgb(
                            30,
                            64,
                            175),
                    Text =
                        "Saldo",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                },
                0,
                0);

            _lblSaldoResumen =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            10.5F,
                            FontStyle.Bold),
                    ForeColor =
                        Color.FromArgb(
                            30,
                            64,
                            175),
                    Text =
                        "Sin consultar",
                    TextAlign =
                        ContentAlignment.MiddleRight,
                    AutoEllipsis = true
                };

            cabecera.Controls.Add(
                _lblSaldoResumen,
                1,
                0);

            _lblReglaSaldoResumen =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            7.5F),
                    ForeColor =
                        ColorTextoSecundario,
                    Text =
                        "Solo considera gastos vigentes no pagados con Tarjeta corporativa.",
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };

            contenido.Controls.Add(
                cabecera,
                0,
                0);

            contenido.Controls.Add(
                _lblReglaSaldoResumen,
                0,
                1);

            panel.Controls.Add(
                contenido);

            return panel;
        }

        private Control CrearDetalleResumen()
        {
            var detalle =
                new TabControl
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F)
                };

            _gridParticipantes =
                CrearGridBase();

            AgregarColumnaTexto(
                _gridParticipantes,
                "NombreCompleto",
                "Apellido y nombre",
                260);

            AgregarColumnaTexto(
                _gridParticipantes,
                "Email",
                "Email",
                260);

            AgregarColumnaCheck(
                _gridParticipantes,
                "Activo",
                "Activo",
                75);

            _gridVisitas =
                CrearGridBase();

            AgregarColumnaTexto(
                _gridVisitas,
                "IdVisita",
                "ID",
                65);

            AgregarColumnaFecha(
                _gridVisitas,
                "Fecha",
                "Fecha",
                100);

            AgregarColumnaTexto(
                _gridVisitas,
                "LocalidadEncuentro",
                "Localidad",
                170);

            AgregarColumnaTexto(
                _gridVisitas,
                "Observacion",
                "Observación",
                520);

            _gridClientes =
                CrearGridBase();

            AgregarColumnaTexto(
                _gridClientes,
                "RazonSocial",
                "Razón social",
                270);

            AgregarColumnaTexto(
                _gridClientes,
                "Cuit",
                "CUIT",
                140);

            AgregarColumnaTexto(
                _gridClientes,
                "Localidad",
                "Localidad",
                170);

            AgregarColumnaTexto(
                _gridClientes,
                "Provincia",
                "Provincia",
                170);

            AgregarColumnaCheck(
                _gridClientes,
                "Activo",
                "Activo",
                75);

            _gridViaticosResumen =
                CrearGridBase();

            AgregarColumnaFecha(
                _gridViaticosResumen,
                "Fecha",
                "Fecha",
                100);

            AgregarColumnaTexto(
                _gridViaticosResumen,
                "Categoria",
                "Categoría",
                130);

            AgregarColumnaTexto(
                _gridViaticosResumen,
                "MetodoPago",
                "Método",
                155);

            AgregarColumnaTexto(
                _gridViaticosResumen,
                "PagadoPor",
                "Persona pagadora",
                220);

            AgregarColumnaMoneda(
                _gridViaticosResumen,
                "Monto",
                "Monto",
                120);

            AgregarColumnaTexto(
                _gridViaticosResumen,
                "Estado",
                "Estado",
                105);

            AgregarColumnaTexto(
                _gridViaticosResumen,
                "Descripcion",
                "Descripción",
                330);

            detalle.TabPages.Add(
                CrearTabConControl(
                    "Participantes",
                    _gridParticipantes));

            detalle.TabPages.Add(
                CrearTabConControl(
                    "Visitas",
                    _gridVisitas));

            detalle.TabPages.Add(
                CrearTabConControl(
                    "Clientes visitados",
                    _gridClientes));

            detalle.TabPages.Add(
                CrearTabConControl(
                    "Viáticos",
                    _gridViaticosResumen));

            return detalle;
        }

        private TabPage CrearTabReporteViaticos()
        {
            var tab =
                CrearTabBase(
                    "Reporte de viáticos");

            var raiz =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding =
                        new Padding(10),
                    BackColor =
                        ColorFondo
                };

            raiz.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    154F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    170F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            raiz.Controls.Add(
                CrearFiltrosViaticos(),
                0,
                0);

            raiz.Controls.Add(
                CrearIndicadoresViaticos(),
                0,
                1);

            _gridViaticosReporte =
                CrearGridBase();

            AgregarColumnaTexto(
                _gridViaticosReporte,
                "IdViaje",
                "ID viaje",
                75);

            AgregarColumnaFecha(
                _gridViaticosReporte,
                "Fecha",
                "Fecha",
                100);

            AgregarColumnaTexto(
                _gridViaticosReporte,
                "Viaje",
                "Viaje",
                250);

            AgregarColumnaTexto(
                _gridViaticosReporte,
                "PersonaPagadora",
                "Persona",
                220);

            AgregarColumnaTexto(
                _gridViaticosReporte,
                "Categoria",
                "Categoría",
                125);

            AgregarColumnaTexto(
                _gridViaticosReporte,
                "MetodoPago",
                "Método",
                150);

            AgregarColumnaMoneda(
                _gridViaticosReporte,
                "Monto",
                "Monto",
                120);

            AgregarColumnaTexto(
                _gridViaticosReporte,
                "EstadoViatico",
                "Estado",
                105);

            AgregarColumnaTexto(
                _gridViaticosReporte,
                "Descripcion",
                "Descripción",
                330);

            raiz.Controls.Add(
                _gridViaticosReporte,
                0,
                2);

            tab.Controls.Add(
                raiz);

            return tab;
        }

        private Control CrearFiltrosViaticos()
        {
            var grupo =
                CrearGrupo(
                    "Filtros");

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 5,
                    RowCount = 2,
                    Padding =
                        new Padding(
                            8,
                            5,
                            8,
                            7),
                    BackColor =
                        ColorTarjeta
                };

            for (int indice = 0;
                indice < 4;
                indice++)
            {
                tabla.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        25F));
            }

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    240F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            _dtpFechaDesde =
                CrearFechaFiltro();

            _dtpFechaHasta =
                CrearFechaFiltro();

            _cmbViajeViatico =
                CrearCombo();

            _cmbPersonaPagadora =
                CrearCombo();

            _cmbCategoria =
                CrearCombo();

            _cmbMetodoPago =
                CrearCombo();

            _cmbEstadoViatico =
                CrearCombo();

            _cmbEstadoViaje =
                CrearCombo();

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Fecha desde",
                    _dtpFechaDesde),
                0,
                0);

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Fecha hasta",
                    _dtpFechaHasta),
                1,
                0);

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Viaje",
                    _cmbViajeViatico),
                2,
                0);

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Persona",
                    _cmbPersonaPagadora),
                3,
                0);

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Categoría",
                    _cmbCategoria),
                0,
                1);

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Método",
                    _cmbMetodoPago),
                1,
                1);

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Estado viático",
                    _cmbEstadoViatico),
                2,
                1);

            tabla.Controls.Add(
                CrearCampoFiltroCompacto(
                    "Estado viaje",
                    _cmbEstadoViaje),
                3,
                1);

            var acciones =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Margin =
                        new Padding(
                            8,
                            4,
                            0,
                            4),
                    BackColor =
                        ColorTarjeta
                };

            acciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            acciones.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            acciones.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            _btnConsultarViaticos =
                CrearBotonPrimario(
                    "Consultar",
                    125);

            _btnLimpiarViaticos =
                CrearBotonSecundario(
                    "Limpiar",
                    110);

            _btnConsultarViaticos.Dock =
                DockStyle.Fill;

            _btnLimpiarViaticos.Dock =
                DockStyle.Fill;

            _btnConsultarViaticos.Margin =
                new Padding(
                    8,
                    3,
                    8,
                    3);

            _btnLimpiarViaticos.Margin =
                new Padding(
                    8,
                    3,
                    8,
                    3);

            _btnConsultarViaticos.Click +=
                BtnConsultarViaticos_Click;

            _btnLimpiarViaticos.Click +=
                BtnLimpiarViaticos_Click;

            acciones.Controls.Add(
                _btnConsultarViaticos,
                0,
                0);

            acciones.Controls.Add(
                _btnLimpiarViaticos,
                0,
                1);

            tabla.Controls.Add(
                acciones,
                4,
                0);

            tabla.SetRowSpan(
                acciones,
                2);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearIndicadoresViaticos()
        {
            var grupo =
                CrearGrupo(
                    "Indicadores");

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 2,
                    Padding =
                        new Padding(
                            8,
                            5,
                            8,
                            7),
                    BackColor =
                        ColorTarjeta
                };

            for (int indice = 0;
                indice < 3;
                indice++)
            {
                tabla.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        33.333F));
            }

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Cantidad",
                    out _lblCantidadReporte),
                0,
                0);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Total registrado",
                    out _lblTotalRegistradoReporte),
                1,
                0);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Total vigente",
                    out _lblTotalVigenteReporte),
                2,
                0);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Total excluido",
                    out _lblTotalExcluidoReporte),
                0,
                1);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Promedio",
                    out _lblPromedioReporte),
                1,
                1);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Categoría con mayor gasto",
                    out _lblCategoriaMayorReporte),
                2,
                1);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private TabPage CrearTabAnalisis()
        {
            var tab =
                CrearTabBase(
                    "Análisis gráfico");

            var raiz =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding =
                        new Padding(10),
                    BackColor =
                        ColorFondo
                };

            raiz.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    202F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    106F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            raiz.Controls.Add(
                CrearConfiguracionAnalisis(),
                0,
                0);

            raiz.Controls.Add(
                CrearIndicadoresAnalisis(),
                0,
                1);

            raiz.Controls.Add(
                CrearResultadoAnalisis(),
                0,
                2);

            tab.Controls.Add(
                raiz);

            return tab;
        }

        private Control CrearConfiguracionAnalisis()
        {
            var grupo =
                CrearGrupo(
                    "Configuración del análisis");

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 6,
                    RowCount = 4,
                    Padding =
                        new Padding(
                            10,
                            6,
                            10,
                            8),
                    BackColor =
                        ColorTarjeta
                };

            for (int indice = 0;
                indice < 6;
                indice++)
            {
                tabla.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        indice < 2
                            ? 17F
                            : 16.5F));
            }

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    25F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    34F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    45F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Fecha desde"),
                0,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Fecha hasta"),
                1,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Área de análisis"),
                2,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Indicador"),
                3,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Agrupar por"),
                4,
                0);

            tabla.Controls.Add(
                CrearEtiquetaSimple(
                    "Top N"),
                5,
                0);

            _dtpAnalisisDesde =
                CrearFechaFiltro();

            _dtpAnalisisHasta =
                CrearFechaFiltro();

            _cmbAreaAnalisis =
                CrearCombo();

            _cmbIndicadorAnalisis =
                CrearCombo();

            _cmbAgrupacionAnalisis =
                CrearCombo();

            _cmbTopAnalisis =
                CrearCombo();

            tabla.Controls.Add(
                _dtpAnalisisDesde,
                0,
                1);

            tabla.Controls.Add(
                _dtpAnalisisHasta,
                1,
                1);

            tabla.Controls.Add(
                _cmbAreaAnalisis,
                2,
                1);

            tabla.Controls.Add(
                _cmbIndicadorAnalisis,
                3,
                1);

            tabla.Controls.Add(
                _cmbAgrupacionAnalisis,
                4,
                1);

            tabla.Controls.Add(
                _cmbTopAnalisis,
                5,
                1);

            var acciones =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection =
                        FlowDirection.LeftToRight,
                    WrapContents = false,
                    Padding =
                        new Padding(
                            0,
                            6,
                            0,
                            0)
                };

            _btnGenerarAnalisis =
                CrearBotonPrimario(
                    "Generar gráfico",
                    135);

            _btnLimpiarAnalisis =
                CrearBotonSecundario(
                    "Limpiar",
                    100);

            _btnGenerarAnalisis.Click +=
                BtnGenerarAnalisis_Click;

            _btnLimpiarAnalisis.Click +=
                BtnLimpiarAnalisis_Click;

            acciones.Controls.Add(
                _btnGenerarAnalisis);

            acciones.Controls.Add(
                _btnLimpiarAnalisis);

            tabla.Controls.Add(
                acciones,
                0,
                2);

            tabla.SetColumnSpan(
                acciones,
                2);

            _lblAyudaAnalisis =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(
                            8,
                            7,
                            0,
                            0),
                    ForeColor =
                        ColorTextoSecundario,
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };

            tabla.Controls.Add(
                _lblAyudaAnalisis,
                2,
                2);

            tabla.SetColumnSpan(
                _lblAyudaAnalisis,
                4);

            var ayuda =
                new Label
                {
                    Dock = DockStyle.Fill,
                    BackColor =
                        Color.FromArgb(
                            239,
                            246,
                            255),
                    ForeColor =
                        Color.FromArgb(
                            30,
                            64,
                            175),
                    Padding =
                        new Padding(
                            12,
                            7,
                            12,
                            7),
                    Text =
                        "Gastos: importe por categoría, empleado, viaje o mes.  " +
                        "Clientes: frecuencia de visitas.  " +
                        "Viajes: cantidad por empleado, tipo, estado o mes.  " +
                        "Empleados: viajes, visitas o gastos.",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            tabla.Controls.Add(
                ayuda,
                0,
                3);

            tabla.SetColumnSpan(
                ayuda,
                6);

            _cmbAreaAnalisis.SelectedIndexChanged +=
                CmbAreaAnalisis_SelectedIndexChanged;

            _cmbIndicadorAnalisis.SelectedIndexChanged +=
                CmbIndicadorAnalisis_SelectedIndexChanged;

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearIndicadoresAnalisis()
        {
            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = 1,
                    Padding =
                        new Padding(
                            0,
                            5,
                            0,
                            5),
                    BackColor =
                        ColorFondo
                };

            for (int indice = 0;
                indice < 4;
                indice++)
            {
                tabla.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        25F));
            }

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Total analizado",
                    out _lblTotalAnalisis),
                0,
                0);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Máximo",
                    out _lblMaximoAnalisis),
                1,
                0);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Promedio",
                    out _lblPromedioAnalisis),
                2,
                0);

            tabla.Controls.Add(
                CrearTarjetaIndicador(
                    "Elemento destacado",
                    out _lblDestacadoAnalisis),
                3,
                0);

            return tabla;
        }

        private Control CrearResultadoAnalisis()
        {
            var contenedor =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor =
                        ColorFondo
                };

            contenedor.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    65F));

            contenedor.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    35F));

            contenedor.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            var panelGrafico =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Padding =
                        new Padding(
                            0,
                            0,
                            5,
                            0),
                    BackColor =
                        ColorFondo
                };

            panelGrafico.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            panelGrafico.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    34F));

            panelGrafico.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            _lblEstadoAnalisis =
                new Label
                {
                    Dock = DockStyle.Fill,
                    BackColor =
                        ColorTarjeta,
                    ForeColor =
                        ColorTextoSecundario,
                    Padding =
                        new Padding(
                            10,
                            0,
                            10,
                            0),
                    Text =
                        "Seleccione la configuración y genere el gráfico.",
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            _chartAnalisis =
                new Chart
                {
                    Dock = DockStyle.Fill,
                    BackColor =
                        ColorTarjeta,
                    BorderlineColor =
                        ColorBorde,
                    BorderlineDashStyle =
                        ChartDashStyle.Solid,
                    BorderlineWidth = 1
                };

            ConfigurarChartBase();

            panelGrafico.Controls.Add(
                _lblEstadoAnalisis,
                0,
                0);

            panelGrafico.Controls.Add(
                _chartAnalisis,
                0,
                1);

            contenedor.Controls.Add(
                panelGrafico,
                0,
                0);

            var panelRanking =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Padding =
                        new Padding(
                            5,
                            0,
                            0,
                            0),
                    BackColor =
                        ColorFondo
                };

            panelRanking.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            panelRanking.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    34F));

            panelRanking.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            panelRanking.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    BackColor =
                        ColorTarjeta,
                    ForeColor =
                        Color.FromArgb(
                            30,
                            41,
                            59),
                    Font =
                        new Font(
                            "Segoe UI",
                            10F,
                            FontStyle.Bold),
                    Padding =
                        new Padding(
                            10,
                            0,
                            10,
                            0),
                    Text =
                        "Detalle del ranking",
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    BorderStyle =
                        BorderStyle.FixedSingle
                },
                0,
                0);

            _gridRankingAnalisis =
                CrearGridBase();

            AgregarColumnaTexto(
                _gridRankingAnalisis,
                "Posicion",
                "#",
                50);

            AgregarColumnaTexto(
                _gridRankingAnalisis,
                "Entidad",
                "Entidad",
                220);

            AgregarColumnaTexto(
                _gridRankingAnalisis,
                "ValorTexto",
                "Valor",
                105);

            AgregarColumnaTexto(
                _gridRankingAnalisis,
                "ParticipacionTexto",
                "Participación",
                105);

            panelRanking.Controls.Add(
                _gridRankingAnalisis,
                0,
                1);

            contenedor.Controls.Add(
                panelRanking,
                1,
                0);

            return contenedor;
        }

        private void ConfigurarChartBase()
        {
            _chartAnalisis.ChartAreas.Clear();
            _chartAnalisis.Legends.Clear();
            _chartAnalisis.Series.Clear();
            _chartAnalisis.Titles.Clear();

            var area =
                new ChartArea(
                    "Analisis");

            area.BackColor =
                ColorTarjeta;

            area.AxisX.MajorGrid.Enabled =
                false;

            area.AxisX.LabelStyle.Font =
                new Font(
                    "Segoe UI",
                    8F);

            area.AxisY.MajorGrid.LineColor =
                Color.FromArgb(
                    228,
                    232,
                    238);

            area.AxisY.MajorGrid.LineDashStyle =
                ChartDashStyle.Dot;

            area.AxisY.LabelStyle.Font =
                new Font(
                    "Segoe UI",
                    8F);

            area.AxisX.IsLabelAutoFit =
                true;

            area.AxisY.IsStartedFromZero =
                true;

            _chartAnalisis.ChartAreas.Add(
                area);
        }

        private void ReportesForm_Load(
            object sender,
            EventArgs e)
        {
            try
            {
                CambiarEstado(
                    true);

                if (_puedeResumen)
                {
                    CargarFiltrosResumen();
                    BuscarViajesResumenInterno();
                }

                if (_puedeFinanciero)
                {
                    CargarCatalogosViaticos();
                    ConsultarViaticosInterno();
                }

                CargarConfiguracionAnalisis();
                GenerarAnalisisInterno();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void CargarFiltrosResumen()
        {
            CargarEnum
                <EstadoViaje>(
                    _cmbResumenEstado,
                    "Todos");

            RestablecerFechas(
                _dtpResumenFechaDesde,
                _dtpResumenFechaHasta);

            _txtResumenBuscar.Clear();
        }

        private void BuscarViajesResumenInterno()
        {
            ViajeFiltro filtro =
                new ViajeFiltro(
                    _dtpResumenFechaDesde.Checked
                        ? _dtpResumenFechaDesde.Value
                        : (DateTime?)null,
                    _dtpResumenFechaHasta.Checked
                        ? _dtpResumenFechaHasta.Value
                        : (DateTime?)null,
                    ObtenerEnum
                        <EstadoViaje>(
                            _cmbResumenEstado),
                    null);

            List<ViajeListadoDto> viajes =
                _viajeService
                    .Listar(
                        filtro)
                    .ToList();

            string textoBusqueda =
                (_txtResumenBuscar.Text
                    ?? string.Empty)
                .Trim();

            if (!string.IsNullOrWhiteSpace(
                    textoBusqueda))
            {
                string idBuscado =
                    textoBusqueda
                        .TrimStart('#')
                        .Trim();

                viajes =
                    viajes
                        .Where(
                            item =>
                                CoincideTexto(
                                    item.Descripcion,
                                    textoBusqueda)
                                ||
                                CoincideTexto(
                                    item.ParticipantesResumen,
                                    textoBusqueda)
                                ||
                                CoincideTexto(
                                    FormatearTextoEnum(
                                        item.TipoViaje.ToString()),
                                    textoBusqueda)
                                ||
                                CoincideTexto(
                                    FormatearTextoEnum(
                                        item.Estado.ToString()),
                                    textoBusqueda)
                                ||
                                item.IdViaje
                                    .ToString(
                                        CulturaArgentina)
                                    .Equals(
                                        idBuscado,
                                        StringComparison
                                            .OrdinalIgnoreCase))
                        .ToList();
            }

            _gridViajesResumen.DataSource =
                CrearOrigenOrdenable(
                    viajes);

            _lblViajeSeleccionado.Text =
                viajes.Count == 0
                    ? "No se encontraron viajes con los filtros seleccionados."
                    : "Resultados: "
                        +
                        viajes.Count.ToString(
                            CulturaArgentina)
                        +
                        ". Seleccione una fila para consultar su resumen.";
        }

        private static bool CoincideTexto(
            string origen,
            string busqueda)
        {
            return !string.IsNullOrWhiteSpace(
                    origen)
                &&
                origen.IndexOf(
                    busqueda,
                    StringComparison
                        .CurrentCultureIgnoreCase)
                >= 0;
        }

        private void CargarCatalogosViaticos()
        {
            var viajes =
                new List<OpcionFiltro>
                {
                    new OpcionFiltro(
                        null,
                        "Todos")
                };

            viajes.AddRange(
                _reporteService
                    .ListarViajesParaReporteViaticos()
                    .Select(
                        item =>
                            new OpcionFiltro(
                                item.IdViaje,
                                item.Presentacion)));

            ConfigurarCombo(
                _cmbViajeViatico,
                viajes);

            var personas =
                new List<OpcionFiltro>
                {
                    new OpcionFiltro(
                        null,
                        "Todas")
                };

            personas.AddRange(
                _reporteService
                    .ListarPersonasPagadoras()
                    .Select(
                        item =>
                            new OpcionFiltro(
                                item.IdPersona,
                                item.Presentacion)));

            ConfigurarCombo(
                _cmbPersonaPagadora,
                personas);

            CargarEnum
                <CategoriaGasto>(
                    _cmbCategoria,
                    "Todas");

            CargarEnum
                <MetodoPago>(
                    _cmbMetodoPago,
                    "Todos");

            CargarEnum
                <EstadoViatico>(
                    _cmbEstadoViatico,
                    "Todos");

            CargarEnum
                <EstadoViaje>(
                    _cmbEstadoViaje,
                    "Todos");

            RestablecerFechas(
                _dtpFechaDesde,
                _dtpFechaHasta);
        }

        private void CargarConfiguracionAnalisis()
        {
            _cargandoConfiguracionAnalisis =
                true;

            try
            {
                var areas =
                    new List
                        <OpcionAnalisis
                            <ReporteAreaAnalisis>>();

                if (_puedeResumen)
                {
                    areas.Add(
                        new OpcionAnalisis
                            <ReporteAreaAnalisis>(
                                ReporteAreaAnalisis
                                    .Clientes,
                                "Clientes"));

                    areas.Add(
                        new OpcionAnalisis
                            <ReporteAreaAnalisis>(
                                ReporteAreaAnalisis
                                    .Viajes,
                                "Viajes"));

                    areas.Add(
                        new OpcionAnalisis
                            <ReporteAreaAnalisis>(
                                ReporteAreaAnalisis
                                    .Empleados,
                                "Empleados"));
                }

                if (_puedeFinanciero)
                {
                    areas.Add(
                        new OpcionAnalisis
                            <ReporteAreaAnalisis>(
                                ReporteAreaAnalisis
                                    .Gastos,
                                "Gastos"));

                    if (!_puedeResumen)
                    {
                        areas.Add(
                            new OpcionAnalisis
                                <ReporteAreaAnalisis>(
                                    ReporteAreaAnalisis
                                        .Empleados,
                                    "Empleados"));
                    }
                }

                ConfigurarComboAnalisis(
                    _cmbAreaAnalisis,
                    areas);

                ConfigurarComboAnalisis(
                    _cmbTopAnalisis,
                    new[]
                    {
                        new OpcionTop(
                            5,
                            "Top 5"),
                        new OpcionTop(
                            10,
                            "Top 10"),
                        new OpcionTop(
                            15,
                            "Top 15"),
                        new OpcionTop(
                            0,
                            "Todos")
                    });

                _cmbTopAnalisis.SelectedIndex =
                    1;

                RestablecerFechas(
                    _dtpAnalisisDesde,
                    _dtpAnalisisHasta);

                CargarIndicadoresAnalisis();
            }
            finally
            {
                _cargandoConfiguracionAnalisis =
                    false;
            }

            ActualizarAyudaAnalisis();
        }

        private void CargarIndicadoresAnalisis()
        {
            ReporteAreaAnalisis? area =
                ObtenerValorAnalisis
                    <ReporteAreaAnalisis>(
                        _cmbAreaAnalisis);

            var opciones =
                new List
                    <OpcionAnalisis
                        <ReporteIndicadorAnalisis>>();

            if (!area.HasValue)
            {
                ConfigurarComboAnalisis(
                    _cmbIndicadorAnalisis,
                    opciones);

                CargarAgrupacionesAnalisis();
                return;
            }

            switch (area.Value)
            {
                case ReporteAreaAnalisis.Gastos:
                    opciones.Add(
                        new OpcionAnalisis
                            <ReporteIndicadorAnalisis>(
                                ReporteIndicadorAnalisis
                                    .Importe,
                                "Total gastado"));
                    break;

                case ReporteAreaAnalisis.Clientes:
                    opciones.Add(
                        new OpcionAnalisis
                            <ReporteIndicadorAnalisis>(
                                ReporteIndicadorAnalisis
                                    .CantidadVisitas,
                                "Frecuencia de visitas"));
                    break;

                case ReporteAreaAnalisis.Viajes:
                    opciones.Add(
                        new OpcionAnalisis
                            <ReporteIndicadorAnalisis>(
                                ReporteIndicadorAnalisis
                                    .CantidadViajes,
                                "Cantidad de viajes"));
                    break;

                case ReporteAreaAnalisis.Empleados:
                    if (_puedeResumen)
                    {
                        opciones.Add(
                            new OpcionAnalisis
                                <ReporteIndicadorAnalisis>(
                                    ReporteIndicadorAnalisis
                                        .CantidadViajes,
                                    "Cantidad de viajes"));

                        opciones.Add(
                            new OpcionAnalisis
                                <ReporteIndicadorAnalisis>(
                                    ReporteIndicadorAnalisis
                                        .CantidadVisitas,
                                    "Cantidad de visitas"));
                    }

                    if (_puedeFinanciero)
                    {
                        opciones.Add(
                            new OpcionAnalisis
                                <ReporteIndicadorAnalisis>(
                                    ReporteIndicadorAnalisis
                                        .Importe,
                                    "Total gastado"));
                    }
                    break;
            }

            ConfigurarComboAnalisis(
                _cmbIndicadorAnalisis,
                opciones);

            CargarAgrupacionesAnalisis();
        }

        private void CargarAgrupacionesAnalisis()
        {
            ReporteAreaAnalisis? area =
                ObtenerValorAnalisis
                    <ReporteAreaAnalisis>(
                        _cmbAreaAnalisis);

            var opciones =
                new List
                    <OpcionAnalisis
                        <ReporteAgrupacionAnalisis>>();

            if (area.HasValue)
            {
                switch (area.Value)
                {
                    case ReporteAreaAnalisis.Gastos:
                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Categoria,
                                "Categoría"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Empleado,
                                "Empleado"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Viaje,
                                "Viaje"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Mes,
                                "Mes"));
                        break;

                    case ReporteAreaAnalisis.Clientes:
                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Cliente,
                                "Cliente"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Localidad,
                                "Localidad"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Provincia,
                                "Provincia"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Mes,
                                "Mes"));
                        break;

                    case ReporteAreaAnalisis.Viajes:
                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Empleado,
                                "Empleado"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .TipoViaje,
                                "Tipo de viaje"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .EstadoViaje,
                                "Estado"));

                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Mes,
                                "Mes"));
                        break;

                    case ReporteAreaAnalisis.Empleados:
                        opciones.Add(
                            CrearAgrupacion(
                                ReporteAgrupacionAnalisis
                                    .Empleado,
                                "Empleado"));
                        break;
                }
            }

            ConfigurarComboAnalisis(
                _cmbAgrupacionAnalisis,
                opciones);

            ActualizarAyudaAnalisis();
        }

        private static OpcionAnalisis
            <ReporteAgrupacionAnalisis>
            CrearAgrupacion(
                ReporteAgrupacionAnalisis valor,
                string texto)
        {
            return new OpcionAnalisis
                <ReporteAgrupacionAnalisis>(
                    valor,
                    texto);
        }

        private void CmbAreaAnalisis_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (_cargandoConfiguracionAnalisis)
            {
                return;
            }

            _cargandoConfiguracionAnalisis =
                true;

            try
            {
                CargarIndicadoresAnalisis();
            }
            finally
            {
                _cargandoConfiguracionAnalisis =
                    false;
            }

            ActualizarAyudaAnalisis();
        }

        private void CmbIndicadorAnalisis_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (_cargandoConfiguracionAnalisis)
            {
                return;
            }

            _cargandoConfiguracionAnalisis =
                true;

            try
            {
                CargarAgrupacionesAnalisis();
            }
            finally
            {
                _cargandoConfiguracionAnalisis =
                    false;
            }

            ActualizarAyudaAnalisis();
        }

        private void ActualizarAyudaAnalisis()
        {
            if (_lblAyudaAnalisis == null)
            {
                return;
            }

            string area =
                ObtenerTextoSeleccionado(
                    _cmbAreaAnalisis);

            string indicador =
                ObtenerTextoSeleccionado(
                    _cmbIndicadorAnalisis);

            string agrupacion =
                ObtenerTextoSeleccionado(
                    _cmbAgrupacionAnalisis);

            _lblAyudaAnalisis.Text =
                string.IsNullOrWhiteSpace(
                    area)
                    ? "Seleccione un área de análisis."
                    : "Se analizará " +
                        indicador.ToLowerInvariant() +
                        " del área " +
                        area.ToLowerInvariant() +
                        ", agrupado por " +
                        agrupacion.ToLowerInvariant() +
                        ".";
        }

        private void BtnBuscarViajesResumen_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                CambiarEstado(
                    true);

                BuscarViajesResumenInterno();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void BtnLimpiarViajesResumen_Click(
            object sender,
            EventArgs e)
        {
            RestablecerFechas(
                _dtpResumenFechaDesde,
                _dtpResumenFechaHasta);

            _txtResumenBuscar.Clear();

            SeleccionarPrimero(
                _cmbResumenEstado);

            BtnBuscarViajesResumen_Click(
                sender,
                e);
        }

        private void GridViajesResumen_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ViajeListadoDto viaje =
                ObtenerViajeResumenSeleccionado();

            if (viaje == null)
            {
                return;
            }

            _lblViajeSeleccionado.Text =
                "Seleccionado: #"
                +
                viaje.IdViaje.ToString(
                    CulturaArgentina)
                +
                " — "
                +
                viaje.Descripcion;
        }

        private void GridViajesResumen_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            ConsultarResumen();
        }

        private void BtnConsultarResumen_Click(
            object sender,
            EventArgs e)
        {
            ConsultarResumen();
        }

        private ViajeListadoDto
            ObtenerViajeResumenSeleccionado()
        {
            if (_gridViajesResumen == null ||
                _gridViajesResumen.SelectedRows.Count != 1)
            {
                return null;
            }

            return _gridViajesResumen
                .SelectedRows[0]
                .DataBoundItem
                as ViajeListadoDto;
        }

        private void ConsultarResumen()
        {
            ViajeListadoDto viaje =
                ObtenerViajeResumenSeleccionado();

            if (viaje == null)
            {
                MessageBox.Show(
                    "Seleccione un viaje en la grilla.",
                    "Reportes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                CambiarEstado(
                    true);

                ReporteViajeResumenDto resumen =
                    _reporteService
                        .ObtenerResumenViaje(
                            viaje.IdViaje);

                MostrarResumen(
                    resumen);
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void MostrarResumen(
            ReporteViajeResumenDto resumen)
        {
            _lblResumenId.Text =
                resumen.IdViaje.ToString(
                    CulturaArgentina);

            _lblResumenDescripcion.Text =
                resumen.Descripcion;

            _lblResumenTipo.Text =
                FormatearTextoEnum(
                    resumen.TipoViaje.ToString());

            _lblResumenEstado.Text =
                FormatearTextoEnum(
                    resumen.Estado.ToString());

            _lblResumenFechas.Text =
                resumen.FechaInicio
                    .ToString(
                        "dd/MM/yyyy",
                        CulturaArgentina)
                +
                " al "
                +
                resumen.FechaFin
                    .ToString(
                        "dd/MM/yyyy",
                        CulturaArgentina);

            _lblResumenEnvio.Text =
                FormatearResponsableFecha(
                    resumen.ResponsableEnvio,
                    resumen.FechaEnvio);

            _lblResumenAprobacion.Text =
                FormatearResponsableFecha(
                    resumen.Aprobador,
                    resumen.FechaAprobacion);

            _lblResumenCancelacion.Text =
                FormatearResponsableFecha(
                    resumen.Cancelador,
                    resumen.FechaCancelacion);

            _lblResumenMotivoCancelacion.Text =
                TextoOpcional(
                    resumen.MotivoCancelacion);

            _lblCantidadParticipantes.Text =
                resumen.CantidadParticipantes
                    .ToString(
                        CulturaArgentina);

            _lblCantidadVisitas.Text =
                resumen.CantidadVisitas
                    .ToString(
                        CulturaArgentina);

            _lblCantidadClientes.Text =
                resumen.CantidadClientesDistintos
                    .ToString(
                        CulturaArgentina);

            _lblAnticipoResumen.Text =
                FormatearMoneda(
                    resumen.MontoAnticipado);

            _lblTotalGastosResumen.Text =
                FormatearMoneda(
                    resumen.TotalGastos);

            _lblSaldoResumen.Text =
                resumen.TipoSaldo
                +
                ": "
                +
                FormatearMoneda(
                    resumen.ImporteDiferencia);

            _gridParticipantes.DataSource =
                CrearOrigenOrdenable(
                    resumen.Participantes);

            _gridVisitas.DataSource =
                CrearOrigenOrdenable(
                    resumen.Visitas);

            _gridClientes.DataSource =
                CrearOrigenOrdenable(
                    resumen.Clientes);

            _gridViaticosResumen.DataSource =
                CrearOrigenOrdenable(
                    resumen.Viaticos);
        }

        private void BtnConsultarViaticos_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                CambiarEstado(
                    true);

                ConsultarViaticosInterno();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void ConsultarViaticosInterno()
        {
            ReporteViaticoFiltro filtro =
                new ReporteViaticoFiltro(
                    _dtpFechaDesde.Checked
                        ? _dtpFechaDesde.Value
                        : (DateTime?)null,
                    _dtpFechaHasta.Checked
                        ? _dtpFechaHasta.Value
                        : (DateTime?)null,
                    ObtenerId(
                        _cmbViajeViatico),
                    ObtenerId(
                        _cmbPersonaPagadora),
                    ObtenerEnum
                        <CategoriaGasto>(
                            _cmbCategoria),
                    ObtenerEnum
                        <MetodoPago>(
                            _cmbMetodoPago),
                    ObtenerEnum
                        <EstadoViatico>(
                            _cmbEstadoViatico),
                    ObtenerEnum
                        <EstadoViaje>(
                            _cmbEstadoViaje));

            ReporteViaticoResultadoDto resultado =
                _reporteService
                    .ConsultarViaticos(
                        filtro);

            MostrarReporteViaticos(
                resultado);
        }

        private void MostrarReporteViaticos(
            ReporteViaticoResultadoDto resultado)
        {
            _lblCantidadReporte.Text =
                resultado.Cantidad
                    .ToString(
                        CulturaArgentina);

            _lblTotalRegistradoReporte.Text =
                FormatearMoneda(
                    resultado.TotalRegistrado);

            _lblTotalVigenteReporte.Text =
                FormatearMoneda(
                    resultado.TotalVigente);

            _lblTotalExcluidoReporte.Text =
                FormatearMoneda(
                    resultado.TotalExcluido);

            _lblPromedioReporte.Text =
                FormatearMoneda(
                    resultado.Promedio);

            _lblCategoriaMayorReporte.Text =
                resultado.CategoriaMayorGasto.HasValue
                    ? FormatearTextoEnum(
                        resultado
                            .CategoriaMayorGasto
                            .Value
                            .ToString())
                        +
                        " — "
                        +
                        FormatearMoneda(
                            resultado
                                .TotalCategoriaMayorGasto)
                    : "Sin datos";

            _gridViaticosReporte.DataSource =
                CrearOrigenOrdenable(
                    resultado.Filas);
        }

        private void BtnLimpiarViaticos_Click(
            object sender,
            EventArgs e)
        {
            RestablecerFechas(
                _dtpFechaDesde,
                _dtpFechaHasta);

            SeleccionarPrimero(
                _cmbViajeViatico);

            SeleccionarPrimero(
                _cmbPersonaPagadora);

            SeleccionarPrimero(
                _cmbCategoria);

            SeleccionarPrimero(
                _cmbMetodoPago);

            SeleccionarPrimero(
                _cmbEstadoViatico);

            SeleccionarPrimero(
                _cmbEstadoViaje);

            BtnConsultarViaticos_Click(
                sender,
                e);
        }

        private void BtnGenerarAnalisis_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                CambiarEstado(
                    true);

                GenerarAnalisisInterno();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void GenerarAnalisisInterno()
        {
            ReporteAreaAnalisis? area =
                ObtenerValorAnalisis
                    <ReporteAreaAnalisis>(
                        _cmbAreaAnalisis);

            ReporteIndicadorAnalisis? indicador =
                ObtenerValorAnalisis
                    <ReporteIndicadorAnalisis>(
                        _cmbIndicadorAnalisis);

            ReporteAgrupacionAnalisis? agrupacion =
                ObtenerValorAnalisis
                    <ReporteAgrupacionAnalisis>(
                        _cmbAgrupacionAnalisis);

            OpcionTop opcionTop =
                _cmbTopAnalisis.SelectedItem
                    as OpcionTop;

            if (!area.HasValue ||
                !indicador.HasValue ||
                !agrupacion.HasValue ||
                opcionTop == null)
            {
                throw new ReglaNegocioException(
                    "Complete la configuración del análisis.");
            }

            var filtro =
                new ReporteAnalisisFiltro(
                    _dtpAnalisisDesde.Checked
                        ? _dtpAnalisisDesde.Value
                        : (DateTime?)null,
                    _dtpAnalisisHasta.Checked
                        ? _dtpAnalisisHasta.Value
                        : (DateTime?)null,
                    area.Value,
                    indicador.Value,
                    agrupacion.Value,
                    opcionTop.Valor);

            ReporteAnalisisResultadoDto resultado =
                _reporteService
                    .ConsultarAnalisis(
                        filtro);

            MostrarAnalisis(
                resultado,
                indicador.Value);
        }

        private void MostrarAnalisis(
            ReporteAnalisisResultadoDto resultado,
            ReporteIndicadorAnalisis indicador)
        {
            bool esMoneda =
                indicador ==
                    ReporteIndicadorAnalisis.Importe;

            _lblTotalAnalisis.Text =
                FormatearMetrica(
                    resultado.Total,
                    esMoneda);

            _lblMaximoAnalisis.Text =
                FormatearMetrica(
                    resultado.Maximo,
                    esMoneda);

            _lblPromedioAnalisis.Text =
                FormatearMetrica(
                    resultado.Promedio,
                    esMoneda);

            _lblDestacadoAnalisis.Text =
                string.IsNullOrWhiteSpace(
                    resultado.ElementoDestacado)
                    ? "Sin datos"
                    : resultado.ElementoDestacado
                        +
                        " — "
                        +
                        FormatearMetrica(
                            resultado.ValorDestacado,
                            esMoneda);

            List<ReporteAnalisisItemDto>
                items =
                    resultado.Items
                        .ToList();

            if (items.Count == 0)
            {
                _lblEstadoAnalisis.Text =
                    "No existen datos para la configuración y el período seleccionados.";

                _chartAnalisis.Series.Clear();
                _chartAnalisis.Titles.Clear();

                _gridRankingAnalisis.DataSource =
                    CrearOrigenOrdenable(
                        Enumerable.Empty
                            <AnalisisRankingFila>());

                return;
            }

            _lblEstadoAnalisis.Text =
                resultado.Titulo
                +
                ". Visualización dinámica según los filtros seleccionados.";

            ActualizarGraficoAnalisis(
                resultado,
                esMoneda);

            List<AnalisisRankingFila>
                ranking =
                    items
                        .Select(
                            (item, indice) =>
                                new AnalisisRankingFila(
                                    indice + 1,
                                    item.Etiqueta,
                                    item.Valor,
                                    item.Participacion,
                                    FormatearMetrica(
                                        item.Valor,
                                        esMoneda)))
                        .ToList();

            _gridRankingAnalisis.DataSource =
                CrearOrigenOrdenable(
                    ranking);
        }

        private void ActualizarGraficoAnalisis(
            ReporteAnalisisResultadoDto resultado,
            bool esMoneda)
        {
            _chartAnalisis.Series.Clear();
            _chartAnalisis.Titles.Clear();

            ChartArea area =
                _chartAnalisis
                    .ChartAreas["Analisis"];

            area.AxisX.Title =
                string.Empty;

            area.AxisY.Title =
                esMoneda
                    ? "Importe en pesos"
                    : resultado.Unidad;

            area.AxisY.LabelStyle.Format =
                esMoneda
                    ? "$ #,##0"
                    : "N0";

            var serie =
                new Series(
                    "Resultado")
                {
                    ChartType =
                        SeriesChartType.Bar,
                    IsValueShownAsLabel =
                        true,
                    LabelForeColor =
                        Color.FromArgb(
                            30,
                            41,
                            59),
                    Font =
                        new Font(
                            "Segoe UI",
                            8F),
                    Color =
                        ColorPrimario,
                    YValueType =
                        ChartValueType.Double
                };

            foreach (
                ReporteAnalisisItemDto item
                in resultado.Items.Reverse())
            {
                int indice =
                    serie.Points.AddXY(
                        item.Etiqueta,
                        Convert.ToDouble(
                            item.Valor));

                DataPoint punto =
                    serie.Points[indice];

                punto.Label =
                    FormatearMetrica(
                        item.Valor,
                        esMoneda);
            }

            _chartAnalisis.Series.Add(
                serie);

            _chartAnalisis.Titles.Add(
                new Title(
                    resultado.Titulo,
                    Docking.Top,
                    new Font(
                        "Segoe UI",
                        11F,
                        FontStyle.Bold),
                    Color.FromArgb(
                        30,
                        41,
                        59)));
        }

        private void BtnLimpiarAnalisis_Click(
            object sender,
            EventArgs e)
        {
            RestablecerFechas(
                _dtpAnalisisDesde,
                _dtpAnalisisHasta);

            if (_cmbAreaAnalisis.Items.Count > 0)
            {
                _cmbAreaAnalisis.SelectedIndex =
                    0;
            }

            if (_cmbTopAnalisis.Items.Count > 1)
            {
                _cmbTopAnalisis.SelectedIndex =
                    1;
            }

            BtnGenerarAnalisis_Click(
                sender,
                e);
        }

        private void CambiarEstado(
            bool ocupado)
        {
            UseWaitCursor =
                ocupado;

            if (_tabPrincipal != null)
            {
                _tabPrincipal.Enabled =
                    !ocupado;
            }

            if (_btnCerrar != null)
            {
                _btnCerrar.Enabled =
                    !ocupado;
            }

            Cursor =
                ocupado
                    ? Cursors.WaitCursor
                    : Cursors.Default;
        }

        private static TabPage CrearTabBase(
            string titulo)
        {
            return new TabPage(
                titulo)
            {
                BackColor =
                    ColorFondo,
                Padding =
                    new Padding(0)
            };
        }

        private static GroupBox CrearGrupo(
            string titulo)
        {
            return new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = titulo,
                BackColor =
                    ColorTarjeta,
                ForeColor =
                    Color.FromArgb(
                        30,
                        41,
                        59),
                Padding =
                    new Padding(8)
            };
        }

        private static Panel CrearFilaIndicadorResumen(
            string titulo,
            out Label valor)
        {
            var panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(3),
                    Padding =
                        new Padding(
                            8,
                            2,
                            8,
                            2),
                    BackColor =
                        ColorTarjeta,
                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            var contenido =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor =
                        ColorTarjeta
                };

            contenido.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    58F));

            contenido.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    42F));

            contenido.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            contenido.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F),
                    ForeColor =
                        ColorTextoSecundario,
                    Text = titulo,
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                },
                0,
                0);

            valor =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            10.5F,
                            FontStyle.Bold),
                    ForeColor =
                        Color.FromArgb(
                            30,
                            41,
                            59),
                    Text =
                        "—",
                    TextAlign =
                        ContentAlignment.MiddleRight,
                    AutoEllipsis = true
                };

            contenido.Controls.Add(
                valor,
                1,
                0);

            panel.Controls.Add(
                contenido);

            return panel;
        }

        private static Control CrearCampoFiltroCompacto(
            string titulo,
            Control control)
        {
            var contenedor =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Margin =
                        new Padding(4),
                    BackColor =
                        ColorTarjeta
                };

            contenedor.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            contenedor.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    20F));

            contenedor.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            contenedor.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold),
                    ForeColor =
                        Color.FromArgb(
                            30,
                            41,
                            59),
                    Text = titulo,
                    TextAlign =
                        ContentAlignment.BottomLeft,
                    AutoEllipsis = true
                },
                0,
                0);

            control.Dock =
                DockStyle.Fill;

            control.Margin =
                new Padding(
                    0,
                    2,
                    0,
                    1);

            contenedor.Controls.Add(
                control,
                0,
                1);

            return contenedor;
        }

        private static Panel CrearTarjetaIndicador(
            string titulo,
            out Label valor)
        {
            var panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(4),
                    Padding =
                        new Padding(
                            8,
                            3,
                            8,
                            3),
                    BackColor =
                        ColorTarjeta,
                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            var contenido =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    BackColor =
                        ColorTarjeta
                };

            contenido.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            contenido.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    19F));

            contenido.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            var etiqueta =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F),
                    ForeColor =
                        ColorTextoSecundario,
                    Text = titulo,
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };

            valor =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            12.5F,
                            FontStyle.Bold),
                    ForeColor =
                        Color.FromArgb(
                            30,
                            41,
                            59),
                    Text =
                        "—",
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };

            contenido.Controls.Add(
                etiqueta,
                0,
                0);

            contenido.Controls.Add(
                valor,
                0,
                1);

            panel.Controls.Add(
                contenido);

            return panel;
        }

        private static Label CrearEtiquetaCampo(
            string texto)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                ForeColor =
                    Color.FromArgb(
                        30,
                        41,
                        59),
                Text = texto,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private static Label CrearEtiquetaSimple(
            string texto)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                ForeColor =
                    ColorTextoSecundario,
                Text = texto,
                TextAlign =
                    ContentAlignment.BottomLeft,
                AutoEllipsis = true
            };
        }

        private static Label CrearEtiquetaValor()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                ForeColor =
                    Color.FromArgb(
                        30,
                        41,
                        59),
                Text =
                    "Sin consultar",
                TextAlign =
                    ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
        }

        private static ComboBox CrearCombo()
        {
            return new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle =
                    ComboBoxStyle.DropDownList,
                IntegralHeight = false,
                DropDownHeight = 260,
                Margin =
                    new Padding(
                        4,
                        3,
                        6,
                        3)
            };
        }

        private static DateTimePicker
            CrearFechaFiltro()
        {
            return new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format =
                    DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Margin =
                    new Padding(
                        4,
                        3,
                        6,
                        3)
            };
        }

        private static Button CrearBotonPrimario(
            string texto,
            int ancho)
        {
            var boton =
                new Button
                {
                    Text = texto,
                    Width = ancho,
                    Height = 31,
                    AutoSize = false,
                    FlatStyle =
                        FlatStyle.Flat,
                    BackColor =
                        ColorPrimario,
                    ForeColor =
                        Color.White,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Margin =
                        new Padding(
                            4,
                            2,
                            7,
                            2),
                    UseVisualStyleBackColor =
                        false
                };

            boton.FlatAppearance.BorderSize =
                0;

            return boton;
        }

        private static Button CrearBotonSecundario(
            string texto,
            int ancho)
        {
            var boton =
                new Button
                {
                    Text = texto,
                    Width = ancho,
                    Height = 31,
                    AutoSize = false,
                    FlatStyle =
                        FlatStyle.Flat,
                    BackColor =
                        Color.White,
                    ForeColor =
                        Color.FromArgb(
                            51,
                            65,
                            85),
                    Font =
                        new Font(
                            "Segoe UI",
                            9F),
                    Margin =
                        new Padding(
                            4,
                            2,
                            7,
                            2),
                    UseVisualStyleBackColor =
                        false
                };

            boton.FlatAppearance.BorderColor =
                ColorBorde;

            boton.FlatAppearance.BorderSize =
                1;

            return boton;
        }

        private static DataGridView CrearGridBase()
        {
            var grilla =
                new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoGenerateColumns = false,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    MultiSelect = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    SelectionMode =
                        DataGridViewSelectionMode
                            .FullRowSelect,
                    BackgroundColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    AutoSizeRowsMode =
                        DataGridViewAutoSizeRowsMode.None,
                    RowTemplate =
                    {
                        Height = 28
                    },
                    EnableHeadersVisualStyles =
                        false
                };

            grilla.ColumnHeadersDefaultCellStyle
                .BackColor =
                    Color.FromArgb(
                        241,
                        245,
                        249);

            grilla.ColumnHeadersDefaultCellStyle
                .ForeColor =
                    Color.FromArgb(
                        30,
                        41,
                        59);

            grilla.ColumnHeadersDefaultCellStyle
                .Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold);

            grilla.ColumnHeadersHeight =
                32;

            grilla.AlternatingRowsDefaultCellStyle
                .BackColor =
                    Color.FromArgb(
                        248,
                        250,
                        252);

            grilla.CellFormatting +=
                Grid_CellFormatting;

            grilla.DataBindingComplete +=
                Grid_DataBindingComplete;

            grilla.Sorted +=
                Grid_Sorted;

            return grilla;
        }

        private static void Grid_DataBindingComplete(
            object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            DataGridView grilla =
                sender as DataGridView;

            LimpiarSeleccionAutomatica(
                grilla);

            if (grilla == null ||
                grilla.IsDisposed ||
                !grilla.IsHandleCreated)
            {
                return;
            }

            grilla.BeginInvoke(
                (MethodInvoker)(
                    () =>
                    {
                        if (!grilla.IsDisposed)
                        {
                            LimpiarSeleccionAutomatica(
                                grilla);
                        }
                    }));
        }

        private static void Grid_Sorted(
            object sender,
            EventArgs e)
        {
            LimpiarSeleccionAutomatica(
                sender as DataGridView);
        }

        private static void LimpiarSeleccionAutomatica(
            DataGridView grilla)
        {
            if (grilla == null)
            {
                return;
            }

            grilla.ClearSelection();
            grilla.CurrentCell = null;
        }

        private static void Grid_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null)
            {
                return;
            }

            Type tipo =
                e.Value.GetType();

            if (tipo.IsEnum)
            {
                e.Value =
                    FormatearTextoEnum(
                        e.Value.ToString());

                e.FormattingApplied =
                    true;
            }
        }

        private static void AgregarColumnaTexto(
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
                    SortMode =
                        DataGridViewColumnSortMode
                            .Automatic,
                    Width = ancho
                });
        }

        private static void AgregarColumnaFecha(
            DataGridView grilla,
            string propiedad,
            string titulo,
            int ancho)
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
                    SortMode =
                        DataGridViewColumnSortMode
                            .Automatic,
                    Width = ancho
                };

            columna.DefaultCellStyle.Format =
                "dd/MM/yyyy";

            columna.DefaultCellStyle
                .FormatProvider =
                    CulturaArgentina;

            grilla.Columns.Add(
                columna);
        }

        private static void AgregarColumnaMoneda(
            DataGridView grilla,
            string propiedad,
            string titulo,
            int ancho)
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
                    SortMode =
                        DataGridViewColumnSortMode
                            .Automatic,
                    Width = ancho,
                    DefaultCellStyle =
                    {
                        Alignment =
                            DataGridViewContentAlignment
                                .MiddleRight,
                        Format =
                            "C2",
                        FormatProvider =
                            CulturaArgentina
                    }
                };

            grilla.Columns.Add(
                columna);
        }

        private static void AgregarColumnaCheck(
            DataGridView grilla,
            string propiedad,
            string titulo,
            int ancho)
        {
            grilla.Columns.Add(
                new DataGridViewCheckBoxColumn
                {
                    DataPropertyName =
                        propiedad,
                    HeaderText =
                        titulo,
                    Name =
                        propiedad,
                    SortMode =
                        DataGridViewColumnSortMode
                            .Automatic,
                    Width = ancho
                });
        }

        private static TabPage CrearTabConControl(
            string titulo,
            Control control)
        {
            var tab =
                new TabPage(
                    titulo)
                {
                    BackColor =
                        Color.White,
                    Padding =
                        new Padding(3)
                };

            tab.Controls.Add(
                control);

            return tab;
        }

        private static void AgregarDato(
            TableLayoutPanel tabla,
            int fila,
            int columnaEtiqueta,
            string texto,
            Label valor)
        {
            tabla.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold),
                    ForeColor =
                        ColorTextoSecundario,
                    Text = texto,
                    TextAlign =
                        ContentAlignment.MiddleLeft
                },
                columnaEtiqueta,
                fila);

            tabla.Controls.Add(
                valor,
                columnaEtiqueta + 1,
                fila);
        }

        private static void AgregarControlFiltro(
            TableLayoutPanel tabla,
            int fila,
            int columnaEtiqueta,
            string texto,
            Control control)
        {
            tabla.Controls.Add(
                CrearEtiquetaCampo(
                    texto),
                columnaEtiqueta,
                fila);

            tabla.Controls.Add(
                control,
                columnaEtiqueta + 1,
                fila);
        }

        private static object CrearOrigenOrdenable<T>(
            IEnumerable<T> elementos)
        {
            return new SortableBindingList<T>(
                (
                    elementos
                    ?? Enumerable.Empty<T>()
                ).ToList());
        }

        private static void ConfigurarCombo(
            ComboBox combo,
            IEnumerable<OpcionFiltro>
                opciones)
        {
            combo.DataSource =
                opciones.ToList();

            combo.DisplayMember =
                "Texto";

            combo.ValueMember =
                "Valor";

            SeleccionarPrimero(
                combo);
        }

        private static void
            ConfigurarComboAnalisis<T>(
                ComboBox combo,
                IEnumerable<T> opciones)
        {
            combo.DataSource =
                opciones.ToList();

            combo.DisplayMember =
                "Texto";

            combo.ValueMember =
                "Valor";

            SeleccionarPrimero(
                combo);
        }

        private static void CargarEnum<TEnum>(
            ComboBox combo,
            string textoTodos)
            where TEnum : struct
        {
            var opciones =
                new List<OpcionFiltro>
                {
                    new OpcionFiltro(
                        null,
                        textoTodos)
                };

            foreach (
                TEnum valor
                in Enum.GetValues(
                    typeof(TEnum)))
            {
                opciones.Add(
                    new OpcionFiltro(
                        valor,
                        FormatearTextoEnum(
                            valor.ToString())));
            }

            ConfigurarCombo(
                combo,
                opciones);
        }

        private static int? ObtenerId(
            ComboBox combo)
        {
            OpcionFiltro opcion =
                combo.SelectedItem
                    as OpcionFiltro;

            if (opcion == null ||
                opcion.Valor == null)
            {
                return null;
            }

            return Convert.ToInt32(
                opcion.Valor,
                CulturaArgentina);
        }

        private static TEnum? ObtenerEnum<TEnum>(
            ComboBox combo)
            where TEnum : struct
        {
            OpcionFiltro opcion =
                combo.SelectedItem
                    as OpcionFiltro;

            if (opcion == null ||
                opcion.Valor == null)
            {
                return null;
            }

            return (TEnum)
                opcion.Valor;
        }

        private static TEnum?
            ObtenerValorAnalisis<TEnum>(
                ComboBox combo)
            where TEnum : struct
        {
            OpcionAnalisis<TEnum>
                opcion =
                    combo.SelectedItem
                        as OpcionAnalisis<TEnum>;

            return opcion == null
                ? (TEnum?)null
                : opcion.Valor;
        }

        private static string
            ObtenerTextoSeleccionado(
                ComboBox combo)
        {
            if (combo == null ||
                combo.SelectedItem == null)
            {
                return string.Empty;
            }

            return combo.Text
                ?? string.Empty;
        }

        private static void RestablecerFechas(
            DateTimePicker desde,
            DateTimePicker hasta)
        {
            desde.Value =
                DateTime.Today
                    .AddMonths(-1);

            hasta.Value =
                DateTime.Today;

            desde.Checked =
                false;

            hasta.Checked =
                false;
        }

        private static void SeleccionarPrimero(
            ComboBox combo)
        {
            if (combo != null &&
                combo.Items.Count > 0)
            {
                combo.SelectedIndex =
                    0;
            }
        }

        private static string
            FormatearMoneda(
                decimal importe)
        {
            return importe.ToString(
                "C2",
                CulturaArgentina);
        }

        private static string
            FormatearMetrica(
                decimal valor,
                bool esMoneda)
        {
            return esMoneda
                ? FormatearMoneda(
                    valor)
                : valor.ToString(
                    valor ==
                        decimal.Truncate(
                            valor)
                        ? "N0"
                        : "N2",
                    CulturaArgentina);
        }

        private static string
            FormatearResponsableFecha(
                string responsable,
                DateTime? fecha)
        {
            if (string.IsNullOrWhiteSpace(
                    responsable)
                &&
                !fecha.HasValue)
            {
                return "Sin registrar";
            }

            string texto =
                string.IsNullOrWhiteSpace(
                    responsable)
                    ? "Sin responsable"
                    : responsable.Trim();

            if (fecha.HasValue)
            {
                texto +=
                    " — "
                    +
                    fecha.Value.ToString(
                        "dd/MM/yyyy HH:mm",
                        CulturaArgentina);
            }

            return texto;
        }

        private static string TextoOpcional(
            string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                    ? "Sin registrar"
                    : valor.Trim();
        }

        private static string FormatearTextoEnum(
            string valor)
        {
            switch (valor)
            {
                case "EnRendicion":
                    return "En rendición";

                case "EventoFeria":
                    return "Evento o feria";

                case "PagoPersonal":
                    return "Pago personal";

                case "TarjetaCorporativa":
                    return "Tarjeta corporativa";

                case "EfectivoEmpresa":
                    return "Efectivo de la empresa";

                default:
                    return valor;
            }
        }

        private static void MostrarErrorControlado(
            Exception exception)
        {
            if (exception is
                    ReglaNegocioException
                ||
                exception is
                    AccesoDenegadoException
                ||
                exception is
                    ArgumentOutOfRangeException
                ||
                exception is
                    ArgumentException)
            {
                MessageBox.Show(
                    exception.Message,
                    "Reportes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is
                PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible consultar los reportes. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado al consultar reportes.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private sealed class OpcionFiltro
        {
            public OpcionFiltro(
                object valor,
                string texto)
            {
                Valor = valor;
                Texto =
                    texto ?? string.Empty;
            }

            public object Valor
            {
                get;
                private set;
            }

            public string Texto
            {
                get;
                private set;
            }
        }

        private sealed class
            OpcionAnalisis<TEnum>
            where TEnum : struct
        {
            public OpcionAnalisis(
                TEnum valor,
                string texto)
            {
                Valor = valor;
                Texto =
                    texto ?? string.Empty;
            }

            public TEnum Valor
            {
                get;
                private set;
            }

            public string Texto
            {
                get;
                private set;
            }
        }

        private sealed class OpcionTop
        {
            public OpcionTop(
                int valor,
                string texto)
            {
                Valor = valor;
                Texto =
                    texto ?? string.Empty;
            }

            public int Valor
            {
                get;
                private set;
            }

            public string Texto
            {
                get;
                private set;
            }
        }

        private sealed class
            AnalisisRankingFila
        {
            public AnalisisRankingFila(
                int posicion,
                string entidad,
                decimal valor,
                decimal participacion,
                string valorTexto)
            {
                Posicion = posicion;
                Entidad =
                    entidad ?? string.Empty;
                Valor = valor;
                Participacion =
                    participacion;
                ValorTexto =
                    valorTexto ?? string.Empty;
                ParticipacionTexto =
                    participacion.ToString(
                        "N1",
                        CulturaArgentina)
                    +
                    " %";
            }

            public int Posicion
            {
                get;
                private set;
            }

            public string Entidad
            {
                get;
                private set;
            }

            public decimal Valor
            {
                get;
                private set;
            }

            public decimal Participacion
            {
                get;
                private set;
            }

            public string ValorTexto
            {
                get;
                private set;
            }

            public string ParticipacionTexto
            {
                get;
                private set;
            }
        }
    }
}
