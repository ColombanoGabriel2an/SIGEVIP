# Matriz de cumplimiento del proyecto SIGEVIP

## 1. Objetivo

Comparar los requisitos, los criterios de aceptación, la documentación de referencia y la evidencia real del proyecto para determinar qué está completo, qué requiere documentación y qué exige implementación adicional.

## 2. Convenciones

- **Cumplido:** existe implementación, validación y evidencia reproducible.
- **Parcial:** existe una parte técnica o documental, pero falta cerrar la trazabilidad o la evidencia.
- **Faltante:** no se identificó una evidencia suficiente.
- **A verificar:** el historial disponible no permite afirmarlo sin revisar el código o la base.
- **Fuera del alcance actual:** se posterga de forma explícita y justificada.

## 3. Requisitos generales del proyecto

| Punto | Estado | Evidencia actual | Faltante / contradicción | Acción |
|---|---|---|---|---|
| Nombre y sigla | Parcial | Word y documentos técnicos | Existen nombres históricos diferentes | Unificar denominación definitiva |
| Descripción de máximo 200 palabras | Parcial | Word actual | Debe actualizarse al sistema implementado | Reescribir resumen ejecutivo |
| Objetivo general y específicos | Parcial | Word actual | Deben ser medibles y no describir funciones | Ajustar redacción |
| Alcance: inclusiones y exclusiones | Parcial | Word y decisiones técnicas | El alcance real creció respecto de febrero | Actualizar sin incorporar mejoras futuras |
| Registro de interesados | Parcial | Word actual | Revisar roles y expectativas | Actualizar tabla |
| Cronograma de hitos | Parcial | Word y Git | No refleja los incrementos reales | Integrar cinco iteraciones y UCP |
| Criterios de aceptación | Parcial | Word y pruebas | Deben vincularse con evidencia | Añadir referencia a pruebas |
| Supuestos | Parcial | Word actual | Algunos supuestos técnicos cambiaron | Revisar monousuario, SQL Server y uso local |
| Restricciones | Parcial | Word actual | Actualizar tecnología y exclusiones | Consolidar restricciones definitivas |
| Definición de requerimientos | Parcial | RF01-RF42 y documentos | Requiere balance final con implementación | Completar matriz de trazabilidad |
| Iteraciones core y artefactos | Parcial | Viajes/Visitas y Viáticos/Rendiciones | Diagramas y textos históricos desactualizados | Actualizar artefactos definitivos |
| Hoja de seguimiento | Cumplido documentalmente | `23-hoja-seguimiento.md` | Falta incorporarla al Word/anexo | Integrar en entrega |

## 4. Ingeniería de software y controles técnicos

| Requisito o control | Estado | Evidencia | Faltante real | Prioridad |
|---|---|---|---|---|
| Arquitectura mínima de cuatro capas | Cumplido | Domain, Application, Infrastructure, WinForms | Documentar componentes y dependencias | Alta |
| Persistencia sin ORM | Cumplido | ADO.NET y SQL Server | Incorporar explicación resumida | Media |
| Módulo de seguridad | Cumplido | Login, sesión, PBKDF2, Usuarios, Grupos y Permisos | Consolidar capítulo técnico | Alta |
| Persona y Usuario | Cumplido con una decisión distinta de la especificación inicial | Asociación 1 a 0..1 | Explicar por qué no se utilizó herencia | Alta |
| Patrón Composite | Cumplido | `IPermisoComponente`, Grupo, Permiso y GrupoGrupo | Actualizar diagrama y mostrar recursividad | Alta |
| Patrón State | Cumplido | Estados de Viaje y transiciones | La documentación histórica conserva versiones aplicadas a Viático | Alta |
| Métrica de software | Cumplido | UCP, proceso de conteo y planilla verificable | Sin faltantes para el alcance medido | Alta |
| Estimación de tiempos | Cumplido | Escenarios y distribución por hitos | Incorporar el resumen al documento maestro | Alta |
| Gestión de riesgos | Cumplido | Matriz, exposición, tratamiento y seguimiento | Mantener actualizado el estado residual | Alta |
| Bitácora consultable | Cumplido | Auditoría general y filtros | Documentar búsqueda combinada | Alta |
| Auditoría de operaciones | Cumplido ampliamente | Escritura transaccional por entidad | Incorporar alcance y limitaciones | Alta |
| Auditoría de login/logout | Cumplido | Eventos `InicioSesion` y `CierreSesion`, 6 pruebas específicas, regresión 688/688 y evidencia SQL | Sin faltantes para el alcance definido | Alta |
| Control de cambios de una entidad | Cumplido | `AuditoriaCambio` conserva campo, valor anterior y valor nuevo para Cliente; pruebas 6/6 y regresión 690/690 | La visualización pertenece al reporte de Auditoría | Alta |
| Tabla paralela por entidad | Cumplido mediante diseño genérico | `AuditoriaCambio` complementa `Auditoria` para Cliente y es reutilizable | Sin faltantes para el control exigido | Media |
| Reporte por iteración | Parcial | Listados y filtros existentes | Definir valor y usuario beneficiado | Alta |
| Instructivo por iteración | Parcial | Documentos de validación | Consolidar manual breve | Alta |
| Prueba de caja negra | Cumplido | Casos, particiones y 6 pruebas correctas | Incorporar evidencia al anexo técnico | Alta |
| Prueba de caja blanca | Cumplido | Grafo, complejidad ciclomática, caminos y 6 pruebas correctas | Incorporar evidencia al anexo técnico | Alta |
| Backup | Cumplido | Backup completo con CHECKSUM y RESTORE VERIFYONLY | Mantener frecuencia y retención | Alta |
| Restauración probada | Cumplido | SIGEVIP_RESTORE_TEST, DBCC CHECKDB y versiones 001 a 007 | Repetir antes de cada liberación principal | Alta |
| Pruebas unitarias | Cumplido ampliamente | MSTest | Preparar resumen y captura | Media |
| Pruebas de integración SQL | Cumplido | Tests de repositorios | Preparar resumen y captura | Media |
| Regresión | Cumplido | 682/682 | Registrar línea base y fecha | Media |
| Manejo de errores | Cumplido técnicamente | Excepciones de negocio y persistencia | Resumir estrategia | Media |
| Control de acceso por permisos | Cumplido | Autorización de aplicación y visual | Incorporar demostración | Alta |
| Hash no reversible | Cumplido | PBKDF2 | Documentar parámetros y sal | Alta |
| Árbol de permisos | Cumplido | Jerarquías y permisos efectivos | Incorporar captura/diagrama | Alta |

## 5. Requisitos funcionales por módulo

| Módulo | Requisitos principales | Estado | Evidencia |
|---|---|---|---|
| Autenticación y acceso | RF01, RF02, RF37-RF39 | Cumplido | Login, sesión, autorización y permisos visuales |
| Usuarios | RF03, RF04 | Cumplido | Alta, modificación, estado y grupos |
| Clientes | RF05-RF08 | Cumplido | ABM lógico, filtros y pruebas |
| Viajes | RF09-RF14 | Cumplido | Registro, participantes, filtros, modificación y cancelación |
| Visitas | RF15-RF19 | Cumplido | Registro dentro de Viaje y relación N:N con Clientes |
| Viáticos | RF20-RF25 | Cumplido | Registro, modificación, comprobante y pagador |
| Rendiciones | RF26-RF32 | Cumplido | Envío, revisión, exclusión, reactivación, aprobación y saldo |
| Consultas y reportes | RF33-RF36 | Parcial | Existen listados y filtros, falta consolidar reportes de valor |
| Persistencia | RF40-RF41 | Cumplido | SQL Server, migraciones e integridad |
| Auditoría | RF42 | Cumplido con alcance a documentar | Auditoría general y transaccional |

## 6. Contradicciones que deben resolverse en la documentación

| Tema | Versión histórica | Versión definitiva recomendada |
|---|---|---|
| Nombre | Gestión de Visitas a Productores | Gestión de Viajes y Viáticos con Registro de Visitas |
| Arquitectura | MVC estricto | Arquitectura por capas con coordinación de interfaz |
| Persona–Usuario | Herencia | Asociación 1 a 0..1 |
| State | Aplicado a Viático | Aplicado al ciclo de vida de Viaje |
| Viático | Asociado a Visita | Asociado directamente a Viaje |
| Visita–Cliente | Uno a muchos | Muchos a muchos |
| Seguridad | Rol directo | Grupos, Permisos y Composite |
| Aprobación | Viático individual | Viaje completo |
| Eliminación de gasto | Eliminación física | Exclusión lógica y reactivación |
| Actor de carga | Comercial | Administrativo |
| Actor aprobador | Administrador | Gerente |
| Comprobante | Archivo obligatorio | Datos opcionales; justificación si no existe |
| Saldo positivo | Devolución a empresa | Reintegro a favor del participante según fórmula vigente |
| Auditoría | Tabla paralela literal por entidad | Auditoría centralizada transaccional; justificar o complementar |

## 7. Plan de acciones para la entrega

### Prioridad 1 — cierre obligatorio del proyecto


1. Definir el reporte de valor por iteración.
2. Crear el instructivo breve por iteración.
3. Actualizar los diagramas UML y DER.
4. Consolidar el documento maestro del proyecto.
5. Preparar el PDF final y las evidencias.

### Prioridad 2 — cierre técnico recomendable

1. Datos definitivos de demostración.
2. Manual de instalación.
3. Manual de usuario.
4. Capturas de pruebas, seguridad, Composite, State y auditoría.
5. Revisión de consistencia entre RF, código, SQL, interfaz y pruebas.

### Fuera del alcance inmediato

- Recuperación por correo electrónico.
- Geolocalización y mapas.
- Multilenguaje.
- Dígitos verificadores.
- Reportes gráficos complejos.
- Aplicación web o servicios externos.
- Instalador completamente automatizado, salvo que sea requerido para el entorno de implantación.

## 8. Criterio de cierre

Un punto se marcará como terminado únicamente cuando exista:

1. definición documentada;
2. implementación, cuando corresponda;
3. validación;
4. prueba reproducible;
5. evidencia incorporable a la entrega.
