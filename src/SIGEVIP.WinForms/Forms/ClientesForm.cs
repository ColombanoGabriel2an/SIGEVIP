using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Clientes;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ClientesForm : Form
    {
        private readonly ClienteService
            _clienteService;

        private readonly ISesionActual
            _sesionActual;

        private readonly AutorizacionService
            _autorizacionService;

        private TextBox _txtBusqueda;
        private TextBox _txtCuit;
        private TextBox _txtLocalidad;
        private TextBox _txtProvincia;
        private ComboBox _cmbEstado;

        private Button _btnBuscar;
        private Button _btnLimpiar;
        private Button _btnNuevo;
        private Button _btnModificar;
        private Button _btnActivar;
        private Button _btnDesactivar;
        private Button _btnCerrar;

        private DataGridView _grilla;
        private Label _lblCantidad;

        public ClientesForm(
            ClienteService clienteService,
            ISesionActual sesionActual,
            AutorizacionService autorizacionService)
        {
            _clienteService =
                clienteService
                ?? throw new ArgumentNullException(
                    nameof(clienteService));

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
                ClientesForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Clientes";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(1080, 650);

            Size =
                new Size(1220, 740);

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
                        new Point(24, 20),
                    Text =
                        "Gestión de clientes"
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
                        new Point(25, 655),
                    Text =
                        "Resultados: 0"
                };

            Controls.Add(lblTitulo);
            Controls.Add(panelFiltros);
            Controls.Add(_grilla);
            Controls.Add(panelAcciones);
            Controls.Add(_lblCantidad);
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
                        new Size(1155, 125)
                };

            _txtBusqueda =
                CrearCampoFiltro(
                    panel,
                    "Búsqueda general",
                    16,
                    42,
                    250);

            _txtCuit =
                CrearCampoFiltro(
                    panel,
                    "CUIT",
                    285,
                    42,
                    170);

            _txtLocalidad =
                CrearCampoFiltro(
                    panel,
                    "Localidad",
                    475,
                    42,
                    180);

            _txtProvincia =
                CrearCampoFiltro(
                    panel,
                    "Provincia",
                    675,
                    42,
                    180);

            var lblEstado =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(875, 17),
                    Text =
                        "Estado"
                };

            _cmbEstado =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(878, 42),
                    Size =
                        new Size(145, 23)
                };

            _cmbEstado.Items.Add(
                "Todos");

            _cmbEstado.Items.Add(
                "Activos");

            _cmbEstado.Items.Add(
                "Inactivos");

            _cmbEstado.SelectedIndex = 0;

            _btnBuscar =
                new Button
                {
                    Location =
                        new Point(878, 78),
                    Size =
                        new Size(120, 32),
                    Text =
                        "Buscar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnLimpiar =
                new Button
                {
                    Location =
                        new Point(1008, 78),
                    Size =
                        new Size(120, 32),
                    Text =
                        "Limpiar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnBuscar.Click +=
                BtnBuscar_Click;

            _btnLimpiar.Click +=
                BtnLimpiar_Click;

            panel.Controls.Add(lblEstado);
            panel.Controls.Add(_cmbEstado);
            panel.Controls.Add(_btnBuscar);
            panel.Controls.Add(_btnLimpiar);

            return panel;
        }

        private static TextBox CrearCampoFiltro(
            Control contenedor,
            string titulo,
            int posicionX,
            int posicionY,
            int ancho)
        {
            var label =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(posicionX, 17),
                    Text =
                        titulo
                };

            var textBox =
                new TextBox
                {
                    Location =
                        new Point(
                            posicionX,
                            posicionY),
                    Size =
                        new Size(ancho, 23)
                };

            contenedor.Controls.Add(label);
            contenedor.Controls.Add(textBox);

            return textBox;
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
                        new Point(24, 208),
                    MultiSelect =
                        false,
                    ReadOnly =
                        true,
                    RowHeadersVisible =
                        false,
                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,
                    Size =
                        new Size(1155, 375)
                };

            AgregarColumna(
                grilla,
                "RazonSocial",
                "Razón social",
                200);

            AgregarColumna(
                grilla,
                "Cuit",
                "CUIT",
                110);

            AgregarColumna(
                grilla,
                "Email",
                "Email",
                185);

            AgregarColumna(
                grilla,
                "Telefono",
                "Teléfono",
                115);

            AgregarColumna(
                grilla,
                "Localidad",
                "Localidad",
                120);

            AgregarColumna(
                grilla,
                "Provincia",
                "Provincia",
                120);

            AgregarColumna(
                grilla,
                "Estado",
                "Estado",
                80);

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
                        new Point(24, 595),
                    Size =
                        new Size(1155, 48),
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

            panel.Controls.Add(_btnNuevo);
            panel.Controls.Add(_btnModificar);
            panel.Controls.Add(_btnActivar);
            panel.Controls.Add(_btnDesactivar);
            panel.Controls.Add(_btnCerrar);

            return panel;
        }

        private static Button CrearBoton(
            string texto)
        {
            return new Button
            {
                Margin =
                    new Padding(0, 0, 10, 0),
                Size =
                    new Size(125, 36),
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
                        ClienteService.PermisoGestionar);

            _btnNuevo.Visible =
                puedeGestionar;

            _btnModificar.Visible =
                puedeGestionar;

            _btnActivar.Visible =
                puedeGestionar;

            _btnDesactivar.Visible =
                puedeGestionar;
        }

        private void ClientesForm_Load(
            object sender,
            EventArgs e)
        {
            CargarClientes();
        }

        private void BtnBuscar_Click(
            object sender,
            EventArgs e)
        {
            CargarClientes();
        }

        private void BtnLimpiar_Click(
            object sender,
            EventArgs e)
        {
            _txtBusqueda.Clear();
            _txtCuit.Clear();
            _txtLocalidad.Clear();
            _txtProvincia.Clear();
            _cmbEstado.SelectedIndex = 0;

            CargarClientes();
        }

        private void BtnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using (
                var formulario =
                    new ClienteEditForm(
                        _clienteService,
                        null))
            {
                if (formulario.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    CargarClientes();
                }
            }
        }

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            ClienteListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            try
            {
                Cliente cliente =
                    _clienteService.Obtener(
                        seleccionado.IdCliente);

                using (
                    var formulario =
                        new ClienteEditForm(
                            _clienteService,
                            cliente))
                {
                    if (formulario.ShowDialog(this) ==
                        DialogResult.OK)
                    {
                        CargarClientes();
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
            ClienteListadoDto seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
            {
                MostrarSeleccionRequerida();
                return;
            }

            if (activar && seleccionado.Activo)
            {
                MessageBox.Show(
                    "El cliente seleccionado ya se encuentra activo.",
                    "Clientes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (!activar && !seleccionado.Activo)
            {
                MessageBox.Show(
                    "El cliente seleccionado ya se encuentra inactivo.",
                    "Clientes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult confirmacion =
                MessageBox.Show(
                    activar
                        ? "¿Desea activar el cliente seleccionado?"
                        : "¿Desea desactivar el cliente seleccionado?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (activar)
                {
                    _clienteService.Activar(
                        seleccionado.IdCliente);
                }
                else
                {
                    _clienteService.Desactivar(
                        seleccionado.IdCliente);
                }

                CargarClientes();
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

        private void CargarClientes()
        {
            CambiarEstadoCarga(true);

            try
            {
                ClienteFiltro filtro =
                    new ClienteFiltro(
                        _txtBusqueda.Text,
                        _txtCuit.Text,
                        _txtLocalidad.Text,
                        _txtProvincia.Text,
                        ObtenerEstadoSeleccionado());

                IReadOnlyCollection<ClienteListadoDto> clientes =
                    _clienteService.Listar(
                        filtro);

                List<ClienteListadoDto> lista =
                    clientes.ToList();

                _grilla.DataSource =
                    null;

                _grilla.DataSource =
                    lista;

                _lblCantidad.Text =
                    "Resultados: " +
                    lista.Count;
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

        private bool? ObtenerEstadoSeleccionado()
        {
            switch (_cmbEstado.SelectedIndex)
            {
                case 1:
                    return true;

                case 2:
                    return false;

                default:
                    return null;
            }
        }

        private ClienteListadoDto ObtenerSeleccionado()
        {
            if (_grilla.CurrentRow == null)
            {
                return null;
            }

            return _grilla.CurrentRow.DataBoundItem
                as ClienteListadoDto;
        }

        private static void MostrarSeleccionRequerida()
        {
            MessageBox.Show(
                "Seleccione un cliente de la lista.",
                "Clientes",
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
                    "Clientes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible acceder a los clientes. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado en el módulo de clientes.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
