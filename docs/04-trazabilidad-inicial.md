# Trazabilidad inicial

| Elemento documental | Implementación o decisión | Proyecto relacionado | Validación | Estado |
|---|---|---|---|---|
| Aplicación de escritorio | Windows Forms | SIGEVIP.WinForms | Login y menú ejecutados manualmente | Implementado |
| Lenguaje C# | .NET Framework 4.8 | Todos | 0 advertencias y 0 errores | Implementado |
| Arquitectura en capas | Domain, Application, Infrastructure y WinForms | Todos | ArchitectureTests | Implementado |
| Persistencia local | SQL Server y ADO.NET | Infrastructure / database | Base SIGEVIP operativa | Implementado para seguridad |
| RF01: iniciar sesión | `LoginForm`, `AutenticacionService`, PBKDF2 y `UsuarioAutenticacionRepository` | WinForms / Application / Infrastructure / database | Autenticación manual y pruebas automatizadas | Implementado |
| RF02: validar credenciales y habilitar funciones | `SesionActual`, `SigevipApplicationContext`, `AutorizacionService` y `MainForm` | Application / Infrastructure / WinForms | Inicio de sesión, menú y permisos visuales | Implementado para menú principal |
| RF03: gestionar usuarios | Entidad Usuario, tablas Persona-Usuario y acceso visual Usuarios | Domain / database / WinForms | Dominio y persistencia base | Parcialmente implementado |
| RF04: asignar uno o más grupos | Colección de grupos, `UsuarioGrupo` y alta inicial | Domain / Application / Infrastructure / database | Duplicados impedidos y administrador inicial | Parcialmente implementado |
| RF05: registrar clientes | `Cliente`, `ClienteService.Registrar`, `ClienteRepository.Insertar` y `ClienteEditForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF06: modificar clientes | `Cliente.ActualizarDatos`, `ClienteService.Modificar`, `ClienteRepository.Actualizar` y `ClienteEditForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF07: activar o desactivar clientes | Entidad, servicio, repositorio, campo `Activo` y acciones de `ClientesForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF08: listado filtrable de clientes | `ClienteFiltro`, `ClienteListadoDto`, `ClienteService.Listar`, consulta ADO.NET y `ClientesForm` | Application / Infrastructure / database / WinForms | Filtros por texto, CUIT, localidad, provincia y estado | Implementado |
| RF09: historial del cliente | Relaciones históricas preservadas | Domain / Application / Infrastructure | Pendiente | Parcialmente implementado |
| RF11: monto anticipado | `Viaje.MontoAnticipado` | Domain | Pruebas de cálculo | Implementado |
| RF12: modificar solo en Abierto | Patrón State | Domain | Bloqueos por estado | Implementado |
| RF13: cancelar viaje | Cancelación condicionada | Domain | Pruebas de transición | Implementado |
| RF14: total y saldo | `TotalGastado` y `SaldoPendiente` | Domain | Saldos positivo, negativo y cero | Implementado |
| RF15: registrar visitas | `Viaje.AgregarVisita()` | Domain | Alta y bloqueos | Implementado en dominio |
| RF16: datos de visita | Fecha, observación y localidad | Domain | Validaciones | Implementado |
| RF17: uno o más clientes | Colección de Cliente | Domain | Uno, varios y duplicados | Implementado |
| RF18: historial de visitas | Relaciones preservadas | Domain / Application / Infrastructure | Pendiente | Parcialmente implementado |
| RF19: impedir visitas sin viaje | Asociación mediante Viaje | Domain | Asociación y reasignación | Implementado |
| RF20: viático asociado | `Viaje.AgregarViatico()` | Domain | Asociación y fechas | Implementado |
| RF25: bloquear modificaciones | Estado del Viaje | Domain | Bloqueos verificados | Implementado |
| RF26: enviar a rendición | `Viaje.EnviarARendicion()` | Domain | Transición válida | Implementado |
| RF27: impedir modificaciones | Estado EnRendicion | Domain | Bloqueos | Implementado |
| RF29: excluir viáticos | Baja lógica | Domain | Exclusión y reactivación | Implementado |
| RF30: aprobar viaje | `Viaje.Aprobar()` | Domain | Transición a Aprobado | Implementado |
| RF32: saldo final | Total vigente menos anticipo | Domain | Pruebas económicas | Implementado |
| RF37: impedir accesos no autorizados | `AutorizacionService`, `ClienteService.ExigirPermiso` y controles visuales | Application / Infrastructure / WinForms | Pruebas de Application y validación manual | Implementado para seguridad y Clientes |
| RF38: ocultar opciones no habilitadas | `MainForm` configura `Visible` según permiso | WinForms | Visitas y Viáticos ocultos para administrador actual | Implementado |
| RF39: acceso mediante grupos y permisos | Usuario, Grupo, Permiso y Composite persistido | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y menú | Implementado |
| Perfil del usuario autenticado | `PerfilSesion`, servicio y repositorio ADO.NET | Application / Infrastructure / WinForms | Nombre completo y usuario visibles | Implementado |
| Cierre de sesión | Evento `CerrarSesionSolicitada` y `SesionActual.Cerrar()` | WinForms / Application | Retorno al login | Implementado |
| Salida controlada | `SalirSolicitado` y `ApplicationContext.ExitThread()` | WinForms | Botón Salir y cruz de ventana | Implementado |
| Persona 1 a 0..1 Usuario | Entidades separadas e índice único | Domain / database | Restricción SQL | Implementado |
| Usuario N a N Grupo | Colección y tabla `UsuarioGrupo` | Domain / database | PK compuesta | Implementado |
| Grupo N a N Permiso | Composite y `GrupoPermiso` | Domain / database | 22 asociaciones iniciales | Implementado |
| Grupo N a N Grupo | Composite y `GrupoGrupo` | Domain / Infrastructure / database | Herencia y ciclos | Implementado |
| Grupo Composite | Grupo contiene Permiso o Grupo | Domain | Ciclos y duplicados | Implementado |
| Permiso hoja | `Permiso : IPermisoComponente` | Domain | Activo e inactivo | Implementado |
| Autenticación segura | Mensaje genérico y PBKDF2 | Application / Infrastructure | Casos exitosos y fallidos | Implementado |
| Sesión actual | `ISesionActual` y `SesionActual` | Application | Inicio y cierre | Implementado |
| Hash de contraseña | PBKDF2-HMAC-SHA256 | Infrastructure | Hash, salt y verificación | Implementado |
| Comparación segura | Comparación en tiempo constante | Infrastructure | Pruebas criptográficas | Implementado |
| Repositorio de autenticación | `UsuarioAutenticacionRepository` | Infrastructure | Integración SQL | Implementado |
| Repositorio de perfil | `PerfilSesionRepository` | Infrastructure | Consulta de Persona | Implementado |
| Tablas de seguridad | Siete tablas | database | Validación SQL | Implementado |
| Bajas lógicas | Campo `Activo` | Domain / database | Defaults y reglas | Implementado |
| Integridad referencial | Siete claves foráneas con `NO_ACTION` | database | Metadatos SQL | Implementado |
| Catálogos de seguridad | 4 grupos y 17 permisos | database/seed | Seed reejecutable | Implementado |
| Usuario administrador inicial | Servicio, repositorio y `SIGEVIP.Setup` | Application / Infrastructure / tools | Creación e idempotencia | Implementado |
| State en Viaje | `IEstadoViaje` y estados concretos | Domain | Transiciones | Implementado |
| Composite en seguridad | `IPermisoComponente`, Grupo y Permiso | Domain | Anidamiento y ciclos | Implementado |
| Auditoría básica | Persistencia y servicios | Infrastructure / Application | Pendiente | Pendiente |
| SQL reproducible | Migraciones, seeds y validaciones de seguridad y Clientes | database | Migraciones `001`, `002`, `003`, seeds reejecutables y validaciones correctas | Implementado para seguridad y Clientes |
