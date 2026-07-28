# Índice maestro de documentación de SIGEVIP

## 1. Objetivo

Este documento organiza la documentación técnica y funcional de SIGEVIP, identifica la función de cada archivo y evita duplicaciones. No reemplaza los documentos existentes: actúa como punto de entrada y criterio de mantenimiento.

## 2. Fuente de verdad y orden de precedencia

Ante contradicciones, se aplicará el siguiente orden:

1. Requisitos vigentes, criterios de aceptación y observaciones de las partes interesadas.
2. Documento maestro de requisitos, alcance y diseño, luego de incorporar las decisiones definitivas.
3. Código, migraciones y pruebas de la rama `desarrollo/interfaz-funcional`.
4. Documentos técnicos canónicos de `docs/`.
5. Documentos históricos, diagramas preliminares y materiales de apoyo.

La implementación no se considerará completa si no existe correspondencia entre requisitos, reglas, código, base de datos, interfaz y pruebas.

## 3. Estado técnico de referencia

- Rama: `desarrollo/interfaz-funcional`.
- Línea base: commit `5c16c08`.
- Compilación: 0 advertencias y 0 errores.
- Pruebas automatizadas: 682 correctas.
- Persistencia: SQL Server y ADO.NET.
- Arquitectura: Domain, Application, Infrastructure, WinForms, Tests y herramienta de Setup.
- Migraciones aplicadas: 001 a 007.

## 4. Clasificación de documentos

### 4.1. Documentos canónicos transversales

| Archivo | Contenido principal | Uso recomendado |
|---|---|---|
| `01-arquitectura.md` | Capas, dependencias, patrones y persistencia | Fuente técnica de arquitectura |
| `02-decisiones-tecnicas.md` | Decisiones y justificaciones | Registro de decisiones vigentes |
| `04-trazabilidad-inicial.md` | Relación inicial entre requisitos y solución | Base para la trazabilidad final |
| `05-modelo-de-dominio.md` | Entidades, atributos, relaciones y patrones | Fuente canónica del dominio |
| `06-reglas-de-negocio.md` | Reglas funcionales y técnicas | Fuente canónica de reglas |
| `08-seguridad.md` | Autenticación, autorización, sesión, grupos y permisos | Fuente canónica de seguridad |
| `09-modelo-de-datos.md` | Tablas, relaciones, migraciones y persistencia | Fuente canónica de datos |

### 4.2. Documentos históricos acumulativos

| Archivo | Tratamiento |
|---|---|
| `03-estado-del-proyecto.md` | Conservar como historial técnico. No seguir ampliándolo como documento de entrega principal. |
| `07-pruebas-del-dominio.md` | Conservar como registro acumulativo de pruebas. Las nuevas evidencias deben ir en documentos específicos. |
| `10-validacion-interfaz-seguridad.md` | Usar como evidencia de validación manual y base para el manual de usuario. |

### 4.3. Evidencia por módulo

| Módulo | Documento funcional | Documento de pruebas |
|---|---|---|
| Clientes | `11-modulo-clientes.md` | `12-pruebas-clientes.md` |
| Viajes | `13-modulo-viajes.md` | `14-pruebas-viajes.md` |
| Viáticos y Rendiciones | `15-modulo-viaticos-rendiciones.md` | `16-pruebas-viaticos-rendiciones.md` |
| Usuarios | — | `17-pruebas-gestion-usuarios.md` |
| Cambio de clave | `18-cambio-clave.md` | Incluidas en el mismo documento |
| Grupos | — | `19-pruebas-gestion-grupos.md` |
| Permisos | — | `20-pruebas-gestion-permisos.md` |
| Jerarquías de grupos | — | `21-pruebas-jerarquias-grupos.md` |

### 4.4. Documentos de cierre del proyecto

| Archivo | Finalidad |
|---|---|
| `23-hoja-seguimiento.md` | Evolución cronológica y trazabilidad de commits |
| `24-metrica-puntos-casos-uso.md` | Métrica UCP y estimación del módulo core |
| `25-matriz-cumplimiento-del-proyecto.md` | Estado de requisitos, controles y evidencias |
| `26-gestion-de-riesgos.md` | Registro, evaluación y tratamiento de riesgos |
| `27-prueba-caja-negra.md` | Prueba funcional del cálculo de saldo |
| `28-prueba-caja-blanca.md` | Análisis estructural de prevención de ciclos |
| `29-politica-backup-restauracion.md` | Política y procedimiento de continuidad |
| `22-cierre-del-proyecto.md` | Se creará después de completar los documentos de operación y cierre |

### 4.5. Evidencias externas al directorio `docs`

| Ruta | Finalidad |
|---|---|
| `evidencias/metricas/SIGEVIP-UCP-Viaticos-Rendiciones.xlsx` | Cálculo verificable de Puntos de Casos de Uso |
| `database/migrations/` | Evolución reproducible de la base de datos |
| `database/seeds/` o ruta equivalente | Datos iniciales y de demostración |
| `tests/SIGEVIP.Tests/` | Evidencia automatizada de calidad |
| `src/` | Implementación funcional |
| `tools/SIGEVIP.Setup/` | Inicialización técnica del sistema |

## 5. Iteraciones consolidadas del proyecto

### Iteración 1 — Arquitectura, dominio y seguridad

Incluye arquitectura, modelo de dominio, State, Composite, autenticación, autorización, hash PBKDF2 y persistencia inicial de seguridad.

### Iteración 2 — Interfaz y Clientes

Incluye inicio y cierre de sesión, perfil, menú por permisos y gestión completa de Clientes.

### Iteración 3 — Viajes y Visitas

Incluye Viajes, participantes, State, Visitas y asociación múltiple con Clientes.

### Iteración 4 — Viáticos y Rendiciones

Incluye Viáticos, comprobantes, envío a rendición, revisión, exclusión, reactivación, ajuste del anticipo, aprobación, cancelación y saldo.

### Iteración 5 — Administración de seguridad y Auditoría

Incluye Usuarios, cambio de clave, Grupos, Permisos, jerarquías Composite, permisos efectivos y auditoría transaccional.

## 6. Documentación aún necesaria para el cierre

- Reporte de valor por iteración.
- Instructivo breve por iteración.
- Diagramas UML y DER balanceados con el código actual.
- Manual de instalación.
- Manual de usuario.
- Evidencias de demostración.
- Documento maestro consolidado y PDF final.

## 7. Política de mantenimiento

1. No repetir descripciones extensas en varios archivos.
2. Referenciar el documento canónico correspondiente.
3. Registrar decisiones nuevas en `02-decisiones-tecnicas.md`.
4. Registrar reglas nuevas en `06-reglas-de-negocio.md`.
5. Registrar cambios de datos en `09-modelo-de-datos.md`.
6. Crear documentos específicos para métricas, riesgos, pruebas formales y backup.
7. Actualizar este índice cuando se agregue, elimine o reemplace documentación.
8. No declarar un requisito completo sin implementación, validación y prueba reproducible.
