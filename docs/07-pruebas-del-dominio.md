# Pruebas de SIGEVIP

## 1. Herramientas

- Framework: MSTest.
- MSTest.TestFramework: 3.7.3.
- MSTest.TestAdapter: 3.7.3.
- Microsoft.NET.Test.Sdk: 17.13.0.
- Ejecutor: VSTest 17.13.0 x64.
- Plataforma: .NET Framework 4.8.

## 2. Proyectos probados

- `SIGEVIP.Domain`
- `SIGEVIP.Application`
- `SIGEVIP.Infrastructure`

## 3. Archivos principales

### Arquitectura

- `tests/SIGEVIP.Tests/ArchitectureTests.cs`

### Dominio

- `Domain/ClienteTests.cs`
- `Domain/GrupoPermisoTests.cs`
- `Domain/GrupoGestionDomainTests.cs`
- `Domain/PermisoGestionDomainTests.cs`
- `Domain/PersonaTests.cs`
- `Domain/UsuarioTests.cs`
- `Domain/ViajeTests.cs`
- `Domain/ViajeParticipanteTests.cs`
- `Domain/ViajeVisitaTests.cs`
- `Domain/ViaticoTests.cs`
- `Domain/VisitaTests.cs`

### Application

- `Application/AutenticacionServiceTests.cs`
- `Application/AutorizacionServiceTests.cs`
- `Application/ClienteServiceTests.cs`
- `Application/GrupoGestionServiceTests.cs`
- `Application/PermisoGestionServiceTests.cs`
- `Application/ViajeServiceTests.cs`
- `Application/VisitaServiceTests.cs`
- `Application/SesionActualTests.cs`

### Infrastructure

- `Infrastructure/Pbkdf2PasswordHasherTests.cs`

### Integración SQL

- `Integration/ClienteRepositoryIntegrationTests.cs`
- `Integration/GrupoGestionRepositoryIntegrationTests.cs`
- `Integration/PermisoGestionRepositoryIntegrationTests.cs`
- `Integration/ViajeRepositoryIntegrationTests.cs`
- `Integration/VisitaRepositoryIntegrationTests.cs`
- `Integration/ViajeVisitaRepositoryIntegrationTests.cs`
- `Integration/InicializacionSeguridadRepositoryIntegrationTests.cs`
- `Integration/UsuarioAutenticacionRepositoryIntegrationTests.cs`
- `Integration/RendicionRepositoryIntegrationTests.cs`
- `Integration/RendicionRevisionRepositoryIntegrationTests.cs`
- `Integration/RendicionRevisionAuditoriaRepositoryIntegrationTests.cs`
- `Integration/AuditoriaDetalleRepositoryIntegrationTests.cs`

## 4. Distribución acumulada

El total definitivo se toma del ejecutor VSTest.

Último resultado consolidado:

- 700 pruebas totales;
- 700 correctas;
- 0 fallidas.

El conjunto incluye pruebas:

- de arquitectura;
- de dominio;
- de Application;
- de Infrastructure;
- de integración real con SQL Server.


## 5. Cobertura de Viaje, Viático, Cliente y Visita

Se verifican:

- construcción válida;
- fechas;
- importes;
- tipos;
- transiciones State;
- estados finales;
- viáticos vigentes y excluidos;
- total y saldo;
- colecciones protegidas;
- asociación de visitas;
- asociación de clientes;
- duplicados;
- cancelación condicionada.

## 6. Cobertura de Persona

Se verifica:

- construcción válida;
- nombre obligatorio;
- apellido obligatorio;
- email obligatorio;
- estado inicial activo;
- desactivación;
- reactivación.

## 7. Cobertura de Usuario

Se verifica:

- asociación con Persona;
- nombre obligatorio;
- normalización;
- hash obligatorio;
- salt obligatorio;
- iteraciones válidas;
- estado inicial activo;
- activación;
- desactivación;
- copias defensivas;
- asignación de uno o varios Grupos;
- rechazo de Grupo nulo;
- rechazo de Grupo duplicado;
- colección protegida.

## 8. Cobertura de Grupo y Permiso

Se verifica:

- creación válida;
- campos obligatorios;
- normalización;
- activación;
- desactivación;
- Permiso hoja;
- Grupo compuesto;
- Permisos directos;
- Grupos anidados;
- rechazo de nulos;
- rechazo de duplicados;
- rechazo de ciclos directos;
- rechazo de ciclos indirectos;
- eliminación de Permisos repetidos;
- inactividad de Grupo;
- inactividad de Grupo hijo;
- inactividad de Permiso;
- colección protegida.

## 9. Cobertura de autenticación

Se verifica:

- nombre vacío;
- contraseña vacía;
- Usuario inexistente;
- Usuario inactivo;
- contraseña incorrecta;
- credenciales correctas;
- normalización del nombre;
- mismo mensaje público para fallos.

## 10. Cobertura de autorización

Se verifica:

- Usuario nulo;
- Usuario inactivo;
- Permiso directo;
- Permiso anidado;
- ausencia de Permiso;
- código vacío;
- normalización de código.

## 11. Cobertura de sesión

Se verifica:

- sesión inicial vacía;
- inicio con Usuario activo;
- exposición del Usuario actual;
- cierre de sesión;
- rechazo de Usuario nulo;
- rechazo de Usuario inactivo.

## 12. Cobertura de PBKDF2

Se verifica:

- generación de hash;
- longitud del hash;
- generación de salt;
- longitud del salt;
- iteraciones predeterminadas;
- salts distintos;
- hashes distintos;
- contraseña correcta;
- contraseña incorrecta;
- hash alterado;
- entradas inválidas.

## 13. Comando de compilación

    MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "SIGEVIP.sln" "/t:Rebuild" "/p:Configuration=Debug" "/m"

## 14. Comando de ejecución

    "/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"

## 15. Último resultado verificado

Compilación:

- 0 advertencias.
- 0 errores.

Pruebas:

- Totales: 700.
- Correctas: 700.
- Fallidas: 0.
- Omitidas: 0.


## Cobertura de Auditoría general

La regresión vigente incluye Auditoría central para cambios de:

- Clientes;
- Usuarios;
- Grupos;
- Permisos;
- Viajes;
- Visitas;
- Viáticos;
- Rendiciones.

Las pruebas de integración SQL verifican:

- persistencia del cambio de negocio;
- persistencia de un único evento;
- módulo, acción, entidad e identificador;
- Usuario actor;
- uso de una única transacción;
- rollback del cambio cuando el actor de Auditoría no existe;
- ausencia de eventos parciales;
- eliminación de todos los datos temporales.

La revisión de Rendiciones agrega diez pruebas SQL:

- exclusión válida y rollback;
- reactivación válida y rollback;
- ajuste del anticipo válido y rollback;
- aprobación válida y rollback;
- cancelación válida y rollback.

Resultado consolidado:

- 682 pruebas totales;
- 682 correctas;
- 0 fallidas;
- 0 residuos SQL de Auditoría.

## 16. Criterio de cierre del módulo Clientes

El módulo se considera cerrado técnicamente cuando:

- la documentación se encuentre actualizada;
- `git diff --check` no informe errores;
- la solución compile;
- las 197 pruebas sean correctas;
- la migración `003` esté aplicada;
- el seed `002` sea reejecutable;
- la validación SQL indique `VALIDACIÓN CORRECTA`;
- la validación manual sea satisfactoria;
- exista commit documental;
- el commit sea publicado;
- el working tree quede limpio.


## Pruebas de integración de seguridad

Además de las pruebas unitarias, se ejecutan pruebas reales contra SQL Server.

Cobertura:

- Usuario inexistente.
- Recuperación de Usuario y Persona.
- Recuperación de grupos directos.
- Recuperación de permisos directos.
- Usuario inactivo.
- Contraseña correcta e incorrecta.
- Creación transaccional del administrador.
- Persistencia verificable de PBKDF2.
- Idempotencia.
- Rollback.
- Herencia desde grupos hijos.
- Rechazo de ciclos persistidos.

Resultado consolidado de seguridad y Clientes:

- 197 pruebas totales.
- 197 correctas.
- 0 fallidas.

## Cobertura específica del módulo Clientes

### Domain

Se verifica:

- creación válida;
- razón social obligatoria;
- CUIT obligatorio;
- normalización de CUIT;
- estado inicial activo;
- activación;
- desactivación;
- modificación controlada;
- preservación de identificador;
- preservación de estado durante la modificación;
- reconstrucción activa;
- reconstrucción inactiva.

### Application

Se verifica:

- rechazo sin sesión;
- rechazo de Usuario inactivo;
- rechazo sin permiso de consulta;
- listado autorizado;
- transferencia de filtros;
- rechazo sin permiso de gestión;
- registro válido;
- rechazo de CUIT duplicado;
- modificación de Cliente inexistente;
- exclusión del identificador actual al verificar CUIT;
- rechazo de CUIT perteneciente a otro Cliente;
- activación;
- desactivación;
- obtención por identificador;
- rechazo de identificador inválido.

### Integración SQL

Se verifica:

- inserción y recuperación;
- persistencia de datos;
- actualización;
- activación;
- desactivación;
- filtro por razón social;
- filtro por CUIT;
- filtro por localidad;
- filtro por estado;
- restricción única de CUIT;
- exclusión del identificador actual.

## Cobertura específica del módulo Viajes

### Domain

Se verifica:

- descripción obligatoria;
- fechas válidas;
- monto anticipado no negativo;
- tipo válido;
- al menos un participante;
- rechazo de participantes nulos;
- rechazo de participantes duplicados;
- reemplazo de participantes;
- modificación únicamente en Abierto;
- conservación de participantes históricos inactivos;
- rechazo de nuevos participantes inactivos durante una modificación;
- reconstrucción del agregado;
- validación del período respecto de Visitas y Viáticos.

### Application

Se verifica:

- sesión requerida;
- Usuario activo;
- permiso de consulta;
- permiso de creación;
- permiso de cancelación;
- listado;
- obtención;
- alta;
- modificación;
- cancelación;
- identificadores inválidos;
- Viaje inexistente;
- participantes inexistentes;
- participantes inactivos;
- participantes duplicados;
- transferencia de filtros.

### Integración SQL

Se verifica:

- consulta de Personas activas;
- recuperación de Personas inactivas;
- inserción y recuperación de Viaje;
- persistencia de varios participantes;
- actualización de datos;
- reemplazo de participantes;
- persistencia de monto y tipo;
- persistencia de cancelación;
- filtros por fechas;
- filtros por estado;
- filtro por participante;
- restricción de asociaciones duplicadas;
- rollback ante participante inexistente.

### Resultado consolidado

- 291 pruebas totales;
- 291 correctas;
- 0 fallidas;
- compilación con 0 advertencias y 0 errores.

## Cobertura específica del módulo Visitas

### Domain

Se verifica:

- creación válida;
- modificación de datos y Clientes;
- rechazo de modificación sin Clientes;
- fecha válida;
- observación obligatoria;
- localidad obligatoria;
- asociación con Viaje;
- fecha dentro del período del Viaje;
- uno o varios Clientes;
- rechazo de Cliente nulo;
- rechazo de Cliente duplicado;
- reconstrucción desde persistencia;
- preservación de Clientes históricos.

### Application

Se verifica:

- sesión requerida;
- Usuario activo;
- permiso `VISITA_REGISTRAR`;
- permiso `VIAJE_CONSULTAR`;
- permiso `CLIENTE_CONSULTAR`;
- registro válido;
- obtención de una Visita perteneciente al Viaje;
- modificación válida y auditoría;
- rechazo de modificación sin permiso o con Viaje no Abierto;
- conservación de Cliente histórico inactivo y rechazo de nuevos inactivos;
- Viaje inexistente;
- identificadores inválidos;
- selección vacía;
- Clientes inexistentes;
- Clientes inactivos;
- Clientes duplicados;
- listado por Viaje;
- listado por Cliente.

### Integración SQL

Se verifica:

- consulta de Clientes activos;
- exclusión de Clientes inactivos del selector;
- recuperación histórica de Cliente inactivo;
- inserción de Visita;
- actualización transaccional de datos y asociaciones;
- auditoría transaccional de modificación;
- asociación con un Cliente;
- asociación con varios Clientes;
- consulta por Viaje;
- consulta por Cliente;
- rollback ante Cliente inexistente;
- reconstrucción de Visitas dentro de Viaje;
- reconstrucción de sus Clientes;
- bloqueo de cancelación con agregado desactualizado;
- persistencia del estado Abierto cuando se rechaza la cancelación.

### Interfaz

Se verificó manualmente:

- acceso desde el menú;
- visibilidad según permiso;
- selección de Viaje;
- consulta de Visitas;
- fecha limitada al período del Viaje;
- selección múltiple de Clientes mediante grilla filtrable y ordenable;
- conservación de selección al filtrar u ordenar;
- registro correcto;
- modificación de Visita mediante botón y doble clic;
- actualización de la grilla;
- rechazo de cancelación del Viaje.

### Resultado consolidado del cierre de Visitas

La cantidad final de pruebas y el resultado de compilación se registran al ejecutar la regresión completa del incremento de modificación de Visitas y grillas de selección. El cierre exige 0 pruebas fallidas, 0 advertencias y 0 errores antes del commit.

## Cobertura específica del módulo Gestión de Grupos

### Domain

Archivo:

`tests/SIGEVIP.Tests/Domain/GrupoGestionDomainTests.cs`

Se verifica:

- actualización de Nombre y Descripción;
- campos obligatorios;
- normalización;
- reemplazo de Permisos directos;
- selección no vacía;
- rechazo de Permiso nulo;
- rechazo de Permisos duplicados;
- preservación de Grupos hijos;
- conservación del estado anterior ante errores.

Cantidad:

- 9 pruebas.

### Application

Archivo:

`tests/SIGEVIP.Tests/Application/GrupoGestionServiceTests.cs`

Se verifica:

- sesión requerida;
- Usuario activo;
- permiso `GRUPO_GESTIONAR`;
- listado;
- detalle;
- alta;
- generación de Código;
- unicidad de Código;
- unicidad de Nombre;
- Permisos activos;
- rechazo de selección vacía;
- rechazo de duplicados;
- modificación;
- Código inmutable;
- activación;
- desactivación;
- protección de `ADMINISTRADOR_GENERAL`;
- conservación de Permisos administrativos mínimos.

Cantidad:

- 25 pruebas.

### Integración SQL

Archivo:

`tests/SIGEVIP.Tests/Integration/GrupoGestionRepositoryIntegrationTests.cs`

Se verifica:

- listado y cantidades agregadas;
- búsqueda;
- filtro por estado;
- detalle;
- reconstrucción de Permisos directos;
- reconstrucción de Grupos hijos;
- listado de Permisos activos;
- inserción transaccional;
- Código duplicado;
- rollback;
- modificación transaccional;
- reemplazo de `GrupoPermiso`;
- preservación de `GrupoGrupo`;
- activación;
- desactivación;
- tratamiento de registros inexistentes.

Cantidad:

- 19 pruebas.

### Resultado del módulo

Pruebas específicas nuevas:

- Domain: 9;
- Application: 25;
- integración SQL: 19;
- total: 53.

Regresión consolidada:

- 532 pruebas totales;
- 532 correctas;
- 0 fallidas;
- compilación con 0 advertencias;
- compilación con 0 errores;
- validación manual aprobada.

## Cobertura específica del módulo Gestión de Permisos

### Domain

Archivo:

`tests/SIGEVIP.Tests/Domain/PermisoGestionDomainTests.cs`

Cantidad:

- 10 pruebas.

Se verifica:

- actualización de Nombre y Descripción;
- Nombre obligatorio;
- Descripción opcional;
- Código inmutable;
- identificador preservado;
- estado preservado;
- activación;
- desactivación;
- conservación del estado anterior ante errores.

### Application

Archivo:

`tests/SIGEVIP.Tests/Application/PermisoGestionServiceTests.cs`

Cantidad:

- 30 pruebas.

Se verifica:

- sesión requerida;
- Usuario activo;
- permiso `PERMISO_GESTIONAR`;
- listado;
- detalle;
- registro;
- normalización del Código;
- Código único;
- Nombre obligatorio;
- modificación;
- Código inmutable;
- activación;
- desactivación;
- protección de `USUARIO_GESTIONAR`;
- protección de `GRUPO_GESTIONAR`;
- protección de `PERMISO_GESTIONAR`.

### Integración SQL

Archivo:

`tests/SIGEVIP.Tests/Integration/PermisoGestionRepositoryIntegrationTests.cs`

Cantidad:

- 19 pruebas.

Se verifica:

- listado y filtros;
- búsqueda por Código, Nombre y Descripción;
- detalle;
- cantidad de Grupos;
- reconstrucción activa e inactiva;
- comprobación de Código;
- alta;
- Descripción nula;
- Código duplicado;
- modificación;
- preservación del Código;
- preservación del estado;
- preservación de `GrupoPermiso`;
- activación y desactivación;
- registros inexistentes.

### Preservación de Permisos inactivos en Grupos

Se agregaron 3 pruebas a:

`GrupoGestionRepositoryIntegrationTests`

Se verifica:

- inclusión del Permiso inactivo ya seleccionado;
- exclusión del Permiso inactivo no seleccionado;
- coexistencia con Permisos activos.

### Resultado del módulo

Pruebas incorporadas:

- Domain: 10;
- Application: 30;
- integración del repositorio: 19;
- preservación en Grupos: 3;
- total: 62.

Regresión consolidada:

- 594 pruebas totales;
- 594 correctas;
- 0 fallidas;
- compilación con 0 advertencias;
- compilación con 0 errores;
- validación manual aprobada;
- datos temporales eliminados.

## 17. Cierre de consulta de Auditoría y controles operativos

### Pruebas específicas

Se ejecutaron 99 pruebas filtradas de:

- `ViaticoServiceTests`;
- `ViaticoApplicationModelTests`;
- `ViajeServiceTests`;
- `ViajeTests`;
- `RendicionServiceTests`.

Resultado:

- 99 ejecutadas;
- 99 correctas;
- 0 fallidas.

### Cobertura incorporada

- rechazo de Persona pagadora ajena al Viaje durante el alta;
- rechazo de Persona pagadora ajena al Viaje durante la modificación;
- selector de pagadores limitado a participantes activos;
- rechazo de Comprobante cuyo total no coincide con el Viático;
- presentación `Sí`/`No` del Comprobante;
- bloqueo de modificación de Viajes no abiertos;
- consulta de eventos de Auditoría;
- recuperación del detalle de cambios de Cliente;
- catálogos de módulos y acciones;
- filtros combinados;
- ordenamiento de Clientes, Auditoría y Rendiciones.

### Regresión completa

- 700 pruebas ejecutadas;
- 700 correctas;
- 0 fallidas;
- tiempo registrado: 23,2707 segundos.

### Validación manual

Se verificó:

- pagadores limitados a participantes;
- registro con pagador válido;
- rechazo visual de modificación de Viajes no abiertos;
- representación `Sí`/`No`;
- ordenamiento de las cuatro grillas de Rendiciones;
- conservación de la selección correcta después de ordenar;
- filtros y detalle de Auditoría;
- ID visible y ordenamiento de Clientes.
