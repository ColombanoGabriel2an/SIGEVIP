# Validación funcional de interfaz y seguridad

## 1. Alcance

Esta validación cubre:

- inicio de la aplicación;
- autenticación;
- sesión;
- perfil;
- menú principal;
- permisos visuales;
- cierre de sesión;
- salida.

## 2. Requisitos previos

- SQL Server operativo.
- Base `SIGEVIP` disponible.
- Migración `002` aplicada.
- Seed de seguridad aplicado.
- Usuario administrador creado mediante `SIGEVIP.Setup`.
- Cadena `SIGEVIP` configurada en `App.config`.

## 3. Compilación

Comando:

```bash
MSYS2_ARG_CONV_EXCL='*' "/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" \
  "SIGEVIP.sln" \
  "/t:Rebuild" \
  "/p:Configuration=Debug" \
  "/m"
```

Resultado verificado:

- 0 advertencias.
- 0 errores.

## 4. Pruebas automatizadas

Comando:

```bash
"/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" \
  "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll"
```

Resultado verificado:

- 197 pruebas totales.
- 197 correctas.
- 0 fallidas.

## 5. Inicio de la aplicación

Comando:

```bash
./src/SIGEVIP.WinForms/bin/Debug/SIGEVIP.WinForms.exe
```

Resultado esperado:

- se abre el formulario de login;
- el campo de usuario recibe el foco;
- la contraseña permanece oculta.

## 6. Credenciales inválidas

Procedimiento:

1. Ingresar un usuario inexistente o una contraseña incorrecta.
2. Presionar **Iniciar sesión**.

Resultado esperado:

- no se inicia sesión;
- se muestra un mensaje genérico;
- no se revela qué credencial fue incorrecta.

## 7. Credenciales válidas

Procedimiento:

1. Ingresar el usuario administrador.
2. Ingresar la contraseña correcta.
3. Presionar **Iniciar sesión**.

Resultado esperado:

- el login se cierra;
- se abre el menú principal;
- se mantiene ejecutándose un único proceso.

## 8. Perfil autenticado

Resultado esperado:

- se muestra nombre y apellido;
- se muestra el nombre de usuario;
- los botones **Cerrar sesión** y **Salir** permanecen visibles.

## 9. Permisos visuales

Para el administrador actual se verificaron visibles:

- Clientes.
- Viajes.
- Usuarios.
- Grupos.
- Permisos.
- Auditoría.

Se verificaron ocultas:

- Visitas.
- Viáticos y rendiciones.

El estado inferior indica seis opciones visibles.

## 10. Cierre de sesión

Procedimiento:

1. Presionar **Cerrar sesión**.
2. Confirmar la operación.

Resultado esperado:

- se cierra el menú;
- se limpia la sesión;
- vuelve a aparecer el login.

## 11. Nueva autenticación

Procedimiento:

1. Autenticarse nuevamente.

Resultado esperado:

- se recupera nuevamente el perfil;
- se abre nuevamente el menú;
- se recalculan los permisos.

## 12. Salida

Procedimiento:

1. Presionar **Salir**.
2. Confirmar.

Resultado esperado:

- la aplicación finaliza;
- no queda el proceso abierto.

## 13. Cierre mediante la cruz

Procedimiento:

1. Abrir el menú.
2. Presionar la cruz de la ventana.

Resultado esperado:

- se muestra una sola confirmación;
- al seleccionar **No**, la aplicación permanece abierta;
- al seleccionar **Sí**, la aplicación finaliza.

## 14. Redimensionamiento

Procedimiento:

1. Maximizar la ventana.
2. Restaurarla.

Resultado esperado:

- nombre y usuario visibles;
- botones **Cerrar sesión** y **Salir** visibles;
- opciones distribuidas sin espacios reservados por módulos ocultos.

## 15. Resultado final

El flujo básico de seguridad e interfaz fue validado correctamente.

Estado:

`APROBADO`

## 16. Validación del módulo Clientes

### Apertura

Procedimiento:

1. Autenticarse con un Usuario autorizado.
2. Presionar **Clientes**.

Resultado verificado:

- se abre `ClientesForm`;
- la grilla se carga;
- no aparece el mensaje de módulo pendiente.

### Alta

Procedimiento:

1. Presionar **Nuevo**.
2. Ingresar razón social y CUIT.
3. Completar datos opcionales.
4. Guardar.

Resultado verificado:

- el Cliente se registra;
- aparece en la grilla;
- permanece después de reiniciar la aplicación.

### CUIT duplicado

Procedimiento:

1. Intentar registrar otro Cliente con el mismo CUIT.

Resultado verificado:

- la operación se rechaza;
- se muestra un mensaje comprensible;
- no se duplica el registro.

### Modificación

Procedimiento:

1. Seleccionar un Cliente.
2. Presionar **Modificar**.
3. Cambiar datos.
4. Guardar.

Resultado verificado:

- los datos se actualizan;
- el identificador se conserva;
- el estado lógico se conserva.

### Desactivación y activación

Resultado verificado:

- la desactivación es lógica;
- el Cliente aparece al filtrar Inactivos;
- puede reactivarse;
- no se elimina físicamente.

### Filtros

Se validaron:

- búsqueda general;
- CUIT;
- localidad;
- provincia;
- activos;
- inactivos;
- todos.

### Permisos visuales

Un Usuario con `CLIENTE_GESTIONAR` visualiza:

- Nuevo;
- Modificar;
- Activar;
- Desactivar.

Las operaciones también vuelven a validar permisos en Application.

## 17. Resultado integral actualizado

El flujo de seguridad, navegación principal y módulo Clientes fue validado correctamente.

Estado:

`APROBADO`
