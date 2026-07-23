using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SIGEVIP.Application.Security;
using SIGEVIP.Domain.Entities;
using SIGEVIP.Domain.Exceptions;
using SIGEVIP.Infrastructure.Data;
using SIGEVIP.Infrastructure.Exceptions;

namespace SIGEVIP.Infrastructure.Security
{
    public sealed class UsuarioAutenticacionRepository
        : IUsuarioAutenticacionRepository
    {
        private const string ConsultaUsuarioYSeguridad = @"
SELECT
    u.IdUsuario,
    u.IdPersona,
    u.NombreUsuario,
    u.PasswordHash,
    u.PasswordSalt,
    u.IteracionesPassword,
    u.Activo AS UsuarioActivo,
    p.Nombre AS PersonaNombre,
    p.Apellido AS PersonaApellido,
    p.Email AS PersonaEmail,
    p.Activo AS PersonaActivo
FROM dbo.Usuario AS u
INNER JOIN dbo.Persona AS p
    ON p.IdPersona = u.IdPersona
WHERE u.NombreUsuario = @NombreUsuario;

SELECT
    g.IdGrupo,
    g.Codigo,
    g.Nombre,
    g.Descripcion,
    g.Activo
FROM dbo.Grupo AS g
ORDER BY g.IdGrupo;

SELECT
    p.IdPermiso,
    p.Codigo,
    p.Nombre,
    p.Descripcion,
    p.Activo
FROM dbo.Permiso AS p
ORDER BY p.IdPermiso;

SELECT
    gp.IdGrupo,
    gp.IdPermiso
FROM dbo.GrupoPermiso AS gp
ORDER BY
    gp.IdGrupo,
    gp.IdPermiso;

SELECT
    gg.IdGrupoPadre,
    gg.IdGrupoHijo
FROM dbo.GrupoGrupo AS gg
ORDER BY
    gg.IdGrupoPadre,
    gg.IdGrupoHijo;

SELECT
    ug.IdGrupo
FROM dbo.UsuarioGrupo AS ug
INNER JOIN dbo.Usuario AS u
    ON u.IdUsuario = ug.IdUsuario
WHERE u.NombreUsuario = @NombreUsuario
ORDER BY ug.IdGrupo;";

        private readonly SqlConnectionFactory _connectionFactory;

        public UsuarioAutenticacionRepository(
            SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory
                ?? throw new ArgumentNullException(
                    nameof(connectionFactory));
        }

        public Usuario BuscarPorNombreUsuario(
            string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return null;
            }

            string nombreNormalizado =
                nombreUsuario
                    .Trim()
                    .ToLowerInvariant();

            try
            {
                using (
                    SqlConnection connection =
                        _connectionFactory.Create())
                using (
                    SqlCommand command =
                        connection.CreateCommand())
                {
                    command.CommandType =
                        CommandType.Text;

                    command.CommandText =
                        ConsultaUsuarioYSeguridad;

                    command.Parameters.Add(
                        "@NombreUsuario",
                        SqlDbType.NVarChar,
                        100).Value = nombreNormalizado;

                    connection.Open();

                    using (
                        SqlDataReader reader =
                            command.ExecuteReader())
                    {
                        Usuario usuario =
                            LeerUsuario(
                                reader,
                                command);

                        if (usuario == null)
                        {
                            return null;
                        }

                        Dictionary<int, Grupo> grupos =
                            LeerGrupos(reader);

                        Dictionary<int, Permiso> permisos =
                            LeerPermisos(reader);

                        AsociarPermisos(
                            reader,
                            grupos,
                            permisos);

                        AsociarGruposHijos(
                            reader,
                            grupos);

                        AsignarGruposAlUsuario(
                            reader,
                            grupos,
                            usuario);

                        return usuario;
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
                    "No fue posible consultar el usuario en SQL Server.",
                    exception);
            }
            catch (ReglaNegocioException exception)
            {
                throw new PersistenciaException(
                    "Los datos persistidos de seguridad son inconsistentes.",
                    exception);
            }
            catch (InvalidOperationException exception)
            {
                throw new PersistenciaException(
                    "No fue posible reconstruir la seguridad del usuario.",
                    exception);
            }
        }

        private static Usuario LeerUsuario(
            SqlDataReader reader,
            SqlCommand command)
        {
            if (!reader.Read())
            {
                return null;
            }

            int idUsuario =
                reader.GetInt32(
                    reader.GetOrdinal("IdUsuario"));

            int idPersona =
                reader.GetInt32(
                    reader.GetOrdinal("IdPersona"));

            string nombreUsuario =
                reader.GetString(
                    reader.GetOrdinal("NombreUsuario"));

            byte[] passwordHash =
                LeerBytesObligatorios(
                    reader,
                    "PasswordHash");

            byte[] passwordSalt =
                LeerBytesObligatorios(
                    reader,
                    "PasswordSalt");

            int iteracionesPassword =
                reader.GetInt32(
                    reader.GetOrdinal(
                        "IteracionesPassword"));

            bool usuarioActivo =
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "UsuarioActivo"));

            ValidarPersonaPersistida(reader);

            Usuario usuario =
                new Usuario(
                    idUsuario,
                    idPersona,
                    nombreUsuario,
                    passwordHash,
                    passwordSalt,
                    iteracionesPassword);

            if (!usuarioActivo)
            {
                usuario.Desactivar();
            }

            return usuario;
        }

        private static void ValidarPersonaPersistida(
            SqlDataReader reader)
        {
            string nombre =
                LeerTextoObligatorio(
                    reader,
                    "PersonaNombre");

            string apellido =
                LeerTextoObligatorio(
                    reader,
                    "PersonaApellido");

            string email =
                LeerTextoObligatorio(
                    reader,
                    "PersonaEmail");

            bool personaActiva =
                reader.GetBoolean(
                    reader.GetOrdinal(
                        "PersonaActivo"));

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(email))
            {
                throw new PersistenciaException(
                    "La persona asociada al usuario contiene datos obligatorios inválidos.");
            }

            // El contrato actual devuelve Usuario y no expone Persona.
            // El estado de Persona se recupera para validar la fila,
            // pero la autenticación actual depende del estado de Usuario.
            _ = personaActiva;
        }

        private static Dictionary<int, Grupo> LeerGrupos(
            SqlDataReader reader)
        {
            ExigirSiguienteResultado(
                reader,
                "grupos");

            Dictionary<int, Grupo> grupos =
                new Dictionary<int, Grupo>();

            while (reader.Read())
            {
                int idGrupo =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdGrupo"));

                if (grupos.ContainsKey(idGrupo))
                {
                    throw new PersistenciaException(
                        "Se recuperó más de una fila para el mismo grupo.");
                }

                Grupo grupo =
                    new Grupo(
                        idGrupo,
                        LeerTextoObligatorio(
                            reader,
                            "Codigo"),
                        LeerTextoObligatorio(
                            reader,
                            "Nombre"),
                        LeerTextoOpcional(
                            reader,
                            "Descripcion"));

                bool activo =
                    reader.GetBoolean(
                        reader.GetOrdinal(
                            "Activo"));

                if (!activo)
                {
                    grupo.Desactivar();
                }

                grupos.Add(
                    idGrupo,
                    grupo);
            }

            return grupos;
        }

        private static Dictionary<int, Permiso> LeerPermisos(
            SqlDataReader reader)
        {
            ExigirSiguienteResultado(
                reader,
                "permisos");

            Dictionary<int, Permiso> permisos =
                new Dictionary<int, Permiso>();

            while (reader.Read())
            {
                int idPermiso =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdPermiso"));

                if (permisos.ContainsKey(idPermiso))
                {
                    throw new PersistenciaException(
                        "Se recuperó más de una fila para el mismo permiso.");
                }

                Permiso permiso =
                    new Permiso(
                        idPermiso,
                        LeerTextoObligatorio(
                            reader,
                            "Codigo"),
                        LeerTextoObligatorio(
                            reader,
                            "Nombre"),
                        LeerTextoOpcional(
                            reader,
                            "Descripcion"));

                bool activo =
                    reader.GetBoolean(
                        reader.GetOrdinal(
                            "Activo"));

                if (!activo)
                {
                    permiso.Desactivar();
                }

                permisos.Add(
                    idPermiso,
                    permiso);
            }

            return permisos;
        }

        private static void AsociarPermisos(
            SqlDataReader reader,
            IDictionary<int, Grupo> grupos,
            IDictionary<int, Permiso> permisos)
        {
            ExigirSiguienteResultado(
                reader,
                "relaciones GrupoPermiso");

            while (reader.Read())
            {
                int idGrupo =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdGrupo"));

                int idPermiso =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdPermiso"));

                Grupo grupo =
                    ObtenerGrupo(
                        grupos,
                        idGrupo,
                        "GrupoPermiso");

                Permiso permiso =
                    ObtenerPermiso(
                        permisos,
                        idPermiso);

                grupo.AgregarComponente(
                    permiso);
            }
        }

        private static void AsociarGruposHijos(
            SqlDataReader reader,
            IDictionary<int, Grupo> grupos)
        {
            ExigirSiguienteResultado(
                reader,
                "relaciones GrupoGrupo");

            while (reader.Read())
            {
                int idGrupoPadre =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdGrupoPadre"));

                int idGrupoHijo =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdGrupoHijo"));

                Grupo grupoPadre =
                    ObtenerGrupo(
                        grupos,
                        idGrupoPadre,
                        "GrupoGrupo");

                Grupo grupoHijo =
                    ObtenerGrupo(
                        grupos,
                        idGrupoHijo,
                        "GrupoGrupo");

                try
                {
                    grupoPadre.AgregarComponente(
                        grupoHijo);
                }
                catch (ReglaNegocioException exception)
                {
                    throw new PersistenciaException(
                        "Se detectó una jerarquía de grupos inválida o cíclica.",
                        exception);
                }
            }
        }

        private static void AsignarGruposAlUsuario(
            SqlDataReader reader,
            IDictionary<int, Grupo> grupos,
            Usuario usuario)
        {
            ExigirSiguienteResultado(
                reader,
                "grupos directos del usuario");

            while (reader.Read())
            {
                int idGrupo =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "IdGrupo"));

                Grupo grupo =
                    ObtenerGrupo(
                        grupos,
                        idGrupo,
                        "UsuarioGrupo");

                usuario.AgregarGrupo(
                    grupo);
            }
        }

        private static Grupo ObtenerGrupo(
            IDictionary<int, Grupo> grupos,
            int idGrupo,
            string origen)
        {
            Grupo grupo;

            if (!grupos.TryGetValue(
                idGrupo,
                out grupo))
            {
                throw new PersistenciaException(
                    "La relación " +
                    origen +
                    " referencia un grupo inexistente.");
            }

            return grupo;
        }

        private static Permiso ObtenerPermiso(
            IDictionary<int, Permiso> permisos,
            int idPermiso)
        {
            Permiso permiso;

            if (!permisos.TryGetValue(
                idPermiso,
                out permiso))
            {
                throw new PersistenciaException(
                    "La relación GrupoPermiso referencia un permiso inexistente.");
            }

            return permiso;
        }

        private static byte[] LeerBytesObligatorios(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            if (reader.IsDBNull(ordinal))
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede ser nula.");
            }

            byte[] valor =
                (byte[])reader.GetValue(
                    ordinal);

            if (valor.Length == 0)
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede estar vacía.");
            }

            return valor;
        }

        private static string LeerTextoObligatorio(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            if (reader.IsDBNull(ordinal))
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede ser nula.");
            }

            string valor =
                reader.GetString(
                    ordinal);

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new PersistenciaException(
                    "La columna " +
                    columna +
                    " no puede estar vacía.");
            }

            return valor;
        }

        private static string LeerTextoOpcional(
            SqlDataReader reader,
            string columna)
        {
            int ordinal =
                reader.GetOrdinal(
                    columna);

            return reader.IsDBNull(ordinal)
                ? string.Empty
                : reader.GetString(ordinal);
        }

        private static void ExigirSiguienteResultado(
            SqlDataReader reader,
            string descripcion)
        {
            if (!reader.NextResult())
            {
                throw new PersistenciaException(
                    "La consulta no devolvió el conjunto esperado de " +
                    descripcion +
                    ".");
            }
        }
    }
}
