# SIGEVIP

SIGEVIP es un sistema de escritorio para la gestión de viajes comerciales, visitas a clientes y viáticos asociados.

El proyecto se desarrolla como trabajo universitario, priorizando una solución funcional, comprobable y mantenible, sin incorporar complejidad innecesaria.

## Tecnologías

- C#
- .NET Framework 4.8
- Windows Forms
- SQL Server
- ADO.NET
- MSTest
- Git
- Visual Studio 2022

## Arquitectura

La solución utiliza una arquitectura en capas compatible con MVC adaptado a Windows Forms:

- `SIGEVIP.Domain`: entidades y reglas de negocio.
- `SIGEVIP.Application`: casos de uso y coordinación de operaciones.
- `SIGEVIP.Infrastructure`: persistencia, SQL Server y servicios técnicos.
- `SIGEVIP.WinForms`: interfaz gráfica.
- `SIGEVIP.Tests`: pruebas automatizadas.

## Estructura

SIGEVIP/
├── src/
│   ├── SIGEVIP.Domain/
│   ├── SIGEVIP.Application/
│   ├── SIGEVIP.Infrastructure/
│   └── SIGEVIP.WinForms/
├── tests/
│   └── SIGEVIP.Tests/
├── tools/
│   └── SIGEVIP.Setup/
├── database/
│   ├── migrations/
│   └── seed/
├── docs/
├── README.md
├── .gitignore
└── SIGEVIP.sln

## Compilación

Desde Git Bash:

MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" "SIGEVIP.sln" "/t:Restore;Rebuild" "/p:Configuration=Debug" "/m"

## Ejecución

Aplicación principal:

./src/SIGEVIP.WinForms/bin/Debug/SIGEVIP.WinForms.exe

Configuración inicial:

./tools/SIGEVIP.Setup/bin/Debug/SIGEVIP.Setup.exe

## Pruebas

MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"

## Estado

Se encuentran implementados:

- arquitectura base por capas;
- dominio de clientes, viajes, visitas y viáticos;
- autenticación, sesión y autorización;
- PBKDF2-HMAC-SHA256;
- persistencia SQL del modelo de seguridad;
- repositorios ADO.NET de autenticación y perfil;
- reconstrucción de grupos, permisos y jerarquías;
- creación transaccional del administrador;
- utilidad `SIGEVIP.Setup`;
- login y menú principal WinForms;
- permisos visuales por autorización efectiva;
- módulo funcional de Gestión de Usuarios;
- listado, búsqueda y filtros de Usuarios;
- alta para Personas activas sin cuenta;
- contraseña inicial protegida mediante PBKDF2;
- modificación del nombre de Usuario;
- asignación y reemplazo transaccional de Grupos directos;
- activación y desactivación lógica de Usuarios;
- protección del Usuario autenticado y del último Administrador;
- cambio seguro de contraseña por el propio Usuario;
- cierre automático de sesión después del cambio;
- módulo funcional de Gestión de Grupos;
- listado, búsqueda y filtro por estado de Grupos;
- alta y modificación de Grupos;
- generación automática e inmutabilidad del Código;
- asignación y reemplazo transaccional de Permisos directos;
- selección y reemplazo transaccional de Grupos hijos;
- detección de autorreferencia y ciclos indirectos;
- vista previa de Permisos efectivos directos y heredados;
- activación y desactivación lógica de Grupos;
- preservación de `GrupoPermiso`, `UsuarioGrupo` y `GrupoGrupo`;
- protección de `ADMINISTRADOR_GENERAL`;
- módulo funcional de Gestión de Permisos;
- listado, búsqueda y filtro por estado de Permisos;
- alta y modificación del catálogo de Permisos;
- normalización, unicidad e inmutabilidad del Código;
- activación y desactivación lógica de Permisos;
- protección de `USUARIO_GESTIONAR`, `GRUPO_GESTIONAR` y `PERMISO_GESTIONAR`;
- conservación de asociaciones `GrupoPermiso`;
- preservación de Permisos inactivos ya asignados al editar Grupos;
- módulo funcional de Clientes;
- persistencia SQL de Clientes;
- alta y modificación de Clientes;
- activación y desactivación lógica;
- listado y filtros de Clientes;
- validación de CUIT duplicado;
- módulo funcional de Viajes;
- persistencia SQL de Viajes y participantes;
- alta, modificación, consulta y cancelación de Viajes;
- filtros por fechas, estado y participante;
- selección múltiple de participantes;
- módulo funcional de Visitas;
- persistencia SQL de Visitas y asociaciones con Clientes;
- alta y consulta de Visitas por Viaje;
- selección múltiple de Clientes activos;
- reconstrucción histórica de Clientes inactivos;
- bloqueo de cancelación de Viajes con Visitas;
- módulo funcional de Viáticos y Rendiciones;
- persistencia SQL de Viáticos y Comprobantes;
- alta, modificación, consulta y filtrado de Viáticos;
- carga opcional de Comprobantes;
- selección de Personas pagadoras;
- reconstrucción completa de Viajes con Viáticos y Comprobantes;
- envío de Viajes a rendición;
- consulta de Rendiciones pendientes;
- exclusión y reactivación lógica de Viáticos;
- ajuste de anticipos;
- aprobación y cancelación de Rendiciones;
- cálculo de total gastado vigente y saldo pendiente;
- validación SQL de las migraciones `003` a `006`;
- 627 pruebas automatizadas correctas;
- compilación con 0 advertencias y 0 errores;
- validación manual de Usuarios, Cambio de clave, Grupos, jerarquías de Grupos, Permisos, Clientes, Viajes, Visitas, Viáticos y Rendiciones.

Permanecen pendientes:

- recuperación de contraseña;
- historial funcional completo del cliente;
- auditoría general consultable;
- reportes;
- mapa y geolocalización;
- datos de demostración finales;
- manual técnico y preparación de la presentación académica.
