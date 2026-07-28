# Pruebas del módulo Viáticos y Rendiciones

## 1. Alcance

Este documento registra las pruebas del módulo Viáticos y Rendiciones en:

- Domain;
- Application;
- Infrastructure;
- SQL Server;
- WinForms;
- regresión general.

## 2. Resultado automatizado

Última ejecución validada:

- 415 pruebas totales;
- 415 correctas;
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

Archivos principales:

- `tests/SIGEVIP.Tests/Domain/ViaticoTests.cs`;
- pruebas del agregado `Viaje`;
- pruebas de estados de Viaje;
- pruebas económicas y de auditoría.

Cobertura:

- asociación del Viático al Viaje;
- fecha dentro del período;
- categoría válida;
- método de pago válido;
- Persona pagadora obligatoria según método;
- monto positivo;
- descripción obligatoria sin Comprobante;
- Comprobante válido;
- exclusión;
- reactivación;
- total gastado vigente;
- saldo pendiente;
- envío a rendición;
- aprobación;
- cancelación;
- bloqueo de modificaciones según estado.

## 5. Pruebas de Application

### ViaticoService

Archivo:

`tests/SIGEVIP.Tests/Application/ViaticoServiceTests.cs`

Cantidad:

- 16 métodos de prueba.

Cobertura:

- sesión requerida;
- Usuario activo;
- permiso de consulta;
- permiso de registro;
- permiso de modificación;
- Viaje existente;
- Persona pagadora existente;
- Persona pagadora activa;
- alta;
- modificación;
- consulta;
- filtros;
- identificadores inválidos;
- errores de negocio;
- transferencia correcta de comandos y datos.

### RendicionService

Archivo:

`tests/SIGEVIP.Tests/Application/RendicionServiceTests.cs`

Cantidad:

- 16 métodos de prueba.

Cobertura:

- permiso de revisión;
- permiso de envío;
- envío a rendición;
- detalle;
- exclusión;
- reactivación;
- ajuste del anticipo;
- aprobación;
- cancelación;
- motivo obligatorio;
- identificadores inválidos;
- Viaje inexistente;
- Usuario autenticado válido;
- auditoría;
- reglas de transición.

## 6. Pruebas de integración SQL

Archivos:

- `tests/SIGEVIP.Tests/Integration/PersonaConsultaViaticoRepositoryIntegrationTests.cs`;
- `tests/SIGEVIP.Tests/Integration/ViaticoRepositoryIntegrationTests.cs`;
- `tests/SIGEVIP.Tests/Integration/ViajeViaticoRepositoryIntegrationTests.cs`;
- `tests/SIGEVIP.Tests/Integration/RendicionRepositoryIntegrationTests.cs`;
- `tests/SIGEVIP.Tests/Integration/RendicionRevisionRepositoryIntegrationTests.cs`;
- `tests/SIGEVIP.Tests/Integration/RendicionRevisionAuditoriaRepositoryIntegrationTests.cs`.

Cobertura:

- consulta de Personas pagadoras activas;
- recuperación de Persona por identificador;
- inserción de Viático;
- inserción de Comprobante;
- actualización de Viático;
- incorporación y eliminación de Comprobante;
- consulta por Viaje;
- filtros por fecha, categoría y estado;
- reconstrucción del Viático;
- reconstrucción del Comprobante;
- reconstrucción de Viajes con Viáticos;
- cálculo de total y saldo;
- envío a rendición;
- listado de pendientes;
- detalle completo;
- exclusión lógica;
- reactivación;
- ajuste del anticipo;
- aprobación;
- cancelación;
- auditoría;
- concurrencia;
- rollback ante errores.

Las pruebas utilizan SQL Server real y datos aislados.

## 7. Validación SQL

Scripts:

- `database/migrations/006_crear_viaticos_rendiciones.sql`;
- `database/migrations/006_validar_viaticos_rendiciones.sql`.

Se verificó:

- versión `006`;
- tablas;
- columnas;
- claves primarias;
- claves foráneas;
- índices;
- restricciones;
- unicidad del Comprobante por Viático;
- importes no negativos;
- categorías válidas;
- métodos de pago válidos;
- estados válidos;
- sucursal de cuatro dígitos;
- número de ocho dígitos;
- campos de auditoría;
- reejecución segura;
- resultado final correcto.

## 8. Validación manual de Viáticos

Se verificó:

- acceso desde el menú principal;
- apertura de Viáticos y Rendiciones;
- apertura de Gestión de viáticos;
- listado de Viajes abiertos;
- filtros por fecha;
- filtro por categoría;
- filtro por estado;
- limpieza de filtros;
- consulta del detalle;
- visualización de anticipo;
- visualización de total vigente;
- visualización de saldo;
- alta sin Comprobante;
- descripción obligatoria sin Comprobante;
- alta con Comprobante;
- CUIT obligatorio;
- razón social obligatoria;
- sucursal de cuatro dígitos;
- número de ocho dígitos;
- cálculo automático del total;
- selección de Persona pagadora;
- pagador obligatorio para PagoPersonal;
- pagador obligatorio para TarjetaCorporativa;
- pagador deshabilitado para EfectivoEmpresa;
- modificación de Viático vigente;
- alta, cambio y eliminación del Comprobante;
- actualización de grilla;
- actualización del total y saldo;
- envío a rendición;
- desaparición del Viaje de la lista de abiertos.

Resultado:

`APROBADO`

## 9. Validación manual de Rendiciones

Se verificó:

- acceso con `RENDICION_REVISAR`;
- listado de pendientes;
- selección de Rendición;
- datos generales;
- fecha de envío;
- anticipo;
- total vigente;
- saldo;
- Viáticos;
- Comprobantes;
- Visitas;
- Clientes;
- Participantes;
- detalle del Viático;
- auditoría de exclusión;
- exclusión con motivo;
- disminución del total;
- actualización del saldo;
- reactivación;
- reincorporación al total;
- ajuste del anticipo;
- aprobación;
- desaparición de pendientes;
- estado Aprobado;
- cancelación con motivo;
- estado Cancelado;
- visibilidad individual según permisos.

Resultado:

`APROBADO`

## 10. Correcciones detectadas durante la validación

Se corrigió:

- incorporación explícita de `ViaticosForm.cs` al proyecto clásico;
- sustitución de permisos visuales obsoletos;
- acceso a Rendiciones condicionado por `RENDICION_REVISAR`;
- reemplazo de mensajes provisionales;
- detalle de Viaje actualizado con total gastado y saldo reales;
- navegación real a Viáticos y Rendiciones.

## 11. Regresión final

Comando:

```bash
"/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" \
  "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"
```

Resultado:

- pruebas totales: 415;
- correctas: 415;
- fallidas: 0.

## 12. Criterio de aceptación

El módulo cumple el criterio de cierre porque:

- compila con 0 advertencias y 0 errores;
- pasa las 415 pruebas automatizadas;
- persiste Viáticos y Comprobantes;
- reconstruye el agregado Viaje completo;
- utiliza transacciones;
- controla concurrencia;
- aplica autorización en Application;
- aplica permisos visuales en WinForms;
- maneja errores de negocio y persistencia;
- posee migración reproducible;
- posee validación SQL;
- posee validación manual reproducible;
- conserva trazabilidad entre requisitos, dominio, casos de uso, base de datos, interfaz y pruebas.

## 13. Alcance pendiente

No forman parte de este cierre:

- exportación de Rendiciones;
- reportes consolidados;
- archivos adjuntos digitales;
- consulta global de auditoría;
- integración contable;
- mapa y geolocalización.

## Cierre de pruebas de Auditoría de Rendiciones

Se agregaron diez pruebas de integración SQL para comprobar la atomicidad de:

- exclusión de Viático;
- reactivación de Viático;
- ajuste del anticipo;
- aprobación;
- cancelación.

Cada operación posee:

1. una prueba de persistencia correcta del cambio y el evento;
2. una prueba con actor inexistente que exige rollback completo.

También se mantiene la cobertura previa del envío a rendición, con persistencia
correcta y rollback.

Resultado consolidado:

- 682 pruebas totales;
- 682 correctas;
- 0 fallidas;
- 0 advertencias de compilación;
- 0 errores de compilación;
- 0 Auditorías temporales;
- 0 Viajes temporales;
- 0 Viáticos temporales;
- 0 Personas temporales.

Archivo principal:

`tests/SIGEVIP.Tests/Integration/RendicionRevisionAuditoriaRepositoryIntegrationTests.cs`
