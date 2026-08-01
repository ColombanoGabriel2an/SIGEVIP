# Hoja de seguimiento del proyecto SIGEVIP

## 1. Objetivo

Registrar la evolución verificable del proyecto mediante Git y relacionar cada cambio con la iteración correspondiente, los requisitos involucrados y su evidencia principal.

## 2. Criterios

- La fecha y el identificador provienen del historial Git.
- La descripción conserva el mensaje original del commit.
- La columna de requisito utiliza los códigos disponibles en la documentación.
- “Transversal” identifica cambios técnicos que no satisfacen por sí solos un requisito funcional.
- La evidencia debe verificarse en código, SQL, interfaz, pruebas o documentación.
- Esta hoja no reemplaza la matriz de trazabilidad detallada.

## 3. Línea base actual

- Rama: `desarrollo/interfaz-funcional`.
- Commit: `5c16c08`.
- Compilación: 0 advertencias y 0 errores.
- Pruebas: 682 correctas.
- Estado del repositorio antes del bloque documental: limpio.

## 4. Seguimiento completo

| Fecha | Commit | Descripción | Iteración | Requisito / tema | Evidencia principal |
|---|---|---|---|---|---|
| 2026-02-17 | `055470b` | Create README.md | Iteración 1 | Transversal | README inicial |
| 2026-02-17 | `308371f` | Agrego .gitignore para Visual Studio | Iteración 1 | Transversal | Control de versiones |
| 2026-02-17 | `54f0df9` | Configuro la arquitectura en capas del sistema | Iteración 1 | T01 / RNF arquitectura | Solución y proyectos |
| 2026-07-11 | `32dc489` | Agrego estructura base y proyecto de pruebas | Iteración 1 | T01 / Calidad | Proyecto MSTest |
| 2026-07-11 | `ea65145` | Agrego documentación técnica inicial | Iteración 1 | RA5 / Documentación | docs iniciales |
| 2026-07-11 | `544047d` | Agrego base inicial y conexión a SQL Server | Iteración 1 | RF40-RF41 | Conexión y esquema base |
| 2026-07-11 | `90edf06` | Implemento dominio de viajes y viáticos | Iteración 1 | RF09-RF32 | Entidades de dominio |
| 2026-07-11 | `dd6915d` | Agrego dominio de clientes y visitas | Iteración 1 | RF05-RF08 / RF15-RF19 | Entidades Cliente y Visita |
| 2026-07-11 | `4ee6c1e` | Integro visitas al agregado Viaje | Iteración 1 | RF15-RF19 | Agregado Viaje |
| 2026-07-11 | `a2788b6` | Documento relaciones de visitas y clientes | Iteración 1 | RF17-RF18 | Documentación de relaciones |
| 2026-07-11 | `09f90eb` | Agrego dominio de personas y usuarios | Iteración 1 | RF01-RF04 | Persona y Usuario |
| 2026-07-11 | `b69fa36` | Implemento Composite de grupos y permisos | Iteración 1 | RF37-RF39 / T04 | Composite |
| 2026-07-11 | `ea8e92e` | Agrego autenticación y autorización | Iteración 1 | RF01-RF02 / RF37-RF39 | Servicios de seguridad |
| 2026-07-11 | `64a2161` | Incorporo hash seguro de contraseñas | Iteración 1 | RNF7-RNF9 / T03 | PBKDF2 |
| 2026-07-11 | `1464aa9` | Documento seguridad y cierre del bloque 3 | Iteración 1 | RF01-RF04 / RF37-RF39 | Documentación de seguridad |
| 2026-07-18 | `a4b631c` | Agrego esquema SQL de seguridad | Iteración 1 | RF01-RF04 / RF37-RF39 | Migración de seguridad |
| 2026-07-18 | `2e567f2` | Documento persistencia de seguridad | Iteración 1 | RF40-RF42 | Modelo de datos |
| 2026-07-23 | `cb52c7e` | Implemento repositorio de autenticación | Iteración 1 | RF01-RF02 | Repositorio SQL |
| 2026-07-23 | `514ba5a` | Agrego inicialización del administrador | Iteración 1 | RF03 / Seguridad operativa | Administrador inicial |
| 2026-07-23 | `a89c186` | Agrego herramienta de configuración inicial | Iteración 1 | Instalación / Configuración | SIGEVIP.Setup |
| 2026-07-23 | `62396ba` | Pruebo jerarquías persistidas de seguridad | Iteración 1 | RF39 / T04 | Pruebas de Composite |
| 2026-07-23 | `0e2efa6` | Documento cierre de persistencia de seguridad | Iteración 1 | RF01-RF04 / RF37-RF42 | Cierre documental |
| 2026-07-25 | `bfc8f80` | Implemento inicio de sesión WinForms | Iteración 2 | RF01-RF02 | LoginForm |
| 2026-07-26 | `263f3c7` | Implemento menu principal y cierre de sesion | Iteración 2 | RF02 / Cerrar sesión | MainForm y sesión |
| 2026-07-26 | `af860a9` | Incorporo perfil de usuario autenticado | Iteración 2 | Mi Perfil | Perfil de sesión |
| 2026-07-26 | `cc6e19e` | Aplico permisos visuales y retiro formulario tecnico | Iteración 2 | RF37-RF39 | Autorización visual |
| 2026-07-26 | `ea8dc75` | Documento cierre de autenticacion e interfaz | Iteración 2 | RF01-RF02 / RF37-RF39 | Validación de interfaz |
| 2026-07-26 | `0994a78` | Agrego casos de uso de clientes | Iteración 2 | RF05-RF08 | Application Clientes |
| 2026-07-26 | `c8a36e3` | Implemento persistencia de clientes | Iteración 2 | RF05-RF08 / RF40-RF41 | Repositorio Cliente |
| 2026-07-26 | `8f45694` | Agrego interfaz de gestion de clientes | Iteración 2 | RF05-RF08 | WinForms Clientes |
| 2026-07-26 | `ab9dd42` | Documento cierre del modulo clientes | Iteración 2 | RF05-RF08 | Documentación y pruebas |
| 2026-07-26 | `8226625` | Agrego casos de uso de viajes | Iteración 3 | RF09-RF14 | Application Viajes |
| 2026-07-26 | `6914a1d` | Implemento persistencia de viajes | Iteración 3 | RF09-RF14 / RF40-RF41 | Repositorio Viaje |
| 2026-07-26 | `88fbd13` | Agrego interfaz de gestion de viajes | Iteración 3 | RF09-RF14 | WinForms Viajes |
| 2026-07-26 | `3a59558` | Documento cierre del modulo viajes | Iteración 3 | RF09-RF14 | Documentación y pruebas |
| 2026-07-26 | `430761e` | Agrego casos de uso de visitas | Iteración 3 | RF15-RF19 | Application Visitas |
| 2026-07-26 | `c76a96e` | Creo esquema SQL de visitas | Iteración 3 | RF15-RF19 / RF41 | Migración Visitas |
| 2026-07-26 | `c6ab260` | Implemento persistencia de visitas | Iteración 3 | RF15-RF19 | Repositorio Visita |
| 2026-07-26 | `d45fb75` | Reconstruyo viajes con visitas persistidas | Iteración 3 | RF18-RF19 | Reconstrucción del agregado |
| 2026-07-26 | `67d3df2` | Agrego formularios de visitas | Iteración 3 | RF15-RF18 | WinForms Visitas |
| 2026-07-26 | `3b13f58` | Conecto modulo de visitas al menu | Iteración 3 | RF15-RF18 / RF38 | Navegación |
| 2026-07-26 | `764f6e9` | Documento cierre del modulo visitas | Iteración 3 | RF15-RF19 | Documentación y pruebas |
| 2026-07-27 | `97a409d` | Completo dominio de viaticos | Iteración 4 | RF20-RF25 | Dominio Viático y Comprobante |
| 2026-07-27 | `0fba174` | Completo aplicacion de viaticos y rendiciones | Iteración 4 | RF20-RF32 | Servicios de aplicación |
| 2026-07-27 | `300350a` | Agrego esquema SQL de viaticos y rendiciones | Iteración 4 | RF20-RF32 / RF41 | Migración |
| 2026-07-27 | `6ae9268` | Agrego consulta de personas pagadoras | Iteración 4 | RF22 | Consulta de pagadores |
| 2026-07-27 | `bef0ee3` | Agrego persistencia de viaticos | Iteración 4 | RF20-RF25 | Repositorio Viático |
| 2026-07-27 | `83582fd` | Integro viaticos en reconstruccion de viajes | Iteración 4 | RF14 / RF32 | Reconstrucción y cálculos |
| 2026-07-27 | `823afc6` | Agrego consulta y envio de rendiciones | Iteración 4 | RF26-RF27 | Envío a rendición |
| 2026-07-27 | `e73b7c5` | Completo revision de rendiciones | Iteración 4 | RF28-RF32 | Revisión gerencial |
| 2026-07-27 | `e2fdb0f` | Integro navegacion de viaticos y rendiciones | Iteración 4 | RF20-RF32 / RF38 | Navegación |
| 2026-07-27 | `acb4607` | Completo gestion de viaticos y comprobantes | Iteración 4 | RF20-RF25 | Interfaz de carga |
| 2026-07-27 | `fb3fa6e` | Completo interfaz de revision de rendiciones | Iteración 4 | RF28-RF32 | Interfaz de revisión |
| 2026-07-27 | `54f16d4` | Documento modulo de viaticos y rendiciones | Iteración 4 | RF20-RF32 | Documentación del módulo |
| 2026-07-27 | `61924b7` | Actualizo documentacion transversal de viaticos | Iteración 4 | RF20-RF32 | Trazabilidad transversal |
| 2026-07-27 | `e13d291` | Agrego casos de uso de usuarios | Iteración 5 | RF03-RF04 | Application Usuarios |
| 2026-07-27 | `ad7772c` | Agrego persistencia de usuarios | Iteración 5 | RF03-RF04 | Repositorio Usuario |
| 2026-07-27 | `9460bf9` | Completo interfaz de gestion de usuarios | Iteración 5 | RF03-RF04 | WinForms Usuarios |
| 2026-07-27 | `70b4ad6` | Documento cierre de gestion de usuarios | Iteración 5 | RF03-RF04 | Documentación y pruebas |
| 2026-07-28 | `1b920cc` | Agrego cambio seguro de clave | Iteración 5 | Cambiar clave / RNF7-RNF9 | Servicio de clave |
| 2026-07-28 | `74dec4b` | Agrego interfaz de cambio de clave | Iteración 5 | Cambiar clave | WinForms |
| 2026-07-28 | `7d1ab29` | Documento cambio de clave | Iteración 5 | Cambiar clave | Documentación y pruebas |
| 2026-07-28 | `fcbc5e2` | Agrego casos de uso de grupos | Iteración 5 | RF39 / T04 | Application Grupos |
| 2026-07-28 | `5da1e81` | Agrego persistencia de grupos | Iteración 5 | RF39 / T04 | Repositorio Grupo |
| 2026-07-28 | `90ec085` | Completo interfaz de gestion de grupos | Iteración 5 | RF39 / T04 | WinForms Grupos |
| 2026-07-28 | `d0ee2a6` | Documento cierre de gestion de grupos | Iteración 5 | RF39 / T04 | Documentación y pruebas |
| 2026-07-28 | `3e8401c` | Agrego casos de uso de permisos | Iteración 5 | RF37-RF39 / T04 | Application Permisos |
| 2026-07-28 | `d61e304` | Agrego persistencia de permisos | Iteración 5 | RF37-RF39 / T04 | Repositorio Permiso |
| 2026-07-28 | `2c16ef2` | Completo interfaz de gestion de permisos | Iteración 5 | RF37-RF39 / T04 | WinForms Permisos |
| 2026-07-28 | `599e339` | Documento cierre de gestion de permisos | Iteración 5 | RF37-RF39 / T04 | Documentación y pruebas |
| 2026-07-28 | `cd67fab` | Agrego casos de uso y persistencia de jerarquias de grupos | Iteración 5 | RF39 / T04 | GrupoGrupo y recursividad |
| 2026-07-28 | `bddc762` | Completo interfaz y permisos efectivos de jerarquias de grupos | Iteración 5 | RF39 / T04 | TreeView y permisos efectivos |
| 2026-07-28 | `7bf36f2` | Agrego base consultable de auditoria | Iteración 5 | RF42 / T06 | Auditoría general |
| 2026-07-28 | `96dfecc` | Integro auditoria transaccional en clientes | Iteración 5 | RF42 / T06 | Auditoría Cliente |
| 2026-07-28 | `0ae66eb` | Integro auditoria transaccional en usuarios | Iteración 5 | RF42 / T06 | Auditoría Usuario |
| 2026-07-28 | `b16615e` | Integro auditoria transaccional en grupos | Iteración 5 | RF42 / T06 | Auditoría Grupo |
| 2026-07-28 | `d2153a2` | Integro auditoria transaccional en permisos | Iteración 5 | RF42 / T06 | Auditoría Permiso |
| 2026-07-28 | `b449528` | Integro auditoria transaccional en viajes | Iteración 5 | RF42 / T06 | Auditoría Viaje |
| 2026-07-28 | `02639df` | Integro auditoria transaccional en visitas | Iteración 5 | RF42 / T06 | Auditoría Visita |
| 2026-07-28 | `9505296` | Integro auditoria transaccional en viaticos | Iteración 5 | RF42 / T06 | Auditoría Viático |
| 2026-07-28 | `c83e420` | Integro auditoria transaccional en envio a rendicion | Iteración 5 | RF31-RF32 / RF42 | Auditoría de envío |
| 2026-07-28 | `f6360f1` | Integro auditoria transaccional en revision de rendiciones | Iteración 5 | RF28-RF32 / RF42 | Auditoría de revisión |
| 2026-07-28 | `5c16c08` | Completo auditoria transaccional de rendiciones | Iteración 5 | RF28-RF32 / RF42 | Cierre de auditoría |

## 5. Resumen por iteración

| Iteración | Rango | Resultado |
|---|---|---|
| Iteración 1 | `055470b` a `0e2efa6` | Arquitectura, dominio, persistencia y seguridad base |
| Iteración 2 | `bfc8f80` a `ab9dd42` | Autenticación visual, sesión, permisos y Clientes |
| Iteración 3 | `8226625` a `764f6e9` | Viajes, participantes, Visitas y Clientes asociados |
| Iteración 4 | `97a409d` a `61924b7` | Viáticos, comprobantes, rendición y aprobación |
| Iteración 5 | `e13d291` a `5c16c08` | Usuarios, clave, Grupos, Permisos, Composite y auditoría |

## 6. Uso en la entrega

En el documento principal se mostrará el resumen por iteración. La tabla completa se incorporará como anexo para demostrar:

- desarrollo iterativo e incremental;
- entregas ejecutables sucesivas;
- trazabilidad temporal;
- separación de responsabilidades;
- incorporación progresiva de pruebas;
- correspondencia entre implementación y documentación.

## Actualización 2026-08-01

| Fecha | Commit | Mensaje | Iteración | Trazabilidad | Resultado |
|---|---|---|---|---|---|
| 2026-08-01 | `0874d48` | Completo consulta de auditoria y controles operativos | Iteraciones 4 y 5 / usabilidad | RF11, RF22, RF23, RF28, RF42 y controles de interfaz | Auditoría consultable, detalle de cambios, validaciones de Viáticos y 700 pruebas |

Evidencia del cierre:

- compilación sin advertencias ni errores;
- 99 pruebas específicas correctas;
- 700 pruebas de regresión correctas;
- validación manual completa;
- documentación posterior registrada en un commit separado.
