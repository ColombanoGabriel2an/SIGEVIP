\# Evidencia de control detallado de cambios de Cliente



\## 1. Identificación



\- Sistema: SIGEVIP.

\- Fecha: 2026-07-28.

\- Control: valores anteriores y nuevos.

\- Entidad: Cliente.

\- Módulo: Clientes.



\## 2. Objetivo



Comprobar que una modificación de Cliente conserve el Usuario responsable, la fecha y hora, la entidad afectada, su identificador y los valores anteriores y nuevos de cada campo efectivamente modificado.



\## 3. Diseño aplicado



Se mantiene `dbo.Auditoria` como encabezado del evento y se incorpora `dbo.AuditoriaCambio` como detalle reutilizable.



```text

Auditoria 1 ----- 0..N AuditoriaCambio

```



Cada detalle conserva:



\- `IdAuditoriaCambio`;

\- `IdAuditoria`;

\- `Campo`;

\- `ValorAnterior`;

\- `ValorNuevo`.



Este diseño evita crear una tabla diferente para cada entidad.



\## 4. Migración



Archivos:



\- `008\_crear\_detalle\_cambios\_auditoria.sql`;

\- `008\_validar\_detalle\_cambios\_auditoria.sql`;

\- `008\_revertir\_detalle\_cambios\_auditoria.sql`.



La migración incorpora:



\- clave primaria;

\- clave foránea hacia `dbo.Auditoria`;

\- eliminación en cascada;

\- restricción única por evento y campo;

\- controles de campo obligatorio;

\- controles de diferencia entre valores;

\- versión de base de datos `008`.



\## 5. Componentes



\- `AuditoriaCambioRegistro`;

\- `AuditoriaRegistro`;

\- `AuditoriaSqlWriter`;

\- `ClienteService`;

\- `dbo.AuditoriaCambio`.



\## 6. Funcionamiento



`ClienteService.Modificar`:



1\. recupera el Cliente persistido;

2\. conserva su estado anterior;

3\. valida y aplica los datos nuevos;

4\. compara cada campo;

5\. registra solamente los campos modificados;

6\. persiste el Cliente, la Auditoría y los detalles dentro de una misma transacción.



Campos controlados:



\- RazonSocial;

\- Cuit;

\- Email;

\- Telefono;

\- Localidad;

\- Provincia.



\## 7. Atomicidad



La modificación del Cliente, el encabezado de Auditoría y los valores anteriores y nuevos utilizan la misma conexión y transacción SQL.



Si falla la Auditoría, también se revierte la modificación funcional.



\## 8. Compilación



\- advertencias: 0;

\- errores: 0;

\- duración: 2,39 segundos.



\## 9. Pruebas específicas



\- pruebas totales: 6;

\- pruebas correctas: 6;

\- pruebas fallidas: 0;

\- duración: 2,7198 segundos.



Se comprobó:



\- alta auditada;

\- rollback de alta;

\- rollback de modificación;

\- activación y desactivación;

\- persistencia de valores anteriores y nuevos;

\- registro exclusivo de campos modificados.



\## 10. Regresión completa



\- pruebas totales: 690;

\- pruebas correctas: 690;

\- pruebas fallidas: 0;

\- duración: 20,8589 segundos.



\## 11. Validación SQL



El script de validación `008\_validar\_detalle\_cambios\_auditoria.sql` se ejecutó sin errores.



Las restricciones permanecen habilitadas y confiables:



\- `CK\_AuditoriaCambio\_Campo\_NoVacio`;

\- `CK\_AuditoriaCambio\_ValoresDiferentes`.



La prueba de integración recuperó desde SQL Server:



\- RazonSocial anterior y nueva;

\- Email anterior y nuevo.



\## 12. Criterios de aceptación



| Criterio | Estado |

|---|---|

| Identificar al Usuario | Cumplido |

| Registrar fecha y hora | Cumplido |

| Identificar Cliente e ID | Cumplido |

| Conservar el campo modificado | Cumplido |

| Conservar el valor anterior | Cumplido |

| Conservar el valor nuevo | Cumplido |

| Omitir campos sin cambios | Cumplido |

| Mantener atomicidad | Cumplido |

| Contar con migración reversible | Cumplido |

| Superar pruebas específicas | Cumplido |

| Superar regresión completa | Cumplido |



\## 13. Alcance pendiente



La persistencia detallada está completa para Cliente.



La visualización de estos valores dentro de una pantalla o reporte de Auditoría permanece como un incremento independiente.



\## 14. Conclusión



El control detallado de cambios de Cliente está implementado, persistido y comprobado.



Estado:



\*\*Cumplido.\*\*

