\# Evidencia de backup y restauración — 28/07/2026



\## 1. Objetivo



Registrar la ejecución comprobada del procedimiento de backup, verificación y restauración de la base de datos SIGEVIP.



\## 2. Entorno



| Elemento | Valor |

|---|---|

| Servidor SQL | `Lenovo\_Gabi` |

| Base de origen | `SIGEVIP` |

| Base restaurada | `SIGEVIP\_RESTORE\_TEST` |

| Autenticación | Seguridad integrada de Windows |

| Fecha de ejecución | 28/07/2026 |

| Hora de finalización | 15:35:22 |

| Responsable | Administrador técnico |



\## 3. Backup generado



Archivo:



`C:\\Program Files\\Microsoft SQL Server\\MSSQL16.MSSQLSERVER\\MSSQL\\Backup\\SIGEVIP\_FULL\_20260728\_152347.bak`



Resultado:



`BACKUP Y VERIFICACION CORRECTOS`



La instrucción `RESTORE VERIFYONLY` confirmó que el conjunto de copia de seguridad contenido en el archivo era válido.

## 3.1. Copia secundaria verificada

Ubicación:

`C:\Users\gabic\Documents\SIGEVIP_Backups\SIGEVIP_FULL_20260728_152347.bak`

Validación:

| Control | Resultado |
|---|---|
| Tamaño del archivo original | 14.798.848 bytes |
| Tamaño de la copia secundaria | 14.798.848 bytes |
| Algoritmo | SHA-256 |
| Hash del archivo original | `46485BB87C5DB13BC0AB668C8BF423511B08BC58358AEFD5A331F2FAA940F5AB` |
| Hash de la copia secundaria | `46485BB87C5DB13BC0AB668C8BF423511B08BC58358AEFD5A331F2FAA940F5AB` |
| Coincidencia | Correcta |

La copia secundaria se almacenó fuera del repositorio Git.



\## 4. Restauración



El backup fue restaurado en una base independiente denominada:



`SIGEVIP\_RESTORE\_TEST`



La base principal `SIGEVIP` no fue reemplazada ni modificada.



Resultado informado por SQL Server:



\- 1.584 páginas procesadas para el archivo de datos;

\- 2 páginas procesadas para el archivo de log;

\- 1.586 páginas procesadas en total;

\- restauración completada correctamente;

\- tiempo de restauración: 0,034 segundos.



\## 5. Validación de estructura



Resultado:



| Base restaurada | Última versión | Cantidad de versiones |

|---|---:|---:|

| `SIGEVIP\_RESTORE\_TEST` | 7 | 7 |



Se verificaron las siguientes tablas principales:



\- `Auditoria`;

\- `Cliente`;

\- `Comprobante`;

\- `Grupo`;

\- `Permiso`;

\- `Persona`;

\- `Usuario`;

\- `Viaje`;

\- `Viatico`;

\- `Visita`.



\## 6. Migraciones verificadas



| Versión | Descripción |

|---|---|

| 001 | Creación inicial de la base de datos SIGEVIP |

| 002 | Creación del modelo relacional de seguridad |

| 003 | Creación del modelo relacional de clientes |

| 004 | Creación del modelo relacional de viajes y participantes |

| 005 | Creación del modelo relacional de visitas comerciales |

| 006 | Creación del modelo relacional de viáticos, comprobantes y auditoría de rendiciones |

| 007 | Creación de la auditoría general consultable del sistema |



\## 7. Integridad



Se ejecutó:



`DBCC CHECKDB (N'SIGEVIP\_RESTORE\_TEST') WITH NO\_INFOMSGS`



No se informaron errores de consistencia.



\## 8. Resultado final



`RESTAURACION Y VALIDACION CORRECTAS`



La prueba confirma que el backup es legible, restaurable y contiene la estructura vigente de SIGEVIP.



\## 9. Conclusión



El procedimiento de continuidad quedó validado de extremo a extremo:



1\. generación del backup;

2\. verificación del archivo;

3\. restauración en una base independiente;

4\. comprobación de integridad;

5\. verificación de migraciones;

6\. verificación de tablas principales.



La base restaurada puede eliminarse después de conservar esta evidencia y las capturas correspondientes.

