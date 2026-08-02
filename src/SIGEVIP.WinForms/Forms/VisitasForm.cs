using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.WinForms.Controls;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class VisitasForm : Form
    {
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
            ColorTexto =
                Color.FromArgb(
                    30,
                    41,
                    59);

        private static readonly Color
            ColorTextoSecundario =
                Color.FromArgb(
                    85,
                    94,
                    108);

        private readonly VisitaService
            _visitaService;

        private readonly ViajeService
            _viajeService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private bool
            _puedeRegistrar;

        private TextBox
            _txtBuscarViaje;

        private DateTimePicker
            _dtpViajeDesde;

        private DateTimePicker
            _dtpViajeHasta;

        private ComboBox
            _cmbEstadoViaje;

        private Button
            _btnBuscarViajes;

        private Button
            _btnLimpiarViajes;

        private Button
            _btnConsultarSeleccion;

        private DataGridView
            _grillaViajes;

        private Label
            _lblViajeSeleccionado;

        private DataGridView
            _grillaVisitas;

        private Button
            _btnNuevaVisita;

        private Button
            _btnModificarVisita;

        private Button
            _btnCerrar;

        private Label
            _lblCantidad;

        private ViajeListadoDto
            _viajeSeleccionado;

        public VisitasForm(
            VisitaService visitaService,
            ViajeService viajeService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _visitaService =
                visitaService
                ?? throw new ArgumentNullException(
                    nameof(visitaService));

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
                VisitasForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Visitas";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(
                    1100,
                    700);

            Size =
                new Size(
                    1320,
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
                    RowCount = 6,
                    Padding =
                        new Padding(14),
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
                    52F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    252F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    50F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    25F));

            raiz.Controls.Add(
                CrearEncabezado(),
                0,
                0);

            raiz.Controls.Add(
                CrearSelectorViajes(),
                0,
                1);

            _lblViajeSeleccionado =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(
                            0,
                            5,
                            0,
                            4),
                    Padding =
                        new Padding(
                            12,
                            0,
                            12,
                            0),
                    BackColor =
                        ColorTarjeta,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    ForeColor =
                        ColorTexto,
                    Text =
                        "Seleccione un viaje para consultar sus visitas.",
                    TextAlign =
                        ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };

            raiz.Controls.Add(
                _lblViajeSeleccionado,
                0,
                2);

            _grillaVisitas =
                CrearGrillaBase();

            AgregarColumna(
                _grillaVisitas,
                "IdVisita",
                "ID",
                60);

            AgregarColumna(
                _grillaVisitas,
                "Fecha",
                "Fecha",
                100,
                "dd/MM/yyyy");

            AgregarColumna(
                _grillaVisitas,
                "LocalidadEncuentro",
                "Localidad",
                170);

            AgregarColumna(
                _grillaVisitas,
                "Observacion",
                "Observación",
                380);

            AgregarColumna(
                _grillaVisitas,
                "ClientesResumen",
                "Clientes",
                420);

            _grillaVisitas.SelectionChanged +=
                GrillaVisitas_SelectionChanged;

            _grillaVisitas.CellDoubleClick +=
                GrillaVisitas_CellDoubleClick;

            _grillaVisitas.DataBindingComplete +=
                GrillaVisitas_DataBindingComplete;

            raiz.Controls.Add(
                _grillaVisitas,
                0,
                3);

            raiz.Controls.Add(
                CrearPanelAcciones(),
                0,
                4);

            _lblCantidad =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    ForeColor =
                        ColorTexto,
                    Text =
                        "Resultados: 0",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            raiz.Controls.Add(
                _lblCantidad,
                0,
                5);

            Controls.Add(
                raiz);

            AcceptButton =
                _btnConsultarSeleccion;

            CancelButton =
                _btnCerrar;
        }

        private static Control CrearEncabezado()
        {
            var panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor =
                        ColorTarjeta,
                    Padding =
                        new Padding(
                            16,
                            5,
                            16,
                            5)
                };

            panel.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            18F,
                            FontStyle.Bold),
                    ForeColor =
                        ColorTexto,
                    Text =
                        "Visitas comerciales",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                });

            return panel;
        }

        private Control CrearSelectorViajes()
        {
            var grupo =
                new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Text =
                        "Seleccionar viaje",
                    BackColor =
                        ColorTarjeta,
                    ForeColor =
                        ColorTexto,
                    Padding =
                        new Padding(8)
                };

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 5,
                    RowCount = 3,
                    Padding =
                        new Padding(
                            5,
                            3,
                            5,
                            5),
                    BackColor =
                        ColorTarjeta
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    31F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    14F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    14F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    15F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    26F));

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
                    SizeType.Percent,
                    100F));

            tabla.Controls.Add(
                CrearEtiquetaFiltro(
                    "Buscar por ID, descripción o participante"),
                0,
                0);

            tabla.Controls.Add(
                CrearEtiquetaFiltro(
                    "Fecha desde"),
                1,
                0);

            tabla.Controls.Add(
                CrearEtiquetaFiltro(
                    "Fecha hasta"),
                2,
                0);

            tabla.Controls.Add(
                CrearEtiquetaFiltro(
                    "Estado"),
                3,
                0);

            tabla.Controls.Add(
                CrearEtiquetaFiltro(
                    "Acciones"),
                4,
                0);

            _txtBuscarViaje =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(
                            3,
                            3,
                            6,
                            3)
                };

            _dtpViajeDesde =
                CrearFechaFiltro();

            _dtpViajeHasta =
                CrearFechaFiltro();

            _cmbEstadoViaje =
                new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Margin =
                        new Padding(
                            3,
                            3,
                            6,
                            3)
                };

            tabla.Controls.Add(
                _txtBuscarViaje,
                0,
                1);

            tabla.Controls.Add(
                _dtpViajeDesde,
                1,
                1);

            tabla.Controls.Add(
                _dtpViajeHasta,
                2,
                1);

            tabla.Controls.Add(
                _cmbEstadoViaje,
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
                    27F));

            acciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    27F));

            acciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    46F));

            acciones.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            _btnBuscarViajes =
                CrearBotonPrimario(
                    "Buscar");

            _btnLimpiarViajes =
                CrearBotonSecundario(
                    "Limpiar");

            _btnLimpiarViajes.Dock =
                DockStyle.Fill;

            _btnLimpiarViajes.Margin =
                new Padding(2);

            _btnConsultarSeleccion =
                CrearBotonPrimario(
                    "Consultar selección");

            _btnBuscarViajes.Click +=
                BtnBuscarViajes_Click;

            _btnLimpiarViajes.Click +=
                BtnLimpiarViajes_Click;

            _btnConsultarSeleccion.Click +=
                BtnConsultarSeleccion_Click;

            acciones.Controls.Add(
                _btnBuscarViajes,
                0,
                0);

            acciones.Controls.Add(
                _btnLimpiarViajes,
                1,
                0);

            acciones.Controls.Add(
                _btnConsultarSeleccion,
                2,
                0);

            tabla.Controls.Add(
                acciones,
                4,
                1);

            _grillaViajes =
                CrearGrillaBase();

            AgregarColumna(
                _grillaViajes,
                "IdViaje",
                "ID",
                65);

            AgregarColumna(
                _grillaViajes,
                "FechaInicio",
                "Inicio",
                95,
                "dd/MM/yyyy");

            AgregarColumna(
                _grillaViajes,
                "FechaFin",
                "Fin",
                95,
                "dd/MM/yyyy");

            AgregarColumna(
                _grillaViajes,
                "Descripcion",
                "Descripción",
                270);

            AgregarColumna(
                _grillaViajes,
                "TipoViaje",
                "Tipo",
                125);

            AgregarColumna(
                _grillaViajes,
                "ParticipantesResumen",
                "Participantes",
                330);

            AgregarColumna(
                _grillaViajes,
                "Estado",
                "Estado",
                115);

            _grillaViajes.SelectionChanged +=
                GrillaViajes_SelectionChanged;

            _grillaViajes.CellDoubleClick +=
                GrillaViajes_CellDoubleClick;

            tabla.Controls.Add(
                _grillaViajes,
                0,
                2);

            tabla.SetColumnSpan(
                _grillaViajes,
                5);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearPanelAcciones()
        {
            var panel =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection =
                        FlowDirection.LeftToRight,
                    WrapContents =
                        false,
                    Padding =
                        new Padding(
                            0,
                            8,
                            0,
                            5),
                    BackColor =
                        ColorFondo
                };

            _btnNuevaVisita =
                CrearBotonSecundario(
                    "Nueva visita");

            _btnNuevaVisita.Width =
                145;

            _btnModificarVisita =
                CrearBotonSecundario(
                    "Modificar visita");

            _btnModificarVisita.Width =
                145;

            _btnCerrar =
                CrearBotonSecundario(
                    "Cerrar");

            _btnCerrar.Width =
                110;

            _btnCerrar.DialogResult =
                DialogResult.Cancel;

            _btnNuevaVisita.Click +=
                BtnNuevaVisita_Click;

            _btnModificarVisita.Click +=
                BtnModificarVisita_Click;

            _btnCerrar.Click +=
                delegate
                {
                    Close();
                };

            panel.Controls.Add(
                _btnNuevaVisita);

            panel.Controls.Add(
                _btnModificarVisita);

            panel.Controls.Add(
                _btnCerrar);

            return panel;
        }

        private static Label CrearEtiquetaFiltro(
            string texto)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Font =
                    new Font(
                        "Segoe UI",
                        8.5F,
                        FontStyle.Bold),
                ForeColor =
                    ColorTexto,
                Text =
                    texto,
                TextAlign =
                    ContentAlignment.BottomLeft,
                AutoEllipsis = true
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
                        3,
                        3,
                        6,
                        3)
            };
        }

        private static Button CrearBotonPrimario(
            string texto)
        {
            var boton =
                new Button
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(2),
                    FlatStyle =
                        FlatStyle.Flat,
                    BackColor =
                        ColorPrimario,
                    ForeColor =
                        Color.White,
                    Font =
                        new Font(
                            "Segoe UI",
                            8.5F,
                            FontStyle.Bold),
                    Text =
                        texto,
                    UseVisualStyleBackColor =
                        false
                };

            boton.FlatAppearance.BorderSize =
                0;

            return boton;
        }

        private static Button CrearBotonSecundario(
            string texto)
        {
            var boton =
                new Button
                {
                    Height = 34,
                    Width = 120,
                    Margin =
                        new Padding(
                            0,
                            0,
                            10,
                            0),
                    FlatStyle =
                        FlatStyle.Flat,
                    BackColor =
                        Color.White,
                    ForeColor =
                        ColorTexto,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F),
                    Text =
                        texto,
                    UseVisualStyleBackColor =
                        false
                };

            boton.FlatAppearance.BorderColor =
                ColorBorde;

            boton.FlatAppearance.BorderSize =
                1;

            return boton;
        }

        private static DataGridView
            CrearGrillaBase()
        {
            var grilla =
                new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AutoGenerateColumns =
                        false,
                    AllowUserToAddRows =
                        false,
                    AllowUserToDeleteRows =
                        false,
                    AllowUserToResizeRows =
                        false,
                    MultiSelect =
                        false,
                    ReadOnly =
                        true,
                    RowHeadersVisible =
                        false,
                    SelectionMode =
                        DataGridViewSelectionMode
                            .FullRowSelect,
                    BackgroundColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    EnableHeadersVisualStyles =
                        false,
                    RowTemplate =
                    {
                        Height = 28
                    }
                };

            grilla.ColumnHeadersDefaultCellStyle
                .BackColor =
                    Color.FromArgb(
                        241,
                        245,
                        249);

            grilla.ColumnHeadersDefaultCellStyle
                .ForeColor =
                    ColorTexto;

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
                Grilla_CellFormatting;

            grilla.DataBindingComplete +=
                Grilla_DataBindingComplete;

            grilla.Sorted +=
                Grilla_Sorted;

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
                        ancho,
                    SortMode =
                        DataGridViewColumnSortMode
                            .Automatic
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

        private void ConfigurarPermisos()
        {
            _puedeRegistrar =
                _sesionActual.HayUsuarioAutenticado
                &&
                _autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        VisitaService
                            .PermisoRegistrar);

            _btnNuevaVisita.Visible =
                _puedeRegistrar;

            _btnModificarVisita.Visible =
                _puedeRegistrar;

            ActualizarEstadoAccionesVisita();
        }

        private void VisitasForm_Load(
            object sender,
            EventArgs e)
        {
            CargarEstados();
            RestablecerFiltrosViaje();
            BuscarViajes();
        }

        private void CargarEstados()
        {
            var opciones =
                new List<EstadoFiltroItem>
                {
                    new EstadoFiltroItem(
                        null,
                        "Todos")
                };

            foreach (
                EstadoViaje estado
                in Enum.GetValues(
                    typeof(EstadoViaje)))
            {
                opciones.Add(
                    new EstadoFiltroItem(
                        estado,
                        FormatearEnum(
                            estado.ToString())));
            }

            _cmbEstadoViaje.DataSource =
                opciones;

            _cmbEstadoViaje.DisplayMember =
                "Texto";

            _cmbEstadoViaje.SelectedIndex =
                0;
        }

        private void RestablecerFiltrosViaje()
        {
            _txtBuscarViaje.Text =
                string.Empty;

            _dtpViajeDesde.Value =
                DateTime.Today
                    .AddMonths(-1);

            _dtpViajeHasta.Value =
                DateTime.Today;

            _dtpViajeDesde.Checked =
                false;

            _dtpViajeHasta.Checked =
                false;

            if (_cmbEstadoViaje.Items.Count > 0)
            {
                _cmbEstadoViaje.SelectedIndex =
                    0;
            }
        }

        private void BtnBuscarViajes_Click(
            object sender,
            EventArgs e)
        {
            BuscarViajes();
        }

        private void BtnLimpiarViajes_Click(
            object sender,
            EventArgs e)
        {
            RestablecerFiltrosViaje();
            BuscarViajes();
        }

        private void BtnConsultarSeleccion_Click(
            object sender,
            EventArgs e)
        {
            CargarVisitas();
        }

        private void GrillaViajes_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.RowIndex >=
                    _grillaViajes.Rows.Count)
            {
                return;
            }

            ViajeListadoDto viaje =
                _grillaViajes.Rows[
                    e.RowIndex]
                    .DataBoundItem
                    as ViajeListadoDto;

            EstablecerViajeSeleccionado(
                viaje);

            CargarVisitas();
        }

        private void GrillaViajes_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ViajeListadoDto viaje =
                null;

            if (_grillaViajes.SelectedRows.Count > 0)
            {
                viaje =
                    _grillaViajes
                        .SelectedRows[0]
                        .DataBoundItem
                        as ViajeListadoDto;
            }

            EstablecerViajeSeleccionado(
                viaje);
        }

        private void EstablecerViajeSeleccionado(
            ViajeListadoDto viaje)
        {
            _viajeSeleccionado =
                viaje;

            ActualizarEstadoAccionesVisita();

            if (viaje == null)
            {
                _lblViajeSeleccionado.Text =
                    "Seleccione un viaje para consultar sus visitas.";

                return;
            }

            _lblViajeSeleccionado.Text =
                "Seleccionado: #"
                +
                viaje.IdViaje
                +
                " — "
                +
                viaje.FechaInicio
                    .ToString(
                        "dd/MM/yyyy")
                +
                " al "
                +
                viaje.FechaFin
                    .ToString(
                        "dd/MM/yyyy")
                +
                " — "
                +
                viaje.Descripcion
                +
                " — "
                +
                FormatearEnum(
                    viaje.Estado
                        .ToString());
        }

        private void BuscarViajes()
        {
            CambiarEstadoCarga(
                true);

            try
            {
                EstadoFiltroItem
                    estadoSeleccionado =
                        _cmbEstadoViaje
                            .SelectedItem
                            as EstadoFiltroItem;

                var filtro =
                    new ViajeFiltro(
                        _dtpViajeDesde.Checked
                            ? _dtpViajeDesde.Value
                            : (DateTime?)null,
                        _dtpViajeHasta.Checked
                            ? _dtpViajeHasta.Value
                            : (DateTime?)null,
                        estadoSeleccionado == null
                            ? (EstadoViaje?)null
                            : estadoSeleccionado.Valor,
                        null);

                string busqueda =
                    (
                        _txtBuscarViaje.Text
                        ?? string.Empty
                    ).Trim();

                List<ViajeListadoDto>
                    viajes =
                        _viajeService
                            .Listar(
                                filtro)
                            .Where(
                                viaje =>
                                    CoincideBusqueda(
                                        viaje,
                                        busqueda))
                            .OrderByDescending(
                                viaje =>
                                    viaje.FechaInicio)
                            .ThenByDescending(
                                viaje =>
                                    viaje.IdViaje)
                            .ToList();

                _grillaViajes.DataSource =
                    new SortableBindingList
                        <ViajeListadoDto>(
                            viajes);

                EstablecerViajeSeleccionado(
                    null);

                LimpiarVisitas();

                if (viajes.Count == 0)
                {
                    _lblViajeSeleccionado.Text =
                        "No se encontraron viajes con los filtros indicados.";
                }
            }
            catch (Exception exception)
            {
                _grillaViajes.DataSource =
                    null;

                EstablecerViajeSeleccionado(
                    null);

                _lblViajeSeleccionado.Text =
                    "No fue posible cargar los viajes.";

                LimpiarVisitas();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private static bool CoincideBusqueda(
            ViajeListadoDto viaje,
            string busqueda)
        {
            if (string.IsNullOrWhiteSpace(
                busqueda))
            {
                return true;
            }

            string termino =
                busqueda.Trim();

            string posibleId =
                termino.StartsWith(
                    "#",
                    StringComparison.Ordinal)
                    ? termino.Substring(1)
                    : termino;

            int idBuscado;

            if (int.TryParse(
                    posibleId,
                    out idBuscado)
                &&
                viaje.IdViaje ==
                    idBuscado)
            {
                return true;
            }

            return ContieneTexto(
                    viaje.Descripcion,
                    termino)
                ||
                ContieneTexto(
                    viaje.ParticipantesResumen,
                    termino);
        }

        private static bool ContieneTexto(
            string origen,
            string termino)
        {
            return (
                origen
                ?? string.Empty
            ).IndexOf(
                termino,
                StringComparison
                    .OrdinalIgnoreCase)
                >= 0;
        }

        private void GrillaVisitas_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ActualizarEstadoAccionesVisita();
        }

        private void GrillaVisitas_DataBindingComplete(
            object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            ActualizarEstadoAccionesVisita();
        }

        private void GrillaVisitas_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.RowIndex >=
                    _grillaVisitas.Rows.Count)
            {
                return;
            }

            VisitaListadoDto visita =
                _grillaVisitas.Rows[
                    e.RowIndex]
                    .DataBoundItem
                    as VisitaListadoDto;

            AbrirModificacionVisita(
                visita);
        }

        private void BtnModificarVisita_Click(
            object sender,
            EventArgs e)
        {
            AbrirModificacionVisita(
                ObtenerVisitaSeleccionada());
        }

        private void AbrirModificacionVisita(
            VisitaListadoDto visitaSeleccionada)
        {
            if (_viajeSeleccionado == null)
            {
                MostrarViajeRequerido();
                return;
            }

            if (_viajeSeleccionado.Estado !=
                EstadoViaje.Abierto)
            {
                MessageBox.Show(
                    "Solo se pueden modificar visitas de viajes abiertos.",
                    "Visitas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (visitaSeleccionada == null)
            {
                MessageBox.Show(
                    "Seleccione una visita para modificarla.",
                    "Visitas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            CambiarEstadoCarga(
                true);

            try
            {
                Visita visita =
                    _visitaService.Obtener(
                        _viajeSeleccionado.IdViaje,
                        visitaSeleccionada.IdVisita);

                using (
                    var formulario =
                        new VisitaEditForm(
                            _visitaService,
                            _viajeSeleccionado.IdViaje,
                            _viajeSeleccionado.FechaInicio,
                            _viajeSeleccionado.FechaFin,
                            CrearDescripcionCompleta(
                                _viajeSeleccionado),
                            visita))
                {
                    if (formulario.ShowDialog(this) ==
                        DialogResult.OK)
                    {
                        CargarVisitas();
                    }
                }
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

        private VisitaListadoDto
            ObtenerVisitaSeleccionada()
        {
            if (_grillaVisitas.SelectedRows.Count == 0)
            {
                return null;
            }

            return _grillaVisitas
                .SelectedRows[0]
                .DataBoundItem
                as VisitaListadoDto;
        }

        private void ActualizarEstadoAccionesVisita()
        {
            bool viajeAbierto =
                _viajeSeleccionado != null &&
                _viajeSeleccionado.Estado ==
                    EstadoViaje.Abierto;

            if (_btnNuevaVisita != null)
            {
                _btnNuevaVisita.Enabled =
                    _puedeRegistrar &&
                    viajeAbierto;
            }

            if (_btnModificarVisita != null)
            {
                _btnModificarVisita.Enabled =
                    _puedeRegistrar &&
                    viajeAbierto &&
                    ObtenerVisitaSeleccionada() != null;
            }
        }

        private void BtnNuevaVisita_Click(
            object sender,
            EventArgs e)
        {
            if (_viajeSeleccionado == null)
            {
                MostrarViajeRequerido();
                return;
            }

            if (_viajeSeleccionado.Estado !=
                EstadoViaje.Abierto)
            {
                MessageBox.Show(
                    "Solo se pueden registrar visitas en viajes abiertos.",
                    "Visitas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (
                var formulario =
                    new VisitaEditForm(
                        _visitaService,
                        _viajeSeleccionado.IdViaje,
                        _viajeSeleccionado.FechaInicio,
                        _viajeSeleccionado.FechaFin,
                        CrearDescripcionCompleta(
                            _viajeSeleccionado)))
            {
                if (formulario.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    CargarVisitas();
                }
            }
        }

        private void CargarVisitas()
        {
            if (_viajeSeleccionado == null)
            {
                MostrarViajeRequerido();
                return;
            }

            CambiarEstadoCarga(
                true);

            try
            {
                List<VisitaListadoDto>
                    visitas =
                        _visitaService
                            .ListarPorViaje(
                                _viajeSeleccionado
                                    .IdViaje)
                            .ToList();

                _grillaVisitas.DataSource =
                    new SortableBindingList
                        <VisitaListadoDto>(
                            visitas);

                _lblCantidad.Text =
                    "Resultados: "
                    +
                    visitas.Count;

                EstablecerViajeSeleccionado(
                    _viajeSeleccionado);

                ActualizarEstadoAccionesVisita();
            }
            catch (Exception exception)
            {
                LimpiarVisitas();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void LimpiarVisitas()
        {
            _grillaVisitas.DataSource =
                null;

            _lblCantidad.Text =
                "Resultados: 0";

            ActualizarEstadoAccionesVisita();
        }

        private static string
            CrearDescripcionCompleta(
                ViajeListadoDto viaje)
        {
            return "#"
                +
                viaje.IdViaje
                +
                " - "
                +
                viaje.FechaInicio
                    .ToString(
                        "dd/MM/yyyy")
                +
                " al "
                +
                viaje.FechaFin
                    .ToString(
                        "dd/MM/yyyy")
                +
                " - "
                +
                viaje.Descripcion
                +
                " - "
                +
                FormatearEnum(
                    viaje.Estado
                        .ToString());
        }

        private static void MostrarViajeRequerido()
        {
            MessageBox.Show(
                "Seleccione un viaje.",
                "Visitas",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void CambiarEstadoCarga(
            bool cargando)
        {
            _txtBuscarViaje.Enabled =
                !cargando;

            _dtpViajeDesde.Enabled =
                !cargando;

            _dtpViajeHasta.Enabled =
                !cargando;

            _cmbEstadoViaje.Enabled =
                !cargando;

            _btnBuscarViajes.Enabled =
                !cargando;

            _btnLimpiarViajes.Enabled =
                !cargando;

            _btnConsultarSeleccion.Enabled =
                !cargando;

            _grillaViajes.Enabled =
                !cargando;

            if (cargando)
            {
                _btnNuevaVisita.Enabled =
                    false;

                _btnModificarVisita.Enabled =
                    false;
            }
            else
            {
                ActualizarEstadoAccionesVisita();
            }

            UseWaitCursor =
                cargando;

            Cursor =
                cargando
                    ? Cursors.WaitCursor
                    : Cursors.Default;
        }

        private static void
            Grilla_DataBindingComplete(
                object sender,
                DataGridViewBindingCompleteEventArgs e)
        {
            DataGridView grilla =
                sender as DataGridView;

            LimpiarSeleccionAutomatica(
                grilla);

            if (grilla == null
                ||
                grilla.IsDisposed
                ||
                !grilla.IsHandleCreated)
            {
                return;
            }

            grilla.BeginInvoke(
                (MethodInvoker)(
                    delegate
                    {
                        if (!grilla.IsDisposed)
                        {
                            LimpiarSeleccionAutomatica(
                                grilla);
                        }
                    }));
        }

        private static void Grilla_Sorted(
            object sender,
            EventArgs e)
        {
            LimpiarSeleccionAutomatica(
                sender as DataGridView);
        }

        private static void
            LimpiarSeleccionAutomatica(
                DataGridView grilla)
        {
            if (grilla == null
                ||
                grilla.IsDisposed)
            {
                return;
            }

            grilla.ClearSelection();
            grilla.CurrentCell =
                null;
        }

        private static void Grilla_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null)
            {
                return;
            }

            Type tipo =
                e.Value.GetType();

            if (!tipo.IsEnum)
            {
                return;
            }

            e.Value =
                FormatearEnum(
                    e.Value.ToString());

            e.FormattingApplied =
                true;
        }

        private static string FormatearEnum(
            string valor)
        {
            switch (valor)
            {
                case "EnRendicion":
                    return "En rendición";

                case "EventoFeria":
                    return "Evento o feria";

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
                    ArgumentException)
            {
                MessageBox.Show(
                    exception.Message,
                    "Visitas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is
                PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible acceder a las visitas. "
                    +
                    "Verifique la conexión con SQL Server "
                    +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado en el módulo de visitas.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private sealed class EstadoFiltroItem
        {
            public EstadoFiltroItem(
                EstadoViaje? valor,
                string texto)
            {
                Valor = valor;
                Texto =
                    texto
                    ?? string.Empty;
            }

            public EstadoViaje? Valor
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
    }
}
