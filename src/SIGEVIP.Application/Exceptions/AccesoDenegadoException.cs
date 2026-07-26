using System;

namespace SIGEVIP.Application.Exceptions
{
    public sealed class AccesoDenegadoException
        : Exception
    {
        public AccesoDenegadoException(
            string message)
            : base(message)
        {
        }
    }
}
