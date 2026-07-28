# Pruebas de gestión de Grupos

## 1. Objetivo

Este documento registra la implementación, validación técnica y validación funcional del módulo de gestión de Grupos de SIGEVIP.

El módulo cubre:

- CUD07: gestionar Grupos;

- CUD08: agregar Grupo;

- CUD09: modificar Grupo;

- CUD10: eliminar Grupo mediante desactivación lógica;

- RF37: impedir accesos no autorizados;

- RF38: ocultar opciones no habilitadas;

- RF39: autorizar mediante Grupos y Permisos.

La expresión documental “Acción del sistema” se implementa mediante la entidad `Permiso`.

No se creó una entidad paralela denominada Acción.

## 2. Alcance implementado

Se implementaron:

- listado de Grupos;

- búsqueda por Código, Nombre o Descripción;

- filtro por estado;

- consulta de detalle;

- alta de Grupo;

- generación automática del Código;

- modificación de Nombre;

- modificación de Descripción;

- asignación de uno o varios Permisos directos;

- reemplazo completo de Permisos directos;

- activación lógica;

- desactivación lógica;

- conservación de asociaciones;

- protección de `ADMINISTRADOR\_GENERAL`;

- navegación desde el menú principal;

- ocultamiento del módulo sin `GRUPO\_GESTIONAR`;

- autorización dentro de Application;

- persistencia ADO.NET;

- transacciones;

- pruebas unitarias;

- pruebas de integración real con SQL Server;

- validación manual reproducible.

## 3. Alcance excluido

No se implementaron en este bloque:

- alta de Permisos;

- modificación de Permisos;

- baja de Permisos;

- gestión visual del catálogo de Permisos;

- asignación directa de Permisos a Usuarios;

- gestión visual de relaciones `GrupoGrupo`;

- edición visual de jerarquías entre Grupos;

- auditoría general consultable;

- recuperación de contraseña.

La gestión de Grupos de este bloque trabajaba con Permisos ya existentes y activos.

Posteriormente se implementó el mantenimiento funcional del catálogo de Permisos, documentado en:

`docs/20-pruebas-gestion-permisos.md`

## 4. Reglas funcionales comprobadas

### Código

El Código:

- es obligatorio;

- se normaliza;

- se genera automáticamente desde el Nombre;

- utiliza mayúsculas;

- elimina acentos;

- transforma espacios y separadores en guion bajo;

- debe ser único;

- permanece estable después del alta;

- no puede modificarse durante la edición.

Ejemplo:

`Grupo demostración manual`

genera:

`GRUPO\_DEMOSTRACION\_MANUAL`

### Nombre

El Nombre:

- es obligatorio;

- se elimina espacio exterior;

- debe ser único según la comparación vigente;

- puede modificarse.

### Descripción

La Descripción:

- es obligatoria;

- se elimina espacio exterior;

- puede modificarse.

### Permisos directos

Cada Grupo debe tener:

- al menos un Permiso directo;

- únicamente Permisos existentes;

- únicamente Permisos activos;

- ausencia de duplicados.

La modificación reemplaza la totalidad de los Permisos directos.

El reemplazo no elimina ni modifica relaciones jerárquicas `GrupoGrupo`.

### Estado

El Grupo se crea activo.

La eliminación funcional se implementa mediante desactivación lógica.

La desactivación:

- no elimina físicamente el Grupo;

- conserva `GrupoPermiso`;

- conserva `UsuarioGrupo`;

- conserva `GrupoGrupo`;

- impide que el Grupo otorgue Permisos efectivos;

- permite reactivación posterior.

Los cambios de autorización se aplican al iniciar una nueva sesión.

### Protección administrativa

El Grupo:

`ADMINISTRADOR\_GENERAL`

no puede:

- desactivarse;

- cambiar su Código;

- quedar sin `GRUPO\_GESTIONAR`;

- quedar sin `USUARIO\_GESTIONAR`;

- quedar sin `PERMISO\_GESTIONAR`.

Estas protecciones se validan en Application y no dependen únicamente de la interfaz.

## 5. Implementación por capa

### Domain

Archivo principal:

`src/SIGEVIP.Domain/Entities/Grupo.cs`

Se incorporaron:

- `ActualizarDatos`;

- `ReemplazarPermisosDirectos`;

- validación de Nombre;

- validación de Descripción;

- validación previa de la colección completa;

- rechazo de Permisos nulos;

- rechazo de Permisos duplicados;

- rechazo de colección vacía;

- conservación de Grupos hijos;

- conservación del estado anterior ante datos inválidos.

### Application

Carpeta:

`src/SIGEVIP.Application/Grupos`

Componentes:

- `GrupoFiltro`;

- `GrupoListadoDto`;

- `GrupoDetalleDto`;

- `PermisoSeleccionGrupoDto`;

- `RegistrarGrupoCommand`;

- `ModificarGrupoCommand`;

- `IGrupoGestionRepository`;

- `GrupoGestionService`.

Casos de uso:

- listar;

- obtener detalle;

- listar Permisos activos;

- registrar;

- modificar;

- activar;

- desactivar.

`GrupoGestionService` valida:

- sesión autenticada;

- Usuario activo;

- permiso `GRUPO\_GESTIONAR`;

- identificadores;

- existencia del Grupo;

- unicidad del Código;

- unicidad del Nombre;

- existencia de Permisos;

- actividad de Permisos;

- selección no vacía;

- ausencia de duplicados;

- protección de `ADMINISTRADOR\_GENERAL`.

### Infrastructure

Archivo:

`src/SIGEVIP.Infrastructure/Grupos/GrupoGestionRepository.cs`

La implementación utiliza:

- ADO.NET;

- `System.Data.SqlClient`;

- consultas parametrizadas;

- selección explícita de columnas;

- transacciones SQL;

- `IsolationLevel.Serializable`;

- `SCOPE\_IDENTITY`;

- traducción de errores SQL `2601` y `2627`;

- `PersistenciaException`;

- validaciones defensivas de concurrencia.

Responsabilidades:

- listar con filtros;

- obtener detalle;

- reconstruir Permisos directos;

- reconstruir Grupos hijos directos;

- listar Permisos activos;

- obtener Permisos por identificadores;

- verificar existencia de Código;

- verificar existencia de Nombre;

- verificar actividad de Permisos;

- insertar Grupo y `GrupoPermiso`;

- actualizar datos y reemplazar `GrupoPermiso`;

- preservar `GrupoGrupo`;

- activar;

- desactivar.

No fue necesaria una migración nueva.

Se reutilizan:

- `dbo.Grupo`;

- `dbo.Permiso`;

- `dbo.GrupoPermiso`;

- `dbo.GrupoGrupo`;

- `dbo.UsuarioGrupo`.

### WinForms

Formularios:

- `src/SIGEVIP.WinForms/Forms/GruposForm.cs`;

- `src/SIGEVIP.WinForms/Forms/GrupoEditForm.cs`.

Integración:

- `MainForm`;

- `SigevipApplicationContext`;

- `Program`;

- `SIGEVIP.WinForms.csproj`.

La pantalla permite:

- listar;

- buscar;

- filtrar;

- registrar;

- modificar;

- activar;

- desactivar;

- visualizar Código;

- visualizar Nombre;

- visualizar Descripción;

- visualizar cantidad de Permisos;

- visualizar cantidad de Usuarios;

- seleccionar varios Permisos;

- mostrar errores funcionales y técnicos controlados.

## 6. Persistencia transaccional

### Alta

La creación ejecuta dentro de una transacción:

1\. comprobación de unicidad del Código;

2\. comprobación de unicidad del Nombre;

3\. comprobación de Permisos activos;

4\. inserción en `dbo.Grupo`;

5\. recuperación del identificador;

6\. inserción en `dbo.GrupoPermiso`;

7\. commit.

Ante un error se ejecuta rollback.

### Modificación

La modificación ejecuta dentro de una transacción:

1\. comprobación de existencia del Grupo;

2\. comprobación de unicidad del Nombre;

3\. comprobación de Permisos activos;

4\. actualización de Nombre y Descripción;

5\. eliminación de Permisos directos anteriores;

6\. inserción de la nueva colección de `GrupoPermiso`;

7\. commit.

No modifica:

- Código;

- `UsuarioGrupo`;

- `GrupoGrupo`.

### Activación y desactivación

El estado se modifica mediante:

`UPDATE dbo.Grupo SET Activo = @Activo`

No se realiza borrado físico.

## 7. Pruebas automatizadas

### Domain

Archivo:

`tests/SIGEVIP.Tests/Domain/GrupoGestionDomainTests.cs`

Cantidad:

9 pruebas.

Cobertura:

- actualización válida;

- Nombre obligatorio;

- Descripción obligatoria;

- normalización;

- reemplazo de Permisos;

- rechazo de colección vacía;

- rechazo de Permiso nulo;

- rechazo de Permisos duplicados;

- preservación de Grupos hijos.

### Application

Archivo:

`tests/SIGEVIP.Tests/Application/GrupoGestionServiceTests.cs`

Cantidad:

25 pruebas.

Cobertura:

- sesión inexistente;

- Usuario inactivo;

- permiso faltante;

- listado autorizado;

- detalle;

- identificador inválido;

- Grupo inexistente;

- Nombre obligatorio;

- Descripción obligatoria;

- generación de Código;

- Código duplicado;

- Nombre duplicado;

- colección de Permisos vacía;

- Permiso inexistente;

- Permiso inactivo;

- Permisos duplicados;

- alta válida;

- modificación válida;

- Código inmutable;

- activación;

- desactivación;

- protección de `ADMINISTRADOR\_GENERAL`;

- protección de Permisos mínimos administrativos.

### Integración SQL

Archivo:

`tests/SIGEVIP.Tests/Integration/GrupoGestionRepositoryIntegrationTests.cs`

Cantidad:

19 pruebas.

Cobertura:

- listado;

- cantidades de Permisos y Usuarios;

- búsqueda;

- filtro por estado;

- detalle;

- Grupo inexistente;

- reconstrucción de Permisos directos;

- reconstrucción de Grupo hijo directo;

- listado de Permisos activos;

- indicador de Permisos seleccionados;

- obtención de Permisos por identificadores;

- existencia de Código;

- existencia de Nombre;

- actividad de Permisos;

- alta transaccional;

- rechazo de Código duplicado;

- rollback ante Permiso inactivo;

- modificación transaccional;

- reemplazo de Permisos;

- preservación de `GrupoGrupo`;

- activación;

- desactivación;

- actualización de Grupo inexistente.

## 8. Resultado automatizado

Pruebas específicas nuevas:

- Domain: 9;

- Application: 25;

- integración SQL: 19;

- total específico: 53.

Regresión completa:

- 532 pruebas totales;

- 532 correctas;

- 0 fallidas.

Compilación:

- 0 advertencias;

- 0 errores.

Herramientas:

- MSBuild de Visual Studio 2022;

- VSTest 17.13;

- MSTest;

- SQL Server real para integración.

## 9. Validación manual

Se comprobó:

- acceso desde el menú principal;

- visibilidad mediante `GRUPO\_GESTIONAR`;

- carga del listado;

- búsqueda por Código;

- búsqueda por Nombre;

- filtro por estado;

- limpieza de filtros;

- alta de Grupo;

- generación automática del Código;

- Código de solo lectura;

- selección múltiple de Permisos;

- persistencia del alta;

- modificación de Nombre;

- modificación de Descripción;

- reemplazo de Permisos;

- conservación del Código;

- conservación de asociaciones;

- desactivación lógica;

- permanencia del Grupo en el listado;

- conservación de Permisos después de desactivar;

- filtro de Grupos inactivos;

- reactivación;

- bloqueo de desactivación de `ADMINISTRADOR\_GENERAL`;

- bloqueo de eliminación de Permisos administrativos mínimos;

- validación de campos obligatorios;

- rechazo de selección vacía;

- ausencia de problemas visuales.

Todos los controles manuales fueron aprobados.

## 10. Datos temporales de validación

Durante la validación manual se creó un Grupo temporal.

Se verificó:

- generación del Código;

- estado inicial activo;

- asociación con Permisos;

- modificación;

- reemplazo de Permisos;

- desactivación;

- reactivación.

El registro se utilizó únicamente para comprobar el comportamiento funcional del módulo.

## 11. Commits del módulo

- `fcbc5e2` — `Agrego casos de uso de grupos`

- `5da1e81` — `Agrego persistencia de grupos`

- `90ec085` — `Completo interfaz de gestion de grupos`

## 12. Estado final

El módulo de Gestión de Grupos se considera implementado dentro del alcance académico aprobado porque cuenta con:

- reglas de dominio;

- casos de uso;

- autorización;

- persistencia;

- transacciones;

- interfaz;

- navegación;

- validaciones;

- pruebas unitarias;

- pruebas de integración SQL;

- validación manual reproducible.

Después de este cierre se implementó:

- gestión funcional del catálogo de Permisos.

Permanecen como módulos posteriores:

- gestión visual de jerarquías `GrupoGrupo`;

- recuperación de contraseña;

- auditoría general consultable.
