using System;

namespace SIGEVIP.Application.Security
{
    public sealed class PerfilSesionService
    {
        private readonly IPerfilSesionRepository
            _perfilSesionRepository;

        public PerfilSesionService(
            IPerfilSesionRepository perfilSesionRepository)
        {
            _perfilSesionRepository =
                perfilSesionRepository
                ?? throw new ArgumentNullException(
                    nameof(perfilSesionRepository));
        }

        public PerfilSesion ObtenerPorIdPersona(
            int idPersona)
        {
            if (idPersona <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idPersona),
                    "El identificador de persona debe ser mayor que cero.");
            }

            return _perfilSesionRepository
                .BuscarPorIdPersona(idPersona);
        }
    }
}
