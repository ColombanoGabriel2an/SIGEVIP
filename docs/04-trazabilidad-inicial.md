# Trazabilidad inicial

| Elemento documental | Implementación o decisión | Proyecto relacionado | Pruebas | Estado |
|---|---|---|---|---|
| Aplicación de escritorio | Windows Forms | SIGEVIP.WinForms | Prueba técnica previa | Preparado |
| Lenguaje C# | .NET Framework 4.8 | Todos | Compilación completa | Implementado |
| Arquitectura MVC | Capas Domain, Application, Infrastructure y WinForms | Todos | ArchitectureTests | Implementado |
| Persistencia local | SQL Server y ADO.NET | SIGEVIP.Infrastructure | Comprobación técnica previa | Preparado |
| RF05: registrar clientes | Entidad `Cliente` con datos principales y validaciones mínimas | SIGEVIP.Domain | Creación válida, razón social y CUIT obligatorios | Parcialmente implementado |
| RF06: modificar clientes | Propiedades encapsuladas; casos de uso de modificación todavía pendientes | SIGEVIP.Domain / Application | Pendientes de Application | Parcialmente implementado |
| RF07: activar o desactivar clientes | `Cliente.Activar()` y `Cliente.Desactivar()` | SIGEVIP.Domain | Activación y desactivación | Implementado en dominio |
| RF08: listado filtrable de clientes | Requiere repositorio, servicio e interfaz | Application / Infrastructure / WinForms | Pendientes | Pendiente |
| RF09: historial del cliente | Asociaciones históricas preservadas en Visita; consulta todavía pendiente | Domain / Application / Infrastructure | Pendientes de consulta | Parcialmente implementado |
| RF11: monto anticipado | `Viaje.MontoAnticipado` con tipo decimal | SIGEVIP.Domain | Validación de monto negativo y cálculos | Implementado |
| RF12: modificar solo en Abierto | `IEstadoViaje.ValidarModificacion` | SIGEVIP.Domain | Bloqueo en EnRendicion, Aprobado y Cancelado | Implementado en dominio |
| RF13: cancelar viaje | Transiciones desde Abierto y EnRendicion, con bloqueo si existen visitas | SIGEVIP.Domain | Cancelación sin visitas y rechazo con visitas | Implementado en dominio |
| RF14: calcular total y saldo | `TotalGastado`, `SaldoPendiente` y `CalcularSaldo()` | SIGEVIP.Domain | Saldo positivo, negativo y cero | Implementado |
| RF15: registrar visitas en viaje Abierto | `Viaje.AgregarVisita()` | SIGEVIP.Domain | Alta válida y bloqueo por estados | Implementado en dominio |
| RF16: fecha, observación y localidad | `Visita.Fecha`, `Observacion` y `LocalidadEncuentro` | SIGEVIP.Domain | Campos obligatorios y fechas límite | Implementado en dominio |
| RF17: uno o más clientes por visita | `Visita.AgregarCliente()` y colección controlada | SIGEVIP.Domain | Uno, varios, nulo y duplicados | Implementado en dominio |
| RF18: consultas históricas | Las asociaciones no se eliminan físicamente; consulta aún no implementada | Domain / Application / Infrastructure | Pendientes | Parcialmente implementado |
| RF19: impedir visitas sin viaje | La visita solo se incorpora mediante `Viaje.AgregarVisita()` | SIGEVIP.Domain | Asociación, duplicados y reasignación | Implementado en dominio |
| RF20: viático asociado al viaje | `Viaje.AgregarViatico()` y `Viatico.IdViaje` | SIGEVIP.Domain | Asociación y validación de fechas | Implementado |
| RF25: bloqueo de viáticos y visitas | Estado del viaje controla modificaciones | SIGEVIP.Domain | Pruebas en estados bloqueados | Implementado en dominio |
| RF26: enviar a rendición | `Viaje.EnviarARendicion()` | SIGEVIP.Domain | Abierto a EnRendicion | Implementado |
| RF27: impedir modificaciones | Estado EnRendicion rechaza nuevos viáticos y nuevas visitas | SIGEVIP.Domain | Bloqueos verificados | Implementado en dominio |
| RF29: excluir viáticos | Baja lógica mediante `EstadoViatico.Excluido` | SIGEVIP.Domain | Exclusión y reactivación | Implementado en dominio |
| RF30: aprobar viaje completo | `Viaje.Aprobar()` | SIGEVIP.Domain | EnRendicion a Aprobado | Implementado en dominio |
| RF32: saldo final | Suma de vigentes menos anticipo | SIGEVIP.Domain | Cálculos económicos | Implementado |
| Viaje 1 a 0..N Visitas | Colección privada `_visitas` | SIGEVIP.Domain | Alta y colección de solo lectura | Implementado |
| Visita N a N Cliente | Colección privada `_clientes` en Visita | SIGEVIP.Domain | Uno, varios y duplicados | Implementado en dominio |
| Tabla futura VisitaCliente | Decisión de persistencia para la relación muchos a muchos | SIGEVIP.Infrastructure | Pendientes | Diseñado |
| State en Viaje | `IEstadoViaje` y cuatro estados concretos | SIGEVIP.Domain | Transiciones válidas e inválidas | Implementado |
| Estado persistible | Enum y `EstadoViajeFactory` | SIGEVIP.Domain | Reconstrucción de estado | Implementado |
| Composite en seguridad | Patrón estructural | SIGEVIP.Domain | Pendientes | Pendiente |
| Auditoría básica | Persistencia y servicios | Infrastructure / Application | Pendientes | Pendiente |
| SQL reproducible | Migraciones y datos de prueba | database | Pendientes | Preparado |
