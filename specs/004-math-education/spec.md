# Feature Specification: Sistema Matemático Educativo

**Feature Branch**: 004-math-education

## User Stories
- US1: Jugador resuelve acertijos matemáticos para desbloquear mapas
- US2: Progresión de skins ligada a logros matemáticos
- US3: Tareas adaptativas por nivel educativo 1°-3° secundaria

## Requirements
- FR-001: Generador de problemas por nivel
- FR-002: Validación de respuesta con feedback pedagógico
- FR-003: Desbloqueo de contenido por logros

## Re-arquitectura
- Añadir capa Domain MathService entre Core y Player
- Separar Gameplay de Educativo vía interfaces
- Persistencia de progreso educativo en SaveSystem
