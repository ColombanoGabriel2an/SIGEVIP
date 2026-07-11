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
- Estados y transiciones del dominio.
- Excepciones de reglas de negocio.
- Patrón State aplicado a Viaje.
- Componentes de seguridad del patrón Composite, en una etapa posterior.

No depende de otras capas.

### SIGEVIP.Application

Contiene:

- Casos de uso.
- Servicios de aplicación.
- Validaciones de coordinación.
- Autorización.
- Interfaces de repositorios.
- Modelos de entrada y salida.

Depende únicamente de `SIGEVIP.Domain`.

### SIGEVIP.Infrastructure

Contiene:

- Implementaciones ADO.NET.
- Acceso a SQL Server.
- Repositorios.
- Configuración de conexión.
- Transacciones.
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
       └── Domain

`SIGEVIP.Domain` no posee referencias hacia Application, Infrastructure, WinForms ni Tests.

## 5. Dominio implementado en el primer bloque

El agregado principal es `Viaje`.

`Viaje` controla:

- Sus fechas.
- Su tipo.
- El monto anticipado.
- Su estado actual.
- Su colección de viáticos.
- El total gastado.
- El saldo pendiente.
- Las transiciones de estado.
- La incorporación y exclusión lógica de viáticos.

La colección interna utiliza `List<Viatico>`, pero se expone como `IReadOnlyCollection<Viatico>` para impedir modificaciones libres desde el exterior.

## 6. Patrón State

El patrón State se aplica al ciclo de vida de `Viaje`.

La interfaz `IEstadoViaje` define las operaciones:

- Validar modificación.
- Enviar a rendición.
- Aprobar.
- Cancelar.

Implementaciones:

- `EstadoViajeAbierto`
- `EstadoViajeEnRendicion`
- `EstadoViajeAprobado`
- `EstadoViajeCancelado`

El estado persistible se representa mediante el enum `EstadoViaje`.

`EstadoViajeFactory` permite reconstruir el objeto State desde ese valor persistible. Los objetos State no se almacenarán directamente en SQL Server.

## 7. Persistencia

La persistencia se implementará mediante ADO.NET y SQL Server.

No se utilizará Entity Framework.

La implementación de repositorios y migraciones pertenece a un bloque posterior.

## 8. Principios aplicados

- Separación de responsabilidades.
- Bajo acoplamiento.
- Alta cohesión.
- Encapsulamiento de colecciones.
- Validación explícita.
- Excepciones específicas del dominio.
- Gestión centralizada de errores en capas superiores.
- Uso de variables de configuración.
- No almacenar secretos en el repositorio.
- Desarrollo incremental y ejecutable.
