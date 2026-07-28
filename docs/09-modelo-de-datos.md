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

Application e Infrastructure impiden:

- autorreferencia funcional;
- ciclos indirectos;
- nuevas asociaciones con Grupos inactivos;
- reconstrucciones inconsistentes.

La interfaz permite administrar estas relaciones desde `GrupoEditForm`.

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

Las asignaciones de Grupos directos se agregan y reemplazan mediante operaciones transaccionales de `UsuarioGestionRepository`.

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

- Auditoría general consultable.

La alta y modificación persistente de los catálogos de Grupos y Permisos se encuentra implementada.

También se encuentran implementadas:

- gestión visual de `GrupoPermiso`;
- gestión visual de `GrupoGrupo`;
- reemplazo transaccional de jerarquías;
- detección de ciclos indirectos;
- vista previa de Permisos efectivos.

## 20. Commits técnicos

Persistencia SQL:

- `a4b631c` — `Agrego esquema SQL de seguridad`
- `2e567f2` — `Documento persistencia de seguridad`

Integración persistente:

- `cb52c7e` — `Implemento repositorio de autenticación`
- `514ba5a` — `Agrego inicialización del administrador`
- `a89c186` — `Agrego herramienta de configuración inicial`
- `62396ba` — `Pruebo jerarquías persistidas de seguridad`

## Gestión persistente de Usuarios

La gestión funcional reutiliza el esquema creado por:

`database/migrations/002_crear_seguridad.sql`

No fue necesaria una migración adicional.

### Alta de Usuario

`UsuarioGestionRepository.Insertar` persiste dentro de una única transacción:

1. comprobación de la Persona;
2. comprobación de disponibilidad de la Persona;
3. comprobación de unicidad del nombre;
4. comprobación de Grupos activos;
5. inserción en `dbo.Usuario`;
6. recuperación del identificador mediante `SCOPE_IDENTITY`;
7. inserción de una o varias filas en `dbo.UsuarioGrupo`;
8. commit.

Ante un error se ejecuta rollback.

### Modificación de Usuario

`UsuarioGestionRepository.Actualizar` ejecuta dentro de una única transacción:

1. comprobación de unicidad del nombre;
2. comprobación de Grupos activos;
3. actualización de `dbo.Usuario.NombreUsuario`;
4. eliminación de asignaciones anteriores en `dbo.UsuarioGrupo`;
5. inserción de la colección completa de Grupos directos;
6. commit.

El reemplazo completo evita diferencias entre:

- la colección del dominio;
- la selección de la interfaz;
- las filas persistidas.

### Activación y desactivación

El estado se modifica mediante:

`UPDATE dbo.Usuario SET Activo = @Activo`

No se realiza borrado físico.

### Concurrencia e integridad

Application realiza validaciones anticipadas.

SQL Server conserva la integridad definitiva mediante:

- `UX_Usuario_NombreUsuario`;
- `UX_Usuario_IdPersona`;
- `PK_UsuarioGrupo`;
- claves foráneas.

Infrastructure reconoce los errores:

- `2601`;
- `2627`.

Los conflictos se traducen a mensajes funcionales sin exponer detalles técnicos de SQL Server.

### Consulta de Personas disponibles

Una Persona está disponible para el alta cuando:

- se encuentra activa;
- no existe un Usuario asociado.

La consulta se implementa mediante `NOT EXISTS` sobre `dbo.Usuario`.

### Consulta del último Administrador

La protección administrativa comprueba la existencia de otro Usuario:

- activo;
- asignado directamente a un Grupo activo;
- cuyo código sea `ADMINISTRADOR_GENERAL`.

Esta consulta respalda la regla que impide eliminar el último acceso administrativo.

## Cambio persistente de clave

CUD11 reutiliza el esquema de seguridad existente.

No fue necesaria una migración nueva.

La actualización se realiza sobre:

`dbo.Usuario`

Columnas modificadas:

- `PasswordHash`;
- `PasswordSalt`;
- `IteracionesPassword`.

Columnas no modificadas:

- `IdUsuario`;
- `IdPersona`;
- `NombreUsuario`;
- `Activo`.

La sentencia exige:

- `IdUsuario` coincidente;
- `Activo = 1`.

El repositorio utiliza:

- consulta parametrizada;
- hash binario;
- salt binario;
- iteraciones enteras;
- control de filas afectadas.

Si no se actualiza exactamente una fila, Application considera que el Usuario ya no se encuentra disponible.

Las restricciones existentes garantizan:

- hash obligatorio;
- salt obligatorio;
- longitudes válidas;
- iteraciones mayores que cero.

El cambio no altera:

- `UsuarioGrupo`;
- `GrupoPermiso`;
- `GrupoGrupo`;
- Persona;
- permisos efectivos.

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

## Gestión persistente de Grupos

La gestión funcional reutiliza el esquema creado por:

`database/migrations/002_crear_seguridad.sql`

No fue necesaria una migración adicional.

### Alta de Grupo

`GrupoGestionRepository.Insertar` persiste dentro de una única transacción:

1. comprobación defensiva de unicidad del Código;
2. comprobación defensiva de unicidad del Nombre;
3. comprobación de existencia y actividad de Permisos;
4. inserción en `dbo.Grupo`;
5. recuperación del identificador mediante `SCOPE_IDENTITY`;
6. inserción de una o varias filas en `dbo.GrupoPermiso`;
7. commit.

Ante cualquier error se ejecuta rollback.

### Generación del Código

Application genera el Código desde el Nombre.

La normalización:

- elimina espacios exteriores;
- transforma caracteres a mayúsculas;
- elimina acentos;
- transforma espacios y separadores en guion bajo;
- evita separadores consecutivos inválidos.

La unicidad definitiva se conserva mediante:

`UX_Grupo_Codigo`

Después del alta el Código es inmutable.

### Modificación de Grupo

`GrupoGestionRepository.Actualizar` puede ejecutar dentro de una única transacción:

1. comprobación de existencia del Grupo;
2. comprobación de unicidad del Nombre;
3. comprobación de Permisos seleccionados;
4. comprobación de Grupos hijos seleccionados;
5. validación de actividad para nuevas asociaciones;
6. detección de autorreferencia;
7. detección de ciclos indirectos;
8. actualización de `Nombre`;
9. actualización de `Descripcion`;
10. eliminación de asociaciones anteriores en `dbo.GrupoPermiso`;
11. inserción de la nueva colección de Permisos directos;
12. eliminación de relaciones anteriores en `dbo.GrupoGrupo`;
13. inserción de la nueva colección de Grupos hijos;
14. commit.

Ante cualquier error se ejecuta rollback de:

- datos del Grupo;
- `GrupoPermiso`;
- `GrupoGrupo`.

No se actualiza:

- `Codigo`;
- `Activo`;
- `UsuarioGrupo`.

La sobrecarga anterior de actualización continúa disponible y preserva `GrupoGrupo`, evitando romper llamadas existentes.

### Reemplazo de GrupoPermiso

La relación:

`Grupo N -------- N Permiso`

se administra mediante reemplazo transaccional completo.

Este enfoque mantiene correspondencia entre:

- selección de la interfaz;
- colección del dominio;
- filas de `dbo.GrupoPermiso`.

La clave primaria compuesta:

`PK_GrupoPermiso`

impide asociaciones duplicadas.

### Reemplazo de GrupoGrupo

La relación:

`Grupo N -------- N Grupo`

se administra mediante reemplazo transaccional completo.

El Grupo que se modifica actúa como padre y los Grupos seleccionados actúan como hijos.

La dirección de herencia es:

> El Grupo padre incorpora los Permisos efectivos de sus Grupos hijos.

Se permite:

- cero hijos;
- un hijo;
- varios hijos;
- jerarquías de varios niveles;
- conservar un hijo inactivo ya relacionado.

Se impide:

- asignar el propio Grupo como hijo;
- asignar identificadores duplicados;
- agregar un hijo inactivo nuevo;
- crear ciclos indirectos.

La base garantiza:

- integridad referencial;
- ausencia de autorreferencia directa;
- ausencia de relaciones duplicadas.

Application y el repositorio validan los ciclos que una restricción relacional simple no puede impedir.

### Cálculo de Permisos efectivos

La vista previa combina:

- Permisos directos seleccionados;
- Permisos de los Grupos hijos;
- Permisos heredados en niveles posteriores.

La consulta recursiva:

- recorre `dbo.GrupoGrupo`;
- utiliza una ruta visitada para evitar recorridos repetidos;
- excluye Grupos inactivos;
- excluye Permisos inactivos;
- agrupa por Permiso;
- elimina duplicados;
- prioriza el origen directo cuando un Permiso también es heredado.

La vista previa no copia filas a `GrupoPermiso`. Es un cálculo informativo basado en la configuración vigente y la selección del formulario.

### Activación y desactivación

El estado se modifica mediante:

`UPDATE dbo.Grupo SET Activo = @Activo`

No se realiza borrado físico.

La desactivación conserva:

- `UsuarioGrupo`;
- `GrupoPermiso`;
- `GrupoGrupo`.

Un Grupo inactivo deja de intervenir en los Permisos efectivos, pero sus asociaciones quedan disponibles para una reactivación posterior.

### Protección de ADMINISTRADOR_GENERAL

Application impide:

- desactivar `ADMINISTRADOR_GENERAL`;
- modificar su Código;
- retirar `GRUPO_GESTIONAR`;
- retirar `USUARIO_GESTIONAR`;
- retirar `PERMISO_GESTIONAR`.

La protección es funcional.

Las restricciones SQL continúan garantizando:

- integridad referencial;
- unicidad de Código;
- asociaciones no duplicadas.

### Concurrencia e integridad

Las escrituras utilizan:

`IsolationLevel.Serializable`

Application realiza validaciones anticipadas.

SQL Server conserva la integridad definitiva mediante:

- `UX_Grupo_Codigo`;
- `PK_GrupoPermiso`;
- claves foráneas;
- restricciones de campos no vacíos.

Infrastructure reconoce los errores:

- `2601`;
- `2627`.

Los conflictos se traducen a excepciones funcionales sin exponer detalles internos de SQL Server.

### Consultas del módulo

El repositorio permite:

- listar con búsqueda;
- filtrar por estado;
- contar Permisos directos;
- contar Usuarios directos;
- obtener detalle;
- reconstruir Permisos;
- reconstruir Grupos hijos;
- listar Grupos hijos disponibles;
- calcular Permisos efectivos directos y heredados;
- listar Permisos activos;
- obtener Permisos por identificadores;
- verificar Código;
- verificar Nombre;
- verificar actividad de Permisos.

El listado utiliza consultas agregadas y evita el problema N+1.

### Validación

Se ejecutaron:

- pruebas de Domain;
- pruebas de Application;
- 19 pruebas de integración real con SQL Server;
- validación manual del ejecutable.

Resultado consolidado:

- 682 pruebas totales;
- 627 correctas;
- 0 fallidas;
- compilación con 0 advertencias;
- compilación con 0 errores.

### Commits

- `fcbc5e2` — `Agrego casos de uso de grupos`
- `5da1e81` — `Agrego persistencia de grupos`
- `90ec085` — `Completo interfaz de gestion de grupos`

## Gestión persistente del catálogo de Permisos

La gestión funcional reutiliza el esquema creado por:

`database/migrations/002_crear_seguridad.sql`

No fue necesaria una migración adicional.

### Alta de Permiso

`PermisoGestionRepository.Insertar` ejecuta dentro de una transacción:

1. comprobación defensiva de unicidad del Código;
2. inserción en `dbo.Permiso`;
3. recuperación del identificador mediante `SCOPE_IDENTITY`;
4. commit.

La transacción utiliza:

`IsolationLevel.Serializable`

Ante cualquier error se ejecuta rollback.

### Código del Permiso

El Código:

- se normaliza en Application;
- se almacena en `dbo.Permiso.Codigo`;
- tiene una longitud máxima de 100 caracteres;
- es único mediante `UX_Permiso_Codigo`;
- no se actualiza después del alta.

Infrastructure reconoce los errores SQL:

- 2601;
- 2627.

Los conflictos se traducen a errores funcionales.

### Modificación

`PermisoGestionRepository.Actualizar` modifica únicamente:

- `Nombre`;
- `Descripcion`.

No modifica:

- `Codigo`;
- `Activo`;
- `GrupoPermiso`.

La Descripción vacía se persiste como `NULL`.

### Activación y desactivación

El estado se modifica mediante:

`UPDATE dbo.Permiso SET Activo = @Activo`

No existe borrado físico desde el módulo.

La operación conserva:

- la fila de `dbo.Permiso`;
- las filas relacionadas de `dbo.GrupoPermiso`.

### Preservación de asociaciones

La desactivación de un Permiso no elimina sus asociaciones con Grupos.

Un Permiso inactivo deja de aportar autorización efectiva, pero puede reactivarse sin reconstruir su configuración.

Al editar un Grupo:

- se listan todos los Permisos activos;
- se incluyen los Permisos inactivos ya seleccionados;
- se excluyen los Permisos inactivos no seleccionados.

### Consultas

El repositorio permite:

- listar con búsqueda general;
- filtrar por estado;
- contar Grupos asociados;
- obtener detalle;
- reconstruir la entidad;
- comprobar existencia de Código.

El listado utiliza una consulta agregada sobre `GrupoPermiso` y evita consultas N+1.

### Integridad

SQL Server garantiza:

- clave primaria `PK_Permiso`;
- unicidad mediante `UX_Permiso_Codigo`;
- campos obligatorios mediante CHECK;
- estado predeterminado activo;
- integridad referencial con `GrupoPermiso`;
- asociaciones no duplicadas mediante `PK_GrupoPermiso`.

### Validación

Se ejecutaron:

- 19 pruebas de integración específicas del repositorio;
- 3 pruebas de preservación de Permisos inactivos en Grupos;
- regresión completa de 594 pruebas;
- validación manual;
- comprobación de ausencia de datos temporales.

### Commits

- `3e8401c` — `Agrego casos de uso de permisos`
- `d61e304` — `Agrego persistencia de permisos`
- `2c16ef2` — `Completo interfaz de gestion de permisos`

## Auditoría general consultable

### Migración

`database/migrations/007_crear_auditoria.sql`

La migración:

- crea `dbo.Auditoria`;
- registra la versión `007`;
- se ejecuta dentro de una transacción;
- exige la existencia previa de `dbo.Usuario`;
- puede reejecutarse sin duplicar el esquema.

### Tabla Auditoria

Nombre:

`dbo.Auditoria`

Columnas principales:

| Columna | Tipo | Nulo | Finalidad |
|---|---|---:|---|
| IdAuditoria | BIGINT IDENTITY | No | Clave primaria |
| FechaHora | DATETIME2(0) | No | Momento del evento |
| IdUsuario | INT | No | Usuario actor |
| NombreUsuario | NVARCHAR(100) | No | Identidad conservada |
| Modulo | NVARCHAR(50) | No | Módulo funcional |
| Accion | NVARCHAR(50) | No | Operación ejecutada |
| Entidad | NVARCHAR(100) | No | Tipo de entidad afectada |
| IdEntidad | INT | Sí | Identificador funcional |
| Descripcion | NVARCHAR(1000) | No | Descripción controlada |

Restricciones principales:

- `PK_Auditoria`;
- `FK_Auditoria_Usuario`;
- campos textuales no vacíos;
- `IdEntidad` nulo o mayor que cero.

Índices:

- `IX_Auditoria_FechaHora`;
- `IX_Auditoria_IdUsuario_FechaHora`;
- `IX_Auditoria_Modulo_Accion_FechaHora`.

### Atomicidad

Las escrituras funcionales auditadas comparten:

- una `SqlConnection`;
- una `SqlTransaction`;
- la actualización de negocio;
- la inserción en `dbo.Auditoria`;
- un único commit.

Ante cualquier error se revierte la operación completa.

La atomicidad se comprobó mediante pruebas que provocan el fallo de la clave
foránea `FK_Auditoria_Usuario` utilizando un actor inexistente.

### Cobertura funcional

La tabla registra cambios exitosos de:

- Clientes;
- Usuarios;
- Grupos;
- Permisos;
- Viajes;
- Visitas;
- Viáticos;
- Rendiciones.

En Rendiciones se registran:

- envío a rendición;
- exclusión de Viático;
- reactivación de Viático;
- ajuste del anticipo;
- aprobación;
- cancelación.

### Alcance histórico y datos excluidos

Los eventos comienzan a registrarse desde la aplicación de la migración `007`.

No se realizó backfill de operaciones anteriores.

No se registran:

- consultas;
- contraseñas;
- hashes;
- salts;
- credenciales temporales;
- cadenas de conexión;
- contenido binario de Comprobantes;
- excepciones completas.
