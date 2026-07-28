# Decisiones técnicas

## DT-01 — Plataforma

Se utiliza .NET Framework 4.8 por compatibilidad con el entorno Windows, la aplicación de escritorio y las herramientas disponibles.

## DT-02 — Interfaz

La interfaz se implementará con Windows Forms.

## DT-03 — Persistencia

Se utilizará ADO.NET con `System.Data.SqlClient` y SQL Server.

No se utilizará Entity Framework.

## DT-04 — Arquitectura

Se aplica una arquitectura en capas con MVC adaptado a Windows Forms.

## DT-05 — Base de datos

La base de datos será local y reproducible mediante scripts SQL.

La base existente se denomina `SIGEVIP`.

## DT-06 — Separación Persona y Usuario

Persona representa los datos personales.

Usuario representa la identidad de acceso al sistema.

Una Persona puede tener cero o un Usuario.

Todo Usuario debe estar asociado con una Persona válida.

## DT-07 — Estado lógico

Persona, Usuario, Grupo, Permiso y Cliente utilizan activación y desactivación lógica.

No se elimina físicamente información que pueda formar parte de relaciones históricas.

## DT-08 — Identificación de Usuario

El nombre de usuario se normaliza:

- se eliminan espacios exteriores;
- se convierte a minúsculas.

La unicidad global se implementará mediante repositorio e índice único en SQL Server.

## DT-09 — Credenciales

Usuario no almacena contraseñas en texto plano.

Almacena:

- hash;
- salt;
- cantidad de iteraciones.

Las colecciones de bytes se protegen mediante copias defensivas.

## DT-10 — Seguridad por grupos y permisos

La autorización se basa en grupos y permisos.

No se utiliza un enum de roles.

Los roles funcionales se representarán mediante grupos configurables.

Un Usuario puede pertenecer a uno o varios Grupos.

## DT-11 — Patrón Composite

El patrón Composite se aplica a Grupo y Permiso.

- Permiso es un componente hoja.
- Grupo es un componente compuesto.
- Un Grupo puede contener permisos y grupos.
- Se rechazan referencias nulas.
- Se rechazan duplicados.
- Se rechazan ciclos directos e indirectos.
- Los permisos efectivos se deduplican por código normalizado.

## DT-12 — Componentes inactivos

Un Grupo inactivo no aporta permisos.

Un Permiso inactivo no se considera efectivo.

Un grupo hijo inactivo tampoco aporta permisos al grupo padre.

## DT-13 — Autenticación

La autenticación se implementa mediante `AutenticacionService`.

Las credenciales inválidas no generan excepciones de flujo normal.

El resultado se representa mediante `ResultadoAutenticacion`.

Usuario inexistente, usuario inactivo y contraseña incorrecta utilizan el mismo mensaje público:

    Credenciales inválidas.

Esta decisión reduce la exposición de información sobre cuentas registradas.

## DT-14 — Autorización

La autorización se realiza mediante `AutorizacionService`.

La consulta se efectúa por código de permiso normalizado.

Un usuario nulo o inactivo nunca se considera autorizado.

Los permisos se obtienen de todos los grupos activos del usuario.

## DT-15 — Sesión actual

La sesión se implementa mediante `ISesionActual` y `SesionActual`.

La implementación:

- no es estática;
- puede reemplazarse en pruebas;
- solo admite usuarios activos;
- permite iniciar y cerrar sesión.

## DT-16 — Hash de contraseñas

Se utiliza PBKDF2 con HMAC-SHA256.

Parámetros actuales:

- Salt: 32 bytes.
- Hash: 32 bytes.
- Iteraciones: 100000.
- Generación de salt: `RandomNumberGenerator`.
- Comparación: tiempo constante.

La cantidad de iteraciones se almacena junto al Usuario para permitir cambios futuros sin invalidar credenciales existentes.

## DT-17 — Contrato de hash

`IPasswordHasher` se define en Application.

`Pbkdf2PasswordHasher` se implementa en Infrastructure.

Esta separación permite:

- probar Application sin criptografía real;
- sustituir el algoritmo;
- respetar la inversión de dependencias.

## DT-18 — Contrato de autenticación

`IUsuarioAutenticacionRepository` se define en Application.

La implementación ADO.NET se desarrollará en un bloque posterior.

La autenticación actual se probó con repositorios falsos en memoria.

## DT-19 — Patrón State en Viaje

El patrón State se aplica a la entidad `Viaje`.

Estados:

- `Abierto`
- `EnRendicion`
- `Aprobado`
- `Cancelado`

Las transiciones inválidas generan `ReglaNegocioException`.

## DT-20 — Estado persistible

El estado de Viaje se expone mediante el enum `EstadoViaje`.

Los objetos que implementan `IEstadoViaje` representan comportamiento en memoria.

`EstadoViajeFactory` reconstruye el comportamiento desde el valor persistido.

## DT-21 — Importes monetarios

Los importes se representan mediante `decimal`.

No se utilizan `float` ni `double`.

## DT-22 — Colecciones protegidas

Las colecciones internas se mantienen modificables solo dentro de las entidades y se exponen mediante `IReadOnlyCollection`.

Se aplica a:

- viáticos de Viaje;
- visitas de Viaje;
- clientes de Visita;
- grupos de Usuario;
- componentes de Grupo.

## DT-23 — Exclusión lógica de viáticos

Un viático no se elimina físicamente.

Su estado cambia entre:

- `Vigente`
- `Excluido`

Solo los viáticos vigentes intervienen en el total gastado.

## DT-24 — Pruebas

Se utiliza MSTest.

La cobertura actual comprende:

- dominio;
- patrones State y Composite;
- autenticación;
- autorización;
- sesión;
- PBKDF2;
- validaciones;
- regresión de bloques anteriores.

## DT-25 — Control de versiones

Se utiliza Git con commits pequeños y descriptivos en tiempo verbal presente.

## DT-26 — Alcance excluido del bloque actual

Todavía no forman parte del bloque implementado:

- repositorios ADO.NET de seguridad;
- tablas SQL de seguridad;
- interfaz de login;
- recuperación de contraseña;
- correo electrónico;
- bloqueo por intentos fallidos;
- MFA;
- JWT;
- servicios web;
- microservicios.
