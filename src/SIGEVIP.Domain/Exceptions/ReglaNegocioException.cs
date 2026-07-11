using System;

namespace SIGEVIP.Domain.Exceptions
{
    public sealed class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string message)
            : base(message)
        {
        }

        public ReglaNegocioException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
