using System;

namespace SIGEVIP.Application.Auditoria
{
    public sealed class AuditoriaListadoDto
    {
        public AuditoriaListadoDto(
            long idAuditoria,
            DateTime fechaHora,
            int idUsuario,
            string nombreUsuario,
            string modulo,
            string accion,
            string entidad,
            int? idEntidad,
            string descripcion)
        {
            IdAuditoria =
                idAuditoria;

            FechaHora =
                fechaHora;

            IdUsuario =
                idUsuario;

            NombreUsuario =
                nombreUsuario;

            Modulo =
                modulo;

            Accion =
                accion;

            Entidad =
                entidad;

            IdEntidad =
                idEntidad;

            Descripcion =
                descripcion;
        }

        public long IdAuditoria
        {
            get;
            private set;
        }

        public DateTime FechaHora
        {
            get;
            private set;
        }

        public int IdUsuario
        {
            get;
            private set;
        }

        public string NombreUsuario
        {
            get;
            private set;
        }

        public string Modulo
        {
            get;
            private set;
        }

        public string Accion
        {
            get;
            private set;
        }

        public string Entidad
        {
            get;
            private set;
        }

        public int? IdEntidad
        {
            get;
            private set;
        }

        public string Descripcion
        {
            get;
            private set;
        }
    }
}