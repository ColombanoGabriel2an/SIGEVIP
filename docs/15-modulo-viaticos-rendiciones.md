# Módulo Viáticos y Rendiciones

## 1. Objetivo

Implementar la gestión integral de Viáticos y Rendiciones de SIGEVIP, incluyendo:

- asociación de gastos a Viajes;
- carga opcional de Comprobantes;
- consulta y filtrado;
- envío a rendición;
- revisión administrativa;
- exclusión y reactivación lógica;
- ajuste de anticipos;
- aprobación;
- cancelación;
- persistencia SQL;
- autorización;
- interfaz funcional;
- pruebas reproducibles.

## 2. Requisitos cubiertos

- RF20: asociar Viáticos a un Viaje.
- RF21: registrar datos del Viático.
- RF22: registrar Comprobante opcional.
- RF23: identificar a la Persona pagadora.
- RF24: consultar Viáticos del Viaje.
- RF25: bloquear modificaciones según estado.
- RF26: enviar el Viaje a rendición.
- RF27: impedir modificaciones durante la rendición.
- RF28: revisar la rendición.
- RF29: excluir y reactivar Viáticos.
- RF30: aprobar la rendición.
- RF31: cancelar la rendición.
- RF32: calcular total gastado y saldo.
- RF37: impedir accesos no autorizados.
- RF38: ocultar opciones no habilitadas.
- RF39: controlar acceso mediante grupos y permisos.

## 3. Arquitectura

### Domain

Archivos principales:

- `Entities/Viaje.cs`
- `Entities/Viatico.cs`
- `Entities/Comprobante.cs`
- `Entities/Persona.cs`
- estados concretos de Viaje;
- enumeraciones de Viáticos y Comprobantes.

Responsabilidades:

- asociar cada Viático a un único Viaje;
- validar la fecha dentro del período;
- validar categoría y método de pago;
- exigir Persona pagadora cuando corresponde;
- validar monto positivo;
- exigir descripción cuando no existe Comprobante;
- validar datos fiscales del Comprobante;
- calcular el total del Comprobante;
- calcular el total gastado vigente;
- calcular el saldo pendiente;
- controlar exclusión y reactivación lógica;
- controlar transiciones del Viaje;
- registrar auditoría de envío, exclusión, reactivación, aprobación y cancelación.

### Application

Archivos principales:

- `ViaticoService.cs`
- `ViaticoFiltro.cs`
- `ViaticoListadoDto.cs`
- `RegistrarViaticoCommand.cs`
- `ModificarViaticoCommand.cs`
- `ComprobanteInput.cs`
- `IViaticoRepository.cs`
- `IPersonaConsultaViaticoRepository.cs`
- `RendicionService.cs`
- comandos y DTO de Rendiciones;
- `IRendicionRepository.cs`.

Responsabilidades:

- coordinar los casos de uso;
- validar sesión autenticada;
- validar Usuario activo;
- exigir permisos;
- validar identificadores;
- recuperar Viajes y Personas;
- registrar y modificar Viáticos;
- listar Viáticos;
- enviar Viajes a rendición;
- construir el detalle de revisión;
- excluir y reactivar Viáticos;
- ajustar anticipos;
- aprobar o cancelar Rendiciones.

### Infrastructure

Archivos principales:

- `Viaticos/PersonaConsultaViaticoRepository.cs`
- `Viaticos/ViaticoRepository.cs`
- `Rendiciones/RendicionRepository.cs`
- `Viajes/ViajeRepository.cs`.

Responsabilidades:

- consultar Personas pagadoras activas;
- insertar y actualizar Viáticos;
- insertar, actualizar o eliminar el Comprobante asociado;
- listar Viáticos con filtros;
- reconstruir Viáticos, Comprobantes y auditoría;
- reconstruir el agregado Viaje completo;
- persistir transiciones de Rendición;
- aplicar control de concurrencia;
- ejecutar transacciones;
- traducir errores técnicos.

### WinForms

Archivos principales:

- `Forms/ViaticosRendicionesForm.cs`
- `Forms/ViaticosForm.cs`
- `Forms/ViaticoEditForm.cs`
- `Forms/RendicionesForm.cs`
- `Forms/ViajeDetalleForm.cs`
- `Forms/MainForm.cs`
- `Navigation/SigevipApplicationContext.cs`
- `Program.cs`.

Responsabilidades:

- navegación al módulo;
- permisos visuales;
- selección de Viajes abiertos;
- consulta y filtros;
- alta y modificación de Viáticos;
- carga y modificación de Comprobantes;
- cálculo visual de totales;
- envío a rendición;
- consulta de Rendiciones pendientes;
- visualización de Viáticos, Visitas, Clientes y Participantes;
- exclusión y reactivación;
- ajuste de anticipos;
- aprobación y cancelación;
- confirmaciones y mensajes controlados.

## 4. Gestión de Viáticos

### Consultar

Permiso:

`VIATICO_CONSULTAR`

Filtros disponibles:

- fecha desde;
- fecha hasta;
- categoría;
- estado.

La consulta se realiza dentro de un Viaje seleccionado.

### Registrar

Permiso:

`VIATICO_REGISTRAR`

Validaciones:

- Viaje existente;
- Viaje en estado Abierto;
- fecha incluida dentro del período;
- categoría válida;
- método de pago válido;
- monto mayor que cero;
- Persona pagadora cuando el método es PagoPersonal o TarjetaCorporativa;
- ausencia de Persona pagadora para EfectivoEmpresa;
- descripción obligatoria cuando no existe Comprobante.

### Modificar

Permiso:

`VIATICO_MODIFICAR`

Solo pueden modificarse:

- Viáticos Vigentes;
- asociados a Viajes Abiertos.

### Comprobante opcional

Campos:

- tipo;
- CUIT del proveedor;
- razón social;
- situación fiscal;
- sucursal;
- número;
- monto gravado;
- impuestos.

Reglas:

- CUIT obligatorio;
- razón social obligatoria;
- sucursal de cuatro dígitos;
- número de ocho dígitos;
- importes no negativos;
- total calculado como monto gravado más impuestos;
- relación máxima de un Comprobante por Viático.

## 5. Rendiciones

### Enviar

Permiso:

`VIAJE_ENVIAR_RENDICION`

Transición:

`Abierto -> EnRendicion`

Después del envío:

- no pueden registrarse nuevos Viáticos;
- no pueden modificarse Viáticos existentes;
- el Viaje aparece en Rendiciones pendientes.

### Revisar

Permiso:

`RENDICION_REVISAR`

El detalle muestra:

- datos generales del Viaje;
- fecha de envío;
- anticipo;
- total gastado vigente;
- saldo;
- Participantes;
- Visitas;
- Clientes;
- Viáticos;
- Comprobantes;
- auditoría de exclusión y reactivación.

### Excluir Viático

Permiso:

`RENDICION_EXCLUIR_VIATICO`

Reglas:

- solo aplica a Viáticos Vigentes;
- el motivo es obligatorio;
- no elimina físicamente el registro;
- deja de integrar el total gastado;
- registra Usuario y fecha.

### Reactivar Viático

Permiso:

`RENDICION_REACTIVAR_VIATICO`

Reglas:

- solo aplica a Viáticos Excluidos;
- vuelve a integrar el total gastado;
- registra Usuario y fecha.

### Ajustar anticipo

Permiso:

`RENDICION_AJUSTAR_ANTICIPO`

El nuevo monto:

- no puede ser negativo;
- actualiza el saldo de la rendición.

### Aprobar

Permiso:

`RENDICION_APROBAR`

Transición:

`EnRendicion -> Aprobado`

La Rendición aprobada deja de aparecer como pendiente.

### Cancelar

Permiso:

`RENDICION_CANCELAR`

Transición:

`EnRendicion -> Cancelado`

El motivo es obligatorio.

## 6. Cálculos económicos

El total gastado considera únicamente Viáticos Vigentes.

Fórmula:

`TotalGastado = suma de Viáticos Vigentes`

Saldo:

`SaldoPendiente = TotalGastado - MontoAnticipado`

Interpretación:

- saldo positivo: la empresa debe reintegrar diferencia;
- saldo cero: no existe diferencia;
- saldo negativo: existe anticipo no consumido.

## 7. Base de datos

Migración principal:

`database/migrations/006_crear_viaticos_rendiciones.sql`

Validación:

`database/migrations/006_validar_viaticos_rendiciones.sql`

Tablas principales:

- `dbo.Viatico`
- `dbo.Comprobante`

También se ampliaron los datos de Viaje necesarios para la auditoría de Rendiciones.

La migración incorpora:

- claves primarias;
- claves foráneas;
- restricciones `CHECK`;
- restricciones `DEFAULT`;
- índices;
- unicidad del Comprobante por Viático;
- campos de auditoría;
- registro de versión `006`.

## 8. Seguridad

Permisos utilizados:

- `VIATICO_CONSULTAR`
- `VIATICO_REGISTRAR`
- `VIATICO_MODIFICAR`
- `VIAJE_ENVIAR_RENDICION`
- `RENDICION_REVISAR`
- `RENDICION_EXCLUIR_VIATICO`
- `RENDICION_REACTIVAR_VIATICO`
- `RENDICION_AJUSTAR_ANTICIPO`
- `RENDICION_APROBAR`
- `RENDICION_CANCELAR`

La seguridad se aplica en dos niveles:

1. visibilidad y habilitación de controles en WinForms;
2. autorización obligatoria en Application.

La interfaz no constituye el límite de seguridad.

## 9. Transacciones y concurrencia

Las operaciones de persistencia utilizan transacciones cuando modifican más de una tabla.

Ejemplos:

- insertar Viático y Comprobante;
- modificar Viático y reemplazar Comprobante;
- excluir o reactivar con auditoría;
- ajustar anticipo;
- aprobar Rendición;
- cancelar Rendición.

Las operaciones de Rendición verifican el estado esperado del Viaje y detectan actualizaciones concurrentes o agregados desactualizados.

## 10. Trazabilidad

| Requisito | Domain | Application | Infrastructure | WinForms | Pruebas |
|---|---|---|---|---|---|
| RF20 | Asociación Viaje-Viático | Registrar y modificar | Persistencia por IdViaje | ViaticosForm | Unitarias, integración y manual |
| RF21 | Viatico | Commands y Service | ViaticoRepository | ViaticoEditForm | Unitarias, integración y manual |
| RF22 | Comprobante | ComprobanteInput | Persistencia 0..1 | ViaticoEditForm | Unitarias, integración y manual |
| RF23 | Persona pagadora | Consulta y validación | PersonaConsultaViaticoRepository | Combo de pagador | Unitarias e integración |
| RF24 | Colección de Viáticos | ListarPorViaje | Consulta filtrada | Grilla y filtros | Integración y manual |
| RF25 | State | Validaciones de servicio | Estado persistido | Botones controlados | Unitarias y manual |
| RF26 | EnviarARendicion | Enviar | Transición y auditoría | Enviar a rendición | Unitarias, integración y manual |
| RF27 | State EnRendicion | Validaciones | Control de concurrencia | Acciones bloqueadas | Unitarias e integración |
| RF28 | Agregado completo | ObtenerDetalle | Reconstrucción | RendicionesForm | Integración y manual |
| RF29 | EstadoViatico | Excluir y Reactivar | Baja lógica y auditoría | Acciones de revisión | Unitarias, integración y manual |
| RF30 | State Aprobado | Aprobar | Persistencia de aprobación | RendicionesForm | Unitarias, integración y manual |
| RF31 | State Cancelado | Cancelar | Persistencia de cancelación | RendicionesForm | Unitarias, integración y manual |
| RF32 | Total y saldo | DTO de detalle | Reconstrucción económica | Resúmenes visuales | Unitarias, integración y manual |
| RF37 | Estados y reglas | ExigirPermiso | Contratos protegidos | Mensajes controlados | Application |
| RF38 | — | Permisos | — | Controles ocultos | Manual |
| RF39 | Grupo y Permiso | AutorizacionService | Seguridad persistida | MainForm y formularios | Unitarias e integración |

## 11. Commits

- `97a409d` — `Completo dominio de viaticos`
- `0fba174` — `Completo aplicacion de viaticos y rendiciones`
- `300350a` — `Agrego esquema SQL de viaticos y rendiciones`
- `6ae9268` — `Agrego consulta de personas pagadoras`
- `bef0ee3` — `Agrego persistencia de viaticos`
- `83582fd` — `Integro viaticos en reconstruccion de viajes`
- `823afc6` — `Agrego consulta y envio de rendiciones`
- `e73b7c5` — `Completo revision de rendiciones`
- `e2fdb0f` — `Integro navegacion de viaticos y rendiciones`
- `acb4607` — `Completo gestion de viaticos y comprobantes`
- `fb3fa6e` — `Completo interfaz de revision de rendiciones`

## 12. Estado

Implementación:

`COMPLETA DENTRO DEL MVP ACADÉMICO`

Validación:

`APROBADA`

Pendientes relacionados:

- reportes consolidados;
- exportación;
- adjuntar archivos digitales de Comprobantes;
- auditoría general consultable;
- mejoras visuales opcionales.
