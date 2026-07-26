# Pruebas del módulo Clientes

## 1. Alcance

Este documento registra las pruebas del módulo Clientes en:

- Domain;
- Application;
- Infrastructure;
- SQL Server;
- WinForms.

## 2. Resultado automatizado

Última ejecución:

- 197 pruebas totales;
- 197 correctas;
- 0 fallidas.

## 3. Compilación

Resultado:

- 0 advertencias;
- 0 errores.

Comando:

```bash
MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe"   "SIGEVIP.sln"   "/t:Rebuild"   "/p:Configuration=Debug"   "/m"4. Pruebas de Domain

Archivo:

tests/SIGEVIP.Tests/Domain/ClienteTests.cs

Cobertura:

construcción;
campos obligatorios;
normalización;
estado inicial;
activación;
desactivación;
modificación;
preservación del identificador;
preservación del estado;
reconstrucción.
5. Pruebas de Application

Archivo:

tests/SIGEVIP.Tests/Application/ClienteServiceTests.cs

Cobertura:

sesión requerida;
Usuario activo;
permiso de consulta;
permiso de gestión;
listado;
filtros;
alta;
duplicados;
modificación;
Cliente inexistente;
activación;
desactivación;
obtención.

Se utiliza:

repositorio falso;
sesión falsa para escenarios defensivos.
6. Pruebas de integración SQL

Archivo:

tests/SIGEVIP.Tests/Integration/ClienteRepositoryIntegrationTests.cs

Cobertura:

inserción;
lectura;
actualización;
activación;
desactivación;
filtros;
CUIT duplicado;
exclusión del identificador actual.

Cada prueba:

genera datos únicos;
utiliza la base SIGEVIP;
limpia los datos en finally.
7. Validación SQL

Scripts ejecutados:

003_crear_clientes.sql
002_permisos_modulo_clientes.sql
reejecución del seed
003_validar_clientes.sql

Resultados:

migración registrada;
tabla creada;
índices creados;
restricciones creadas;
CUIT duplicados: 0;
asignaciones CLIENTE_GESTIONAR: 1;
validación final: VALIDACIÓN CORRECTA.
8. Validación manual

Se verificó:

apertura desde MainForm;
grilla;
botones de gestión;
alta;
persistencia;
rechazo de CUIT duplicado;
modificación;
desactivación;
filtro de inactivos;
reactivación;
búsqueda;
cierre del módulo;
cierre de sesión;
nueva ejecución de la aplicación.

Resultado:

APROBADO

9. Criterio de aceptación

El módulo cumple el criterio de cierre porque:

compila;
pasa todas las pruebas;
persiste datos;
aplica seguridad;
maneja errores;
posee validación SQL;
posee validación manual reproducible;
conserva trazabilidad documental.
