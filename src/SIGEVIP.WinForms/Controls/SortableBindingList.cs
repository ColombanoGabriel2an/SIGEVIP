using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SIGEVIP.WinForms.Controls
{
    public sealed class SortableBindingList<T>
        : BindingList<T>
    {
        private bool _estaOrdenada;
        private PropertyDescriptor
            _propiedadOrdenada;
        private ListSortDirection
            _direccionOrden;

        public SortableBindingList(
            IList<T> elementos)
            : base(
                elementos
                ?? throw new ArgumentNullException(
                    nameof(elementos)))
        {
        }

        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        protected override bool IsSortedCore
        {
            get
            {
                return _estaOrdenada;
            }
        }

        protected override PropertyDescriptor
            SortPropertyCore
        {
            get
            {
                return _propiedadOrdenada;
            }
        }

        protected override ListSortDirection
            SortDirectionCore
        {
            get
            {
                return _direccionOrden;
            }
        }

        protected override void ApplySortCore(
            PropertyDescriptor propiedad,
            ListSortDirection direccion)
        {
            if (propiedad == null)
            {
                throw new ArgumentNullException(
                    nameof(propiedad));
            }

            List<T> elementos =
                Items as List<T>;

            if (elementos == null)
            {
                return;
            }

            elementos.Sort(
                (izquierdo, derecho) =>
                    Comparar(
                        propiedad.GetValue(
                            izquierdo),
                        propiedad.GetValue(
                            derecho),
                        direccion));

            _propiedadOrdenada =
                propiedad;

            _direccionOrden =
                direccion;

            _estaOrdenada =
                true;

            ResetBindings();
        }

        protected override void RemoveSortCore()
        {
            _estaOrdenada =
                false;

            _propiedadOrdenada =
                null;
        }

        private static int Comparar(
            object izquierdo,
            object derecho,
            ListSortDirection direccion)
        {
            int resultado;

            if (ReferenceEquals(
                izquierdo,
                derecho))
            {
                resultado = 0;
            }
            else if (izquierdo == null)
            {
                resultado = -1;
            }
            else if (derecho == null)
            {
                resultado = 1;
            }
            else
            {
                string textoIzquierdo =
                    izquierdo as string;

                string textoDerecho =
                    derecho as string;

                if (textoIzquierdo != null &&
                    textoDerecho != null)
                {
                    resultado =
                        StringComparer
                            .CurrentCultureIgnoreCase
                            .Compare(
                                textoIzquierdo,
                                textoDerecho);
                }
                else
                {
                    resultado =
                        Comparer
                            .DefaultInvariant
                            .Compare(
                                izquierdo,
                                derecho);
                }
            }

            return direccion ==
                    ListSortDirection.Ascending
                ? resultado
                : -resultado;
        }
    }
}
