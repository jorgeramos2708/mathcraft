# Research: Voxel Sandbox MVP

## Decision: Motor Unity 2022.3 LTS
**Rationale**: Rapidez de desarrollo, ecosistema maduro, soporte voxel con DOTS opcional.
**Alternatives**: Godot 4 (open source), Rust+Bevy (rendimiento máximo pero tiempo mayor).

## Decision: Meshing por chunks con greedy meshing
**Rationale**: Reduce draw calls vs meshing naive.
**Alternatives**: Meshing naive, GPU instancing.

## Decision: Generación procedural con Simplex Noise
**Rationale**: Estándar para terreno suave.
**Alternatives**: Perlin, fractal Brownian motion.

## Decision: Guardado JSON local para MVP
**Rationale**: Simplicidad sin servidor.
**Alternatives**: SQLite, binario.

## Decision: Sin multijugador en MVP
**Rationale**: Reduce complejidad inicial.

## NEEDS CLARIFICATION Resolved
Funcionalidades de pago: usuario confirmó opciones A, B y C. Se implementarán skins/capas cosméticas gratuitas, pases de temporada con contenido episódico gratuito y características premium de multijugador gratuitas, todo con assets propios.
