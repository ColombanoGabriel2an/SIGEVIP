# Estado del proyecto

## Etapa actual

Etapa 4: módulos funcionales documentados.

Bloque actual: cierre técnico y documental del módulo Viajes.

## Rama de trabajo

`desarrollo/interfaz-funcional`

## Último commit técnico publicado

- Commit: `88fbd13`
- Mensaje: `Agrego interfaz de gestion de viajes`

La rama local se encuentra sincronizada con:

`origin/desarrollo/interfaz-funcional`

## Arquitectura vigente

La solución utiliza:

- C#.
- .NET Framework 4.8.
- Windows Forms.
- SQL Server.
- ADO.NET.
- MSTest.
- Arquitectura por capas:
  - Domain.
  - Application.
  - Infrastructure.
  - WinForms.
  - Tests.

No se utiliza Entity Framework.

Los proyectos utilizan archivos `.csproj` clásicos con inclusiones explícitas.

## Funcionalidades completadas

### Dominio general

- Cliente.
- Viaje.
- Visita.
- Viático.
- Relación Viaje-Visita.
- Relación muchos a muchos Visita-Cliente.
- Patrón State aplicado a Viaje.
- Reglas de modificación según estado.
- Cálculo de total gastado y saldo pendiente.
- Baja lógica de viáticos.

### Seguridad

- Persona.
- Usuario.
- Grupo.
- Permiso.
- Patrón Composite.
- Autenticación.
- Autorización.
- Sesión actual.
- PBKDF2-HMAC-SHA256.
- Persistencia SQL del modelo de seguridad.
- Reconstrucción de grupos y permisos desde SQL Server.
- Detección de ciclos persistidos.
- Inicialización del administrador.
- Herramienta `SIGEVIP.Setup`.

### Base de datos de seguridad

Tablas:

- `dbo.Persona`
- `dbo.Usuario`
- `dbo.Grupo`
- `dbo.Permiso`
- `dbo.UsuarioGrupo`
- `dbo.GrupoPermiso`
- `dbo.GrupoGrupo`

Migración:

`database/migrations/002_crear_seguridad.sql`

Seed:

`database/seed/001_catalogos_seguridad.sql`

Validación:

`database/migrations/002_validar_seguridad.sql`

Resultado verificado:

`VALIDACIÓN CORRECTA`

### Interfaz de autenticación

Se implementó:

`src/SIGEVIP.WinForms/Forms/LoginForm.cs`

La pantalla permite:

- ingresar nombre de usuario;
- ingresar contraseña oculta;
- autenticar contra SQL Server;
- mostrar un mensaje público genérico ante credenciales inválidas;
- iniciar la sesión en memoria;
- abrir el menú principal después de autenticar;
- cerrar la aplicación desde el login.

### Navegación principal

Se implementó:

`src/SIGEVIP.WinForms/Navigation/SigevipApplicationContext.cs`

Responsabilidades:

- mostrar el login;
- reaccionar ante autenticación correcta;
- cerrar el login sin finalizar el proceso;
- mostrar el menú principal;
- cerrar sesión;
- volver al login;
- finalizar la aplicación;
- limpiar la sesión al salir.

No se utiliza la propiedad `ApplicationContext.MainForm`, porque el cambio entre formularios provocaba el cierre prematuro del ciclo de mensajes.

### Perfil de la persona autenticada

Se implementaron:

- `PerfilSesion`;
- `IPerfilSesionRepository`;
- `PerfilSesionService`;
- `PerfilSesionRepository`.

La interfaz muestra:

- nombre completo de la persona;
- nombre de usuario.

La separación entre Persona y Usuario se conserva.

### Menú principal

Se implementó:

`src/SIGEVIP.WinForms/Forms/MainForm.cs`

El menú:

- muestra el nombre completo;
- muestra el nombre de usuario;
- permite cerrar sesión;
- permite salir;
- presenta únicamente opciones autorizadas;
- oculta las opciones sin permiso;
- separa los accesos de seguridad.

Opciones contempladas:

- Clientes.
- Viajes.
- Visitas.
- Viáticos y rendiciones.
- Usuarios.
- Grupos.
- Permisos.
- Auditoría.

Cada opción se controla mediante permisos efectivos y no mediante nombres de grupos.

### Permisos visuales

Permisos utilizados:

- `CLIENTE_CONSULTAR`
- `CLIENTE_GESTIONAR`
- `VIAJE_CREAR`
- `VIAJE_CONSULTAR`
- `VIAJE_ENVIAR_RENDICION`
- `VIAJE_APROBAR`
- `VIAJE_CANCELAR`
- `VISITA_REGISTRAR`
- `VIATICO_CARGAR`
- `VIATICO_MODIFICAR`
- `VIATICO_EXCLUIR`
- `VIATICO_REACTIVAR`
- `RENDICION_REVISAR`
- `USUARIO_GESTIONAR`
- `GRUPO_GESTIONAR`
- `PERMISO_GESTIONAR`
- `AUDITORIA_CONSULTAR`

Para el administrador actual se verificaron como visibles:

- Clientes.
- Viajes.
- Usuarios.
- Grupos.
- Permisos.
- Auditoría.

Se verificaron como ocultas:

- Visitas.
- Viáticos y rendiciones.

### Eliminación del formulario técnico

Se eliminaron:

- `Form1.cs`
- `Form1.Designer.cs`

Ese formulario solo servía para comprobar la conexión inicial y ya no participaba del flujo funcional.

## Commits relevantes de interfaz

- `bfc8f80` — `Implemento inicio de sesión WinForms`
- `263f3c7` — `Implemento menu principal y cierre de sesion`
- `af860a9` — `Incorporo perfil de usuario autenticado`
- `cc6e19e` — `Aplico permisos visuales y retiro formulario tecnico`

## Resultado técnico verificado

### Compilación

- 0 advertencias.
- 0 errores.
- Compilación completa mediante MSBuild de Visual Studio 2022.

### Pruebas

- Totales: 246.
- Correctas: 246.
- Fallidas: 0.

Incluyen:

- pruebas unitarias;
- pruebas de dominio;
- pruebas de Application;
- pruebas de Infrastructure;
- pruebas de integración real con SQL Server;
- pruebas del perfil de sesión.

Ejecutor:

`VSTest 17.13.0 x64`

### Validación manual

Se verificó:

- apertura del login;
- rechazo de credenciales inválidas;
- autenticación correcta;
- apertura del menú principal;
- visualización del nombre completo;
- visualización del nombre de usuario;
- opciones visibles según permisos;
- opciones no autorizadas ocultas;
- cierre de sesión;
- retorno al login;
- nueva autenticación;
- salida desde botón;
- salida desde la cruz;
- un único mensaje de confirmación al cerrar;
- maximización y restauración de ventana.

## Estado de requisitos de seguridad

### RF01 — Iniciar sesión

Implementado.

Incluye:

- interfaz gráfica;
- validación de entradas;
- autenticación persistente;
- PBKDF2;
- mensaje público genérico;
- inicio de sesión;
- transición al menú principal.

### RF02 — Validar credenciales y habilitar funciones

Implementado para el flujo principal.

Incluye:

- recuperación del usuario;
- reconstrucción de grupos y permisos;
- inicio de sesión;
- autorización;
- habilitación visual por permisos efectivos.

Las operaciones funcionales concretas de cada módulo deberán validar nuevamente los permisos en Application.

### RF03 — Gestionar usuarios

Parcialmente implementado.

Implementado:

- entidad Usuario;
- persistencia base;
- grupos;
- permisos;
- acceso visual autorizado.

Pendiente:

- casos de uso de mantenimiento;
- repositorios de mantenimiento;
- formulario funcional.

### RF04 — Asignar uno o más grupos

Parcialmente implementado.

Implementado:

- modelo de dominio;
- tabla `UsuarioGrupo`;
- asignación inicial del administrador.

Pendiente:

- mantenimiento desde casos de uso e interfaz.

### RF37 — Impedir accesos no autorizados

Implementado en:

- Domain;
- Application;
- Infrastructure;
- menú principal WinForms.

Pendiente:

- aplicar la misma autorización dentro de cada caso de uso funcional.

### RF38 — Ocultar opciones no habilitadas

Implementado en el menú principal.

Los controles no autorizados utilizan:

`Visible = false`

### RF39 — Acceso mediante grupos y permisos

Implementado.

El acceso no depende de roles codificados en WinForms.

Se utilizan:

- Usuario;
- Grupo;
- Permiso;
- GrupoPermiso;
- GrupoGrupo;
- permisos efectivos.

## Pendiente inmediato

- Completar el cierre documental del módulo Clientes.
- Ejecutar regresión final.
- Publicar el commit documental.
- Preparar informe de transferencia a MAESTRO.

## Etapas posteriores

- Gestión funcional de clientes.
- Persistencia de clientes.
- Listado y filtros.
- Activación y desactivación.
- Historial del cliente.
- Gestión de viajes.
- Gestión de visitas.
- Gestión de viáticos y rendiciones.
- Gestión visual de usuarios.
- Gestión visual de grupos.
- Gestión visual de permisos.
- Cambio de contraseña.
- Recuperación de contraseña.
- Auditoría persistente.
- Reportes.
- Datos de demostración.
- Manual técnico.
- Preparación de la presentación académica.

## Módulo funcional de Clientes

El módulo Clientes se encuentra implementado de extremo a extremo.

### Domain

Se implementó:

- validación de razón social obligatoria;
- validación de CUIT obligatorio;
- normalización de CUIT;
- actualización controlada de datos;
- reconstrucción desde persistencia;
- activación y desactivación lógica.

### Application

Se implementaron:

- `ClienteFiltro`;
- `ClienteListadoDto`;
- `IClienteRepository`;
- `ClienteService`;
- `AccesoDenegadoException`.

Casos de uso disponibles:

- listar clientes;
- obtener cliente;
- registrar cliente;
- modificar cliente;
- activar cliente;
- desactivar cliente.

Cada operación valida:

- sesión autenticada;
- usuario activo;
- permiso requerido;
- identificadores;
- existencia del cliente;
- unicidad de CUIT.

### Infrastructure

Se implementó:

`src/SIGEVIP.Infrastructure/Clientes/ClienteRepository.cs`

Responsabilidades:

- persistir clientes mediante ADO.NET;
- obtener por identificador;
- verificar existencia de CUIT;
- listar con filtros;
- insertar;
- actualizar;
- activar;
- desactivar;
- reconstruir la entidad;
- traducir errores técnicos;
- reconocer conflictos por índice único.

### Base de datos

Tabla:

`dbo.Cliente`

Scripts:

- `database/migrations/003_crear_clientes.sql`
- `database/seed/002_permisos_modulo_clientes.sql`
- `database/migrations/003_validar_clientes.sql`

Validación ejecutada:

`VALIDACIÓN CORRECTA`

### WinForms

Se implementaron:

- `ClientesForm`;
- `ClienteEditForm`;
- navegación desde `MainForm`;
- coordinación desde `SigevipApplicationContext`;
- composición de dependencias en `Program`.

La interfaz permite:

- listar;
- filtrar;
- registrar;
- modificar;
- activar;
- desactivar;
- visualizar estados;
- rechazar CUIT duplicado;
- conservar datos entre ejecuciones.

### Commits del módulo

- `0994a78` — `Agrego casos de uso de clientes`
- `c8a36e3` — `Implemento persistencia de clientes`
- `8f45694` — `Agrego interfaz de gestion de clientes`

## Módulo funcional de Viajes

El módulo Viajes se encuentra implementado de extremo a extremo dentro del alcance aprobado.

### Domain

Se implementó:

- descripción obligatoria;
- período válido;
- tipo de viaje válido;
- monto anticipado no negativo;
- participantes obligatorios;
- rechazo de participantes duplicados;
- relación muchos a muchos Viaje-Persona;
- modificación únicamente en estado Abierto;
- cancelación condicionada por reglas de estado;
- reconstrucción desde persistencia;
- preservación histórica de participantes inactivos.

### Application

Se implementaron:

- `ViajeFiltro`;
- `ViajeListadoDto`;
- `PersonaSeleccionDto`;
- `IViajeRepository`;
- `IPersonaConsultaRepository`;
- `ViajeService`.

Casos de uso disponibles:

- listar viajes;
- obtener un viaje;
- listar participantes activos;
- registrar viaje;
- modificar viaje;
- cancelar viaje.

Cada operación valida:

- sesión autenticada;
- Usuario activo;
- permiso requerido;
- identificadores;
- existencia del Viaje;
- existencia y actividad de participantes;
- reglas de estado.

### Infrastructure

Se implementaron:

- `PersonaConsultaRepository`;
- `ViajeRepository`.

Responsabilidades:

- consultar Personas activas;
- recuperar Personas activas o inactivas por identificador;
- insertar Viaje y participantes dentro de una transacción;
- obtener el agregado;
- actualizar datos y participantes;
- cancelar;
- listar con filtros;
- generar resumen de participantes;
- traducir errores técnicos;
- revertir operaciones incompletas.

### Base de datos

Tablas:

- `dbo.Viaje`;
- `dbo.ViajeParticipante`.

Scripts:

- `database/migrations/004_crear_viajes.sql`;
- `database/seed/003_permisos_modulo_viajes.sql`;
- `database/migrations/004_validar_viajes.sql`.

Validación ejecutada:

`VALIDACIÓN CORRECTA`

### WinForms

Se implementaron:

- `ViajesForm`;
- `ViajeEditForm`;
- `ViajeDetalleForm`;
- navegación desde `MainForm`;
- coordinación desde `SigevipApplicationContext`;
- composición en `Program`.

La interfaz permite:

- listar;
- filtrar;
- registrar;
- modificar;
- consultar detalle;
- cancelar;
- seleccionar varios participantes;
- visualizar participantes inactivos históricos;
- aplicar permisos visuales;
- mostrar mensajes de validación y errores controlados.

### Commits del módulo

- `8226625` — `Agrego casos de uso de viajes`
- `6914a1d` — `Implemento persistencia de viajes`
- `88fbd13` — `Agrego interfaz de gestion de viajes`

### Alcance parcial

Permanece pendiente:

- persistencia de Visitas;
- persistencia de Viáticos;
- total gastado y saldo persistidos;
- bloqueo de cancelación por Visitas persistidas;
- historial completo del Viaje.

## Pendiente inmediato

- completar el cierre documental del módulo Viajes;
- ejecutar regresión final;
- publicar el commit documental;
- preparar informe de transferencia a MAESTRO.
