# Plan 0.6: Multijugador Avanzado

## Objetivo
Transformar la base de networking en una experiencia multijugador completa con servidor dedicado, autenticación y sincronización de mundo.

## Alcance
- **T001**: Servidor Dedicado (Headless) + Autenticación
- **T002**: Matchmaking y Lobby
- **T003**: Soporte Mods/Comunidad

## Stack
- Unity 2022.3 LTS / C# 10
- Netcode for GameObjects 1.8
- Transport: KCP (UDP) para mejor rendimiento en voxel

## Arquitectura
- **Server Authority**: Todos los cambios de mundo validados en servidor
- **Client Prediction**: Movimiento local inmediato con reconciliación
- **Chunk Sync**: Serialización delta por región (16x16x16)