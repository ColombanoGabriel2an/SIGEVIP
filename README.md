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
- módulo funcional de Clientes;
- persistencia SQL de Clientes;
- alta y modificación de Clientes;
- activación y desactivación lógica;
- listado y filtros;
- validación de CUIT duplicado;
- 197 pruebas automatizadas correctas.

Permanecen pendientes:

- gestión funcional de usuarios, grupos y permisos;
- persistencia de viajes, visitas y viáticos;
- historial funcional del cliente;
- auditoría;
- reportes;
- mapa y geolocalización;
- datos de demostración finales;
- manual técnico y preparación de la presentación académica.
