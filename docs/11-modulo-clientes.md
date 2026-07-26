# Módulo Clientes

## 1. Objetivo

Implementar la gestión funcional de Clientes de SIGEVIP con trazabilidad desde los requisitos hasta la interfaz y las pruebas.

## 2. Requisitos cubiertos

- RF05: registrar clientes.
- RF06: modificar clientes.
- RF07: activar o desactivar clientes.
- RF08: listado filtrable de clientes.
- RF37: impedir accesos no autorizados.
- RF38: ocultar opciones no habilitadas.
- RF39: acceso mediante grupos y permisos.

RF09, historial del cliente, permanece pendiente para una etapa posterior.

## 3. Arquitectura

### Domain

Archivo principal:

`src/SIGEVIP.Domain/Entities/Cliente.cs`

Responsabilidades:

- representar al Cliente;
- validar datos obligatorios;
- normalizar CUIT;
- controlar la actualización;
- controlar el estado lógico;
- reconstruirse desde persistencia.

### Application

Archivos:

- `ClienteFiltro.cs`
- `ClienteListadoDto.cs`
- `IClienteRepository.cs`
- `ClienteService.cs`
- `AccesoDenegadoException.cs`

Responsabilidades:

- coordinar casos de uso;
- validar sesión;
- validar usuario activo;
- validar permisos;
- comprobar existencia;
- comprobar unicidad de CUIT;
- separar contratos de persistencia.

### Infrastructure

Archivo:

`src/SIGEVIP.Infrastructure/Clientes/ClienteRepository.cs`

Responsabilidades:

- ejecutar SQL parametrizado;
- mapear filas;
- reconstruir entidades;
- traducir errores técnicos;
- implementar filtros;
- persistir altas y modificaciones;
- actualizar estado lógico.

### WinForms

Archivos:

- `Forms/ClientesForm.cs`
- `Forms/ClienteEditForm.cs`
- `Forms/MainForm.cs`
- `Navigation/SigevipApplicationContext.cs`
- `Program.cs`

Responsabilidades:

- navegación;
- listado;
- filtros;
- edición;
- confirmaciones;
- mensajes;
- permisos visuales;
- composición de dependencias.

## 4. Casos de uso

### Listar

Permiso:

`CLIENTE_CONSULTAR`

Filtros:

- texto general;
- CUIT;
- localidad;
- provincia;
- estado.

### Registrar

Permiso:

`CLIENTE_GESTIONAR`

Validaciones:

- razón social obligatoria;
- CUIT obligatorio;
- CUIT no duplicado.

### Modificar

Permiso:

`CLIENTE_GESTIONAR`

Validaciones:

- identificador válido;
- Cliente existente;
- CUIT no utilizado por otro Cliente.

### Activar y desactivar

Permiso:

`CLIENTE_GESTIONAR`

La operación modifica únicamente el campo `Activo`.

## 5. Base de datos

Tabla:

`dbo.Cliente`

Migración:

`database/migrations/003_crear_clientes.sql`

Seed:

`database/seed/002_permisos_modulo_clientes.sql`

Validación:

`database/migrations/003_validar_clientes.sql`

Resultado:

`VALIDACIÓN CORRECTA`

## 6. Seguridad

Permisos:

- `CLIENTE_CONSULTAR`
- `CLIENTE_GESTIONAR`

La seguridad se aplica en dos niveles:

1. visibilidad de controles en WinForms;
2. autorización obligatoria en `ClienteService`.

La interfaz no constituye el límite de seguridad.

## 7. Manejo de errores

### Validaciones de negocio

Se utilizan:

- `ReglaNegocioException`
- `AccesoDenegadoException`

### Errores técnicos

Se utiliza:

`PersistenciaException`

La interfaz no muestra detalles internos de SQL Server.

## 8. Trazabilidad

| Requisito | Domain | Application | Infrastructure | WinForms | Pruebas |
|---|---|---|---|---|---|
| RF05 | Cliente | Registrar | Insertar | ClienteEditForm | Unitarias e integración |
| RF06 | ActualizarDatos | Modificar | Actualizar | ClienteEditForm | Unitarias e integración |
| RF07 | Activar/Desactivar | Activar/Desactivar | UPDATE Activo | ClientesForm | Unitarias e integración |
| RF08 | Datos del Cliente | Filtro y DTO | Consulta dinámica | Grilla y filtros | Integración y manual |
| RF37 | Usuario activo | ExigirPermiso | Contrato protegido | Mensajes controlados | Application |
| RF38 | — | Permisos | — | Botones ocultos | Manual |
| RF39 | Grupo y Permiso | AutorizacionService | Seguridad persistida | MainForm y ClientesForm | Unitarias e integración |

## 9. Commits

- `0994a78` — `Agrego casos de uso de clientes`
- `c8a36e3` — `Implemento persistencia de clientes`
- `8f45694` — `Agrego interfaz de gestion de clientes`

## 10. Estado

Implementación:

`COMPLETA`

Validación:

`APROBADA`

Pendiente relacionado:

- RF09: historial del cliente.
