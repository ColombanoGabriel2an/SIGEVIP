# Pruebas del dominio: Viajes, Viáticos, Clientes y Visitas

## 1. Herramientas

- Framework: MSTest.
- MSTest.TestFramework: 3.7.3.
- MSTest.TestAdapter: 3.7.3.
- Microsoft.NET.Test.Sdk: 17.13.0.
- Ejecutor: VSTest 17.13.0 x64.
- Plataforma: .NET Framework 4.8.

## 2. Archivos

- `tests/SIGEVIP.Tests/ArchitectureTests.cs`
- `tests/SIGEVIP.Tests/Domain/ClienteTests.cs`
- `tests/SIGEVIP.Tests/Domain/ViajeTests.cs`
- `tests/SIGEVIP.Tests/Domain/ViajeVisitaTests.cs`
- `tests/SIGEVIP.Tests/Domain/ViaticoTests.cs`
- `tests/SIGEVIP.Tests/Domain/VisitaTests.cs`

## 3. Distribución verificada

- Pruebas existentes del Bloque 1: 38.
- Pruebas de Cliente y Visita: 15.
- Pruebas de integración Viaje-Visita: 18.
- Total final ejecutado: 71.

## 4. Cobertura de Cliente

Las pruebas verifican:

- Cliente nuevo activo.
- Razón social obligatoria.
- CUIT obligatorio.
- Desactivación.
- Reactivación.
- Normalización de espacios y guiones del CUIT.

## 5. Cobertura de Visita y Cliente

Las pruebas verifican:

- Observación obligatoria.
- Localidad del encuentro obligatoria.
- Rechazo de cliente nulo.
- Asociación de un cliente.
- Asociación de varios clientes.
- Rechazo de la misma referencia.
- Rechazo del mismo identificador persistido.
- Rechazo del mismo CUIT normalizado.
- Colección de clientes protegida.

## 6. Cobertura de Viaje y Visita

Las pruebas verifican:

- Rechazo de visita sin clientes.
- Incorporación válida en viaje Abierto.
- Fecha igual a FechaInicio.
- Fecha igual a FechaFin.
- Rechazo de fecha anterior.
- Rechazo de fecha posterior.
- Rechazo de visita nula.
- Bloqueo en EnRendicion.
- Bloqueo en Aprobado.
- Bloqueo en Cancelado.
- Rechazo de la misma referencia.
- Rechazo del mismo identificador persistido.
- Rechazo de reasignación a otro viaje.
- Colección de visitas protegida.
- Cancelación rechazada en Abierto con visitas.
- Conservación del estado Abierto.
- Cancelación rechazada en EnRendicion con visitas.
- Conservación del estado EnRendicion.

## 7. Cobertura de regresión

Las 38 pruebas existentes continúan verificando:

- Disponibilidad de Domain y Application.
- Estado inicial del viaje.
- Validación del período.
- Tipos de viaje.
- Monto anticipado.
- Incorporación de viáticos.
- Fechas de viáticos.
- Viático nulo y repetido.
- Colección protegida.
- Total gastado.
- Saldo positivo, negativo y cero.
- Transiciones del patrón State.
- Bloqueos por estado.
- Exclusión y reactivación.
- Reconstrucción desde estado persistible.
- Validaciones monetarias del viático.

## 8. Comando de compilación

    MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "SIGEVIP.sln" "/t:Rebuild" "/p:Configuration=Debug" "/m"

## 9. Comando de ejecución

    "/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"

## 10. Resultado verificado

Compilación:

- 0 advertencias.
- 0 errores.
- Tiempo registrado: 0,82 segundos.

Pruebas:

- Totales: 71.
- Correctas: 71.
- Fallidas: 0.
- Omitidas: 0.
- Tiempo total registrado: 0,8536 segundos.

## 11. Criterio de finalización del Bloque 2

El bloque podrá declararse técnicamente cerrado después de:

- Revisar los cambios documentales.
- Ejecutar `git diff --check`.
- Crear el commit documental.
- Repetir compilación.
- Repetir las 71 pruebas.
- Subir el commit.
- Verificar working tree limpio.
- Verificar sincronización con la rama remota.
- Generar el informe de cierre para MAESTRO.
