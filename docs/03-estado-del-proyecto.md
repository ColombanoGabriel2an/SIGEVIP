# Estado del proyecto

## Etapa actual

Etapa 2: modelo de dominio.

Bloque actual: Cliente, Visita e integración de visitas con Viaje.

## Rama de trabajo

`desarrollo/dominio-viajes-viaticos`

## Punto de partida del Bloque 2

- Rama base conservada: `desarrollo/dominio-viajes-viaticos`
- Commit inicial aprobado: `90edf06`
- Mensaje: `Implemento dominio de viajes y viáticos`
- Working tree inicial verificado como limpio.
- Rama local sincronizada con `origin/desarrollo/dominio-viajes-viaticos`.

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
- Dominio de Viaje y Viatico.
- Patrón State aplicado a Viaje.
- Reglas económicas.
- Exclusión y reactivación lógica de viáticos.
- Excepción `ReglaNegocioException`.

## Completado en el Bloque 2

### Cliente

- Entidad `Cliente`.
- Razón social obligatoria.
- CUIT obligatorio.
- Normalización básica del CUIT.
- Estado inicial activo.
- Activación.
- Desactivación lógica.
- Conservación de datos históricos.
- Sin borrado físico.

### Visita

- Entidad `Visita`.
- Fecha de visita.
- Observación obligatoria.
- Localidad del encuentro obligatoria.
- Asociación controlada con uno o varios clientes.
- Colección privada de clientes.
- Exposición mediante `IReadOnlyCollection<Cliente>`.
- Prevención de clientes nulos.
- Prevención de asociaciones duplicadas.
- Asociación controlada con un viaje.

### Integración con Viaje

- Colección privada de visitas.
- Exposición mediante `IReadOnlyCollection<Visita>`.
- Alta de visitas como operación del agregado Viaje.
- Validación de estado Abierto.
- Validación de fecha dentro del período.
- Validación de al menos un cliente.
- Prevención de visitas duplicadas.
- Prevención de reasignación a otro viaje.
- Bloqueo de nuevas visitas en EnRendicion, Aprobado y Cancelado.
- Bloqueo de cancelación cuando existen visitas registradas.
- Conservación del estado cuando la cancelación es rechazada.

### Relación Visita-Cliente

- Relación muchos a muchos representada en el dominio mediante la colección de clientes de Visita.
- Cliente no mantiene una colección bidireccional de visitas.
- La tabla asociativa futura se denominará `VisitaCliente`.
- La persistencia SQL de esta relación todavía no fue implementada.

## Decisión de identificación de duplicados

### Clientes dentro de una visita

Un cliente se considera repetido cuando:

- es la misma referencia;
- ambos objetos tienen `IdCliente` mayor que cero e igual;
- ambos objetos poseen el mismo CUIT normalizado.

No se sobrescribieron `Equals` ni `GetHashCode`.

### Visitas dentro de un viaje

Una visita se considera repetida cuando:

- es la misma referencia;
- ambos objetos tienen `IdVisita` mayor que cero e igual.

## Commits realizados en el Bloque 2

- `dd6915d` — `Agrego dominio de clientes y visitas`
- `4ee6c1e` — `Integro visitas al agregado Viaje`

El commit documental se registrará después de validar estos documentos.

## Resultado técnico verificado

Compilación:

- 0 advertencias.
- 0 errores.

Pruebas:

- Pruebas totales: 71.
- Pruebas correctas: 71.
- Pruebas fallidas: 0.
- Pruebas omitidas: 0.

Ejecutor utilizado:

`VSTest 17.13.0 x64`

Tiempo total registrado en la última ejecución:

`0,8536 segundos`

## Estado del repositorio antes del commit documental

- Rama local sincronizada con la rama remota.
- Último commit publicado: `4ee6c1e`.
- Working tree limpio antes de modificar la documentación.
- No se realizó merge a `main`.
- No se creó una nueva rama.

## Pendiente inmediato

- Revisar diferencias documentales.
- Validar que no existan errores de formato.
- Crear commit documental.
- Ejecutar compilación y pruebas finales del Bloque 2.
- Subir el commit documental.
- Preparar el informe de cierre para MAESTRO.

## Pendiente de bloques posteriores

- Modificación completa de clientes mediante casos de uso.
- Unicidad global de CUIT.
- Repositorios de Cliente, Viaje y Visita.
- Persistencia de la relación `VisitaCliente`.
- Consultas históricas.
- Listados y filtros.
- Persona y participantes del viaje.
- Seguridad, usuarios, grupos y permisos.
- Patrón Composite.
- Autorización por roles.
- Servicios de aplicación.
- Migraciones SQL.
- Auditoría persistente.
- Interfaz funcional Windows Forms.
- Reportes.
- Mapas y geolocalización.
- Pruebas de integración con SQL Server.
