using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Viaticos;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ViaticoEditForm : Form
    {
        private readonly ViaticoService
            _viaticoService;

        private readonly int
            _idViaje;

        private readonly DateTime
            _fechaInicioViaje;

        private readonly DateTime
            _fechaFinViaje;

        private readonly string
            _descripcionViaje;

        private readonly Viatico
            _viatico;

        private readonly ErrorProvider
            _errorProvider;

        private DateTimePicker _dtpFecha;
        private ComboBox _cmbCategoria;
        private ComboBox _cmbMetodoPago;
        private ComboBox _cmbPagador;
        private NumericUpDown _nudMonto;
        private TextBox _txtDescripcion;

        private CheckBox _chkTieneComprobante;
        private GroupBox _grpComprobante;
        private ComboBox _cmbTipoComprobante;
        private TextBox _txtCuitProveedor;
        private TextBox _txtRazonSocial;
        private ComboBox _cmbSituacionFiscal;
        private TextBox _txtSucursal;
        private TextBox _txtNumero;
        private NumericUpDown _nudMontoGravado;
        private NumericUpDown _nudMontoImpuestos;
        private Label _lblTotalComprobante;

        private Button _btnGuardar;
        private Button _btnCancelar;

        private bool EsEdicion
        {
            get
            {
                return _viatico != null;
            }
        }

        public ViaticoEditForm(
            ViaticoService viaticoService,
            int idViaje,
            DateTime fechaInicioViaje,
            DateTime fechaFinViaje,
            string descripcionViaje,
            Viatico viatico)
        {
            _viaticoService =
                viaticoService
                ?? throw new ArgumentNullException(
                    nameof(viaticoService));

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

            if (_fechaInicioViaje >
                _fechaFinViaje)
            {
                throw new ArgumentException(
                    "Las fechas del viaje no son válidas.");
            }

            _descripcionViaje =
                descripcionViaje
                ?? string.Empty;

            _viatico =
                viatico;

            if (_viatico != null &&
                _viatico.IdViaje !=
                    _idViaje)
            {
                throw new ArgumentException(
                    "El viático no pertenece al viaje indicado.",
                    nameof(viatico));
            }

            _errorProvider =
                new ErrorProvider();

            InicializarFormulario();

            Load +=
                ViaticoEditForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                EsEdicion
                    ? "SIGEVIP - Modificar viático"
                    : "SIGEVIP - Nuevo viático";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(
                    910,
                    750);

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            BackColor =
                Color.WhiteSmoke;

            _errorProvider.ContainerControl =
                this;

            Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            16F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            24,
                            18),
                    Text =
                        EsEdicion
                            ? "Modificar viático"
                            : "Registrar viático"
                });

            Controls.Add(
                new Label
                {
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
                            25,
                            58),
                    Padding =
                        new Padding(10),
                    Size =
                        new Size(
                            860,
                            52),
                    Text =
                        "Viaje " +
                        _idViaje +
                        " | " +
                        _fechaInicioViaje
                            .ToString("dd/MM/yyyy") +
                        " al " +
                        _fechaFinViaje
                            .ToString("dd/MM/yyyy") +
                        " | " +
                        _descripcionViaje,
                    TextAlign =
                        ContentAlignment.MiddleLeft
                });

            CrearDatosViatico();
            CrearDatosComprobante();
            CrearAcciones();
        }

        private void CrearDatosViatico()
        {
            var grupo =
                new GroupBox
                {
                    Location =
                        new Point(
                            25,
                            125),
                    Size =
                        new Size(
                            860,
                            235),
                    Text =
                        "Datos del viático"
                };

            grupo.Controls.Add(
                CrearEtiqueta(
                    "Fecha",
                    18,
                    30));

            _dtpFecha =
                new DateTimePicker
                {
                    Format =
                        DateTimePickerFormat.Short,
                    Location =
                        new Point(
                            18,
                            54),
                    MinDate =
                        _fechaInicioViaje,
                    MaxDate =
                        _fechaFinViaje,
                    Size =
                        new Size(
                            170,
                            24)
                };

            grupo.Controls.Add(
                _dtpFecha);

            grupo.Controls.Add(
                CrearEtiqueta(
                    "Categoría",
                    215,
                    30));

            _cmbCategoria =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            215,
                            54),
                    Size =
                        new Size(
                            180,
                            24)
                };

            _cmbCategoria.DataSource =
                Enum.GetValues(
                    typeof(CategoriaGasto));

            grupo.Controls.Add(
                _cmbCategoria);

            grupo.Controls.Add(
                CrearEtiqueta(
                    "Método de pago",
                    420,
                    30));

            _cmbMetodoPago =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            420,
                            54),
                    Size =
                        new Size(
                            190,
                            24)
                };

            _cmbMetodoPago.DataSource =
                Enum.GetValues(
                    typeof(MetodoPago));

            _cmbMetodoPago.SelectedIndexChanged +=
                CmbMetodoPago_SelectedIndexChanged;

            grupo.Controls.Add(
                _cmbMetodoPago);

            grupo.Controls.Add(
                CrearEtiqueta(
                    "Monto",
                    635,
                    30));

            _nudMonto =
                CrearMonto(
                    635,
                    54,
                    190);

            grupo.Controls.Add(
                _nudMonto);

            grupo.Controls.Add(
                CrearEtiqueta(
                    "Persona pagadora",
                    18,
                    96));

            _cmbPagador =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    DisplayMember =
                        "NombreCompleto",
                    Location =
                        new Point(
                            18,
                            120),
                    Size =
                        new Size(
                            377,
                            24)
                };

            grupo.Controls.Add(
                _cmbPagador);

            grupo.Controls.Add(
                CrearEtiqueta(
                    "Descripción o justificación",
                    420,
                    96));

            _txtDescripcion =
                new TextBox
                {
                    AcceptsReturn = true,
                    Location =
                        new Point(
                            420,
                            120),
                    MaxLength = 1000,
                    Multiline = true,
                    ScrollBars =
                        ScrollBars.Vertical,
                    Size =
                        new Size(
                            405,
                            78)
                };

            grupo.Controls.Add(
                _txtDescripcion);

            _chkTieneComprobante =
                new CheckBox
                {
                    AutoSize = true,
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            18,
                            174),
                    Text =
                        "El viático posee comprobante"
                };

            _chkTieneComprobante.CheckedChanged +=
                ChkTieneComprobante_CheckedChanged;

            grupo.Controls.Add(
                _chkTieneComprobante);

            Controls.Add(
                grupo);
        }

        private void CrearDatosComprobante()
        {
            _grpComprobante =
                new GroupBox
                {
                    Location =
                        new Point(
                            25,
                            372),
                    Size =
                        new Size(
                            860,
                            300),
                    Text =
                        "Comprobante"
                };

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "Tipo",
                    18,
                    30));

            _cmbTipoComprobante =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            18,
                            54),
                    Size =
                        new Size(
                            180,
                            24)
                };

            _cmbTipoComprobante.DataSource =
                Enum.GetValues(
                    typeof(TipoComprobante));

            _grpComprobante.Controls.Add(
                _cmbTipoComprobante);

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "CUIT del proveedor",
                    220,
                    30));

            _txtCuitProveedor =
                new TextBox
                {
                    Location =
                        new Point(
                            220,
                            54),
                    MaxLength = 20,
                    Size =
                        new Size(
                            190,
                            24)
                };

            _grpComprobante.Controls.Add(
                _txtCuitProveedor);

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "Razón social",
                    432,
                    30));

            _txtRazonSocial =
                new TextBox
                {
                    Location =
                        new Point(
                            432,
                            54),
                    MaxLength = 200,
                    Size =
                        new Size(
                            393,
                            24)
                };

            _grpComprobante.Controls.Add(
                _txtRazonSocial);

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "Situación fiscal",
                    18,
                    96));

            _cmbSituacionFiscal =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            18,
                            120),
                    Size =
                        new Size(
                            190,
                            24)
                };

            _cmbSituacionFiscal.DataSource =
                Enum.GetValues(
                    typeof(SituacionFiscal));

            _grpComprobante.Controls.Add(
                _cmbSituacionFiscal);

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "Sucursal (4 dígitos)",
                    230,
                    96));

            _txtSucursal =
                new TextBox
                {
                    Location =
                        new Point(
                            230,
                            120),
                    MaxLength = 4,
                    Size =
                        new Size(
                            150,
                            24)
                };

            _txtSucursal.KeyPress +=
                SoloNumeros_KeyPress;

            _grpComprobante.Controls.Add(
                _txtSucursal);

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "Número (8 dígitos)",
                    402,
                    96));

            _txtNumero =
                new TextBox
                {
                    Location =
                        new Point(
                            402,
                            120),
                    MaxLength = 8,
                    Size =
                        new Size(
                            170,
                            24)
                };

            _txtNumero.KeyPress +=
                SoloNumeros_KeyPress;

            _grpComprobante.Controls.Add(
                _txtNumero);

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "Monto gravado",
                    18,
                    166));

            _nudMontoGravado =
                CrearMonto(
                    18,
                    190,
                    190);

            _nudMontoGravado.ValueChanged +=
                MontoComprobante_ValueChanged;

            _grpComprobante.Controls.Add(
                _nudMontoGravado);

            _grpComprobante.Controls.Add(
                CrearEtiqueta(
                    "Impuestos",
                    230,
                    166));

            _nudMontoImpuestos =
                CrearMonto(
                    230,
                    190,
                    190);

            _nudMontoImpuestos.ValueChanged +=
                MontoComprobante_ValueChanged;

            _grpComprobante.Controls.Add(
                _nudMontoImpuestos);

            _lblTotalComprobante =
                new Label
                {
                    BackColor =
                        Color.White,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Font =
                        new Font(
                            "Segoe UI",
                            10F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            450,
                            183),
                    Padding =
                        new Padding(10),
                    Size =
                        new Size(
                            375,
                            44),
                    Text =
                        "Total del comprobante: 0,00",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            _grpComprobante.Controls.Add(
                _lblTotalComprobante);

            _grpComprobante.Controls.Add(
                new Label
                {
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            18,
                            245),
                    Size =
                        new Size(
                            807,
                            38),
                    Text =
                        "El total se calcula automáticamente como monto gravado más impuestos. " +
                        "No es obligatorio que coincida con el monto del viático."
                });

            Controls.Add(
                _grpComprobante);
        }

        private void CrearAcciones()
        {
            _btnGuardar =
                new Button
                {
                    Font =
                        new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold),
                    Location =
                        new Point(
                            635,
                            692),
                    Size =
                        new Size(
                            120,
                            36),
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
                        new Point(
                            765,
                            692),
                    Size =
                        new Size(
                            120,
                            36),
                    Text =
                        "Cancelar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnGuardar.Click +=
                BtnGuardar_Click;

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
                Font =
                    new Font(
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

        private static NumericUpDown CrearMonto(
            int posicionX,
            int posicionY,
            int ancho)
        {
            return new NumericUpDown
            {
                DecimalPlaces = 2,
                Increment = 100m,
                Maximum =
                    9999999999999999m,
                Minimum = 0m,
                ThousandsSeparator = true,
                Location =
                    new Point(
                        posicionX,
                        posicionY),
                Size =
                    new Size(
                        ancho,
                        24)
            };
        }

        private void ViaticoEditForm_Load(
            object sender,
            EventArgs e)
        {
            CargarPagadores();
        }

        private void CargarPagadores()
        {
            CambiarEstado(
                true);

            try
            {
                List<PagadorSeleccionDto> pagadores =
                    _viaticoService
                        .ListarPagadoresDisponibles()
                        .Where(
                            item =>
                                item.Activo)
                        .OrderBy(
                            item =>
                                item.NombreCompleto)
                        .ThenBy(
                            item =>
                                item.IdPersona)
                        .ToList();

                pagadores.Insert(
                    0,
                    new PagadorSeleccionDto(
                        0,
                        "Seleccione una persona",
                        true));

                _cmbPagador.DataSource =
                    null;

                _cmbPagador.DataSource =
                    pagadores;

                _cmbPagador.DisplayMember =
                    "NombreCompleto";

                CargarDatosIniciales();
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

        private void CargarDatosIniciales()
        {
            _dtpFecha.Value =
                ObtenerFechaInicial();

            _cmbCategoria.SelectedItem =
                CategoriaGasto.Otros;

            _cmbMetodoPago.SelectedItem =
                MetodoPago.EfectivoEmpresa;

            _cmbTipoComprobante.SelectedItem =
                TipoComprobante.FacturaB;

            _cmbSituacionFiscal.SelectedItem =
                SituacionFiscal.NoInformada;

            _chkTieneComprobante.Checked =
                false;

            if (!EsEdicion)
            {
                ActualizarEstadoPagador();
                ActualizarEstadoComprobante();
                ActualizarTotalComprobante();
                return;
            }

            _dtpFecha.Value =
                _viatico.Fecha;

            _cmbCategoria.SelectedItem =
                _viatico.Categoria;

            _cmbMetodoPago.SelectedItem =
                _viatico.MetodoPago;

            SeleccionarPagadorActual();

            _nudMonto.Value =
                LimitarValor(
                    _viatico.Monto,
                    _nudMonto);

            _txtDescripcion.Text =
                _viatico.Descripcion;

            Comprobante comprobante =
                _viatico.Comprobante;

            _chkTieneComprobante.Checked =
                comprobante != null;

            if (comprobante != null)
            {
                _cmbTipoComprobante.SelectedItem =
                    comprobante.Tipo;

                _txtCuitProveedor.Text =
                    comprobante.CuitProveedor;

                _txtRazonSocial.Text =
                    comprobante
                        .RazonSocialProveedor;

                _cmbSituacionFiscal.SelectedItem =
                    comprobante
                        .SituacionFiscal;

                _txtSucursal.Text =
                    comprobante.Sucursal;

                _txtNumero.Text =
                    comprobante.Numero;

                _nudMontoGravado.Value =
                    LimitarValor(
                        comprobante.MontoGravado,
                        _nudMontoGravado);

                _nudMontoImpuestos.Value =
                    LimitarValor(
                        comprobante.MontoImpuestos,
                        _nudMontoImpuestos);
            }

            ActualizarEstadoPagador();
            ActualizarEstadoComprobante();
            ActualizarTotalComprobante();
        }

        private DateTime ObtenerFechaInicial()
        {
            if (EsEdicion)
            {
                return _viatico.Fecha;
            }

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

        private void SeleccionarPagadorActual()
        {
            if (_viatico.PagadoPor == null)
            {
                _cmbPagador.SelectedIndex = 0;
                return;
            }

            for (
                int indice = 0;
                indice <
                    _cmbPagador.Items.Count;
                indice++)
            {
                PagadorSeleccionDto item =
                    _cmbPagador.Items[indice]
                        as PagadorSeleccionDto;

                if (item != null &&
                    item.IdPersona ==
                        _viatico.PagadoPor.IdPersona)
                {
                    _cmbPagador.SelectedIndex =
                        indice;

                    return;
                }
            }

            _cmbPagador.SelectedIndex = 0;
        }

        private void CmbMetodoPago_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ActualizarEstadoPagador();
        }

        private void ActualizarEstadoPagador()
        {
            MetodoPago? metodo =
                _cmbMetodoPago.SelectedItem
                    as MetodoPago?;

            bool requierePagador =
                metodo.HasValue &&
                (
                    metodo.Value ==
                        MetodoPago.PagoPersonal ||
                    metodo.Value ==
                        MetodoPago.TarjetaCorporativa
                );

            _cmbPagador.Enabled =
                requierePagador;

            if (!requierePagador &&
                _cmbPagador.Items.Count > 0)
            {
                _cmbPagador.SelectedIndex = 0;
            }
        }

        private void ChkTieneComprobante_CheckedChanged(
            object sender,
            EventArgs e)
        {
            ActualizarEstadoComprobante();
        }

        private void ActualizarEstadoComprobante()
        {
            _grpComprobante.Enabled =
                _chkTieneComprobante.Checked;
        }

        private void MontoComprobante_ValueChanged(
            object sender,
            EventArgs e)
        {
            ActualizarTotalComprobante();
        }

        private void ActualizarTotalComprobante()
        {
            decimal total =
                _nudMontoGravado.Value +
                _nudMontoImpuestos.Value;

            _lblTotalComprobante.Text =
                "Total del comprobante: " +
                total.ToString("N2");
        }

        private static void SoloNumeros_KeyPress(
            object sender,
            KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            _errorProvider.Clear();

            if (!ValidarCampos())
            {
                MessageBox.Show(
                    "Revise los datos indicados.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            CambiarEstado(
                true);

            try
            {
                CategoriaGasto categoria =
                    (CategoriaGasto)
                        _cmbCategoria.SelectedItem;

                MetodoPago metodoPago =
                    (MetodoPago)
                        _cmbMetodoPago.SelectedItem;

                int? idPagador =
                    ObtenerIdPagador(
                        metodoPago);

                ComprobanteInput comprobante =
                    CrearComprobanteInput();

                if (EsEdicion)
                {
                    _viaticoService.Modificar(
                        new ModificarViaticoCommand(
                            _idViaje,
                            _viatico.IdViatico,
                            _dtpFecha.Value.Date,
                            categoria,
                            metodoPago,
                            idPagador,
                            _nudMonto.Value,
                            _txtDescripcion.Text,
                            comprobante));
                }
                else
                {
                    _viaticoService.Registrar(
                        new RegistrarViaticoCommand(
                            _idViaje,
                            _dtpFecha.Value.Date,
                            categoria,
                            metodoPago,
                            idPagador,
                            _nudMonto.Value,
                            _txtDescripcion.Text,
                            comprobante));
                }

                MessageBox.Show(
                    EsEdicion
                        ? "El viático fue modificado correctamente."
                        : "El viático fue registrado correctamente.",
                    "Viáticos",
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

            if (_cmbCategoria.SelectedItem == null)
            {
                _errorProvider.SetError(
                    _cmbCategoria,
                    "Seleccione una categoría.");

                valido = false;
            }

            if (_cmbMetodoPago.SelectedItem == null)
            {
                _errorProvider.SetError(
                    _cmbMetodoPago,
                    "Seleccione un método de pago.");

                valido = false;
            }

            if (_nudMonto.Value <= 0m)
            {
                _errorProvider.SetError(
                    _nudMonto,
                    "El monto debe ser mayor que cero.");

                valido = false;
            }

            MetodoPago? metodo =
                _cmbMetodoPago.SelectedItem
                    as MetodoPago?;

            if (metodo.HasValue &&
                (
                    metodo.Value ==
                        MetodoPago.PagoPersonal ||
                    metodo.Value ==
                        MetodoPago.TarjetaCorporativa
                ) &&
                ObtenerPagadorSeleccionado() == null)
            {
                _errorProvider.SetError(
                    _cmbPagador,
                    "Seleccione la persona pagadora.");

                valido = false;
            }

            if (!_chkTieneComprobante.Checked &&
                string.IsNullOrWhiteSpace(
                    _txtDescripcion.Text))
            {
                _errorProvider.SetError(
                    _txtDescripcion,
                    "Debe justificar el gasto que no posee comprobante.");

                valido = false;
            }

            if (_chkTieneComprobante.Checked)
            {
                valido =
                    ValidarComprobante() &&
                    valido;
            }

            return valido;
        }

        private bool ValidarComprobante()
        {
            bool valido = true;

            if (_cmbTipoComprobante
                    .SelectedItem == null)
            {
                _errorProvider.SetError(
                    _cmbTipoComprobante,
                    "Seleccione un tipo de comprobante.");

                valido = false;
            }

            if (string.IsNullOrWhiteSpace(
                _txtCuitProveedor.Text))
            {
                _errorProvider.SetError(
                    _txtCuitProveedor,
                    "El CUIT del proveedor es obligatorio.");

                valido = false;
            }

            if (string.IsNullOrWhiteSpace(
                _txtRazonSocial.Text))
            {
                _errorProvider.SetError(
                    _txtRazonSocial,
                    "La razón social es obligatoria.");

                valido = false;
            }

            if (_cmbSituacionFiscal
                    .SelectedItem == null)
            {
                _errorProvider.SetError(
                    _cmbSituacionFiscal,
                    "Seleccione la situación fiscal.");

                valido = false;
            }

            if (_txtSucursal.Text.Trim().Length !=
                    4 ||
                !_txtSucursal.Text
                    .Trim()
                    .All(char.IsDigit))
            {
                _errorProvider.SetError(
                    _txtSucursal,
                    "La sucursal debe contener exactamente cuatro dígitos.");

                valido = false;
            }

            if (_txtNumero.Text.Trim().Length !=
                    8 ||
                !_txtNumero.Text
                    .Trim()
                    .All(char.IsDigit))
            {
                _errorProvider.SetError(
                    _txtNumero,
                    "El número debe contener exactamente ocho dígitos.");

                valido = false;
            }

            return valido;
        }

        private PagadorSeleccionDto
            ObtenerPagadorSeleccionado()
        {
            PagadorSeleccionDto item =
                _cmbPagador.SelectedItem
                    as PagadorSeleccionDto;

            return item == null ||
                   item.IdPersona <= 0
                ? null
                : item;
        }

        private int? ObtenerIdPagador(
            MetodoPago metodoPago)
        {
            bool requierePagador =
                metodoPago ==
                    MetodoPago.PagoPersonal ||
                metodoPago ==
                    MetodoPago.TarjetaCorporativa;

            if (!requierePagador)
            {
                return null;
            }

            PagadorSeleccionDto pagador =
                ObtenerPagadorSeleccionado();

            return pagador == null
                ? (int?)null
                : pagador.IdPersona;
        }

        private ComprobanteInput
            CrearComprobanteInput()
        {
            if (!_chkTieneComprobante.Checked)
            {
                return null;
            }

            int idComprobante =
                EsEdicion &&
                _viatico.Comprobante != null
                    ? _viatico.Comprobante
                        .IdComprobante
                    : 0;

            return new ComprobanteInput(
                idComprobante,
                (TipoComprobante)
                    _cmbTipoComprobante
                        .SelectedItem,
                _txtCuitProveedor.Text,
                _txtRazonSocial.Text,
                (SituacionFiscal)
                    _cmbSituacionFiscal
                        .SelectedItem,
                _txtSucursal.Text,
                _txtNumero.Text,
                _nudMontoGravado.Value,
                _nudMontoImpuestos.Value);
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

        private static decimal LimitarValor(
            decimal valor,
            NumericUpDown control)
        {
            if (valor < control.Minimum)
            {
                return control.Minimum;
            }

            if (valor > control.Maximum)
            {
                return control.Maximum;
            }

            return valor;
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
                    "Viáticos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is
                PersistenciaException)
            {
                MessageBox.Show(
                    "No fue posible guardar el viático. " +
                    "Verifique la conexión con SQL Server e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                "Ocurrió un error inesperado al guardar el viático.",
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
