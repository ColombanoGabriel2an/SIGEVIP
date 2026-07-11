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
        {
            if (monto <= 0m)
            {
                throw new ReglaNegocioException(
                    "El monto del viático debe ser mayor que cero.");
            }

            IdViatico = idViatico;
            Fecha = fecha.Date;
            Monto = monto;
            Descripcion = descripcion ?? string.Empty;
            Estado = EstadoViatico.Vigente;
        }

        public int IdViatico { get; private set; }

        public int IdViaje { get; private set; }

        public DateTime Fecha { get; private set; }

        public decimal Monto { get; private set; }

        public string Descripcion { get; private set; }

        public EstadoViatico Estado { get; private set; }

        public bool EstaVigente
        {
            get { return Estado == EstadoViatico.Vigente; }
        }

        internal void AsociarAViaje(int idViaje)
        {
            if (IdViaje != 0 && IdViaje != idViaje)
            {
                throw new ReglaNegocioException(
                    "El viático ya pertenece a otro viaje.");
            }

            IdViaje = idViaje;
        }

        internal void Excluir()
        {
            if (Estado == EstadoViatico.Excluido)
            {
                throw new ReglaNegocioException(
                    "El viático ya se encuentra Excluido.");
            }

            Estado = EstadoViatico.Excluido;
        }

        internal void Reactivar()
        {
            if (Estado == EstadoViatico.Vigente)
            {
                throw new ReglaNegocioException(
                    "El viático ya se encuentra Vigente.");
            }

            Estado = EstadoViatico.Vigente;
        }
    }
}
