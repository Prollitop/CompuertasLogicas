# Diseño

Los componentes tienen identificadores estables y nombres de presentación. Cada compuerta define su ID, nombre, aridad mínima, capacidad de aridad variable, evaluación y generación de tabla de verdad. Esto permite que futuras paletas visuales y paneles educativos consuman metadatos sin conocer la implementación lógica.

Los estados usan el enum `LogicState` para comunicar explícitamente 0 y 1. La primera versión se limita a lógica combinacional; los elementos secuenciales y la resolución temporal se añadirán cuando la simulación visual lo requiera.

## Interacción del editor

El editor representa cada componente con un cuerpo de laboratorio, símbolo vectorial, entradas a la izquierda y salida a la derecha. Las líneas de cable conectan solamente un `OutputPort` con un `InputPort` usando `Circuit.Connect`; una línea punteada anticipa el cable durante el gesto. La selección, los puertos, el estado ON/OFF y las conexiones inválidas tienen mensajes y contrastes visuales propios.

Los controles de zoom transforman únicamente el canvas y el pan se realiza con el botón derecho. Ninguna de estas operaciones modifica las identidades lógicas; el movimiento actualiza exclusivamente `ComponentPosition`.

## Fase 3: laboratorio y aprendizaje

Cada cambio de Switch solicita inmediatamente una evaluación al `CircuitSimulator`; no se usan temporizadores. Los cables y componentes leen después sus estados desde los puertos del Core y los cables HIGH reciben un contraste y halo visual. El modo Aprender usa el orden AND, OR, NOT, NAND, NOR, XOR y XNOR, permite alterar entradas y resalta la fila actual de la tabla generada por cada definición.

Los circuitos se guardan en JSON con componentes, tipo, identificadores, coordenadas, estados de Switch y referencias de puertos. La carga valida estas referencias antes de reconstruir el circuito.

## Fase 4: desafíos

Cada desafío define su comportamiento esperado como función de las entradas, componentes permitidos, dificultad, XP y requisito de desbloqueo. El validador recorre todas las combinaciones binarias y llama al simulador existente; no reconoce soluciones por el simple nombre de una compuerta. Las estrellas premian soluciones correctas y más compactas sin bloquear el aprendizaje.
