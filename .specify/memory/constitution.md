<!-- Sync Impact Report
Version change: 1.0.0 initial creation
Modified principles: N/A
Added sections: Core Principles, Additional Constraints, Development Workflow, Governance
Removed sections: N/A
-->

# Voxel Sandbox Libre Constitution

## Core Principles

### I. No IP Copy, Clean Room
Todo el código, arte, sonido y diseño de UI se desarrolla desde cero con estilo propio. Se prohíbe reutilizar assets, nombres, texturas o sonidos de Minecraft. Las mecánicas genéricas son aceptables, las expresiones específicas no.

### II. Test-First Obligatorio
TDD es no negociable: tests redactados y aprobados, fallan, implementar, pasan, refactorizar. Red-Green-Refactor estricto.

### III. Rendimiento Como Requisito
Mundo abierto con chunks streaming a 60 FPS en hardware de referencia. Meshing optimizado, culling, LOD y profiling continuo.

### IV. Seguridad y Privacidad
OWASP Top Ten, validación de inputs, autorización estricta, cifrado en tránsito. Sin datos personales innecesarios.

### V. Observabilidad y Versionado
Logging estructurado, métricas y trazabilidad. Versionado SemVer. Breaking changes documentados con plan de migración.

## Additional Constraints

Tecnología: motor con soporte voxel o desarrollo propio modular. Postgres para metadatos. CI/CD con gates de calidad. CONSTRAINTS.md prevalece sobre decisiones ad-hoc.

## Development Workflow

Code review obligatorio, OCR → ponytail-review → code-review-and-quality. Cambios de BD con supabase-postgres-best-practices. Commits contextuales con git-workflow-and-versioning.

## Governance

La Constitución prevalece sobre prácticas. Enmiendas requieren documentación, aprobación y plan de migración. Complejidad debe justificarse.

**Version**: 1.0.0 | **Ratified**: 2025-10-15 | **Last Amended**: 2025-10-15
