namespace SIGEVIP.WinForms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblServidor;
        private System.Windows.Forms.Button btnProbarConexion;
        private System.Windows.Forms.Label lblResultado;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblServidor = new System.Windows.Forms.Label();
            this.btnProbarConexion = new System.Windows.Forms.Button();
            this.lblResultado = new System.Windows.Forms.Label();
            this.SuspendLayout();

            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font =
                new System.Drawing.Font(
                    "Segoe UI",
                    16F,
                    System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location =
                new System.Drawing.Point(28, 25);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size =
                new System.Drawing.Size(269, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "SIGEVIP - Estado técnico";

            this.lblServidor.AutoSize = true;
            this.lblServidor.Font =
                new System.Drawing.Font("Segoe UI", 10F);
            this.lblServidor.Location =
                new System.Drawing.Point(30, 82);
            this.lblServidor.Name = "lblServidor";
            this.lblServidor.Size =
                new System.Drawing.Size(250, 19);
            this.lblServidor.TabIndex = 1;
            this.lblServidor.Text =
                "Servidor SQL Server: Lenovo_Gabi";

            this.btnProbarConexion.Font =
                new System.Drawing.Font("Segoe UI", 10F);
            this.btnProbarConexion.Location =
                new System.Drawing.Point(33, 124);
            this.btnProbarConexion.Name =
                "btnProbarConexion";
            this.btnProbarConexion.Size =
                new System.Drawing.Size(180, 38);
            this.btnProbarConexion.TabIndex = 2;
            this.btnProbarConexion.Text =
                "Probar conexión";
            this.btnProbarConexion.UseVisualStyleBackColor =
                true;
            this.btnProbarConexion.Click +=
                new System.EventHandler(
                    this.BtnProbarConexion_Click);

            this.lblResultado.AutoSize = false;
            this.lblResultado.Font =
                new System.Drawing.Font("Segoe UI", 10F);
            this.lblResultado.Location =
                new System.Drawing.Point(30, 188);
            this.lblResultado.Name =
                "lblResultado";
            this.lblResultado.Size =
                new System.Drawing.Size(520, 55);
            this.lblResultado.TabIndex = 3;
            this.lblResultado.Text =
                "La conexión todavía no fue comprobada.";

            this.AutoScaleDimensions =
                new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode =
                System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize =
                new System.Drawing.Size(590, 280);
            this.Controls.Add(this.lblResultado);
            this.Controls.Add(this.btnProbarConexion);
            this.Controls.Add(this.lblServidor);
            this.Controls.Add(this.lblTitulo);
            this.Font =
                new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle =
                System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition =
                System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SIGEVIP";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
