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

- repositorios ADO.NET;
- procedimientos almacenados;
- auditoría;
- persistencia de viajes;
- persistencia de clientes;
- persistencia de visitas;
- persistencia de viáticos;
- usuario administrador inicial.

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
