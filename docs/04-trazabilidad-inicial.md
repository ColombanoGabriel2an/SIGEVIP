# Trazabilidad inicial

| Elemento documental | Implementación o decisión | Proyecto relacionado | Pruebas | Estado |
|---|---|---|---|---|
| Aplicación de escritorio | Windows Forms | SIGEVIP.WinForms | Prueba técnica previa | Preparado |
| Lenguaje C# | .NET Framework 4.8 | Todos | Compilación completa | Implementado |
| Arquitectura MVC | Capas Domain, Application, Infrastructure y WinForms | Todos | ArchitectureTests | Implementado |
| Persistencia local | SQL Server y ADO.NET | SIGEVIP.Infrastructure | Comprobación técnica previa | Preparado |
| RF11: monto anticipado | `Viaje.MontoAnticipado` con tipo decimal | SIGEVIP.Domain | Validación de monto negativo y cálculos | Implementado |
| RF12: modificar solo en Abierto | `IEstadoViaje.ValidarModificacion` | SIGEVIP.Domain | Bloqueo en EnRendicion, Aprobado y Cancelado | Parcialmente implementado |
| RF13: cancelar viaje | Transiciones a Cancelado desde Abierto y EnRendicion | SIGEVIP.Domain | Pruebas de cancelación | Parcialmente implementado |
| RF14: calcular total y saldo | `TotalGastado`, `SaldoPendiente` y `CalcularSaldo()` | SIGEVIP.Domain | Saldo positivo, negativo y cero | Implementado |
| RF20: viático asociado al viaje | `Viaje.AgregarViatico` y `Viatico.IdViaje` | SIGEVIP.Domain | Asociación y validación de fechas | Implementado |
| RF25: bloqueo de viáticos | Estado del viaje controla modificaciones | SIGEVIP.Domain | Pruebas en estados bloqueados | Implementado |
| RF26: enviar a rendición | `Viaje.EnviarARendicion()` | SIGEVIP.Domain | Abierto a EnRendicion | Implementado |
| RF27: impedir modificaciones | Estado EnRendicion rechaza modificaciones | SIGEVIP.Domain | Agregar viático bloqueado | Implementado |
| RF29: excluir viáticos | Baja lógica mediante `EstadoViatico.Excluido` | SIGEVIP.Domain | Exclusión y reactivación | Implementado en dominio |
| RF30: aprobar viaje completo | `Viaje.Aprobar()` | SIGEVIP.Domain | EnRendicion a Aprobado | Implementado en dominio |
| RF32: saldo final | Suma de vigentes menos anticipo | SIGEVIP.Domain | Cálculos económicos | Implementado |
| State en Viaje | `IEstadoViaje` y cuatro estados concretos | SIGEVIP.Domain | Transiciones válidas e inválidas | Implementado |
| Estado persistible | Enum y `EstadoViajeFactory` | SIGEVIP.Domain | Reconstrucción de estado | Implementado |
| Composite en seguridad | Patrón estructural | SIGEVIP.Domain | Pendientes | Pendiente |
| Auditoría básica | Persistencia y servicios | Infrastructure/Application | Pendientes | Pendiente |
| Visitas comerciales | Módulo funcional principal | Domain/Application/WinForms | Pendientes | Pendiente |
| SQL reproducible | Migraciones y datos de prueba | database | Pendientes | Preparado |
