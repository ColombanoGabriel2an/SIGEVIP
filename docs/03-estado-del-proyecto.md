# Estado del proyecto

## Etapa actual

Etapa 4: módulos funcionales documentados.

Bloque actual: cierre técnico y documental de Gestión de Permisos.

## Rama de trabajo

`desarrollo/interfaz-funcional`

## Último cierre funcional publicado

- Commit: `2c16ef2`
- Mensaje: `Completo interfaz de gestion de permisos`

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

Se verificó que la visibilidad se determina mediante permisos efectivos.

El módulo Visitas se muestra a los usuarios que poseen:

- `VISITA_REGISTRAR`

El módulo Viáticos y Rendiciones se encuentra implementado de extremo a extremo.

Incluye:

- carga y modificación de Viáticos;
- Comprobantes opcionales;
- Personas pagadoras;
- consulta y filtros;
- envío a rendición;
- revisión de Rendiciones pendientes;
- exclusión y reactivación lógica;
- ajuste de anticipos;
- aprobación;
- cancelación;
- permisos visuales y autorización en Application.

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

- Totales: 594.
- Correctas: 594.
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

Implementado.

Incluye:

- listado;
- búsqueda;
- filtros;
- detalle;
- alta;
- modificación;
- activación;
- desactivación lógica;
- validación de Persona;
- contraseña inicial segura;
- repositorio ADO.NET;
- interfaz funcional;
- navegación;
- pruebas unitarias;
- pruebas de integración SQL;
- validación manual.

### RF04 — Asignar uno o más grupos

Implementado.

Incluye:

- selección de uno o varios Grupos activos;
- rechazo de asignaciones vacías;
- rechazo de duplicados;
- reemplazo completo de Grupos directos;
- persistencia transaccional;
- protección del último Administrador activo;
- aplicación de los cambios en la siguiente autenticación.

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

- Completar el cierre documental del módulo Gestión de Permisos.
- Ejecutar regresión documental final.
- Publicar el cierre documental.
- Preparar el siguiente módulo funcional.

## Etapas posteriores

- Gestión visual de jerarquías entre Grupos.
- Recuperación de contraseña.
- Historial funcional completo del Cliente.
- Auditoría general consultable.
- Reportes.
- Mapa y geolocalización.
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

- persistencia de Viáticos;
- total gastado y saldo persistidos;
- historial completo de Viáticos del Viaje.

La reconstrucción de Visitas y el bloqueo de cancelación por Visitas persistidas ya se encuentran implementados.

## Módulo funcional de Visitas

El módulo Visitas se encuentra implementado de extremo a extremo dentro del alcance aprobado.

### Domain

Se implementó:

- fecha de Visita obligatoriamente comprendida dentro del período del Viaje;
- observación obligatoria;
- localidad del encuentro obligatoria;
- asociación de una Visita con un único Viaje;
- asociación de uno o varios Clientes;
- rechazo de Clientes duplicados;
- preservación histórica de Clientes inactivos;
- reconstrucción desde persistencia;
- bloqueo de cancelación de Viajes con Visitas.

### Application

Se implementaron:

- `VisitaListadoDto`;
- `ClienteSeleccionVisitaDto`;
- `IVisitaRepository`;
- `IClienteConsultaVisitaRepository`;
- `VisitaService`.

Casos de uso disponibles:

- listar Clientes activos disponibles;
- registrar una Visita;
- listar Visitas por Viaje;
- listar Visitas por Cliente.

Las operaciones validan:

- sesión autenticada;
- Usuario activo;
- permisos requeridos;
- existencia del Viaje;
- existencia y actividad de Clientes;
- selección de al menos un Cliente;
- ausencia de duplicados;
- reglas temporales del Viaje.

### Infrastructure

Se implementaron:

- `VisitaRepository`;
- `ClienteConsultaVisitaRepository`;
- reconstrucción de Visitas desde `ViajeRepository`.

Responsabilidades:

- insertar Visitas y asociaciones dentro de una transacción;
- consultar Visitas por Viaje;
- consultar Visitas por Cliente;
- consultar Clientes activos;
- recuperar Clientes históricos activos o inactivos;
- reconstruir el agregado Viaje con sus Visitas;
- impedir en SQL la cancelación de Viajes con Visitas;
- revertir operaciones incompletas;
- traducir errores técnicos.

### Base de datos

Tablas:

- `dbo.Visita`;
- `dbo.VisitaCliente`.

Scripts:

- `database/migrations/005_crear_visitas.sql`;
- `database/seed/004_permisos_modulo_visitas.sql`;
- `database/migrations/005_validar_visitas.sql`.

Validación ejecutada:

`VALIDACIÓN CORRECTA`

### WinForms

Se implementaron:

- `VisitasForm`;
- `VisitaEditForm`;
- navegación desde `MainForm`;
- coordinación desde `SigevipApplicationContext`;
- composición de dependencias en `Program`.

La interfaz permite:

- seleccionar un Viaje;
- consultar sus Visitas;
- registrar una nueva Visita;
- limitar la fecha al período del Viaje;
- seleccionar uno o varios Clientes activos;
- visualizar fecha, localidad, observación y Clientes;
- actualizar el listado después del alta;
- mostrar validaciones y errores controlados.

### Commits del módulo

- `430761e` — `Agrego casos de uso de visitas`
- `c76a96e` — `Creo esquema SQL de visitas`
- `c6ab260` — `Implemento persistencia de visitas`
- `d45fb75` — `Reconstruyo viajes con visitas persistidas`
- `67d3df2` — `Agrego formularios de visitas`
- `3b13f58` — `Conecto modulo de visitas al menu`

### Validación técnica

- compilación con 0 advertencias y 0 errores;
- 415 pruebas de regresión general correctas;
- 0 pruebas fallidas;
- prueba manual completa del ejecutable;
- alta y consulta de Visitas verificadas;
- selección múltiple de Clientes verificada;
- bloqueo de cancelación verificado.

## Cierre posterior del módulo Viáticos y Rendiciones

Después del cierre de Visitas se completaron:

- dominio de Viáticos y Rendiciones;
- casos de uso de Application;
- migración y validación SQL;
- persistencia ADO.NET;
- reconstrucción completa del Viaje;
- gestión de Viáticos y Comprobantes;
- envío y revisión de Rendiciones;
- interfaz funcional;
- 415 pruebas automatizadas correctas;
- validación manual aprobada;
- documentación específica del módulo.

## Módulo funcional de Gestión de Usuarios

El módulo Usuarios se encuentra implementado de extremo a extremo dentro del alcance aprobado.

### Domain

Se implementaron:

- actualización normalizada del nombre de Usuario;
- reemplazo controlado de Grupos directos;
- rechazo de colecciones vacías;
- rechazo de Grupos nulos;
- rechazo de Grupos duplicados;
- preservación del estado anterior ante una modificación inválida.

### Application

Se implementaron:

- `UsuarioFiltro`;
- `UsuarioListadoDto`;
- `UsuarioDetalleDto`;
- `PersonaSeleccionUsuarioDto`;
- `GrupoSeleccionUsuarioDto`;
- `RegistrarUsuarioCommand`;
- `ModificarUsuarioCommand`;
- `IUsuarioGestionRepository`;
- `UsuarioGestionService`.

Casos de uso disponibles:

- listar;
- obtener detalle;
- listar Personas disponibles;
- listar Grupos activos;
- registrar;
- modificar;
- activar;
- desactivar.

Las operaciones validan:

- sesión autenticada;
- Usuario activo;
- permiso `USUARIO_GESTIONAR`;
- Persona existente y activa;
- ausencia de otro Usuario para la Persona;
- unicidad del nombre de Usuario;
- contraseña inicial;
- al menos un Grupo activo;
- protección del Usuario autenticado;
- protección del último Administrador activo.

### Infrastructure

Se implementó:

`src/SIGEVIP.Infrastructure/Usuarios/UsuarioGestionRepository.cs`

Responsabilidades:

- listar con búsqueda y filtros;
- recuperar detalle;
- reconstruir Usuario y Grupos directos;
- consultar Personas disponibles;
- consultar Grupos activos;
- insertar Usuario y asignaciones dentro de una transacción;
- modificar nombre y reemplazar asignaciones dentro de una transacción;
- activar;
- desactivar;
- detectar otro Administrador activo;
- traducir duplicados y errores técnicos.

No fue necesaria una migración nueva porque el esquema de seguridad existente ya contenía:

- `dbo.Persona`;
- `dbo.Usuario`;
- `dbo.Grupo`;
- `dbo.UsuarioGrupo`;
- índices únicos;
- claves foráneas;
- clave primaria compuesta.

### WinForms

Se implementaron:

- `UsuariosForm`;
- `UsuarioEditForm`;
- navegación desde `MainForm`;
- coordinación desde `SigevipApplicationContext`;
- composición en `Program`.

La interfaz permite:

- listar;
- buscar;
- filtrar;
- registrar;
- modificar;
- activar;
- desactivar;
- seleccionar una Persona disponible;
- seleccionar varios Grupos;
- validar contraseña inicial;
- visualizar el Usuario autenticado;
- bloquear visualmente operaciones prohibidas.

### Validación técnica

- compilación con 0 advertencias y 0 errores;
- 479 pruebas automatizadas correctas;
- 0 pruebas fallidas;
- pruebas unitarias de Domain y Application;
- pruebas de integración real con SQL Server;
- validación manual completa;
- limpieza comprobada de los datos temporales.

### Commits del módulo

- `e13d291` — `Agrego casos de uso de usuarios`
- `ad7772c` — `Agrego persistencia de usuarios`
- `9460bf9` — `Completo interfaz de gestion de usuarios`

## Cambio de clave del Usuario autenticado

CUD11: Cambiar clave se encuentra implementado de extremo a extremo.

### Domain

Se incorporó:

`Usuario.ActualizarCredenciales`

La entidad valida y reemplaza:

- hash;
- salt;
- iteraciones.

Los arreglos se copian defensivamente y las credenciales anteriores se conservan cuando los datos nuevos son inválidos.

### Application

Se implementaron:

- `CambiarClaveCommand`;
- `IUsuarioClaveRepository`;
- `CambiarClaveService`.

El servicio valida:

- sesión autenticada;
- Usuario activo;
- campos obligatorios;
- contraseña nueva de al menos ocho caracteres;
- confirmación;
- contraseña actual;
- disponibilidad persistente del Usuario.

La operación no requiere un permiso administrativo porque cada Usuario modifica únicamente su propia contraseña.

### Infrastructure

Se implementó:

`src/SIGEVIP.Infrastructure/Security/UsuarioClaveRepository.cs`

La actualización ADO.NET modifica exclusivamente:

- `PasswordHash`;
- `PasswordSalt`;
- `IteracionesPassword`.

La operación exige que el Usuario continúe activo.

No fue necesaria una migración nueva.

### WinForms

Se implementó:

`src/SIGEVIP.WinForms/Forms/CambiarClaveForm.cs`

El menú principal permite abrir el formulario a todo Usuario autenticado.

Después de un cambio correcto:

1. se informa el resultado;
2. se cierra el formulario;
3. se cierra la sesión;
4. se vuelve al login.

### Validación

- 17 pruebas nuevas;
- 479 pruebas totales;
- 479 correctas;
- 0 fallidas;
- compilación con 0 advertencias y 0 errores;
- validación manual aprobada;
- ausencia de problemas visuales.

### Commits

- `1b920cc` — `Agrego cambio seguro de clave`
- `74dec4b` — `Agrego interfaz de cambio de clave`

CUD12: Recuperar clave permanece pendiente.

## Módulo funcional de Gestión de Grupos

El módulo Grupos se encuentra implementado de extremo a extremo dentro del alcance aprobado.

### Domain

Se implementaron:

- actualización validada de Nombre y Descripción;
- reemplazo controlado de Permisos directos;
- rechazo de colecciones vacías;
- rechazo de Permisos nulos;
- rechazo de Permisos duplicados;
- preservación de Grupos hijos;
- conservación del estado anterior ante modificaciones inválidas.

### Application

Se implementaron:

- `GrupoFiltro`;
- `GrupoListadoDto`;
- `GrupoDetalleDto`;
- `PermisoSeleccionGrupoDto`;
- `RegistrarGrupoCommand`;
- `ModificarGrupoCommand`;
- `IGrupoGestionRepository`;
- `GrupoGestionService`.

Casos de uso disponibles:

- listar;
- obtener detalle;
- listar Permisos activos;
- registrar;
- modificar;
- activar;
- desactivar.

Las operaciones validan:

- sesión autenticada;
- Usuario activo;
- permiso `GRUPO_GESTIONAR`;
- Código generado y único;
- Nombre obligatorio y único;
- Descripción obligatoria;
- al menos un Permiso activo;
- ausencia de Permisos duplicados;
- protección de `ADMINISTRADOR_GENERAL`;
- conservación de los Permisos administrativos mínimos.

### Infrastructure

Se implementó:

`src/SIGEVIP.Infrastructure/Grupos/GrupoGestionRepository.cs`

Responsabilidades:

- listar con búsqueda y filtro de estado;
- recuperar detalle;
- reconstruir Permisos directos;
- reconstruir Grupos hijos directos;
- consultar Permisos activos;
- consultar Permisos por identificadores;
- verificar Código;
- verificar Nombre;
- verificar actividad de Permisos;
- insertar Grupo y Permisos dentro de una transacción;
- actualizar Nombre y Descripción;
- reemplazar `GrupoPermiso` dentro de una transacción;
- preservar `GrupoGrupo`;
- activar;
- desactivar;
- traducir duplicados y errores técnicos.

No fue necesaria una migración nueva porque el esquema de seguridad existente ya contenía:

- `dbo.Grupo`;
- `dbo.Permiso`;
- `dbo.GrupoPermiso`;
- `dbo.GrupoGrupo`;
- `dbo.UsuarioGrupo`;
- claves primarias;
- claves foráneas;
- índice único de Código.

### WinForms

Se implementaron:

- `GruposForm`;
- `GrupoEditForm`;
- navegación desde `MainForm`;
- coordinación desde `SigevipApplicationContext`;
- composición en `Program`.

La interfaz permite:

- listar;
- buscar;
- filtrar;
- registrar;
- modificar;
- activar;
- desactivar;
- generar el Código;
- visualizar el Código inmutable;
- seleccionar varios Permisos;
- visualizar cantidades de Permisos y Usuarios;
- mostrar validaciones y errores controlados.

### Validación técnica

- compilación con 0 advertencias y 0 errores;
- 53 pruebas específicas nuevas;
- 532 pruebas automatizadas correctas;
- 0 pruebas fallidas;
- pruebas de Domain;
- pruebas de Application;
- pruebas de integración real con SQL Server;
- validación manual completa.

### Validación manual

Se verificó:

- listado;
- búsqueda;
- filtros;
- alta;
- generación del Código;
- modificación;
- reemplazo de Permisos;
- Código inmutable;
- desactivación lógica;
- conservación de asociaciones;
- reactivación;
- protección de `ADMINISTRADOR_GENERAL`;
- protección de los Permisos administrativos mínimos;
- validación de campos obligatorios.

### Commits del módulo

- `fcbc5e2` — `Agrego casos de uso de grupos`
- `5da1e81` — `Agrego persistencia de grupos`
- `90ec085` — `Completo interfaz de gestion de grupos`

### Alcance posterior

Después del cierre de este módulo se implementó la gestión funcional del catálogo de Permisos.

Permanece pendiente:

- gestión visual de relaciones `GrupoGrupo`;
- auditoría general consultable.

## Módulo funcional de Gestión de Permisos

El catálogo de Permisos se encuentra implementado de extremo a extremo.

La documentación académica no define un CUD numerado específico para este mantenimiento.

Por ese motivo el módulo se registra como una extensión funcional del subsistema de seguridad, sin inventar numeración documental.

### Domain

Se implementaron:

- actualización validada de Nombre y Descripción;
- Descripción opcional;
- Código normalizado;
- Código inmutable;
- preservación del identificador;
- preservación del estado durante la modificación;
- activación y desactivación lógica.

### Application

Se implementaron:

- `PermisoFiltro`;
- `PermisoListadoDto`;
- `PermisoDetalleDto`;
- `RegistrarPermisoCommand`;
- `ModificarPermisoCommand`;
- `IPermisoGestionRepository`;
- `PermisoGestionService`.

Casos de uso:

- listar;
- obtener detalle;
- registrar;
- modificar;
- activar;
- desactivar.

Las operaciones requieren:

`PERMISO_GESTIONAR`

Se protegen contra desactivación:

- `USUARIO_GESTIONAR`;
- `GRUPO_GESTIONAR`;
- `PERMISO_GESTIONAR`.

### Infrastructure

Se implementó:

`src/SIGEVIP.Infrastructure/Permisos/PermisoGestionRepository.cs`

El repositorio permite:

- listar con búsqueda;
- filtrar por estado;
- contar Grupos asociados;
- recuperar detalle;
- reconstruir Permisos activos e inactivos;
- verificar Código;
- insertar;
- actualizar Nombre y Descripción;
- activar;
- desactivar.

No fue necesaria una migración nueva.

La Descripción vacía se persiste como `NULL`.

El Código no se modifica durante la actualización.

La activación y desactivación conservan `GrupoPermiso`.

### WinForms

Se implementaron:

- `PermisosForm`;
- `PermisoEditForm`;
- navegación desde `MainForm`;
- coordinación desde `SigevipApplicationContext`;
- composición en `Program`.

La interfaz permite:

- listar;
- buscar;
- filtrar;
- registrar;
- modificar;
- activar;
- desactivar;
- visualizar cantidad de Grupos;
- mostrar el Código como inmutable.

### Corrección transversal de Grupos

La edición de Grupos fue ajustada para:

- incluir Permisos inactivos ya seleccionados;
- conservarlos al guardar;
- excluir Permisos inactivos no seleccionados;
- impedir nuevas asignaciones de Permisos inactivos.

### Validación

- 40 pruebas nuevas de Domain y Application;
- 19 pruebas nuevas de integración del repositorio;
- 3 pruebas nuevas de preservación en Grupos;
- 594 pruebas totales;
- 594 correctas;
- 0 fallidas;
- compilación con 0 advertencias;
- compilación con 0 errores;
- validación manual completa;
- datos temporales eliminados.

### Commits

- `3e8401c` — `Agrego casos de uso de permisos`
- `d61e304` — `Agrego persistencia de permisos`
- `2c16ef2` — `Completo interfaz de gestion de permisos`

Documento específico:

`docs/20-pruebas-gestion-permisos.md`
