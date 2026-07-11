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
- `Application/SesionActualTests.cs`

### Infrastructure

- `Infrastructure/Pbkdf2PasswordHasherTests.cs`

## 4. Distribución acumulada

- Bloques 1 y 2: 71 pruebas.
- Persona y Usuario: 17 pruebas.
- Grupo y Permiso: 26 pruebas nuevas acumuladas con Usuario.
- Autenticación, autorización y sesión: 19 pruebas.
- PBKDF2: 12 pruebas.
- Total ejecutado: 145.

La suma se expresa por incrementos históricos. El total definitivo se toma del ejecutor VSTest.

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
- Tiempo: 1,29 segundos.

Pruebas:

- Totales: 145.
- Correctas: 145.
- Fallidas: 0.
- Omitidas: 0.
- Tiempo: 4,0902 segundos.

## 16. Criterio de cierre del Bloque 3

El bloque se considera cerrado técnicamente cuando:

- toda la documentación esté actualizada;
- `git diff --check` no informe errores;
- la solución compile;
- las 145 pruebas sean correctas;
- exista commit documental;
- el commit sea publicado;
- el working tree quede limpio;
- se informe a MAESTRO el estado y los pendientes.
