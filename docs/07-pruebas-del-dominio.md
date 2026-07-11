# Pruebas del dominio: Viajes y Viáticos

## 1. Herramientas

- Framework: MSTest.
- MSTest.TestFramework: 3.7.3.
- MSTest.TestAdapter: 3.7.3.
- Microsoft.NET.Test.Sdk: 17.13.0.
- Ejecutor: VSTest 17.13.0 x64.
- Plataforma: .NET Framework 4.8.

## 2. Archivos

- `tests/SIGEVIP.Tests/ArchitectureTests.cs`
- `tests/SIGEVIP.Tests/Domain/ViajeTests.cs`
- `tests/SIGEVIP.Tests/Domain/ViaticoTests.cs`

## 3. Cobertura funcional

Las pruebas verifican:

- Disponibilidad de Domain y Application desde Tests.
- Estado inicial del viaje.
- Validación del período.
- Tipo EventoFeria.
- Tipo de viaje inválido.
- Monto anticipado negativo.
- Incorporación válida de viáticos.
- Fecha de viático anterior al viaje.
- Fecha de viático posterior al viaje.
- Viático nulo.
- Viático repetido.
- Colección de solo lectura.
- Total gastado con viáticos vigentes.
- Total gastado sin viáticos vigentes.
- Total gastado sin viáticos.
- Saldo positivo.
- Saldo negativo.
- Saldo cero.
- Envío a rendición.
- Bloqueo de carga en rendición.
- Aprobación desde rendición.
- Rechazo de aprobación directa.
- Bloqueo en aprobado.
- Bloqueo en cancelado.
- Cancelación desde abierto.
- Cancelación desde rendición.
- Rechazo de cancelación desde aprobado.
- Rechazo de doble envío a rendición.
- Exclusión lógica.
- Reactivación.
- Rechazo de exclusión en abierto.
- Rechazo de exclusión de un viático ajeno.
- Reconstrucción desde estado persistible.
- Estado persistible inválido.
- Creación válida de viático.
- Monto de viático igual a cero.
- Monto de viático negativo.
- Descripción nula normalizada.

## 4. Comando de compilación

    MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "SIGEVIP.sln" "/t:Rebuild" "/p:Configuration=Debug" "/m"

## 5. Comando de ejecución

    "/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"

## 6. Resultado verificado

Compilación:

- 0 advertencias.
- 0 errores.

Pruebas:

- Totales: 38.
- Correctas: 38.
- Fallidas: 0.
- Omitidas: 0.
- Tiempo total registrado: 0,8346 segundos.

## 7. Criterio de finalización

El primer bloque podrá considerarse técnicamente validado después de:

- Revisar los cambios documentales.
- Repetir compilación.
- Repetir las pruebas.
- Crear el commit.
- Subir la rama remota.
