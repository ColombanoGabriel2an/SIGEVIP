# Modelo de dominio: Viajes y Viáticos

## 1. Alcance

Este documento describe el primer incremento del modelo de dominio de SIGEVIP.

Incluye:

- Viaje.
- Viático.
- Tipo de viaje.
- Estado del viaje.
- Estado del viático.
- Patrón State.
- Reglas económicas.

No incluye todavía:

- Cliente.
- Visita.
- Persona.
- Participantes.
- Usuario.
- Grupo.
- Permiso.
- Persistencia.
- Autorización por roles.

## 2. Agregado principal

`Viaje` es la raíz del agregado de rendición.

Es responsable de controlar:

- Datos generales del viaje.
- Período de fechas.
- Tipo de viaje.
- Monto anticipado.
- Estado actual.
- Colección de viáticos.
- Incorporación de viáticos.
- Exclusión y reactivación de viáticos.
- Total gastado.
- Saldo pendiente.
- Transiciones de estado.

Los viáticos no deben administrarse independientemente del viaje en las operaciones del dominio.

## 3. Entidad Viaje

### Atributos

- `IdViaje`: identificador del viaje.
- `FechaInicio`: inicio del período.
- `FechaFin`: fin del período.
- `Descripcion`: descripción general.
- `TipoViaje`: clasificación funcional.
- `MontoAnticipado`: anticipo único asociado al viaje.
- `EstadoActual`: representación persistible del estado.
- `Viaticos`: colección de solo lectura.

### Propiedades calculadas

`TotalGastado`:

    suma de los montos de viáticos Vigentes

`SaldoPendiente`:

    TotalGastado - MontoAnticipado

### Comportamientos

- `AgregarViatico`
- `ExcluirViatico`
- `ReactivarViatico`
- `CalcularSaldo`
- `EnviarARendicion`
- `Aprobar`
- `Cancelar`

## 4. Entidad Viatico

### Atributos

- `IdViatico`: identificador del gasto.
- `IdViaje`: identificador del viaje al que pertenece.
- `Fecha`: fecha del gasto.
- `Monto`: importe decimal mayor que cero.
- `Descripcion`: detalle inicial del gasto.
- `Estado`: Vigente o Excluido.

### Comportamientos internos

- Asociar a un viaje.
- Excluir lógicamente.
- Reactivar.

Las operaciones internas evitan que capas externas alteren directamente la vigencia o asociación.

## 5. Enumeración TipoViaje

Valores exactos:

- `Desplazamiento`
- `EnOficina`
- `EventoFeria`

## 6. Enumeración EstadoViaje

Valores exactos:

- `Abierto`
- `EnRendicion`
- `Aprobado`
- `Cancelado`

## 7. Enumeración EstadoViatico

Valores:

- `Vigente`
- `Excluido`

## 8. Patrón State

La interfaz `IEstadoViaje` representa las operaciones cuyo resultado depende del estado actual.

Cada clase concreta encapsula las transiciones permitidas y rechaza las inválidas mediante `ReglaNegocioException`.

### Abierto

Permite:

- Agregar viáticos.
- Enviar a rendición.
- Cancelar.

No permite:

- Aprobar directamente.

### EnRendicion

Permite:

- Aprobar.
- Cancelar.
- Excluir viáticos.
- Reactivar viáticos.

Bloquea:

- Agregar viáticos.
- Modificaciones administrativas generales.

### Aprobado

Es final.

No permite:

- Modificar.
- Enviar a rendición.
- Aprobar nuevamente.
- Cancelar.

### Cancelado

Es final.

No permite:

- Modificar.
- Enviar a rendición.
- Aprobar.
- Cancelar nuevamente.

## 9. Compatibilidad con persistencia

El objeto State se mantiene únicamente en memoria.

Para persistencia se utilizará `EstadoViaje`.

Al reconstruir el agregado, `EstadoViajeFactory` transforma el enum almacenado en una implementación concreta de `IEstadoViaje`.

## 10. Pendientes del modelo

La regla que impide cancelar un viaje con visitas registradas queda pendiente hasta incorporar `Visita` en el Bloque 2.

Los datos de motivo, usuario y fecha de exclusión de un viático se incorporarán al implementar auditoría y casos de uso.
