# Arquitectura

## Proyectos y responsabilidades

`SimuladorCompuertrtas` es la aplicación WPF. Contiene `ViewModels/`, donde `MainWindowViewModel` expone `CircuitEditorViewModel`; `Resources/Theme.xaml` centraliza color, paneles y botones. El code-behind de la ventana se limita a traducir gestos WPF (arrastre, punteros de cable y pan) a métodos del view model.

`SimuladorCompuertrtas.Core` no tiene dependencias WPF. Contiene:

- `Logic/`: contrato `ILogicGateDefinition`, definiciones de compuerta, catálogo extensible y tablas de verdad.
- `Models/`: `Circuit`, `Component`, `LogicGate`, componentes de entrada/salida, puertos y conexiones.
- `Services/`: `CircuitSimulator`, resultado de evaluación y excepción de simulación.

`SimuladorCompuertrtas.Core.Tests` es un proyecto xUnit independiente de la interfaz.

## Editor gráfico (Fase 2)

`CircuitEditorViewModel` mantiene una única instancia de `Circuit`, el catálogo existente y `CircuitSimulator`. Sus colecciones observables son adaptadores de presentación: cada `CircuitComponentViewModel` conserva la referencia a su `Component` del Core, y cada `WireViewModel` conserva su `Connection` del Core. Por tanto, la interfaz no duplica ni reemplaza el modelo lógico.

El canvas usa las posiciones `ComponentPosition` del Core. Las coordenadas visuales del cable se recalculan a partir de los componentes; zoom y pan solo afectan al renderizado WPF. Al pulsar el Switch se actualiza `InputComponent.State` y se solicita una evaluación al simulador existente. Los errores de conexión o de evaluación se presentan en la barra de estado.

## Simulación y aprendizaje (Fase 3)

`CircuitDocumentService` pertenece al Core y serializa un circuito en JSON sin ninguna dependencia de archivos o WPF. El editor se limita a abrir los diálogos del sistema y delega la reconstrucción de componentes, posiciones, estados y conexiones al servicio. `LearningViewModel` consume las definiciones de compuerta y sus tablas de verdad existentes: no conserva una segunda implementación de las reglas booleanas.

## Juego y progresión (Fase 4)

`Gameplay/ChallengeSystem.cs` contiene definiciones de desafíos, validador por tabla de verdad y progreso local. `ChallengeValidator` usa exclusivamente `CircuitSimulator` para probar cada combinación y rechaza tipos de componente no permitidos. `ProgressService` persiste JSON en LocalApplicationData y devuelve progreso vacío con seguridad si el archivo falta o está corrupto. La UI sólo coordina esos servicios mediante `ChallengesViewModel`.

## Evaluación

Una conexión siempre va de `OutputPort` a `InputPort`; ni posiciones ni tipos visuales forman parte de ella. El simulador evalúa recursivamente las dependencias, propaga estados a las entradas y devuelve los estados de `OutputComponent`. Los ciclos y las entradas requeridas sin conexión producen `CircuitSimulationException` en esta fase combinacional.

Las coordenadas previstas para el editor se almacenan opcionalmente como `ComponentPosition`, separadas de la evaluación lógica. El catálogo permite registrar nuevas compuertas sin cambiar las existentes.

## Pruebas

Las pruebas verifican todas las filas binarias de las tablas de verdad, NOT, aridad inválida, modelos y puertos, conexiones válidas/inválidas, propagación, circuitos combinados, entradas sin conexión, ciclos y eliminación segura de componentes/conexiones. La lógica añadida para el editor continúa en el Core y es cubierta sin depender de WPF.
