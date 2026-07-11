# Arquitectura de SIGEVIP

## 1. Objetivo

Definir una estructura técnica simple, mantenible y adecuada para una aplicación universitaria de escritorio.

## 2. Estilo arquitectónico

SIGEVIP utiliza una arquitectura en capas con separación de responsabilidades y una adaptación del patrón MVC para Windows Forms.

## 3. Capas

### SIGEVIP.Domain

Contiene:

- Entidades del negocio.
- Reglas de negocio.
- Enumeraciones.
- Interfaces del dominio.
- Patrón State aplicado a Viaje.
- Componentes de seguridad del patrón Composite.

No debe depender de otras capas.

### SIGEVIP.Application

Contiene:

- Casos de uso.
- Servicios de aplicación.
- Validaciones de coordinación.
- Interfaces de repositorios.
- Modelos de entrada y salida.

Depende únicamente de `SIGEVIP.Domain`.

### SIGEVIP.Infrastructure

Contiene:

- Implementaciones ADO.NET.
- Acceso a SQL Server.
- Repositorios.
- Configuración de conexión.
- Auditoría técnica.
- Servicios externos o de infraestructura.

Depende de `SIGEVIP.Domain` y `SIGEVIP.Application`.

### SIGEVIP.WinForms

Contiene:

- Formularios.
- Controles visuales.
- Controladores adaptados a Windows Forms.
- Navegación.
- Presentación de errores y resultados.

Depende de `SIGEVIP.Domain`, `SIGEVIP.Application` y `SIGEVIP.Infrastructure`.

### SIGEVIP.Tests

Contiene:

- Pruebas unitarias.
- Pruebas de reglas del dominio.
- Pruebas de servicios de aplicación.
- Pruebas de integración prioritarias.

Depende inicialmente de `SIGEVIP.Domain` y `SIGEVIP.Application`.

## 4. Dirección de dependencias

WinForms
   ├── Application
   ├── Domain
   └── Infrastructure
          ├── Application
          └── Domain

Tests
   ├── Application
   └── Domain>>>EOF

## 5. Persistencia

La persistencia se implementará mediante ADO.NET y SQL Server.

No se utilizará Entity Framework.

## 6. Principios

 - Separación de responsabilidades.
 - Bajo acoplamiento.
 - Alta cohesión.
 - Validación explícita.
 - Gestión centralizada de errores.
 - Uso de variables de configuración.
 - No almacenar secretos en el repositorio.
 - Desarrollo incremental y ejecutable.