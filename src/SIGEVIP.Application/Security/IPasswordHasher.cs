namespace SIGEVIP.Application.Security
{
    public interface IPasswordHasher
    {
        PasswordHashResult CrearHash(string password);

        bool Verificar(
            string password,
            byte[] hashEsperado,
            byte[] salt,
            int iteraciones);
    }
}
