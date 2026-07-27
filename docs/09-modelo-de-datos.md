# Modelo de datos de SIGEVIP

## 1. Alcance

Este documento describe el modelo relacional de seguridad implementado en el Bloque 4.

Incluye:

- Persona;
- Usuario;
- Grupo;
- Permiso;
- UsuarioGrupo;
- GrupoPermiso;
- GrupoGrupo;
- claves;
- índices;
- restricciones;
- catálogos iniciales;
- criterios de eliminación;
- validación ejecutada.

No incluye todavía:

- procedimientos almacenados;
- auditoría general consultable;
- reportes;
- geolocalización.

El documento incorpora actualmente:

- seguridad;
- Clientes;
- Viajes;
- participantes;
- Visitas;
- asociación Visita-Cliente;
- Viáticos;
- Comprobantes;
- Personas pagadoras;
- auditoría operativa de Rendiciones;
- exclusión y reactivación lógica;
- aprobación y cancelación de Rendiciones.

## 2. Scripts

### Migración

`database/migrations/002_crear_seguridad.sql`

Responsabilidades:

- crear el esquema de seguridad;
- preservar la base existente;
- ejecutar dentro de una transacción;
- revertir ante errores;
- registrar la versión `002`.

### Datos iniciales

`database/seed/001_catalogos_seguridad.sql`

Responsabilidades:

- crear grupos iniciales;
- crear permisos iniciales;
- crear asociaciones Grupo-Permiso;
- permitir reejecución sin duplicados.

### Validación

`database/migrations/002_validar_seguridad.sql`

Responsabilidades:

- comprobar migración;
- comprobar tablas;
- comprobar cantidades;
- comprobar catálogos;
- comprobar asociaciones;
- comprobar índices;
- comprobar claves foráneas;
- comprobar restricciones;
- comprobar ausencia de duplicados.

## 3. Tabla Persona

Nombre:

`dbo.Persona`

Columnas:

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdPersona | INT IDENTITY | No | Clave primaria |
| Nombre | NVARCHAR(100) | No | Nombre de la persona |
| Apellido | NVARCHAR(100) | No | Apellido de la persona |
| Email | NVARCHAR(254) | No | Correo electrónico |
| Activo | BIT | No | Estado lógico |

Restricciones:

- `PK_Persona`
- `CK_Persona_Nombre_NoVacio`
- `CK_Persona_Apellido_NoVacio`
- `CK_Persona_Email_NoVacio`
- `DF_Persona_Activo`

No se impuso unicidad global sobre Email.

## 4. Tabla Usuario

Nombre:

`dbo.Usuario`

Columnas:

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdUsuario | INT IDENTITY | No | Clave primaria |
| IdPersona | INT | No | Persona asociada |
| NombreUsuario | NVARCHAR(100) | No | Identidad de acceso |
| PasswordHash | VARBINARY(32) | No | Hash PBKDF2 |
| PasswordSalt | VARBINARY(32) | No | Salt criptográfico |
| IteracionesPassword | INT | No | Iteraciones utilizadas |
| Activo | BIT | No | Estado lógico |

Claves:

- `PK_Usuario`
- `FK_Usuario_Persona`

Índices:

- `UX_Usuario_NombreUsuario`
- `UX_Usuario_IdPersona`

Restricciones:

- `CK_Usuario_NombreUsuario_NoVacio`
- `CK_Usuario_PasswordHash_Longitud`
- `CK_Usuario_PasswordSalt_Longitud`
- `CK_Usuario_IteracionesPassword`
- `DF_Usuario_Activo`

Reglas representadas:

- cada Usuario requiere una Persona;
- una Persona puede tener como máximo un Usuario;
- el nombre de usuario es único;
- hash y salt deben medir 32 bytes;
- las iteraciones deben ser mayores que cero;
- la contraseña no se almacena en texto plano.

## 5. Tabla Grupo

Nombre:

`dbo.Grupo`

Columnas:

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdGrupo | INT IDENTITY | No | Clave primaria |
| Codigo | NVARCHAR(100) | No | Código normalizado |
| Nombre | NVARCHAR(150) | No | Nombre visible |
| Descripcion | NVARCHAR(500) | Sí | Descripción funcional |
| Activo | BIT | No | Estado lógico |

Restricciones e índices:

- `PK_Grupo`
- `UX_Grupo_Codigo`
- `CK_Grupo_Codigo_NoVacio`
- `CK_Grupo_Nombre_NoVacio`
- `DF_Grupo_Activo`

## 6. Tabla Permiso

Nombre:

`dbo.Permiso`

Columnas:

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdPermiso | INT IDENTITY | No | Clave primaria |
| Codigo | NVARCHAR(100) | No | Código normalizado |
| Nombre | NVARCHAR(150) | No | Nombre visible |
| Descripcion | NVARCHAR(500) | Sí | Descripción funcional |
| Activo | BIT | No | Estado lógico |

Restricciones e índices:

- `PK_Permiso`
- `UX_Permiso_Codigo`
- `CK_Permiso_Codigo_NoVacio`
- `CK_Permiso_Nombre_NoVacio`
- `DF_Permiso_Activo`

## 7. Tabla UsuarioGrupo

Nombre:

`dbo.UsuarioGrupo`

Columnas:

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdUsuario | INT | No | Usuario asociado |
| IdGrupo | INT | No | Grupo asociado |

Claves:

- `PK_UsuarioGrupo`
- `FK_UsuarioGrupo_Usuario`
- `FK_UsuarioGrupo_Grupo`

Índice auxiliar:

- `IX_UsuarioGrupo_IdGrupo`

Cardinalidad:

    Usuario N -------- N Grupo

La clave primaria compuesta impide asignaciones duplicadas.

## 8. Tabla GrupoPermiso

Nombre:

`dbo.GrupoPermiso`

Columnas:

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdGrupo | INT | No | Grupo asociado |
| IdPermiso | INT | No | Permiso asociado |

Claves:

- `PK_GrupoPermiso`
- `FK_GrupoPermiso_Grupo`
- `FK_GrupoPermiso_Permiso`

Índice auxiliar:

- `IX_GrupoPermiso_IdPermiso`

Cardinalidad:

    Grupo N -------- N Permiso

La clave primaria compuesta impide asociaciones duplicadas.

## 9. Tabla GrupoGrupo

Nombre:

`dbo.GrupoGrupo`

Columnas:

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdGrupoPadre | INT | No | Grupo compuesto |
| IdGrupoHijo | INT | No | Grupo contenido |

Claves:

- `PK_GrupoGrupo`
- `FK_GrupoGrupo_GrupoPadre`
- `FK_GrupoGrupo_GrupoHijo`

Restricción:

- `CK_GrupoGrupo_GruposDiferentes`

Índice auxiliar:

- `IX_GrupoGrupo_IdGrupoHijo`

Cardinalidad:

    Grupo N -------- N Grupo

La tabla representa la parte jerárquica del patrón Composite.

La base impide:

- autorreferencia directa;
- relación padre-hijo duplicada.

Application deberá impedir:

- ciclos indirectos;
- reconstrucciones inconsistentes.

## 10. Persistencia del Composite

El Composite se persiste mediante dos tablas:

- `GrupoPermiso`;
- `GrupoGrupo`.

No se utiliza una tabla única `GrupoComponente`.

Esta decisión permite:

- claves foráneas directas;
- consultas más simples;
- integridad referencial explícita;
- mejor comprensión académica;
- menor complejidad en ADO.NET.

## 11. Bajas lógicas

Las tablas principales de seguridad utilizan:

`Activo BIT NOT NULL DEFAULT 1`

Aplica a:

- Persona;
- Usuario;
- Grupo;
- Permiso.

No se implementó eliminación física como operación funcional.

Las tablas asociativas no contienen columna Activo.

Las asignaciones se podrán agregar o retirar mediante operaciones transaccionales posteriores.

## 12. Política de cascada

Todas las claves foráneas utilizan:

`NO_ACTION`

No se configuró:

`ON DELETE CASCADE`

Motivos:

- preservar relaciones históricas;
- evitar eliminaciones accidentales;
- mantener coherencia con las bajas lógicas;
- exigir decisiones explícitas desde Application.

## 13. Grupos iniciales

| Código | Nombre |
|---|---|
| COMERCIAL | Comercial |
| ADMINISTRATIVO | Administrativo |
| GERENTE | Gerente |
| ADMINISTRADOR_GENERAL | Administrador General |

## 14. Permisos iniciales

- `CLIENTE_CONSULTAR`
- `CLIENTE_GESTIONAR`
- `VIAJE_CREAR`
- `VIAJE_CONSULTAR`
- `VISITA_REGISTRAR`
- `VIATICO_CARGAR`
- `VIATICO_MODIFICAR`
- `VIAJE_ENVIAR_RENDICION`
- `RENDICION_REVISAR`
- `VIATICO_EXCLUIR`
- `VIATICO_REACTIVAR`
- `VIAJE_APROBAR`
- `VIAJE_CANCELAR`
- `USUARIO_GESTIONAR`
- `GRUPO_GESTIONAR`
- `PERMISO_GESTIONAR`
- `AUDITORIA_CONSULTAR`

## 15. Asignaciones iniciales

### COMERCIAL

- `CLIENTE_CONSULTAR`
- `VIAJE_CREAR`
- `VIAJE_CONSULTAR`
- `VISITA_REGISTRAR`

Cantidad:

4 permisos.

### ADMINISTRATIVO

- `CLIENTE_CONSULTAR`
- `VIAJE_CONSULTAR`
- `VIATICO_CARGAR`
- `VIATICO_MODIFICAR`
- `VIAJE_ENVIAR_RENDICION`

Cantidad:

5 permisos.

### GERENTE

- `CLIENTE_CONSULTAR`
- `VIAJE_CONSULTAR`
- `RENDICION_REVISAR`
- `VIATICO_EXCLUIR`
- `VIATICO_REACTIVAR`
- `VIAJE_APROBAR`
- `VIAJE_CANCELAR`

Cantidad:

7 permisos.

### ADMINISTRADOR_GENERAL

- `CLIENTE_CONSULTAR`
- `VIAJE_CONSULTAR`
- `USUARIO_GESTIONAR`
- `GRUPO_GESTIONAR`
- `PERMISO_GESTIONAR`
- `AUDITORIA_CONSULTAR`

Cantidad:

6 permisos.

Total:

22 asociaciones `GrupoPermiso`.

## 16. Seed

El seed es reejecutable.

Comportamiento:

- actualiza nombres y descripciones de catálogos conocidos;
- inserta grupos faltantes;
- inserta permisos faltantes;
- inserta asociaciones faltantes;
- no cambia el estado lógico de registros existentes;
- no crea usuarios;
- no crea contraseñas;
- no crea relaciones GrupoGrupo.

## 17. Validación ejecutada

Resultado de cantidades:

| Elemento | Cantidad |
|---|---:|
| Persona | 0 |
| Usuario | 0 |
| Grupo | 4 |
| Permiso | 17 |
| UsuarioGrupo | 0 |
| GrupoPermiso | 22 |
| GrupoGrupo | 0 |

Se verificaron:

- siete tablas;
- claves primarias;
- claves foráneas;
- restricciones CHECK;
- índices únicos;
- índices auxiliares;
- acciones `NO_ACTION`;
- ausencia de duplicados;
- reejecución del seed.

Resultado consolidado:

`VALIDACIÓN CORRECTA`

## 18. Reglas implementadas en el Bloque 5

- Repositorio ADO.NET de autenticación.
- Reconstrucción de Usuario con sus grupos.
- Reconstrucción de Grupo con permisos y grupos hijos.
- Detección de ciclos indirectos.
- Creación transaccional del administrador inicial.
- Integración con SQL Server.
- Pruebas de integración.
- PBKDF2 persistido.
- Rollback e idempotencia.

## 19. Reglas pendientes

- Repositorios de mantenimiento de usuarios.
- Alta y modificación persistente de grupos.
- Alta y modificación persistente de permisos.
- Persistencia funcional de nuevas asociaciones.
- Auditoría persistente.
- Integración con WinForms.

## 20. Commits técnicos

Persistencia SQL:

- `a4b631c` — `Agrego esquema SQL de seguridad`
- `2e567f2` — `Documento persistencia de seguridad`

Integración persistente:

- `cb52c7e` — `Implemento repositorio de autenticación`
- `514ba5a` — `Agrego inicialización del administrador`
- `a89c186` — `Agrego herramienta de configuración inicial`
- `62396ba` — `Pruebo jerarquías persistidas de seguridad`

## Modelo relacional de Clientes

### Script de migración

`database/migrations/003_crear_clientes.sql`

Responsabilidades:

- crear `dbo.Cliente`;
- crear restricciones;
- crear índices;
- ejecutar en transacción;
- registrar la versión `003`;
- permitir reejecución.

### Tabla Cliente

Nombre:

`dbo.Cliente`

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdCliente | INT IDENTITY | No | Clave primaria |
| RazonSocial | NVARCHAR(150) | No | Denominación del cliente |
| Cuit | NVARCHAR(20) | No | CUIT normalizado |
| Email | NVARCHAR(150) | Sí | Correo electrónico |
| Telefono | NVARCHAR(50) | Sí | Teléfono |
| Localidad | NVARCHAR(100) | Sí | Localidad |
| Provincia | NVARCHAR(100) | Sí | Provincia |
| Activo | BIT | No | Estado lógico |

Restricciones:

- `PK_Cliente`
- `CK_Cliente_RazonSocial_NoVacia`
- `CK_Cliente_Cuit_NoVacio`
- `DF_Cliente_Activo`

Índices:

- `UX_Cliente_Cuit`
- `IX_Cliente_RazonSocial`
- `IX_Cliente_Activo`

### Unicidad de CUIT

El índice:

`UX_Cliente_Cuit`

impide duplicados incluso ante concurrencia o acceso directo a la base.

Application realiza una validación anticipada para ofrecer un mensaje funcional.

Infrastructure reconoce los errores SQL `2601` y `2627` y los traduce a una excepción de negocio.

### Baja lógica

Cliente utiliza:

`Activo BIT NOT NULL DEFAULT 1`

La activación y desactivación se realizan mediante `UPDATE`.

No se elimina físicamente el registro.

### Seed del módulo

Script:

`database/seed/002_permisos_modulo_clientes.sql`

Responsabilidad:

- asignar `CLIENTE_GESTIONAR` a `ADMINISTRADOR_GENERAL`;
- evitar asociaciones duplicadas;
- permitir reejecución segura.

### Validación

Script:

`database/migrations/003_validar_clientes.sql`

Se verificó:

- versión `003`;
- existencia de `dbo.Cliente`;
- columnas;
- tipos;
- longitudes;
- nulabilidad;
- identity;
- clave primaria;
- índices;
- restricciones CHECK;
- DEFAULT;
- ausencia de CUIT duplicados;
- asignación de `CLIENTE_GESTIONAR`.

Resultado:

`VALIDACIÓN CORRECTA`

## Modelo relacional de Viajes

### Script de migración

`database/migrations/004_crear_viajes.sql`

Responsabilidades:

- crear `dbo.Viaje`;
- crear `dbo.ViajeParticipante`;
- crear restricciones;
- crear índices;
- ejecutar en una transacción;
- registrar la versión `004`;
- permitir reejecución segura.

### Tabla Viaje

Nombre:

`dbo.Viaje`

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdViaje | INT IDENTITY | No | Clave primaria |
| FechaInicio | DATE | No | Inicio del período |
| FechaFin | DATE | No | Fin del período |
| Descripcion | NVARCHAR(500) | No | Descripción funcional |
| TipoViaje | TINYINT | No | Tipo persistible |
| MontoAnticipado | DECIMAL(18,2) | No | Anticipo |
| EstadoViaje | TINYINT | No | Estado persistible |

Restricciones:

- `PK_Viaje`
- `CK_Viaje_Descripcion_NoVacia`
- `CK_Viaje_Periodo`
- `CK_Viaje_TipoViaje`
- `CK_Viaje_MontoAnticipado`
- `CK_Viaje_EstadoViaje`
- `DF_Viaje_MontoAnticipado`
- `DF_Viaje_EstadoViaje`

Índices:

- `IX_Viaje_FechaInicio`
- `IX_Viaje_EstadoViaje`

### Correspondencia de TipoViaje

| Valor | Tipo |
|---:|---|
| 1 | Desplazamiento |
| 2 | EnOficina |
| 3 | EventoFeria |

### Correspondencia de EstadoViaje

| Valor | Estado |
|---:|---|
| 1 | Abierto |
| 2 | EnRendicion |
| 3 | Aprobado |
| 4 | Cancelado |

### Tabla ViajeParticipante

Nombre:

`dbo.ViajeParticipante`

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdViaje | INT | No | Viaje asociado |
| IdPersona | INT | No | Persona participante |

Claves:

- `PK_ViajeParticipante`
- `FK_ViajeParticipante_Viaje`
- `FK_ViajeParticipante_Persona`

Índice:

- `IX_ViajeParticipante_IdPersona`

Cardinalidad:

    Viaje N -------- N Persona

La clave primaria compuesta impide asociaciones duplicadas.

No se utiliza `ON DELETE CASCADE`.

### Seed del módulo

Script:

`database/seed/003_permisos_modulo_viajes.sql`

Responsabilidades:

- asignar `VIAJE_CREAR` a `ADMINISTRADOR_GENERAL`;
- asignar `VIAJE_CANCELAR` a `ADMINISTRADOR_GENERAL`;
- conservar `VIAJE_CONSULTAR`;
- impedir asociaciones duplicadas;
- permitir reejecución segura.

### Validación

Script:

`database/migrations/004_validar_viajes.sql`

Se verificó:

- versión `004`;
- existencia de ambas tablas;
- columnas;
- tipos;
- longitudes;
- precisión y escala;
- claves primarias;
- claves foráneas;
- índices;
- restricciones CHECK;
- restricciones DEFAULT;
- ausencia de participantes duplicados;
- ausencia de períodos inválidos;
- ausencia de montos negativos;
- ausencia de tipos inválidos;
- ausencia de estados inválidos;
- permisos del administrador.

Resultado:

`VALIDACIÓN CORRECTA`

## Modelo relacional de Visitas

### Script de migración

`database/migrations/005_crear_visitas.sql`

Responsabilidades:

- crear `dbo.Visita`;
- crear `dbo.VisitaCliente`;
- definir claves y restricciones;
- crear índices;
- ejecutar dentro de una transacción;
- registrar la versión `005`;
- permitir reejecución segura.

### Tabla Visita

Nombre:

`dbo.Visita`

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdVisita | INT IDENTITY | No | Clave primaria |
| IdViaje | INT | No | Viaje asociado |
| Fecha | DATE | No | Fecha de la visita |
| Observacion | NVARCHAR(1000) | No | Resultado u observación |
| LocalidadEncuentro | NVARCHAR(150) | No | Localidad del encuentro |

Claves:

- `PK_Visita`
- `FK_Visita_Viaje`

Restricciones:

- observación no vacía;
- localidad no vacía.

Índices:

- `IX_Visita_IdViaje_Fecha`;
- `IX_Visita_Fecha`.

Cardinalidad:

    Viaje 1 -------- N Visita

Una Visita pertenece a un único Viaje.

No se utiliza `ON DELETE CASCADE`.

La regla que exige que la fecha esté dentro del período del Viaje se valida en Domain y Application. La validación SQL comprueba que los datos existentes respeten esa regla.

### Tabla VisitaCliente

Nombre:

`dbo.VisitaCliente`

| Columna | Tipo | Nulo | Descripción |
|---|---|---:|---|
| IdVisita | INT | No | Visita asociada |
| IdCliente | INT | No | Cliente visitado |

Claves:

- `PK_VisitaCliente`;
- `FK_VisitaCliente_Visita`;
- `FK_VisitaCliente_Cliente`.

Índice:

- `IX_VisitaCliente_IdCliente`.

Cardinalidad:

    Visita N -------- N Cliente

La clave primaria compuesta:

`(IdVisita, IdCliente)`

impide asociaciones duplicadas.

No se utiliza `ON DELETE CASCADE`.

Los Clientes pueden desactivarse sin perder su participación histórica en Visitas.

### Persistencia transaccional

`VisitaRepository.Insertar` realiza dentro de una misma transacción:

1. inserción de `dbo.Visita`;
2. recuperación de `IdVisita`;
3. inserción de asociaciones en `dbo.VisitaCliente`;
4. commit únicamente cuando toda la operación finaliza correctamente.

Ante un error se ejecuta rollback.

### Reconstrucción histórica

`ViajeRepository.ObtenerPorId` recupera:

- datos del Viaje;
- participantes;
- Visitas;
- Clientes de cada Visita;
- estado activo o inactivo de cada Cliente.

Esto permite reconstruir el agregado completo y conservar relaciones históricas.

### Bloqueo de cancelación

La cancelación se protege en dos niveles:

- Domain rechaza cancelar un Viaje que contiene Visitas;
- SQL utiliza `NOT EXISTS` sobre `dbo.Visita`.

La validación SQL evita que un agregado desactualizado cancele un Viaje después de que otra operación haya registrado una Visita.

### Seed del módulo

Script:

`database/seed/004_permisos_modulo_visitas.sql`

Responsabilidades:

- asignar `VISITA_REGISTRAR` a `ADMINISTRADOR_GENERAL`;
- conservar la asignación del grupo `COMERCIAL`;
- impedir asociaciones duplicadas;
- permitir reejecución segura.

### Validación

Script:

`database/migrations/005_validar_visitas.sql`

Se verificó:

- versión `005`;
- existencia de `dbo.Visita`;
- existencia de `dbo.VisitaCliente`;
- columnas;
- tipos;
- longitudes;
- nulabilidad;
- identity;
- claves primarias;
- claves foráneas;
- índices;
- restricciones;
- ausencia de asociaciones duplicadas;
- integridad de Viajes y Clientes;
- fechas dentro de los períodos;
- asignación del permiso.

Resultado:

`VALIDACIÓN CORRECTA`

## Modelo relacional de Viáticos y Rendiciones

### Script de migración

`database/migrations/006_crear_viaticos_rendiciones.sql`

Responsabilidades:

- crear `dbo.Viatico`;
- crear `dbo.Comprobante`;
- ampliar `dbo.Viaje` con datos de auditoría;
- crear claves y restricciones;
- crear índices;
- ejecutar dentro de una transacción;
- registrar la versión `006`;
- permitir reejecución segura.

### Tabla Viatico

Nombre:

`dbo.Viatico`

Representa un gasto asociado a un Viaje.

Datos principales:

- Viaje asociado;
- fecha;
- categoría;
- método de pago;
- Persona pagadora opcional;
- monto;
- descripción;
- estado;
- motivo de exclusión;
- Usuario y fecha de exclusión;
- Usuario y fecha de reactivación.

Cardinalidad:

    Viaje 1 -------- N Viatico

La eliminación funcional es lógica.

Solo los Viáticos Vigentes intervienen en el total gastado.

### Tabla Comprobante

Nombre:

`dbo.Comprobante`

Representa el documento fiscal opcional de un Viático.

Datos principales:

- tipo;
- CUIT del proveedor;
- razón social;
- situación fiscal;
- sucursal;
- número;
- monto gravado;
- impuestos;
- total.

Cardinalidad:

    Viatico 1 -------- 0..1 Comprobante

La relación se protege mediante una clave foránea y unicidad por Viático.

### Auditoría de Rendiciones

`dbo.Viaje` conserva:

- Usuario que envió a rendición;
- fecha de envío;
- Usuario que aprobó;
- fecha de aprobación;
- Usuario que canceló;
- fecha de cancelación;
- motivo de cancelación.

`dbo.Viatico` conserva:

- motivo de exclusión;
- Usuario y fecha de exclusión;
- Usuario y fecha de reactivación.

### Reconstrucción del agregado

`ViajeRepository.ObtenerPorId` reconstruye:

- datos generales;
- participantes;
- Visitas;
- Clientes;
- Viáticos;
- Personas pagadoras;
- Comprobantes;
- estados;
- datos de auditoría.

Esto permite calcular en Domain:

`TotalGastado = suma de Viáticos Vigentes`

`SaldoPendiente = TotalGastado - MontoAnticipado`

### Validación

Script:

`database/migrations/006_validar_viaticos_rendiciones.sql`

La validación comprueba:

- versión de migración;
- tablas y columnas;
- claves primarias;
- claves foráneas;
- índices;
- restricciones;
- tipos y estados válidos;
- importes no negativos;
- unicidad de Comprobante;
- integridad de auditoría;
- asignación de permisos.

Resultado:

`VALIDACIÓN CORRECTA`
