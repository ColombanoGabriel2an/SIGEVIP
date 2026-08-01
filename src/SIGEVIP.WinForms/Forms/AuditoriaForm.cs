using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;
using SIGEVIP.WinForms.Controls;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class AuditoriaForm : Form
    {
        private readonly AuditoriaService _auditoriaService;

        private DateTimePicker _dtpFechaDesde;
        private DateTimePicker _dtpFechaHasta;
        private TextBox _txtUsuario;
        private ComboBox _cmbModulo;
        private ComboBox _cmbAccion;
        private TextBox _txtTextoGeneral;
        private DataGridView _grillaEventos;
        private DataGridView _grillaCambios;
        private Label _lblTotal;
        private Label _lblDetalle;
        private bool _cargandoEventos;
        private bool _cargandoCatalogos;

        public AuditoriaForm(
            AuditoriaService auditoriaService)
        {
            _auditoriaService =
                auditoriaService
                ?? throw new ArgumentNullException(
                    nameof(auditoriaService));

            InicializarFormulario();
            Load += AuditoriaForm_Load;
        }

        private void InicializarFormulario()
        {
            Text = "SIGEVIP - Auditoría";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1180, 720);
            Size = new Size(1320, 850);
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.WhiteSmoke;

            Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        18F,
                        FontStyle.Bold),
                    Location = new Point(24, 18),
                    Text = "Auditoría y control de cambios"
                });

            Controls.Add(
                new Label
                {
                    AutoSize = true,
                    ForeColor = Color.DimGray,
                    Location = new Point(27, 53),
                    Text =
                        "Consulte eventos del sistema y los valores anteriores y nuevos registrados."
                });

            Controls.Add(CrearPanelFiltros());
            Controls.Add(CrearSeparador());

            var btnCerrar =
                new Button
                {
                    Anchor =
                        AnchorStyles.Right |
                        AnchorStyles.Bottom,
                    DialogResult = DialogResult.OK,
                    Location = new Point(1174, 774),
                    Size = new Size(110, 34),
                    Text = "Cerrar",
                    UseVisualStyleBackColor = true
                };

            Controls.Add(btnCerrar);
            CancelButton = btnCerrar;
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
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(24, 82),
                    Size = new Size(1260, 125)
                };

            _dtpFechaDesde = CrearFecha(panel, "Fecha desde", 16);
            _dtpFechaHasta = CrearFecha(panel, "Fecha hasta", 172);
            _txtUsuario = CrearTexto(panel, "Usuario", 328, 145);
            _cmbModulo = CrearCombo(panel, "Módulo", 489, 135);
            _cmbAccion = CrearCombo(panel, "Acción", 640, 135);
            _txtTextoGeneral = CrearTexto(panel, "Texto general", 791, 235);

            _cmbModulo.SelectedIndexChanged +=
                CmbModulo_SelectedIndexChanged;

            var btnBuscar =
                new Button
                {
                    Location = new Point(1044, 39),
                    Size = new Size(96, 31),
                    Text = "Buscar",
                    UseVisualStyleBackColor = true
                };

            var btnLimpiar =
                new Button
                {
                    Location = new Point(1148, 39),
                    Size = new Size(96, 31),
                    Text = "Limpiar",
                    UseVisualStyleBackColor = true
                };

            _lblTotal =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location = new Point(1044, 87),
                    Text = "Resultados: 0"
                };

            btnBuscar.Click += BtnBuscar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            _txtTextoGeneral.KeyDown += TxtTextoGeneral_KeyDown;

            panel.Controls.Add(btnBuscar);
            panel.Controls.Add(btnLimpiar);
            panel.Controls.Add(_lblTotal);

            return panel;
        }

        private static DateTimePicker CrearFecha(
            Control contenedor,
            string titulo,
            int posicionX)
        {
            contenedor.Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location = new Point(posicionX, 17),
                    Text = titulo
                });

            var control =
                new DateTimePicker
                {
                    Format = DateTimePickerFormat.Short,
                    Location = new Point(posicionX, 42),
                    ShowCheckBox = true,
                    Size = new Size(140, 23)
                };

            control.Checked = false;
            contenedor.Controls.Add(control);
            return control;
        }

        private static TextBox CrearTexto(
            Control contenedor,
            string titulo,
            int posicionX,
            int ancho)
        {
            contenedor.Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location = new Point(posicionX, 17),
                    Text = titulo
                });

            var control =
                new TextBox
                {
                    Location = new Point(posicionX, 42),
                    Size = new Size(ancho, 23)
                };

            contenedor.Controls.Add(control);
            return control;
        }

        private static ComboBox CrearCombo(
            Control contenedor,
            string titulo,
            int posicionX,
            int ancho)
        {
            contenedor.Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location = new Point(posicionX, 17),
                    Text = titulo
                });

            var control =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            posicionX,
                            42),
                    Size =
                        new Size(
                            ancho,
                            23)
                };

            contenedor.Controls.Add(
                control);

            return control;
        }

        private SplitContainer CrearSeparador()
        {
            var separador =
                new SplitContainer
                {
                    Anchor =
                        AnchorStyles.Top |
                        AnchorStyles.Bottom |
                        AnchorStyles.Left |
                        AnchorStyles.Right,
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(24, 222),
                    Orientation = Orientation.Horizontal,
                    Size = new Size(1260, 535),
                    SplitterDistance = 320
                };

            separador.Panel1.Controls.Add(
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        11F,
                        FontStyle.Bold),
                    Location = new Point(10, 9),
                    Text = "Eventos registrados"
                });

            _grillaEventos = CrearGrillaEventos();
            _grillaEventos.Location = new Point(10, 38);
            _grillaEventos.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;
            _grillaEventos.Size =
                new Size(
                    separador.Panel1.Width - 20,
                    separador.Panel1.Height - 48);
            _grillaEventos.SelectionChanged +=
                GrillaEventos_SelectionChanged;
            separador.Panel1.Controls.Add(_grillaEventos);

            _lblDetalle =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        11F,
                        FontStyle.Bold),
                    Location = new Point(10, 9),
                    Text = "Detalle del evento seleccionado"
                };

            _grillaCambios = CrearGrillaCambios();
            _grillaCambios.Location = new Point(10, 38);
            _grillaCambios.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Bottom |
                AnchorStyles.Left |
                AnchorStyles.Right;
            _grillaCambios.Size =
                new Size(
                    separador.Panel2.Width - 20,
                    separador.Panel2.Height - 48);

            separador.Panel2.Controls.Add(_lblDetalle);
            separador.Panel2.Controls.Add(_grillaCambios);
            return separador;
        }

        private static DataGridView CrearGrillaBase()
        {
            return new DataGridView
            {
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect
            };
        }

        private static DataGridView CrearGrillaEventos()
        {
            DataGridView grilla = CrearGrillaBase();

            var fecha =
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "FechaHora",
                    HeaderText = "Fecha y hora",
                    Name = "FechaHora",
                    SortMode =
                        DataGridViewColumnSortMode.Automatic,
                    Width = 145
                };

            fecha.DefaultCellStyle.Format =
                "dd/MM/yyyy HH:mm:ss";
            grilla.Columns.Add(fecha);

            AgregarColumna(grilla, "NombreUsuario", "Usuario", 125);
            AgregarColumna(grilla, "Modulo", "Módulo", 105);
            AgregarColumna(grilla, "Accion", "Acción", 115);
            AgregarColumna(grilla, "Entidad", "Entidad", 105);
            AgregarColumna(grilla, "IdEntidad", "ID entidad", 80);
            AgregarColumna(grilla, "Descripcion", "Descripción", 465);
            return grilla;
        }

        private static DataGridView CrearGrillaCambios()
        {
            DataGridView grilla = CrearGrillaBase();
            AgregarColumna(grilla, "Campo", "Campo", 210);
            AgregarColumna(
                grilla,
                "ValorAnterior",
                "Valor anterior",
                480);
            AgregarColumna(
                grilla,
                "ValorNuevo",
                "Valor nuevo",
                480);
            grilla.CellFormatting +=
                GrillaCambios_CellFormatting;
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
                    DataPropertyName = propiedad,
                    HeaderText = titulo,
                    Name = propiedad,
                    SortMode =
                        DataGridViewColumnSortMode.Automatic,
                    Width = ancho
                });
        }

        private void AuditoriaForm_Load(
            object sender,
            EventArgs e)
        {
            CargarCatalogos();
            Buscar();
        }

        private void BtnBuscar_Click(
            object sender,
            EventArgs e)
        {
            Buscar();
        }

        private void BtnLimpiar_Click(
            object sender,
            EventArgs e)
        {
            _dtpFechaDesde.Checked = false;
            _dtpFechaHasta.Checked = false;
            _txtUsuario.Clear();

            if (_cmbModulo.Items.Count > 0)
            {
                _cmbModulo.SelectedIndex = 0;
            }

            if (_cmbAccion.Items.Count > 0)
            {
                _cmbAccion.SelectedIndex = 0;
            }

            _txtTextoGeneral.Clear();
            Buscar();
        }

        private void CmbModulo_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            if (_cargandoCatalogos)
            {
                return;
            }

            try
            {
                _cargandoCatalogos =
                    true;

                CargarAcciones();
            }
            catch (Exception)
            {
                MostrarError(
                    "No fue posible actualizar las acciones disponibles.");
            }
            finally
            {
                _cargandoCatalogos =
                    false;
            }
        }

        private void CargarCatalogos()
        {
            try
            {
                _cargandoCatalogos =
                    true;

                CargarOpciones(
                    _cmbModulo,
                    _auditoriaService
                        .ListarModulos());

                CargarAcciones();
            }
            catch (AccesoDenegadoException exception)
            {
                MostrarAdvertencia(
                    exception.Message);
            }
            catch (PersistenciaException exception)
            {
                MostrarError(
                    exception.Message);
            }
            catch (Exception)
            {
                MostrarError(
                    "No fue posible cargar los filtros disponibles de auditoría.");
            }
            finally
            {
                _cargandoCatalogos =
                    false;
            }
        }

        private void CargarAcciones()
        {
            CargarOpciones(
                _cmbAccion,
                _auditoriaService
                    .ListarAcciones(
                        ObtenerValorFiltro(
                            _cmbModulo)));
        }

        private static void CargarOpciones(
            ComboBox control,
            IEnumerable<string> valores)
        {
            control.Items.Clear();
            control.Items.Add(
                "Todos");

            if (valores != null)
            {
                foreach (
                    string valor
                    in valores
                        .Where(
                            item =>
                                !string.IsNullOrWhiteSpace(
                                    item))
                        .Distinct(
                            StringComparer
                                .CurrentCultureIgnoreCase))
                {
                    control.Items.Add(
                        valor);
                }
            }

            control.SelectedIndex = 0;
        }

        private static string ObtenerValorFiltro(
            ComboBox control)
        {
            if (control == null ||
                control.SelectedIndex <= 0)
            {
                return string.Empty;
            }

            return Convert.ToString(
                control.SelectedItem)
                ?? string.Empty;
        }

        private void TxtTextoGeneral_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            Buscar();
        }

        private void Buscar()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                AuditoriaFiltro filtro =
                    new AuditoriaFiltro(
                        _dtpFechaDesde.Checked
                            ? (DateTime?)_dtpFechaDesde.Value.Date
                            : null,
                        _dtpFechaHasta.Checked
                            ? (DateTime?)_dtpFechaHasta.Value.Date
                            : null,
                        _txtUsuario.Text,
                        ObtenerValorFiltro(
                            _cmbModulo),
                        ObtenerValorFiltro(
                            _cmbAccion),
                        _txtTextoGeneral.Text);

                IReadOnlyCollection<AuditoriaListadoDto>
                    resultados =
                        _auditoriaService.Listar(filtro);

                _cargandoEventos = true;
                _grillaEventos.DataSource =
                    new SortableBindingList
                        <AuditoriaListadoDto>(
                            resultados.ToList());
                _lblTotal.Text =
                    "Resultados: " + resultados.Count;
                _grillaCambios.DataSource =
                    new SortableBindingList
                        <AuditoriaCambioDto>(
                            new List
                                <AuditoriaCambioDto>());
                _lblDetalle.Text =
                    resultados.Count == 0
                        ? "No se encontraron eventos."
                        : "Seleccione un evento para consultar sus cambios.";
            }
            catch (AccesoDenegadoException exception)
            {
                MostrarAdvertencia(exception.Message);
            }
            catch (ReglaNegocioException exception)
            {
                MostrarAdvertencia(exception.Message);
            }
            catch (PersistenciaException exception)
            {
                MostrarError(exception.Message);
            }
            catch (Exception)
            {
                MostrarError(
                    "No fue posible consultar la auditoría.");
            }
            finally
            {
                _cargandoEventos = false;
                Cursor = Cursors.Default;
            }

            if (_grillaEventos.Rows.Count > 0)
            {
                _grillaEventos.Rows[0].Selected = true;
                CargarCambiosSeleccionados();
            }
        }

        private void GrillaEventos_SelectionChanged(
            object sender,
            EventArgs e)
        {
            if (!_cargandoEventos)
            {
                CargarCambiosSeleccionados();
            }
        }

        private void CargarCambiosSeleccionados()
        {
            AuditoriaListadoDto evento =
                ObtenerEventoSeleccionado();

            if (evento == null)
            {
                _grillaCambios.DataSource =
                    new SortableBindingList
                        <AuditoriaCambioDto>(
                            new List
                                <AuditoriaCambioDto>());
                _lblDetalle.Text =
                    "Seleccione un evento para consultar sus cambios.";
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                IReadOnlyCollection<AuditoriaCambioDto> cambios =
                    _auditoriaService.ObtenerCambios(
                        evento.IdAuditoria);
                _grillaCambios.DataSource =
                    new SortableBindingList
                        <AuditoriaCambioDto>(
                            cambios.ToList());
                _lblDetalle.Text =
                    cambios.Count == 0
                        ? "El evento seleccionado no posee cambios detallados."
                        : "Cambios del evento #" +
                          evento.IdAuditoria +
                          " — " +
                          cambios.Count +
                          " campo(s).";
            }
            catch (AccesoDenegadoException exception)
            {
                MostrarAdvertencia(exception.Message);
            }
            catch (ReglaNegocioException exception)
            {
                MostrarAdvertencia(exception.Message);
            }
            catch (PersistenciaException exception)
            {
                MostrarError(exception.Message);
            }
            catch (Exception)
            {
                MostrarError(
                    "No fue posible consultar el detalle del evento.");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private AuditoriaListadoDto ObtenerEventoSeleccionado()
        {
            if (_grillaEventos.SelectedRows.Count == 0)
            {
                return null;
            }

            return _grillaEventos
                .SelectedRows[0]
                .DataBoundItem
                as AuditoriaListadoDto;
        }

        private static void GrillaCambios_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            var grilla = sender as DataGridView;
            if (grilla == null || e.Value != null)
            {
                return;
            }

            string columna =
                grilla.Columns[e.ColumnIndex].Name;
            if (columna != "ValorAnterior" &&
                columna != "ValorNuevo")
            {
                return;
            }

            e.Value = "(sin valor)";
            e.FormattingApplied = true;
        }

        private static void MostrarAdvertencia(
            string mensaje)
        {
            MessageBox.Show(
                mensaje,
                "Auditoría",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static void MostrarError(
            string mensaje)
        {
            MessageBox.Show(
                mensaje,
                "Error de auditoría",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
