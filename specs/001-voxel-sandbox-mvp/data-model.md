# Data Model

## Mundo
- chunkX: int
- chunkZ: int
- blocks: Block[16][16][256]

## Block
- typeId: int
- metadata: int

## Jugador
- position: Vector3
- inventory: Item[]
- health: float
- hunger: float

## Item
- id: string
- count: int

## Receta
- inputs: Item[]
- output: Item
