using System;
using System.Drawing;
using System.Windows.Forms;
using SIGEVIP.Application.Security;
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

        private const string ViajeEnviarRendicion =
            "VIAJE_ENVIAR_RENDICION";

        private const string ViajeAprobar =
            "VIAJE_APROBAR";

        private const string ViajeCancelar =
            "VIAJE_CANCELAR";

        private const string VisitaRegistrar =
            "VISITA_REGISTRAR";

        private const string ViaticoCargar =
            "VIATICO_CARGAR";

        private const string ViaticoModificar =
            "VIATICO_MODIFICAR";

        private const string ViaticoExcluir =
            "VIATICO_EXCLUIR";

        private const string ViaticoReactivar =
            "VIATICO_REACTIVAR";

        private const string RendicionRevisar =
            "RENDICION_REVISAR";

        private const string UsuarioGestionar =
            "USUARIO_GESTIONAR";

        private const string GrupoGestionar =
            "GRUPO_GESTIONAR";

        private const string PermisoGestionar =
            "PERMISO_GESTIONAR";

        private const string AuditoriaConsultar =
            "AUDITORIA_CONSULTAR";

        private readonly ISesionActual _sesionActual;
        private readonly AutorizacionService _autorizacionService;
        private readonly PerfilSesion _perfilSesion;

        private Label _lblUsuario;
        private Label _lblEstado;
        private Button _btnClientes;
        private Button _btnViajes;
        private Button _btnVisitas;
        private Button _btnViaticos;
        private Button _btnSeguridad;
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

        public event EventHandler CerrarSesionSolicitada;

        public event EventHandler SalirSolicitado;

        private Usuario UsuarioActual
        {
            get { return _sesionActual.UsuarioActual; }
        }

        private void InicializarFormulario()
        {
            Text = "SIGEVIP - Menu principal";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(920, 600);
            Size = new Size(1020, 680);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.WhiteSmoke;

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
                    Location = new Point(28, 16),
                    Text = "SIGEVIP"
                };

            var lblSubtitulo =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        10F),
                    Location = new Point(31, 56),
                    Text =
                        "Sistema de Gestion de Viajes y Viaticos"
                };

            var panelAcciones =
    new TableLayoutPanel
    {
        Dock = DockStyle.Right,
        Width = 390,
        ColumnCount = 2,
        RowCount = 2,
        Padding = new Padding(10, 10, 20, 8),
        BackColor = Color.White
    };



            panelAcciones.ColumnStyles.Add(

                new ColumnStyle(

                    SizeType.Percent,

                    50F));



            panelAcciones.ColumnStyles.Add(

                new ColumnStyle(

                    SizeType.Percent,

                    50F));



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



            _btnCerrarSesion =

                new Button

                {

                    Dock = DockStyle.Fill,

                    Margin = new Padding(4),

                    Text = "Cerrar sesión",

                    UseVisualStyleBackColor = true

                };



            _btnSalir =

                new Button

                {

                    Dock = DockStyle.Fill,

                    Margin = new Padding(4),

                    Text = "Salir",

                    UseVisualStyleBackColor = true

                };



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

                2);



            panelAcciones.Controls.Add(

                _btnCerrarSesion,

                0,

                1);



            panelAcciones.Controls.Add(

                _btnSalir,

                1,

                1);



            panelEncabezado.Controls.Add(

                panelAcciones);



            panelEncabezado.Controls.Add(

                lblTitulo);



            panelEncabezado.Controls.Add(

                lblSubtitulo);

            var lblModulos =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        14F,
                        FontStyle.Bold),
                    Location = new Point(31, 113),
                    Text = "Modulos disponibles"
                };

            var tablaModulos =
                new TableLayoutPanel
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    ColumnCount = 3,
                    RowCount = 2,
                    Location = new Point(31, 158),
                    Size = new Size(944, 310),
                    Padding = new Padding(0),
                    BackColor = Color.Transparent
                };

            tablaModulos.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.333F));

            tablaModulos.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.333F));

            tablaModulos.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.333F));

            tablaModulos.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

            tablaModulos.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50F));

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

            _btnSeguridad =
                CrearBotonModulo(
                    "Usuarios y seguridad",
                    "Usuarios, grupos, permisos y auditoria.");

            _btnClientes.Click +=
                BtnClientes_Click;

            _btnViajes.Click +=
                BtnViajes_Click;

            _btnVisitas.Click +=
                BtnVisitas_Click;

            _btnViaticos.Click +=
                BtnViaticos_Click;

            _btnSeguridad.Click +=
                BtnSeguridad_Click;

            tablaModulos.Controls.Add(
                _btnClientes,
                0,
                0);

            tablaModulos.Controls.Add(
                _btnViajes,
                1,
                0);

            tablaModulos.Controls.Add(
                _btnVisitas,
                2,
                0);

            tablaModulos.Controls.Add(
                _btnViaticos,
                0,
                1);

            tablaModulos.Controls.Add(
                _btnSeguridad,
                1,
                1);

            _lblEstado =
                new Label
                {
                    Anchor =
                        AnchorStyles.Left |
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Location = new Point(31, 510),
                    Size = new Size(944, 84),
                    Padding = new Padding(12),
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            Controls.Add(_lblEstado);
            Controls.Add(tablaModulos);
            Controls.Add(lblModulos);
            Controls.Add(panelEncabezado);

            FormClosing +=
                MainForm_FormClosing;
        }

        private static Button CrearBotonModulo(
            string titulo,
            string descripcion)
        {
            return new Button
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
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
                UseVisualStyleBackColor = true
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
                    ViajeEnviarRendicion,
                    ViajeAprobar,
                    ViajeCancelar);

            bool puedeVisitas =
                TieneAlgunPermiso(
                    VisitaRegistrar);

            bool puedeViaticos =
                TieneAlgunPermiso(
                    ViaticoCargar,
                    ViaticoModificar,
                    ViaticoExcluir,
                    ViaticoReactivar,
                    RendicionRevisar);

            bool puedeSeguridad =
                TieneAlgunPermiso(
                    UsuarioGestionar,
                    GrupoGestionar,
                    PermisoGestionar,
                    AuditoriaConsultar);

            ConfigurarBoton(
                _btnClientes,
                puedeClientes);

            ConfigurarBoton(
                _btnViajes,
                puedeViajes);

            ConfigurarBoton(
                _btnVisitas,
                puedeVisitas);

            ConfigurarBoton(
                _btnViaticos,
                puedeViaticos);

            ConfigurarBoton(
                _btnSeguridad,
                puedeSeguridad);

            int cantidadDisponibles = 0;

            if (puedeClientes)
            {
                cantidadDisponibles++;
            }

            if (puedeViajes)
            {
                cantidadDisponibles++;
            }

            if (puedeVisitas)
            {
                cantidadDisponibles++;
            }

            if (puedeViaticos)
            {
                cantidadDisponibles++;
            }

            if (puedeSeguridad)
            {
                cantidadDisponibles++;
            }

            _lblEstado.Text =
                "Sesion iniciada correctamente. " +
                "Modulos habilitados segun permisos: " +
                cantidadDisponibles +
                "." +
                Environment.NewLine +
                "Las pantallas operativas se implementaran " +
                "en los siguientes incrementos.";
        }

        private bool TieneAlgunPermiso(
            params string[] codigos)
        {
            foreach (string codigo in codigos)
            {
                if (_autorizacionService.TienePermiso(
                    UsuarioActual,
                    codigo))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ConfigurarBoton(
            Button boton,
            bool habilitado)
        {
            boton.Enabled = habilitado;

            if (!habilitado)
            {
                boton.Text +=
                    Environment.NewLine +
                    Environment.NewLine +
                    "Sin permiso";
            }
        }

        private void BtnClientes_Click(
            object sender,
            EventArgs e)
        {
            MostrarModuloPendiente(
                "Clientes");
        }

        private void BtnViajes_Click(
            object sender,
            EventArgs e)
        {
            MostrarModuloPendiente(
                "Viajes");
        }

        private void BtnVisitas_Click(
            object sender,
            EventArgs e)
        {
            MostrarModuloPendiente(
                "Visitas");
        }

        private void BtnViaticos_Click(
            object sender,
            EventArgs e)
        {
            MostrarModuloPendiente(
                "Viaticos y rendiciones");
        }

        private void BtnSeguridad_Click(
            object sender,
            EventArgs e)
        {
            MostrarModuloPendiente(
                "Usuarios y seguridad");
        }

        private static void MostrarModuloPendiente(
            string nombreModulo)
        {
            MessageBox.Show(
                "El modulo " +
                nombreModulo +
                " esta autorizado para el usuario actual, " +
                "pero su pantalla funcional se implementara " +
                "en un incremento posterior.",
                "Modulo pendiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnCerrarSesion_Click(
            object sender,
            EventArgs e)
        {
            DialogResult resultado =
                MessageBox.Show(
                    "¿Desea cerrar la sesion actual?",
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
                    "¿Desea salir de SIGEVIP?",
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

                    "¿Desea salir de SIGEVIP?",

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
