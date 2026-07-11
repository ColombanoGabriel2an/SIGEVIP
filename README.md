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

./src/SIGEVIP.WinForms/bin/Debug/SIGEVIP.WinForms.exe

## Pruebas

MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"

## Estado

Actualmente se encuentra preparada la estructura técnica inicial. El dominio funcional será desarrollado en etapas posteriores.