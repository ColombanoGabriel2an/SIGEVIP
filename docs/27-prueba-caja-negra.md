# Prueba de caja negra — Cálculo de saldo de un Viaje

## 1. Objetivo

Verificar el comportamiento observable del cálculo de saldo de un Viaje sin analizar su implementación interna.

La funcionalidad evaluada corresponde al cierre de la rendición de Viáticos.

## 2. Elemento bajo prueba

Clase:

`src/SIGEVIP.Domain/Entities/Viaje.cs`

Operación pública:

```csharp
public decimal CalcularSaldo()
```

Entradas funcionales:

- monto anticipado del Viaje;
- suma de los Viáticos vigentes;
- estado vigente o excluido de cada Viático.

Salida:

- saldo numérico del Viaje.

## 3. Regla funcional

**Saldo = total de Viáticos vigentes − monto anticipado**

Interpretación:

- saldo positivo: el total gastado supera el anticipo y existe una diferencia a reintegrar;
- saldo negativo: el anticipo supera el total gastado y existe una diferencia a devolver;
- saldo cero: no existe diferencia pendiente;
- los Viáticos excluidos no forman parte del total gastado.

## 4. Técnica aplicada

Se utilizan:

- particiones de equivalencia;
- valores límite;
- combinación de Viáticos vigentes y excluidos.

### 4.1. Particiones de equivalencia

| Partición | Condición | Resultado esperado |
|---|---|---|
| PE1 | Total vigente mayor que anticipo | Saldo positivo |
| PE2 | Total vigente menor que anticipo | Saldo negativo |
| PE3 | Total vigente igual al anticipo | Saldo cero |
| PE4 | No existen Viáticos vigentes | Saldo igual a `0 − anticipo` |
| PE5 | Existen Viáticos excluidos | Los importes excluidos no se suman |

### 4.2. Valores límite

- anticipo igual a cero;
- total vigente igual a cero;
- diferencia exacta igual a cero;
- primer valor positivo;
- primer valor negativo;
- coexistencia de Viáticos vigentes y excluidos.

## 5. Casos de prueba

| ID | Anticipo | Viáticos vigentes | Viáticos excluidos | Saldo esperado | Partición |
|---|---:|---:|---:|---:|---|
| CN-01 | 100.000 | 120.000 | 0 | 20.000 | PE1 |
| CN-02 | 100.000 | 80.000 | 0 | -20.000 | PE2 |
| CN-03 | 100.000 | 100.000 | 0 | 0 | PE3 |
| CN-04 | 0 | 0 | 0 | 0 | PE3 / límite |
| CN-05 | 100.000 | 0 | 0 | -100.000 | PE4 |
| CN-06 | 80.000 | 70.000 | 50.000 | -10.000 | PE2 / PE5 |
| CN-07 | 0 | 10.000 | 0 | 10.000 | PE1 / límite |

## 6. Correspondencia con pruebas automatizadas

Las siguientes pruebas MSTest existentes validan las particiones principales:

- `CalcularSaldo_ConResultadoPositivo_DevuelveDiferencia`
- `CalcularSaldo_ConResultadoNegativo_DevuelveDiferencia`
- `CalcularSaldo_ConResultadoCero_DevuelveCero`
- `TotalGastado_SumaSolamenteViaticosVigentes`
- `TotalGastado_SinViaticosVigentes_DevuelveCero`
- `TotalGastado_SinViaticos_DevuelveCero`

Archivo:

`tests/SIGEVIP.Tests/Domain/ViajeTests.cs`

## 7. Ejecución reproducible

Desde Git Bash:

```bash
cd /c/Users/gabic/Desktop/UAI/SIGEVIP

"/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" \
  "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll" \
  /TestCaseFilter:"Name~CalcularSaldo|Name~TotalGastado"
```

Resultado esperado:

- todas las pruebas filtradas correctas;
- ninguna prueba fallida;
- el saldo positivo, negativo y cero coincide con las salidas esperadas;
- los Viáticos excluidos no alteran `TotalGastado`.

## 8. Resultado de aceptación

La prueba de caja negra se considera satisfactoria cuando:

1. CN-01 produce un saldo positivo;
2. CN-02 produce un saldo negativo;
3. CN-03 y CN-04 producen cero;
4. CN-05 representa correctamente la ausencia de gastos;
5. CN-06 ignora el importe excluido;
6. CN-07 funciona con anticipo cero;
7. las pruebas automatizadas finalizan sin errores.

## 9. Evidencia requerida

- salida de VSTest filtrada;
- captura del resultado;
- referencia al commit evaluado;
- tabla de casos incorporada al anexo técnico.


## 10. Resultado ejecutado

Fecha:

`28/07/2026`

Resultado de VSTest:

- pruebas ejecutadas: 6;
- pruebas correctas: 6;
- pruebas fallidas: 0;
- tiempo total: 0,9897 segundos.

Pruebas verificadas:

- `TotalGastado_SumaSolamenteViaticosVigentes`;
- `TotalGastado_SinViaticosVigentes_DevuelveCero`;
- `TotalGastado_SinViaticos_DevuelveCero`;
- `CalcularSaldo_ConResultadoPositivo_DevuelveDiferencia`;
- `CalcularSaldo_ConResultadoNegativo_DevuelveDiferencia`;
- `CalcularSaldo_ConResultadoCero_DevuelveCero`.

Estado:

`PRUEBA DE CAJA NEGRA CORRECTA`
