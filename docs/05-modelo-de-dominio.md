# Modelo de dominio: Viajes, Viáticos, Clientes y Visitas

## 1. Alcance

Este documento describe los dos primeros incrementos del modelo de dominio de SIGEVIP.

Incluye:

- Viaje.
- Viático.
- Cliente.
- Visita.
- Tipo de viaje.
- Estado del viaje.
- Estado del viático.
- Patrón State.
- Reglas económicas.
- Relación Viaje-Visita.
- Relación muchos a muchos Visita-Cliente.

No incluye todavía:

- Persona.
- Participantes.
- Usuario.
- Grupo.
- Permiso.
- Persistencia de las entidades.
- Tabla `VisitaCliente`.
- Servicios de aplicación.
- Autorización por roles.
- Formularios.
- Reportes.
- Mapas.
- Geolocalización.

## 2. Agregado principal

`Viaje` continúa siendo la raíz del agregado.

Es responsable de controlar:

- Datos generales del viaje.
- Período de fechas.
- Tipo de viaje.
- Monto anticipado.
- Estado actual.
- Colección de viáticos.
- Colección de visitas.
- Incorporación de viáticos.
- Incorporación de visitas.
- Exclusión y reactivación de viáticos.
- Total gastado.
- Saldo pendiente.
- Transiciones de estado.
- Regla de cancelación condicionada por visitas.

Los viáticos y las visitas se administran mediante operaciones del agregado Viaje.

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
- `Visitas`: colección de solo lectura.

### Propiedades calculadas

`TotalGastado`:

    suma de los montos de viáticos Vigentes

`SaldoPendiente`:

    TotalGastado - MontoAnticipado

### Comportamientos

- `AgregarViatico`
- `AgregarVisita`
- `ExcluirViatico`
- `ReactivarViatico`
- `CalcularSaldo`
- `EnviarARendicion`
- `Aprobar`
- `Cancelar`

### Reglas sobre visitas

- Solo el estado Abierto admite nuevas visitas.
- La fecha debe estar dentro del período del viaje.
- La visita debe poseer al menos un cliente.
- No se admiten visitas duplicadas.
- Una visita no puede reasignarse a otro viaje.
- Un viaje con visitas no puede cancelarse.

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

Las operaciones internas evitan que capas externas alteren directamente la vigencia o la asociación.

## 5. Entidad Cliente

### Atributos

- `IdCliente`: identificador.
- `RazonSocial`: nombre empresarial obligatorio.
- `Cuit`: identificador fiscal obligatorio.
- `Email`: correo electrónico.
- `Telefono`: teléfono.
- `Localidad`: localidad habitual.
- `Provincia`: provincia habitual.
- `Activo`: estado lógico.

### Comportamientos

- `Activar`
- `Desactivar`

### Normalización de CUIT

El CUIT se almacena sin espacios ni guiones.

Ejemplo:

    30-12345678-9 -> 30123456789

Esta normalización no valida matemáticamente el CUIT ni reemplaza una futura validación fiscal.

### Borrado lógico

Cliente no se elimina físicamente.

La propiedad `Activo` permite excluirlo de operaciones nuevas sin perder las relaciones históricas.

### Unicidad pendiente

La unicidad global de CUIT no se controla dentro de la entidad.

Se implementará mediante:

- servicio de aplicación;
- repositorio;
- índice único en SQL Server.

## 6. Entidad Visita

### Atributos

- `IdVisita`: identificador.
- `IdViaje`: viaje al que pertenece.
- `Fecha`: fecha concreta de la visita.
- `Observacion`: detalle obligatorio.
- `LocalidadEncuentro`: localidad concreta del encuentro.
- `Clientes`: colección de solo lectura.

### Comportamientos

- `AgregarCliente`
- asociación interna a un Viaje.

### Reglas

- Observación obligatoria.
- Localidad del encuentro obligatoria.
- Cliente no nulo.
- Uno o varios clientes.
- Sin clientes duplicados.
- La colección no puede modificarse directamente.
- Antes de incorporarse a un Viaje debe tener al menos un cliente.
- Una vez asociada no puede cambiar de Viaje.

## 7. Cardinalidades

### Viaje y Visita

    Viaje 1 -------- 0..N Visita

Cada Visita pertenece a un único Viaje.

No existen visitas independientes dentro del flujo funcional del sistema.

### Visita y Cliente

    Visita N -------- N Cliente

Cada Visita debe tener al menos un Cliente.

Un Cliente puede aparecer en múltiples visitas históricas.

En el dominio, la relación se representa desde Visita mediante una colección de Cliente.

Cliente no mantiene una colección bidireccional de visitas porque no aporta comportamiento necesario en este incremento.

## 8. Identificación de duplicados

### Cliente dentro de Visita

Dos clientes se consideran equivalentes para una asociación cuando:

- son la misma referencia;
- ambos tienen `IdCliente > 0` y el mismo identificador;
- poseen el mismo CUIT normalizado.

No se sobrescriben `Equals` ni `GetHashCode`.

Esta decisión evita problemas de igualdad entre entidades nuevas sin identificador persistido.

### Visita dentro de Viaje

Dos visitas se consideran duplicadas cuando:

- son la misma referencia;
- ambas tienen `IdVisita > 0` y el mismo identificador.

## 9. Enumeración TipoViaje

Valores exactos:

- `Desplazamiento`
- `EnOficina`
- `EventoFeria`

## 10. Enumeración EstadoViaje

Valores exactos:

- `Abierto`
- `EnRendicion`
- `Aprobado`
- `Cancelado`

## 11. Enumeración EstadoViatico

Valores:

- `Vigente`
- `Excluido`

## 12. Patrón State

La interfaz `IEstadoViaje` representa las operaciones cuyo resultado depende del estado actual.

Cada clase concreta encapsula las transiciones permitidas y rechaza las inválidas mediante `ReglaNegocioException`.

### Abierto

Permite:

- Agregar viáticos.
- Agregar visitas.
- Enviar a rendición.
- Cancelar cuando no existen visitas.

No permite:

- Aprobar directamente.

### EnRendicion

Permite:

- Aprobar.
- Cancelar cuando no existen visitas.
- Excluir viáticos.
- Reactivar viáticos.

Bloquea:

- Agregar viáticos.
- Agregar visitas.
- Modificaciones administrativas generales.

### Aprobado

Es final.

No permite:

- Modificar.
- Agregar viáticos.
- Agregar visitas.
- Enviar a rendición.
- Aprobar nuevamente.
- Cancelar.

### Cancelado

Es final.

No permite:

- Modificar.
- Agregar viáticos.
- Agregar visitas.
- Enviar a rendición.
- Aprobar.
- Cancelar nuevamente.

## 13. Cancelación condicionada

Antes de ejecutar la transición del patrón State, `Viaje.Cancelar()` verifica la colección de visitas.

Cuando existe al menos una visita:

- se genera `ReglaNegocioException`;
- no se ejecuta la transición;
- el estado anterior se conserva.

La regla se aplica tanto en Abierto como en EnRendicion.

## 14. Compatibilidad con persistencia

El objeto State se mantiene únicamente en memoria.

Para persistencia se utilizará `EstadoViaje`.

Al reconstruir el agregado, `EstadoViajeFactory` transforma el enum almacenado en una implementación concreta de `IEstadoViaje`.

La persistencia futura utilizará:

- tabla `Viaje`;
- tabla `Viatico`;
- tabla `Cliente`;
- tabla `Visita`;
- tabla asociativa `VisitaCliente`.

## 15. Pendientes del modelo

- Reconstrucción completa del agregado con colecciones persistidas.
- Unicidad global de CUIT.
- Casos de uso de modificación de Cliente.
- Persistencia de Visita y Cliente.
- Persistencia de `VisitaCliente`.
- Consultas históricas.
- Auditoría de exclusión de viáticos.
- Servicios de autorización.
