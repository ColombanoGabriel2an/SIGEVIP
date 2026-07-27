# Trazabilidad inicial

| Elemento documental | Implementación o decisión | Proyecto relacionado | Validación | Estado |
|---|---|---|---|---|
| Aplicación de escritorio | Windows Forms | SIGEVIP.WinForms | Login y menú ejecutados manualmente | Implementado |
| Lenguaje C# | .NET Framework 4.8 | Todos | 0 advertencias y 0 errores | Implementado |
| Arquitectura en capas | Domain, Application, Infrastructure y WinForms | Todos | ArchitectureTests | Implementado |
| Persistencia local | SQL Server y ADO.NET | Infrastructure / database | Base SIGEVIP operativa | Implementado para seguridad, Clientes, Viajes y Visitas |
| RF01: iniciar sesión | `LoginForm`, `AutenticacionService`, PBKDF2 y `UsuarioAutenticacionRepository` | WinForms / Application / Infrastructure / database | Autenticación manual y pruebas automatizadas | Implementado |
| RF02: validar credenciales y habilitar funciones | `SesionActual`, `SigevipApplicationContext`, `AutorizacionService` y `MainForm` | Application / Infrastructure / WinForms | Inicio de sesión, menú y permisos visuales | Implementado para menú principal |
| RF03: gestionar usuarios | Entidad Usuario, tablas Persona-Usuario y acceso visual Usuarios | Domain / database / WinForms | Dominio y persistencia base | Parcialmente implementado |
| RF04: asignar uno o más grupos | Colección de grupos, `UsuarioGrupo` y alta inicial | Domain / Application / Infrastructure / database | Duplicados impedidos y administrador inicial | Parcialmente implementado |
| RF05: registrar clientes | `Cliente`, `ClienteService.Registrar`, `ClienteRepository.Insertar` y `ClienteEditForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF06: modificar clientes | `Cliente.ActualizarDatos`, `ClienteService.Modificar`, `ClienteRepository.Actualizar` y `ClienteEditForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF07: activar o desactivar clientes | Entidad, servicio, repositorio, campo `Activo` y acciones de `ClientesForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF08: listado filtrable de clientes | `ClienteFiltro`, `ClienteListadoDto`, `ClienteService.Listar`, consulta ADO.NET y `ClientesForm` | Application / Infrastructure / database / WinForms | Filtros por texto, CUIT, localidad, provincia y estado | Implementado |
| RF09: historial del cliente | Relaciones históricas preservadas | Domain / Application / Infrastructure | Pendiente | Parcialmente implementado |
| RF10: registrar viaje | `Viaje`, participantes, `ViajeService.Registrar`, `ViajeRepository.Insertar` y `ViajeEditForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF11: modificar viaje | `Viaje.ActualizarDatos`, `ViajeService.Modificar`, `ViajeRepository.Actualizar` y `ViajeEditForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF12: consultar viajes | `ViajeFiltro`, `ViajeListadoDto`, `ViajeService.Listar`, consulta ADO.NET y `ViajesForm` | Application / Infrastructure / database / WinForms | Filtros por fechas, estado y participante | Implementado |
| RF13: cancelar viaje | `Viaje.Cancelar`, `ViajeService.Cancelar`, control SQL en `ViajeRepository.Cancelar` y `ViajesForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, agregado desactualizado, integración SQL y validación manual | Implementado; se rechaza cuando existen Visitas |
| RF14: consultar detalle del viaje | `ViajeService.Obtener`, reconstrucción completa del agregado y `ViajeDetalleForm` | Domain / Application / Infrastructure / database / WinForms | Participantes, Visitas, Clientes, Viáticos, Comprobantes, total y saldo | Implementado |
| RF15: filtrar viajes | `ViajeFiltro`, `ViajeRepository.Listar` y controles de `ViajesForm` | Application / Infrastructure / WinForms | Filtros por fecha desde, fecha hasta, estado y participante | Parcial según alcance documentado |
| RF16: datos de visita | `Visita`, `VisitaService.Registrar`, `VisitaRepository.Insertar` y `VisitaEditForm` | Domain / Application / Infrastructure / database / WinForms | Pruebas unitarias, integración SQL y validación manual | Implementado |
| RF17: uno o más clientes | Colección de Cliente, `VisitaCliente`, selección múltiple y consulta de Clientes activos | Domain / Application / Infrastructure / database / WinForms | Uno, varios, duplicados, inactivos e integración SQL | Implementado |
| RF18: historial de visitas | `VisitaRepository.ListarPorViaje`, `ListarPorCliente`, reconstrucción histórica y `VisitasForm` | Domain / Application / Infrastructure / database / WinForms | Clientes inactivos preservados y pruebas de integración | Implementado dentro del alcance actual |
| RF19: impedir visitas sin viaje | `Viaje.AgregarVisita`, FK `Visita.IdViaje` y validación de `VisitaService` | Domain / Application / Infrastructure / database | Asociación, Viaje inexistente, FK y reconstrucción | Implementado |
| RF20: viático asociado | `Viaje.AgregarViatico`, `ViaticoService` y `ViaticoRepository` | Domain / Application / Infrastructure / database / WinForms | Unitarias, integración SQL y validación manual | Implementado |
| RF21: registrar datos del viático | `Viatico`, commands, `ViaticoService` y `ViaticoEditForm` | Domain / Application / Infrastructure / database / WinForms | Validaciones funcionales, persistencia y manual | Implementado |
| RF22: registrar comprobante | `Comprobante`, `ComprobanteInput`, persistencia 0..1 y formulario | Domain / Application / Infrastructure / database / WinForms | Unitarias, integración SQL y manual | Implementado |
| RF23: identificar persona pagadora | Reglas de método de pago, consulta de Personas y selector visual | Domain / Application / Infrastructure / WinForms | Unitarias, integración SQL y manual | Implementado |
| RF24: consultar viáticos | `ViaticoService.ListarPorViaje`, filtros ADO.NET y `ViaticosForm` | Application / Infrastructure / database / WinForms | Filtros, integración SQL y manual | Implementado |
| RF25: bloquear modificaciones | Estados de Viaje y Viático, validaciones de servicios y controles visuales | Domain / Application / Infrastructure / WinForms | Unitarias, integración y manual | Implementado |
| RF26: enviar a rendición | `Viaje.EnviarARendicion`, `RendicionService.Enviar` y persistencia de auditoría | Domain / Application / Infrastructure / database / WinForms | Unitarias, integración SQL y manual | Implementado |
| RF27: impedir modificaciones | Estado EnRendicion, validación de Application y control de concurrencia | Domain / Application / Infrastructure | Unitarias e integración SQL | Implementado |
| RF28: revisar rendición | `RendicionService.ObtenerDetalle`, reconstrucción completa y `RendicionesForm` | Application / Infrastructure / database / WinForms | Integración SQL y validación manual | Implementado |
| RF29: excluir viáticos | Exclusión y reactivación lógica con motivo y auditoría | Domain / Application / Infrastructure / database / WinForms | Unitarias, integración SQL y manual | Implementado |
| RF30: aprobar viaje | `Viaje.Aprobar`, `RendicionService.Aprobar` y persistencia | Domain / Application / Infrastructure / database / WinForms | Unitarias, integración SQL y manual | Implementado |
| RF31: cancelar rendición | `Viaje.Cancelar`, `RendicionService.Cancelar` y motivo obligatorio | Domain / Application / Infrastructure / database / WinForms | Unitarias, integración SQL y manual | Implementado |
| RF32: saldo final | Total de Viáticos vigentes menos anticipo | Domain / Application / Infrastructure / WinForms | Pruebas económicas, integración y manual | Implementado |
| RF37: impedir accesos no autorizados | `AutorizacionService`, validaciones de Cliente, Viaje, Visita, Viático y Rendición | Application / Infrastructure / WinForms | Pruebas de Application y validación manual | Implementado para seguridad y módulos funcionales actuales |
| RF38: ocultar opciones no habilitadas | `MainForm` y formularios configuran visibilidad y habilitación según permisos efectivos | WinForms | Clientes, Viajes, Visitas, Viáticos y Rendiciones | Implementado |
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
| SQL reproducible | Migraciones, seeds y validaciones de seguridad, Clientes, Viajes, Visitas, Viáticos y Rendiciones | database | Migraciones `001` a `006`, seeds reejecutables y validaciones correctas | Implementado para los módulos funcionales actuales |
