# Métrica de software — Puntos de Casos de Uso

## 1. Objetivo

Estimar el tamaño y el esfuerzo de la funcionalidad core **“Cargar viáticos, enviar a rendición y aprobar viaje”** del sistema SIGEVIP mediante el método de **Puntos de Casos de Uso (Use Case Points, UCP)**.

## 2. Alcance de la estimación

La medición comprende los requerimientos RF20 a RF32:

- carga y modificación de viáticos;
- comprobante opcional y justificación;
- persona pagadora cuando corresponde;
- envío del viaje a rendición;
- revisión gerencial;
- exclusión y reactivación de gastos;
- ajuste del anticipo;
- aprobación o cancelación;
- cálculo e información del saldo final,

Se incluyen análisis, diseño, implementación por capas, persistencia ADO,NET, interfaz WinForms, pruebas y documentación de la iteración,

Se excluyen la arquitectura inicial, Clientes, Viajes y Visitas desarrollados previamente, la auditoría general agregada posteriormente, el instalador, mapas, geolocalización y servicios externos,

## 3. Consideraciones de conteo

Se cuenta un único caso de uso core complejo, Los subprocesos de registrar viáticos, enviar a rendición, revisar y aprobar no se suman de manera independiente porque forman parte del mismo flujo integral y hacerlo produciría doble conteo,

Una transacción se interpreta como una interacción completa entre actor y sistema que produce una validación, consulta o modificación observable,

## 4. Actores

| Actor          | Tipo                                                   | Peso |
|---             |---                                                     |---:  |
| Administrativo | Complejo, porque utiliza una interfaz gráfica WinForms | 3    |
| Gerente        | Complejo, porque utiliza una interfaz gráfica WinForms |    3 |
| **UAW**        |                                                        | **6**|

## 5. Caso de uso

El flujo principal y sus extensiones contienen trece transacciones relevantes, por lo que se clasifica como **complejo**,

| Caso de uso                                           | Transacciones | Complejidad | Peso |
|---                                                    |---:           |---          |---:  |
| Cargar viáticos, enviar a rendición y aprobar viaje   | 13            | Compleja    | 15   |
| **UUCW** |                                            |               |             **15** |

| N,º | Transacción                                                                       |
| --: | --------------------------------------------------------------------------------- |
|   1 | El Administrativo selecciona un Viaje en estado Abierto,                          |
|   2 | El sistema recupera y muestra el detalle del Viaje y sus Viáticos,                |
|   3 | El Administrativo inicia el registro de un Viático,                               |
|   4 | El sistema valida fecha, categoría, método de pago, importe y descripción,        |
|   5 | El sistema valida la persona pagadora cuando el método lo requiere,               |
|   6 | El sistema valida y registra el comprobante o la justificación por su ausencia,   |
|   7 | El sistema guarda el Viático y actualiza el total gastado,                        |
|   8 | El Administrativo envía el Viaje a rendición,                                     |
|   9 | El sistema valida el estado y bloquea modificaciones posteriores,                 |
|  10 | El Gerente consulta y revisa el detalle integral de la Rendición,                 |
|  11 | El Gerente excluye o reactiva Viáticos y, cuando corresponde, ajusta el anticipo, |
|  12 | El Gerente aprueba o cancela la Rendición,                                        |
|  13 | El sistema registra la auditoría, actualiza el estado y calcula el saldo final,   |


## 6. Puntos no ajustados

**UUCP = UAW + UUCW**

**UUCP = 6 + 15 = 21**

## 7. Factor técnico

La valoración técnica considera que SIGEVIP es una aplicación local y monousuario, pero posee reglas de negocio, transacciones, seguridad, auditoría, facilidad de uso y una arquitectura mantenible,

La suma ponderada de los factores técnicos es 29,

**TCF = 0,6 + (0,01 × 29) = 0,89**

## 8. Factor ambiental

La valoración ambiental considera experiencia previa en desarrollo, conocimiento del dominio, motivación alta, requisitos que experimentaron ajustes y trabajo realizado en tiempo parcial,

La suma ponderada de los factores ambientales es 11,

**ECF = 1,4 - (0,03 × 11) = 1,07**

## 9. Resultado

**UCP = UUCP × TCF × ECF**

**UCP = 21 × 0,89 × 1,07 = 20,00**

El tamaño estimado de la funcionalidad analizada es, por lo tanto, de **aproximadamente 20 Puntos de Casos de Uso**,

## 10. Estimación de esfuerzo

Como no existe una serie histórica formal de productividad propia, se utilizan escenarios:

| Escenario          | Horas por UCP | Esfuerzo     |
|---                 |---:           |---:          |
| Optimista          | 8             |  159,99 horas|
| Probable           | 10            | 199,98 horas |
| Conservador        | 15            | 299,97 horas |
| Referencia clásica | 20            | 399,97 horas |

Para el cronograma del proyecto se adopta el escenario probable, equivalente a **aproximadamente 200 horas**,

## 11. Distribución temporal del escenario probable

| Fase | Porcentaje | Horas aproximadas |
|---|---:|---:|
| Análisis y alcance | 10 % | 20,00 |
| Diseño | 15 % | 30,00 |
| Domain y Application | 20 % | 40,00 |
| Persistencia y transacciones | 20 % | 40,00 |
| WinForms e integración | 15 % | 30,00 |
| Pruebas y correcciones | 15 % | 30,00 |
| Documentación y cierre | 5 % | 10,00 |
| **Total** | **100 %** | **199,98** |

La distribución se integra en un cronograma de ocho semanas, asignando las semanas 3 y 4 a Domain y Application y una semana a cada uno de los demás hitos,

## 12. Limitaciones

- La productividad no está calibrada con registros históricos de horas,
- El resultado es una estimación preliminar y no constituye un compromiso contractual.
- La cantidad de transacciones puede ajustarse si durante la revisión metodológica se decide separar los subprocesos en varios casos de uso,
- La planilla adjunta conserva todos los valores y fórmulas editables para repetir el cálculo,

## 13. Evidencia

Archivo de cálculo:

`evidencias/metricas/SIGEVIP-UCP-Viaticos-Rendiciones.xlsx`

Las celdas amarillas contienen supuestos editables y las celdas verdes resultados calculados automáticamente,
