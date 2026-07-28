# Cambio de clave

## 1. Objetivo

Este documento registra la implementación y validación de CUD11: Cambiar clave.

La funcionalidad permite que cada Usuario autenticado cambie su propia contraseña sin intervención del Administrador ni acceso directo a SQL Server.

CUD12: Recuperar clave permanece fuera del MVP actual.

## 2. Alcance implementado

El Usuario autenticado puede:

- abrir la opción Cambiar contraseña desde el menú principal;
- ingresar su contraseña actual;
- ingresar una contraseña nueva;
- confirmar la contraseña nueva;
- guardar el cambio;
- volver al login después de una actualización correcta;
- autenticarse con la contraseña nueva.

La operación no requiere un permiso administrativo específico.

La autorización se basa en la existencia de una sesión válida y en que el Usuario autenticado continúe activo.

## 3. Reglas funcionales

Se validan las siguientes reglas:

- debe existir una sesión autenticada;
- el Usuario de la sesión debe estar activo;
- los tres campos son obligatorios;
- la contraseña nueva debe contener al menos ocho caracteres;
- la confirmación debe coincidir exactamente;
- la contraseña actual debe ser válida;
- solo puede modificarse la contraseña del Usuario autenticado;
- una cuenta desactivada concurrentemente no puede actualizar sus credenciales;
- Persona, nombre de Usuario, estado y Grupos no se modifican;
- después de un cambio correcto se cierra la sesión.

No se incorporó una regla que impida reutilizar la misma contraseña porque esa restricción no está definida en la documentación funcional vigente.

## 4. Seguridad criptográfica

La implementación reutiliza:

`Pbkdf2PasswordHasher`

Algoritmo:

- PBKDF2;
- HMAC-SHA256;
- salt aleatorio de 32 bytes;
- hash de 32 bytes;
- 100000 iteraciones.

En cada cambio correcto se generan:

- un nuevo hash;
- un nuevo salt;
- la cantidad vigente de iteraciones.

No se almacena:

- contraseña en texto plano;
- contraseña reversible;
- clave temporal;
- historial de contraseñas.

La contraseña actual se verifica mediante comparación de hashes en tiempo constante.

## 5. Domain

Archivo modificado:

`src/SIGEVIP.Domain/Entities/Usuario.cs`

Se incorporó:

`ActualizarCredenciales`

La operación:

- valida hash;
- valida salt;
- valida iteraciones;
- copia defensivamente los arreglos;
- reemplaza las credenciales únicamente con datos válidos;
- conserva las credenciales anteriores ante una validación fallida.

## 6. Application

Archivos:

- `src/SIGEVIP.Application/Security/CambiarClaveCommand.cs`;
- `src/SIGEVIP.Application/Security/IUsuarioClaveRepository.cs`;
- `src/SIGEVIP.Application/Security/CambiarClaveService.cs`.

`CambiarClaveCommand` transporta:

- clave actual;
- clave nueva;
- confirmación.

`CambiarClaveService` coordina:

1. validación de sesión;
2. validación del Usuario activo;
3. validación de campos;
4. validación de longitud;
5. validación de confirmación;
6. verificación de la clave actual;
7. generación de nuevas credenciales;
8. actualización persistente;
9. actualización del Usuario en memoria.

La operación no depende de nombres de Grupos ni de permisos administrativos.

## 7. Infrastructure

Archivo:

`src/SIGEVIP.Infrastructure/Security/UsuarioClaveRepository.cs`

La implementación utiliza:

- ADO.NET;
- `System.Data.SqlClient`;
- consulta parametrizada;
- selección explícita de columnas a actualizar;
- `PersistenciaException`;
- validación defensiva del identificador;
- validación defensiva del resultado criptográfico.

La sentencia actualiza exclusivamente:

- `PasswordHash`;
- `PasswordSalt`;
- `IteracionesPassword`.

La condición persistente exige:

- identificador coincidente;
- Usuario activo.

Cuando el Usuario no existe o está inactivo, el repositorio devuelve `false`.

## 8. Base de datos

No fue necesaria una migración nueva.

Se reutilizan las columnas existentes de:

`dbo.Usuario`

Columnas:

- `PasswordHash`;
- `PasswordSalt`;
- `IteracionesPassword`;
- `Activo`.

La estructura creada por la migración de seguridad ya contenía las restricciones necesarias:

- hash obligatorio;
- salt obligatorio;
- longitudes controladas;
- iteraciones mayores que cero;
- estado lógico obligatorio.

## 9. WinForms

Archivo nuevo:

`src/SIGEVIP.WinForms/Forms/CambiarClaveForm.cs`

Archivos modificados:

- `src/SIGEVIP.WinForms/Forms/MainForm.cs`;
- `src/SIGEVIP.WinForms/Navigation/SigevipApplicationContext.cs`;
- `src/SIGEVIP.WinForms/Program.cs`;
- `src/SIGEVIP.WinForms/SIGEVIP.WinForms.csproj`.

El formulario contiene:

- contraseña actual;
- contraseña nueva;
- confirmación;
- botón Cambiar contraseña;
- botón Cancelar.

Los campos utilizan caracteres ocultos.

El menú principal muestra la opción para todos los Usuarios autenticados.

Después de un cambio correcto:

1. se informa el resultado;
2. se cierra el formulario;
3. se cierra el menú;
4. se limpia la sesión;
5. se vuelve al login.

## 10. Pruebas automatizadas

### Domain

Archivo:

`tests/SIGEVIP.Tests/Domain/UsuarioCredencialesTests.cs`

Pruebas:

- actualización válida;
- copia defensiva;
- rechazo de hash vacío;
- rechazo de salt vacío;
- rechazo de iteraciones inválidas.

Cantidad:

5 pruebas.

### Application

Archivo:

`tests/SIGEVIP.Tests/Application/CambiarClaveServiceTests.cs`

Pruebas:

- sesión inexistente;
- Usuario inactivo;
- command nulo;
- campos incompletos;
- contraseña nueva corta;
- confirmación distinta;
- contraseña actual inválida;
- Usuario no disponible al persistir;
- actualización válida.

Cantidad:

9 pruebas.

### Integración SQL

Archivo:

`tests/SIGEVIP.Tests/Integration/UsuarioClaveRepositoryIntegrationTests.cs`

Pruebas:

- actualización real de las credenciales de un Usuario activo;
- rechazo y conservación de credenciales para un Usuario inactivo;
- resultado negativo para un Usuario inexistente.

Cantidad:

3 pruebas.

### Resultado consolidado

Pruebas nuevas:

17.

Regresión total:

- 479 pruebas totales;
- 479 correctas;
- 0 fallidas.

Compilación:

- 0 advertencias;
- 0 errores.

## 11. Validación manual

Se comprobó:

- botón visible;
- distribución correcta;
- campos protegidos;
- rechazo de campos vacíos;
- rechazo de contraseña corta;
- rechazo de confirmación distinta;
- rechazo de contraseña actual incorrecta;
- cambio correcto;
- cierre automático de sesión;
- rechazo de la contraseña anterior;
- aceptación de la contraseña nueva;
- conservación de Grupos y Permisos;
- restauración posterior de la contraseña original;
- ausencia de problemas visuales.

## 12. Commits

- `1b920cc` — `Agrego cambio seguro de clave`
- `74dec4b` — `Agrego interfaz de cambio de clave`

## 13. Alcance pendiente

Permanece pendiente:

- CUD12: Recuperar clave;
- restablecimiento administrativo;
- recuperación por correo;
- contraseña temporal;
- bloqueo por intentos fallidos;
- autenticación multifactor;
- historial de contraseñas.

Estas funciones se clasifican como versión completa o mejoras futuras y no son necesarias para el funcionamiento del MVP actual.

## 14. Estado final

CUD11: Cambiar clave se considera implementado porque cuenta con:

- reglas funcionales;
- seguridad criptográfica;
- Domain;
- Application;
- Infrastructure;
- persistencia SQL;
- interfaz;
- cierre de sesión;
- pruebas unitarias;
- pruebas de integración;
- validación manual reproducible.
