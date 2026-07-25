namespace SIGEVIP.WinForms.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer
            components = null;

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkMostrarPassword;
        private System.Windows.Forms.Button btnIniciarSesion;
        private System.Windows.Forms.Button btnSalir;
        private System.Windows.Forms.Label lblEstado;

        protected override void Dispose(
            bool disposing)
        {
            if (disposing &&
                components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitulo =
                new System.Windows.Forms.Label();

            this.lblSubtitulo =
                new System.Windows.Forms.Label();

            this.lblUsuario =
                new System.Windows.Forms.Label();

            this.txtUsuario =
                new System.Windows.Forms.TextBox();

            this.lblPassword =
                new System.Windows.Forms.Label();

            this.txtPassword =
                new System.Windows.Forms.TextBox();

            this.chkMostrarPassword =
                new System.Windows.Forms.CheckBox();

            this.btnIniciarSesion =
                new System.Windows.Forms.Button();

            this.btnSalir =
                new System.Windows.Forms.Button();

            this.lblEstado =
                new System.Windows.Forms.Label();

            this.SuspendLayout();

            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    22F,
                    System.Drawing.FontStyle.Bold);

            this.lblTitulo.Location =
                new System.Drawing.Point(134, 29);

            this.lblTitulo.Name =
                "lblTitulo";

            this.lblTitulo.Size =
                new System.Drawing.Size(135, 41);

            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "SIGEVIP";

            this.lblSubtitulo.AutoSize = true;
            this.lblSubtitulo.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F);

            this.lblSubtitulo.Location =
                new System.Drawing.Point(93, 77);

            this.lblSubtitulo.Name =
                "lblSubtitulo";

            this.lblSubtitulo.Size =
                new System.Drawing.Size(221, 19);

            this.lblSubtitulo.TabIndex = 1;
            this.lblSubtitulo.Text =
                "Sistema de Gestión de Viajes y Viáticos";

            this.lblUsuario.AutoSize = true;
            this.lblUsuario.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblUsuario.Location =
                new System.Drawing.Point(52, 126);

            this.lblUsuario.Name =
                "lblUsuario";

            this.lblUsuario.Size =
                new System.Drawing.Size(50, 15);

            this.lblUsuario.TabIndex = 2;
            this.lblUsuario.Text = "Usuario";

            this.txtUsuario.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F);

            this.txtUsuario.Location =
                new System.Drawing.Point(55, 147);

            this.txtUsuario.MaxLength = 100;
            this.txtUsuario.Name = "txtUsuario";

            this.txtUsuario.Size =
                new System.Drawing.Size(294, 25);

            this.txtUsuario.TabIndex = 0;

            this.lblPassword.AutoSize = true;
            this.lblPassword.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.lblPassword.Location =
                new System.Drawing.Point(52, 190);

            this.lblPassword.Name =
                "lblPassword";

            this.lblPassword.Size =
                new System.Drawing.Size(70, 15);

            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "Contraseña";

            this.txtPassword.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    10F);

            this.txtPassword.Location =
                new System.Drawing.Point(55, 211);

            this.txtPassword.MaxLength = 200;
            this.txtPassword.Name = "txtPassword";

            this.txtPassword.Size =
                new System.Drawing.Size(294, 25);

            this.txtPassword.TabIndex = 1;

            this.txtPassword.UseSystemPasswordChar =
                true;

            this.chkMostrarPassword.AutoSize = true;
            this.chkMostrarPassword.Location =
                new System.Drawing.Point(55, 247);

            this.chkMostrarPassword.Name =
                "chkMostrarPassword";

            this.chkMostrarPassword.Size =
                new System.Drawing.Size(130, 19);

            this.chkMostrarPassword.TabIndex = 2;
            this.chkMostrarPassword.Text =
                "Mostrar contraseña";

            this.chkMostrarPassword
                .UseVisualStyleBackColor = true;

            this.chkMostrarPassword
                .CheckedChanged +=
                new System.EventHandler(
                    this
                        .ChkMostrarPassword_CheckedChanged);

            this.btnIniciarSesion.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F,
                    System.Drawing.FontStyle.Bold);

            this.btnIniciarSesion.Location =
                new System.Drawing.Point(55, 290);

            this.btnIniciarSesion.Name =
                "btnIniciarSesion";

            this.btnIniciarSesion.Size =
                new System.Drawing.Size(184, 38);

            this.btnIniciarSesion.TabIndex = 3;
            this.btnIniciarSesion.Text =
                "Iniciar sesión";

            this.btnIniciarSesion
                .UseVisualStyleBackColor = true;

            this.btnIniciarSesion.Click +=
                new System.EventHandler(
                    this.BtnIniciarSesion_Click);

            this.btnSalir.Location =
                new System.Drawing.Point(249, 290);

            this.btnSalir.Name =
                "btnSalir";

            this.btnSalir.Size =
                new System.Drawing.Size(100, 38);

            this.btnSalir.TabIndex = 4;
            this.btnSalir.Text = "Salir";

            this.btnSalir
                .UseVisualStyleBackColor = true;

            this.btnSalir.Click +=
                new System.EventHandler(
                    this.BtnSalir_Click);

            this.lblEstado.AutoEllipsis = true;
            this.lblEstado.Location =
                new System.Drawing.Point(52, 347);

            this.lblEstado.Name =
                "lblEstado";

            this.lblEstado.Size =
                new System.Drawing.Size(297, 48);

            this.lblEstado.TabIndex = 9;
            this.lblEstado.TextAlign =
                System.Drawing.ContentAlignment.TopCenter;

            this.AcceptButton =
                this.btnIniciarSesion;

            this.AutoScaleDimensions =
                new System.Drawing.SizeF(7F, 15F);

            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;

            this.CancelButton =
                this.btnSalir;

            this.ClientSize =
                new System.Drawing.Size(404, 421);

            this.Controls.Add(this.lblEstado);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.btnIniciarSesion);
            this.Controls.Add(this.chkMostrarPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUsuario);
            this.Controls.Add(this.lblUsuario);
            this.Controls.Add(this.lblSubtitulo);
            this.Controls.Add(this.lblTitulo);

            this.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    9F);

            this.FormBorderStyle =
                System.Windows.Forms
                    .FormBorderStyle.FixedDialog;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";

            this.StartPosition =
                System.Windows.Forms
                    .FormStartPosition.CenterScreen;

            this.Text = "Inicio de sesión - SIGEVIP";

            this.Load +=
                new System.EventHandler(
                    this.LoginForm_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}