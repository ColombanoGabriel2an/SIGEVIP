using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viajes;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class VisitasForm : Form
    {
        private readonly VisitaService
            _visitaService;

        private readonly ViajeService
            _viajeService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private ComboBox _cmbViaje;
        private Button _btnBuscar;
        private Button _btnActualizar;
        private Button _btnNuevaVisita;
        private Button _btnCerrar;
        private DataGridView _grilla;
        private Label _lblCantidad;
        private Label _lblViajeSeleccionado;

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
                new Size(1050, 650);

            Size =
                new Size(1220, 740);

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
                        "Visitas comerciales"
                });

            Controls.Add(
                CrearPanelFiltros());

            _lblViajeSeleccionado =
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
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(24, 165),
                    Padding =
                        new Padding(10),
                    Size =
                        new Size(1155, 43),
                    Text =
                        "Seleccione un viaje para consultar sus visitas.",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            Controls.Add(
                _lblViajeSeleccionado);

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
                        new Point(25, 660),
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
                        new Size(1155, 82)
                };

            panel.Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(16, 15),
                    Text =
                        "Viaje"
                });

            _cmbViaje =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    DisplayMember =
                        "DescripcionCompleta",
                    Location =
                        new Point(19, 40),
                    Size =
                        new Size(760, 24)
                };

            _btnBuscar =
                CrearBotonFiltro(
                    "Consultar",
                    800);

            _btnActualizar =
                CrearBotonFiltro(
                    "Actualizar viajes",
                    930);

            _btnBuscar.Click +=
                BtnBuscar_Click;

            _btnActualizar.Click +=
                BtnActualizar_Click;

            panel.Controls.Add(
                _cmbViaje);

            panel.Controls.Add(
                _btnBuscar);

            panel.Controls.Add(
                _btnActualizar);

            return panel;
        }

        private static Button CrearBotonFiltro(
            string texto,
            int posicionX)
        {
            return new Button
            {
                Location =
                    new Point(posicionX, 34),
                Size =
                    new Size(145, 32),
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
                    AllowUserToAddRows =
                        false,
                    AllowUserToDeleteRows =
                        false,
                    AllowUserToResizeRows =
                        false,
                    AutoGenerateColumns =
                        false,
                    BackgroundColor =
                        Color.White,
                    Location =
                        new Point(24, 224),
                    MultiSelect =
                        false,
                    ReadOnly =
                        true,
                    RowHeadersVisible =
                        false,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    Size =
                        new Size(1155, 355)
                };

            AgregarColumna(
                grilla,
                "IdVisita",
                "Id",
                55);

            AgregarColumna(
                grilla,
                "Fecha",
                "Fecha",
                95,
                "dd/MM/yyyy");

            AgregarColumna(
                grilla,
                "LocalidadEncuentro",
                "Localidad",
                150);

            AgregarColumna(
                grilla,
                "Observacion",
                "Observación",
                360);

            AgregarColumna(
                grilla,
                "ClientesResumen",
                "Clientes",
                380);

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
                        new Point(24, 596),
                    Size =
                        new Size(1155, 50),
                    WrapContents =
                        false
                };

            _btnNuevaVisita =
                CrearBotonAccion(
                    "Nueva visita");

            _btnCerrar =
                CrearBotonAccion(
                    "Cerrar");

            _btnNuevaVisita.Click +=
                BtnNuevaVisita_Click;

            _btnCerrar.Click +=
                delegate
                {
                    Close();
                };

            panel.Controls.Add(
                _btnNuevaVisita);

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
                    new Size(145, 36),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private void ConfigurarPermisos()
        {
            bool puedeRegistrar =
                _sesionActual.HayUsuarioAutenticado
                && _autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        VisitaService.PermisoRegistrar);

            _btnNuevaVisita.Visible =
                puedeRegistrar;

            _btnNuevaVisita.Enabled =
                puedeRegistrar;
        }

        private void VisitasForm_Load(
            object sender,
            EventArgs e)
        {
            CargarViajes();
        }

        private void BtnBuscar_Click(
            object sender,
            EventArgs e)
        {
            CargarVisitas();
        }

        private void BtnActualizar_Click(
            object sender,
            EventArgs e)
        {
            CargarViajes();
        }

        private void BtnNuevaVisita_Click(
            object sender,
            EventArgs e)
        {
            ViajeItem viaje =
                ObtenerViajeSeleccionado();

            if (viaje == null)
            {
                MostrarViajeRequerido();
                return;
            }

            using (
                var formulario =
                    new VisitaEditForm(
                        _visitaService,
                        viaje.IdViaje,
                        viaje.FechaInicio,
                        viaje.FechaFin,
                        viaje.DescripcionCompleta))
            {
                if (formulario.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    CargarVisitas();
                }
            }
        }

        private void CargarViajes()
        {
            CambiarEstadoCarga(
                true);

            int? idAnterior =
                ObtenerViajeSeleccionado()
                    ?.IdViaje;

            try
            {
                List<ViajeListadoDto> viajes =
                    _viajeService
                        .Listar(
                            ViajeFiltro
                                .CrearSinFiltros())
                        .OrderByDescending(
                            viaje =>
                                viaje.FechaInicio)
                        .ThenByDescending(
                            viaje =>
                                viaje.IdViaje)
                        .ToList();

                _cmbViaje.DataSource =
                    null;

                _cmbViaje.DataSource =
                    viajes
                        .Select(
                            viaje =>
                                new ViajeItem(
                                    viaje))
                        .ToList();

                _cmbViaje.DisplayMember =
                    "DescripcionCompleta";

                if (_cmbViaje.Items.Count == 0)
                {
                    _lblViajeSeleccionado.Text =
                        "No existen viajes disponibles.";

                    LimpiarGrilla();
                    return;
                }

                SeleccionarViajeAnterior(
                    idAnterior);

                CargarVisitas();
            }
            catch (Exception exception)
            {
                _cmbViaje.DataSource =
                    null;

                _lblViajeSeleccionado.Text =
                    "No fue posible cargar los viajes.";

                LimpiarGrilla();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void SeleccionarViajeAnterior(
            int? idAnterior)
        {
            if (!idAnterior.HasValue)
            {
                _cmbViaje.SelectedIndex = 0;
                return;
            }

            for (
                int indice = 0;
                indice < _cmbViaje.Items.Count;
                indice++)
            {
                ViajeItem item =
                    _cmbViaje.Items[indice]
                        as ViajeItem;

                if (item != null &&
                    item.IdViaje ==
                        idAnterior.Value)
                {
                    _cmbViaje.SelectedIndex =
                        indice;

                    return;
                }
            }

            _cmbViaje.SelectedIndex = 0;
        }

        private void CargarVisitas()
        {
            ViajeItem viaje =
                ObtenerViajeSeleccionado();

            if (viaje == null)
            {
                _lblViajeSeleccionado.Text =
                    "Seleccione un viaje para consultar sus visitas.";

                LimpiarGrilla();
                return;
            }

            CambiarEstadoCarga(
                true);

            try
            {
                List<VisitaListadoDto> visitas =
                    _visitaService
                        .ListarPorViaje(
                            viaje.IdViaje)
                        .ToList();

                _grilla.DataSource =
                    null;

                _grilla.DataSource =
                    visitas;

                _lblCantidad.Text =
                    "Resultados: " +
                    visitas.Count;

                _lblViajeSeleccionado.Text =
                    viaje.DescripcionCompleta;
            }
            catch (Exception exception)
            {
                LimpiarGrilla();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private ViajeItem ObtenerViajeSeleccionado()
        {
            return _cmbViaje.SelectedItem
                as ViajeItem;
        }

        private void LimpiarGrilla()
        {
            _grilla.DataSource =
                null;

            _lblCantidad.Text =
                "Resultados: 0";
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
            _btnBuscar.Enabled =
                !cargando;

            _btnActualizar.Enabled =
                !cargando;

            _cmbViaje.Enabled =
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
                    "Visitas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible acceder a las visitas. " +
                    "Verifique la conexión con SQL Server " +
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

        private sealed class ViajeItem
        {
            public ViajeItem(
                ViajeListadoDto viaje)
            {
                if (viaje == null)
                {
                    throw new ArgumentNullException(
                        nameof(viaje));
                }

                IdViaje =
                    viaje.IdViaje;

                FechaInicio =
                    viaje.FechaInicio;

                FechaFin =
                    viaje.FechaFin;

                Estado =
                    viaje.Estado;

                DescripcionCompleta =
                    "#" +
                    viaje.IdViaje +
                    " - " +
                    viaje.FechaInicio
                        .ToString("dd/MM/yyyy") +
                    " al " +
                    viaje.FechaFin
                        .ToString("dd/MM/yyyy") +
                    " - " +
                    viaje.Descripcion +
                    " - " +
                    viaje.Estado;
            }

            public int IdViaje { get; private set; }

            public DateTime FechaInicio { get; private set; }

            public DateTime FechaFin { get; private set; }

            public EstadoViaje Estado { get; private set; }

            public string DescripcionCompleta
            {
                get;
                private set;
            }
        }
    }
}