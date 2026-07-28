# Pruebas de jerarquías de Grupos

## 1. Alcance

Este documento registra la validación de la gestión funcional y visual de relaciones `GrupoGrupo`.

Trazabilidad:

- CUD07 — Gestionar Grupos;
- CUD08 — Agregar Grupo;
- CUD09 — Modificar Grupo;
- RN-SEG-10 — Grupo puede contener otros Grupos;
- RN-SEG-13 — Grupo no puede contenerse a sí mismo;
- RN-SEG-14 — Grupo no puede formar ciclos indirectos.

## 2. Regla de herencia

Cuando un Grupo padre contiene un Grupo hijo:

- el padre conserva sus Permisos directos;
- el padre incorpora los Permisos efectivos del hijo;
- el hijo no incorpora Permisos del padre;
- no se copian asociaciones a `GrupoPermiso`;
- la autorización se calcula mediante el patrón Composite.

Ejemplo:

```text
GERENTE
├── Permisos directos de GERENTE
└── COMERCIAL
    └── Permisos efectivos de COMERCIAL

GERENTE incorpora los Permisos de COMERCIAL.

3. Implementación
Domain

Grupo.ReemplazarGruposHijos valida:

colección obligatoria;
elementos no nulos;
ausencia de duplicados;
ausencia de autorreferencia;
ausencia de ciclos indirectos;
reemplazo atómico.
Application

GrupoGestionService permite:

listar Grupos hijos disponibles;
registrar un Grupo con hijos;
modificar y reemplazar hijos;
conservar hijos inactivos existentes;
rechazar hijos inactivos nuevos;
obtener vista previa de Permisos efectivos.
Infrastructure

GrupoGestionRepository permite:

recuperar hijos persistidos;
listar activos y seleccionados;
insertar GrupoGrupo;
reemplazar GrupoGrupo;
detectar ciclos con una consulta recursiva;
ejecutar rollback ante errores;
calcular Permisos efectivos multinivel.
WinForms

GrupoEditForm contiene:

pestaña Permisos directos;
pestaña Grupos hijos;
pestaña Permisos efectivos.

La vista previa distingue:

[Directo];
[Heredado].
4. Pruebas automatizadas

Se validaron:

reemplazo válido;
colección nula;
hijo nulo;
duplicados;
autorreferencia;
ciclo directo e indirecto;
autorización;
alta con hijos;
modificación con reemplazo;
compatibilidad con actualización anterior;
conservación de inactivos existentes;
rechazo de inactivos nuevos;
recuperación SQL de hijos;
selección de activos;
exclusión del propio Grupo;
alta transaccional;
reemplazo transaccional;
eliminación de relaciones;
rollback ante jerarquía inválida;
vista previa directa;
herencia multinivel;
eliminación de duplicados;
prioridad de origen directo;
exclusión de Grupos y Permisos inactivos.

Archivos principales:

tests/SIGEVIP.Tests/Domain/GrupoGestionDomainTests.cs;
tests/SIGEVIP.Tests/Application/GrupoJerarquiaServiceTests.cs;
tests/SIGEVIP.Tests/Integration/GrupoJerarquiaGestionRepositoryIntegrationTests.cs;
tests/SIGEVIP.Tests/Integration/GrupoPermisosEfectivosRepositoryIntegrationTests.cs.
5. Resultado automatizado
Compilación correcta.
0 Advertencia(s)
0 Errores

Pruebas totales: 627
Correcto: 627
Fallidas: 0
6. Validación manual

Se comprobó:

visualización de las tres pestañas;
obligatoriedad de al menos un Permiso directo;
selección opcional de cero Grupos hijos;
exclusión del Grupo editado;
actualización de contadores;
vista previa inmediata;
Permisos directos identificados;
Permisos heredados identificados;
eliminación de duplicados;
prioridad del origen directo;
persistencia al cerrar y reabrir;
ausencia de problemas visuales.

Resultado:

VALIDACIÓN MANUAL APROBADA

7. Limpieza

Las pruebas automatizadas eliminan:

Grupos temporales;
Permisos temporales;
UsuarioGrupo;
GrupoPermiso;
GrupoGrupo.

Antes del commit debe verificarse que no existan registros con prefijos temporales usados por las pruebas.

8. Estado

La gestión de jerarquías de Grupos se considera terminada porque incluye:

implementación;
validación de negocio;
persistencia transaccional;
interfaz;
pruebas unitarias;
pruebas de integración SQL;
validación manual;
documentación;
trazabilidad.
