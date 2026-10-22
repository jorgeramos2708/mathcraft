# Release 0.6 - Multijugador Avanzado y Mods

## Nuevas Características

### 🌐 T001: Servidor Dedicado (Headless)
- **NetworkManager**: Wrapper completo sobre Unity Netcode for GameObjects
  - `StartDedicatedServer()` para builds headless sin UI
  - `StartHost()` para modo host (servidor + cliente local)
  - `JoinServer(address, port)` para clientes
  - Configuración de puerto y transporte via `ServerConfig`
- **DedicatedServerManager**: Ciclo de vida completo del servidor
  - `StartServer()`, `StopServer()`, `RestartServer()`
  - Gestión de sesiones de jugadores conectados
  - Eventos: `OnPlayerConnected`, `OnPlayerDisconnected`, `OnServerLog`
  - Límite configurable de jugadores (`MaxPlayers`)
- **AuthenticationHandler**: Autenticación por token en `ConnectionApprovalCallback`
  - Validación de token desde payload de conexión
  - Rechazo por token inválido o servidor lleno
  - Configurable: `RequireAuthentication`, lista de tokens válidos
- **ServerAuthority**: Validación server-side de acciones críticas
  - Movimiento: anti-teleport (distancia máxima por tick)
  - Acciones de bloque: distancia máxima de construcción (6m)
  - Transacciones de inventario: validación server-side
  - `RequireOwnership=false` en RPCs críticos

### 🎮 T002: Matchmaking y Lobby
- **MatchmakingService**: Cola de emparejamiento inteligente
  - Agrupación por `PreferredEducationLevel` (1=Secundaria, 2=Bachillerato...)
  - Preferencias: modo de juego (Survival/Creative/Adventure/Education), región
  - Procesamiento periódico configurable (default 2s)
  - Evento `OnMatchFound` con lista de jugadores emparejados
- **LobbyManager**: Gestión completa de salas
  - Creación/unión/salida de lobbies
  - Estados: `Waiting` → `Starting` → `InGame` → `Ended`
  - Configuración: `MaxPlayers`, `IsPrivate`, `AllowedPlayers`, `GameMode`
  - Transferencia automática de ownership si el host se va
  - Auto-limpieza de lobbies abandonados (>30 min en Waiting)
  - Filtrado de lobbies públicos para UI

### 🧩 T003: Sistema de Mods/Comunidad
- **ModManifest**: Metadatos completos (ScriptableObject)
  - Identidad: ModId, DisplayName, Version, Author, Description
  - Compatibilidad: GameVersion, Min/MaxGameVersion
  - Dependencias: ModId, MinVersion, MaxVersion, IsOptional
  - Seguridad: PublicKey, Signature (SHA256)
  - Tipos: Content, Gameplay, UI, WorldGen, Education, Utility
  - Categorías: Vanilla, Gameplay, Cosmetic, QoL, Education, Experimental
- **ModLoader**: Carga segura con verificación
  - Verificación de dependencias antes de cargar
  - Verificación de firma criptográfica (claves públicas confiables)
  - Modo desarrollo: `AllowUnsignedMods`
  - Hot-reload opcional para desarrollo
- **ModRepository**: Distribución y descubrimiento
  - Descubrimiento local (`FindManifests`)
  - Instalación drag-and-drop (`InstallModFromDirectory`)
  - Descarga remota async (`DownloadModAsync`) con hash de integridad
  - Extracción ZIP automática, caché, directorio temporal
- **ModManager**: Coordinador principal
  - Install/Load/Unload/Reload mods
  - Filtrado por tipo (`GetModsByType`)
  - Verificación de actualizaciones (placeholder para backend)

## Tests Añadidos
- **DedicatedServerTests**: 5 tests (config, auth, tokens)
- **MatchmakingTests**: 13 tests (cola, lobby, estados, filtros)
- **ModTests**: 15 tests (manifest, dependencias, hash, instalación, repository, loader, manager)

## Arquitectura
- **Server Authority**: Todas las acciones validadas en servidor
- **Client Prediction**: Movimiento local con reconciliación (preparado)
- **Chunk Sync**: Serialización delta por región 16³ (base existente)
- **Transport**: KCP/UDP via Unity Transport (configurado en NetworkManager)

## Próximos Pasos (Iteración 0.7)
- Sincronización de chunks en tiempo real multijugador
- Predicción de movimiento cliente + reconciliación
- Sistema de chat y comandos
- Persistencia de mundo en servidor dedicado
- UI de lobby y matchmaking en cliente