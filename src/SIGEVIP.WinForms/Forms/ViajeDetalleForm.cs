using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIGEVIP.Domain.Entities;

namespace SIGEVIP.WinForms.Forms
{
    public sealed class ViajeDetalleForm : Form
    {
        private readonly Viaje
            _viaje;

        public ViajeDetalleForm(
            Viaje viaje)
        {
            _viaje =
                viaje
                ?? throw new ArgumentNullException(
                    nameof(viaje));

            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            Text =
                "Detalle del viaje";

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ClientSize =
                new Size(700, 640);

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
                        new Point(28, 20),
                    Text =
                        "Detalle del viaje"
                });

            int posicionY = 82;

            AgregarDato(
                "Identificador",
                _viaje.IdViaje.ToString(),
                posicionY);

            posicionY += 42;

            AgregarDato(
                "Descripción",
                _viaje.Descripcion,
                posicionY);

            posicionY += 64;

            AgregarDato(
                "Tipo",
                _viaje.TipoViaje.ToString(),
                posicionY);

            posicionY += 42;

            AgregarDato(
                "Fecha de inicio",
                _viaje.FechaInicio
                    .ToString("dd/MM/yyyy"),
                posicionY);

            posicionY += 42;

            AgregarDato(
                "Fecha de fin",
                _viaje.FechaFin
                    .ToString("dd/MM/yyyy"),
                posicionY);

            posicionY += 42;

            AgregarDato(
                "Estado",
                _viaje.EstadoActual.ToString(),
                posicionY);

            posicionY += 42;

            AgregarDato(
                "Monto anticipado",
                _viaje.MontoAnticipado
                    .ToString("N2"),
                posicionY);

            posicionY += 52;

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
                        "Participantes"
                });

            var participantes =
                new ListBox
                {
                    Location =
                        new Point(185, posicionY - 4),
                    Size =
                        new Size(470, 100)
                };

            participantes.Items.AddRange(
                _viaje.Participantes
                    .Select(
                        persona =>
                            persona.Apellido +
                            ", " +
                            persona.Nombre +
                            (
                                persona.Activo
                                    ? string.Empty
                                    : " (inactivo)"
                            ))
                    .Cast<object>()
                    .ToArray());

            Controls.Add(
                participantes);

            var lblPendiente =
                new Label
                {
                    AutoSize = false,
                    BorderStyle =
                        BorderStyle.FixedSingle,
                    Location =
                        new Point(30, 520),
                    Padding =
                        new Padding(10),
                    Size =
                        new Size(625, 42),
                    Text =
                        "Total gastado y saldo: disponibles al incorporar la persistencia de viáticos.",
                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            var btnCerrar =
                new Button
                {
                    DialogResult =
                        DialogResult.OK,
                    Location =
                        new Point(545, 580),
                    Size =
                        new Size(110, 34),
                    Text =
                        "Cerrar",
                    UseVisualStyleBackColor =
                        true
                };

            Controls.Add(
                lblPendiente);

            Controls.Add(
                btnCerrar);

            AcceptButton =
                btnCerrar;

            CancelButton =
                btnCerrar;
        }

        private void AgregarDato(
            string etiqueta,
            string valor,
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
                        etiqueta
                });

            Controls.Add(
                new Label
                {
                    AutoEllipsis = true,
                    Location =
                        new Point(185, posicionY),
                    Size =
                        new Size(470, 42),
                    Text =
                        valor ?? string.Empty
                });
        }
    }
}
