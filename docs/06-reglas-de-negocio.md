# Reglas de negocio de SIGEVIP

## 1. Viaje y Viático

### RN-VIA-01

Un Viaje nuevo nace en estado `Abierto`.

### RN-VIA-02

La fecha de inicio no puede ser posterior a la fecha de fin.

### RN-VIA-03

El tipo debe ser un valor definido.

### RN-VIA-04

El monto anticipado no puede ser negativo.

### RN-VIA-05

Solo un Viaje Abierto admite nuevos Viáticos.

### RN-VIA-06

Solo un Viaje Abierto admite nuevas Visitas.

### RN-VIA-07

Abierto puede pasar a EnRendicion.

### RN-VIA-08

EnRendicion puede pasar a Aprobado.

### RN-VIA-09

Abierto y EnRendicion pueden cancelarse cuando no poseen Visitas.

### RN-VIA-10

Un Viaje con Visitas no puede cancelarse.

### RN-VIA-11

Aprobado y Cancelado son estados finales.

### RN-VIA-12

Las transiciones inválidas generan `ReglaNegocioException`.

### RN-VIA-13

El monto de un Viático debe ser mayor que cero.

### RN-VIA-14

La fecha del Viático debe estar dentro del período.

### RN-VIA-15

Todo Viático nace Vigente.

### RN-VIA-16

La exclusión del Viático es lógica.

### RN-VIA-17

Solo los Viáticos vigentes integran el total gastado.

### RN-VIA-18

El saldo se calcula mediante:

    TotalGastado - MontoAnticipado


### RN-VIA-19

La descripción del Viaje es obligatoria.

### RN-VIA-20

Todo Viaje registrado debe tener al menos un participante.

### RN-VIA-21

No se admiten participantes duplicados.

### RN-VIA-22

Para registrar un Viaje solo pueden seleccionarse Personas activas. Al modificar, un participante histórico inactivo puede conservarse o quitarse, pero no puede agregarse una nueva Persona inactiva.

### RN-VIA-23

Los participantes inactivos se conservan al reconstruir un Viaje histórico y permanecen visibles en el selector de modificación.

### RN-VIA-24

La consulta de Viajes requiere `VIAJE_CONSULTAR`.

### RN-VIA-25

El alta y la modificación requieren `VIAJE_CREAR`.

### RN-VIA-26

La cancelación requiere `VIAJE_CANCELAR`.

### RN-VIA-27

Un Usuario sin sesión, inactivo o sin permiso no puede ejecutar operaciones de Viajes.

### RN-VIA-28

La inserción y actualización del Viaje y sus participantes son transaccionales.

### RN-VIA-29

La relación Viaje-Persona no admite asociaciones duplicadas.

### RN-VIA-30

Un nuevo período no puede dejar Visitas o Viáticos cargados fuera de las fechas del Viaje.

## 2. Cliente y Visita

### RN-CLI-01

La razón social es obligatoria.

### RN-CLI-02

El CUIT es obligatorio.

### RN-CLI-03

El CUIT se normaliza eliminando espacios y guiones.

### RN-CLI-04

Cliente nace activo.

### RN-CLI-05

Cliente utiliza baja lógica.

### RN-CLI-06

La unicidad global del CUIT no pertenece exclusivamente a la entidad.

Se controla mediante Application, Infrastructure y SQL Server.

### RN-CLI-07

Al registrar un Cliente no puede existir otro Cliente con el mismo CUIT normalizado.

### RN-CLI-08

Al modificar un Cliente, la verificación de CUIT debe excluir el identificador del Cliente actual.

### RN-CLI-09

La modificación no puede alterar el identificador ni el estado lógico del Cliente.

### RN-CLI-10

Un Cliente persistido debe reconstruirse conservando su estado activo o inactivo.

### RN-CLI-11

Las acciones de consulta requieren el permiso `CLIENTE_CONSULTAR`.

### RN-CLI-12

Las acciones de alta, modificación, activación y desactivación requieren el permiso `CLIENTE_GESTIONAR`.

### RN-CLI-13

Un Usuario sin sesión, inactivo o sin permiso no puede ejecutar operaciones de Clientes.

### RN-CLI-14

La baja de Cliente es lógica y no elimina físicamente el registro.

### RN-VIS-01

Toda Visita debe pertenecer a un Viaje.

### RN-VIS-02

La fecha debe estar dentro del período del Viaje.

### RN-VIS-03

La observación es obligatoria.

### RN-VIS-04

La localidad del encuentro es obligatoria.

### RN-VIS-05

Una Visita debe tener al menos un Cliente.

### RN-VIS-06

Puede asociarse con varios Clientes.

### RN-VIS-07

No admite Clientes nulos.

### RN-VIS-08

No admite Clientes duplicados.

### RN-VIS-09

Una Visita no puede reasignarse a otro Viaje.

### RN-VIS-10

Una Visita solo puede modificarse mientras su Viaje se encuentra Abierto.

### RN-VIS-11

La modificación conserva el identificador de la Visita y reemplaza sus datos y asociaciones de Clientes dentro de una única transacción.

### RN-VIS-12

La fecha modificada debe permanecer dentro del período del Viaje.

### RN-VIS-13

En una modificación, un Cliente histórico inactivo puede conservarse o quitarse, pero no puede agregarse un nuevo Cliente inactivo.

### RN-VIS-14

Toda modificación de Visita debe registrar una auditoría con acción `Modificacion` e identificador de la Visita.

### RN-VIS-15

La selección de Clientes y Participantes debe conservarse al filtrar u ordenar las grillas de selección.

## 3. Persona

### RN-PER-01

Nombre es obligatorio.

### RN-PER-02

Apellido es obligatorio.

### RN-PER-03

Email es obligatorio.

### RN-PER-04

Persona nace activa.

### RN-PER-05

La baja de Persona es lógica.

### RN-PER-06

Persona puede reactivarse.

## 4. Usuario

### RN-USU-01

Todo Usuario debe estar asociado con una Persona válida.

### RN-USU-02

El nombre de usuario es obligatorio.

### RN-USU-03

El nombre se normaliza eliminando espacios exteriores y convirtiéndolo a minúsculas.

### RN-USU-04

Usuario no almacena contraseña en texto plano.

### RN-USU-05

Hash y salt son obligatorios.

### RN-USU-06

Las iteraciones deben ser mayores que cero.

### RN-USU-07

Usuario nace activo.

### RN-USU-08

Usuario puede desactivarse y reactivarse.

### RN-USU-09

Usuario puede pertenecer a uno o varios Grupos.

### RN-USU-10

No se admiten Grupos nulos.

### RN-USU-11

No se admiten Grupos duplicados.

### RN-USU-12

La colección de Grupos no puede modificarse directamente.

### RN-USU-13

La unicidad global del nombre de usuario se controlará fuera de la entidad.

## 5. Grupo y Permiso

### RN-SEG-01

El código de Permiso es obligatorio.

### RN-SEG-02

El nombre de Permiso es obligatorio.

### RN-SEG-03

El código de Permiso se normaliza a mayúsculas.

### RN-SEG-04

Permiso nace activo.

### RN-SEG-05

Un Permiso inactivo no es efectivo.

### RN-SEG-06

El código de Grupo es obligatorio.

### RN-SEG-07

El nombre de Grupo es obligatorio.

### RN-SEG-08

Grupo nace activo.

### RN-SEG-09

Grupo puede contener Permisos.

### RN-SEG-10

Grupo puede contener otros Grupos.

### RN-SEG-11

Grupo no admite componentes nulos.

### RN-SEG-12

Grupo no admite componentes duplicados.

### RN-SEG-13

Grupo no puede contenerse a sí mismo.

### RN-SEG-14

Grupo no puede formar ciclos indirectos.

### RN-SEG-15

Grupo inactivo no aporta permisos.

### RN-SEG-16

Grupo hijo inactivo no aporta permisos.

### RN-SEG-17

Los permisos efectivos se deduplican por código normalizado.

## 6. Autenticación

### RN-AUT-01

Nombre de usuario y contraseña son obligatorios para autenticar.

### RN-AUT-02

El nombre de usuario se normaliza antes de consultar el repositorio.

### RN-AUT-03

Usuario inexistente no puede autenticarse.

### RN-AUT-04

Usuario inactivo no puede autenticarse.

### RN-AUT-05

Contraseña incorrecta no puede autenticarse.

### RN-AUT-06

Las credenciales inválidas producen un resultado fallido y no una excepción de flujo normal.

### RN-AUT-07

Usuario inexistente, inactivo y contraseña incorrecta utilizan el mismo mensaje público.

### RN-AUT-08

Una autenticación exitosa devuelve el Usuario autenticado.

## 7. Autorización

### RN-AUTZ-01

Usuario nulo no está autorizado.

### RN-AUTZ-02

Usuario inactivo no está autorizado.

### RN-AUTZ-03

El código de permiso solicitado es obligatorio.

### RN-AUTZ-04

El código se normaliza antes de comparar.

### RN-AUTZ-05

La autorización considera todos los Grupos activos del Usuario.

### RN-AUTZ-06

La autorización considera Permisos anidados.

### RN-AUTZ-07

La autorización ignora Grupos y Permisos inactivos.

### RN-AUTZ-08

La ausencia del Permiso produce resultado falso.

## 8. Sesión

### RN-SES-01

Una sesión nueva comienza sin Usuario.

### RN-SES-02

Solo un Usuario activo puede iniciar sesión.

### RN-SES-03

No puede iniciarse sesión con Usuario nulo.

### RN-SES-04

Cerrar sesión elimina el Usuario actual.

### RN-SES-05

La sesión no se implementa mediante estado global estático.

## 9. Hash de contraseñas

### RN-CRY-01

Las contraseñas se transforman mediante PBKDF2-HMAC-SHA256.

### RN-CRY-02

Cada creación de hash genera un salt aleatorio.

### RN-CRY-03

El salt posee 32 bytes.

### RN-CRY-04

El hash posee 32 bytes.

### RN-CRY-05

La implementación actual utiliza 100000 iteraciones.

### RN-CRY-06

Las iteraciones se almacenan con las credenciales.

### RN-CRY-07

La comparación de hashes se realiza en tiempo constante.

### RN-CRY-08

Un hash modificado no valida la contraseña.

### RN-CRY-09

Una contraseña vacía no puede procesarse.

## 10. Reglas pendientes

### RN-PEN-01

La unicidad de nombre de usuario debe asegurarse mediante servicio, repositorio e índice único.

### RN-PEN-02

La unicidad de CUIT fue implementada mediante servicio, repositorio e índice único.

### RN-PEN-03

La exclusión de Viáticos debe registrar motivo, Usuario y fecha.

### RN-PEN-04

Los casos de uso del módulo Clientes consultan `AutorizacionService`.

El módulo Viajes aplica el mismo criterio mediante `ViajeService`.

### RN-PEN-05

La interfaz de Clientes oculta las acciones de gestión cuando el Usuario no posee `CLIENTE_GESTIONAR`.

Los módulos posteriores deberán aplicar el mismo criterio.

### RN-PEN-06

Cambio y recuperación de contraseña requieren definición e implementación adicional.

### RN-PEN-07

La persistencia del Composite debe impedir ciclos también a nivel de datos.

## 11. Consolidación de reglas operativas

### RN-VIA-12 — Modificación según estado

Solo un Viaje en estado `Abierto` puede modificarse.

La regla se controla en:

- Domain, mediante State;
- Application, mediante `ViajeService`;
- WinForms, antes de abrir `ViajeEditForm`.

Los estados `EnRendicion`, `Aprobado` y `Cancelado` impiden la modificación.

Clasificación: obligatoria para el funcionamiento.

### RN-VIA-13 — Persona pagadora participante

Cuando un Viático requiere Persona pagadora, esta debe:

- existir;
- encontrarse activa;
- participar del Viaje al que pertenece el Viático.

El selector visual solo lista participantes activos, pero `ViaticoService`
repite la validación para impedir que otro cliente de Application eluda el
control de interfaz.

Clasificación: obligatoria para la integridad funcional.

### RN-VIA-14 — Total del Comprobante

Cuando un Viático posee Comprobante:

`MontoGravado + MontoImpuestos = Monto del Viático`

La igualdad se controla en WinForms y Application.

Los datos históricos se reconstruyen sin aplicar retroactivamente esta regla,
para no impedir la lectura de registros previos que deban corregirse.

Clasificación: obligatoria para la consistencia económica.

### RN-UI-01 — Representación de valores lógicos

Los valores lógicos destinados al usuario deben expresarse mediante términos
de negocio. La columna Comprobante utiliza `Sí` y `No` en lugar de `True` y
`False`.

Clasificación: recomendable.

### RN-UI-02 — Ordenamiento de listados

Las grillas principales de consulta deben permitir ordenamiento ascendente y
descendente cuando su origen de datos lo soporte.

La implementación actual cubre Clientes, Auditoría y Rendiciones. Su extensión
a las demás grillas se conserva como mejora recomendable.
