# Estado del proyecto

## Etapa actual

Etapa 3: autenticación, usuarios, grupos, permisos y seguridad.

Bloque actual: cierre documental del dominio y servicios básicos de seguridad.

## Rama de trabajo

`desarrollo/dominio-viajes-viaticos`

## Último commit técnico publicado

- Commit: `64a2161`
- Mensaje: `Incorporo hash seguro de contraseñas`

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

## Commits del Bloque 3

- `09f90eb` — `Agrego dominio de personas y usuarios`
- `b69fa36` — `Implemento Composite de grupos y permisos`
- `ea8e92e` — `Agrego autenticación y autorización`
- `64a2161` — `Incorporo hash seguro de contraseñas`

## Resultado técnico verificado

Compilación:

- 0 advertencias.
- 0 errores.
- Tiempo registrado: 1,29 segundos.

Pruebas:

- Totales: 145.
- Correctas: 145.
- Fallidas: 0.
- Omitidas: 0.
- Tiempo registrado: 4,0902 segundos.

Ejecutor:

`VSTest 17.13.0 x64`

## Estado de requisitos de seguridad

### Parcialmente implementados

- RF01: iniciar sesión.
- RF02: gestionar usuarios.
- RF03: modificar usuarios.

Faltan persistencia e interfaz.

### Implementado en dominio

- RF04: eliminar o desactivar usuario.

Se implementó como desactivación lógica.

### Implementado en Application

- RF37: validar permisos antes de ejecutar operaciones.
- RF39: restringir acceso según permisos.

La aplicación concreta de estos servicios a cada caso de uso se realizará en bloques funcionales posteriores.

### Pendiente

- RF38: ocultar o deshabilitar funciones no autorizadas en la interfaz.

## Pendiente inmediato

- Actualizar documentación del Bloque 3.
- Validar diferencias documentales.
- Compilar nuevamente.
- Ejecutar las 145 pruebas.
- Crear commit documental.
- Publicar el commit.
- Generar informe de transferencia para MAESTRO.

## Pendiente de etapas posteriores

- Tablas SQL de Persona, Usuario, Grupo y Permiso.
- Relaciones UsuarioGrupo.
- Relaciones GrupoComponente.
- Repositorio ADO.NET de autenticación.
- Unicidad de nombre de usuario.
- Datos iniciales de grupos y permisos.
- Interfaz de login.
- Gestión visual de usuarios.
- Cambio de contraseña.
- Recuperación de contraseña.
- Auditoría persistente.
- Aplicación de permisos a casos de uso.
- Pruebas de integración con SQL Server.
