# Reglas de negocio: Viajes, Viáticos, Clientes y Visitas

## 1. Reglas de Viaje y Viático implementadas

### RN-VIA-01

Un viaje nuevo nace en estado `Abierto`.

### RN-VIA-02

La fecha de inicio no puede ser posterior a la fecha de fin.

### RN-VIA-03

El tipo de viaje debe ser uno de los valores definidos:

- Desplazamiento.
- EnOficina.
- EventoFeria.

### RN-VIA-04

El monto anticipado no puede ser negativo.

### RN-VIA-05

Solo un viaje en estado `Abierto` admite la incorporación de nuevos viáticos.

### RN-VIA-06

Un viaje `Abierto` puede pasar a `EnRendicion`.

### RN-VIA-07

Un viaje `EnRendicion` puede pasar a `Aprobado`.

### RN-VIA-08

Un viaje no puede aprobarse directamente desde `Abierto`.

### RN-VIA-09

Un viaje sin visitas puede cancelarse desde `Abierto` o `EnRendicion`.

### RN-VIA-10

Un viaje con una o más visitas registradas no puede cancelarse.

### RN-VIA-11

Cuando una cancelación se rechaza por visitas existentes, el estado previo se conserva.

### RN-VIA-12

`Aprobado` es un estado final.

### RN-VIA-13

`Cancelado` es un estado final.

### RN-VIA-14

Las transiciones inválidas generan `ReglaNegocioException`.

### RN-VIA-15

La colección de viáticos no puede modificarse desde el exterior.

### RN-VIA-16

Un mismo objeto viático no puede agregarse dos veces.

### RN-VIA-17

Un viático asociado a un viaje no puede reasignarse.

### RN-VIA-18

La fecha del viático debe estar entre `FechaInicio` y `FechaFin`, inclusive.

### RN-VIA-19

El monto de un viático debe ser mayor que cero.

### RN-VIA-20

Todo viático nuevo nace en estado `Vigente`.

### RN-VIA-21

La exclusión de un viático es lógica.

### RN-VIA-22

Un viático puede excluirse o reactivarse mientras el viaje está `EnRendicion`.

### RN-VIA-23

Solo los viáticos vigentes participan de `TotalGastado`.

### RN-VIA-24

Cuando no existen viáticos vigentes, `TotalGastado` es cero.

### RN-VIA-25

El saldo se calcula mediante:

    SaldoPendiente = TotalGastado - MontoAnticipado

### RN-VIA-26

Cuando el saldo es mayor que cero, la empresa debe pagar la diferencia.

### RN-VIA-27

Cuando el saldo es menor que cero, corresponde una devolución a la empresa.

### RN-VIA-28

Cuando el saldo es igual a cero, no existe diferencia económica.

### RN-VIA-29

La liquidación económica se realiza fuera de SIGEVIP.

## 2. Reglas de Cliente implementadas

### RN-CLI-01

La razón social es obligatoria.

### RN-CLI-02

El CUIT es obligatorio.

### RN-CLI-03

El CUIT se normaliza eliminando espacios y guiones.

### RN-CLI-04

La normalización no constituye una validación fiscal estricta.

### RN-CLI-05

Todo cliente nuevo nace activo.

### RN-CLI-06

Un cliente puede desactivarse lógicamente.

### RN-CLI-07

Un cliente desactivado puede volver a activarse.

### RN-CLI-08

Cliente no se elimina físicamente.

### RN-CLI-09

La unicidad global de CUIT no se controla dentro de la entidad.

## 3. Reglas de Visita implementadas

### RN-VIS-01

Toda visita debe incorporarse a un viaje existente.

### RN-VIS-02

Solo un viaje `Abierto` admite nuevas visitas.

### RN-VIS-03

EnRendicion, Aprobado y Cancelado bloquean nuevas visitas.

### RN-VIS-04

La fecha de la visita debe estar entre `FechaInicio` y `FechaFin`, inclusive.

### RN-VIS-05

La observación es obligatoria.

### RN-VIS-06

La localidad del encuentro es obligatoria.

### RN-VIS-07

Una visita debe tener al menos un cliente antes de incorporarse a un viaje.

### RN-VIS-08

Una visita puede asociarse con uno o varios clientes.

### RN-VIS-09

No se admiten clientes nulos.

### RN-VIS-10

No se admite dos veces el mismo cliente en una visita.

### RN-VIS-11

La colección de clientes no puede modificarse directamente desde el exterior.

### RN-VIS-12

La colección de visitas del viaje no puede modificarse directamente desde el exterior.

### RN-VIS-13

Una visita no puede agregarse dos veces al mismo viaje.

### RN-VIS-14

Una visita asociada a un viaje no puede reasignarse a otro.

### RN-VIS-15

La localidad concreta del encuentro pertenece a Visita.

### RN-VIS-16

La localidad habitual pertenece a Cliente.

### RN-VIS-17

Viatico pertenece exclusivamente a Viaje y no a Visita.

## 4. Reglas de identificación de duplicados

### RN-DUP-01

Un cliente se considera repetido si es la misma referencia.

### RN-DUP-02

Un cliente persistido se considera repetido cuando posee el mismo `IdCliente` mayor que cero.

### RN-DUP-03

Un cliente se considera repetido cuando posee el mismo CUIT normalizado.

### RN-DUP-04

Una visita se considera repetida si es la misma referencia.

### RN-DUP-05

Una visita persistida se considera repetida cuando posee el mismo `IdVisita` mayor que cero.

## 5. Reglas pendientes

### RN-PEN-01

La exclusión de un viático deberá registrar:

- Motivo.
- Usuario.
- Fecha y hora.

Estado: pendiente de Application, seguridad, auditoría y persistencia.

### RN-PEN-02

Solo el rol Administrativo podrá cargar viáticos, registrar visitas y enviar el viaje a rendición.

Estado: pendiente de la capa Application.

### RN-PEN-03

Solo el rol Gerente podrá excluir viáticos y aprobar el viaje completo.

Estado: pendiente de la capa Application.

### RN-PEN-04

La unicidad global de CUIT deberá controlarse mediante repositorio, servicio e índice único.

Estado: pendiente de Application, Infrastructure y SQL Server.

### RN-PEN-05

Las consultas históricas de clientes y visitas requieren repositorios y servicios.

Estado: pendiente.

### RN-PEN-06

El comprobante y sus validaciones fiscales se incorporarán en una ampliación posterior del modelo de viáticos.

Estado: pendiente de definición.
