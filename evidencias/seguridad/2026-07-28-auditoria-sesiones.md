# Evidencia de auditoría de sesiones

## 1. Identificación

- Sistema: SIGEVIP.
- Fecha de validación: 2026-07-28.
- Control: auditoría persistente de inicio y cierre de sesión.
- Módulo: Seguridad.
- Entidad auditada: Sesion.

## 2. Objetivo

Comprobar que SIGEVIP registre de forma persistente el inicio y el cierre de una sesión válida, identificando al Usuario responsable y sin almacenar credenciales ni otros secretos.

## 3. Alcance implementado

Se registran los siguientes eventos:

| Evento | Módulo | Acción | Entidad | Identificador |
|---|---|---|---|---|
| Inicio de sesión correcto | Seguridad | InicioSesion | Sesion | IdUsuario |
| Cierre explícito de sesión | Seguridad | CierreSesion | Sesion | IdUsuario |
| Salida con sesión activa | Seguridad | CierreSesion | Sesion | IdUsuario |
| Cierre posterior al cambio de clave | Seguridad | CierreSesion | Sesion | IdUsuario |

Los intentos fallidos de autenticación no se registran en este incremento.

La tabla `dbo.Auditoria` exige un `IdUsuario` válido mediante clave foránea. Registrar intentos correspondientes a nombres inexistentes requeriría ampliar el modelo actual.

## 4. Componentes implementados

### Application

- `ISesionAuditoriaRepository`
- `SesionAuditoriaService`

Responsabilidades:

- comprobar que exista una sesión autenticada;
- obtener el Usuario actual;
- construir el evento de auditoría;
- registrar `InicioSesion`;
- registrar `CierreSesion`;
- excluir contraseñas, hashes, salts e iteraciones.

### Infrastructure

- `SesionAuditoriaRepository`
- reutilización de `AuditoriaSqlWriter`

Responsabilidades:

- abrir la conexión;
- iniciar una transacción;
- insertar el evento;
- confirmar la transacción;
- ejecutar rollback ante un error.

### WinForms

- `SigevipApplicationContext`
- composición desde `Program`

El inicio de sesión se registra después de:

1. validar las credenciales;
2. establecer `SesionActual`;
3. recuperar correctamente el perfil asociado.

El menú principal solo se abre cuando la auditoría de inicio fue persistida correctamente.

El cierre de sesión:

1. intenta registrar `CierreSesion`;
2. limpia siempre `SesionActual`;
3. vuelve al formulario de acceso o finaliza la aplicación;
4. evita mantener una sesión abierta cuando falla la auditoría;
5. evita eventos duplicados durante la finalización del hilo de interfaz.

## 5. Archivos incorporados

- `src/SIGEVIP.Application/Security/ISesionAuditoriaRepository.cs`
- `src/SIGEVIP.Application/Security/SesionAuditoriaService.cs`
- `src/SIGEVIP.Infrastructure/Security/SesionAuditoriaRepository.cs`
- `tests/SIGEVIP.Tests/Application/SesionAuditoriaServiceTests.cs`
- `tests/SIGEVIP.Tests/Integration/SesionAuditoriaRepositoryIntegrationTests.cs`

## 6. Archivos modificados

- `src/SIGEVIP.Application/SIGEVIP.Application.csproj`
- `src/SIGEVIP.Infrastructure/SIGEVIP.Infrastructure.csproj`
- `src/SIGEVIP.WinForms/Navigation/SigevipApplicationContext.cs`
- `src/SIGEVIP.WinForms/Program.cs`
- `tests/SIGEVIP.Tests/SIGEVIP.Tests.csproj`

## 7. Compilación

La solución completa se reconstruyó mediante MSBuild de Visual Studio 2022.

Resultado:

| Control | Resultado |
|---|---|
| Advertencias | 0 |
| Errores | 0 |
| Estado | Correcto |

## 8. Pruebas específicas

Se ejecutaron cinco pruebas unitarias y una prueba de integración real con SQL Server.

| Prueba | Resultado |
|---|---|
| Inicio con sesión activa registra evento | Correcta |
| Cierre con sesión activa registra evento | Correcta |
| El inicio no expone credenciales | Correcta |
| Inicio sin sesión es rechazado | Correcta |
| Cierre sin sesión es rechazado | Correcta |
| Inicio y cierre persisten dos eventos en SQL Server | Correcta |

Resultado:

- pruebas totales: 6;
- pruebas correctas: 6;
- pruebas fallidas: 0;
- duración: 4,7957 segundos.

## 9. Regresión completa

Se ejecutó la totalidad de las pruebas de SIGEVIP.

Resultado:

- pruebas totales: 688;
- pruebas correctas: 688;
- pruebas fallidas: 0;
- duración: 19,2903 segundos.

## 10. Validación manual

Se ejecutó la aplicación compilada y se realizó la siguiente secuencia:

1. iniciar sesión con un Usuario válido;
2. abrir el menú principal;
3. cerrar la sesión;
4. regresar al formulario de acceso;
5. iniciar una nueva sesión;
6. salir de la aplicación.

Resultado:

- el primer inicio abrió el menú;
- el cierre regresó al formulario de acceso;
- el segundo inicio funcionó correctamente;
- la salida finalizó la aplicación;
- no se mostraron errores de auditoría.

## 11. Registros persistidos

La comprobación sobre `dbo.Auditoria` devolvió:

| IdAuditoria | FechaHora | IdUsuario | NombreUsuario | Acción | IdEntidad |
|---:|---|---:|---|---|---:|
| 645 | 2026-07-28 16:19:33 | 33 | gabriel | InicioSesion | 33 |
| 646 | 2026-07-28 16:19:41 | 33 | gabriel | CierreSesion | 33 |
| 647 | 2026-07-28 16:19:52 | 33 | gabriel | InicioSesion | 33 |
| 648 | 2026-07-28 16:19:55 | 33 | gabriel | CierreSesion | 33 |

En todos los eventos:

- `Modulo` es `Seguridad`;
- `Entidad` es `Sesion`;
- `IdEntidad` coincide con `IdUsuario`;
- la descripción identifica al Usuario;
- no se almacenan credenciales.

## 12. Consulta de comprobación

```sql
USE SIGEVIP;
GO

SELECT TOP (20)
    IdAuditoria,
    FechaHora,
    IdUsuario,
    NombreUsuario,
    Modulo,
    Accion,
    Entidad,
    IdEntidad,
    Descripcion
FROM dbo.Auditoria
WHERE
    Modulo = N'Seguridad'
    AND Entidad = N'Sesion'
    AND Accion IN
    (
        N'InicioSesion',
        N'CierreSesion'
    )
ORDER BY
    FechaHora DESC,
    IdAuditoria DESC;
GO
```

## 13. Control de información sensible

Consulta ejecutada:

```sql
USE SIGEVIP;
GO

SELECT
    COUNT(*) AS RegistrosSensibles
FROM dbo.Auditoria
WHERE
    Modulo = N'Seguridad'
    AND Entidad = N'Sesion'
    AND
    (
        Descripcion LIKE N'%password%'
        OR Descripcion LIKE N'%contraseña%'
        OR Descripcion LIKE N'%hash%'
        OR Descripcion LIKE N'%salt%'
    );
GO
```

Resultado:

```text
RegistrosSensibles
0
```

## 14. Criterios de aceptación

| Criterio | Estado |
|---|---|
| Registrar inicio de sesión exitoso | Cumplido |
| Registrar cierre de sesión | Cumplido |
| Identificar al Usuario responsable | Cumplido |
| Registrar fecha y hora | Cumplido |
| No almacenar credenciales | Cumplido |
| Persistir en SQL Server | Cumplido |
| Integrarse al flujo real de interfaz | Cumplido |
| Contar con pruebas automatizadas | Cumplido |
| Superar la regresión completa | Cumplido |

## 15. Conclusión

El control de auditoría de inicio y cierre de sesión se encuentra implementado, integrado, persistido y comprobado.

Estado final:

**Cumplido.**