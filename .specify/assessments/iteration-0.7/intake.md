# Intake 0.7 - Sincronización en Tiempo Real y Experiencia de Juego

## Objetivos
- Sincronizar chunks y entidades entre clientes en tiempo real
- Implementar predicción de movimiento cliente + reconciliación servidor
- Añadir chat y comandos de servidor
- Persistencia de mundo en servidor dedicado
- UI de lobby/matchmaking en cliente

## Alcance
- **Chunk Sync**: Clientes reciben updates de ChunkNetworkBehaviour en tiempo real
- **Client Prediction**: Movimiento local inmediato con corrección servidor
- **Chat System**: Mensajes globales, privados, comandos (/tp, /give, /gamemode)
- **World Persistence**: Guardado automático periódico + al apagar servidor
- **Client UI**: Pantalla de lobby, lista de jugadores, chat en juego

## Fuera de Alcance
- Sistema de combate avanzado
- IA de mobs sincronizada
- Economía/comercio entre jugadores