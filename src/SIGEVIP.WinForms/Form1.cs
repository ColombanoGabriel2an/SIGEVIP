using System;
using System.Configuration;
using System.Windows.Forms;
using SIGEVIP.Infrastructure.Data;

namespace SIGEVIP.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BtnProbarConexion_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectionStringSettings settings =
                    ConfigurationManager.ConnectionStrings["SIGEVIP"];

                if (settings == null ||
                    string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "No se encontró la cadena de conexión SIGEVIP.");
                }

                var factory =
                    new SqlConnectionFactory(settings.ConnectionString);

                var healthCheck =
                    new DatabaseHealthCheck(factory);

                lblResultado.Text = healthCheck.Check();
                lblResultado.ForeColor = System.Drawing.Color.DarkGreen;
            }
            catch (Exception ex)
            {
                lblResultado.Text = "Error: " + ex.Message;
                lblResultado.ForeColor = System.Drawing.Color.DarkRed;
            }
        }
    }
}
