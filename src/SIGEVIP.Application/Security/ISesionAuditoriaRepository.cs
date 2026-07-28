using SIGEVIP.Application.Auditoria;

namespace SIGEVIP.Application.Security
{
    public interface ISesionAuditoriaRepository
    {
        void Registrar(
            AuditoriaRegistro registro);
    }
}