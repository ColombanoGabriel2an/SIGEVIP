# Gestión de riesgos del proyecto SIGEVIP

## 1. Objetivo

Identificar, evaluar, tratar y controlar los riesgos que pueden afectar la liberación, la ejecución, la integridad de la información y la presentación del sistema SIGEVIP.

Este documento adopta un registro de seguimiento y gestión de riesgos apropiado para un proyecto con un responsable técnico principal.

## 2. Alcance

La evaluación comprende:

- documentación funcional y técnica;
- código fuente;
- base de datos SQL Server;
- autenticación y autorización;
- ejecución durante demostraciones y revisiones;
- cronograma de cierre;
- pruebas;
- respaldos;
- evidencias;
- dependencia del responsable único.

La presente evaluación se limita a riesgos técnicos, operativos, de calidad y continuidad. Los riesgos comerciales, contractuales y regulatorios deberán evaluarse antes de una implantación organizacional.

## 3. Método de evaluación

### 3.1. Probabilidad

| Valor | Nivel | Criterio |
|---:|---|---|
| 1 | Rara | Es excepcional y no existen antecedentes cercanos |
| 2 | Improbable | Puede ocurrir, pero no se espera durante el cierre |
| 3 | Posible | Podría ocurrir si no se aplican controles |
| 4 | Probable | Existen condiciones concretas que favorecen su ocurrencia |
| 5 | Casi segura | Ya ocurrió o es muy probable durante el proyecto |

### 3.2. Impacto

| Valor | Nivel | Criterio |
|---:|---|---|
| 1 | Insignificante | No afecta la entrega ni la ejecución |
| 2 | Menor | Produce una corrección breve |
| 3 | Moderado | Retrasa una actividad o degrada una evidencia |
| 4 | Alto | Compromete un módulo, una evidencia o una parte de la presentación del sistema |
| 5 | Crítico | Puede impedir la entrega, la ejecución o la recuperación de datos |

### 3.3. Exposición

**Exposición = Probabilidad × Impacto**

| Resultado | Nivel | Tratamiento |
|---:|---|---|
| 1 a 4 | Bajo | Aceptar y observar |
| 5 a 9 | Medio | Prevenir y revisar |
| 10 a 16 | Alto | Tratar antes de la entrega |
| 17 a 25 | Crítico | Acción inmediata y seguimiento frecuente |

## 4. Registro de riesgos

| ID | Riesgo | Categoría | P | I | Exposición | Nivel | Prevención | Contingencia | Responsable | Estado |
|---|---|---|---:|---:|---:|---|---|---|---|---|
| R01 | Documentación desalineada con el sistema implementado | Calidad | 5 | 5 | 25 | Crítico | Matriz de cumplimiento, índice documental y trazabilidad | Corregir el documento maestro y retirar artefactos históricos de la versión final | Responsable del proyecto | En tratamiento |
| R02 | Demora que impida completar los entregables del proyecto | Proyecto | 4 | 5 | 20 | Crítico | Priorización por obligatoriedad y trabajo incremental | Reducir mejoras opcionales y entregar el MVP documentado | Responsable del proyecto | En tratamiento |
| R03 | Dependencia de una sola persona para análisis, código y documentación | Recursos | 5 | 4 | 20 | Crítico | Instructivos, Git, scripts y documentación reproducible | Usar README, evidencias y procedimientos para reconstruir el trabajo | Responsable del proyecto | Mitigado parcialmente |
| R04 | Diagramas UML o DER desactualizados | Calidad | 4 | 4 | 16 | Alto | Generar diagramas desde el modelo definitivo | Exponer únicamente diagramas validados contra código y SQL | Responsable del proyecto | Pendiente |
| R05 | SQL Server no disponible durante una demostración o puesta en marcha | Técnico | 3 | 5 | 15 | Alto | Validación previa, servicio iniciado y prueba de conexión | Restaurar una copia local verificada | Administrador técnico | Mitigado parcialmente |
| R06 | Backup existente pero no restaurable | Continuidad | 3 | 5 | 15 | Alto | `RESTORE VERIFYONLY` y restauración de prueba | Restaurar desde una copia anterior o recrear con migraciones y seeds | Administrador técnico | Mitigado |
| R07 | Crecimiento no controlado del alcance | Proyecto | 4 | 4 | 16 | Alto | Separar MVP, versión completa y mejoras futuras | Congelar funcionalidades y documentar exclusiones | Responsable del proyecto | Mitigado |
| R08 | Datos de demostración insuficientes o incoherentes | Demostración | 3 | 4 | 12 | Alto | Seed idempotente `006_datos_demostracion.sql` y datos representativos | Reejecutar el seed y validar conteos antes de la demostración | Responsable del proyecto | Mitigado |
| R09 | Pérdida o corrupción de la base SIGEVIP | Datos | 2 | 5 | 10 | Alto | Backup completo semanal y antes de cambios críticos | Restaurar el último backup verificado | Administrador técnico | Mitigado |
| R10 | Pérdida del acceso administrativo | Seguridad | 2 | 5 | 10 | Alto | Mantener al menos un administrador activo y documentar Setup | Ejecutar la inicialización técnica controlada | Administrador técnico | Mitigado |
| R11 | Regresión producida por cambios de cierre | Calidad | 2 | 5 | 10 | Alto | Commits pequeños, compilación y 739 pruebas | Revertir el commit defectuoso y repetir validación | Responsable del proyecto | Mitigado |
| R12 | Configuración de conexión dependiente del equipo `Lenovo_Gabi` | Técnico | 3 | 3 | 9 | Medio | Documentar la cadena y el servidor requerido | Modificar únicamente `App.config` en el nuevo entorno | Administrador técnico | Aceptado para MVP |
| R13 | Exposición de datos o hashes contenidos en una copia `.bak` | Seguridad | 2 | 5 | 10 | Alto | Restringir acceso y no subir backups al repositorio público | Eliminar copias expuestas y generar nuevas credenciales | Administrador técnico | Pendiente |
| R14 | Evidencia insuficiente de caja negra, caja blanca o métricas | Calidad | 3 | 4 | 12 | Alto | Documentos específicos, pruebas reproducibles y planilla UCP | Incorporar anexos técnicos en la presentación | Responsable del proyecto | Mitigado |

## 5. Priorización inmediata

Los riesgos que deben tratarse antes de reorganizar el documento final son:

1. R01 — documentación desalineada;
2. R02 — demora de cierre;
3. R04 — diagramas desactualizados;
4. R05 — indisponibilidad de SQL Server;
5. R06 — restauración no comprobada;
6. R14 — evidencia técnica insuficiente.

## 6. Plan de seguimiento

| Momento | Control |
|---|---|
| Al finalizar cada bloque documental | Revisar alcance, estado Git y pendientes |
| Antes de modificar código | Confirmar la necesidad funcional o técnica y revisar el contenido actual |
| Antes de cada commit | Ejecutar `git diff --check` y revisar archivos afectados |
| Después de modificar código o SQL | Compilar y ejecutar las pruebas pertinentes |
| Antes de la liberación | Ejecutar las 739 pruebas, realizar backup y probar restauración |
| Antes de una demostración o liberación | Ejecutar el guion completo con datos representativos |
| Después de la entrega | Conservar código, documentación, PDF, planilla y backup verificado |

## 7. Criterio de aceptación

La gestión de riesgos se considera implementada cuando:

- existe el registro actualizado;
- los riesgos críticos y altos tienen respuesta;
- el backup fue verificado;
- la restauración fue probada;
- el sistema compila;
- las pruebas se ejecutan correctamente;
- existe un plan alternativo para la demostración.


## 8. Tratamiento verificado al 28/07/2026

Se comprobaron las siguientes medidas:

- backup completo de `SIGEVIP`;
- verificación mediante `RESTORE VERIFYONLY`;
- restauración independiente en `SIGEVIP_RESTORE_TEST`;
- validación de migraciones 001 a 007;
- comprobación de las tablas principales;
- ejecución de `DBCC CHECKDB` sin errores informados;
- seis pruebas correctas de caja negra;
- seis pruebas correctas de caja blanca;
- cálculo UCP documentado y reproducible.

Los riesgos R06, R09 y R14 quedan mitigados. R05 permanece con riesgo residual porque la disponibilidad del servicio depende del entorno operativo.
