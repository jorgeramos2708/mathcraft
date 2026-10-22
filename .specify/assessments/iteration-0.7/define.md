# Define 0.7 - Arquitectura de Sincronización en Tiempo Real

## Chunk Synchronization
- **ChunkNetworkBehaviour** ya existe con `RequestChunkDataServerRpc` / `SendChunkDataClientRpc`
- **Nuevo**: `BroadcastBlockUpdateClientRpc` para cambios incrementales
- **Optimización**: Solo enviar chunks sucios (dirty chunks) cada tick
- **Compresión**: Delta encoding para actualizaciones de bloques

## Client Prediction & Reconciliation
- **Input Buffer**: Cliente guarda inputs locales (movimiento, salto, etc.)
- **Server Reconciliation**: Servidor valida y corrige posición cada N ticks
- **Interpolation**: Suavizado visual de otras entidades remotas

## Chat & Commands
- **ChatMessage** NetworkBehaviour con RPCs
- **Command Parser**: Prefijo `/` para comandos admin
- **Permissions**: Roles (Player, Moderator, Admin, Owner)

## World Persistence
- **Auto-save**: Cada 5 minutos + OnApplicationQuit
- **Formato**: Mismo JSON que singleplayer (SaveSystem)
- **Carga**: Al iniciar servidor, cargar mundo guardado

## Client UI
- **LobbyUI**: Lista lobbies públicos, crear/unirse, chat pre-partida
- **InGameUI**: Chat, lista jugadores, HUD, menú pause
- **Settings**: Gráficos, controles, accesibilidad