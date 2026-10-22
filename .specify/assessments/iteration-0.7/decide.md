# Decide 0.7

## Decisiones Técnicas

### Chunk Sync
- **Pull model**: Cliente solicita chunks cercanos (`RequestChunkDataServerRpc`)
- **Push model**: Servidor envía `BroadcastBlockUpdateClientRpc` para ediciones
- **Frecuencia**: 20 updates/seg para bloques, chunks bajo demanda

### Client Prediction
- **Tick rate**: 60 Hz (FixedUpdate)
- **Buffer**: Últimos 1024 inputs por cliente
- **Reconciliación**: Cada 3 ticks servidor envía posición autoritativa

### Chat & Commands
- **RPC**: `SendChatMessageServerRpc` → `ReceiveChatMessageClientRpc`
- **Comandos**: Solo servidor ejecuta, respuesta via Chat RPC

### Persistencia
- **Thread**: Background thread para serialización (evitar lag)
- **Checkpoint**: SaveSystem existente compatible

### UI
- **UI Toolkit** (Unity UI Toolkit) para escalabilidad
- **Localización**: Sistema de keys para múltiples idiomas

## Riesgos
- Latencia alta en chunk sync inicial (muchos datos)
- Desincronización por predicción incorrecta
- Memoria: chunks cargados por múltiples clientes