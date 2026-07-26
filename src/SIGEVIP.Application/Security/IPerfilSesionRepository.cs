namespace SIGEVIP.Application.Security
{
    public interface IPerfilSesionRepository
    {
        PerfilSesion BuscarPorIdPersona(int idPersona);
    }
}
