# CONSTRAINTS.md

## Calidad no negociable
- Tests para toda lógica core, TDD obligatorio.
- Seguridad OWASP Top Ten en inputs, auth, red.
- Accesibilidad WCAG AA en UI.
- Rendimiento: 60 FPS en mundo de prueba con 100 chunks cargados.

## Legal
- Ningún asset, nombre, textura, sonido de Minecraft.
- Mecánicas genéricas sí, implementaciones específicas no copiar.
- No usar marcas registradas Mojang/Microsoft.

## Alcance
- Estilo visual propio, no voxel cúbico idéntico? Puede ser voxel pero con estética propia.
- MVP sin multijugador persistente.

## Límites técnicos
- Motor: preferir Unity/Unreal o open source con soporte voxel.
- No @ts-ignore, no eslint-disable sin justificación.
