using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.WinForms.Controls;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ViajeEditForm : Form
    {
        private readonly ViajeService
            _viajeService;

        private readonly Viaje
            _viaje;

        private readonly ErrorProvider
            _errorProvider;

        private readonly HashSet<int>
            _idsParticipantesSeleccionados;

        private List<PersonaSeleccionDto>
            _participantesDisponibles;

        private DateTimePicker _dtpFechaInicio;
        private DateTimePicker _dtpFechaFin;
        private TextBox _txtDescripcion;
        private ComboBox _cmbTipoViaje;
        private NumericUpDown _nudMontoAnticipado;
        private TextBox _txtBuscarParticipante;
        private Button _btnBuscarParticipantes;
        private Button _btnLimpiarFiltro;
        private DataGridView _grillaParticipantes;
        private Label _lblParticipantes;
        private Button _btnSeleccionarVisibles;
        private Button _btnQuitarSeleccion;
        private Button _btnGuardar;
        private Button _btnCancelar;
        private bool _actualizandoGrilla;

        private bool EsEdicion
        {
            get
            {
                return _viaje != null;
            }
        }

        public ViajeEditForm(
            ViajeService viajeService,
            Viaje viaje)
        {
            _viajeService =
                viajeService
                ?? throw new ArgumentNullException(
                    nameof(viajeService));

            _viaje = viaje;

            _errorProvider =
                new ErrorProvider();

            _idsParticipantesSeleccionados =
                new HashSet<int>();

            _participantesDisponibles =
                new List<PersonaSeleccionDto>();

            InicializarFormulario();

            Load +=
                ViajeEditForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                EsEdicion
                    ? "Modificar viaje"
                    : "Nuevo viaje";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(
                    900,
                    720);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            _errorProvider.ContainerControl =
                this;

            var raiz =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 5,
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
                    44F));

            raiz.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    248F));

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
                            ? "Modificar viaje"
                            : "Nuevo viaje",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                },
                0,
                0);

            raiz.Controls.Add(
                CrearDatosGenerales(),
                0,
                1);

            raiz.Controls.Add(
                CrearSelectorParticipantes(),
                0,
                2);

            raiz.Controls.Add(
                CrearResumenParticipantes(),
                0,
                3);

            raiz.Controls.Add(
                CrearAcciones(),
                0,
                4);

            Controls.Add(
                raiz);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private Control CrearDatosGenerales()
        {
            var grupo =
                new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Text =
                        "Datos del viaje",
                    Padding =
                        new Padding(10),
                    BackColor =
                        Color.White
                };

            var tabla =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = 4,
                    Padding =
                        new Padding(
                            6,
                            3,
                            6,
                            6),
                    BackColor =
                        Color.White
                };

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    125F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    125F));

            tabla.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    38F));

            tabla.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    38F));

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
                    "Fecha de inicio"),
                0,
                0);

            _dtpFechaInicio =
                CrearFecha();

            tabla.Controls.Add(
                _dtpFechaInicio,
                1,
                0);

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Fecha de fin"),
                2,
                0);

            _dtpFechaFin =
                CrearFecha();

            tabla.Controls.Add(
                _dtpFechaFin,
                3,
                0);

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Tipo de viaje"),
                0,
                1);

            _cmbTipoViaje =
                new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Margin =
                        new Padding(
                            3,
                            5,
                            12,
                            4)
                };

            _cmbTipoViaje.DataSource =
                Enum.GetValues(
                    typeof(TipoViaje));

            tabla.Controls.Add(
                _cmbTipoViaje,
                1,
                1);

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Monto anticipado"),
                2,
                1);

            _nudMontoAnticipado =
                new NumericUpDown
                {
                    Dock = DockStyle.Fill,
                    DecimalPlaces = 2,
                    Increment = 100m,
                    Maximum =
                        9999999999999999m,
                    Minimum = 0m,
                    ThousandsSeparator = true,
                    Margin =
                        new Padding(
                            3,
                            5,
                            3,
                            4)
                };

            tabla.Controls.Add(
                _nudMontoAnticipado,
                3,
                1);

            tabla.Controls.Add(
                CrearEtiqueta(
                    "Descripción"),
                0,
                2);

            _txtDescripcion =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    MaxLength = 500,
                    Multiline = true,
                    ScrollBars =
                        ScrollBars.Vertical,
                    Margin =
                        new Padding(
                            3,
                            5,
                            3,
                            3)
                };

            tabla.Controls.Add(
                _txtDescripcion,
                1,
                2);

            tabla.SetColumnSpan(
                _txtDescripcion,
                3);

            tabla.SetRowSpan(
                _txtDescripcion,
                2);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearSelectorParticipantes()
        {
            var grupo =
                new GroupBox
                {
                    Dock = DockStyle.Fill,
                    Text =
                        "Participantes",
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
                    "Buscar por nombre, apellido o correo"),
                0,
                0);

            Label acciones =
                CrearEtiqueta(
                    "Acciones");

            tabla.Controls.Add(
                acciones,
                1,
                0);

            tabla.SetColumnSpan(
                acciones,
                2);

            _txtBuscarParticipante =
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

            _btnBuscarParticipantes =
                CrearBoton(
                    "Buscar");

            _btnLimpiarFiltro =
                CrearBoton(
                    "Limpiar");

            _btnBuscarParticipantes.Dock =
                DockStyle.Fill;

            _btnLimpiarFiltro.Dock =
                DockStyle.Fill;

            _btnBuscarParticipantes.Margin =
                new Padding(2);

            _btnLimpiarFiltro.Margin =
                new Padding(2);

            _btnBuscarParticipantes.Click +=
                BtnBuscarParticipantes_Click;

            _btnLimpiarFiltro.Click +=
                BtnLimpiarFiltro_Click;

            tabla.Controls.Add(
                _txtBuscarParticipante,
                0,
                1);

            tabla.Controls.Add(
                _btnBuscarParticipantes,
                1,
                1);

            tabla.Controls.Add(
                _btnLimpiarFiltro,
                2,
                1);

            _grillaParticipantes =
                CrearGrillaParticipantes();

            tabla.Controls.Add(
                _grillaParticipantes,
                0,
                2);

            tabla.SetColumnSpan(
                _grillaParticipantes,
                3);

            grupo.Controls.Add(
                tabla);

            return grupo;
        }

        private Control CrearResumenParticipantes()
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

            _lblParticipantes =
                new Label
                {
                    Dock = DockStyle.Fill,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Text =
                        "Participantes seleccionados: 0",
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
                _lblParticipantes,
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
                    ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
        }

        private static DateTimePicker CrearFecha()
        {
            return new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format =
                    DateTimePickerFormat.Short,
                Margin =
                    new Padding(
                        3,
                        5,
                        12,
                        4)
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

        private DataGridView CrearGrillaParticipantes()
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
                "NombreCompleto",
                "Apellido y nombre",
                320);

            AgregarColumnaTexto(
                grilla,
                "Email",
                "Correo",
                330);

            AgregarColumnaTexto(
                grilla,
                "Activo",
                "Estado",
                120);

            grilla.CurrentCellDirtyStateChanged +=
                GrillaParticipantes_CurrentCellDirtyStateChanged;

            grilla.CellBeginEdit +=
                GrillaParticipantes_CellBeginEdit;

            grilla.CellValueChanged +=
                GrillaParticipantes_CellValueChanged;

            grilla.CellFormatting +=
                GrillaParticipantes_CellFormatting;

            grilla.DataBindingComplete +=
                GrillaParticipantes_DataBindingComplete;

            grilla.Sorted +=
                GrillaParticipantes_Sorted;

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

        private void ViajeEditForm_Load(
            object sender,
            EventArgs e)
        {
            CargarDatosGenerales();
            CargarParticipantes();
        }

        private void CargarDatosGenerales()
        {
            if (!EsEdicion)
            {
                _dtpFechaInicio.Value =
                    DateTime.Today;

                _dtpFechaFin.Value =
                    DateTime.Today;

                _cmbTipoViaje.SelectedItem =
                    TipoViaje.Desplazamiento;

                return;
            }

            _dtpFechaInicio.Value =
                _viaje.FechaInicio;

            _dtpFechaFin.Value =
                _viaje.FechaFin;

            _txtDescripcion.Text =
                _viaje.Descripcion;

            _cmbTipoViaje.SelectedItem =
                _viaje.TipoViaje;

            _nudMontoAnticipado.Value =
                _viaje.MontoAnticipado;

            foreach (
                Persona participante
                in _viaje.Participantes)
            {
                if (participante.IdPersona > 0)
                {
                    _idsParticipantesSeleccionados.Add(
                        participante.IdPersona);
                }
            }
        }

        private void CargarParticipantes()
        {
            CambiarEstado(
                true);

            try
            {
                List<PersonaSeleccionDto> activos =
                    _viajeService
                        .ListarParticipantesDisponibles()
                        .ToList();

                var porId =
                    activos.ToDictionary(
                        participante =>
                            participante.IdPersona);

                if (EsEdicion)
                {
                    foreach (
                        Persona participante
                        in _viaje.Participantes)
                    {
                        if (!porId.ContainsKey(
                            participante.IdPersona))
                        {
                            porId.Add(
                                participante.IdPersona,
                                new PersonaSeleccionDto(
                                    participante.IdPersona,
                                    participante.Apellido
                                    +
                                    ", "
                                    +
                                    participante.Nombre,
                                    participante.Email,
                                    participante.Activo));
                        }
                    }
                }

                _participantesDisponibles =
                    porId.Values
                        .OrderBy(
                            participante =>
                                participante.NombreCompleto)
                        .ThenBy(
                            participante =>
                                participante.IdPersona)
                        .ToList();

                AplicarFiltroParticipantes();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);

                Close();
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void BtnBuscarParticipantes_Click(
            object sender,
            EventArgs e)
        {
            AplicarFiltroParticipantes();
        }

        private void BtnLimpiarFiltro_Click(
            object sender,
            EventArgs e)
        {
            _txtBuscarParticipante.Text =
                string.Empty;

            AplicarFiltroParticipantes();
        }

        private void AplicarFiltroParticipantes()
        {
            string filtro =
                (
                    _txtBuscarParticipante.Text
                    ?? string.Empty
                ).Trim();

            List<PersonaSeleccionDto> filtrados =
                _participantesDisponibles
                    .Where(
                        participante =>
                            string.IsNullOrWhiteSpace(
                                filtro)
                            ||
                            ContieneTexto(
                                participante.NombreCompleto,
                                filtro)
                            ||
                            ContieneTexto(
                                participante.Email,
                                filtro))
                    .OrderBy(
                        participante =>
                            participante.NombreCompleto)
                    .ThenBy(
                        participante =>
                            participante.IdPersona)
                    .ToList();

            _actualizandoGrilla = true;

            try
            {
                _grillaParticipantes.DataSource =
                    new SortableBindingList
                        <PersonaSeleccionDto>(
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

        private void GrillaParticipantes_CurrentCellDirtyStateChanged(
            object sender,
            EventArgs e)
        {
            if (_grillaParticipantes.IsCurrentCellDirty)
            {
                _grillaParticipantes.CommitEdit(
                    DataGridViewDataErrorContexts
                        .Commit);
            }
        }

        private void GrillaParticipantes_CellBeginEdit(
            object sender,
            DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex !=
                    _grillaParticipantes.Columns[
                        "Seleccionado"].Index)
            {
                return;
            }

            PersonaSeleccionDto participante =
                _grillaParticipantes.Rows[
                    e.RowIndex]
                    .DataBoundItem
                    as PersonaSeleccionDto;

            if (participante == null)
            {
                return;
            }

            if (!participante.Activo &&
                !_idsParticipantesSeleccionados.Contains(
                    participante.IdPersona))
            {
                e.Cancel = true;
            }
        }

        private void GrillaParticipantes_CellValueChanged(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (_actualizandoGrilla ||
                e.RowIndex < 0 ||
                e.ColumnIndex !=
                    _grillaParticipantes.Columns[
                        "Seleccionado"].Index)
            {
                return;
            }

            DataGridViewRow fila =
                _grillaParticipantes.Rows[
                    e.RowIndex];

            PersonaSeleccionDto participante =
                fila.DataBoundItem
                    as PersonaSeleccionDto;

            if (participante == null)
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
                if (!participante.Activo &&
                    !_idsParticipantesSeleccionados.Contains(
                        participante.IdPersona))
                {
                    AplicarSeleccionesVisibles();
                    return;
                }

                _idsParticipantesSeleccionados.Add(
                    participante.IdPersona);
            }
            else
            {
                _idsParticipantesSeleccionados.Remove(
                    participante.IdPersona);
            }

            ActualizarCantidadParticipantes();
        }

        private void GrillaParticipantes_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            PersonaSeleccionDto participante =
                _grillaParticipantes.Rows[
                    e.RowIndex]
                    .DataBoundItem
                    as PersonaSeleccionDto;

            if (participante == null)
            {
                return;
            }

            if (_grillaParticipantes.Columns[
                    e.ColumnIndex].Name ==
                "Activo")
            {
                e.Value =
                    participante.Activo
                        ? "Activo"
                        : "Inactivo";

                e.FormattingApplied = true;
            }

            if (!participante.Activo)
            {
                _grillaParticipantes.Rows[
                    e.RowIndex]
                    .DefaultCellStyle
                    .ForeColor =
                        Color.Gray;
            }
        }

        private void GrillaParticipantes_DataBindingComplete(
            object sender,
            DataGridViewBindingCompleteEventArgs e)
        {
            AplicarSeleccionesVisibles();
            LimpiarSeleccionGrilla();
        }

        private void GrillaParticipantes_Sorted(
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
                    in _grillaParticipantes.Rows)
                {
                    PersonaSeleccionDto participante =
                        fila.DataBoundItem
                            as PersonaSeleccionDto;

                    if (participante == null)
                    {
                        continue;
                    }

                    fila.Cells[
                        "Seleccionado"].Value =
                            _idsParticipantesSeleccionados
                                .Contains(
                                    participante.IdPersona);
                }
            }
            finally
            {
                _actualizandoGrilla =
                    estadoAnterior;
            }

            ActualizarCantidadParticipantes();
        }

        private void LimpiarSeleccionGrilla()
        {
            _grillaParticipantes.ClearSelection();
            _grillaParticipantes.CurrentCell =
                null;
        }

        private void BtnSeleccionarVisibles_Click(
            object sender,
            EventArgs e)
        {
            foreach (
                DataGridViewRow fila
                in _grillaParticipantes.Rows)
            {
                PersonaSeleccionDto participante =
                    fila.DataBoundItem
                        as PersonaSeleccionDto;

                if (participante != null &&
                    participante.Activo)
                {
                    _idsParticipantesSeleccionados.Add(
                        participante.IdPersona);
                }
            }

            AplicarSeleccionesVisibles();
        }

        private void BtnQuitarSeleccion_Click(
            object sender,
            EventArgs e)
        {
            _idsParticipantesSeleccionados.Clear();
            AplicarSeleccionesVisibles();
        }

        private void ActualizarCantidadParticipantes()
        {
            _lblParticipantes.Text =
                "Participantes seleccionados: "
                +
                _idsParticipantesSeleccionados.Count;
        }

        private void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            _errorProvider.Clear();

            if (!ValidarCampos())
            {
                return;
            }

            CambiarEstado(
                true);

            try
            {
                List<int> idsParticipantes =
                    _participantesDisponibles
                        .Where(
                            participante =>
                                _idsParticipantesSeleccionados
                                    .Contains(
                                        participante.IdPersona))
                        .Select(
                            participante =>
                                participante.IdPersona)
                        .ToList();

                TipoViaje tipo =
                    (TipoViaje)
                        _cmbTipoViaje.SelectedItem;

                if (EsEdicion)
                {
                    _viajeService.Modificar(
                        _viaje.IdViaje,
                        _dtpFechaInicio.Value.Date,
                        _dtpFechaFin.Value.Date,
                        _txtDescripcion.Text,
                        tipo,
                        _nudMontoAnticipado.Value,
                        idsParticipantes);
                }
                else
                {
                    _viajeService.Registrar(
                        _dtpFechaInicio.Value.Date,
                        _dtpFechaFin.Value.Date,
                        _txtDescripcion.Text,
                        tipo,
                        _nudMontoAnticipado.Value,
                        idsParticipantes);
                }

                MessageBox.Show(
                    EsEdicion
                        ? "El viaje fue modificado correctamente."
                        : "El viaje fue registrado correctamente.",
                    "Viajes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

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
                CambiarEstado(
                    false);
            }
        }

        private bool ValidarCampos()
        {
            bool valido = true;

            if (string.IsNullOrWhiteSpace(
                _txtDescripcion.Text))
            {
                _errorProvider.SetError(
                    _txtDescripcion,
                    "La descripción es obligatoria.");

                valido = false;
            }

            if (_dtpFechaInicio.Value.Date >
                _dtpFechaFin.Value.Date)
            {
                _errorProvider.SetError(
                    _dtpFechaFin,
                    "La fecha de fin no puede ser anterior a la fecha de inicio.");

                valido = false;
            }

            if (_cmbTipoViaje.SelectedItem == null)
            {
                _errorProvider.SetError(
                    _cmbTipoViaje,
                    "Seleccione un tipo de viaje.");

                valido = false;
            }

            if (_idsParticipantesSeleccionados.Count == 0)
            {
                _errorProvider.SetError(
                    _grillaParticipantes,
                    "Seleccione al menos un participante.");

                valido = false;
            }

            if (!valido)
            {
                MessageBox.Show(
                    "Revise los datos indicados.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return valido;
        }

        private void CambiarEstado(
            bool procesando)
        {
            _btnGuardar.Enabled =
                !procesando;

            _btnCancelar.Enabled =
                !procesando;

            _btnBuscarParticipantes.Enabled =
                !procesando;

            _btnLimpiarFiltro.Enabled =
                !procesando;

            _btnSeleccionarVisibles.Enabled =
                !procesando;

            _btnQuitarSeleccion.Enabled =
                !procesando;

            _grillaParticipantes.Enabled =
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
                MessageBox.Show(
                    exception.Message,
                    "Viajes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible guardar el viaje. "
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
                "Ocurrió un error inesperado al guardar el viaje.",
                "Error",
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

            base.Dispose(
                disposing);
        }
    }
}
