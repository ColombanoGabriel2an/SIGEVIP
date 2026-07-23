using System;

namespace SIGEVIP.Infrastructure.Exceptions
{
    public sealed class PersistenciaException : Exception
    {
        public PersistenciaException(string message)
            : base(message)
        {
        }

        public PersistenciaException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
