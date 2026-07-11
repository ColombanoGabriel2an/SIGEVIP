# Decisiones técnicas

## DT-01 — Plataforma

Se utilizará .NET Framework 4.8 por compatibilidad con el entorno académico y Windows Forms.

## DT-02 — Interfaz

La interfaz se implementará con Windows Forms.

## DT-03 — Persistencia

Se utilizará ADO.NET con SQL Server.

No se utilizará Entity Framework.

## DT-04 — Arquitectura

Se aplicará una arquitectura en capas con MVC adaptado a Windows Forms.

## DT-05 — Base de datos

La base de datos será local y reproducible mediante scripts SQL.

## DT-06 — Seguridad

La seguridad utilizará autenticación por usuario y contraseña, grupos y permisos.

El patrón Composite se aplicará a la estructura de permisos.

## DT-07 — Estados

El patrón State se aplicará a la entidad Viaje.

Estados previstos:

- Abierto.
- EnRendicion.
- Aprobado.
- Cancelado.

## DT-08 — Pruebas

Se utilizará MSTest.

Las primeras pruebas se enfocarán en reglas del dominio y servicios de aplicación.

## DT-09 — Control de versiones

Se utilizará Git con commits pequeños y descriptivos en tiempo verbal presente.

Ejemplo:

Agrego estructura base y proyecto de pruebas

## DT-10 — Alcance excluido

No se implementarán como parte principal:

 - Mapas.
 - Geolocalización.
 - Planificación automática de rutas.
 - Servicios web.
 - Arquitectura distribuida.
 - Microservicios.
