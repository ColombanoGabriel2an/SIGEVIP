# Módulo Viajes

## 1. Objetivo

Implementar la gestión funcional de Viajes de SIGEVIP con participantes, persistencia SQL, autorización, interfaz y pruebas reproducibles.

## 2. Requisitos cubiertos

- RF10: registrar viaje.
- RF11: modificar viaje.
- RF12: consultar viajes.
- RF13: cancelar viaje.
- RF14: consultar detalle del viaje.
- RF15: filtrar viajes.
- RF37: impedir accesos no autorizados.
- RF38: ocultar opciones no habilitadas.
- RF39: acceso mediante grupos y permisos.

Alcance parcial:

- RF13 no contempla todavía Visitas persistidas.
- RF14 no muestra Viáticos persistidos ni saldo real.
- RF15 cubre fechas, estado y participante.

## 3. Arquitectura

### Domain

Archivos principales:

- `Entities/Viaje.cs`
- `Entities/Persona.cs`
- `Enums/TipoViaje.cs`
- `Enums/EstadoViaje.cs`
- estados concretos de Viaje.

Responsabilidades:

- representar el Viaje;
- validar datos generales;
- controlar participantes;
- impedir duplicados;
- preservar participantes históricos;
- controlar transiciones;
- controlar modificaciones;
- reconstruir el agregado.

### Application

Archivos:

- `ViajeFiltro.cs`
- `ViajeListadoDto.cs`
- `PersonaSeleccionDto.cs`
- `IViajeRepository.cs`
- `IPersonaConsultaRepository.cs`
- `ViajeService.cs`

Responsabilidades:

- coordinar casos de uso;
- validar sesión;
- validar Usuario activo;
- validar permisos;
- validar identificadores;
- validar participantes;
- separar contratos de persistencia.

### Infrastructure

Archivos:

- `Viajes/PersonaConsultaRepository.cs`
- `Viajes/ViajeRepository.cs`

Responsabilidades:

- consultar Personas activas;
- recuperar Personas por identificador;
- insertar Viajes y participantes;
- actualizar Viajes y participantes;
- reconstruir Viajes;
- cancelar;
- listar con filtros;
- ejecutar transacciones;
- traducir errores técnicos.

### WinForms

Archivos:

- `Forms/ViajesForm.cs`
- `Forms/ViajeEditForm.cs`
- `Forms/ViajeDetalleForm.cs`
- `Forms/MainForm.cs`
- `Navigation/SigevipApplicationContext.cs`
- `Program.cs`

Responsabilidades:

- navegación;
- listado;
- filtros;
- edición;
- selección múltiple;
- detalle;
- cancelación;
- mensajes;
- permisos visuales;
- composición de dependencias.

## 4. Casos de uso

### Listar

Permiso:

`VIAJE_CONSULTAR`

Filtros:

- fecha desde;
- fecha hasta;
- estado;
- participante.

### Registrar

Permiso:

`VIAJE_CREAR`

Validaciones:

- descripción obligatoria;
- período válido;
- tipo válido;
- monto no negativo;
- al menos un participante;
- participantes existentes y activos;
- ausencia de duplicados.

### Modificar

Permiso:

`VIAJE_CREAR`

Validaciones:

- identificador válido;
- Viaje existente;
- estado Abierto;
- nuevo período válido;
- participantes existentes y activos;
- al menos un participante.

### Consultar detalle

Permiso:

`VIAJE_CONSULTAR`

Muestra:

- identificador;
- descripción;
- tipo;
- fechas;
- estado;
- monto anticipado;
- participantes;
- participantes inactivos históricos.

### Cancelar

Permiso:

`VIAJE_CANCELAR`

La cancelación:

- utiliza las reglas del patrón State;
- persiste el estado Cancelado;
- no elimina el Viaje;
- en el alcance actual opera sobre Viajes sin Visitas persistidas.

## 5. Base de datos

Tablas:

- `dbo.Viaje`
- `dbo.ViajeParticipante`

Migración:

`database/migrations/004_crear_viajes.sql`

Seed:

`database/seed/003_permisos_modulo_viajes.sql`

Validación:

`database/migrations/004_validar_viajes.sql`

Resultado:

`VALIDACIÓN CORRECTA`

## 6. Seguridad

Permisos:

- `VIAJE_CONSULTAR`
- `VIAJE_CREAR`
- `VIAJE_CANCELAR`

La seguridad se aplica en:

1. visibilidad de botones;
2. apertura del módulo desde `MainForm`;
3. autorización obligatoria en `ViajeService`.

WinForms no constituye el límite de seguridad.

## 7. Transacciones

El alta ejecuta dentro de una misma transacción:

1. inserción de Viaje;
2. inserción de participantes;
3. commit.

La modificación ejecuta:

1. actualización de datos;
2. eliminación de asociaciones anteriores;
3. inserción de nuevas asociaciones;
4. commit.

Ante un error se ejecuta rollback.

## 8. Trazabilidad

| Requisito | Domain | Application | Infrastructure | WinForms | Pruebas |
|---|---|---|---|---|---|
| RF10 | Viaje y participantes | Registrar | Insertar transaccional | ViajeEditForm | Unitarias, SQL y manual |
| RF11 | ActualizarDatos | Modificar | Actualizar transaccional | ViajeEditForm | Unitarias, SQL y manual |
| RF12 | Reconstrucción | Obtener y Listar | Consultas ADO.NET | ViajesForm | SQL y manual |
| RF13 | Cancelar y State | Cancelar | UPDATE de estado | ViajesForm | Unitarias, SQL y manual |
| RF14 | Agregado Viaje | Obtener | Reconstrucción | ViajeDetalleForm | SQL y manual |
| RF15 | — | ViajeFiltro | Consulta dinámica | Controles de filtros | SQL y manual |
| RF37 | Estado de Usuario | ExigirPermiso | Contratos protegidos | Mensajes controlados | Application |
| RF38 | — | Permisos | — | Botones ocultos | Manual |
| RF39 | Grupo y Permiso | AutorizacionService | Seguridad persistida | MainForm y ViajesForm | Unitarias e integración |

## 9. Commits

- `8226625` — `Agrego casos de uso de viajes`
- `6914a1d` — `Implemento persistencia de viajes`
- `88fbd13` — `Agrego interfaz de gestion de viajes`

## 10. Estado

Implementación:

`COMPLETA DENTRO DEL ALCANCE APROBADO`

Validación:

`APROBADA`

Pendientes relacionados:

- persistencia de Visitas;
- persistencia de Viáticos;
- saldo económico persistido;
- historial completo;
- bloqueo de cancelación con Visitas persistidas.
