# Trazabilidad inicial

| Elemento documental | Implementación o decisión | Proyecto relacionado | Validación | Estado |
|---|---|---|---|---|
| Aplicación de escritorio | Windows Forms | SIGEVIP.WinForms | Compilación completa | Preparado |
| Lenguaje C# | .NET Framework 4.8 | Todos | 0 advertencias y 0 errores | Implementado |
| Arquitectura en capas | Domain, Application, Infrastructure y WinForms | Todos | ArchitectureTests | Implementado |
| Persistencia local | SQL Server y ADO.NET | Infrastructure / database | Base SIGEVIP operativa y repositorios de seguridad | Implementado para seguridad |
| RF01: iniciar sesión | `AutenticacionService`, PBKDF2, `UsuarioAutenticacionRepository` y tabla Usuario | Application / Infrastructure / database | Pruebas unitarias e integración SQL | Implementado sin interfaz gráfica |
| RF02: validar credenciales y habilitar funciones | Autenticación, autorización, sesión y persistencia concreta | Application / Infrastructure / database / WinForms | Permisos directos, heredados y ciclos persistidos | Parcial: falta integración visual |
| RF03: gestionar usuarios | Entidad Usuario y esquema Persona-Usuario-UsuarioGrupo | Domain / Application / database | Pruebas de Usuario y validación SQL | Parcialmente implementado |
| RF04: asignar uno o más grupos | Colección de grupos y tabla `UsuarioGrupo` | Domain / database | Duplicados impedidos por dominio y PK compuesta | Parcialmente implementado |
| RF05: registrar clientes | Entidad Cliente | Domain | Creación y validaciones | Parcialmente implementado |
| RF06: modificar clientes | Propiedades encapsuladas; caso de uso pendiente | Domain / Application | Pendiente | Parcialmente implementado |
| RF07: activar o desactivar clientes | `Cliente.Activar()` y `Cliente.Desactivar()` | Domain | Activación y desactivación | Implementado en dominio |
| RF08: listado filtrable de clientes | Requiere repositorio, servicio e interfaz | Application / Infrastructure / WinForms | Pendiente | Pendiente |
| RF09: historial del cliente | Relaciones históricas preservadas | Domain / Application / Infrastructure | Pendiente | Parcialmente implementado |
| RF11: monto anticipado | `Viaje.MontoAnticipado` decimal | Domain | Validaciones y cálculos | Implementado |
| RF12: modificar solo en Abierto | Patrón State | Domain | Bloqueos por estado | Implementado |
| RF13: cancelar viaje | Cancelación condicionada por visitas | Domain | Cancelación y rechazo | Implementado |
| RF14: total y saldo | `TotalGastado` y `SaldoPendiente` | Domain | Saldo positivo, negativo y cero | Implementado |
| RF15: registrar visitas | `Viaje.AgregarVisita()` | Domain | Alta y bloqueo por estado | Implementado en dominio |
| RF16: datos de visita | Fecha, observación y localidad | Domain | Validaciones obligatorias | Implementado |
| RF17: uno o más clientes | Colección controlada de Cliente | Domain | Uno, varios y duplicados | Implementado |
| RF18: historial de visitas | Asociaciones preservadas; consulta pendiente | Domain / Application / Infrastructure | Pendiente | Parcialmente implementado |
| RF19: impedir visitas sin viaje | Asociación mediante Viaje | Domain | Asociación y reasignación | Implementado |
| RF20: viático asociado | `Viaje.AgregarViatico()` | Domain | Asociación y fechas | Implementado |
| RF25: bloquear modificaciones | Estado del Viaje | Domain | Bloqueos verificados | Implementado |
| RF26: enviar a rendición | `Viaje.EnviarARendicion()` | Domain | Transición válida | Implementado |
| RF27: impedir modificaciones | Estado EnRendicion | Domain | Bloqueos | Implementado |
| RF29: excluir viáticos | Baja lógica | Domain | Exclusión y reactivación | Implementado |
| RF30: aprobar viaje | `Viaje.Aprobar()` | Domain | EnRendicion a Aprobado | Implementado |
| RF32: saldo final | Total vigente menos anticipo | Domain | Cálculos económicos | Implementado |
| RF37: impedir accesos no autorizados | `AutorizacionService.TienePermiso()` y persistencia de permisos | Application / Infrastructure / database | Permisos directos, anidados y reconstrucción SQL | Implementado en backend |
| RF38: ocultar opciones no habilitadas | Ocultar o deshabilitar controles | WinForms | Pendiente | Pendiente |
| RF39: acceso mediante grupos y permisos | Usuario activo, grupos, permisos y Composite persistido | Domain / Application / Infrastructure / database | Pruebas unitarias e integración SQL | Implementado en backend |
| Persona 1 a 0..1 Usuario | Entidades separadas e índice único `UX_Usuario_IdPersona` | Domain / database | Restricción SQL verificada | Implementado |
| Usuario N a N Grupo | Colección de Usuario y tabla `UsuarioGrupo` | Domain / database | PK compuesta y claves foráneas | Implementado en dominio y esquema |
| Grupo N a N Permiso | Composite y tabla `GrupoPermiso` | Domain / database | 22 asociaciones iniciales | Implementado en dominio y esquema |
| Grupo N a N Grupo | Composite y tabla `GrupoGrupo` | Domain / Infrastructure / database | Herencia real y ciclos persistidos | Implementado y probado |
| Grupo Composite | Grupo contiene Permiso o Grupo | Domain | Ciclos, duplicados y anidamiento | Implementado |
| Permiso hoja | `Permiso : IPermisoComponente` | Domain | Activo e inactivo | Implementado |
| Autenticación segura | Mensaje público genérico | Application | Casos exitosos y fallidos | Implementado en Application |
| Sesión actual | `ISesionActual` y `SesionActual` | Application | Inicio y cierre | Implementado |
| Hash de contraseña | PBKDF2-HMAC-SHA256 | Infrastructure | Hash, salt y verificación | Implementado |
| Hash persistente | `VARBINARY(32)` para hash y salt | database | CHECK de longitud | Implementado en esquema |
| Iteraciones persistentes | `IteracionesPassword` mayor que cero | Domain / database | Validación de dominio y CHECK SQL | Implementado |
| Comparación segura | Comparación en tiempo constante | Infrastructure | Hash correcto y modificado | Implementado |
| Repositorio de autenticación | `IUsuarioAutenticacionRepository` | Application / Infrastructure | Repositorio falso y repositorio SQL | Contrato e implementación probados |
| Repositorio SQL de usuario | `UsuarioAutenticacionRepository` con ADO.NET | Infrastructure | Integración real con SQL Server | Implementado |
| Tablas de seguridad | Persona, Usuario, Grupo, Permiso y relaciones | database | Siete tablas verificadas | Implementado |
| Bajas lógicas | Columnas `Activo` con valor inicial 1 | Domain / database | Defaults y reglas de dominio | Implementado |
| Unicidad de usuario | `UX_Usuario_NombreUsuario` | database | Índice único verificado | Implementado en esquema |
| Unicidad Persona-Usuario | `UX_Usuario_IdPersona` | database | Índice único verificado | Implementado |
| Unicidad de códigos | `UX_Grupo_Codigo` y `UX_Permiso_Codigo` | database | Índices únicos verificados | Implementado |
| Integridad referencial | Siete claves foráneas con `NO_ACTION` | database | Consulta de metadatos SQL | Implementado |
| Catálogos de seguridad | 4 grupos y 17 permisos | database/seed | Seed inicial y reejecución | Implementado |
| Asignaciones iniciales | 22 asociaciones Grupo-Permiso | database/seed | Conteo y detalle por grupo | Implementado |
| Seed reejecutable | Inserciones condicionadas y actualización controlada | database/seed | Segunda ejecución sin duplicados | Implementado |
| Validación de esquema | `002_validar_seguridad.sql` | database/migrations | Resultado `VALIDACIÓN CORRECTA` | Implementado |
| Usuario administrador inicial | Servicio, repositorio transaccional y `SIGEVIP.Setup` | Application / Infrastructure / tools | Creación, idempotencia, rollback y validación manual | Implementado |
| Viaje 1 a 0..N Visitas | Colección privada `_visitas` | Domain | Alta y colección protegida | Implementado |
| Visita N a N Cliente | Colección privada `_clientes` | Domain | Uno, varios y duplicados | Implementado |
| State en Viaje | `IEstadoViaje` y estados concretos | Domain | Transiciones | Implementado |
| Composite en seguridad | `IPermisoComponente`, Grupo y Permiso | Domain | Pruebas de ciclos, anidamiento y duplicados | Implementado |
| Reconstrucción de seguridad | Usuario, grupos, permisos y grupos hijos desde SQL Server | Infrastructure | Grupos directos, permisos heredados y ciclos | Implementado |
| Configuración inicial | `tools/SIGEVIP.Setup` | tools / Application / Infrastructure | Códigos 0 y 3, PBKDF2 y grupo verificados | Implementado |
| Auditoría básica | Persistencia y servicios | Infrastructure / Application | Pendiente | Pendiente |
| SQL reproducible | Migraciones, seed y validación | database | Ejecución reproducible en SSMS | Implementado para seguridad |
