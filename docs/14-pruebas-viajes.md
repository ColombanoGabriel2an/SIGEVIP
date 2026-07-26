# Pruebas del módulo Viajes

## 1. Alcance

Este documento registra las pruebas del módulo Viajes en:

- Domain;
- Application;
- Infrastructure;
- SQL Server;
- WinForms.

## 2. Resultado automatizado

Última ejecución:

- 246 pruebas totales;
- 246 correctas;
- 0 fallidas.

## 3. Compilación

Resultado:

- 0 advertencias;
- 0 errores.

Comando:

```bash
MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" \
  "SIGEVIP.sln" \
  "/t:Rebuild" \
  "/p:Configuration=Debug" \
  "/m"
```

## 4. Pruebas de Domain

Archivo:

`tests/SIGEVIP.Tests/Domain/ViajeParticipanteTests.cs`

Cobertura:

- participante válido;
- participante nulo;
- participante duplicado;
- colección protegida;
- reemplazo;
- al menos un participante;
- participante inactivo histórico;
- modificación según estado;
- reconstrucción;
- validación del período.

También se conservan:

- `tests/SIGEVIP.Tests/Domain/ViajeTests.cs`;
- `tests/SIGEVIP.Tests/Domain/ViajeVisitaTests.cs`;
- `tests/SIGEVIP.Tests/Domain/ViaticoTests.cs`.

## 5. Pruebas de Application

Archivo:

`tests/SIGEVIP.Tests/Application/ViajeServiceTests.cs`

Cobertura:

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
- Viaje inexistente;
- identificadores inválidos;
- participantes inexistentes;
- participantes inactivos;
- participantes duplicados;
- transferencia de filtros.

Se utilizan:

- repositorio falso de Viajes;
- repositorio falso de Personas;
- sesión controlada;
- Usuarios con permisos configurados para cada escenario.

## 6. Pruebas de integración SQL

Archivo:

`tests/SIGEVIP.Tests/Integration/ViajeRepositoryIntegrationTests.cs`

Cobertura:

- consulta de Personas activas;
- recuperación de Personas inactivas históricas;
- inserción y recuperación de Viaje;
- persistencia de varios participantes;
- actualización de datos;
- reemplazo de participantes;
- persistencia del monto anticipado;
- persistencia del tipo de Viaje;
- persistencia del estado Cancelado;
- filtro por fecha desde;
- filtro por fecha hasta;
- filtro por estado;
- filtro por participante;
- restricción de asociaciones duplicadas;
- rollback ante participante inexistente.

Cada prueba:

- genera datos únicos;
- utiliza la base `SIGEVIP`;
- ejecuta operaciones reales contra SQL Server;
- revierte o elimina los datos generados;
- conserva aislado el resultado de las demás pruebas.

## 7. Validación SQL

Scripts ejecutados:

- `database/migrations/004_crear_viajes.sql`;
- `database/seed/003_permisos_modulo_viajes.sql`;
- reejecución de `003_permisos_modulo_viajes.sql`;
- `database/migrations/004_validar_viajes.sql`.

Resultados verificados:

- versión `004` registrada;
- tabla `dbo.Viaje` creada;
- tabla `dbo.ViajeParticipante` creada;
- claves primarias creadas;
- claves foráneas creadas;
- índices creados;
- restricciones `CHECK` creadas;
- restricciones `DEFAULT` creadas;
- períodos inválidos: 0;
- montos negativos: 0;
- tipos inválidos: 0;
- estados inválidos: 0;
- participantes duplicados: 0;
- asignación de `VIAJE_CREAR`: 1;
- asignación de `VIAJE_CANCELAR`: 1;
- validación final: `VALIDACIÓN CORRECTA`.

## 8. Validación manual

Se verificó:

- inicio de sesión;
- visualización del botón Viajes;
- apertura del módulo desde `MainForm`;
- desaparición del mensaje de módulo pendiente;
- carga de la grilla;
- alta válida;
- persistencia del Viaje;
- persistencia de participantes;
- descripción obligatoria;
- participante obligatorio;
- rechazo de fecha final anterior a la inicial;
- modificación de un Viaje Abierto;
- actualización de descripción;
- actualización del monto anticipado;
- consulta del detalle;
- visualización de participantes;
- filtro por fecha desde;
- filtro por fecha hasta;
- filtro por estado;
- filtro por participante;
- limpieza de filtros;
- cancelación;
- persistencia del estado Cancelado;
- rechazo de una segunda cancelación;
- rechazo de modificación de un Viaje Cancelado;
- cierre del módulo;
- cierre de sesión;
- nueva autenticación;
- nueva apertura del módulo.

Se corrigió durante la validación:

- superposición visual entre la lista de participantes y el aviso de Viáticos en `ViajeDetalleForm`.

Resultado:

`APROBADO`

## 9. Regresión final

Comando:

```bash
"/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" \
  "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"
```

Resultado:

- pruebas totales: 246;
- correctas: 246;
- fallidas: 0.

Tiempo de la última ejecución:

`10,3794 segundos`

## 10. Criterio de aceptación

El módulo cumple el criterio de cierre porque:

- compila con 0 advertencias y 0 errores;
- pasa las 246 pruebas automatizadas;
- persiste Viajes y participantes;
- utiliza transacciones;
- aplica autorización en Application;
- aplica permisos visuales en WinForms;
- maneja errores de negocio y persistencia;
- posee migración reproducible;
- posee seed reejecutable;
- posee validación SQL;
- posee validación manual reproducible;
- conserva trazabilidad entre requisitos, dominio, casos de uso, base de datos, interfaz y pruebas.

## 11. Alcance pendiente

No forman parte de este cierre:

- persistencia de Visitas;
- persistencia de Viáticos;
- total gastado persistido;
- saldo pendiente persistido;
- bloqueo de cancelación basado en Visitas persistidas;
- historial completo del Viaje;
- envío a rendición desde la interfaz;
- aprobación desde la interfaz.

Estas funciones deberán incorporarse en incrementos posteriores.