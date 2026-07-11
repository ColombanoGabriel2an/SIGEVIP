# Reglas de negocio: Viajes y Viáticos

## 1. Reglas implementadas

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

Un viaje puede cancelarse desde `Abierto` o `EnRendicion`.

### RN-VIA-10

`Aprobado` es un estado final.

### RN-VIA-11

`Cancelado` es un estado final.

### RN-VIA-12

Las transiciones inválidas generan una `ReglaNegocioException` con un mensaje claro.

### RN-VIA-13

La colección de viáticos no puede modificarse libremente desde el exterior del agregado.

### RN-VIA-14

Un mismo objeto viático no puede agregarse dos veces al mismo viaje.

### RN-VIA-15

Un viático asociado a un viaje no puede reasignarse a otro viaje.

### RN-VIA-16

La fecha del viático debe encontrarse entre `FechaInicio` y `FechaFin`, inclusive.

### RN-VIA-17

El monto de un viático debe ser mayor que cero.

### RN-VIA-18

Todo viático nuevo nace en estado `Vigente`.

### RN-VIA-19

La exclusión de un viático es lógica y no implica borrado físico.

### RN-VIA-20

Un viático puede excluirse o reactivarse mientras el viaje se encuentra `EnRendicion`.

### RN-VIA-21

Solo los viáticos vigentes participan del cálculo de `TotalGastado`.

### RN-VIA-22

Cuando no existen viáticos vigentes, `TotalGastado` es cero.

### RN-VIA-23

El saldo se calcula mediante:

    SaldoPendiente = TotalGastado - MontoAnticipado

### RN-VIA-24

Cuando el saldo es mayor que cero, la empresa debe pagar la diferencia.

### RN-VIA-25

Cuando el saldo es menor que cero, corresponde una devolución a la empresa.

### RN-VIA-26

Cuando el saldo es igual a cero, no existe diferencia económica.

### RN-VIA-27

La liquidación económica se realiza fuera de SIGEVIP.

## 2. Reglas pendientes

### RN-PEN-01

La cancelación deberá impedirse cuando el viaje posea visitas registradas.

Estado: pendiente del Bloque 2, porque la entidad `Visita` todavía no existe.

### RN-PEN-02

La exclusión de un viático deberá registrar:

- Motivo.
- Usuario.
- Fecha y hora.

Estado: pendiente de Application, seguridad, auditoría y persistencia.

### RN-PEN-03

Solo el rol Administrativo podrá cargar viáticos y enviar el viaje a rendición.

Estado: pendiente de la capa Application.

### RN-PEN-04

Solo el rol Gerente podrá excluir viáticos y aprobar el viaje completo.

Estado: pendiente de la capa Application.

### RN-PEN-05

El comprobante y sus validaciones fiscales se incorporarán en una ampliación posterior del modelo de viáticos.

Estado: pendiente de definición e implementación incremental.
