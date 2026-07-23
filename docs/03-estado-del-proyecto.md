# Estado del proyecto

## Etapa actual

Etapa 3: autenticación, usuarios, grupos, permisos y seguridad.

Bloque actual: cierre técnico y documental de la persistencia de seguridad.

## Rama de trabajo

`desarrollo/dominio-viajes-viaticos`

## Último commit técnico publicado

- Commit: `62396ba`
- Mensaje: `Pruebo jerarquías persistidas de seguridad`

La rama local se encuentra sincronizada con:

`origin/desarrollo/dominio-viajes-viaticos`

## Completado previamente

- Repositorio Git configurado.
- Remoto GitHub verificado.
- Solución ubicada en la raíz.
- Proyectos Domain, Application, Infrastructure, WinForms y Tests.
- .NET Framework 4.8.
- MSTest.
- SQL Server local.
- Base `SIGEVIP`.
- Conexión integrada comprobada.
- Aplicación WinForms mínima.
- Dominio de Viaje y Viático.
- Patrón State.
- Cliente.
- Visita.
- Relación Viaje-Visita.
- Relación muchos a muchos Visita-Cliente.

## Completado en el Bloque 3

### Persona

- Entidad separada de Usuario.
- Nombre obligatorio.
- Apellido obligatorio.
- Email obligatorio.
- Estado activo inicial.
- Activación y desactivación lógica.

### Usuario

- Asociación obligatoria con Persona.
- Nombre de usuario normalizado.
- Hash, salt e iteraciones.
- Copias defensivas de datos criptográficos.
- Activación y desactivación.
- Asociación con uno o varios grupos.
- Prevención de grupos nulos y duplicados.

### Permiso

- Código obligatorio y normalizado.
- Nombre obligatorio.
- Descripción opcional.
- Estado activo.
- Activación y desactivación.
- Componente hoja del patrón Composite.

### Grupo

- Código obligatorio y normalizado.
- Nombre obligatorio.
- Descripción opcional.
- Estado activo.
- Activación y desactivación.
- Componente compuesto.
- Permisos directos.
- Grupos anidados.
- Prevención de duplicados.
- Prevención de ciclos directos e indirectos.
- Obtención de permisos efectivos.
- Eliminación de permisos repetidos.

### Autenticación

- Contrato `IUsuarioAutenticacionRepository`.
- Contrato `IPasswordHasher`.
- Servicio `AutenticacionService`.
- Resultado explícito de autenticación.
- Rechazo de entradas vacías.
- Rechazo de usuario inexistente.
- Rechazo de usuario inactivo.
- Rechazo de contraseña incorrecta.
- Mensaje público genérico.
- Normalización del nombre de usuario.

### Autorización

- Servicio `AutorizacionService`.
- Consulta por código de permiso.
- Normalización de código.
- Unión de permisos de múltiples grupos.
- Soporte de permisos anidados.
- Rechazo de usuario nulo o inactivo.

### Sesión

- Contrato `ISesionActual`.
- Implementación `SesionActual`.
- Sesión no estática.
- Inicio con usuario activo.
- Cierre de sesión.
- Rechazo de usuario nulo o inactivo.

### Hash seguro

- Implementación `Pbkdf2PasswordHasher`.
- PBKDF2-HMAC-SHA256.
- Salt aleatorio de 32 bytes.
- Hash de 32 bytes.
- 100000 iteraciones.
- Comparación en tiempo constante.
- Sin dependencias externas.

## Completado en el Bloque 4

### Migración de seguridad

Se creó:

`database/migrations/002_crear_seguridad.sql`

La migración:

- utiliza la base `SIGEVIP`;
- se ejecuta dentro de una transacción;
- utiliza `TRY/CATCH`;
- revierte los cambios ante errores;
- no elimina tablas;
- no recrea la base;
- registra la versión `002` en `dbo.VersionBaseDatos`;
- evita registrar la versión más de una vez.

### Tablas creadas

- `dbo.Persona`
- `dbo.Usuario`
- `dbo.Grupo`
- `dbo.Permiso`
- `dbo.UsuarioGrupo`
- `dbo.GrupoPermiso`
- `dbo.GrupoGrupo`

### Relaciones persistidas

- Persona 1 a 0..1 Usuario.
- Usuario N a N Grupo.
- Grupo N a N Permiso.
- Grupo N a N Grupo mediante relación padre-hijo.

### Integridad

Se incorporaron:

- claves primarias simples;
- claves primarias compuestas;
- claves foráneas;
- restricciones `CHECK`;
- valores predeterminados;
- índices únicos;
- índices auxiliares;
- acciones de eliminación `NO_ACTION`;
- prevención de relaciones duplicadas;
- prevención de autorreferencia directa entre grupos.

### Credenciales

La tabla `Usuario` persiste:

- hash de 32 bytes;
- salt de 32 bytes;
- cantidad de iteraciones;
- estado lógico.

No persiste:

- contraseña en texto plano;
- contraseña reversible;
- contraseña temporal;
- rol como texto.

### Catálogos iniciales

Se creó:

`database/seed/001_catalogos_seguridad.sql`

Grupos iniciales:

- `COMERCIAL`
- `ADMINISTRATIVO`
- `GERENTE`
- `ADMINISTRADOR_GENERAL`

Permisos iniciales:

- 17 permisos funcionales.

Asociaciones iniciales:

- 22 asociaciones `GrupoPermiso`.

El seed:

- puede ejecutarse nuevamente;
- no duplica grupos;
- no duplica permisos;
- no duplica asociaciones;
- no crea usuarios;
- no crea contraseñas;
- no crea relaciones `GrupoGrupo` sin justificación funcional.

### Validación SQL

Se creó:

`database/migrations/002_validar_seguridad.sql`

La validación comprobó:

- migración `002` registrada;
- existencia de las siete tablas;
- 4 grupos;
- 17 permisos;
- 22 asociaciones `GrupoPermiso`;
- 0 usuarios;
- 0 asociaciones `UsuarioGrupo`;
- 0 relaciones `GrupoGrupo`;
- índices únicos;
- índices auxiliares;
- siete claves foráneas;
- acciones de eliminación `NO_ACTION`;
- restricciones `CHECK` habilitadas;
- restricciones confiables;
- ausencia de duplicados.

Resultado final:

`VALIDACIÓN CORRECTA`

## Completado en el Bloque 5

### Repositorio de autenticación

Se implementó:

`SIGEVIP.Infrastructure.Security.UsuarioAutenticacionRepository`

La implementación:

- utiliza ADO.NET y `System.Data.SqlClient`;
- implementa `IUsuarioAutenticacionRepository`;
- utiliza consultas parametrizadas;
- no utiliza `SELECT *`;
- evita consultas N+1;
- recupera Usuario y datos obligatorios de Persona;
- recupera grupos directos;
- recupera permisos y asociaciones `GrupoPermiso`;
- recupera grupos hijos y asociaciones `GrupoGrupo`;
- reconstruye el patrón Composite;
- conserva estados activos e inactivos;
- detecta jerarquías persistidas inválidas;
- transforma fallas técnicas en `PersistenciaException`.

### Reconstrucción del Composite

Se verificó mediante integración real con SQL Server:

- recuperación de grupos directos;
- recuperación de permisos directos;
- herencia de permisos desde un grupo hijo;
- detección y rechazo de ciclos persistidos;
- limpieza de los datos temporales de prueba.

### Inicialización del administrador

Se implementaron:

- `IInicializacionSeguridadRepository`;
- `InicializacionSeguridadService`;
- `ResultadoInicializacionSeguridad`;
- `InicializacionSeguridadRepository`.

La creación inicial:

- valida campos obligatorios;
- confirma la contraseña;
- normaliza el nombre de usuario;
- genera PBKDF2 mediante `Pbkdf2PasswordHasher`;
- inserta Persona, Usuario y UsuarioGrupo;
- asigna `ADMINISTRADOR_GENERAL`;
- utiliza una única transacción;
- revierte los cambios ante errores;
- evita duplicar usuarios.

### Herramienta de configuración

Se creó:

`tools/SIGEVIP.Setup`

La utilidad:

- es una consola .NET Framework 4.8;
- solicita los datos del administrador;
- oculta y confirma la contraseña;
- utiliza la cadena de conexión `SIGEVIP`;
- no muestra contraseña, hash ni salt;
- devuelve códigos de salida explícitos.

Validación manual:

- administrador creado correctamente;
- grupo `ADMINISTRADOR_GENERAL` asignado;
- hash de 32 bytes;
- salt de 32 bytes;
- 100000 iteraciones;
- código `0` al crear;
- código `3` ante usuario existente;
- segunda ejecución sin duplicados.

## Commits relevantes

### Bloque 3

- `09f90eb` — `Agrego dominio de personas y usuarios`
- `b69fa36` — `Implemento Composite de grupos y permisos`
- `ea8e92e` — `Agrego autenticación y autorización`
- `64a2161` — `Incorporo hash seguro de contraseñas`
- `1464aa9` — `Documento seguridad y cierre del bloque 3`

### Bloque 4

- `a4b631c` — `Agrego esquema SQL de seguridad`
- `2e567f2` — `Documento persistencia de seguridad`

### Bloque 5

- `cb52c7e` — `Implemento repositorio de autenticación`
- `514ba5a` — `Agrego inicialización del administrador`
- `a89c186` — `Agrego herramienta de configuración inicial`
- `62396ba` — `Pruebo jerarquías persistidas de seguridad`

## Resultado técnico verificado

### Compilación

- 0 advertencias.
- 0 errores.
- Compilación completa verificada.

### Pruebas

- Totales: 164.
- Correctas: 164.
- Fallidas: 0.
- Incluyen pruebas unitarias y pruebas reales contra SQL Server.

Ejecutor:

`VSTest 17.13.0 x64`

### Base de datos

- Migración `002`: aplicada correctamente.
- Seed: aplicado correctamente.
- Seed reejecutado sin duplicados.
- Validación SQL: correcta.

## Estado de requisitos de seguridad

### RF01 — Iniciar sesión

Parcialmente cubierto.

Implementado:

- servicio de autenticación;
- verificación segura de contraseña;
- repositorio ADO.NET;
- recuperación persistente desde SQL Server;
- reconstrucción de grupos y permisos;
- esquema SQL de credenciales;
- pruebas de integración.

Pendiente:

- interfaz de login;
- integración con la sesión de WinForms.

### RF02 — Validar credenciales y habilitar funciones

Parcialmente cubierto.

Implementado:

- autenticación en Application;
- autorización por permisos;
- repositorio concreto;
- persistencia de Usuario, Grupo y Permiso;
- reconstrucción del Composite;
- detección de ciclos persistidos.

Pendiente:

- sesión integrada con WinForms;
- habilitación visual de funciones.

### RF03 — Gestionar usuarios

Parcialmente cubierto.

Implementado:

- entidad Usuario;
- activación y desactivación;
- asignación de grupos;
- tablas Persona, Usuario y UsuarioGrupo.

Pendiente:

- casos de uso;
- repositorios;
- interfaz.

### RF04 — Asignar uno o más grupos

Persistencia preparada mediante `dbo.UsuarioGrupo`.

La asignación funcional mediante casos de uso e interfaz permanece pendiente.

### RF37 — Impedir accesos no autorizados

Implementado en Application.

La persistencia de grupos y permisos está preparada.

La integración con los casos de uso concretos permanece pendiente.

### RF38 — Ocultar opciones no habilitadas

Pendiente de WinForms.

### RF39 — Acceso mediante grupos y permisos

Implementado en Domain y Application.

Persistencia preparada mediante:

- `UsuarioGrupo`;
- `GrupoPermiso`;
- `GrupoGrupo`.

## Pendiente inmediato

- Validar la actualización documental del Bloque 5.
- Crear el commit documental.
- Publicar el commit.
- Generar el informe de cierre para MAESTRO.

## Pendiente de etapas posteriores

- Implementación ADO.NET de `IUsuarioAutenticacionRepository`.
- Repositorios de Usuario, Grupo y Permiso.
- Reconstrucción persistente del Composite.
- Usuario administrador inicial generado desde C#.
- Casos de uso de gestión de usuarios.
- Interfaz de login.
- Gestión visual de usuarios.
- Gestión visual de grupos.
- Gestión visual de permisos.
- Cambio de contraseña.
- Recuperación de contraseña.
- Auditoría persistente.
- Aplicación de permisos a casos de uso.
- Pruebas de integración con SQL Server.
- Persistencia de viajes, clientes, visitas y viáticos.
