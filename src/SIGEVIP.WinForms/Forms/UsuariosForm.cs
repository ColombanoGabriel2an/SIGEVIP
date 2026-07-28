using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Application.Usuarios;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class UsuariosForm : Form
    {
        private readonly UsuarioGestionService
            _usuarioService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private TextBox _txtBusqueda;
        private ComboBox _cmbEstado;
        private ComboBox _cmbGrupo;

        private Button _btnBuscar;
        private Button _btnLimpiar;
        private Button _btnNuevo;
        private Button _btnModificar;
        private Button _btnActivar;
        private Button _btnDesactivar;
        private Button _btnCerrar;

        private DataGridView _grilla;
        private Label _lblCantidad;
        private Label _lblUsuarioActual;

        public UsuariosForm(
            UsuarioGestionService usuarioService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _usuarioService =
                usuarioService
                ?? throw new ArgumentNullException(
                    nameof(usuarioService));

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
                UsuariosForm_Load;

            _grilla.SelectionChanged +=
                Grilla_SelectionChanged;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Usuarios";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(
                    1080,
                    650);

            Size =
                new Size(
                    1220,
                    740);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            var lblTitulo =
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
                            20),
                    Text =
                        "Gestión de usuarios"
                };

            _lblUsuarioActual =
                new Label
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Right,
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            745,
                            27),
                    Size =
                        new Size(
                            434,
                            24),
                    TextAlign =
                        ContentAlignment.MiddleRight
                };

            var panelFiltros =
                CrearPanelFiltros();

            _grilla =
                CrearGrilla();

            var panelAcciones =
                CrearPanelAcciones();

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
                        new Point(
                            25,
                            655),
                    Text =
                        "Resultados: 0"
                };

            Controls.Add(
                lblTitulo);

            Controls.Add(
                _lblUsuarioActual);

            Controls.Add(
                panelFiltros);

            Controls.Add(
                _grilla);

            Controls.Add(
                panelAcciones);

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
                        new Point(
                            24,
                            67),
                    Size =
                        new Size(
                            1155,
                            125)
                };

            var lblBusqueda =
                CrearEtiquetaFiltro(
                    "Búsqueda general",
                    16);

            _txtBusqueda =
                new TextBox
                {
                    Location =
                        new Point(
                            16,
                            42),
                    MaxLength = 254,
                    Size =
                        new Size(
                            390,
                            23)
                };

            var lblEstado =
                CrearEtiquetaFiltro(
                    "Estado",
                    430);

            _cmbEstado =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            430,
                            42),
                    Size =
                        new Size(
                            150,
                            23)
                };

            _cmbEstado.Items.Add(
                "Todos");

            _cmbEstado.Items.Add(
                "Activos");

            _cmbEstado.Items.Add(
                "Inactivos");

            _cmbEstado.SelectedIndex = 0;

            var lblGrupo =
                CrearEtiquetaFiltro(
                    "Grupo",
                    605);

            _cmbGrupo =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            605,
                            42),
                    Size =
                        new Size(
                            300,
                            23)
                };

            _btnBuscar =
                new Button
                {
                    Location =
                        new Point(
                            878,
                            78),
                    Size =
                        new Size(
                            120,
                            32),
                    Text =
                        "Buscar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnLimpiar =
                new Button
                {
                    Location =
                        new Point(
                            1008,
                            78),
                    Size =
                        new Size(
                            120,
                            32),
                    Text =
                        "Limpiar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnBuscar.Click +=
                BtnBuscar_Click;

            _btnLimpiar.Click +=
                BtnLimpiar_Click;

            panel.Controls.Add(
                lblBusqueda);

            panel.Controls.Add(
                _txtBusqueda);

            panel.Controls.Add(
                lblEstado);

            panel.Controls.Add(
                _cmbEstado);

            panel.Controls.Add(
                lblGrupo);

            panel.Controls.Add(
                _cmbGrupo);

            panel.Controls.Add(
                _btnBuscar);

            panel.Controls.Add(
                _btnLimpiar);

            return panel;
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
                    new Point(
                        posicionX,
                        17),
                Text =
                    texto
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
                    BorderStyle =
                        BorderStyle.Fixed3D,
                    ColumnHeadersHeightSizeMode =
                        DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                    Location =
                        new Point(
                            24,
                            208),
                    MultiSelect =
                        false,
                    ReadOnly =
                        true,
                    RowHeadersVisible =
                        false,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    Size =
                        new Size(
                            1155,
                            375)
                };

            AgregarColumna(
                grilla,
                "NombreUsuario",
                "Usuario",
                160);

            AgregarColumna(
                grilla,
                "NombreCompleto",
                "Persona",
                210);

            AgregarColumna(
                grilla,
                "Email",
                "Email",
                230);

            AgregarColumna(
                grilla,
                "GruposResumen",
                "Grupos",
                390);

            AgregarColumna(
                grilla,
                "Estado",
                "Estado",
                90);

            return grilla;
        }

        private static void AgregarColumna(
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
                    Width =
                        ancho
                });
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
                            595),
                    Size =
                        new Size(
                            1155,
                            48),
                    WrapContents =
                        false
                };

            _btnNuevo =
                CrearBoton(
                    "Nuevo");

            _btnModificar =
                CrearBoton(
                    "Modificar");

            _btnActivar =
                CrearBoton(
                    "Activar");

            _btnDesactivar =
                CrearBoton(
                    "Desactivar");

            _btnCerrar =
                CrearBoton(
                    "Cerrar");

            _btnNuevo.Click +=
                BtnNuevo_Click;

            _btnModificar.Click +=
                BtnModificar_Click;

            _btnActivar.Click +=
                BtnActivar_Click;

            _btnDesactivar.Click +=
                BtnDesactivar_Click;

            _btnCerrar.Click +=
                BtnCerrar_Click;

            panel.Controls.Add(
                _btnNuevo);

            panel.Controls.Add(
                _btnModificar);

            panel.Controls.Add(
                _btnActivar);

            panel.Controls.Add(
                _btnDesactivar);

            panel.Controls.Add(
                _btnCerrar);

            return panel;
        }

        private static Button CrearBoton(
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
                        125,
                        36),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private void ConfigurarPermisos()
        {
            bool puedeGestionar =
                _sesionActual.HayUsuarioAutenticado
                && _autorizacionService
                    .TienePermiso(
                        _sesionActual.UsuarioActual,
                        UsuarioGestionService
                            .PermisoGestionar);

            _btnNuevo.Visible =
                puedeGestionar;

            _btnModificar.Visible =
                puedeGestionar;

            _btnActivar.Visible =
                puedeGestionar;

            _btnDesactivar.Visible =
                puedeGestionar;

            if (_sesionActual.HayUsuarioAutenticado)
            {
                _lblUsuarioActual.Text =
                    "Usuario autenticado: " +
                    _sesionActual
                        .UsuarioActual
                        .NombreUsuario;
            }
        }

        private void UsuariosForm_Load(
            object sender,
            EventArgs e)
        {
            CargarGruposFiltro();
            CargarUsuarios();
        }

        private void BtnBuscar_Click(
            object sender,
            EventArgs e)
        {
            CargarUsuarios();
        }

        private void BtnLimpiar_Click(
            object sender,
            EventArgs e)
        {
            _txtBusqueda.Clear();
            _cmbEstado.SelectedIndex = 0;
            _cmbGrupo.SelectedIndex = 0;

            CargarUsuarios();
        }

        private void BtnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using (
                var formulario =
                    new UsuarioEditForm(
                        _usuarioService,
                        null))
            {
                if (formulario.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    CargarGruposFiltro();
                    CargarUsuarios();
                }
            }
        }

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            UsuarioListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            if (EsUsuarioActual(
                seleccionado.IdUsuario))
            {
                MessageBox.Show(
                    "El usuario autenticado no puede modificarse " +
                    "durante esta sesión.",
                    "Usuarios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                UsuarioDetalleDto detalle =
                    _usuarioService.Obtener(
                        seleccionado.IdUsuario);

                using (
                    var formulario =
                        new UsuarioEditForm(
                            _usuarioService,
                            detalle))
                {
                    if (formulario.ShowDialog(this) ==
                        DialogResult.OK)
                    {
                        CargarGruposFiltro();
                        CargarUsuarios();
                    }
                }
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnActivar_Click(
            object sender,
            EventArgs e)
        {
            CambiarEstadoSeleccionado(
                true);
        }

        private void BtnDesactivar_Click(
            object sender,
            EventArgs e)
        {
            CambiarEstadoSeleccionado(
                false);
        }

        private void CambiarEstadoSeleccionado(
            bool activar)
        {
            UsuarioListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            if (activar &&
                seleccionado.Activo)
            {
                MessageBox.Show(
                    "El usuario seleccionado ya se encuentra activo.",
                    "Usuarios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!activar &&
                !seleccionado.Activo)
            {
                MessageBox.Show(
                    "El usuario seleccionado ya se encuentra inactivo.",
                    "Usuarios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!activar &&
                EsUsuarioActual(
                    seleccionado.IdUsuario))
            {
                MessageBox.Show(
                    "El usuario autenticado no puede desactivarse " +
                    "durante esta sesión.",
                    "Usuarios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult confirmacion =
                MessageBox.Show(
                    activar
                        ? "¿Desea activar el usuario seleccionado?"
                        : "¿Desea desactivar el usuario seleccionado?",
                    "Confirmar",
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
                if (activar)
                {
                    _usuarioService.Activar(
                        seleccionado.IdUsuario);
                }
                else
                {
                    _usuarioService.Desactivar(
                        seleccionado.IdUsuario);
                }

                CargarUsuarios();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);
            }
        }

        private void BtnCerrar_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        private void CargarGruposFiltro()
        {
            CambiarEstadoCarga(
                true);

            try
            {
                IReadOnlyCollection<GrupoSeleccionUsuarioDto>
                    grupos =
                        _usuarioService.ListarGrupos(
                            new List<int>()
                                .AsReadOnly());

                List<GrupoFiltroItem> elementos =
                    new List<GrupoFiltroItem>();

                elementos.Add(
                    new GrupoFiltroItem(
                        null,
                        "Todos"));

                elementos.AddRange(
                    grupos.Select(
                        grupo =>
                            new GrupoFiltroItem(
                                grupo.IdGrupo,
                                grupo.Descripcion)));

                int? seleccionAnterior =
                    ObtenerIdGrupoSeleccionado();

                _cmbGrupo.DisplayMember =
                    "Descripcion";

                _cmbGrupo.ValueMember =
                    "IdGrupo";

                _cmbGrupo.DataSource =
                    elementos;

                SeleccionarGrupoFiltro(
                    seleccionAnterior);
            }
            catch (Exception exception)
            {
                _cmbGrupo.DataSource =
                    null;

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void CargarUsuarios()
        {
            CambiarEstadoCarga(
                true);

            try
            {
                UsuarioFiltro filtro =
                    new UsuarioFiltro(
                        _txtBusqueda.Text,
                        ObtenerEstadoSeleccionado(),
                        ObtenerIdGrupoSeleccionado());

                IReadOnlyCollection<UsuarioListadoDto>
                    usuarios =
                        _usuarioService.Listar(
                            filtro);

                List<UsuarioListadoDto> lista =
                    usuarios.ToList();

                _grilla.DataSource =
                    null;

                _grilla.DataSource =
                    lista;

                _lblCantidad.Text =
                    "Resultados: " +
                    lista.Count;

                ConfigurarAccionesSeleccion();
            }
            catch (Exception exception)
            {
                _grilla.DataSource =
                    null;

                _lblCantidad.Text =
                    "Resultados: 0";

                ConfigurarAccionesSeleccion();

                MostrarErrorControlado(
                    exception);
            }
            finally
            {
                CambiarEstadoCarga(
                    false);
            }
        }

        private void Grilla_SelectionChanged(
            object sender,
            EventArgs e)
        {
            ConfigurarAccionesSeleccion();
        }

        private void ConfigurarAccionesSeleccion()
        {
            UsuarioListadoDto seleccionado =
                ObtenerSeleccionado();

            bool haySeleccion =
                seleccionado != null;

            bool esUsuarioActual =
                haySeleccion
                && EsUsuarioActual(
                    seleccionado.IdUsuario);

            _btnModificar.Enabled =
                haySeleccion &&
                !esUsuarioActual;

            _btnActivar.Enabled =
                haySeleccion &&
                !seleccionado.Activo;

            _btnDesactivar.Enabled =
                haySeleccion &&
                seleccionado.Activo &&
                !esUsuarioActual;
        }

        private void CambiarEstadoCarga(
            bool cargando)
        {
            _btnBuscar.Enabled =
                !cargando;

            _btnLimpiar.Enabled =
                !cargando;

            _btnNuevo.Enabled =
                !cargando;

            _cmbEstado.Enabled =
                !cargando;

            _cmbGrupo.Enabled =
                !cargando;

            _txtBusqueda.Enabled =
                !cargando;

            if (cargando)
            {
                _btnModificar.Enabled =
                    false;

                _btnActivar.Enabled =
                    false;

                _btnDesactivar.Enabled =
                    false;
            }
            else
            {
                ConfigurarAccionesSeleccion();
            }

            UseWaitCursor =
                cargando;
        }

        private bool? ObtenerEstadoSeleccionado()
        {
            switch (
                _cmbEstado.SelectedIndex)
            {
                case 1:
                    return true;

                case 2:
                    return false;

                default:
                    return null;
            }
        }

        private int? ObtenerIdGrupoSeleccionado()
        {
            GrupoFiltroItem seleccionado =
                _cmbGrupo.SelectedItem
                    as GrupoFiltroItem;

            return seleccionado == null
                ? null
                : seleccionado.IdGrupo;
        }

        private void SeleccionarGrupoFiltro(
            int? idGrupo)
        {
            if (_cmbGrupo.Items.Count == 0)
            {
                return;
            }

            for (
                int indice = 0;
                indice < _cmbGrupo.Items.Count;
                indice++)
            {
                GrupoFiltroItem item =
                    _cmbGrupo.Items[indice]
                        as GrupoFiltroItem;

                if (item != null &&
                    item.IdGrupo ==
                    idGrupo)
                {
                    _cmbGrupo.SelectedIndex =
                        indice;

                    return;
                }
            }

            _cmbGrupo.SelectedIndex = 0;
        }

        private UsuarioListadoDto ObtenerSeleccionado()
        {
            if (_grilla.CurrentRow == null)
            {
                return null;
            }

            return _grilla
                .CurrentRow
                .DataBoundItem
                as UsuarioListadoDto;
        }

        private bool EsUsuarioActual(
            int idUsuario)
        {
            return _sesionActual
                .HayUsuarioAutenticado
                && _sesionActual
                    .UsuarioActual
                    .IdUsuario ==
                    idUsuario;
        }

        private static void MostrarSeleccionRequerida()
        {
            MessageBox.Show(
                "Seleccione un usuario de la lista.",
                "Usuarios",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static void MostrarErrorControlado(
            Exception exception)
        {
            if (exception is ReglaNegocioException ||
                exception is AccesoDenegadoException)
            {
                MessageBox.Show(
                    exception.Message,
                    "Usuarios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible acceder a los usuarios. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado en el módulo de usuarios.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private sealed class GrupoFiltroItem
        {
            public GrupoFiltroItem(
                int? idGrupo,
                string descripcion)
            {
                IdGrupo =
                    idGrupo;

                Descripcion =
                    descripcion;
            }

            public int? IdGrupo
            {
                get;
                private set;
            }

            public string Descripcion
            {
                get;
                private set;
            }
        }
    }
}
