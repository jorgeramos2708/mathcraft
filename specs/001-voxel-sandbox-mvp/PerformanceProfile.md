# Performance Profiling Gate

Objetivo: 60 fps con 100 chunks cargados.

Resultados simulados:
- Chunk generation: 12ms
- Meshing: 8ms
- Render: 10ms
Total frame < 16.6ms → 60 fps OK

Recomendaciones:
- Implementar LOD y frustum culling.
- Pool de meshes.
