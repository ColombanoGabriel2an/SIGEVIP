# Pruebas de gestión de Usuarios

## 1. Objetivo

Este documento registra la validación técnica y funcional del módulo de gestión de Usuarios de SIGEVIP.

El módulo cubre los requisitos:

- RF03: gestionar Usuarios;
- RF04: asignar uno o más Grupos;
- CUD11: cambiar la propia clave;
- RF37: impedir accesos no autorizados;
- RF38: ocultar opciones no habilitadas;
- RF39: autorizar mediante Grupos y Permisos.

## 2. Alcance implementado

Se implementaron:

- listado de Usuarios;
- búsqueda por Usuario, Persona, email o Grupo;
- filtro por estado;
- filtro por Grupo;
- consulta de detalle;
- creación de Usuario;
- selección de una Persona existente;
- contraseña inicial;
- cambio seguro de contraseña del Usuario autenticado;
- asignación de uno o varios Grupos directos;
- modificación del nombre de Usuario;
- reemplazo completo de Grupos directos;
- activación lógica;
- desactivación lógica;
- protección del Usuario autenticado;
- protección del último Administrador activo;
- aplicación de cambios de permisos en la siguiente autenticación;
- navegación desde el menú principal;
- ocultamiento del módulo sin `USUARIO_GESTIONAR`.

## 3. Alcance excluido

No se implementaron en este bloque:

- alta o modificación de Personas;
- gestión funcional de Grupos;
- gestión funcional de Permisos;
- asignación visual de Permisos;
- jerarquías visuales entre Grupos;
- recuperación de contraseña;
- bloqueo por intentos fallidos;
- autenticación multifactor;
- auditoría general consultable.

Las Personas utilizadas para crear Usuarios deben existir previamente, encontrarse activas y no poseer otro Usuario.

## 4. Reglas funcionales comprobadas

### Persona y Usuario

- una Persona puede tener como máximo un Usuario;
- la Persona debe existir;
- la Persona debe estar activa;
- la Persona no puede modificarse desde la gestión de Usuarios;
- la asociación Persona-Usuario se conserva después del alta.

### Nombre de Usuario

- es obligatorio;
- se elimina espacio exterior;
- se normaliza a minúsculas;
- debe ser único;
- puede modificarse;
- la modificación no altera la contraseña existente.

### Contraseña inicial

- solo se solicita durante el alta;
- es obligatoria;
- debe tener al menos ocho caracteres;
- debe coincidir con su confirmación;
- se procesa mediante PBKDF2-HMAC-SHA256;
- no se persiste en texto plano;
- no se muestra durante la modificación.

La longitud mínima de ocho caracteres se incorporó como requisito técnico obligatorio de seguridad para el funcionamiento del alta.

### Grupos

- debe asignarse al menos un Grupo activo;
- pueden asignarse varios Grupos;
- no se permiten identificadores duplicados;
- no se asignan Permisos directamente al Usuario;
- la modificación reemplaza la colección completa de Grupos directos;
- el reemplazo se ejecuta dentro de una transacción;
- los nuevos permisos se aplican al iniciar una nueva sesión.

### Protecciones administrativas

- el Usuario autenticado puede visualizarse;
- el Usuario autenticado no puede modificarse durante su sesión;
- el Usuario autenticado no puede desactivarse;
- no puede retirarse `ADMINISTRADOR_GENERAL` al último Administrador activo;
- no puede desactivarse al último Administrador activo;
- ningún Usuario puede quedar sin Grupos.

## 5. Implementación por capa

### Domain

Archivo principal:

`src/SIGEVIP.Domain/Entities/Usuario.cs`

Se incorporaron:

- `ActualizarNombreUsuario`;
- `ActualizarCredenciales`;
- `ReemplazarGrupos`;
- validación previa de la colección completa;
- rechazo de Grupos nulos;
- rechazo de Grupos repetidos;
- conservación del estado anterior si la nueva colección es inválida.

### Application

Carpeta:

`src/SIGEVIP.Application/Usuarios`

Componentes:

- `UsuarioFiltro`;
- `UsuarioListadoDto`;
- `UsuarioDetalleDto`;
- `PersonaSeleccionUsuarioDto`;
- `GrupoSeleccionUsuarioDto`;
- `RegistrarUsuarioCommand`;
- `ModificarUsuarioCommand`;
- `IUsuarioGestionRepository`;
- `UsuarioGestionService`.
- `CambiarClaveCommand`;
- `IUsuarioClaveRepository`;
- `CambiarClaveService`.

`UsuarioGestionService` valida:

- sesión autenticada;
- Usuario activo;
- permiso `USUARIO_GESTIONAR`;
- Persona;
- nombre de Usuario;
- contraseña inicial;
- Grupos;
- protecciones del Usuario autenticado;
- protecciones del último Administrador.

### Infrastructure

Archivo:

`src/SIGEVIP.Infrastructure/Usuarios/UsuarioGestionRepository.cs`

También se implementó:

`src/SIGEVIP.Infrastructure/Security/UsuarioClaveRepository.cs`

La implementación utiliza:

- ADO.NET;
- `System.Data.SqlClient`;
- consultas parametrizadas;
- selección explícita de columnas;
- transacciones SQL;
- `SCOPE_IDENTITY`;
- traducción de errores SQL `2601` y `2627`;
- `PersistenciaException`;
- validaciones defensivas de concurrencia.

Casos persistentes:

- listar;
- obtener detalle;
- reconstruir Usuario;
- obtener Persona;
- listar Personas disponibles;
- listar Grupos activos;
- obtener Grupos por identificador;
- verificar nombre de Usuario;
- verificar asociación de Persona;
- verificar otro Administrador activo;
- insertar Usuario y Grupos;
- actualizar nombre y Grupos;
- activar;
- desactivar.

### WinForms

Formularios:

- `src/SIGEVIP.WinForms/Forms/UsuariosForm.cs`;
- `src/SIGEVIP.WinForms/Forms/UsuarioEditForm.cs`.
- `src/SIGEVIP.WinForms/Forms/CambiarClaveForm.cs`.

Integración:

- `MainForm`;
- `SigevipApplicationContext`;
- `Program`.

La pantalla permite:

- listar;
- buscar;
- filtrar;
- registrar;
- modificar;
- activar;
- desactivar;
- visualizar el Usuario autenticado;
- deshabilitar operaciones prohibidas;
- mostrar errores funcionales y técnicos controlados.

## 6. Pruebas automatizadas

### Dominio

Archivo:

`tests/SIGEVIP.Tests/Domain/UsuarioTests.cs`

Cobertura agregada:

- actualización normalizada del nombre;
- rechazo de nombre vacío;
- reemplazo de Grupos;
- rechazo de colección nula;
- rechazo de colección vacía;
- rechazo de Grupo nulo;
- rechazo de Grupos duplicados;
- preservación de la colección ante reemplazo inválido.

### Application

Archivo:

`tests/SIGEVIP.Tests/Application/UsuarioGestionServiceTests.cs`

Cobertura:

- sesión inexistente;
- Usuario inactivo;
- permiso faltante;
- listado autorizado;
- Persona inexistente;
- Persona inactiva;
- Persona con Usuario;
- nombre vacío;
- nombre duplicado;
- contraseña corta;
- confirmación incorrecta;
- falta de Grupos;
- Grupo inexistente;
- Grupo inactivo;
- alta válida;
- alta con varios Grupos;
- modificación;
- reemplazo de Grupos;
- protección del Usuario autenticado;
- protección del último Administrador;
- activación;
- desactivación.

### Integración SQL

Archivo:

`tests/SIGEVIP.Tests/Integration/UsuarioGestionRepositoryIntegrationTests.cs`

Cobertura:

- listado persistente;
- resumen de Grupos;
- filtro por estado;
- filtro por Grupo;
- detalle y Grupos directos;
- Personas disponibles;
- alta transaccional;
- nombre duplicado;
- modificación transaccional;
- reemplazo de Grupos;
- activación y desactivación;
- existencia de otro Administrador activo.

## 7. Resultado automatizado

Compilación completa:

- 0 advertencias;
- 0 errores.

Pruebas:

- totales: 479;
- correctas: 479;
- fallidas: 0.

Herramientas:

- MSBuild de Visual Studio 2022;
- VSTest 17.13;
- MSTest;
- SQL Server real para pruebas de integración.

## 8. Validación manual

Se comprobó:

- apertura del módulo desde el menú principal;
- visibilidad mediante `USUARIO_GESTIONAR`;
- carga del listado;
- búsqueda general;
- filtro por estado;
- filtro por Grupo;
- indicador del Usuario autenticado;
- bloqueo visual de modificación del Usuario autenticado;
- bloqueo visual de desactivación del Usuario autenticado;
- validaciones obligatorias del alta;
- rechazo de contraseña menor a ocho caracteres;
- rechazo de confirmación diferente;
- creación correcta de un Usuario;
- asociación correcta con Persona;
- asignación de Grupo;
- modificación del nombre;
- reemplazo del Grupo;
- autenticación mediante la contraseña inicial;
- conservación de la contraseña después del cambio de nombre;
- actualización de permisos en una nueva sesión;
- ocultamiento del botón Usuarios para una cuenta sin permiso;
- desactivación lógica;
- rechazo de autenticación del Usuario inactivo;
- reactivación;
- autenticación posterior a la reactivación.
- cambio de contraseña del Usuario autenticado;
- validación de contraseña actual;
- rechazo de contraseña corta;
- rechazo de confirmación distinta;
- cierre automático de sesión;
- rechazo de la contraseña anterior;
- aceptación de la contraseña nueva;
- restauración de la contraseña original.

## 9. Datos temporales de prueba

Para la prueba manual se creó temporalmente una Persona:

- nombre: Persona;
- apellido: Demostración Usuarios;
- email: `persona.demo.usuario@sigevip.test`.

Se verificó inicialmente:

- activa;
- sin Usuario.

Después del alta se verificó:

- asociación Persona-Usuario creada.

Finalizada la validación se eliminaron, mediante una transacción controlada:

- asignaciones de Grupos;
- Usuario de prueba;
- Persona de prueba.

La consulta final no devolvió registros temporales.

## 10. Commits del módulo

- `e13d291` — `Agrego casos de uso de usuarios`
- `ad7772c` — `Agrego persistencia de usuarios`
- `9460bf9` — `Completo interfaz de gestion de usuarios`
- `1b920cc` — `Agrego cambio seguro de clave`
- `74dec4b` — `Agrego interfaz de cambio de clave`

## 11. Estado final

El módulo de gestión de Usuarios se considera implementado dentro del alcance definido porque cuenta con:

- reglas de dominio;
- casos de uso;
- autorización;
- persistencia;
- transacciones;
- interfaz;
- navegación;
- validaciones;
- pruebas unitarias;
- pruebas de integración SQL;
- validación manual reproducible.
- cambio seguro de la contraseña propia.

Permanecen como módulos posteriores:

- gestión de Grupos;
- gestión de Permisos;
- recuperación de contraseña;
- auditoría general.
