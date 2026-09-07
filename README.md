# SIMULADORCOMPUERTTAS · LOGIC LAB

Simulador educativo interactivo de compuertas lógicas hecho con **C#, WPF, XAML y MVVM**. Combina un editor de circuitos, simulación combinacional en tiempo real, lecciones con tablas de verdad, laboratorio libre y desafíos gamificados.

## Características

- Compuertas AND, OR, NOT, NAND, NOR, XOR y XNOR con tablas de verdad generadas desde el Core.
- Editor visual con Switch, LED, puertos, cables HIGH/LOW, selección, mover, eliminar, zoom, pan y guardado/carga JSON.
- **Aprender:** experimentos interactivos y tabla de verdad con la fila actual resaltada.
- **Laboratorio:** construcción libre, ejemplos listos para demostrar y simulación inmediata.
- **Desafíos:** diez retos de comportamiento, estrellas, XP, rangos, desbloqueos y logros locales.

## Arquitectura

- `SimuladorCompuertrtas.Core`: lógica digital, modelos de circuito, simulador, documentos JSON, desafíos y progreso; no depende de WPF.
- `SimuladorCompuertrtas`: interfaz WPF, recursos y ViewModels MVVM.
- `SimuladorCompuertrtas.Core.Tests`: pruebas xUnit independientes de WPF.

El progreso se guarda localmente en `%LocalAppData%/LogicLab/player_progress.json`. Los circuitos se guardan mediante el comando **Guardar** como JSON.

## Ejecutar y probar

Se requiere el SDK de .NET 10 con soporte WPF en Windows:

```bash
dotnet build SimuladorCompuertrtas.slnx
dotnet test SimuladorCompuertrtas.slnx
dotnet run --project SimuladorCompuertrtas.csproj
```

Consulte `docs/ARCHITECTURE.md`, `docs/DESIGN.md`, `docs/GAMEPLAY.md` y `docs/ROADMAP.md` para el detalle de la implementación final.
