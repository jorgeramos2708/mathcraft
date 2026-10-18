# Tasks: Voxel Sandbox MVP

**Input**: Design documents from `/specs/001-voxel-sandbox-mvp/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md

## Phase 1: Setup

- [ ] T001 Create Unity project structure per implementation plan Assets/Scripts/Core, World, Player, UI
- [ ] T002 Initialize Unity 2022.3 LTS project with Input System package
- [ ] T003 [P] Configure linting and formatting tools for C#

## Phase 2: Foundational

- [ ] T004 Setup voxel core library Assets/Scripts/Core/VoxelEngine
- [ ] T005 [P] Implement Chunk data structure in Assets/Scripts/World/Chunk.cs
- [ ] T006 [P] Implement Greedy Meshing in Assets/Scripts/World/Mesher.cs
- [ ] T007 Setup save/load JSON framework in Assets/Scripts/Core/SaveSystem
- [ ] T008 Configure error handling and logging

**Checkpoint**: Foundation ready

## Phase 3: User Story 1 - Explorar y construir mundo voxel P1

**Goal**: Jugador puede explorar, romper y colocar bloques

**Independent Test**: Crear mundo nuevo y verificar interacción de bloques

- [ ] T009 [P] [US1] Create Block model in Assets/Scripts/World/Block.cs
- [ ] T010 [P] [US1] Create WorldGenerator in Assets/Scripts/World/WorldGenerator.cs
- [ ] T011 [US1] Implement PlayerController movimiento y colisión in Assets/Scripts/Player/PlayerController.cs
- [ ] T012 [US1] Implement Block interaction romper/colocar in Assets/Scripts/World/BlockInteraction.cs
- [ ] T013 [US1] Implement Hotbar UI in Assets/Scripts/UI/Hotbar.cs
- [ ] T014 [US1] Add persistencia de bloques al guardar

**Checkpoint**: US1 funcional

## Phase 4: User Story 2 - Supervivencia básica P2

**Goal**: Recolección, inventario, crafting

- [ ] T015 [P] [US2] Create Item model in Assets/Scripts/Core/Item.cs
- [ ] T016 [P] [US2] Create Inventory system in Assets/Scripts/Player/Inventory.cs
- [ ] T017 [US2] Implement crafting recipes en Assets/Scripts/Core/Crafting.cs
- [ ] T018 [US2] Implement salud/hambre en Assets/Scripts/Player/Stats.cs
- [ ] T019 [US2] Implement ciclo día/noche en Assets/Scripts/World/DayNightCycle.cs

## Phase 5: User Story 3 - Estilo propio sin IP P3

**Goal**: Assets originales

- [ ] T020 [P] [US3] Create estilo visual propio assets en Assets/Art
- [ ] T021 [P] [US3] Auditar assets contra IP en docs/IP_Audit.md
- [ ] T022 [US3] Reemplazar nombres de items por nombres propios

## Phase 6: Polish

- [ ] T023 [P] Performance profiling 60 fps
- [ ] T024 Documentación quickstart validation
