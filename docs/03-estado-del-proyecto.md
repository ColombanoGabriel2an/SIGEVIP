# Estado del proyecto

## Etapa actual

Etapa 2: modelo de dominio.

Bloque actual: Viajes, viáticos, patrón State y reglas económicas.

## Rama de trabajo

`desarrollo/dominio-viajes-viaticos`

## Base técnica

- Rama de origen: `desarrollo/estructura-base`
- Commit base: `544047d`
- Working tree inicial verificado como limpio.

## Completado previamente

- Repositorio Git configurado.
- Remoto GitHub verificado.
- Solución ubicada en la raíz.
- Proyectos Domain, Application, Infrastructure, WinForms y Tests creados.
- Referencias entre proyectos configuradas.
- Todos los proyectos configurados para .NET Framework 4.8.
- Proyecto MSTest configurado.
- SQL Server configurado.
- Base de datos `SIGEVIP` creada.
- Conexión mediante autenticación integrada comprobada.
- Aplicación WinForms mínima compilada y ejecutada.
- Documentación técnica inicial creada.

## Completado en el bloque actual

- Enum `TipoViaje`.
- Enum `EstadoViaje`.
- Enum `EstadoViatico`.
- Entidad `Viaje`.
- Entidad `Viatico`.
- Excepción `ReglaNegocioException`.
- Interfaz `IEstadoViaje`.
- Estado `Abierto`.
- Estado `EnRendicion`.
- Estado `Aprobado`.
- Estado `Cancelado`.
- Fábrica `EstadoViajeFactory`.
- Colección controlada de viáticos.
- Validación del período del viaje.
- Validación de fecha del viático.
- Validación de importes monetarios.
- Cálculo de `TotalGastado`.
- Cálculo de `SaldoPendiente`.
- Exclusión lógica de viáticos.
- Reactivación de viáticos.
- Reconstrucción de un viaje desde un estado persistible.
- Pruebas unitarias del dominio.

## Resultado técnico verificado

Compilación:

- 0 advertencias.
- 0 errores.

Pruebas:

- Pruebas totales: 38.
- Pruebas correctas: 38.
- Pruebas fallidas: 0.
- Pruebas omitidas: 0.

Ejecutor utilizado:

`VSTest 17.13.0 x64`

## Pendiente del bloque actual

- Confirmar documentación.
- Crear commit del bloque.
- Subir la rama a GitHub.
- Informar el resultado a la conversación MAESTRO.

## Pendiente de bloques posteriores

- Entidades Cliente y Visita.
- Relación muchos a muchos entre Visita y Cliente.
- Regla de cancelación condicionada por visitas existentes.
- Persona y participantes del viaje.
- Seguridad, usuarios, grupos y permisos.
- Patrón Composite.
- Servicios de aplicación y autorización.
- Interfaces de repositorio.
- Repositorios ADO.NET.
- Migraciones SQL.
- Auditoría persistente.
- Interfaz funcional Windows Forms.
- Pruebas de integración.
