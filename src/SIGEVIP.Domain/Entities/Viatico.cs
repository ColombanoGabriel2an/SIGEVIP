using System;
using SIGEVIP.Domain.Enums;
using SIGEVIP.Domain.Exceptions;

namespace SIGEVIP.Domain.Entities
{
    public sealed class Viatico
    {
        public Viatico(
            int idViatico,
            DateTime fecha,
            decimal monto,
            string descripcion)
            : this(
                idViatico,
                fecha,
                CategoriaGasto.Otros,
                MetodoPago.EfectivoEmpresa,
                null,
                monto,
                descripcion,
                null)
        {
        }

        public Viatico(
            int idViatico,
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            Persona pagadoPor,
            decimal monto,
            string descripcion,
            Comprobante comprobante)
        {
            ValidarDatos(
                fecha,
                categoria,
                metodoPago,
                pagadoPor,
                monto,
                descripcion,
                comprobante);

            IdViatico = idViatico;
            Fecha = fecha.Date;
            Categoria = categoria;
            MetodoPago = metodoPago;
            PagadoPor = pagadoPor;
            Monto = monto;
            Descripcion =
                NormalizarDescripcion(
                    descripcion);
            Comprobante = comprobante;
            Estado = EstadoViatico.Vigente;
        }

        public int IdViatico { get; private set; }

        public int IdViaje { get; private set; }

        public DateTime Fecha { get; private set; }

        public CategoriaGasto Categoria
        {
            get;
            private set;
        }

        public MetodoPago MetodoPago
        {
            get;
            private set;
        }

        public Persona PagadoPor
        {
            get;
            private set;
        }

        public decimal Monto { get; private set; }

        public string Descripcion { get; private set; }

        public Comprobante Comprobante
        {
            get;
            private set;
        }

        public EstadoViatico Estado
        {
            get;
            private set;
        }

        public string MotivoExclusion
        {
            get;
            private set;
        }

        public int? IdUsuarioExclusion
        {
            get;
            private set;
        }

        public DateTime? FechaExclusion
        {
            get;
            private set;
        }

        public int? IdUsuarioReactivacion
        {
            get;
            private set;
        }

        public DateTime? FechaReactivacion
        {
            get;
            private set;
        }

        public static Viatico Reconstruir(
            int idViatico,
            int idViaje,
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            Persona pagadoPor,
            decimal monto,
            string descripcion,
            Comprobante comprobante,
            EstadoViatico estado,
            string motivoExclusion,
            int? idUsuarioExclusion,
            DateTime? fechaExclusion,
            int? idUsuarioReactivacion,
            DateTime? fechaReactivacion)
        {
            if (idViatico <= 0)
            {
                throw new ReglaNegocioException(
                    "El identificador persistido del viático debe ser válido.");
            }

            if (idViaje <= 0)
            {
                throw new ReglaNegocioException(
                    "El identificador persistido del viaje debe ser válido.");
            }

            if (!Enum.IsDefined(
                typeof(EstadoViatico),
                estado))
            {
                throw new ReglaNegocioException(
                    "El estado persistido del viático no es válido.");
            }

            ValidarAuditoriaReconstruida(
                estado,
                motivoExclusion,
                idUsuarioExclusion,
                fechaExclusion,
                idUsuarioReactivacion,
                fechaReactivacion);

            var viatico =
                new Viatico(
                    idViatico,
                    fecha,
                    categoria,
                    metodoPago,
                    pagadoPor,
                    monto,
                    descripcion,
                    comprobante);

            viatico.AsociarAViaje(
                idViaje);

            viatico.Estado =
                estado;

            viatico.MotivoExclusion =
                NormalizarTextoOpcional(
                    motivoExclusion);

            viatico.IdUsuarioExclusion =
                idUsuarioExclusion;

            viatico.FechaExclusion =
                fechaExclusion;

            viatico.IdUsuarioReactivacion =
                idUsuarioReactivacion;

            viatico.FechaReactivacion =
                fechaReactivacion;

            return viatico;
        }

        public bool EstaVigente
        {
            get
            {
                return Estado ==
                       EstadoViatico.Vigente;
            }
        }

        public bool TieneComprobante
        {
            get
            {
                return Comprobante != null;
            }
        }

        internal void AsociarAViaje(
            int idViaje)
        {
            if (idViaje <= 0)
            {
                throw new ReglaNegocioException(
                    "El viático debe asociarse a un viaje válido.");
            }

            if (IdViaje != 0 &&
                IdViaje != idViaje)
            {
                throw new ReglaNegocioException(
                    "El viático ya pertenece a otro viaje.");
            }

            IdViaje = idViaje;
        }

        internal void Modificar(
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            Persona pagadoPor,
            decimal monto,
            string descripcion,
            Comprobante comprobante)
        {
            if (Estado != EstadoViatico.Vigente)
            {
                throw new ReglaNegocioException(
                    "No se puede modificar un viático Excluido.");
            }

            ValidarDatos(
                fecha,
                categoria,
                metodoPago,
                pagadoPor,
                monto,
                descripcion,
                comprobante);

            Fecha = fecha.Date;
            Categoria = categoria;
            MetodoPago = metodoPago;
            PagadoPor = pagadoPor;
            Monto = monto;
            Descripcion =
                NormalizarDescripcion(
                    descripcion);
            Comprobante = comprobante;
        }

        internal void Excluir()
        {
            if (Estado ==
                EstadoViatico.Excluido)
            {
                throw new ReglaNegocioException(
                    "El viático ya se encuentra Excluido.");
            }

            Estado = EstadoViatico.Excluido;
        }

        internal void Excluir(
            string motivo,
            int idUsuario,
            DateTime fecha)
        {
            if (Estado ==
                EstadoViatico.Excluido)
            {
                throw new ReglaNegocioException(
                    "El viático ya se encuentra Excluido.");
            }

            if (string.IsNullOrWhiteSpace(
                motivo))
            {
                throw new ReglaNegocioException(
                    "El motivo de exclusión es obligatorio.");
            }

            ValidarUsuarioAuditoria(
                idUsuario);

            ValidarFechaAuditoria(
                fecha);

            Estado = EstadoViatico.Excluido;
            MotivoExclusion = motivo.Trim();
            IdUsuarioExclusion = idUsuario;
            FechaExclusion = fecha;
        }

        internal void Reactivar()
        {
            if (Estado ==
                EstadoViatico.Vigente)
            {
                throw new ReglaNegocioException(
                    "El viático ya se encuentra Vigente.");
            }

            Estado = EstadoViatico.Vigente;
        }

        internal void Reactivar(
            int idUsuario,
            DateTime fecha)
        {
            if (Estado ==
                EstadoViatico.Vigente)
            {
                throw new ReglaNegocioException(
                    "El viático ya se encuentra Vigente.");
            }

            ValidarUsuarioAuditoria(
                idUsuario);

            ValidarFechaAuditoria(
                fecha);

            Estado = EstadoViatico.Vigente;
            IdUsuarioReactivacion = idUsuario;
            FechaReactivacion = fecha;
        }

        private static void ValidarDatos(
            DateTime fecha,
            CategoriaGasto categoria,
            MetodoPago metodoPago,
            Persona pagadoPor,
            decimal monto,
            string descripcion,
            Comprobante comprobante)
        {
            if (fecha == DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La fecha del viático es obligatoria.");
            }

            if (!Enum.IsDefined(
                typeof(CategoriaGasto),
                categoria))
            {
                throw new ReglaNegocioException(
                    "La categoría de gasto indicada no es válida.");
            }

            if (!Enum.IsDefined(
                typeof(MetodoPago),
                metodoPago))
            {
                throw new ReglaNegocioException(
                    "El método de pago indicado no es válido.");
            }

            ValidarPagadoPor(
                metodoPago,
                pagadoPor);

            if (monto <= 0m)
            {
                throw new ReglaNegocioException(
                    "El monto del viático debe ser mayor que cero.");
            }

            if (comprobante == null &&
                string.IsNullOrWhiteSpace(
                    descripcion))
            {
                throw new ReglaNegocioException(
                    "Debe justificar en el detalle el gasto que no posee comprobante.");
            }
        }

        private static void ValidarPagadoPor(
            MetodoPago metodoPago,
            Persona pagadoPor)
        {
            bool requierePagador =
                metodoPago ==
                    MetodoPago.PagoPersonal ||
                metodoPago ==
                    MetodoPago.TarjetaCorporativa;

            if (requierePagador)
            {
                if (pagadoPor == null ||
                    pagadoPor.IdPersona <= 0)
                {
                    throw new ReglaNegocioException(
                        "Debe indicar una persona pagadora válida para el método de pago seleccionado.");
                }

                return;
            }

            if (pagadoPor != null)
            {
                throw new ReglaNegocioException(
                    "El método de pago seleccionado no admite una persona pagadora.");
            }
        }

        private static void
            ValidarAuditoriaReconstruida(
                EstadoViatico estado,
                string motivoExclusion,
                int? idUsuarioExclusion,
                DateTime? fechaExclusion,
                int? idUsuarioReactivacion,
                DateTime? fechaReactivacion)
        {
            if (estado ==
                EstadoViatico.Excluido)
            {
                if (string.IsNullOrWhiteSpace(
                    motivoExclusion))
                {
                    throw new ReglaNegocioException(
                        "El viático Excluido debe conservar su motivo de exclusión.");
                }

                if (!idUsuarioExclusion.HasValue ||
                    idUsuarioExclusion.Value <= 0)
                {
                    throw new ReglaNegocioException(
                        "El viático Excluido debe conservar un usuario de exclusión válido.");
                }

                if (!fechaExclusion.HasValue ||
                    fechaExclusion.Value ==
                        DateTime.MinValue)
                {
                    throw new ReglaNegocioException(
                        "El viático Excluido debe conservar su fecha de exclusión.");
                }
            }

            ValidarParAuditoriaOpcional(
                idUsuarioReactivacion,
                fechaReactivacion,
                "reactivación");
        }

        private static void
            ValidarParAuditoriaOpcional(
                int? idUsuario,
                DateTime? fecha,
                string operacion)
        {
            bool tieneUsuario =
                idUsuario.HasValue;

            bool tieneFecha =
                fecha.HasValue;

            if (tieneUsuario != tieneFecha)
            {
                throw new ReglaNegocioException(
                    "Los datos de auditoría de " +
                    operacion +
                    " deben informarse de forma completa.");
            }

            if (tieneUsuario &&
                idUsuario.Value <= 0)
            {
                throw new ReglaNegocioException(
                    "El usuario de auditoría de " +
                    operacion +
                    " debe ser válido.");
            }

            if (tieneFecha &&
                fecha.Value ==
                    DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La fecha de auditoría de " +
                    operacion +
                    " debe ser válida.");
            }
        }

        private static string
            NormalizarTextoOpcional(
                string valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                    ? null
                    : valor.Trim();
        }

        private static string
            NormalizarDescripcion(
                string descripcion)
        {
            return string.IsNullOrWhiteSpace(
                descripcion)
                    ? string.Empty
                    : descripcion.Trim();
        }

        private static void
            ValidarUsuarioAuditoria(
                int idUsuario)
        {
            if (idUsuario <= 0)
            {
                throw new ReglaNegocioException(
                    "El usuario de auditoría debe ser válido.");
            }
        }

        private static void
            ValidarFechaAuditoria(
                DateTime fecha)
        {
            if (fecha == DateTime.MinValue)
            {
                throw new ReglaNegocioException(
                    "La fecha de auditoría es obligatoria.");
            }
        }
    }
}
