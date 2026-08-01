using System;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Rendiciones;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class MainForm : Form
    {
        private const string ClienteConsultar =
            "CLIENTE_CONSULTAR";

        private const string ClienteGestionar =
            "CLIENTE_GESTIONAR";

        private const string ViajeCrear =
            "VIAJE_CREAR";

        private const string ViajeConsultar =
            "VIAJE_CONSULTAR";

        private const string VisitaRegistrar =
            "VISITA_REGISTRAR";

        private const string UsuarioGestionar =
            "USUARIO_GESTIONAR";

        private const string GrupoGestionar =
            "GRUPO_GESTIONAR";

        private const string PermisoGestionar =
            "PERMISO_GESTIONAR";

        private const string AuditoriaConsultar =
            "AUDITORIA_CONSULTAR";

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private readonly PerfilSesion
            _perfilSesion;

        private Label _lblUsuario;
        private Label _lblEstado;

        private Button _btnClientes;
        private Button _btnViajes;
        private Button _btnVisitas;
        private Button _btnViaticos;
        private Button _btnUsuarios;
        private Button _btnGrupos;
        private Button _btnPermisos;
        private Button _btnAuditoria;
        private Button _btnCambiarClave;
        private Button _btnCerrarSesion;
        private Button _btnSalir;

        private bool _cerrandoControladamente;

        public MainForm(
            ISesionActual sesionActual,
            AutorizacionService autorizacionService,
            PerfilSesion perfilSesion)
        {
            _sesionActual =
                sesionActual
                ?? throw new ArgumentNullException(
                    nameof(sesionActual));

            _autorizacionService =
                autorizacionService
                ?? throw new ArgumentNullException(
                    nameof(autorizacionService));

            _perfilSesion =
                perfilSesion
                ?? throw new ArgumentNullException(
                    nameof(perfilSesion));

            if (!_sesionActual.HayUsuarioAutenticado)
            {
                throw new InvalidOperationException(
                    "No existe una sesion autenticada.");
            }

            InicializarFormulario();
            ConfigurarUsuario();
            ConfigurarPermisos();
        }

        public event EventHandler
            ClientesSolicitados;

        public event EventHandler
            ViajesSolicitados;

        public event EventHandler
            VisitasSolicitadas;

        public event EventHandler
            ViaticosSolicitados;

        public event EventHandler
            UsuariosSolicitados;

        public event EventHandler
            GruposSolicitados;

        public event EventHandler
            PermisosSolicitados;

        public event EventHandler
            AuditoriaSolicitada;

        public event EventHandler
            CambiarClaveSolicitada;

        public event EventHandler
            CerrarSesionSolicitada;

        public event EventHandler
            SalirSolicitado;

        private Usuario UsuarioActual
        {
            get
            {
                return _sesionActual.UsuarioActual;
            }
        }

        private void InicializarFormulario()
        {
            Text = "SIGEVIP - Menu principal";
            StartPosition =
                FormStartPosition.CenterScreen;

            MinimumSize =
                new Size(920, 620);

            Size =
                new Size(1080, 720);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            var panelEncabezado =
                CrearPanelEncabezado();

            var lblModulos =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        14F,
                        FontStyle.Bold),
                    Location =
                        new Point(31, 113),
                    Text =
                        "Modulos disponibles"
                };

            var panelModulos =
                new FlowLayoutPanel
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    AutoScroll = true,
                    BackColor =
                        Color.Transparent,
                    FlowDirection =
                        FlowDirection.LeftToRight,
                    Location =
                        new Point(23, 150),
                    Padding =
                        new Padding(0),
                    Size =
                        new Size(1018, 410),
                    WrapContents = true
                };

            CrearBotonesModulos();

            panelModulos.Controls.Add(
                _btnClientes);

            panelModulos.Controls.Add(
                _btnViajes);

            panelModulos.Controls.Add(
                _btnVisitas);

            panelModulos.Controls.Add(
                _btnViaticos);

            panelModulos.Controls.Add(
                _btnUsuarios);

            panelModulos.Controls.Add(
                _btnGrupos);

            panelModulos.Controls.Add(
                _btnPermisos);

            panelModulos.Controls.Add(
                _btnAuditoria);

            _lblEstado =
                new Label
                {
                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Location =
                        new Point(31, 585),
                    Padding =
                        new Padding(12),
                    Size =
                        new Size(994, 70),
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            Controls.Add(_lblEstado);
            Controls.Add(panelModulos);
            Controls.Add(lblModulos);
            Controls.Add(panelEncabezado);

            FormClosing +=
                MainForm_FormClosing;
        }

        private Panel CrearPanelEncabezado()
        {
            var panelEncabezado =
                new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 88,
                    BackColor = Color.White
                };

            var lblTitulo =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        20F,
                        FontStyle.Bold),
                    Location =
                        new Point(28, 16),
                    Text = "SIGEVIP"
                };

            var lblSubtitulo =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        10F),
                    Location =
                        new Point(31, 56),
                    Text =
                        "Sistema de Gestion de Viajes y Viaticos"
                };

            var panelAcciones =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Right,
                    Width = 540,
                    ColumnCount = 3,
                    RowCount = 2,
                    Padding =
                        new Padding(
                            10,
                            10,
                            20,
                            8),
                    BackColor = Color.White
                };

            panelAcciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    40F));

            panelAcciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    30F));

            panelAcciones.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    30F));

            panelAcciones.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            panelAcciones.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            _lblUsuario =
                new Label
                {
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    Font = new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold),
                    TextAlign =
                        ContentAlignment.MiddleRight
                };

            _btnCambiarClave =
                new Button
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(4),
                    Text =
                        "Cambiar clave",
                    UseVisualStyleBackColor =
                        true
                };

            _btnCerrarSesion =
                new Button
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(4),
                    Text =
                        "Cerrar sesion",
                    UseVisualStyleBackColor =
                        true
                };

            _btnSalir =
                new Button
                {
                    Dock = DockStyle.Fill,
                    Margin =
                        new Padding(4),
                    Text =
                        "Salir",
                    UseVisualStyleBackColor =
                        true
                };

            _btnCambiarClave.Click +=
                BtnCambiarClave_Click;

            _btnCerrarSesion.Click +=
                BtnCerrarSesion_Click;

            _btnSalir.Click +=
                BtnSalir_Click;

            panelAcciones.Controls.Add(
                _lblUsuario,
                0,
                0);

            panelAcciones.SetColumnSpan(
                _lblUsuario,
                3);

            panelAcciones.Controls.Add(
                _btnCambiarClave,
                0,
                1);

            panelAcciones.Controls.Add(
                _btnCerrarSesion,
                1,
                1);

            panelAcciones.Controls.Add(
                _btnSalir,
                2,
                1);

            panelEncabezado.Controls.Add(
                panelAcciones);

            panelEncabezado.Controls.Add(
                lblTitulo);

            panelEncabezado.Controls.Add(
                lblSubtitulo);

            return panelEncabezado;
        }

        private void CrearBotonesModulos()
        {
            _btnClientes =
                CrearBotonModulo(
                    "Clientes",
                    "Consulta y gestion de clientes.");

            _btnViajes =
                CrearBotonModulo(
                    "Viajes",
                    "Alta, consulta y estados de viajes.");

            _btnVisitas =
                CrearBotonModulo(
                    "Visitas",
                    "Registro de visitas comerciales.");

            _btnViaticos =
                CrearBotonModulo(
                    "Viaticos y rendiciones",
                    "Carga y revision de gastos.");

            _btnUsuarios =
                CrearBotonModulo(
                    "Usuarios",
                    "Gestion de usuarios y asignacion de grupos.");

            _btnGrupos =
                CrearBotonModulo(
                    "Grupos",
                    "Gestion de grupos de acceso.");

            _btnPermisos =
                CrearBotonModulo(
                    "Permisos",
                    "Gestion de permisos del sistema.");

            _btnAuditoria =
                CrearBotonModulo(
                    "Auditoria",
                    "Consulta de registros de auditoria.");

            _btnClientes.Click +=
                BtnClientes_Click;

            _btnViajes.Click +=
                BtnViajes_Click;

            _btnVisitas.Click +=
                BtnVisitas_Click;

            _btnViaticos.Click +=
                BtnViaticos_Click;

            _btnUsuarios.Click +=
                BtnUsuarios_Click;

            _btnGrupos.Click +=
                BtnGrupos_Click;

            _btnPermisos.Click +=
                BtnPermisos_Click;

            _btnAuditoria.Click +=
                BtnAuditoria_Click;
        }

        private static Button CrearBotonModulo(
            string titulo,
            string descripcion)
        {
            return new Button
            {
                Margin =
                    new Padding(8),
                Size =
                    new Size(310, 135),
                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold),
                Text =
                    titulo +
                    Environment.NewLine +
                    Environment.NewLine +
                    descripcion,
                TextAlign =
                    ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor =
                    true
            };
        }

        private void ConfigurarUsuario()
        {
            _lblUsuario.Text =
                _perfilSesion.NombreCompleto +
                Environment.NewLine +
                "Usuario: " +
                UsuarioActual.NombreUsuario;
        }

        private void ConfigurarPermisos()
        {
            bool puedeClientes =
                TieneAlgunPermiso(
                    ClienteConsultar,
                    ClienteGestionar);

            bool puedeViajes =
                TieneAlgunPermiso(
                    ViajeCrear,
                    ViajeConsultar,
                    RendicionService.PermisoEnviar,
                    RendicionService.PermisoAprobar,
                    RendicionService.PermisoCancelar);

            bool puedeVisitas =
                TieneAlgunPermiso(
                    VisitaRegistrar);

            bool puedeViaticos =
                TieneAlgunPermiso(
                    ViaticoService.PermisoConsultar,
                    ViaticoService.PermisoRegistrar,
                    ViaticoService.PermisoModificar,
                    RendicionService.PermisoEnviar,
                    RendicionService.PermisoRevisar,
                    RendicionService.PermisoExcluirViatico,
                    RendicionService.PermisoReactivarViatico,
                    RendicionService.PermisoAjustarAnticipo,
                    RendicionService.PermisoAprobar,
                    RendicionService.PermisoCancelar);

            bool puedeUsuarios =
                TieneAlgunPermiso(
                    UsuarioGestionar);

            bool puedeGrupos =
                TieneAlgunPermiso(
                    GrupoGestionar);

            bool puedePermisos =
                TieneAlgunPermiso(
                    PermisoGestionar);

            bool puedeAuditoria =
                TieneAlgunPermiso(
                    AuditoriaConsultar);

            ConfigurarVisibilidad(
                _btnClientes,
                puedeClientes);

            ConfigurarVisibilidad(
                _btnViajes,
                puedeViajes);

            ConfigurarVisibilidad(
                _btnVisitas,
                puedeVisitas);

            ConfigurarVisibilidad(
                _btnViaticos,
                puedeViaticos);

            ConfigurarVisibilidad(
                _btnUsuarios,
                puedeUsuarios);

            ConfigurarVisibilidad(
                _btnGrupos,
                puedeGrupos);

            ConfigurarVisibilidad(
                _btnPermisos,
                puedePermisos);

            ConfigurarVisibilidad(
                _btnAuditoria,
                puedeAuditoria);

            int cantidadDisponibles =
                ContarOpcionesDisponibles(
                    puedeClientes,
                    puedeViajes,
                    puedeVisitas,
                    puedeViaticos,
                    puedeUsuarios,
                    puedeGrupos,
                    puedePermisos,
                    puedeAuditoria);

            _lblEstado.Text =
                "Sesion iniciada correctamente. " +
                "Opciones visibles segun permisos: " +
                cantidadDisponibles +
                "." +
                Environment.NewLine +
                "Las opciones no autorizadas permanecen ocultas.";
        }

        private bool TieneAlgunPermiso(
            params string[] codigos)
        {
            foreach (string codigo in codigos)
            {
                if (_autorizacionService
                    .TienePermiso(
                        UsuarioActual,
                        codigo))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ConfigurarVisibilidad(
            Button boton,
            bool visible)
        {
            boton.Visible = visible;
            boton.Enabled = visible;
        }

        private static int ContarOpcionesDisponibles(
            params bool[] opciones)
        {
            int cantidad = 0;

            foreach (bool opcion in opciones)
            {
                if (opcion)
                {
                    cantidad++;
                }
            }

            return cantidad;
        }

        private void BtnClientes_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                ClientesSolicitados;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnViajes_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                ViajesSolicitados;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnVisitas_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                VisitasSolicitadas;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnViaticos_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                ViaticosSolicitados;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnUsuarios_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                UsuariosSolicitados;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnGrupos_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                GruposSolicitados;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnPermisos_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                PermisosSolicitados;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnAuditoria_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                AuditoriaSolicitada;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnCambiarClave_Click(
            object sender,
            EventArgs e)
        {
            EventHandler handler =
                CambiarClaveSolicitada;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void BtnCerrarSesion_Click(
            object sender,
            EventArgs e)
        {
            DialogResult resultado =
                MessageBox.Show(
                    "Desea cerrar la sesion actual?",
                    "Cerrar sesion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes)
            {
                return;
            }

            OnCerrarSesionSolicitada();
        }

        private void BtnSalir_Click(
            object sender,
            EventArgs e)
        {
            DialogResult resultado =
                MessageBox.Show(
                    "Desea salir de SIGEVIP?",
                    "Salir",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes)
            {
                return;
            }

            OnSalirSolicitado();
        }

        private void MainForm_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            if (_cerrandoControladamente)
            {
                return;
            }

            if (e.CloseReason !=
                CloseReason.UserClosing)
            {
                return;
            }

            DialogResult resultado =
                MessageBox.Show(
                    "Desea salir de SIGEVIP?",
                    "Salir",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            e.Cancel = true;
            _cerrandoControladamente = true;

            OnSalirSolicitado();
        }

        private void OnCerrarSesionSolicitada()
        {
            _cerrandoControladamente = true;

            EventHandler handler =
                CerrarSesionSolicitada;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }

        private void OnSalirSolicitado()
        {
            _cerrandoControladamente = true;

            EventHandler handler =
                SalirSolicitado;

            handler?.Invoke(
                this,
                EventArgs.Empty);
        }
    }
}
