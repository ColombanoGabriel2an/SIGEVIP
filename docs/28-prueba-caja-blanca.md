# Prueba de caja blanca — Prevención de ciclos entre Grupos

## 1. Objetivo

Analizar la estructura interna del método que impide ciclos en la jerarquía Composite de Grupos y diseñar casos que recorran sus caminos independientes.

## 2. Elemento bajo análisis

Clase:

`src/SIGEVIP.Domain/Entities/Grupo.cs`

Método:

```csharp
private void ValidarAusenciaDeCiclo(Grupo grupoCandidato)
```

El método se ejecuta desde las operaciones públicas que agregan o reemplazan Grupos hijos.

También utiliza el método recursivo:

```csharp
private bool ContieneGrupo(
    Grupo grupoBuscado,
    HashSet<Grupo> visitados)
```

## 3. Código lógico analizado

El comportamiento del método es:

1. comprobar si el Grupo candidato es la misma instancia que el Grupo actual;
2. rechazar la autorreferencia;
3. crear el conjunto de Grupos visitados;
4. recorrer recursivamente al candidato;
5. rechazar la relación si el candidato ya contiene al Grupo actual;
6. permitir la asociación cuando no existe autorreferencia ni ciclo indirecto.

## 4. Nodos del grafo

| Nodo | Instrucción |
|---:|---|
| N1 | Inicio |
| N2 | `ReferenceEquals(this, grupoCandidato)` |
| N3 | Lanzar excepción por autorreferencia |
| N4 | Crear `HashSet<Grupo> visitados` |
| N5 | `grupoCandidato.ContieneGrupo(this, visitados)` |
| N6 | Lanzar excepción por ciclo indirecto |
| N7 | Validación correcta; continuar |
| N8 | Fin común |

## 5. Grafo de flujo

```text
                 ┌────────────┐
                 │ N1 Inicio  │
                 └─────┬──────┘
                       │
                       v
             ┌───────────────────┐
             │ N2 ¿mismo Grupo?  │
             └──────┬───────┬────┘
                  Sí│       │No
                    v       v
          ┌────────────┐  ┌─────────────────┐
          │ N3 Error   │  │ N4 Crear        │
          │ autorref.  │  │ visitados       │
          └─────┬──────┘  └────────┬────────┘
                │                  v
                │        ┌──────────────────────┐
                │        │ N5 ¿candidato        │
                │        │ contiene al actual?  │
                │        └──────┬─────────┬─────┘
                │             Sí│         │No
                │               v         v
                │       ┌────────────┐  ┌─────────────┐
                │       │ N6 Error   │  │ N7 Relación │
                │       │ por ciclo  │  │ válida      │
                │       └─────┬──────┘  └──────┬──────┘
                └─────────────┴────────────────┘
                              v
                        ┌──────────┐
                        │ N8 Fin   │
                        └──────────┘
```

## 6. Complejidad ciclomática

Cantidad de nodos:

**N = 8**

Cantidad de aristas:

1. N1 → N2
2. N2 → N3
3. N3 → N8
4. N2 → N4
5. N4 → N5
6. N5 → N6
7. N6 → N8
8. N5 → N7
9. N7 → N8

**E = 9**

Componentes conectados:

**P = 1**

Fórmula:

**M = E − N + 2P**

**M = 9 − 8 + 2 = 3**

Comprobación alternativa:

- decisiones: 2;
- complejidad: decisiones + 1;
- **M = 3**.

Por lo tanto, se requieren al menos tres caminos independientes.

## 7. Caminos independientes

### Camino CB-01 — Autorreferencia

`N1 → N2(Sí) → N3 → N8`

Resultado esperado:

- se lanza `ReglaNegocioException`;
- mensaje: “Un grupo no puede agregarse a sí mismo.”;
- la colección no se modifica.

Prueba existente:

`AgregarGrupo_AElMismoGrupo_LanzaExcepcion`

### Camino CB-02 — Ciclo indirecto

`N1 → N2(No) → N4 → N5(Sí) → N6 → N8`

Resultado esperado:

- el recorrido recursivo detecta que el candidato contiene al Grupo actual;
- se lanza `ReglaNegocioException`;
- mensaje: “La asociación produciría un ciclo entre grupos.”;
- la estructura anterior se conserva.

Pruebas existentes:

- `AgregarGrupo_QueProduceCicloIndirecto_LanzaExcepcion`
- `ReemplazarGruposHijos_QueGeneraCicloIndirecto_RechazaOperacion`
- `Actualizar_QueProduceCicloIndirecto_RechazaYConservaEstado`

### Camino CB-03 — Asociación válida

`N1 → N2(No) → N4 → N5(No) → N7 → N8`

Resultado esperado:

- no se lanza excepción;
- el Grupo candidato se agrega como hijo;
- la jerarquía permanece acíclica.

Prueba existente:

`AgregarComponente_ConGrupoHijoValido_AgregaGrupo`

## 8. Papel del método recursivo

`ContieneGrupo()`:

1. registra cada Grupo en `visitados`;
2. evita repetir nodos previamente recorridos;
3. devuelve verdadero cuando encuentra el Grupo buscado;
4. recorre únicamente componentes de tipo `Grupo`;
5. devuelve falso cuando agota la jerarquía sin encontrarlo.

El conjunto `visitados` evita recorridos infinitos aun ante una estructura persistida inválida o un grafo con caminos convergentes.

## 9. Ejecución reproducible

Desde Git Bash:

```bash
cd /c/Users/gabic/Desktop/UAI/SIGEVIP

"/c/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" \
  "tests/SIGEVIP.Tests/bin/Debug/SIGEVIP.Tests.dll" \
  /TestCaseFilter:"Name~AgregarGrupo_AElMismoGrupo|Name~CicloIndirecto|Name~GrupoHijoValido"
```

Resultado esperado:

- los tres caminos se ejecutan mediante la API pública;
- las operaciones inválidas lanzan la excepción esperada;
- la operación válida agrega el Grupo;
- ninguna prueba falla.

## 10. Criterio de aceptación

La prueba de caja blanca queda aprobada cuando:

- se cubren los tres caminos independientes;
- la autorreferencia es rechazada;
- el ciclo indirecto es rechazado;
- una jerarquía válida es aceptada;
- el estado previo no se corrompe ante una excepción;
- la ejecución automatizada finaliza correctamente.

## 11. Trazabilidad

- Patrón: Composite.
- Entidades: `Grupo` y `Permiso`.
- Requisitos relacionados: RF37 a RF39.
- Evidencia de dominio: `Grupo.cs`.
- Evidencia automatizada: `GrupoPermisoTests.cs`, `GrupoGestionDomainTests.cs` y prueba de integración de jerarquías.


## 12. Resultado ejecutado

Fecha:

`28/07/2026`

Resultado de VSTest:

- pruebas ejecutadas: 6;
- pruebas correctas: 6;
- pruebas fallidas: 0;
- tiempo total: 3,2930 segundos.

Pruebas verificadas:

- `Actualizar_QueProduceCicloIndirecto_RechazaYConservaEstado`;
- `ReemplazarGruposHijos_QueGeneraCicloIndirecto_RechazaOperacion`;
- `AgregarComponente_ConGrupoHijoValido_AgregaGrupo`;
- `AgregarGrupo_AElMismoGrupo_LanzaExcepcion`;
- `AgregarGrupo_QueProduceCicloIndirecto_LanzaExcepcion`;
- `Registrar_ConGrupoHijoValido_AsignaYPersisteJerarquia`.

Estado:

`PRUEBA DE CAJA BLANCA CORRECTA`
