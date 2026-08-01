using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.WinForms.Controls;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class RendicionesForm : Form
    {
        private readonly RendicionService
            _rendicionService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private DataGridView _grillaRendiciones;
        private DataGridView _grillaViaticos;
        private DataGridView _grillaVisitas;
        private DataGridView _grillaParticipantes;

        private Label _lblEncabezado;
        private Label _lblMontoAnticipado;
        private Label _lblTotalGastado;
        private Label _lblSaldo;
        private Label _lblEstado;

        private Button _btnActualizar;
        private Button _btnVerDetalle;
        private Button _btnExcluir;
        private Button _btnReactivar;
        private Button _btnAjustarAnticipo;
        private Button _btnAprobar;
        private Button _btnCancelarRendicion;
        private Button _btnCerrar;

        private RendicionDetalleDto
            _detalleActual;

        private bool _cargando;

        private bool _puedeExcluir;
        private bool _puedeReactivar;
        private bool _puedeAjustarAnticipo;
        private bool _puedeAprobar;
        private bool _puedeCancelar;

        public RendicionesForm(
            RendicionService rendicionService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _rendicionService =
                rendicionService
                ?? throw new ArgumentNullException(
                    nameof(rendicionService));

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
                RendicionesForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Rendiciones pendientes";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(
                    1180,
                    720);

            Size =
                new Size(
                    1400,
                    850);

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
                    Font =
                        new Font(
                            "Segoe UI",
                            18F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            24,
                            16),
                    Text =
                        "Rendiciones pendientes"
                });

            Controls.Add(
                new Label
                {
                    AutoSize = true,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            27,
                            51),
                    Text =
                        "Seleccione un viaje para revisar sus gastos, visitas, participantes y saldo."
                });

            _grillaRendiciones =
                CrearGrillaRendiciones();

            _grillaRendiciones.SelectionChanged +=
                GrillaRendiciones_SelectionChanged;

            Controls.Add(
                _grillaRendiciones);

            _lblEncabezado =
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
                            279),
                    Padding =
                        new Padding(10),
                    Size =
                        new Size(
                            1330,
                            48),
                    Text =
                        "Seleccione una rendición pendiente.",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            Controls.Add(
                _lblEncabezado);

            Controls.Add(
                CrearPanelResumen());

            Controls.Add(
                CrearPestanas());

            Controls.Add(
                CrearPanelAcciones());

            _lblEstado =
                new Label
                {
                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,
                    AutoEllipsis = true,
                    Location =
                        new Point(
                            25,
                            780),
                    Size =
                        new Size(
                            1328,
                            22),
                    Text =
                        "Pendientes: 0"
                };

            Controls.Add(
                _lblEstado);
        }

        private DataGridView CrearGrillaRendiciones()
        {
            var grilla =
                CrearGrillaBase();

            grilla.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            grilla.Location =
                new Point(
                    24,
                    78);

            grilla.Size =
                new Size(
                    1330,
                    190);

            AgregarColumna(
                grilla,
                "IdViaje",
                "Viaje",
                60);

            AgregarColumna(
                grilla,
                "FechaInicio",
                "Desde",
                85,
                "dd/MM/yyyy");

            AgregarColumna(
                grilla,
                "FechaFin",
                "Hasta",
                85,
                "dd/MM/yyyy");

            AgregarColumna(
                grilla,
                "TipoViaje",
                "Tipo",
                115);

            AgregarColumna(
                grilla,
                "Descripcion",
                "Descripción",
                285);

            AgregarColumna(
                grilla,
                "ParticipantesResumen",
                "Participantes",
                220);

            AgregarColumna(
                grilla,
                "MontoAnticipado",
                "Anticipo",
                105,
                "N2");

            AgregarColumna(
                grilla,
                "TotalGastado",
                "Total",
                105,
                "N2");

            AgregarColumna(
                grilla,
                "Saldo",
                "Saldo",
                105,
                "N2");

            AgregarColumna(
                grilla,
                "FechaEnvioRendicion",
                "Enviada",
                135,
                "dd/MM/yyyy HH:mm");

            return grilla;
        }

        private Control CrearPanelResumen()
        {
            var panel =
                new TableLayoutPanel
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    BackColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    ColumnCount = 3,
                    Location =
                        new Point(
                            24,
                            337),
                    RowCount = 1,
                    Size =
                        new Size(
                            1330,
                            60)
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

        private Control CrearPestanas()
        {
            var pestanas =
                new TabControl
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    Location =
                        new Point(
                            24,
                            407),
                    Size =
                        new Size(
                            1330,
                            300)
                };

            var paginaViaticos =
                new TabPage(
                    "Viáticos");

            var paginaVisitas =
                new TabPage(
                    "Visitas");

            var paginaParticipantes =
                new TabPage(
                    "Participantes");

            _grillaViaticos =
                CrearGrillaViaticos();

            _grillaViaticos.SelectionChanged +=
                GrillaViaticos_SelectionChanged;

            _grillaVisitas =
                CrearGrillaVisitas();

            _grillaParticipantes =
                CrearGrillaParticipantes();

            paginaViaticos.Controls.Add(
                _grillaViaticos);

            paginaVisitas.Controls.Add(
                _grillaVisitas);

            paginaParticipantes.Controls.Add(
                _grillaParticipantes);

            pestanas.TabPages.Add(
                paginaViaticos);

            pestanas.TabPages.Add(
                paginaVisitas);

            pestanas.TabPages.Add(
                paginaParticipantes);

            return pestanas;
        }

        private DataGridView CrearGrillaViaticos()
        {
            var grilla =
                CrearGrillaBase();

            grilla.Dock =
                DockStyle.Fill;

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
                115);

            AgregarColumna(
                grilla,
                "MetodoPago",
                "Método",
                145);

            AgregarColumna(
                grilla,
                "PagadoPor",
                "Pagado por",
                180);

            AgregarColumna(
                grilla,
                "Monto",
                "Monto",
                105,
                "N2");

            AgregarColumna(
                grilla,
                "Descripcion",
                "Descripción",
                260);

            AgregarColumna(
                grilla,
                "Estado",
                "Estado",
                90);

            AgregarColumnaCheckBox(
                grilla,
                "TieneComprobante",
                "Comprobante",
                100);

            AgregarColumna(
                grilla,
                "MotivoExclusion",
                "Motivo de exclusión",
                245);

            return grilla;
        }

        private DataGridView CrearGrillaVisitas()
        {
            var grilla =
                CrearGrillaBase();

            grilla.Dock =
                DockStyle.Fill;

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
                200);

            AgregarColumna(
                grilla,
                "ClientesResumen",
                "Clientes",
                420);

            AgregarColumna(
                grilla,
                "Observacion",
                "Observación",
                475);

            return grilla;
        }

        private DataGridView CrearGrillaParticipantes()
        {
            var grilla =
                CrearGrillaBase();

            grilla.Dock =
                DockStyle.Fill;

            AgregarColumna(
                grilla,
                "IdPersona",
                "Id",
                70);

            AgregarColumna(
                grilla,
                "NombreCompleto",
                "Nombre",
                350);

            AgregarColumna(
                grilla,
                "Email",
                "Correo electrónico",
                460);

            AgregarColumnaCheckBox(
                grilla,
                "Activo",
                "Activo",
                80);

            return grilla;
        }

        private Control CrearPanelAcciones()
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
                            720),
                    Size =
                        new Size(
                            1330,
                            45),
                    WrapContents = false
                };

            _btnVerDetalle =
                CrearBotonAccion(
                    "Ver viático",
                    125);

            _btnExcluir =
                CrearBotonAccion(
                    "Excluir viático",
                    135);

            _btnReactivar =
                CrearBotonAccion(
                    "Reactivar viático",
                    145);

            _btnAjustarAnticipo =
                CrearBotonAccion(
                    "Ajustar anticipo",
                    145);

            _btnAprobar =
                CrearBotonAccion(
                    "Aprobar rendición",
                    145);

            _btnCancelarRendicion =
                CrearBotonAccion(
                    "Cancelar rendición",
                    150);

            _btnActualizar =
                CrearBotonAccion(
                    "Actualizar",
                    115);

            _btnCerrar =
                CrearBotonAccion(
                    "Cerrar",
                    100);

            _btnVerDetalle.Click +=
                BtnVerDetalle_Click;

            _btnExcluir.Click +=
                BtnExcluir_Click;

            _btnReactivar.Click +=
                BtnReactivar_Click;

            _btnAjustarAnticipo.Click +=
                BtnAjustarAnticipo_Click;

            _btnAprobar.Click +=
                BtnAprobar_Click;

            _btnCancelarRendicion.Click +=
                BtnCancelarRendicion_Click;

            _btnActualizar.Click +=
                BtnActualizar_Click;

            _btnCerrar.Click +=
                delegate
                {
                    Close();
                };

            panel.Controls.Add(
                _btnVerDetalle);

            panel.Controls.Add(
                _btnExcluir);

            panel.Controls.Add(
                _btnReactivar);

            panel.Controls.Add(
                _btnAjustarAnticipo);

            panel.Controls.Add(
                _btnAprobar);

            panel.Controls.Add(
                _btnCancelarRendicion);

            panel.Controls.Add(
                _btnActualizar);

            panel.Controls.Add(
                _btnCerrar);

            return panel;
        }

        private static DataGridView
            CrearGrillaBase()
        {
            return new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoGenerateColumns = false,
                BackgroundColor =
                    Color.White,
                BorderStyle =
                    BorderStyle.Fixed3D,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode
                        .FullRowSelect
            };
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
                    SortMode =
                        DataGridViewColumnSortMode.Automatic,
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

        private static void AgregarColumnaCheckBox(
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
                        DataGridViewColumnSortMode.Automatic,
                    Width =
                        ancho
                });
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

        private static Button CrearBotonAccion(
            string texto,
            int ancho)
        {
            return new Button
            {
                Margin =
                    new Padding(
                        0,
                        0,
                        8,
                        0),
                Size =
                    new Size(
                        ancho,
                        36),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private void ConfigurarPermisos()
        {
            _puedeExcluir =
                TienePermiso(
                    RendicionService
                        .PermisoExcluirViatico);

            _puedeReactivar =
                TienePermiso(
                    RendicionService
                        .PermisoReactivarViatico);

            _puedeAjustarAnticipo =
                TienePermiso(
                    RendicionService
                        .PermisoAjustarAnticipo);

            _puedeAprobar =
                TienePermiso(
                    RendicionService
                        .PermisoAprobar);

            _puedeCancelar =
                TienePermiso(
                    RendicionService
                        .PermisoCancelar);

            _btnExcluir.Visible =
                _puedeExcluir;

            _btnReactivar.Visible =
                _puedeReactivar;

            _btnAjustarAnticipo.Visible =
                _puedeAjustarAnticipo;

            _btnAprobar.Visible =
                _puedeAprobar;

            _btnCancelarRendicion.Visible =
                _puedeCancelar;

            ActualizarDisponibilidadAcciones();
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

        private void RendicionesForm_Load(
            object sender,
            EventArgs e)
        {
            CargarPendientes(
                null);
        }

        private void GrillaRendiciones_SelectionChanged(
            object sender,
            EventArgs e)
        {
            if (_cargando)
            {
                return;
            }

            RendicionListadoDto seleccionada =
                ObtenerRendicionSeleccionada();

            if (seleccionada == null)
            {
                LimpiarDetalle();
                return;
            }

            CargarDetalle(
                seleccionada.IdViaje);
        }

        private void GrillaViaticos_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ActualizarDisponibilidadAcciones();
        }

        private void BtnActualizar_Click(
            object sender,
            EventArgs e)
        {
            int? idViaje =
                _detalleActual == null
                    ? (int?)null
                    : _detalleActual.IdViaje;

            CargarPendientes(
                idViaje);
        }

        private void CargarPendientes(
            int? idViajePreferido)
        {
            CambiarEstadoCarga(
                true);

            try
            {
                List<RendicionListadoDto> pendientes =
                    _rendicionService
                        .ListarPendientes()
                        .OrderBy(
                            item =>
                                item.FechaEnvioRendicion
                                ?? DateTime.MaxValue)
                        .ThenBy(
                            item =>
                                item.IdViaje)
                        .ToList();

                _grillaRendiciones.DataSource =
                    null;

                _grillaRendiciones.DataSource =
                    CrearOrigenOrdenable(
                        pendientes);

                if (pendientes.Count == 0)
                {
                    LimpiarDetalle();

                    _lblEstado.Text =
                        "No existen rendiciones pendientes.";

                    return;
                }

                SeleccionarRendicion(
                    idViajePreferido);

                RendicionListadoDto seleccionada =
                    ObtenerRendicionSeleccionada();

                if (seleccionada != null)
                {
                    RendicionDetalleDto detalle =
                        _rendicionService
                            .ObtenerDetalle(
                                seleccionada.IdViaje);

                    MostrarDetalle(
                        detalle);
                }

                _lblEstado.Text =
                    "Rendiciones pendientes: " +
                    pendientes.Count +
                    ".";
            }
            catch (Exception exception)
            {
                LimpiarDetalle();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void SeleccionarRendicion(
            int? idViajePreferido)
        {
            if (_grillaRendiciones.Rows.Count == 0)
            {
                return;
            }

            DataGridViewRow filaSeleccionada =
                null;

            if (idViajePreferido.HasValue)
            {
                foreach (
                    DataGridViewRow fila
                    in _grillaRendiciones.Rows)
                {
                    RendicionListadoDto item =
                        fila.DataBoundItem
                            as RendicionListadoDto;

                    if (item != null &&
                        item.IdViaje ==
                            idViajePreferido.Value)
                    {
                        filaSeleccionada =
                            fila;

                        break;
                    }
                }
            }

            if (filaSeleccionada == null)
            {
                filaSeleccionada =
                    _grillaRendiciones.Rows[0];
            }

            filaSeleccionada.Selected =
                true;

            _grillaRendiciones.CurrentCell =
                filaSeleccionada.Cells[0];
        }

        private void CargarDetalle(
            int idViaje)
        {
            CambiarEstadoCarga(
                true);

            try
            {
                RendicionDetalleDto detalle =
                    _rendicionService
                        .ObtenerDetalle(
                            idViaje);

                MostrarDetalle(
                    detalle);
            }
            catch (Exception exception)
            {
                LimpiarDetalle();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void MostrarDetalle(
            RendicionDetalleDto detalle)
        {
            _detalleActual =
                detalle;

            _lblEncabezado.Text =
                "Viaje " +
                detalle.IdViaje +
                " | " +
                detalle.FechaInicio
                    .ToString("dd/MM/yyyy") +
                " al " +
                detalle.FechaFin
                    .ToString("dd/MM/yyyy") +
                " | " +
                detalle.TipoViaje +
                " | " +
                detalle.Descripcion +
                " | Enviada: " +
                FormatearFecha(
                    detalle.FechaEnvioRendicion);

            _lblMontoAnticipado.Text =
                "Monto anticipado: " +
                detalle.MontoAnticipado
                    .ToString("N2");

            _lblTotalGastado.Text =
                "Total gastado vigente: " +
                detalle.TotalGastado
                    .ToString("N2");

            _lblSaldo.Text =
                "Saldo: " +
                detalle.Saldo
                    .ToString("N2");

            _grillaViaticos.DataSource =
                null;

            _grillaViaticos.DataSource =
                CrearOrigenOrdenable(
                    detalle.Viaticos
                        .OrderBy(
                            item =>
                                item.Fecha)
                        .ThenBy(
                            item =>
                                item.IdViatico)
                        .ToList());

            _grillaVisitas.DataSource =
                null;

            _grillaVisitas.DataSource =
                CrearOrigenOrdenable(
                    detalle.Visitas
                        .OrderBy(
                            item =>
                                item.Fecha)
                        .ThenBy(
                            item =>
                                item.IdVisita)
                        .Select(
                            item =>
                                new VisitaFila(
                                    item))
                        .ToList());

            _grillaParticipantes.DataSource =
                null;

            _grillaParticipantes.DataSource =
                CrearOrigenOrdenable(
                    detalle.Participantes
                        .OrderBy(
                            item =>
                                item.NombreCompleto)
                        .ThenBy(
                            item =>
                                item.IdPersona)
                        .ToList());

            _lblEstado.Text =
                "Viáticos: " +
                detalle.Viaticos.Count +
                " | Visitas: " +
                detalle.Visitas.Count +
                " | Participantes: " +
                detalle.Participantes.Count +
                ".";

            ActualizarDisponibilidadAcciones();
        }

        private void LimpiarDetalle()
        {
            _detalleActual =
                null;

            _lblEncabezado.Text =
                "Seleccione una rendición pendiente.";

            _lblMontoAnticipado.Text =
                "Monto anticipado: 0,00";

            _lblTotalGastado.Text =
                "Total gastado vigente: 0,00";

            _lblSaldo.Text =
                "Saldo: 0,00";

            _grillaViaticos.DataSource =
                null;

            _grillaVisitas.DataSource =
                null;

            _grillaParticipantes.DataSource =
                null;

            ActualizarDisponibilidadAcciones();
        }

        private RendicionListadoDto
            ObtenerRendicionSeleccionada()
        {
            if (_grillaRendiciones.CurrentRow == null)
            {
                return null;
            }

            return _grillaRendiciones
                .CurrentRow
                .DataBoundItem
                as RendicionListadoDto;
        }

        private RendicionViaticoDto
            ObtenerViaticoSeleccionado()
        {
            if (_grillaViaticos.CurrentRow == null)
            {
                return null;
            }

            return _grillaViaticos
                .CurrentRow
                .DataBoundItem
                as RendicionViaticoDto;
        }

        private void BtnVerDetalle_Click(
            object sender,
            EventArgs e)
        {
            RendicionViaticoDto viatico =
                ObtenerViaticoSeleccionado();

            if (viatico == null)
            {
                MessageBox.Show(
                    "Seleccione un viático.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(
                CrearDetalleViatico(
                    viatico),
                "Detalle del viático",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnExcluir_Click(
            object sender,
            EventArgs e)
        {
            RendicionViaticoDto viatico =
                ObtenerViaticoSeleccionado();

            if (_detalleActual == null ||
                viatico == null)
            {
                MessageBox.Show(
                    "Seleccione un viático vigente.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (viatico.Estado !=
                EstadoViatico.Vigente)
            {
                MessageBox.Show(
                    "Solo pueden excluirse viáticos vigentes.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (
                var dialogo =
                    new TextoObligatorioDialog(
                        "Excluir viático",
                        "Indique el motivo de exclusión:",
                        500))
            {
                if (dialogo.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                EjecutarOperacion(
                    delegate
                    {
                        _rendicionService
                            .ExcluirViatico(
                                new ExcluirViaticoCommand(
                                    _detalleActual.IdViaje,
                                    viatico.IdViatico,
                                    dialogo.Valor));
                    },
                    "El viático fue excluido correctamente.",
                    _detalleActual.IdViaje);
            }
        }

        private void BtnReactivar_Click(
            object sender,
            EventArgs e)
        {
            RendicionViaticoDto viatico =
                ObtenerViaticoSeleccionado();

            if (_detalleActual == null ||
                viatico == null)
            {
                MessageBox.Show(
                    "Seleccione un viático excluido.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (viatico.Estado !=
                EstadoViatico.Excluido)
            {
                MessageBox.Show(
                    "Solo pueden reactivarse viáticos excluidos.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult confirmacion =
                MessageBox.Show(
                    "¿Desea reactivar el viático seleccionado?" +
                    Environment.NewLine +
                    Environment.NewLine +
                    "El importe volverá a integrar el total vigente.",
                    "Confirmar reactivación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (confirmacion !=
                DialogResult.Yes)
            {
                return;
            }

            EjecutarOperacion(
                delegate
                {
                    _rendicionService
                        .ReactivarViatico(
                            new ReactivarViaticoCommand(
                                _detalleActual.IdViaje,
                                viatico.IdViatico));
                },
                "El viático fue reactivado correctamente.",
                _detalleActual.IdViaje);
        }

        private void BtnAjustarAnticipo_Click(
            object sender,
            EventArgs e)
        {
            if (_detalleActual == null)
            {
                MessageBox.Show(
                    "Seleccione una rendición.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (
                var dialogo =
                    new MontoAnticipadoDialog(
                        _detalleActual
                            .MontoAnticipado))
            {
                if (dialogo.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                EjecutarOperacion(
                    delegate
                    {
                        _rendicionService
                            .AjustarAnticipo(
                                new AjustarAnticipoCommand(
                                    _detalleActual.IdViaje,
                                    dialogo.Monto));
                    },
                    "El monto anticipado fue ajustado correctamente.",
                    _detalleActual.IdViaje);
            }
        }

        private void BtnAprobar_Click(
            object sender,
            EventArgs e)
        {
            if (_detalleActual == null)
            {
                MessageBox.Show(
                    "Seleccione una rendición.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult confirmacion =
                MessageBox.Show(
                    "¿Desea aprobar definitivamente la rendición del viaje " +
                    _detalleActual.IdViaje +
                    "?" +
                    Environment.NewLine +
                    Environment.NewLine +
                    "Total vigente: " +
                    _detalleActual.TotalGastado
                        .ToString("N2") +
                    Environment.NewLine +
                    "Anticipo: " +
                    _detalleActual.MontoAnticipado
                        .ToString("N2") +
                    Environment.NewLine +
                    "Saldo: " +
                    _detalleActual.Saldo
                        .ToString("N2"),
                    "Confirmar aprobación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (confirmacion !=
                DialogResult.Yes)
            {
                return;
            }

            int idViaje =
                _detalleActual.IdViaje;

            EjecutarOperacion(
                delegate
                {
                    _rendicionService
                        .Aprobar(
                            new AprobarRendicionCommand(
                                idViaje));
                },
                "La rendición fue aprobada correctamente.",
                null);
        }

        private void BtnCancelarRendicion_Click(
            object sender,
            EventArgs e)
        {
            if (_detalleActual == null)
            {
                MessageBox.Show(
                    "Seleccione una rendición.",
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            using (
                var dialogo =
                    new TextoObligatorioDialog(
                        "Cancelar rendición",
                        "Indique el motivo de cancelación:",
                        1000))
            {
                if (dialogo.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                DialogResult confirmacion =
                    MessageBox.Show(
                        "¿Confirma la cancelación definitiva de la rendición del viaje " +
                        _detalleActual.IdViaje +
                        "?" +
                        Environment.NewLine +
                        Environment.NewLine +
                        "Motivo: " +
                        dialogo.Valor,
                        "Confirmar cancelación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);

                if (confirmacion !=
                    DialogResult.Yes)
                {
                    return;
                }

                int idViaje =
                    _detalleActual.IdViaje;

                EjecutarOperacion(
                    delegate
                    {
                        _rendicionService
                            .Cancelar(
                                new CancelarRendicionCommand(
                                    idViaje,
                                    dialogo.Valor));
                    },
                    "La rendición fue cancelada correctamente.",
                    null);
            }
        }

        private void EjecutarOperacion(
            Action operacion,
            string mensajeExito,
            int? idViajePreferido)
        {
            CambiarEstadoCarga(
                true);

            try
            {
                operacion();

                MessageBox.Show(
                    mensajeExito,
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarPendientes(
                    idViajePreferido);
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
            _cargando =
                cargando;

            _grillaRendiciones.Enabled =
                !cargando;

            _grillaViaticos.Enabled =
                !cargando;

            _btnActualizar.Enabled =
                !cargando;

            _btnCerrar.Enabled =
                !cargando;

            UseWaitCursor =
                cargando;

            ActualizarDisponibilidadAcciones();
        }

        private void ActualizarDisponibilidadAcciones()
        {
            bool existeDetalle =
                !_cargando &&
                _detalleActual != null;

            RendicionViaticoDto viatico =
                ObtenerViaticoSeleccionado();

            _btnVerDetalle.Enabled =
                existeDetalle &&
                viatico != null;

            _btnExcluir.Enabled =
                existeDetalle &&
                _puedeExcluir &&
                viatico != null &&
                viatico.Estado ==
                    EstadoViatico.Vigente;

            _btnReactivar.Enabled =
                existeDetalle &&
                _puedeReactivar &&
                viatico != null &&
                viatico.Estado ==
                    EstadoViatico.Excluido;

            _btnAjustarAnticipo.Enabled =
                existeDetalle &&
                _puedeAjustarAnticipo;

            _btnAprobar.Enabled =
                existeDetalle &&
                _puedeAprobar;

            _btnCancelarRendicion.Enabled =
                existeDetalle &&
                _puedeCancelar;
        }

        private static string CrearDetalleViatico(
            RendicionViaticoDto viatico)
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
                    string.IsNullOrWhiteSpace(
                        viatico.PagadoPor)
                        ? "No corresponde"
                        : viatico.PagadoPor
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
                    viatico.TieneComprobante
                        ? "Sí"
                        : "No"
                ));

            if (viatico.Comprobante != null)
            {
                detalle.AppendLine();
                detalle.AppendLine(
                    "DATOS DEL COMPROBANTE");

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
                    "Situación fiscal: " +
                    viatico.Comprobante
                        .SituacionFiscal);

                detalle.AppendLine(
                    "Número: " +
                    viatico.Comprobante
                        .Sucursal +
                    "-" +
                    viatico.Comprobante
                        .Numero);

                detalle.AppendLine(
                    "Monto gravado: " +
                    viatico.Comprobante
                        .MontoGravado
                        .ToString("N2"));

                detalle.AppendLine(
                    "Impuestos: " +
                    viatico.Comprobante
                        .MontoImpuestos
                        .ToString("N2"));

                detalle.AppendLine(
                    "Total: " +
                    viatico.Comprobante
                        .Total
                        .ToString("N2"));
            }

            if (viatico.Estado ==
                EstadoViatico.Excluido)
            {
                detalle.AppendLine();
                detalle.AppendLine(
                    "AUDITORÍA DE EXCLUSIÓN");

                detalle.AppendLine(
                    "Motivo: " +
                    viatico.MotivoExclusion);

                detalle.AppendLine(
                    "Usuario: " +
                    FormatearEntero(
                        viatico.IdUsuarioExclusion));

                detalle.AppendLine(
                    "Fecha: " +
                    FormatearFecha(
                        viatico.FechaExclusion));
            }

            if (viatico.FechaReactivacion.HasValue)
            {
                detalle.AppendLine();
                detalle.AppendLine(
                    "ÚLTIMA REACTIVACIÓN");

                detalle.AppendLine(
                    "Usuario: " +
                    FormatearEntero(
                        viatico.IdUsuarioReactivacion));

                detalle.AppendLine(
                    "Fecha: " +
                    FormatearFecha(
                        viatico.FechaReactivacion));
            }

            return detalle.ToString();
        }

        private static string FormatearFecha(
            DateTime? fecha)
        {
            return fecha.HasValue
                ? fecha.Value
                    .ToString("dd/MM/yyyy HH:mm")
                : "No informada";
        }

        private static string FormatearEntero(
            int? valor)
        {
            return valor.HasValue
                ? valor.Value.ToString()
                : "No informado";
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
                    "Rendiciones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is
                PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible acceder a los datos de rendiciones. " +
                    "Verifique la conexión con SQL Server e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado en el módulo de rendiciones.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private sealed class VisitaFila
        {
            public VisitaFila(
                RendicionVisitaDto visita)
            {
                IdVisita =
                    visita.IdVisita;

                Fecha =
                    visita.Fecha;

                LocalidadEncuentro =
                    visita.LocalidadEncuentro;

                Observacion =
                    visita.Observacion;

                ClientesResumen =
                    string.Join(
                        ", ",
                        visita.Clientes
                            .Select(
                                cliente =>
                                    cliente.RazonSocial)
                            .ToArray());
            }

            public int IdVisita
            {
                get;
                private set;
            }

            public DateTime Fecha
            {
                get;
                private set;
            }

            public string LocalidadEncuentro
            {
                get;
                private set;
            }

            public string ClientesResumen
            {
                get;
                private set;
            }

            public string Observacion
            {
                get;
                private set;
            }
        }

        private sealed class TextoObligatorioDialog
            : Form
        {
            private readonly TextBox
                _txtValor;

            private readonly Button
                _btnAceptar;

            public TextoObligatorioDialog(
                string titulo,
                string etiqueta,
                int longitudMaxima)
            {
                Text =
                    titulo;

                StartPosition =
                    FormStartPosition.CenterParent;

                FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                MaximizeBox = false;
                MinimizeBox = false;

                ClientSize =
                    new Size(
                        520,
                        245);

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
                        Font =
                            new Font(
                                "Segoe UI",
                                10F,
                                FontStyle.Bold),
                        Location =
                            new Point(
                                20,
                                20),
                        Text =
                            etiqueta
                    });

                _txtValor =
                    new TextBox
                    {
                        AcceptsReturn = true,
                        Location =
                            new Point(
                                20,
                                52),
                        MaxLength =
                            longitudMaxima,
                        Multiline = true,
                        ScrollBars =
                            ScrollBars.Vertical,
                        Size =
                            new Size(
                                480,
                                120)
                    };

                _btnAceptar =
                    new Button
                    {
                        Location =
                            new Point(
                                260,
                                190),
                        Size =
                            new Size(
                                115,
                                34),
                        Text =
                            "Aceptar",
                        UseVisualStyleBackColor =
                            true
                    };

                var btnCancelar =
                    new Button
                    {
                        DialogResult =
                            DialogResult.Cancel,
                        Location =
                            new Point(
                                385,
                                190),
                        Size =
                            new Size(
                                115,
                                34),
                        Text =
                            "Cancelar",
                        UseVisualStyleBackColor =
                            true
                    };

                _btnAceptar.Click +=
                    BtnAceptar_Click;

                Controls.Add(
                    _txtValor);

                Controls.Add(
                    _btnAceptar);

                Controls.Add(
                    btnCancelar);

                AcceptButton =
                    _btnAceptar;

                CancelButton =
                    btnCancelar;
            }

            public string Valor
            {
                get
                {
                    return _txtValor
                        .Text
                        .Trim();
                }
            }

            private void BtnAceptar_Click(
                object sender,
                EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(
                    _txtValor.Text))
                {
                    MessageBox.Show(
                        "Debe ingresar un motivo.",
                        "Validación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    _txtValor.Focus();

                    return;
                }

                DialogResult =
                    DialogResult.OK;

                Close();
            }
        }

        private sealed class MontoAnticipadoDialog
            : Form
        {
            private readonly NumericUpDown
                _nudMonto;

            public MontoAnticipadoDialog(
                decimal montoActual)
            {
                Text =
                    "Ajustar anticipo";

                StartPosition =
                    FormStartPosition.CenterParent;

                FormBorderStyle =
                    FormBorderStyle.FixedDialog;

                MaximizeBox = false;
                MinimizeBox = false;

                ClientSize =
                    new Size(
                        430,
                        180);

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
                        Font =
                            new Font(
                                "Segoe UI",
                                10F,
                                FontStyle.Bold),
                        Location =
                            new Point(
                                20,
                                20),
                        Text =
                            "Nuevo monto anticipado"
                    });

                _nudMonto =
                    new NumericUpDown
                    {
                        DecimalPlaces = 2,
                        Increment = 100m,
                        Location =
                            new Point(
                                20,
                                53),
                        Maximum =
                            9999999999999999m,
                        Minimum = 0m,
                        Size =
                            new Size(
                                390,
                                25),
                        ThousandsSeparator =
                            true,
                        Value =
                            LimitarMonto(
                                montoActual)
                    };

                var btnAceptar =
                    new Button
                    {
                        DialogResult =
                            DialogResult.OK,
                        Location =
                            new Point(
                                170,
                                115),
                        Size =
                            new Size(
                                115,
                                34),
                        Text =
                            "Aceptar",
                        UseVisualStyleBackColor =
                            true
                    };

                var btnCancelar =
                    new Button
                    {
                        DialogResult =
                            DialogResult.Cancel,
                        Location =
                            new Point(
                                295,
                                115),
                        Size =
                            new Size(
                                115,
                                34),
                        Text =
                            "Cancelar",
                        UseVisualStyleBackColor =
                            true
                    };

                Controls.Add(
                    _nudMonto);

                Controls.Add(
                    btnAceptar);

                Controls.Add(
                    btnCancelar);

                AcceptButton =
                    btnAceptar;

                CancelButton =
                    btnCancelar;
            }

            public decimal Monto
            {
                get
                {
                    return _nudMonto.Value;
                }
            }

            private static decimal LimitarMonto(
                decimal monto)
            {
                if (monto < 0m)
                {
                    return 0m;
                }

                if (monto >
                    9999999999999999m)
                {
                    return 9999999999999999m;
                }

                return monto;
            }
        }
    }
}
