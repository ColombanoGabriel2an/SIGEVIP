# Seguridad de SIGEVIP

## 1. Alcance implementado

El subsistema de seguridad contiene:

- Persona.
- Usuario.
- Grupo.
- Permiso.
- Patrón Composite.
- Autenticación.
- Autorización.
- Sesión actual.
- Hash PBKDF2-HMAC-SHA256.
- Persistencia SQL del modelo de seguridad.
- Catálogos iniciales de grupos y permisos.
- Script reproducible de validación.

No incluye todavía:

- repositorio ADO.NET de autenticación;
- casos de uso de gestión de usuarios;
- usuario administrador inicial;
- interfaz de login;
- recuperación por correo;
- bloqueo por intentos fallidos;
- autenticación multifactor;
- tokens;
- servicios web;
- auditoría persistente.

## 2. Persona y Usuario

Persona y Usuario son entidades separadas.

Persona contiene información personal.

Usuario contiene:

- identidad de acceso;
- hash;
- salt;
- iteraciones;
- estado;
- grupos asignados.

Todo Usuario requiere una Persona válida.

La relación persistente es:

    Persona 1 -------- 0..1 Usuario

Se implementa mediante:

- clave foránea `FK_Usuario_Persona`;
- índice único `UX_Usuario_IdPersona`.

## 3. Grupos y permisos

La autorización no utiliza roles fijos en código.

Los roles funcionales se modelan mediante Grupos configurables.

Un Grupo puede contener:

- permisos simples;
- otros grupos.

La estructura utiliza el patrón Composite.

La persistencia se divide en:

- `UsuarioGrupo`;
- `GrupoPermiso`;
- `GrupoGrupo`.

No se utiliza una tabla polimórfica única.

## 4. Persistencia del Composite

### GrupoPermiso

Representa la asociación entre un Grupo y un Permiso.

La clave primaria compuesta:

    IdGrupo + IdPermiso

impide relaciones duplicadas.

### GrupoGrupo

Representa una relación jerárquica:

- grupo padre;
- grupo hijo.

La clave primaria compuesta:

    IdGrupoPadre + IdGrupoHijo

impide relaciones duplicadas.

La restricción:

`CK_GrupoGrupo_GruposDiferentes`

impide que un grupo se asocie directamente consigo mismo.

Los ciclos indirectos no se resuelven mediante triggers.

Se validarán:

- en Application;
- al reconstruir el Composite desde persistencia.

## 5. Permisos efectivos

Los permisos efectivos de un Usuario son la unión de los permisos de todos sus grupos activos.

Se consideran:

- permisos directos;
- permisos heredados de grupos anidados.

No se consideran:

- grupos inactivos;
- grupos hijos inactivos;
- permisos inactivos.

Los permisos repetidos se eliminan por código normalizado.

## 6. Prevención de ciclos

Grupo rechaza:

- agregarse a sí mismo;
- duplicados por referencia;
- duplicados por identificador persistido;
- duplicados por código;
- ciclos indirectos.

Esto evita recorridos infinitos y configuraciones inválidas.

La base de datos complementa esta validación impidiendo la autorreferencia directa.

## 7. Autenticación

`AutenticacionService` coordina:

1. Validación de entrada.
2. Normalización del nombre.
3. Consulta del Usuario.
4. Validación de estado activo.
5. Verificación de contraseña.
6. Construcción del resultado.

Las credenciales inválidas generan un resultado fallido y no una excepción.

El mensaje público es siempre:

    Credenciales inválidas.

La autenticación persistente todavía requiere la implementación concreta de:

`IUsuarioAutenticacionRepository`

## 8. Hash de contraseñas

La implementación concreta es:

`SIGEVIP.Infrastructure.Security.Pbkdf2PasswordHasher`

Parámetros:

- algoritmo: PBKDF2;
- función seudorrandom: HMAC-SHA256;
- salt: 32 bytes;
- hash: 32 bytes;
- iteraciones: 100000.

El salt se genera mediante `RandomNumberGenerator`.

Cada contraseña produce un salt distinto.

## 9. Comparación de hashes

La comparación no utiliza una igualdad que finalice al encontrar el primer byte diferente.

Se acumulan las diferencias de todos los bytes.

Esto reduce diferencias temporales observables durante la comparación.

## 10. Almacenamiento de credenciales

La tabla `dbo.Usuario` almacena:

- `IdUsuario`;
- `IdPersona`;
- `NombreUsuario`;
- `PasswordHash`;
- `PasswordSalt`;
- `IteracionesPassword`;
- `Activo`.

Tipos relevantes:

- `PasswordHash VARBINARY(32)`;
- `PasswordSalt VARBINARY(32)`;
- `IteracionesPassword INT`.

Restricciones:

- hash obligatorio;
- salt obligatorio;
- longitud exacta de 32 bytes;
- iteraciones mayores que cero;
- nombre de usuario obligatorio;
- nombre de usuario único;
- Persona obligatoria;
- una Persona no puede tener más de un Usuario.

No se almacena:

- contraseña en texto plano;
- contraseña reversible;
- contraseña temporal;
- rol como texto;
- clave criptográfica fija.

## 11. Autorización

`AutorizacionService.TienePermiso()` recibe:

- Usuario;
- código de permiso.

El servicio:

- rechaza usuario nulo;
- rechaza usuario inactivo;
- normaliza el código;
- recorre grupos activos;
- consulta permisos efectivos;
- devuelve verdadero o falso.

## 12. Sesión

`SesionActual` es una implementación en memoria.

No es estática.

Permite:

- iniciar sesión;
- consultar el usuario actual;
- cerrar sesión.

No permite iniciar sesión con:

- usuario nulo;
- usuario inactivo.

## 13. Bajas lógicas

Las tablas:

- Persona;
- Usuario;
- Grupo;
- Permiso;

incluyen la columna:

`Activo BIT NOT NULL DEFAULT 1`

No se implementa borrado físico como operación funcional.

Las claves foráneas utilizan:

`NO_ACTION`

No se utiliza `ON DELETE CASCADE`.

## 14. Catálogos iniciales

El seed crea los grupos:

- `COMERCIAL`;
- `ADMINISTRATIVO`;
- `GERENTE`;
- `ADMINISTRADOR_GENERAL`.

También crea 17 permisos funcionales y 22 asociaciones Grupo-Permiso.

El seed:

- es reejecutable;
- no duplica registros;
- no crea usuarios;
- no crea contraseñas;
- no crea jerarquías de grupos no documentadas.

El usuario administrador inicial se creará posteriormente desde C# mediante `Pbkdf2PasswordHasher`.

## 15. Validación ejecutada

La migración `002` se ejecutó correctamente.

El seed se ejecutó dos veces sin duplicar información.

Se comprobó:

- 7 tablas;
- 4 grupos;
- 17 permisos;
- 22 asociaciones GrupoPermiso;
- 0 usuarios;
- 0 relaciones UsuarioGrupo;
- 0 relaciones GrupoGrupo;
- índices únicos;
- índices auxiliares;
- claves foráneas;
- acciones `NO_ACTION`;
- restricciones `CHECK`;
- ausencia de duplicados.

Resultado:

`VALIDACIÓN CORRECTA`

## 16. Responsabilidades pendientes

### Application

- Casos de uso de alta y modificación de usuarios.
- Cambio de contraseña.
- Recuperación de contraseña.
- Aplicación de permisos a operaciones funcionales.
- Auditoría de acciones.
- Validación de ciclos al reconstruir grupos persistidos.

### Infrastructure

- Repositorio ADO.NET de Usuario.
- Implementación de `IUsuarioAutenticacionRepository`.
- Repositorios de Grupo y Permiso.
- Reconstrucción del Composite.
- Persistencia de asociaciones.
- Creación controlada del administrador inicial.
- Registro de auditoría.

### WinForms

- Formulario de login.
- Cierre de sesión.
- Gestión visual de usuarios.
- Gestión visual de grupos.
- Gestión visual de permisos.
- Ocultamiento o deshabilitación de controles.

## 17. Estado académico

La base de seguridad está implementada y probada en:

- Domain;
- Application;
- Infrastructure;
- SQL Server.

Los requisitos de autenticación y gestión de usuarios continúan parciales hasta incorporar:

- repositorios ADO.NET;
- integración persistente;
- casos de uso;
- interfaz;
- validación integrada reproducible.
