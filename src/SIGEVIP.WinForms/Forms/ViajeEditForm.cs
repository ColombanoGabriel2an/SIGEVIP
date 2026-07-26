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

        private DateTimePicker _dtpFechaInicio;
        private DateTimePicker _dtpFechaFin;
        private TextBox _txtDescripcion;
        private ComboBox _cmbTipoViaje;
        private NumericUpDown _nudMontoAnticipado;
        private CheckedListBox _lstParticipantes;
        private Label _lblParticipantes;

        private Button _btnGuardar;
        private Button _btnCancelar;

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
                new Size(720, 640);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            _errorProvider.ContainerControl =
                this;

            var lblTitulo =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        16F,
                        FontStyle.Bold),
                    Location =
                        new Point(28, 20),
                    Text =
                        EsEdicion
                            ? "Modificar viaje"
                            : "Nuevo viaje"
                };

            _dtpFechaInicio =
                CrearFecha(
                    "Fecha de inicio",
                    82);

            _dtpFechaFin =
                CrearFecha(
                    "Fecha de fin",
                    132);

            _txtDescripcion =
                CrearTexto(
                    "Descripción",
                    182,
                    500);

            _txtDescripcion.Multiline =
                true;

            _txtDescripcion.ScrollBars =
                ScrollBars.Vertical;

            _txtDescripcion.Size =
                new Size(475, 70);

            _cmbTipoViaje =
                CrearTipoViaje(
                    272);

            _nudMontoAnticipado =
                CrearMonto(
                    322);

            _lblParticipantes =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(30, 378),
                    Text =
                        "Participantes"
                };

            _lstParticipantes =
                new CheckedListBox
                {
                    CheckOnClick =
                        true,
                    DisplayMember =
                        "NombreCompleto",
                    Location =
                        new Point(185, 374),
                    Size =
                        new Size(475, 170)
                };

            _btnGuardar =
                new Button
                {
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(426, 574),
                    Size =
                        new Size(110, 36),
                    Text =
                        "Guardar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnCancelar =
                new Button
                {
                    DialogResult =
                        DialogResult.Cancel,
                    Location =
                        new Point(550, 574),
                    Size =
                        new Size(110, 36),
                    Text =
                        "Cancelar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnGuardar.Click +=
                BtnGuardar_Click;

            Controls.Add(lblTitulo);
            Controls.Add(_lblParticipantes);
            Controls.Add(_lstParticipantes);
            Controls.Add(_btnGuardar);
            Controls.Add(_btnCancelar);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private DateTimePicker CrearFecha(
            string etiqueta,
            int posicionY)
        {
            CrearEtiqueta(
                etiqueta,
                posicionY);

            var control =
                new DateTimePicker
                {
                    Format =
                        DateTimePickerFormat.Short,
                    Location =
                        new Point(185, posicionY - 4),
                    Size =
                        new Size(180, 25)
                };

            Controls.Add(control);

            return control;
        }

        private TextBox CrearTexto(
            string etiqueta,
            int posicionY,
            int longitudMaxima)
        {
            CrearEtiqueta(
                etiqueta,
                posicionY);

            var control =
                new TextBox
                {
                    Location =
                        new Point(185, posicionY - 4),
                    MaxLength =
                        longitudMaxima,
                    Size =
                        new Size(475, 25)
                };

            Controls.Add(control);

            return control;
        }

        private ComboBox CrearTipoViaje(
            int posicionY)
        {
            CrearEtiqueta(
                "Tipo de viaje",
                posicionY);

            var control =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(185, posicionY - 4),
                    Size =
                        new Size(260, 25)
                };

            control.DataSource =
                Enum.GetValues(
                    typeof(TipoViaje));

            Controls.Add(control);

            return control;
        }

        private NumericUpDown CrearMonto(
            int posicionY)
        {
            CrearEtiqueta(
                "Monto anticipado",
                posicionY);

            var control =
                new NumericUpDown
                {
                    DecimalPlaces = 2,
                    Increment = 100m,
                    Maximum = 9999999999999999m,
                    Minimum = 0m,
                    ThousandsSeparator = true,
                    Location =
                        new Point(185, posicionY - 4),
                    Size =
                        new Size(260, 25)
                };

            Controls.Add(control);

            return control;
        }

        private void CrearEtiqueta(
            string texto,
            int posicionY)
        {
            Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(30, posicionY),
                    Text =
                        texto
                });
        }

        private void ViajeEditForm_Load(
            object sender,
            EventArgs e)
        {
            CargarParticipantes();
        }

        private void CargarParticipantes()
        {
            CambiarEstado(true);

            try
            {
                IReadOnlyCollection<PersonaSeleccionDto>
                    disponibles =
                        _viajeService
                            .ListarParticipantesDisponibles();

                _lstParticipantes.Items.Clear();

                foreach (
                    PersonaSeleccionDto participante
                    in disponibles)
                {
                    bool seleccionado =
                        EsEdicion &&
                        _viaje.Participantes.Any(
                            actual =>
                                actual.IdPersona ==
                                participante.IdPersona);

                    _lstParticipantes.Items.Add(
                        participante,
                        seleccionado);
                }

                CargarDatosGenerales();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);

                Close();
            }
            finally
            {
                CambiarEstado(false);
            }
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

            CambiarEstado(true);

            try
            {
                List<int> idsParticipantes =
                    ObtenerIdsParticipantes();

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
                CambiarEstado(false);
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

            if (_lstParticipantes.CheckedItems.Count == 0)
            {
                _errorProvider.SetError(
                    _lstParticipantes,
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

        private List<int> ObtenerIdsParticipantes()
        {
            return _lstParticipantes
                .CheckedItems
                .Cast<PersonaSeleccionDto>()
                .Select(
                    participante =>
                        participante.IdPersona)
                .ToList();
        }

        private void CambiarEstado(
            bool procesando)
        {
            _btnGuardar.Enabled =
                !procesando;

            _btnCancelar.Enabled =
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
                    "No fue posible guardar el viaje. " +
                    "Verifique la conexión con SQL Server " +
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

            base.Dispose(disposing);
        }
    }
}
