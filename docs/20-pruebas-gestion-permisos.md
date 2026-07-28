# Gestión funcional del catálogo de Permisos



## 1. Identificación del módulo



El sistema incorpora la gestión funcional del catálogo de Permisos de seguridad.



La documentación académica disponible no define un CUD numerado específico para este mantenimiento.



Por ese motivo:



- no se inventa una numeración de caso de uso;

- el módulo se registra como una extensión funcional del subsistema de seguridad;

- su incorporación se fundamenta en la entidad `Permiso`;

- se fundamenta en la tabla `dbo.Permiso`;

- se fundamenta en el permiso administrativo `PERMISO\_GESTIONAR`;

- se fundamenta en la necesidad de administrar los Permisos asignables a los Grupos;

- se mantiene la trazabilidad con RF37, RF38 y RF39.



## 2. Requisitos relacionados



El módulo se vincula con:



- RF37: impedir accesos no autorizados;

- RF38: ocultar opciones no habilitadas;

- RF39: acceso mediante Grupos y Permisos;

- mantenimiento del modelo de seguridad;

- administración de la relación `GrupoPermiso`.



El acceso al módulo requiere:



`PERMISO\_GESTIONAR`



La autorización se valida:



- en el menú principal;

- dentro de la pantalla de Permisos;

- nuevamente en `PermisoGestionService`.



## 3. Alcance implementado



El módulo permite:



- listar Permisos;

- buscar por Código;

- buscar por Nombre;

- buscar por Descripción;

- filtrar por estado;

- consultar detalle;

- registrar Permisos;

- modificar Nombre;

- modificar Descripción;

- activar Permisos;

- desactivar Permisos;

- visualizar la cantidad de Grupos asociados;

- conservar asociaciones históricas;

- proteger Permisos administrativos críticos.



No permite:



- modificar el Código después del alta;

- eliminar físicamente Permisos;

- desactivar Permisos administrativos críticos;

- asignar Permisos directamente a Usuarios;

- editar jerarquías `GrupoGrupo`;

- administrar auditoría general.



## 4. Entidad Permiso



Archivo:



`src/SIGEVIP.Domain/Entities/Permiso.cs`



Propiedades principales:



- `IdPermiso`;

- `Codigo`;

- `Nombre`;

- `Descripcion`;

- `Activo`.



Reglas:



- el Código es obligatorio;

- el Código se normaliza;

- el Nombre es obligatorio;

- la Descripción es opcional;

- el Permiso nace activo;

- el Código permanece inmutable;

- la modificación no cambia el identificador;

- la modificación no cambia el estado;

- un Permiso inactivo no aporta autorización efectiva.



Método agregado:



`ActualizarDatos`



Este método permite modificar:



- Nombre;

- Descripción.



No modifica:



- IdPermiso;

- Código;

- Activo.



## 5. Normalización del Código



La normalización se implementa en:



`PermisoGestionService.NormalizarCodigo`



La operación:



- elimina espacios exteriores;

- transforma letras a mayúsculas;

- elimina acentos;

- transforma separadores en guion bajo;

- evita guiones bajos consecutivos;

- rechaza códigos sin caracteres válidos;

- respeta la longitud máxima de 100 caracteres.



Ejemplo:



`gestión / clientes - región norte`



se transforma en:



`GESTION\_CLIENTES\_REGION\_NORTE`



## 6. Reglas de datos



### Código



El Código:



- es obligatorio;

- es único;

- se normaliza;

- tiene una longitud máxima de 100 caracteres;

- se define durante el alta;

- es inmutable después del alta.



La unicidad funcional se valida en Application.



La unicidad definitiva se garantiza mediante:



`UX\_Permiso\_Codigo`



### Nombre



El Nombre:



- es obligatorio;

- se normaliza mediante `Trim`;

- tiene una longitud máxima de 150 caracteres;

- puede modificarse.



No se estableció unicidad por Nombre.



### Descripción



La Descripción:



- es opcional;

- tiene una longitud máxima de 500 caracteres;

- puede modificarse;

- se representa como texto vacío en Domain;

- se persiste como `NULL` cuando está vacía.



### Estado



El Permiso nace activo.



La baja es lógica.



No se realiza borrado físico desde el sistema.



## 7. Permisos administrativos críticos



No pueden desactivarse:



- `USUARIO\_GESTIONAR`;

- `GRUPO\_GESTIONAR`;

- `PERMISO\_GESTIONAR`.



La protección se aplica en:



`PermisoGestionService.Desactivar`



El objetivo es impedir que el catálogo de seguridad quede sin capacidades administrativas básicas.



## 8. Application



Carpeta:



`src/SIGEVIP.Application/Permisos`



Componentes:



- `PermisoFiltro`;

- `PermisoListadoDto`;

- `PermisoDetalleDto`;

- `RegistrarPermisoCommand`;

- `ModificarPermisoCommand`;

- `IPermisoGestionRepository`;

- `PermisoGestionService`.



Casos de uso disponibles:



- `Listar`;

- `Obtener`;

- `Registrar`;

- `Modificar`;

- `Activar`;

- `Desactivar`.



Cada operación valida:



- sesión autenticada;

- Usuario activo;

- permiso `PERMISO\_GESTIONAR`.



## 9. Persistencia ADO.NET



Implementación:



`src/SIGEVIP.Infrastructure/Permisos/PermisoGestionRepository.cs`



El repositorio implementa:



- `Listar`;

- `ObtenerDetallePorId`;

- `ObtenerPorId`;

- `ExisteCodigo`;

- `Insertar`;

- `Actualizar`;

- `Activar`;

- `Desactivar`.



No fue necesaria una migración nueva.



El módulo reutiliza:



- `dbo.Permiso`;

- `dbo.GrupoPermiso`;

- `PK\_Permiso`;

- `UX\_Permiso\_Codigo`;

- restricciones CHECK;

- claves foráneas;

- índice inverso de `GrupoPermiso`.



## 10. Alta persistente



El alta utiliza una transacción con:



`IsolationLevel.Serializable`



La operación:



1. valida defensivamente la unicidad del Código;

2. inserta el Permiso;

3. recupera el identificador mediante `SCOPE\_IDENTITY`;

4. confirma la transacción.



Ante cualquier error se ejecuta rollback.



SQL Server conserva la unicidad definitiva mediante:



`UX\_Permiso\_Codigo`



Los errores SQL:



- 2601;

- 2627;



se traducen a una excepción funcional sin exponer detalles internos.



## 11. Modificación persistente



La modificación actualiza únicamente:



- `Nombre`;

- `Descripcion`.



No actualiza:



- `Codigo`;

- `Activo`;

- filas de `GrupoPermiso`.



La operación utiliza una transacción con:



`IsolationLevel.Serializable`



La Descripción vacía se persiste como:



`DBNull.Value`



## 12. Activación y desactivación



El estado se modifica mediante:



`UPDATE dbo.Permiso SET Activo = @Activo`



No se eliminan filas.



La desactivación conserva:



- el Permiso;

- su Código;

- Nombre;

- Descripción;

- asociaciones `GrupoPermiso`.



Un Permiso inactivo:



- no aporta autorización efectiva;

- puede reactivarse;

- conserva su trazabilidad con los Grupos.



## 13. Cantidad de Grupos asociados



El listado y el detalle calculan la cantidad de Grupos mediante consultas agregadas sobre:



`dbo.GrupoPermiso`



Esto permite mostrar:



- Código;

- Nombre;

- Descripción;

- cantidad de Grupos;

- Estado.



La consulta evita realizar una consulta adicional por cada Permiso.



## 14. Interfaz WinForms



Se implementaron:



- `PermisosForm`;

- `PermisoEditForm`.



También se modificaron:



- `MainForm`;

- `SigevipApplicationContext`;

- `Program`;

- `SIGEVIP.WinForms.csproj`.



### PermisosForm



Permite:



- listar;

- buscar;

- filtrar por estado;

- registrar;

- modificar;

- activar;

- desactivar;

- visualizar cantidad de Grupos;

- mostrar errores controlados.



### PermisoEditForm



Permite:



- ingresar Código durante el alta;

- visualizar el Código normalizado;

- modificar Nombre;

- modificar Descripción;

- consultar Estado;

- consultar cantidad de Grupos.



Durante la modificación:



- el Código es de solo lectura;

- el Código no se envía para actualizar;

- Estado y cantidad de Grupos son informativos.



## 15. Navegación



`MainForm` expone el evento:



`PermisosSolicitados`



`SigevipApplicationContext`:



- escucha el evento;

- crea `PermisosForm`;

- muestra el formulario modal;

- conserva el menú principal como formulario propietario.



`Program` construye:



- `PermisoGestionRepository`;

- `PermisoGestionService`;

- dependencias de autorización y sesión.



## 16. Corrección transversal en Gestión de Grupos



Durante la implementación se detectó un riesgo funcional.



Anteriormente:



`GrupoGestionRepository.ListarPermisosActivos`



devolvía únicamente Permisos activos.



Esto provocaba que, al editar un Grupo que tuviera un Permiso posteriormente desactivado:



- el Permiso no apareciera en la lista;

- la asociación pudiera perderse al guardar el Grupo.



La consulta fue corregida para devolver:



- todos los Permisos activos;

- los Permisos inactivos que ya están seleccionados;

- ningún otro Permiso inactivo.



Resultado:



- un Permiso inactivo ya asociado sigue visible;

- sigue seleccionado;

- puede conservarse al guardar;

- no se ofrece para Grupos nuevos;

- no se permite asignar un Permiso inactivo nuevo.



## 17. Conservación de GrupoPermiso



La modificación de un Permiso no elimina asociaciones.



La activación y desactivación tampoco eliminan asociaciones.



La relación:



`Grupo N -------- N Permiso`



permanece persistida en:



`dbo.GrupoPermiso`



Esta decisión permite:



- reactivar Permisos;

- conservar configuraciones;

- mantener trazabilidad;

- evitar pérdida silenciosa de asignaciones.



## 18. Pruebas de Domain



Archivo:



`tests/SIGEVIP.Tests/Domain/PermisoGestionDomainTests.cs`



Cantidad:



- 10 pruebas.



Se verifica:



- actualización válida;

- Nombre obligatorio;

- Descripción opcional;

- normalización de texto;

- Código inmutable;

- identificador inmutable;

- estado preservado;

- conservación de datos ante error;

- activación;

- desactivación.



## 19. Pruebas de Application



Archivo:



`tests/SIGEVIP.Tests/Application/PermisoGestionServiceTests.cs`



Cantidad:



- 30 pruebas.



Se verifica:



- sesión requerida;

- Usuario activo;

- permiso `PERMISO\_GESTIONAR`;

- listado;

- detalle;

- identificadores válidos;

- alta;

- normalización de Código;

- Código duplicado;

- Nombre obligatorio;

- Descripción opcional;

- modificación;

- Código inmutable;

- activación;

- desactivación;

- protección de `USUARIO\_GESTIONAR`;

- protección de `GRUPO\_GESTIONAR`;

- protección de `PERMISO\_GESTIONAR`.



## 20. Pruebas de integración SQL



Archivo:



`tests/SIGEVIP.Tests/Integration/PermisoGestionRepositoryIntegrationTests.cs`



Cantidad:



- 19 pruebas.



Se verifica:



- listado;

- búsqueda por Código;

- búsqueda por Nombre;

- búsqueda por Descripción;

- filtro activo;

- filtro inactivo;

- cantidad de Grupos;

- detalle;

- inexistencia;

- reconstrucción activa;

- reconstrucción inactiva;

- existencia de Código;

- alta;

- Descripción `NULL`;

- Código duplicado;

- modificación;

- preservación del Código;

- preservación del estado;

- preservación de `GrupoPermiso`;

- activación y desactivación;

- tratamiento de registros inexistentes.



## 21. Pruebas de preservación en Grupos



Se agregaron tres pruebas a:



`GrupoGestionRepositoryIntegrationTests`



Se verifica:



- inclusión de Permiso inactivo seleccionado;

- exclusión de Permiso inactivo no seleccionado;

- coexistencia de Permisos activos con un Permiso inactivo seleccionado.



## 22. Resultado automatizado



Resultado consolidado:



- 594 pruebas totales;

- 594 correctas;

- 0 fallidas;

- 0 advertencias de compilación;

- 0 errores de compilación.



Las pruebas temporales no dejaron registros remanentes en:



- `dbo.Permiso`;

- `dbo.Grupo`.



## 23. Validación manual



Se verificó:



- acceso desde el menú;

- ocultamiento sin autorización;

- listado;

- búsqueda;

- filtros;

- alta;

- normalización del Código;

- rechazo de Código duplicado;

- modificación;

- Código inmutable;

- Descripción opcional;

- desactivación;

- reactivación;

- protección de Permisos críticos;

- conservación de asociaciones;

- visualización de Permiso inactivo asignado al editar un Grupo;

- exclusión del Permiso inactivo al crear un Grupo nuevo;

- mensajes controlados;

- limpieza de datos temporales.



Resultado:



`VALIDACIÓN MANUAL APROBADA`



## 24. Commits del módulo



- `3e8401c` — `Agrego casos de uso de permisos`

- `d61e304` — `Agrego persistencia de permisos`

- `2c16ef2` — `Completo interfaz de gestion de permisos`



## 25. Trazabilidad técnica



| Elemento | Implementación | Persistencia | Interfaz | Validación |

|---|---|---|---|---|

| Gestión del catálogo de Permisos | `PermisoGestionService` | `PermisoGestionRepository` | `PermisosForm` | Unitarias, integración y manual |

| Alta | `RegistrarPermisoCommand` | `INSERT dbo.Permiso` | `PermisoEditForm` | Código, Nombre, Descripción y estado |

| Modificación | `ModificarPermisoCommand` | `UPDATE Nombre, Descripcion` | `PermisoEditForm` | Código inmutable |

| Activación | `PermisoGestionService.Activar` | `UPDATE Activo = 1` | `PermisosForm` | Integración y manual |

| Desactivación | `PermisoGestionService.Desactivar` | `UPDATE Activo = 0` | `PermisosForm` | Protecciones críticas |

| Autorización | `PERMISO\_GESTIONAR` | Catálogo de seguridad | Botón visible según permiso | Application y manual |

| Relación con Grupos | Reglas de preservación | `GrupoPermiso` | Selector de Grupos | Integración SQL |

| Código único | Application | `UX\_Permiso\_Codigo` | Validación controlada | Unitarias e integración |



## 26. Estado final



El módulo se considera cerrado porque incluye:



- Domain;

- Application;

- Infrastructure;

- WinForms;

- autorización;

- validación de datos;

- manejo de errores;

- persistencia reproducible;

- pruebas unitarias;

- pruebas de integración SQL;

- validación manual;

- commits publicados;

- working tree limpio.



## 27. Alcance pendiente



Continúan pendientes:



- gestión visual de jerarquías `GrupoGrupo`;

- recuperación de contraseña;

- auditoría general consultable;

- historial funcional completo del Cliente;

- reportes;

- mapa y geolocalización;

- datos finales de demostración;

- manual técnico;

- preparación de la presentación académica.
