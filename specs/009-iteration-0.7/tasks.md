# Tasks 0.7

## T001: Chunk Sync + Client Prediction
- [ ] T001.1 Extender `ChunkNetworkBehaviour` con broadcast de cambios
- [ ] T001.2 `ChunkManager` servidor: track dirty chunks, broadcast periódico
- [ ] T001.3 `ChunkManager` cliente: cache chunks, request missing
- [ ] T001.4 `NetworkTransform` personalizado con predicción
- [ ] T001.5 `InputBuffer` cliente + reconciliación servidor
- [ ] T001.6 Tests: chunk sync, predicción, reconciliación

## T002: Chat System + Comandos
- [ ] T002.1 `ChatNetworkBehaviour`: RPCs send/receive
- [ ] T002.2 `CommandProcessor`: parser, permisos, ejecución
- [ ] T002.3 Comandos base: /help, /tp, /give, /gamemode, /kick, /ban
- [ ] T002.4 Chat channels: global, local, privado, admin
- [ ] T002.5 Tests: chat, comandos, permisos

## T003: Persistencia Mundo Servidor
- [ ] T003.1 `WorldPersistenceManager`: save/load chunks modificados
- [ ] T003.2 Auto-save timer (5 min) + OnApplicationQuit
- [ ] T003.3 Background serialization (Thread/Job System)
- [ ] T003.4 Carga al inicio: `DedicatedServerManager` integra
- [ ] T003.5 Tests: save/load world, auto-save

## T004: Client UI
- [ ] T004.1 `LobbyUI` (UI Toolkit): lista lobbies, crear, unirse, chat pre-partida
- [ ] T004.2 `InGameHUD`: hotbar, salud, hambre, coordenadas
- [ ] T004.3 `ChatUI`: mensajes, canales, historial, input
- [ ] T004.4 `SettingsUI`: gráficos, controles, audio, accesibilidad
- [ ] T004.5 `PauseMenu`: reanudar, desconectar, settings, salir
- [ ] T004.6 Tests: UI bindings, navegación teclado/gamepad