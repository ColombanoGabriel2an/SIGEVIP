# Modelo de dominio de SIGEVIP

## 1. Alcance

Este documento describe el modelo de dominio implementado hasta el Bloque 3.

Incluye:

- Viaje.
- Viático.
- Cliente.
- Visita.
- Persona.
- Usuario.
- Grupo.
- Permiso.
- Patrón State.
- Patrón Composite.
- Relaciones entre entidades.
- Activación y desactivación lógica.
- Reglas económicas.
- Representación segura de credenciales.

No incluye todavía:

- persistencia completa;
- repositorios ADO.NET;
- migraciones de seguridad;
- formularios funcionales;
- recuperación de contraseña;
- reportes;
- mapas;
- geolocalización.

## 2. Agregado Viaje

`Viaje` es la raíz del agregado que administra:

- datos generales;
- período;
- tipo;
- monto anticipado;
- estado;
- viáticos;
- visitas;
- reglas económicas;
- transiciones;
- cancelación condicionada.

### Atributos principales

- `IdViaje`
- `FechaInicio`
- `FechaFin`
- `Descripcion`
- `TipoViaje`
- `MontoAnticipado`
- `EstadoActual`
- `Viaticos`
- `Visitas`

### Propiedades calculadas

`TotalGastado`:

    suma de los viáticos vigentes

`SaldoPendiente`:

    TotalGastado - MontoAnticipado

### Comportamientos

- `AgregarViatico`
- `AgregarVisita`
- `ExcluirViatico`
- `ReactivarViatico`
- `CalcularSaldo`
- `EnviarARendicion`
- `Aprobar`
- `Cancelar`

## 3. Entidad Viatico

### Atributos

- `IdViatico`
- `IdViaje`
- `Fecha`
- `Monto`
- `Descripcion`
- `Estado`

### Reglas principales

- Debe pertenecer a un Viaje.
- Su monto debe ser mayor que cero.
- Su fecha debe estar dentro del período.
- Nace como `Vigente`.
- Puede excluirse lógicamente.
- Solo los vigentes forman parte del total.

## 4. Entidad Cliente

### Atributos

- `IdCliente`
- `RazonSocial`
- `Cuit`
- `Email`
- `Telefono`
- `Localidad`
- `Provincia`
- `Activo`

### Reglas principales

- Razón social obligatoria.
- CUIT obligatorio.
- Normalización sin espacios ni guiones.
- Nace activo.
- Admite activación y desactivación lógica.
- No se elimina físicamente.

La unicidad global del CUIT todavía requiere repositorio e índice único.

## 5. Entidad Visita

### Atributos

- `IdVisita`
- `IdViaje`
- `Fecha`
- `Observacion`
- `LocalidadEncuentro`
- `Clientes`

### Reglas principales

- Observación obligatoria.
- Localidad obligatoria.
- Debe tener al menos un Cliente.
- Puede tener varios Clientes.
- No admite Clientes nulos.
- No admite Clientes duplicados.
- No puede reasignarse a otro Viaje.
- Solo puede agregarse a un Viaje Abierto.

## 6. Entidad Persona

Persona representa información personal separada de la identidad de acceso.

### Atributos

- `IdPersona`
- `Nombre`
- `Apellido`
- `Email`
- `Activo`

### Reglas principales

- Nombre obligatorio.
- Apellido obligatorio.
- Email obligatorio.
- Nace activa.
- Puede desactivarse.
- Puede reactivarse.
- La baja es lógica.

## 7. Entidad Usuario

Usuario representa la identidad de acceso al sistema.

### Atributos

- `IdUsuario`
- `IdPersona`
- `NombreUsuario`
- `PasswordHash`
- `PasswordSalt`
- `IteracionesPassword`
- `Activo`
- `Grupos`

### Reglas principales

- Todo Usuario requiere una Persona válida.
- El nombre se normaliza a minúsculas.
- No almacena contraseña en texto plano.
- Hash y salt se protegen mediante copias defensivas.
- Las iteraciones se conservan junto a las credenciales.
- Nace activo.
- Puede activarse y desactivarse.
- Puede pertenecer a uno o varios Grupos.
- No admite Grupos nulos.
- No admite Grupos duplicados.
- La colección de Grupos es de solo lectura.

La unicidad global del nombre de usuario se implementará en Application, Infrastructure y SQL Server.

## 8. Entidad Permiso

Permiso representa una autorización funcional concreta.

### Atributos

- `IdPermiso`
- `Codigo`
- `Nombre`
- `Descripcion`
- `Activo`

### Reglas principales

- Código obligatorio.
- Nombre obligatorio.
- Código normalizado a mayúsculas.
- Descripción opcional.
- Nace activo.
- Puede activarse y desactivarse.
- Un Permiso inactivo no es efectivo.

Permiso implementa `IPermisoComponente` como componente hoja.

## 9. Entidad Grupo

Grupo representa una agrupación configurable de permisos.

### Atributos

- `IdGrupo`
- `Codigo`
- `Nombre`
- `Descripcion`
- `Activo`
- `Componentes`

### Comportamientos

- `AgregarComponente`
- `Activar`
- `Desactivar`
- `ObtenerPermisosEfectivos`

### Reglas principales

- Código obligatorio.
- Nombre obligatorio.
- Código normalizado a mayúsculas.
- Puede contener Permisos.
- Puede contener Grupos.
- No admite componentes nulos.
- No admite duplicados.
- No puede contenerse a sí mismo.
- No puede formar ciclos indirectos.
- Un Grupo inactivo no aporta permisos.
- Un Grupo hijo inactivo no aporta permisos.
- Los permisos efectivos se deduplican por código.

Grupo implementa `IPermisoComponente` como componente compuesto.

## 10. Interfaz IPermisoComponente

La interfaz permite tratar uniformemente a Permiso y Grupo.

Expone:

- `IdComponente`
- `Codigo`
- `Activo`
- `ObtenerPermisosEfectivos()`

Esta estructura implementa el patrón Composite.

## 11. Relaciones

### Persona y Usuario

    Persona 1 -------- 0..1 Usuario

Una Persona puede existir sin Usuario.

Todo Usuario requiere una Persona.

### Usuario y Grupo

    Usuario N -------- N Grupo

Un Usuario puede pertenecer a varios Grupos.

Un Grupo puede asignarse a varios Usuarios.

La tabla asociativa futura será `UsuarioGrupo`.

### Grupo y componentes

    Grupo 1 -------- 0..N IPermisoComponente

Un componente puede ser:

- Permiso;
- Grupo.

La persistencia deberá representar la estructura sin introducir ciclos.

### Viaje y Visita

    Viaje 1 -------- 0..N Visita

Cada Visita pertenece a un único Viaje.

### Visita y Cliente

    Visita N -------- N Cliente

La tabla asociativa futura será `VisitaCliente`.

## 12. Identificación de duplicados

### Cliente dentro de Visita

Se considera duplicado cuando:

- es la misma referencia;
- tiene el mismo `IdCliente` persistido;
- tiene el mismo CUIT normalizado.

### Visita dentro de Viaje

Se considera duplicada cuando:

- es la misma referencia;
- tiene el mismo `IdVisita` persistido.

### Grupo dentro de Usuario

Se considera duplicado cuando:

- es la misma referencia;
- tiene el mismo identificador persistido;
- tiene el mismo código normalizado.

### Componente dentro de Grupo

Se considera duplicado cuando:

- es la misma referencia;
- tiene el mismo identificador persistido;
- tiene el mismo código normalizado.

No se sobrescribieron `Equals` ni `GetHashCode`.

## 13. Patrón State

El patrón State se aplica a Viaje.

Estados:

- `Abierto`
- `EnRendicion`
- `Aprobado`
- `Cancelado`

`IEstadoViaje` encapsula las operaciones dependientes del estado.

`EstadoViajeFactory` reconstruye el comportamiento desde el enum persistible.

## 14. Patrón Composite

El patrón Composite se aplica a la seguridad.

- Permiso es hoja.
- Grupo es compuesto.
- Grupo puede contener permisos y grupos.
- La autorización consulta permisos efectivos sin distinguir el tipo concreto.
- Se previenen ciclos y duplicados.

## 15. Credenciales

Usuario almacena:

- hash;
- salt;
- iteraciones.

No almacena:

- contraseña en texto plano;
- contraseña reversible;
- algoritmo embebido en la entidad.

La generación y verificación pertenece a `IPasswordHasher`.

## 16. Compatibilidad con persistencia

La persistencia futura requerirá:

- `Persona`
- `Usuario`
- `Grupo`
- `Permiso`
- `UsuarioGrupo`
- estructura de componentes de Grupo
- `Viaje`
- `Viatico`
- `Cliente`
- `Visita`
- `VisitaCliente`

Los objetos de comportamiento State no se almacenarán directamente.

## 17. Pendientes

- Repositorios de seguridad.
- Reconstrucción de Usuario con Grupos.
- Persistencia de Composite.
- Unicidad de nombre de usuario.
- Unicidad de CUIT.
- Casos de uso de gestión.
- Auditoría.
- Interfaz.
- Pruebas de integración con SQL Server.
