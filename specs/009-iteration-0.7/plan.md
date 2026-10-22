# Plan 0.7: Sincronización en Tiempo Real y Experiencia de Juego

## Objetivo
Convertir la base multijugador en una experiencia jugable completa con sincronización fluida, chat, comandos y persistencia.

## Stack
- Unity 2022.3 LTS / C# 10
- Netcode for GameObjects 1.8
- UI Toolkit (UnityEngine.UIElements)
- SaveSystem existente (JSON)

## Arquitectura
- **Server Authority** mantenido para todo estado crítico
- **Client Prediction** para movimiento suave (60 Hz)
- **Chunk Sync** híbrido: pull (carga inicial) + push (ediciones)
- **Persistencia** asíncrona en background thread

## Fases
1. **T001**: Chunk Sync en tiempo real + Client Prediction
2. **T002**: Chat System + Comandos de servidor
3. **T003**: Persistencia mundo servidor + Auto-save
4. **T004**: Client UI (Lobby, InGame, Chat, Settings)