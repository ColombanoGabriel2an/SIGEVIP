using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Visitas;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

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

        private DateTimePicker _dtpFecha;
        private TextBox _txtLocalidad;
        private TextBox _txtObservacion;
        private CheckedListBox _lstClientes;
        private Label _lblCantidadClientes;
        private Button _btnSeleccionarTodos;
        private Button _btnQuitarSeleccion;
        private Button _btnGuardar;
        private Button _btnCancelar;

        public VisitaEditForm(
            VisitaService visitaService,
            int idViaje,
            DateTime fechaInicioViaje,
            DateTime fechaFinViaje,
            string descripcionViaje)
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

            _idViaje =
                idViaje;

            _fechaInicioViaje =
                fechaInicioViaje.Date;

            _fechaFinViaje =
                fechaFinViaje.Date;

            _descripcionViaje =
                descripcionViaje
                ?? string.Empty;

            InicializarFormulario();

            Load +=
                VisitaEditForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                "SIGEVIP - Registrar visita";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox =
                false;

            MinimizeBox =
                false;

            ClientSize =
                new Size(760, 650);

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
                        16F,
                        FontStyle.Bold),
                    Location =
                        new Point(24, 18),
                    Text =
                        "Registrar visita comercial"
                });

            var lblViaje =
                new Label
                {
                    AutoEllipsis =
                        true,
                    BackColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(25, 63),
                    Padding =
                        new Padding(10),
                    Size =
                        new Size(710, 55),
                    Text =
                        _descripcionViaje,
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            Controls.Add(
                lblViaje);

            Controls.Add(
                CrearEtiqueta(
                    "Fecha",
                    25,
                    140));

            _dtpFecha =
                new DateTimePicker
                {
                    Format =
                        DateTimePickerFormat.Short,
                    Location =
                        new Point(25, 164),
                    MinDate =
                        _fechaInicioViaje,
                    MaxDate =
                        _fechaFinViaje,
                    Size =
                        new Size(180, 24),
                    Value =
                        ObtenerFechaInicial()
                };

            Controls.Add(
                _dtpFecha);

            Controls.Add(
                CrearEtiqueta(
                    "Localidad del encuentro",
                    230,
                    140));

            _txtLocalidad =
                new TextBox
                {
                    Location =
                        new Point(230, 164),
                    MaxLength =
                        150,
                    Size =
                        new Size(505, 24)
                };

            Controls.Add(
                _txtLocalidad);

            Controls.Add(
                CrearEtiqueta(
                    "Observación",
                    25,
                    210));

            _txtObservacion =
                new TextBox
                {
                    AcceptsReturn =
                        true,
                    Location =
                        new Point(25, 234),
                    MaxLength =
                        1000,
                    Multiline =
                        true,
                    ScrollBars =
                        ScrollBars.Vertical,
                    Size =
                        new Size(710, 105)
                };

            Controls.Add(
                _txtObservacion);

            Controls.Add(
                CrearEtiqueta(
                    "Clientes",
                    25,
                    360));

            _lstClientes =
                new CheckedListBox
                {
                    CheckOnClick =
                        true,
                    DisplayMember =
                        "Descripcion",
                    HorizontalScrollbar =
                        true,
                    Location =
                        new Point(25, 385),
                    Size =
                        new Size(710, 150)
                };

            _lstClientes.ItemCheck +=
                LstClientes_ItemCheck;

            Controls.Add(
                _lstClientes);

            _lblCantidadClientes =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(25, 545),
                    Text =
                        "Clientes seleccionados: 0"
                };

            Controls.Add(
                _lblCantidadClientes);

            _btnSeleccionarTodos =
                CrearBoton(
                    "Seleccionar todos",
                    355,
                    540,
                    150);

            _btnQuitarSeleccion =
                CrearBoton(
                    "Quitar selección",
                    515,
                    540,
                    140);

            _btnSeleccionarTodos.Click +=
                BtnSeleccionarTodos_Click;

            _btnQuitarSeleccion.Click +=
                BtnQuitarSeleccion_Click;

            Controls.Add(
                _btnSeleccionarTodos);

            Controls.Add(
                _btnQuitarSeleccion);

            _btnGuardar =
                CrearBoton(
                    "Guardar",
                    485,
                    592,
                    120);

            _btnCancelar =
                CrearBoton(
                    "Cancelar",
                    615,
                    592,
                    120);

            _btnGuardar.Click +=
                BtnGuardar_Click;

            _btnCancelar.Click +=
                delegate
                {
                    DialogResult =
                        DialogResult.Cancel;

                    Close();
                };

            Controls.Add(
                _btnGuardar);

            Controls.Add(
                _btnCancelar);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private static Label CrearEtiqueta(
            string texto,
            int posicionX,
            int posicionY)
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
                        posicionY),
                Text =
                    texto
            };
        }

        private static Button CrearBoton(
            string texto,
            int posicionX,
            int posicionY,
            int ancho)
        {
            return new Button
            {
                Location =
                    new Point(
                        posicionX,
                        posicionY),
                Size =
                    new Size(
                        ancho,
                        34),
                Text =
                    texto,
                UseVisualStyleBackColor =
                    true
            };
        }

        private DateTime ObtenerFechaInicial()
        {
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
            CargarClientes();
        }

        private void CargarClientes()
        {
            CambiarEstadoCarga(
                true);

            try
            {
                List<ClienteSeleccionVisitaDto> clientes =
                    _visitaService
                        .ListarClientesDisponibles()
                        .OrderBy(
                            cliente =>
                                cliente.RazonSocial)
                        .ThenBy(
                            cliente =>
                                cliente.IdCliente)
                        .ToList();

                _lstClientes.DataSource =
                    null;

                _lstClientes.DataSource =
                    clientes;

                _lstClientes.DisplayMember =
                    "Descripcion";

                _lblCantidadClientes.Text =
                    "Clientes seleccionados: 0";

                if (clientes.Count == 0)
                {
                    MessageBox.Show(
                        "No existen clientes activos disponibles. " +
                        "Debe registrar o activar un cliente antes " +
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

        private void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            List<int> idsClientes =
                ObtenerClientesSeleccionados();

            CambiarEstadoCarga(
                true);

            try
            {
                int idVisita =
                    _visitaService.Registrar(
                        _idViaje,
                        _dtpFecha.Value.Date,
                        _txtObservacion.Text,
                        _txtLocalidad.Text,
                        idsClientes);

                MessageBox.Show(
                    "La visita fue registrada correctamente. " +
                    "Identificador: " +
                    idVisita +
                    ".",
                    "Visitas",
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
                CambiarEstadoCarga(
                    false);
            }
        }

        private List<int> ObtenerClientesSeleccionados()
        {
            return _lstClientes
                .CheckedItems
                .Cast<ClienteSeleccionVisitaDto>()
                .Select(
                    cliente =>
                        cliente.IdCliente)
                .ToList();
        }

        private void BtnSeleccionarTodos_Click(
            object sender,
            EventArgs e)
        {
            for (
                int indice = 0;
                indice < _lstClientes.Items.Count;
                indice++)
            {
                _lstClientes.SetItemChecked(
                    indice,
                    true);
            }

            ActualizarCantidadSeleccionada();
        }

        private void BtnQuitarSeleccion_Click(
            object sender,
            EventArgs e)
        {
            for (
                int indice = 0;
                indice < _lstClientes.Items.Count;
                indice++)
            {
                _lstClientes.SetItemChecked(
                    indice,
                    false);
            }

            ActualizarCantidadSeleccionada();
        }

        private void LstClientes_ItemCheck(
            object sender,
            ItemCheckEventArgs e)
        {
            BeginInvoke(
                new Action(
                    ActualizarCantidadSeleccionada));
        }

        private void ActualizarCantidadSeleccionada()
        {
            _lblCantidadClientes.Text =
                "Clientes seleccionados: " +
                _lstClientes.CheckedItems.Count;
        }

        private void CambiarEstadoCarga(
            bool cargando)
        {
            _btnGuardar.Enabled =
                !cargando;

            _btnSeleccionarTodos.Enabled =
                !cargando;

            _btnQuitarSeleccion.Enabled =
                !cargando;

            _lstClientes.Enabled =
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
                    "No fue posible registrar la visita. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado al registrar la visita.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}