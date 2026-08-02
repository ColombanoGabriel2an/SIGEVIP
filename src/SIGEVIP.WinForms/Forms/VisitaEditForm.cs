using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.WinForms.Controls;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class VisitaEditForm : Form
    {
        private readonly VisitaService
            _visitaService;

        private readonly int
            _idViaje;

        private readonly DateTime
            _fechaInicioViaje;

        private readonly DateTime
            _fechaFinViaje;

        private readonly string
            _descripcionViaje;

        private readonly Visita
            _visita;

        private readonly HashSet<int>
            _idsClientesSeleccionados;

        private List<ClienteSeleccionVisitaDto>
            _clientesDisponibles;

        private DateTimePicker _dtpFecha;
        private TextBox _txtLocalidad;
        private TextBox _txtObservacion;
        private TextBox _txtBuscarCliente;
        private Button _btnBuscarClientes;
        private Button _btnLimpiarFiltro;
        private DataGridView _grillaClientes;
        private Label _lblCantidadClientes;
        private Button _btnSeleccionarVisibles;
        private Button _btnQuitarSeleccion;
        private Button _btnGuardar;
        private Button _btnCancelar;
        private bool _actualizandoGrilla;

        private bool EsEdicion
        {
            get
            {
                return _visita != null;
            }
        }

        public VisitaEditForm(
            VisitaService visitaService,
            int idViaje,
            DateTime fechaInicioViaje,
            DateTime fechaFinViaje,
            string descripcionViaje)
            : this(
                visitaService,
                idViaje,
                fechaInicioViaje,
                fechaFinViaje,
                descripcionViaje,
                null)
        {
        }

        public VisitaEditForm(
            VisitaService visitaService,
            int idViaje,
            DateTime fechaInicioViaje,
            DateTime fechaFinViaje,
            string descripcionViaje,
            Visita visita)
        {
            _visitaService =
                visitaService
                ?? throw new ArgumentNullException(
                    nameof(visitaService));

            if (idViaje <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idViaje));
            }

            if (visita != null &&
                visita.IdViaje != idViaje)
            {
                throw new ArgumentException(
                    "La visita no pertenece al viaje indicado.",
                    nameof(visita));
            }

            _idViaje = idViaje;
            _fechaInicioViaje =
                fechaInicioViaje.Date;
            _fechaFinViaje =
                fechaFinViaje.Date;
            _descripcionViaje =
                descripcionViaje
                ?? string.Empty;
            _visita = visita;

            _idsClientesSeleccionados =
                new HashSet<int>();

            _clientesDisponibles =
                new List<ClienteSeleccionVisitaDto>();

            InicializarFormulario();

            Load +=
                VisitaEditForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                EsEdicion
                    ? "SIGEVIP - Modificar visita"
                    : "SIGEVIP - Registrar visita";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(
                    980,
                    760);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            var raiz =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 7,
                    Padding =
                        new Padding(18),
                    BackColor =
                        Color.WhiteSmoke
                };

            raiz.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    42F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    58F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    62F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    112F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    44F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    52F));

            raiz.Controls.Add(
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            16F,
                            FontStyle.Bold),
                    Text =
                        EsEdicion
                            ? "Modificar visita comercial"
                            : "Registrar visita comercial",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                },
                0,
                0);

            raiz.Controls.Add(
                CrearCabeceraViaje(),
                0,
                1);

            raiz.Controls.Add(
                CrearDatosBasicos(),
                0,
                2);

            raiz.Controls.Add(
                CrearObservacion(),
                0,
                3);

            raiz.Controls.Add(
                CrearSelectorClientes(),
                0,
                4);

            raiz.Controls.Add(
                CrearResumenSeleccion(),
                0,
                5);

            raiz.Controls.Add(
                CrearAcciones(),
                0,
                6);

            Controls.Add(
                raiz);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private Control CrearCabeceraViaje()
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Margin =
                    new Padding(
                        3,
                        2,
                        3,
                        6),
                Padding =
                    new Padding(10),
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
                Text =
                    _descripcionViaje,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private Control CrearDatosBasicos()
        {
            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    Margin =
                        new Padding(
                            3,
                            0,
                            3,
                            4)
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    27F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    73F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    22F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Fecha"),
                0,
                0);

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Localidad del encuentro"),
                1,
                0);

            _dtpFecha =
                new DateTimePicker
                {
                    Dock = DockStyle.Fill,
                    Format =
                        DateTimePickerFormat.Short,
                    MinDate =
                        _fechaInicioViaje,
                    MaxDate =
                        _fechaFinViaje,
                    Value =
                        ObtenerFechaInicial(),
                    Margin =
                        new Padding(
                            3,
                            2,
                            12,
                            3)
                };

            _txtLocalidad =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    MaxLength = 150,
                    Margin =
                        new Padding(
                            3,
                            2,
                            3,
                            3)
                };

            tabla.Controls.Add(
                _dtpFecha,
                0,
                1);

            tabla.Controls.Add(
                _txtLocalidad,
                1,
                1);

            return tabla;
        }

        private Control CrearObservacion()
        {
            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Margin =
                        new Padding(
                            3,
                            0,
                            3,
                            5)
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    22F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Observación"),
                0,
                0);

            _txtObservacion =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    AcceptsReturn = true,
                    MaxLength = 1000,
                    Multiline = true,
                    ScrollBars =
                        ScrollBars.Vertical,
                    Margin =
                        new Padding(
                            3,
                            2,
                            3,
                            3)
                };

            tabla.Controls.Add(
                _txtObservacion,
                0,
                1);

            return tabla;
        }

        private Control CrearSelectorClientes()
        {
            var grupo =
                new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Text =
                        "Clientes",
                    Padding =
                        new Padding(8),
                    BackColor =
                        Color.White
                };

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 3,
                    Padding =
                        new Padding(
                            5,
                            2,
                            5,
                            5),
                    BackColor =
                        Color.White
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    105F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    105F));

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
                CrearEtiqueta(
                    "Buscar por razón social o CUIT"),
                0,
                0);

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Acciones"),
                1,
                0);

            tabla.SetColumnSpan(
                tabla.GetControlFromPosition(
                    1,
                    0),
                2);

            _txtBuscarCliente =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(
                            3,
                            3,
                            8,
                            3)
                };

            _btnBuscarClientes =
                CrearBoton(
                    "Buscar");

            _btnLimpiarFiltro =
                CrearBoton(
                    "Limpiar");

            _btnBuscarClientes.Dock =
                DockStyle.Fill;

            _btnLimpiarFiltro.Dock =
                DockStyle.Fill;

            _btnBuscarClientes.Margin =
                new Padding(2);

            _btnLimpiarFiltro.Margin =
                new Padding(2);

            _btnBuscarClientes.Click +=
                BtnBuscarClientes_Click;

            _btnLimpiarFiltro.Click +=
                BtnLimpiarFiltro_Click;

            tabla.Controls.Add(
                _txtBuscarCliente,
                0,
                1);

            tabla.Controls.Add(
                _btnBuscarClientes,
                1,
                1);

            tabla.Controls.Add(
                _btnLimpiarFiltro,
                2,
                1);

            _grillaClientes =
                CrearGrillaClientes();

            tabla.Controls.Add(
                _grillaClientes,
                0,
                2);

            tabla.SetColumnSpan(
                _grillaClientes,
                3);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearResumenSeleccion()
        {
            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 1,
                    Margin =
                        new Padding(
                            3,
                            3,
                            3,
                            3)
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    165F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    145F));

            _lblCantidadClientes =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Text =
                        "Clientes seleccionados: 0",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            _btnSeleccionarVisibles =
                CrearBoton(
                    "Seleccionar visibles");

            _btnQuitarSeleccion =
                CrearBoton(
                    "Quitar selección");

            _btnSeleccionarVisibles.Dock =
                DockStyle.Fill;

            _btnQuitarSeleccion.Dock =
                DockStyle.Fill;

            _btnSeleccionarVisibles.Margin =
                new Padding(
                    3,
                    2,
                    3,
                    2);

            _btnQuitarSeleccion.Margin =
                new Padding(
                    3,
                    2,
                    3,
                    2);

            _btnSeleccionarVisibles.Click +=
                BtnSeleccionarVisibles_Click;

            _btnQuitarSeleccion.Click +=
                BtnQuitarSeleccion_Click;

            tabla.Controls.Add(
                _lblCantidadClientes,
                0,
                0);

            tabla.Controls.Add(
                _btnSeleccionarVisibles,
                1,
                0);

            tabla.Controls.Add(
                _btnQuitarSeleccion,
                2,
                0);

            return tabla;
        }

        private Control CrearAcciones()
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
                            0,
                            0)
                };

            _btnCancelar =
                CrearBoton(
                    "Cancelar");

            _btnCancelar.Width =
                120;

            _btnCancelar.DialogResult =
                DialogResult.Cancel;

            _btnGuardar =
                CrearBoton(
                    "Guardar");

            _btnGuardar.Width =
                120;

            _btnGuardar.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            _btnGuardar.Click +=
                BtnGuardar_Click;

            _btnCancelar.Click +=
                delegate
                {
                    DialogResult =
                        DialogResult.Cancel;

                    Close();
                };

            panel.Controls.Add(
                _btnCancelar);

            panel.Controls.Add(
                _btnGuardar);

            return panel;
        }

        private static Label CrearEtiqueta(
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
                Text = texto,
                TextAlign =
                    ContentAlignment.BottomLeft,
                AutoEllipsis = true
            };
        }

        private static Button CrearBoton(
            string texto)
        {
            return new Button
            {
                Height = 34,
                Text = texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private DataGridView CrearGrillaClientes()
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
                    ReadOnly = false,
                    RowHeadersVisible = false,
                    SelectionMode =
                        DataGridViewSelectionMode
                            .FullRowSelect,
                    BackgroundColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
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
                .Font =
                    new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold);

            grilla.ColumnHeadersHeight =
                32;

            grilla.RowTemplate.Height =
                28;

            grilla.Columns.Add(
                new DataGridViewCheckBoxColumn
                {
                    Name =
                        "Seleccionado",
                    HeaderText =
                        "Seleccionar",
                    Width = 90,
                    ReadOnly = false,
                    SortMode =
                        DataGridViewColumnSortMode
                            .NotSortable
                });

            AgregarColumnaTexto(
                grilla,
                "RazonSocial",
                "Razón social",
                430);

            AgregarColumnaTexto(
                grilla,
                "Cuit",
                "CUIT",
                170);

            AgregarColumnaTexto(
                grilla,
                "Activo",
                "Estado",
                120);

            grilla.CurrentCellDirtyStateChanged +=
                GrillaClientes_CurrentCellDirtyStateChanged;

            grilla.CellBeginEdit +=
                GrillaClientes_CellBeginEdit;

            grilla.CellValueChanged +=
                GrillaClientes_CellValueChanged;

            grilla.CellFormatting +=
                GrillaClientes_CellFormatting;

            grilla.DataBindingComplete +=
                GrillaClientes_DataBindingComplete;

            grilla.Sorted +=
                GrillaClientes_Sorted;

            return grilla;
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
                    HeaderText = titulo,
                    Name = propiedad,
                    Width = ancho,
                    ReadOnly = true,
                    SortMode =
                        DataGridViewColumnSortMode
                            .Automatic
                });
        }

        private DateTime ObtenerFechaInicial()
        {
            if (EsEdicion)
            {
                return _visita.Fecha;
            }

            DateTime hoy =
                DateTime.Today;

            if (hoy < _fechaInicioViaje)
            {
                return _fechaInicioViaje;
            }

            if (hoy > _fechaFinViaje)
            {
                return _fechaFinViaje;
            }

            return hoy;
        }

        private void VisitaEditForm_Load(
            object sender,
            EventArgs e)
        {
            CargarDatosVisita();
            CargarClientes();
        }

        private void CargarDatosVisita()
        {
            if (!EsEdicion)
            {
                return;
            }

            _dtpFecha.Value =
                _visita.Fecha;

            _txtLocalidad.Text =
                _visita.LocalidadEncuentro;

            _txtObservacion.Text =
                _visita.Observacion;

            foreach (
                Cliente cliente
                in _visita.Clientes)
            {
                if (cliente.IdCliente > 0)
                {
                    _idsClientesSeleccionados.Add(
                        cliente.IdCliente);
                }
            }
        }

        private void CargarClientes()
        {
            CambiarEstadoCarga(
                true);

            try
            {
                List<ClienteSeleccionVisitaDto> activos =
                    _visitaService
                        .ListarClientesDisponibles()
                        .ToList();

                var porId =
                    activos.ToDictionary(
                        cliente =>
                            cliente.IdCliente);

                if (EsEdicion)
                {
                    foreach (
                        Cliente cliente
                        in _visita.Clientes)
                    {
                        if (!porId.ContainsKey(
                            cliente.IdCliente))
                        {
                            porId.Add(
                                cliente.IdCliente,
                                new ClienteSeleccionVisitaDto(
                                    cliente.IdCliente,
                                    cliente.RazonSocial,
                                    cliente.Cuit,
                                    cliente.Activo));
                        }
                    }
                }

                _clientesDisponibles =
                    porId.Values
                        .OrderBy(
                            cliente =>
                                cliente.RazonSocial)
                        .ThenBy(
                            cliente =>
                                cliente.IdCliente)
                        .ToList();

                AplicarFiltroClientes();

                if (_clientesDisponibles.Count == 0)
                {
                    MessageBox.Show(
                        "No existen clientes activos disponibles. "
                        +
                        "Debe registrar o activar un cliente antes "
                        +
                        "de cargar una visita.",
                        "Visitas",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
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

        private void BtnBuscarClientes_Click(
            object sender,
            EventArgs e)
        {
            AplicarFiltroClientes();
        }

        private void BtnLimpiarFiltro_Click(
            object sender,
            EventArgs e)
        {
            _txtBuscarCliente.Text =
                string.Empty;

            AplicarFiltroClientes();
        }

        private void AplicarFiltroClientes()
        {
            string filtro =
                (
                    _txtBuscarCliente.Text
                    ?? string.Empty
                ).Trim();

            List<ClienteSeleccionVisitaDto> filtrados =
                _clientesDisponibles
                    .Where(
                        cliente =>
                            string.IsNullOrWhiteSpace(
                                filtro)
                            ||
                            ContieneTexto(
                                cliente.RazonSocial,
                                filtro)
                            ||
                            ContieneTexto(
                                cliente.Cuit,
                                filtro))
                    .OrderBy(
                        cliente =>
                            cliente.RazonSocial)
                    .ThenBy(
                        cliente =>
                            cliente.IdCliente)
                    .ToList();

            _actualizandoGrilla = true;

            try
            {
                _grillaClientes.DataSource =
                    new SortableBindingList
                        <ClienteSeleccionVisitaDto>(
                            filtrados);

                AplicarSeleccionesVisibles();
            }
            finally
            {
                _actualizandoGrilla = false;
            }
        }

        private static bool ContieneTexto(
            string origen,
            string filtro)
        {
            return (
                origen
                ?? string.Empty
            ).IndexOf(
                filtro,
                StringComparison.OrdinalIgnoreCase)
                >= 0;
        }

        private void GrillaClientes_CurrentCellDirtyStateChanged(
            object sender,
            EventArgs e)
        {
            if (_grillaClientes.IsCurrentCellDirty)
            {
                _grillaClientes.CommitEdit(
                    DataGridViewDataErrorContexts
                        .Commit);
            }
        }

        private void GrillaClientes_CellBeginEdit(
            object sender,
            DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex !=
                    _grillaClientes.Columns[
                        "Seleccionado"].Index)
            {
                return;
            }

            ClienteSeleccionVisitaDto cliente =
                _grillaClientes.Rows[
                    e.RowIndex]
                    .DataBoundItem
                    as ClienteSeleccionVisitaDto;

            if (cliente == null)
            {
                return;
            }

            if (!cliente.Activo &&
                !_idsClientesSeleccionados.Contains(
                    cliente.IdCliente))
            {
                e.Cancel = true;
            }
        }

        private void GrillaClientes_CellValueChanged(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (_actualizandoGrilla ||
                e.RowIndex < 0 ||
                e.ColumnIndex !=
                    _grillaClientes.Columns[
                        "Seleccionado"].Index)
            {
                return;
            }

            DataGridViewRow fila =
                _grillaClientes.Rows[
                    e.RowIndex];

            ClienteSeleccionVisitaDto cliente =
                fila.DataBoundItem
                    as ClienteSeleccionVisitaDto;

            if (cliente == null)
            {
                return;
            }

            bool seleccionado =
                Convert.ToBoolean(
                    fila.Cells[
                        "Seleccionado"].Value
                    ?? false);

            if (seleccionado)
            {
                if (!cliente.Activo &&
                    !_idsClientesSeleccionados.Contains(
                        cliente.IdCliente))
                {
                    AplicarSeleccionesVisibles();
                    return;
                }

                _idsClientesSeleccionados.Add(
                    cliente.IdCliente);
            }
            else
            {
                _idsClientesSeleccionados.Remove(
                    cliente.IdCliente);
            }

            ActualizarCantidadSeleccionada();
        }

        private void GrillaClientes_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            ClienteSeleccionVisitaDto cliente =
                _grillaClientes.Rows[
                    e.RowIndex]
                    .DataBoundItem
                    as ClienteSeleccionVisitaDto;

            if (cliente == null)
            {
                return;
            }

            if (_grillaClientes.Columns[
                    e.ColumnIndex].Name ==
                "Activo")
            {
                e.Value =
                    cliente.Activo
                        ? "Activo"
                        : "Inactivo";

                e.FormattingApplied = true;
            }

            if (!cliente.Activo)
            {
                _grillaClientes.Rows[
                    e.RowIndex]
                    .DefaultCellStyle
                    .ForeColor =
                        Color.Gray;
            }
        }

        private void GrillaClientes_DataBindingComplete(
            object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            AplicarSeleccionesVisibles();
            LimpiarSeleccionGrilla();
        }

        private void GrillaClientes_Sorted(
            object sender,
            EventArgs e)
        {
            AplicarSeleccionesVisibles();
            LimpiarSeleccionGrilla();
        }

        private void AplicarSeleccionesVisibles()
        {
            bool estadoAnterior =
                _actualizandoGrilla;

            _actualizandoGrilla = true;

            try
            {
                foreach (
                    DataGridViewRow fila
                    in _grillaClientes.Rows)
                {
                    ClienteSeleccionVisitaDto cliente =
                        fila.DataBoundItem
                            as ClienteSeleccionVisitaDto;

                    if (cliente == null)
                    {
                        continue;
                    }

                    fila.Cells[
                        "Seleccionado"].Value =
                            _idsClientesSeleccionados
                                .Contains(
                                    cliente.IdCliente);
                }
            }
            finally
            {
                _actualizandoGrilla =
                    estadoAnterior;
            }

            ActualizarCantidadSeleccionada();
        }

        private void LimpiarSeleccionGrilla()
        {
            _grillaClientes.ClearSelection();
            _grillaClientes.CurrentCell =
                null;
        }

        private void BtnSeleccionarVisibles_Click(
            object sender,
            EventArgs e)
        {
            foreach (
                DataGridViewRow fila
                in _grillaClientes.Rows)
            {
                ClienteSeleccionVisitaDto cliente =
                    fila.DataBoundItem
                        as ClienteSeleccionVisitaDto;

                if (cliente != null &&
                    cliente.Activo)
                {
                    _idsClientesSeleccionados.Add(
                        cliente.IdCliente);
                }
            }

            AplicarSeleccionesVisibles();
        }

        private void BtnQuitarSeleccion_Click(
            object sender,
            EventArgs e)
        {
            _idsClientesSeleccionados.Clear();
            AplicarSeleccionesVisibles();
        }

        private void ActualizarCantidadSeleccionada()
        {
            _lblCantidadClientes.Text =
                "Clientes seleccionados: "
                +
                _idsClientesSeleccionados.Count;
        }

        private void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            List<int> idsClientes =
                _clientesDisponibles
                    .Where(
                        cliente =>
                            _idsClientesSeleccionados
                                .Contains(
                                    cliente.IdCliente))
                    .Select(
                        cliente =>
                            cliente.IdCliente)
                    .ToList();

            CambiarEstadoCarga(
                true);

            try
            {
                if (EsEdicion)
                {
                    _visitaService.Modificar(
                        _idViaje,
                        _visita.IdVisita,
                        _dtpFecha.Value.Date,
                        _txtObservacion.Text,
                        _txtLocalidad.Text,
                        idsClientes);

                    MessageBox.Show(
                        "La visita fue modificada correctamente.",
                        "Visitas",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    int idVisita =
                        _visitaService.Registrar(
                            _idViaje,
                            _dtpFecha.Value.Date,
                            _txtObservacion.Text,
                            _txtLocalidad.Text,
                            idsClientes);

                    MessageBox.Show(
                        "La visita fue registrada correctamente. "
                        +
                        "Identificador: "
                        +
                        idVisita
                        +
                        ".",
                        "Visitas",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                DialogResult =
                    DialogResult.OK;

                Close();
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

        private void CambiarEstadoCarga(
            bool cargando)
        {
            _btnGuardar.Enabled =
                !cargando;

            _btnSeleccionarVisibles.Enabled =
                !cargando;

            _btnQuitarSeleccion.Enabled =
                !cargando;

            _btnBuscarClientes.Enabled =
                !cargando;

            _btnLimpiarFiltro.Enabled =
                !cargando;

            _grillaClientes.Enabled =
                !cargando;

            UseWaitCursor =
                cargando;
        }

        private static void MostrarErrorControlado(
            Exception exception)
        {
            if (exception is ReglaNegocioException ||
                exception is AccesoDenegadoException ||
                exception is ArgumentException)
            {
                MessageBox.Show(
                    exception.Message,
                    "Visitas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible guardar la visita. "
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
                "Ocurrió un error inesperado al guardar la visita.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
