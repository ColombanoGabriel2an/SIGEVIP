using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ViajesForm : Form
    {
        private readonly ViajeService
            _viajeService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private DateTimePicker _dtpFechaDesde;
        private DateTimePicker _dtpFechaHasta;
        private ComboBox _cmbEstado;
        private ComboBox _cmbParticipante;

        private Button _btnBuscar;
        private Button _btnLimpiar;
        private Button _btnNuevo;
        private Button _btnModificar;
        private Button _btnDetalle;
        private Button _btnCancelarViaje;
        private Button _btnCerrar;

        private DataGridView _grilla;
        private Label _lblCantidad;

        public ViajesForm(
            ViajeService viajeService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
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
                ViajesForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Viajes";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(1150, 680);

            Size =
                new Size(1300, 760);

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
                        new Point(24, 20),
                    Text =
                        "Gestión de viajes"
                });

            Controls.Add(
                CrearPanelFiltros());

            _grilla =
                CrearGrilla();

            Controls.Add(
                _grilla);

            Controls.Add(
                CrearPanelAcciones());

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
                        new Point(25, 675),
                    Text =
                        "Resultados: 0"
                };

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
                        new Point(24, 67),
                    Size =
                        new Size(1235, 115)
                };

            _dtpFechaDesde =
                CrearFechaFiltro(
                    panel,
                    "Fecha desde",
                    18);

            _dtpFechaHasta =
                CrearFechaFiltro(
                    panel,
                    "Fecha hasta",
                    220);

            _cmbEstado =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(424, 42),
                    Size =
                        new Size(180, 24)
                };

            panel.Controls.Add(
                CrearEtiquetaFiltro(
                    "Estado",
                    424));

            _cmbParticipante =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    DisplayMember =
                        "NombreCompleto",
                    Location =
                        new Point(624, 42),
                    Size =
                        new Size(300, 24)
                };

            panel.Controls.Add(
                CrearEtiquetaFiltro(
                    "Participante",
                    624));

            _btnBuscar =
                CrearBotonFiltro(
                    "Buscar",
                    950);

            _btnLimpiar =
                CrearBotonFiltro(
                    "Limpiar",
                    1080);

            _btnBuscar.Click +=
                BtnBuscar_Click;

            _btnLimpiar.Click +=
                BtnLimpiar_Click;

            panel.Controls.Add(
                _cmbEstado);

            panel.Controls.Add(
                _cmbParticipante);

            panel.Controls.Add(
                _btnBuscar);

            panel.Controls.Add(
                _btnLimpiar);

            return panel;
        }

        private static DateTimePicker CrearFechaFiltro(
            Control contenedor,
            string titulo,
            int posicionX)
        {
            contenedor.Controls.Add(
                CrearEtiquetaFiltro(
                    titulo,
                    posicionX));

            var control =
                new DateTimePicker
                {
                    Checked = false,
                    Format =
                        DateTimePickerFormat.Short,
                    ShowCheckBox = true,
                    Location =
                        new Point(posicionX, 42),
                    Size =
                        new Size(180, 24)
                };

            contenedor.Controls.Add(
                control);

            return control;
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
                    new Point(posicionX, 17),
                Text =
                    texto
            };
        }

        private static Button CrearBotonFiltro(
            string texto,
            int posicionX)
        {
            return new Button
            {
                Location =
                    new Point(posicionX, 67),
                Size =
                    new Size(120, 32),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
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
                    Location =
                        new Point(24, 198),
                    MultiSelect = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    Size =
                        new Size(1235, 390)
                };

            AgregarColumna(
                grilla,
                "IdViaje",
                "Id",
                55);

            AgregarColumna(
                grilla,
                "FechaInicio",
                "Fecha inicio",
                95,
                "dd/MM/yyyy");

            AgregarColumna(
                grilla,
                "FechaFin",
                "Fecha fin",
                95,
                "dd/MM/yyyy");

            AgregarColumna(
                grilla,
                "Descripcion",
                "Descripción",
                230);

            AgregarColumna(
                grilla,
                "TipoViaje",
                "Tipo",
                115);

            AgregarColumna(
                grilla,
                "ParticipantesResumen",
                "Participantes",
                270);

            AgregarColumna(
                grilla,
                "MontoAnticipado",
                "Anticipo",
                100,
                "N2");

            AgregarColumna(
                grilla,
                "Estado",
                "Estado",
                105);

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
                        new Point(24, 602),
                    Size =
                        new Size(1235, 50),
                    WrapContents = false
                };

            _btnNuevo =
                CrearBotonAccion(
                    "Nuevo");

            _btnModificar =
                CrearBotonAccion(
                    "Modificar");

            _btnDetalle =
                CrearBotonAccion(
                    "Ver detalle");

            _btnCancelarViaje =
                CrearBotonAccion(
                    "Cancelar viaje");

            _btnCerrar =
                CrearBotonAccion(
                    "Cerrar");

            _btnNuevo.Click +=
                BtnNuevo_Click;

            _btnModificar.Click +=
                BtnModificar_Click;

            _btnDetalle.Click +=
                BtnDetalle_Click;

            _btnCancelarViaje.Click +=
                BtnCancelarViaje_Click;

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
                _btnCancelarViaje);

            panel.Controls.Add(
                _btnCerrar);

            return panel;
        }

        private static Button CrearBotonAccion(
            string texto)
        {
            return new Button
            {
                Margin =
                    new Padding(0, 0, 10, 0),
                Size =
                    new Size(130, 36),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private void ConfigurarPermisos()
        {
            bool puedeCrear =
                TienePermiso(
                    ViajeService.PermisoCrear);

            bool puedeCancelar =
                TienePermiso(
                    ViajeService.PermisoCancelar);

            _btnNuevo.Visible =
                puedeCrear;

            _btnModificar.Visible =
                puedeCrear;

            _btnCancelarViaje.Visible =
                puedeCancelar;
        }

        private bool TienePermiso(
            string codigo)
        {
            return _sesionActual
                .HayUsuarioAutenticado
                && _autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        codigo);
        }

        private void ViajesForm_Load(
            object sender,
            EventArgs e)
        {
            CargarFiltros();
            CargarViajes();
        }

        private void CargarFiltros()
        {
            _cmbEstado.Items.Clear();

            _cmbEstado.Items.Add(
                new EstadoFiltroItem(
                    "Todos",
                    null));

            foreach (
                EstadoViaje estado
                in Enum.GetValues(
                    typeof(EstadoViaje)))
            {
                _cmbEstado.Items.Add(
                    new EstadoFiltroItem(
                        estado.ToString(),
                        estado));
            }

            _cmbEstado.DisplayMember =
                "Texto";

            _cmbEstado.SelectedIndex = 0;

            _cmbParticipante.Items.Clear();

            _cmbParticipante.Items.Add(
                new ParticipanteFiltroItem(
                    0,
                    "Todos"));

            try
            {
                foreach (
                    PersonaSeleccionDto participante
                    in _viajeService
                        .ListarParticipantesDisponibles())
                {
                    _cmbParticipante.Items.Add(
                        new ParticipanteFiltroItem(
                            participante.IdPersona,
                            participante.NombreCompleto));
                }

                _cmbParticipante.DisplayMember =
                    "NombreCompleto";

                _cmbParticipante.SelectedIndex = 0;
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnBuscar_Click(
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
            _cmbEstado.SelectedIndex = 0;
            _cmbParticipante.SelectedIndex = 0;

            CargarViajes();
        }

        private void BtnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using (
                var formulario =
                    new ViajeEditForm(
                        _viajeService,
                        null))
            {
                if (formulario.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    CargarViajes();
                }
            }
        }

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            ViajeListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            try
            {
                Viaje viaje =
                    _viajeService.Obtener(
                        seleccionado.IdViaje);

                using (
                    var formulario =
                        new ViajeEditForm(
                            _viajeService,
                            viaje))
                {
                    if (formulario.ShowDialog(this) ==
                        DialogResult.OK)
                    {
                        CargarViajes();
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
            ViajeListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            try
            {
                Viaje viaje =
                    _viajeService.Obtener(
                        seleccionado.IdViaje);

                using (
                    var formulario =
                        new ViajeDetalleForm(
                            viaje))
                {
                    formulario.ShowDialog(
                        this);
                }
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnCancelarViaje_Click(
            object sender,
            EventArgs e)
        {
            ViajeListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            if (seleccionado.Estado ==
                EstadoViaje.Cancelado)
            {
                MessageBox.Show(
                    "El viaje ya se encuentra cancelado.",
                    "Viajes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult confirmacion =
                MessageBox.Show(
                    "¿Desea cancelar el viaje seleccionado?",
                    "Confirmar cancelación",
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
                _viajeService.Cancelar(
                    seleccionado.IdViaje);

                MessageBox.Show(
                    "El viaje fue cancelado correctamente.",
                    "Viajes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarViajes();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void CargarViajes()
        {
            CambiarEstadoCarga(true);

            try
            {
                var filtro =
                    new ViajeFiltro(
                        _dtpFechaDesde.Checked
                            ? _dtpFechaDesde.Value.Date
                            : (DateTime?)null,
                        _dtpFechaHasta.Checked
                            ? _dtpFechaHasta.Value.Date
                            : (DateTime?)null,
                        ObtenerEstadoFiltro(),
                        ObtenerParticipanteFiltro());

                List<ViajeListadoDto> viajes =
                    _viajeService
                        .Listar(
                            filtro)
                        .ToList();

                _grilla.DataSource =
                    null;

                _grilla.DataSource =
                    viajes;

                _lblCantidad.Text =
                    "Resultados: " +
                    viajes.Count;
            }
            catch (Exception exception)
            {
                _grilla.DataSource =
                    null;

                _lblCantidad.Text =
                    "Resultados: 0";

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(false);
            }
        }

        private EstadoViaje? ObtenerEstadoFiltro()
        {
            EstadoFiltroItem item =
                _cmbEstado.SelectedItem
                    as EstadoFiltroItem;

            return item == null
                ? null
                : item.Estado;
        }

        private int? ObtenerParticipanteFiltro()
        {
            ParticipanteFiltroItem item =
                _cmbParticipante.SelectedItem
                    as ParticipanteFiltroItem;

            if (item == null ||
                item.IdPersona <= 0)
            {
                return null;
            }

            return item.IdPersona;
        }

        private ViajeListadoDto ObtenerSeleccionado()
        {
            if (_grilla.CurrentRow == null)
            {
                return null;
            }

            return _grilla.CurrentRow
                .DataBoundItem
                as ViajeListadoDto;
        }

        private static void MostrarSeleccionRequerida()
        {
            MessageBox.Show(
                "Seleccione un viaje de la lista.",
                "Viajes",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void CambiarEstadoCarga(
            bool cargando)
        {
            _btnBuscar.Enabled =
                !cargando;

            _btnLimpiar.Enabled =
                !cargando;

            UseWaitCursor =
                cargando;
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
                    "No fue posible acceder a los viajes. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado en el módulo de viajes.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private sealed class EstadoFiltroItem
        {
            public EstadoFiltroItem(
                string texto,
                EstadoViaje? estado)
            {
                Texto = texto;
                Estado = estado;
            }

            public string Texto { get; private set; }

            public EstadoViaje? Estado { get; private set; }
        }

        private sealed class ParticipanteFiltroItem
        {
            public ParticipanteFiltroItem(
                int idPersona,
                string nombreCompleto)
            {
                IdPersona = idPersona;
                NombreCompleto =
                    nombreCompleto;
            }

            public int IdPersona { get; private set; }

            public string NombreCompleto { get; private set; }
        }
    }
}
