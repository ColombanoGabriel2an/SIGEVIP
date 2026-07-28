using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Application.Exceptions;
using SIGEVIP.Application.Usuarios;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class UsuarioEditForm : Form
    {
        private readonly UsuarioGestionService
            _usuarioService;

        private readonly UsuarioDetalleDto
            _usuario;

        private readonly ErrorProvider
            _errorProvider;

        private Label _lblPersona;
        private ComboBox _cmbPersona;
        private TextBox _txtPersona;

        private TextBox _txtNombreUsuario;

        private Label _lblPassword;
        private TextBox _txtPassword;

        private Label _lblConfirmacionPassword;
        private TextBox _txtConfirmacionPassword;

        private CheckedListBox _lstGrupos;

        private Button _btnGuardar;
        private Button _btnCancelar;

        private bool EsEdicion
        {
            get
            {
                return _usuario != null;
            }
        }

        public UsuarioEditForm(
            UsuarioGestionService usuarioService,
            UsuarioDetalleDto usuario)
        {
            _usuarioService =
                usuarioService
                ?? throw new ArgumentNullException(
                    nameof(usuarioService));

            _usuario =
                usuario;

            _errorProvider =
                new ErrorProvider();

            InicializarFormulario();

            Load +=
                UsuarioEditForm_Load;
        }

        private void InicializarFormulario()
        {
            Text =
                EsEdicion
                    ? "Modificar usuario"
                    : "Nuevo usuario";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(
                    720,
                    EsEdicion
                        ? 530
                        : 650);

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
                        new Point(
                            28,
                            22),
                    Text =
                        EsEdicion
                            ? "Modificar usuario"
                            : "Nuevo usuario"
                };

            var lblAclaracion =
                new Label
                {
                    AutoSize = false,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            31,
                            60),
                    Size =
                        new Size(
                            650,
                            42),
                    Text =
                        EsEdicion
                            ? "La persona asociada no puede modificarse. " +
                              "Los cambios de grupos se aplicarán en la próxima autenticación."
                            : "Seleccione una persona sin usuario, defina la contraseña inicial " +
                              "y asigne al menos un grupo."
                };

            CrearControlesPersona();

            _txtNombreUsuario =
                CrearCampoTexto(
                    "Nombre de usuario",
                    170,
                    100);

            CrearControlesPassword();

            CrearControlesGrupos();

            _btnGuardar =
                new Button
                {
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            436,
                            EsEdicion
                                ? 468
                                : 588),
                    Size =
                        new Size(
                            116,
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
                            562,
                            EsEdicion
                                ? 468
                                : 588),
                    Size =
                        new Size(
                            116,
                            36),
                    Text =
                        "Cancelar",
                    UseVisualStyleBackColor =
                        true
                };

            _btnGuardar.Click +=
                BtnGuardar_Click;

            Controls.Add(
                lblTitulo);

            Controls.Add(
                lblAclaracion);

            Controls.Add(
                _btnGuardar);

            Controls.Add(
                _btnCancelar);

            AcceptButton =
                _btnGuardar;

            CancelButton =
                _btnCancelar;
        }

        private void CrearControlesPersona()
        {
            _lblPersona =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            31,
                            116),
                    Text =
                        "Persona"
                };

            _cmbPersona =
                new ComboBox
                {
                    DropDownStyle =
                        ComboBoxStyle.DropDownList,
                    Location =
                        new Point(
                            205,
                            112),
                    Size =
                        new Size(
                            473,
                            25),
                    Visible =
                        !EsEdicion
                };

            _txtPersona =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            112),
                    ReadOnly = true,
                    Size =
                        new Size(
                            473,
                            25),
                    Visible =
                        EsEdicion
                };

            Controls.Add(
                _lblPersona);

            Controls.Add(
                _cmbPersona);

            Controls.Add(
                _txtPersona);
        }

        private void CrearControlesPassword()
        {
            _lblPassword =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            31,
                            226),
                    Text =
                        "Contraseña inicial",
                    Visible =
                        !EsEdicion
                };

            _txtPassword =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            222),
                    MaxLength = 200,
                    Size =
                        new Size(
                            473,
                            25),
                    UseSystemPasswordChar =
                        true,
                    Visible =
                        !EsEdicion
                };

            _lblConfirmacionPassword =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            31,
                            282),
                    Text =
                        "Confirmar contraseña",
                    Visible =
                        !EsEdicion
                };

            _txtConfirmacionPassword =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            278),
                    MaxLength = 200,
                    Size =
                        new Size(
                            473,
                            25),
                    UseSystemPasswordChar =
                        true,
                    Visible =
                        !EsEdicion
                };

            Controls.Add(
                _lblPassword);

            Controls.Add(
                _txtPassword);

            Controls.Add(
                _lblConfirmacionPassword);

            Controls.Add(
                _txtConfirmacionPassword);
        }

        private void CrearControlesGrupos()
        {
            int posicionEtiqueta =
                EsEdicion
                    ? 230
                    : 342;

            int posicionLista =
                EsEdicion
                    ? 256
                    : 368;

            var lblGrupos =
                new Label
                {
                    AutoSize = true,
                    Font = new Font(
                        "Segoe UI",
                        9F,
                        FontStyle.Bold),
                    Location =
                        new Point(
                            31,
                            posicionEtiqueta),
                    Text =
                        "Grupos directos"
                };

            var lblAclaracionGrupos =
                new Label
                {
                    AutoSize = true,
                    ForeColor =
                        Color.DimGray,
                    Location =
                        new Point(
                            205,
                            posicionEtiqueta),
                    Text =
                        "Marque uno o más grupos."
                };

            _lstGrupos =
                new CheckedListBox
                {
                    CheckOnClick = true,
                    HorizontalScrollbar =
                        true,
                    IntegralHeight =
                        false,
                    Location =
                        new Point(
                            205,
                            posicionLista),
                    Size =
                        new Size(
                            473,
                            175)
                };

            Controls.Add(
                lblGrupos);

            Controls.Add(
                lblAclaracionGrupos);

            Controls.Add(
                _lstGrupos);
        }

        private TextBox CrearCampoTexto(
            string etiqueta,
            int posicionY,
            int longitudMaxima)
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
                        new Point(
                            31,
                            posicionY),
                    Text =
                        etiqueta
                };

            var textBox =
                new TextBox
                {
                    Location =
                        new Point(
                            205,
                            posicionY - 4),
                    MaxLength =
                        longitudMaxima,
                    Size =
                        new Size(
                            473,
                            25)
                };

            Controls.Add(
                label);

            Controls.Add(
                textBox);

            return textBox;
        }

        private void UsuarioEditForm_Load(
            object sender,
            EventArgs e)
        {
            CambiarEstado(
                true);

            try
            {
                CargarPersona();
                CargarGrupos();
                CargarDatosUsuario();

                _txtNombreUsuario.Focus();
            }
            catch (Exception exception)
            {
                MostrarErrorControlado(
                    exception);

                DialogResult =
                    DialogResult.Cancel;

                Close();
            }
            finally
            {
                CambiarEstado(
                    false);
            }
        }

        private void CargarPersona()
        {
            if (EsEdicion)
            {
                _txtPersona.Text =
                    CrearDescripcionPersona(
                        _usuario.NombreCompleto,
                        _usuario.Email);

                return;
            }

            IReadOnlyCollection<PersonaSeleccionUsuarioDto>
                personas =
                    _usuarioService
                        .ListarPersonasDisponibles();

            List<PersonaSeleccionUsuarioDto> lista =
                personas.ToList();

            _cmbPersona.DisplayMember =
                "Descripcion";

            _cmbPersona.ValueMember =
                "IdPersona";

            _cmbPersona.DataSource =
                lista;

            _cmbPersona.SelectedIndex =
                lista.Count > 0
                    ? 0
                    : -1;
        }

        private void CargarGrupos()
        {
            IReadOnlyCollection<int> seleccionados =
                EsEdicion
                    ? _usuario.IdsGrupos
                    : new List<int>()
                        .AsReadOnly();

            IReadOnlyCollection<GrupoSeleccionUsuarioDto>
                grupos =
                    _usuarioService.ListarGrupos(
                        seleccionados);

            _lstGrupos.Items.Clear();

            foreach (
                GrupoSeleccionUsuarioDto grupo
                in grupos)
            {
                _lstGrupos.Items.Add(
                    grupo,
                    grupo.Seleccionado);
            }

            _lstGrupos.DisplayMember =
                "Descripcion";
        }

        private void CargarDatosUsuario()
        {
            if (!EsEdicion)
            {
                return;
            }

            _txtNombreUsuario.Text =
                _usuario.NombreUsuario;
        }

        private void BtnGuardar_Click(
            object sender,
            EventArgs e)
        {
            LimpiarErrores();

            if (!ValidarCampos())
            {
                return;
            }

            CambiarEstado(
                true);

            try
            {
                List<int> idsGrupos =
                    ObtenerIdsGruposSeleccionados();

                if (EsEdicion)
                {
                    _usuarioService.Modificar(
                        new ModificarUsuarioCommand(
                            _usuario.IdUsuario,
                            _txtNombreUsuario.Text,
                            idsGrupos));
                }
                else
                {
                    PersonaSeleccionUsuarioDto persona =
                        ObtenerPersonaSeleccionada();

                    _usuarioService.Registrar(
                        new RegistrarUsuarioCommand(
                            persona.IdPersona,
                            _txtNombreUsuario.Text,
                            _txtPassword.Text,
                            _txtConfirmacionPassword.Text,
                            idsGrupos));
                }

                MessageBox.Show(
                    EsEdicion
                        ? "El usuario fue modificado correctamente."
                        : "El usuario fue registrado correctamente.",
                    "Usuarios",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (ReglaNegocioException exception)
            {
                MostrarError(
                    exception.Message,
                    "Validación",
                    MessageBoxIcon.Warning);
            }
            catch (AccesoDenegadoException exception)
            {
                MostrarError(
                    exception.Message,
                    "Acceso denegado",
                    MessageBoxIcon.Warning);
            }
            catch (PersistenciaException)
            {
                MostrarError(
                    "No fue posible guardar el usuario. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MostrarError(
                    "Ocurrió un error inesperado al guardar el usuario.",
                    "Error",
                    MessageBoxIcon.Error);
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

            if (!EsEdicion &&
                ObtenerPersonaSeleccionada() == null)
            {
                _errorProvider.SetError(
                    _cmbPersona,
                    "Debe seleccionar una persona.");

                valido = false;
            }

            if (string.IsNullOrWhiteSpace(
                _txtNombreUsuario.Text))
            {
                _errorProvider.SetError(
                    _txtNombreUsuario,
                    "El nombre de usuario es obligatorio.");

                valido = false;
            }

            if (!EsEdicion)
            {
                if (string.IsNullOrWhiteSpace(
                    _txtPassword.Text))
                {
                    _errorProvider.SetError(
                        _txtPassword,
                        "La contraseña inicial es obligatoria.");

                    valido = false;
                }
                else if (
                    _txtPassword.Text.Length <
                    UsuarioGestionService
                        .LongitudMinimaPassword)
                {
                    _errorProvider.SetError(
                        _txtPassword,
                        "La contraseña debe contener al menos " +
                        UsuarioGestionService
                            .LongitudMinimaPassword +
                        " caracteres.");

                    valido = false;
                }

                if (!string.Equals(
                    _txtPassword.Text,
                    _txtConfirmacionPassword.Text,
                    StringComparison.Ordinal))
                {
                    _errorProvider.SetError(
                        _txtConfirmacionPassword,
                        "La contraseña y su confirmación no coinciden.");

                    valido = false;
                }
            }

            if (_lstGrupos.CheckedItems.Count == 0)
            {
                _errorProvider.SetError(
                    _lstGrupos,
                    "Debe seleccionar al menos un grupo.");

                valido = false;
            }

            if (!valido)
            {
                MessageBox.Show(
                    "Revise los campos indicados antes de continuar.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return valido;
        }

        private PersonaSeleccionUsuarioDto
            ObtenerPersonaSeleccionada()
        {
            return _cmbPersona.SelectedItem
                as PersonaSeleccionUsuarioDto;
        }

        private List<int>
            ObtenerIdsGruposSeleccionados()
        {
            List<int> ids =
                new List<int>();

            foreach (
                object elemento
                in _lstGrupos.CheckedItems)
            {
                GrupoSeleccionUsuarioDto grupo =
                    elemento
                        as GrupoSeleccionUsuarioDto;

                if (grupo != null)
                {
                    ids.Add(
                        grupo.IdGrupo);
                }
            }

            return ids;
        }

        private void LimpiarErrores()
        {
            _errorProvider.Clear();
        }

        private void CambiarEstado(
            bool procesando)
        {
            _btnGuardar.Enabled =
                !procesando;

            _btnCancelar.Enabled =
                !procesando;

            _cmbPersona.Enabled =
                !procesando &&
                !EsEdicion;

            _txtNombreUsuario.Enabled =
                !procesando;

            _txtPassword.Enabled =
                !procesando;

            _txtConfirmacionPassword.Enabled =
                !procesando;

            _lstGrupos.Enabled =
                !procesando;

            UseWaitCursor =
                procesando;
        }

        private static string CrearDescripcionPersona(
            string nombreCompleto,
            string email)
        {
            return string.IsNullOrWhiteSpace(
                email)
                ? nombreCompleto
                : nombreCompleto +
                  " — " +
                  email;
        }

        private static void MostrarErrorControlado(
            Exception exception)
        {
            if (exception is ReglaNegocioException ||
                exception is AccesoDenegadoException)
            {
                MostrarError(
                    exception.Message,
                    "Usuarios",
                    MessageBoxIcon.Warning);

                return;
            }

            if (exception is PersistenciaException)
            {
                MostrarError(
                    "No fue posible obtener los datos de usuarios. " +
                    "Verifique la conexión con SQL Server " +
                    "e intente nuevamente.",
                    "Error de persistencia",
                    MessageBoxIcon.Error);

                return;
            }

            MostrarError(
                "Ocurrió un error inesperado al cargar el formulario.",
                "Error",
                MessageBoxIcon.Error);
        }

        private static void MostrarError(
            string mensaje,
            string titulo,
            MessageBoxIcon icono)
        {
            MessageBox.Show(
                mensaje,
                titulo,
                MessageBoxButtons.OK,
                icono);
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
