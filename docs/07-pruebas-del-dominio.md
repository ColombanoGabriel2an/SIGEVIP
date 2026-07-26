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
- `Domain/PersonaTests.cs`
- `Domain/UsuarioTests.cs`
- `Domain/ViajeTests.cs`
- `Domain/ViajeVisitaTests.cs`
- `Domain/ViaticoTests.cs`
- `Domain/VisitaTests.cs`

### Application

- `Application/AutenticacionServiceTests.cs`
- `Application/AutorizacionServiceTests.cs`
- `Application/ClienteServiceTests.cs`
- `Application/SesionActualTests.cs`

### Infrastructure

- `Infrastructure/Pbkdf2PasswordHasherTests.cs`

### Integración SQL

- `Integration/ClienteRepositoryIntegrationTests.cs`
- `Integration/InicializacionSeguridadRepositoryIntegrationTests.cs`
- `Integration/UsuarioAutenticacionRepositoryIntegrationTests.cs`

## 4. Distribución acumulada

El total definitivo se toma del ejecutor VSTest.

Último resultado consolidado:

- 197 pruebas totales;
- 197 correctas;
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

- Totales: 197.
- Correctas: 197.
- Fallidas: 0.
- Omitidas: 0.


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
