# Trazabilidad inicial

| Elemento documental | Implementación o decisión | Proyecto relacionado | Pruebas | Estado |
|---|---|---|---|---|
| Aplicación de escritorio | Windows Forms | SIGEVIP.WinForms | Prueba técnica previa | Preparado |
| Lenguaje C# | .NET Framework 4.8 | Todos | Compilación completa | Implementado |
| Arquitectura en capas | Domain, Application, Infrastructure y WinForms | Todos | ArchitectureTests | Implementado |
| Persistencia local | SQL Server y ADO.NET | SIGEVIP.Infrastructure | Comprobación técnica previa | Preparado |
| RF01: iniciar sesión | `AutenticacionService`, `IPasswordHasher`, resultado genérico | Application / Infrastructure | Autenticación válida e inválida | Parcialmente implementado |
| RF02: gestionar usuarios | Entidad Usuario, activación y grupos | Domain | Usuario y grupos | Parcialmente implementado |
| RF03: modificar usuarios | Usuario encapsulado; persistencia y caso de uso pendientes | Domain / Application | Validaciones de Usuario | Parcialmente implementado |
| RF04: eliminar usuario | `Usuario.Desactivar()` como baja lógica | Domain | Desactivación y reactivación | Implementado en dominio |
| RF05: registrar clientes | Entidad Cliente | Domain | Creación y validaciones | Parcialmente implementado |
| RF06: modificar clientes | Propiedades encapsuladas; caso de uso pendiente | Domain / Application | Pendientes de Application | Parcialmente implementado |
| RF07: activar o desactivar clientes | `Cliente.Activar()` y `Cliente.Desactivar()` | Domain | Activación y desactivación | Implementado en dominio |
| RF08: listado filtrable de clientes | Requiere repositorio, servicio e interfaz | Application / Infrastructure / WinForms | Pendientes | Pendiente |
| RF09: historial del cliente | Relaciones históricas preservadas | Domain / Application / Infrastructure | Pendientes de consulta | Parcialmente implementado |
| RF11: monto anticipado | `Viaje.MontoAnticipado` decimal | Domain | Validaciones y cálculos | Implementado |
| RF12: modificar solo en Abierto | Patrón State | Domain | Bloqueos por estado | Implementado |
| RF13: cancelar viaje | Cancelación condicionada por visitas | Domain | Cancelación y rechazo | Implementado |
| RF14: total y saldo | `TotalGastado` y `SaldoPendiente` | Domain | Saldo positivo, negativo y cero | Implementado |
| RF15: registrar visitas | `Viaje.AgregarVisita()` | Domain | Alta y bloqueo por estado | Implementado en dominio |
| RF16: datos de visita | Fecha, observación y localidad | Domain | Validaciones obligatorias | Implementado |
| RF17: uno o más clientes | Colección controlada de Cliente | Domain | Uno, varios y duplicados | Implementado |
| RF18: historial de visitas | Asociaciones preservadas; consulta pendiente | Domain / Application / Infrastructure | Pendientes | Parcialmente implementado |
| RF19: impedir visitas sin viaje | Asociación mediante Viaje | Domain | Asociación y reasignación | Implementado |
| RF20: viático asociado | `Viaje.AgregarViatico()` | Domain | Asociación y fechas | Implementado |
| RF25: bloquear modificaciones | Estado del Viaje | Domain | Bloqueos verificados | Implementado |
| RF26: enviar a rendición | `Viaje.EnviarARendicion()` | Domain | Transición válida | Implementado |
| RF27: impedir modificaciones | Estado EnRendicion | Domain | Bloqueos | Implementado |
| RF29: excluir viáticos | Baja lógica | Domain | Exclusión y reactivación | Implementado |
| RF30: aprobar viaje | `Viaje.Aprobar()` | Domain | EnRendicion a Aprobado | Implementado |
| RF32: saldo final | Total vigente menos anticipo | Domain | Cálculos económicos | Implementado |
| RF37: validar permisos | `AutorizacionService.TienePermiso()` | Application | Permisos directos y anidados | Implementado en Application |
| RF38: adaptar interfaz a permisos | Ocultar o deshabilitar controles | WinForms | Pendientes | Pendiente |
| RF39: restringir acceso | Usuario activo y permiso efectivo | Domain / Application | Usuario nulo, inactivo y sin permiso | Implementado en Application |
| Persona 0..1 Usuario | Entidades separadas | Domain | Construcción y asociación | Implementado en dominio |
| Usuario N a N Grupo | Colección de grupos de Usuario | Domain | Uno, varios y duplicados | Implementado en dominio |
| Grupo Composite | Grupo contiene Permiso o Grupo | Domain | Ciclos, duplicados y anidamiento | Implementado |
| Permiso hoja | `Permiso : IPermisoComponente` | Domain | Activo e inactivo | Implementado |
| Autenticación segura | Mensaje público genérico | Application | Casos exitosos y fallidos | Implementado en Application |
| Sesión actual | `ISesionActual` y `SesionActual` | Application | Inicio y cierre | Implementado |
| Hash de contraseña | PBKDF2-HMAC-SHA256 | Infrastructure | Hash, salt y verificación | Implementado |
| Comparación segura | Comparación en tiempo constante | Infrastructure | Hash correcto y modificado | Implementado |
| Repositorio de autenticación | `IUsuarioAutenticacionRepository` | Application | Repositorio falso | Contrato implementado |
| Repositorio SQL de usuario | Implementación ADO.NET | Infrastructure | Pendientes | Pendiente |
| Tablas de seguridad | Persona, Usuario, Grupo, Permiso y relaciones | database | Pendientes | Pendiente |
| Viaje 1 a 0..N Visitas | Colección privada `_visitas` | Domain | Alta y colección protegida | Implementado |
| Visita N a N Cliente | Colección privada `_clientes` | Domain | Uno, varios y duplicados | Implementado |
| State en Viaje | `IEstadoViaje` y estados concretos | Domain | Transiciones | Implementado |
| Composite en seguridad | `IPermisoComponente`, Grupo y Permiso | Domain | 21 pruebas iniciales del patrón | Implementado |
| Auditoría básica | Persistencia y servicios | Infrastructure / Application | Pendientes | Pendiente |
| SQL reproducible | Migraciones y datos iniciales | database | Pendientes | Preparado |
