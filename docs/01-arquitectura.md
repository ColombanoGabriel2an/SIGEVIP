# Arquitectura de SIGEVIP

## 1. Objetivo

Definir una estructura técnica simple, mantenible y adecuada para una aplicación universitaria de escritorio.

## 2. Estilo arquitectónico

SIGEVIP utiliza una arquitectura en capas con separación de responsabilidades y una adaptación del patrón MVC para Windows Forms.

La solución evita microservicios y dependencias innecesarias. Cada capa contiene responsabilidades concretas y mantiene una dirección de dependencias controlada.

## 3. Capas

### SIGEVIP.Domain

Contiene:

- Entidades del negocio.
- Reglas de negocio.
- Enumeraciones.
- Estados y transiciones del dominio.
- Excepciones de reglas de negocio.
- Patrón State aplicado a Viaje.
- Patrón Composite aplicado a Grupo y Permiso.
- Persona.
- Usuario.
- Grupo.
- Permiso.
- Relaciones controladas entre entidades.

No depende de otras capas del sistema.

### SIGEVIP.Application

Contiene:

- Servicios de aplicación.
- Contratos de repositorios.
- Contratos de servicios de infraestructura.
- Autenticación.
- Autorización.
- Gestión de sesión actual.
- Modelos de resultado de aplicación.

Depende únicamente de `SIGEVIP.Domain`.

### SIGEVIP.Infrastructure

Contiene:

- Implementaciones ADO.NET.
- Acceso a SQL Server.
- Configuración de conexión.
- Servicios técnicos.
- Implementación de hash de contraseñas.
- Implementaciones futuras de repositorios.
- Transacciones.
- Auditoría técnica.
- Integraciones externas.

Depende de `SIGEVIP.Domain` y `SIGEVIP.Application`.

### SIGEVIP.WinForms

Contiene:

- Formularios.
- Controles visuales.
- Controladores adaptados a Windows Forms.
- Navegación.
- Presentación de errores y resultados.
- Composición futura de dependencias.

Depende de `SIGEVIP.Domain`, `SIGEVIP.Application` y `SIGEVIP.Infrastructure`.

### SIGEVIP.Tests

Contiene:

- Pruebas unitarias.
- Pruebas de reglas de dominio.
- Pruebas de servicios de aplicación.
- Pruebas de seguridad.
- Pruebas de infraestructura.
- Pruebas de integración prioritarias.

Depende de:

- `SIGEVIP.Domain`
- `SIGEVIP.Application`
- `SIGEVIP.Infrastructure`

## 4. Dirección de dependencias

    WinForms
       ├── Application
       ├── Domain
       └── Infrastructure
              ├── Application
              └── Domain

    Tests
       ├── Application
       ├── Domain
       └── Infrastructure

`SIGEVIP.Domain` no posee referencias hacia Application, Infrastructure, WinForms ni Tests.

`SIGEVIP.Application` no depende de Infrastructure ni de WinForms.

Las implementaciones concretas de servicios técnicos se ubican en Infrastructure y satisfacen contratos definidos en Application.

## 5. Dominio implementado

### Viaje

`Viaje` controla:

- Fechas.
- Tipo.
- Monto anticipado.
- Estado actual.
- Viáticos.
- Visitas.
- Total gastado.
- Saldo pendiente.
- Transiciones de estado.
- Incorporación y exclusión lógica de viáticos.
- Incorporación controlada de visitas.

### Cliente y Visita

Se implementaron:

- Cliente con activación y desactivación lógica.
- Visita con uno o varios clientes.
- Relación Viaje-Visita.
- Relación muchos a muchos Visita-Cliente.

### Persona y Usuario

Se implementaron:

- Persona separada de Usuario.
- Usuario asociado obligatoriamente a una Persona.
- Estado activo e inactivo.
- Credenciales representadas mediante hash, salt e iteraciones.
- Asociación de uno o varios grupos.

### Grupo y Permiso

Se implementó el patrón Composite:

- `Permiso` actúa como componente hoja.
- `Grupo` actúa como componente compuesto.
- Un Grupo puede contener permisos y otros grupos.
- Se rechazan ciclos directos e indirectos.
- Los permisos efectivos se obtienen de forma recursiva.
- Los permisos duplicados se eliminan por código normalizado.
- Los componentes inactivos no aportan permisos.

## 6. Patrón State

El patrón State se aplica al ciclo de vida de `Viaje`.

La interfaz `IEstadoViaje` define operaciones dependientes del estado.

Implementaciones:

- `EstadoViajeAbierto`
- `EstadoViajeEnRendicion`
- `EstadoViajeAprobado`
- `EstadoViajeCancelado`

El estado persistible se representa mediante el enum `EstadoViaje`.

`EstadoViajeFactory` permite reconstruir el comportamiento desde el valor almacenado.

Los objetos State no se persistirán directamente en SQL Server.

## 7. Patrón Composite

El patrón Composite se aplica a la estructura de seguridad.

La interfaz `IPermisoComponente` permite tratar de forma uniforme:

- permisos simples;
- grupos de permisos;
- grupos anidados.

`Grupo.ObtenerPermisosEfectivos()` recorre la estructura y devuelve la unión de permisos activos.

## 8. Seguridad de aplicación

La capa Application contiene:

- `IPasswordHasher`
- `IUsuarioAutenticacionRepository`
- `AutenticacionService`
- `AutorizacionService`
- `ISesionActual`
- `SesionActual`
- `ResultadoAutenticacion`
- `PasswordHashResult`

La autenticación:

- normaliza el nombre de usuario;
- rechaza entradas vacías;
- rechaza usuarios inexistentes;
- rechaza usuarios inactivos;
- verifica el hash de contraseña;
- utiliza un mensaje público genérico para credenciales inválidas.

La autorización verifica permisos efectivos por código normalizado.

La sesión actual se implementa mediante una instancia no estática y almacena únicamente un usuario activo.

## 9. Seguridad de infraestructura

`Pbkdf2PasswordHasher` implementa `IPasswordHasher` mediante:

- PBKDF2.
- HMAC-SHA256.
- Salt aleatorio de 32 bytes.
- Hash de 32 bytes.
- 100000 iteraciones.
- Comparación de hashes en tiempo constante.

No se almacena ninguna contraseña en texto plano.

## 10. Persistencia

La persistencia se implementará mediante ADO.NET y SQL Server.

No se utilizará Entity Framework.

En el estado actual todavía están pendientes:

- tablas de Persona, Usuario, Grupo y Permiso;
- relaciones UsuarioGrupo;
- relaciones GrupoPermiso;
- repositorios ADO.NET;
- migraciones;
- carga inicial de grupos y permisos;
- persistencia de sesiones y auditoría.

## 11. Principios aplicados

- Separación de responsabilidades.
- Bajo acoplamiento.
- Alta cohesión.
- Inversión de dependencias.
- Encapsulamiento de colecciones.
- Validación explícita.
- Excepciones específicas del dominio.
- Mensajes públicos seguros.
- Gestión centralizada futura de errores.
- Variables de configuración.
- No almacenar secretos en el repositorio.
- Hash seguro de contraseñas.
- Desarrollo incremental y ejecutable.
