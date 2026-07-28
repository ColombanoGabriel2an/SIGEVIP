# Política de backup y restauración de SIGEVIP

## 1. Objetivo

Definir un procedimiento reproducible para respaldar, verificar y restaurar la base de datos `SIGEVIP`, reduciendo el riesgo de pérdida de información y comprobando la capacidad de recuperación del sistema.

## 2. Alcance

La política cubre:

- base SQL Server `SIGEVIP`;
- migraciones 001 a 007;
- datos funcionales;
- Usuarios, Grupos y Permisos;
- Clientes;
- Viajes, Visitas, Viáticos y Rendiciones;
- Auditoría.

No cubre el repositorio Git, la documentación maestra ni la planilla UCP. Esos elementos deben respaldarse mediante Git y una copia independiente del paquete de liberación.

## 3. Entorno actual

| Elemento | Valor |
|---|---|
| Servidor SQL configurado | `Lenovo_Gabi` |
| Base | `SIGEVIP` |
| Autenticación | Seguridad integrada de Windows |
| Aplicación | .NET Framework 4.8 |
| Persistencia | ADO.NET |
| Migraciones | 001 a 007 |

La cadena de conexión se encuentra en:

- `src/SIGEVIP.WinForms/App.config`
- `tools/SIGEVIP.Setup/App.config`
- `tests/SIGEVIP.Tests/App.config`

## 4. Política definida

| Aspecto | Decisión |
|---|---|
| Responsable | Administrador técnico del sistema |
| Tipo principal | Backup completo |
| Frecuencia regular | Semanal |
| Backup extraordinario | Antes de migraciones, cambios críticos y entrega |
| Verificación inmediata | `RESTORE VERIFYONLY` con checksum |
| Restauración de prueba | Antes de la entrega y luego de un cambio estructural importante |
| Retención | Últimas cuatro copias semanales y una copia de entrega |
| Ubicación primaria | Directorio predeterminado de backups de SQL Server |
| Copia secundaria | Unidad externa o carpeta protegida fuera del repositorio |
| Formato | Archivo `.bak` |
| Versionado en Git | Prohibido para archivos `.bak` |
| Protección | Acceso restringido porque contiene datos y hashes de contraseñas |

## 5. Objetivos operativos propuestos

Estos valores son objetivos técnicos propuestos para el MVP del producto:

- **RPO:** hasta siete días, por la frecuencia semanal;
- **RTO:** hasta dos horas para restaurar, validar y reconfigurar la aplicación.

No constituyen un acuerdo productivo ni un SLA empresarial.

## 6. Archivos de mantenimiento

### Backup

`database/maintenance/001_backup_completo.sql`

Funciones:

- verifica que exista la base;
- obtiene el directorio predeterminado de backup;
- genera un nombre con fecha y hora;
- realiza un backup completo `COPY_ONLY`;
- utiliza checksum;
- ejecuta `RESTORE VERIFYONLY`;
- informa la ruta generada.

### Restauración de prueba

`database/maintenance/002_restaurar_backup_prueba.sql`

Funciones:

- recibe la ruta de un `.bak`;
- verifica la base de origen;
- obtiene los nombres lógicos actuales;
- restaura en `SIGEVIP_RESTORE_TEST`;
- no sobrescribe `SIGEVIP`;
- ejecuta `DBCC CHECKDB`;
- consulta versiones y tablas principales.

## 7. Procedimiento de backup

### En SSMS

1. Abrir SQL Server Management Studio.
2. Conectarse al servidor `Lenovo_Gabi` con autenticación de Windows.
3. Abrir el archivo:
   `database/maintenance/001_backup_completo.sql`.
4. Verificar que la ventana esté conectada a la instancia correcta.
5. Ejecutar el script completo.
6. Copiar la ruta informada en la grilla o en Mensajes.
7. Confirmar que el archivo `.bak` exista.
8. Conservar una copia secundaria fuera de la carpeta pública del repositorio.

### Resultado esperado

- mensaje de backup completado;
- progreso hasta 100 %;
- `RESTORE VERIFYONLY` correcto;
- ruta completa del archivo generado.

## 8. Procedimiento de restauración de prueba

### En SSMS

1. Obtener la ruta del `.bak` generado.
2. Abrir:
   `database/maintenance/002_restaurar_backup_prueba.sql`.
3. Reemplazar únicamente:

```sql
N'REEMPLAZAR_CON_LA_RUTA_COMPLETA_DEL_ARCHIVO_BAK'
```

por la ruta real.

4. Confirmar que no exista una base llamada `SIGEVIP_RESTORE_TEST`.
5. Ejecutar el script.
6. Revisar:
   - `DBCC CHECKDB`;
   - versión 007;
   - tablas principales;
   - cantidad de registros.
7. Conservar la base restaurada hasta obtener capturas o evidencia.
8. Eliminarla manualmente solo después de completar la validación.

### Resultado esperado

- base `SIGEVIP_RESTORE_TEST` creada;
- `DBCC CHECKDB` sin errores;
- versiones 001 a 007 disponibles;
- tablas de seguridad, Clientes, Viajes, Visitas, Viáticos y Auditoría presentes.

## 9. Validación posterior

Después de restaurar:

1. comprobar `dbo.VersionBaseDatos`;
2. comprobar las tablas principales;
3. ejecutar los scripts `*_validar_*.sql` contra la base de prueba cuando corresponda;
4. verificar la integridad mediante `DBCC CHECKDB`;
5. registrar fecha, archivo, responsable y resultado;
6. registrar capturas y resultados como evidencia de continuidad operativa.

## 10. Registro de ejecución

| Fecha | Archivo `.bak` | Tamaño | Verificación | Restauración | Responsable | Observaciones |
|---|---|---:|---|---|---|---|
| 28/07/2026 | `SIGEVIP_FULL_20260728_152347.bak` | No registrado | Correcta | Correcta | Administrador técnico | Restauración en `SIGEVIP_RESTORE_TEST`; versiones 001 a 007 y tablas principales verificadas |

## 11. Seguridad

- no subir archivos `.bak` a GitHub;
- no enviarlos por canales públicos;
- restringir el acceso a la carpeta;
- recordar que contienen información funcional y hashes;
- eliminar copias temporales que ya no sean necesarias;
- conservar al menos una copia verificada fuera del equipo principal.

## 12. Recuperación alternativa

Si no existe un backup restaurable:

1. ejecutar las migraciones 001 a 007 en orden;
2. ejecutar los seeds de seguridad y permisos;
3. ejecutar `SIGEVIP.Setup` para crear el administrador inicial;
4. cargar datos de demostración;
5. validar la base;
6. ejecutar las pruebas.

Esta alternativa reconstruye la estructura, pero no recupera datos históricos no respaldados.

## 13. Criterio de aceptación

La política queda cumplida cuando:

- se genera un `.bak`;
- `RESTORE VERIFYONLY` finaliza correctamente;
- se restaura `SIGEVIP_RESTORE_TEST`;
- `DBCC CHECKDB` no informa errores;
- se verifican migraciones y tablas;
- la ejecución queda registrada;
- se conserva evidencia.


## 14. Última verificación ejecutada

Fecha:

`28/07/2026`

Backup:

`C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup\SIGEVIP_FULL_20260728_152347.bak`

Resultados:

- conjunto de backup válido;
- restauración completada en `SIGEVIP_RESTORE_TEST`;
- 1.586 páginas procesadas;
- última versión: 007;
- siete versiones registradas;
- diez tablas principales verificadas;
- `DBCC CHECKDB` sin errores informados;
- base principal `SIGEVIP` preservada.
- copia secundaria almacenada fuera del repositorio;
- tamaño de origen y destino: 14.798.848 bytes;
- hash SHA-256 verificado: `46485BB87C5DB13BC0AB668C8BF423511B08BC58358AEFD5A331F2FAA940F5AB`;
- ubicación secundaria: `C:\Users\gabic\Documents\SIGEVIP_Backups\SIGEVIP_FULL_20260728_152347.bak`.

Conclusión:

`BACKUP, RESTAURACION Y VALIDACION CORRECTOS`
