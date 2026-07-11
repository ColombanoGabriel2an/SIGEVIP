# Seguridad de SIGEVIP

## 1. Alcance implementado

El bloque de seguridad contiene:

- Persona.
- Usuario.
- Grupo.
- Permiso.
- Patrón Composite.
- Autenticación.
- Autorización.
- Sesión actual.
- Hash PBKDF2-HMAC-SHA256.

No incluye todavía:

- persistencia SQL;
- interfaz de login;
- recuperación por correo;
- bloqueo por intentos fallidos;
- autenticación multifactor;
- tokens;
- servicios web.

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

## 3. Grupos y permisos

La autorización no utiliza roles fijos en código.

Los roles funcionales se modelan mediante Grupos configurables.

Un Grupo puede contener:

- permisos simples;
- otros grupos.

La estructura utiliza el patrón Composite.

## 4. Permisos efectivos

Los permisos efectivos de un Usuario son la unión de los permisos de todos sus grupos activos.

Se consideran:

- permisos directos;
- permisos heredados de grupos anidados.

No se consideran:

- grupos inactivos;
- grupos hijos inactivos;
- permisos inactivos.

Los permisos repetidos se eliminan por código normalizado.

## 5. Prevención de ciclos

Grupo rechaza:

- agregarse a sí mismo;
- duplicados por referencia;
- duplicados por identificador persistido;
- duplicados por código;
- ciclos indirectos.

Esto evita recorridos infinitos y configuraciones inválidas.

## 6. Autenticación

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

## 7. Hash de contraseñas

La implementación concreta es:

`SIGEVIP.Infrastructure.Security.Pbkdf2PasswordHasher`

Parámetros:

- Algoritmo: PBKDF2.
- Función seudorrandom: HMAC-SHA256.
- Salt: 32 bytes.
- Hash: 32 bytes.
- Iteraciones: 100000.

El salt se genera mediante `RandomNumberGenerator`.

Cada contraseña produce un salt distinto.

## 8. Comparación de hashes

La comparación no utiliza una igualdad que finalice al encontrar el primer byte diferente.

Se acumulan las diferencias de todos los bytes.

Esto reduce diferencias temporales observables durante la comparación.

## 9. Almacenamiento futuro

La tabla Usuario deberá almacenar:

- IdUsuario.
- IdPersona.
- NombreUsuario.
- PasswordHash.
- PasswordSalt.
- IteracionesPassword.
- Activo.

No se almacenará:

- contraseña en texto plano;
- contraseña reversible;
- clave fija dentro del código.

## 10. Autorización

`AutorizacionService.TienePermiso()` recibe:

- Usuario.
- Código de permiso.

El servicio:

- rechaza usuario nulo;
- rechaza usuario inactivo;
- normaliza el código;
- recorre grupos activos;
- consulta permisos efectivos;
- devuelve verdadero o falso.

## 11. Sesión

`SesionActual` es una implementación en memoria.

No es estática.

Permite:

- iniciar sesión;
- consultar el usuario actual;
- cerrar sesión.

No permite iniciar sesión con:

- usuario nulo;
- usuario inactivo.

## 12. Responsabilidades pendientes

### Application

- Casos de uso de alta y modificación de usuarios.
- Cambio de contraseña.
- Recuperación de contraseña.
- Aplicación de permisos a operaciones funcionales.
- Auditoría de acciones.

### Infrastructure

- Repositorio ADO.NET de Usuario.
- Repositorios de Grupo y Permiso.
- Persistencia de asociaciones.
- Migraciones.
- Semillas iniciales.
- Registro de auditoría.

### WinForms

- Formulario de login.
- Cierre de sesión.
- Gestión visual de usuarios.
- Gestión visual de grupos.
- Gestión visual de permisos.
- Ocultamiento o deshabilitación de controles.

## 13. Estado académico

La base de seguridad está implementada y probada en Domain, Application e Infrastructure.

Los requisitos de autenticación y gestión de usuarios continúan parciales hasta incorporar:

- persistencia;
- interfaz;
- validación integrada reproducible.
