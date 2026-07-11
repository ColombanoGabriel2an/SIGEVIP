# Decisiones técnicas

## DT-01 — Plataforma

Se utiliza .NET Framework 4.8 por compatibilidad con el entorno académico y Windows Forms.

## DT-02 — Interfaz

La interfaz se implementará con Windows Forms.

## DT-03 — Persistencia

Se utilizará ADO.NET con `System.Data.SqlClient` y SQL Server.

No se utilizará Entity Framework.

## DT-04 — Arquitectura

Se aplica una arquitectura en capas con MVC adaptado a Windows Forms.

## DT-05 — Base de datos

La base de datos será local y reproducible mediante scripts SQL.

La base existente se denomina `SIGEVIP`.

## DT-06 — Seguridad

La seguridad utilizará autenticación por usuario y contraseña, grupos y permisos.

El patrón Composite se aplicará a la estructura de permisos en un bloque posterior.

## DT-07 — Patrón State en Viaje

El patrón State se aplica a la entidad `Viaje`.

Estados:

- `Abierto`
- `EnRendicion`
- `Aprobado`
- `Cancelado`

Transiciones implementadas:

- `Abierto` a `EnRendicion`
- `Abierto` a `Cancelado`
- `EnRendicion` a `Aprobado`
- `EnRendicion` a `Cancelado`

`Aprobado` y `Cancelado` son estados finales.

Las transiciones inválidas generan `ReglaNegocioException`.

## DT-08 — Estado persistible

El estado se expone mediante el enum `EstadoViaje`.

Los objetos que implementan `IEstadoViaje` representan comportamiento en memoria y no serán almacenados directamente en SQL Server.

`EstadoViajeFactory` reconstruye el comportamiento desde el valor persistido.

## DT-09 — Importes monetarios

Los importes se representan mediante `decimal`.

No se utilizan `float` ni `double` para:

- Monto del viático.
- Monto anticipado.
- Total gastado.
- Saldo pendiente.

## DT-10 — Colección de viáticos

`Viaje` mantiene internamente una colección modificable, pero la expone mediante `IReadOnlyCollection<Viatico>`.

La incorporación, exclusión y reactivación de viáticos se realiza exclusivamente mediante operaciones del agregado.

## DT-11 — Exclusión lógica

Un viático no se elimina físicamente.

Su estado cambia entre:

- `Vigente`
- `Excluido`

Solo los viáticos vigentes intervienen en el cálculo de `TotalGastado`.

La exclusión y reactivación se permiten inicialmente cuando el viaje se encuentra `EnRendicion`.

Los datos de auditoría de la exclusión se incorporarán en el bloque de persistencia y aplicación.

## DT-12 — Pruebas

Se utiliza MSTest.

El primer bloque contiene pruebas sobre:

- Construcción de entidades.
- Validación de fechas.
- Validación de importes.
- Tipos de viaje.
- Colección controlada.
- Patrón State.
- Transiciones válidas e inválidas.
- Exclusión y reactivación lógica.
- Total gastado.
- Cálculo de saldo.
- Reconstrucción desde estado persistible.

## DT-13 — Control de versiones

Se utiliza Git con commits pequeños y descriptivos en tiempo verbal presente.

Ejemplo:

    Implemento dominio de viajes y viáticos

## DT-14 — Alcance excluido

No se implementarán como parte del alcance principal:

- Mapas.
- Geolocalización.
- Planificación automática de rutas.
- Servicios web.
- Arquitectura distribuida.
- Microservicios.
