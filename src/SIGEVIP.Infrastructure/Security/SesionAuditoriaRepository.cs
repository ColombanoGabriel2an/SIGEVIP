using System;
using System.Data.SqlClient;
using SIGEVIP.Application.Auditoria;
using SIGEVIP.Application.Security;
using SIGEVIP.Infrastructure.Auditoria;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Security
{
    public sealed class SesionAuditoriaRepository
        : ISesionAuditoriaRepository
    {
        private readonly SqlConnectionFactory
            _connectionFactory;

        public SesionAuditoriaRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory =
                connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public void Registrar(
            AuditoriaRegistro registro)
        {
            if (registro == null)
            {
                throw new ArgumentNullException(
                    nameof(registro));
            }

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                {
                    connection.Open();

                    using (
                        SqlTransaction transaction =
                            connection.BeginTransaction())
                    {
                        try
                        {
                            AuditoriaSqlWriter.Insertar(
                                connection,
                                transaction,
                                registro);

                            transaction.Commit();
                        }
                        catch
                        {
                            RevertirSiCorresponde(
                                transaction);

                            throw;
                        }
                    }
                }
            }
            catch (PersistenciaException)
            {
                throw;
            }
            catch (SqlException exception)
            {
                throw new PersistenciaException(
                    "No fue posible registrar el evento de sesión.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "La transacción de auditoría de sesión no pudo completarse.",
                    exception);
            }
        }

        private static void RevertirSiCorresponde(
            SqlTransaction transaction)
        {
            if (transaction == null)
            {
                return;
            }

            try
            {
                transaction.Rollback();
            }
            catch (SqlException)
            {
                // Se conserva la excepción original.
            }
            catch (InvalidOperationException)
            {
                // La transacción ya pudo haber finalizado.
            }
        }
    }
}