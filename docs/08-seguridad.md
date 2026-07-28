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
- PBKDF2-HMAC-SHA256.
- Persistencia SQL.
- Repositorio ADO.NET de autenticación.
- Reconstrucción de permisos efectivos.
- Inicialización del administrador.
- Interfaz de login.
- Perfil de la persona autenticada.
- Menú principal autorizado.
- Cierre de sesión.
- Ocultamiento de opciones no autorizadas.
- Mantenimiento funcional de Usuarios.
- Asignación de uno o varios Grupos directos.
- Activación y desactivación lógica de Usuarios.
- Protección del Usuario autenticado.
- Protección del último Administrador activo.
- Cambio seguro de clave del Usuario autenticado.

No incluye todavía:

- mantenimiento de grupos;
- mantenimiento de permisos;
- recuperación por correo;
- bloqueo por intentos fallidos;
- autenticación multifactor;
- auditoría persistente;
- autorización dentro de todos los casos de uso funcionales.

## 2. Persona y Usuario

Persona y Usuario son entidades separadas.

Persona contiene información personal:

- nombre;
- apellido;
- email;
- estado.

Usuario contiene:

- identidad de acceso;
- hash;
- salt;
- iteraciones;
- estado;
- grupos asignados.

La relación persistente es:

    Persona 1 -------- 0..1 Usuario

Se implementa mediante:

- `FK_Usuario_Persona`;
- `UX_Usuario_IdPersona`.

## 3. Grupos y permisos

La autorización no utiliza roles fijos en WinForms.

Los roles funcionales se modelan mediante grupos configurables.

Un Grupo puede contener:

- permisos simples;
- otros grupos.

La persistencia utiliza:

- `UsuarioGrupo`;
- `GrupoPermiso`;
- `GrupoGrupo`.

## 4. Permisos efectivos

Los permisos efectivos son la unión de los permisos de todos los grupos activos del usuario.

Se consideran:

- permisos directos;
- permisos heredados;
- grupos activos;
- permisos activos.

Se excluyen:

- grupos inactivos;
- grupos hijos inactivos;
- permisos inactivos.

Los permisos repetidos se eliminan mediante código normalizado.

## 5. Prevención de ciclos

Grupo rechaza:

- autorreferencia;
- duplicados;
- ciclos directos;
- ciclos indirectos.

La base de datos impide la autorreferencia directa.

La reconstrucción desde persistencia detecta ciclos indirectos y produce `PersistenciaException`.

## 6. Autenticación

`AutenticacionService` coordina:

1. Validación de entrada.
2. Normalización del nombre.
3. Consulta del Usuario.
4. Validación de estado.
5. Verificación de contraseña.
6. Construcción del resultado.

El mensaje público para credenciales inválidas es:

`Credenciales inválidas.`

No se informa si falló:

- el nombre de usuario;
- la contraseña;
- el estado del usuario.

## 7. Repositorio de autenticación

La implementación concreta es:

`SIGEVIP.Infrastructure.Security.UsuarioAutenticacionRepository`

Utiliza:

- ADO.NET;
- `System.Data.SqlClient`;
- consultas parametrizadas;
- selección explícita de columnas;
- una única ejecución con múltiples resultados.

Recupera:

- Usuario;
- Persona asociada;
- grupos;
- permisos;
- asociaciones Grupo-Permiso;
- grupos hijos;
- grupos asignados al usuario.

## 8. Hash de contraseñas

Implementación:

`Pbkdf2PasswordHasher`

Parámetros:

- PBKDF2;
- HMAC-SHA256;
- salt de 32 bytes;
- hash de 32 bytes;
- 100000 iteraciones.

No se almacena:

- contraseña en texto plano;
- contraseña reversible;
- clave global fija.

## 9. Comparación de hashes

La comparación acumula diferencias entre todos los bytes.

No finaliza al detectar el primer byte distinto.

Esto reduce variaciones temporales observables.

## 10. Sesión actual

`SesionActual` es una implementación en memoria.

No es estática.

Permite:

- iniciar sesión;
- consultar el usuario;
- cerrar sesión.

Rechaza:

- usuario nulo;
- usuario inactivo.

## 11. Perfil de sesión

La interfaz no incorpora datos personales dentro de `Usuario`.

Se implementaron:

- `PerfilSesion`;
- `IPerfilSesionRepository`;
- `PerfilSesionService`;
- `PerfilSesionRepository`.

El perfil consulta `Persona` por `IdPersona` y expone:

- nombre;
- apellido;
- email;
- nombre completo.

## 12. Integración WinForms

El formulario de login utiliza:

- `AutenticacionService`;
- `SesionActual`;
- `UsuarioAutenticacionRepository`;
- `Pbkdf2PasswordHasher`.

Después de autenticar:

1. se inicia la sesión;
2. se recupera el perfil;
3. se cierra el login;
4. se muestra el menú principal.

La navegación se coordina mediante:

`SigevipApplicationContext`

## 13. Autorización visual

El menú utiliza:

`AutorizacionService.TienePermiso()`

No consulta nombres de grupos.

Las opciones no autorizadas se configuran con:

`Visible = false`

Accesos separados:

- Clientes.
- Viajes.
- Visitas.
- Viáticos y rendiciones.
- Usuarios.
- Grupos.
- Permisos.
- Auditoría.

Dentro de Viáticos y Rendiciones se aplican permisos específicos para:

- consultar Viáticos;
- registrar Viáticos;
- modificar Viáticos;
- enviar Viajes a rendición;
- revisar Rendiciones;
- excluir Viáticos;
- reactivar Viáticos;
- ajustar anticipos;
- aprobar Rendiciones;
- cancelar Rendiciones.

La autorización se vuelve a validar en `ViaticoService` y `RendicionService`.

## 14. Cierre de sesión

Al cerrar sesión:

1. se cierra el menú;
2. se limpia `SesionActual`;
3. se vuelve a mostrar el login.

El usuario debe autenticarse nuevamente para regresar al menú.

## 15. Salida de la aplicación

La salida puede solicitarse mediante:

- botón Salir;
- cruz de la ventana.

Se muestra una única confirmación.

Al salir:

- se cierran los formularios;
- se limpia la sesión;
- se finaliza el hilo de interfaz.

## 16. Bajas lógicas

Las tablas principales de seguridad utilizan:

`Activo BIT NOT NULL DEFAULT 1`

No se utiliza borrado físico funcional.

Las claves foráneas usan:

`NO_ACTION`

## 17. Administrador inicial

La herramienta:

`tools/SIGEVIP.Setup`

permite crear el administrador inicial.

Características:

- contraseña oculta;
- confirmación;
- PBKDF2;
- transacción;
- grupo `ADMINISTRADOR_GENERAL`;
- detección de duplicados.

## 18. Validaciones ejecutadas

### SQL Server

- migración aplicada;
- seed reejecutado sin duplicados;
- claves foráneas verificadas;
- restricciones verificadas;
- índices verificados;
- Composite reconstruido desde datos reales.

### Pruebas automatizadas

- 479 pruebas totales;
- 479 correctas;
- 0 fallidas.

La regresión incluye seguridad, Gestión de Usuarios, Clientes, Viajes, Visitas, Viáticos y Rendiciones.

### Interfaz

Se verificó:

- login;
- credenciales inválidas;
- credenciales correctas;
- menú principal;
- nombre completo;
- usuario;
- permisos visuales;
- opciones ocultas;
- cierre de sesión;
- retorno al login;
- salida controlada.

## 19. Gestión funcional de Usuarios

La gestión funcional de Usuarios utiliza:

- `UsuarioGestionService`;
- `IUsuarioGestionRepository`;
- `UsuarioGestionRepository`;
- `UsuariosForm`;
- `UsuarioEditForm`.

El acceso requiere:

`USUARIO_GESTIONAR`

La autorización se valida tanto en la interfaz como en Application.

### Alta

El alta exige:

- Persona existente;
- Persona activa;
- Persona sin otro Usuario;
- nombre de Usuario único;
- contraseña inicial;
- confirmación coincidente;
- longitud mínima de ocho caracteres;
- al menos un Grupo activo.

La contraseña inicial se transforma mediante:

`Pbkdf2PasswordHasher`

No se persiste ni registra en texto plano.

### Modificación

La modificación permite:

- cambiar el nombre de Usuario;
- reemplazar la totalidad de los Grupos directos.

No permite:

- cambiar la Persona asociada;
- modificar la contraseña;
- modificar al Usuario autenticado durante la sesión;
- dejar al Usuario sin Grupos.

### Persistencia transaccional

El alta ejecuta dentro de una transacción:

1. validación defensiva de Persona;
2. validación de nombre;
3. validación de Grupos;
4. inserción de Usuario;
5. inserción de `UsuarioGrupo`;
6. commit.

La modificación ejecuta dentro de una transacción:

1. validación de nombre;
2. validación de Grupos;
3. actualización del nombre;
4. eliminación de asignaciones anteriores;
5. inserción de las nuevas asignaciones;
6. commit.

Ante cualquier error se ejecuta rollback.

### Protecciones administrativas

Se impide:

- modificar al Usuario autenticado;
- desactivar al Usuario autenticado;
- desactivar al último Administrador activo;
- retirar `ADMINISTRADOR_GENERAL` al último Administrador activo;
- dejar a un Usuario sin Grupos.

Los cambios de autorización se reflejan al iniciar una nueva sesión.

## 20. Validación del módulo Usuarios

Se ejecutaron:

- pruebas de Domain;
- pruebas de Application;
- pruebas de integración SQL;
- validación manual del ejecutable.

Resultado:

- 0 advertencias;
- 0 errores;
- 479 pruebas correctas;
- 0 pruebas fallidas.

La validación manual comprobó:

- alta;
- modificación;
- asignación de Grupos;
- autenticación con contraseña inicial;
- ocultamiento de opciones;
- desactivación;
- bloqueo de autenticación del Usuario inactivo;
- reactivación;
- protecciones sobre el Usuario autenticado.

## 21. Cambio de clave

La opción Cambiar contraseña se encuentra disponible para todo Usuario autenticado.

No requiere un permiso administrativo porque la operación solo afecta las credenciales del Usuario de la sesión.

Componentes:

- `CambiarClaveCommand`;
- `CambiarClaveService`;
- `IUsuarioClaveRepository`;
- `UsuarioClaveRepository`;
- `CambiarClaveForm`.

La operación valida:

- sesión activa;
- Usuario activo;
- contraseña actual;
- contraseña nueva;
- confirmación;
- longitud mínima de ocho caracteres.

La contraseña actual se verifica mediante PBKDF2.

La contraseña nueva genera:

- nuevo hash;
- nuevo salt;
- iteraciones vigentes.

Después de persistir correctamente:

1. se actualiza el Usuario en memoria;
2. se cierra el formulario;
3. se cierra la sesión;
4. se vuelve al login.

No se modifican:

- Persona;
- nombre de Usuario;
- Grupos;
- Permisos;
- estado.

Resultado:

- 17 pruebas nuevas;
- 479 pruebas totales correctas;
- validación manual aprobada.

CUD12: Recuperar clave continúa pendiente.

## 22. Responsabilidades pendientes

### Application

- recuperación de contraseña;
- gestión de grupos;
- gestión de permisos;
- auditoría;
- autorización dentro de cada operación funcional.

### Infrastructure

- persistencia de cambios de grupos administrados desde un módulo específico de Grupos;
- persistencia de cambios de permisos;
- auditoría.

### WinForms

- formulario funcional de grupos;
- formulario funcional de permisos;
- pantalla de auditoría;
- recuperación de contraseña.

## 23. Estado académico

El flujo de seguridad básico del MVP está implementado y comprobado:

- autenticación;
- autorización;
- sesión;
- perfil;
- permisos visuales;
- cierre de sesión;
- salida.

La gestión funcional de Usuarios y sus Grupos directos se encuentra implementada y comprobada.

Continúan pendientes para etapas posteriores:

- gestión del catálogo de Grupos;
- gestión del catálogo de Permisos;
- recuperación de contraseña;
- auditoría general.
