using System;
using UnityEngine;

namespace Dwarfageddon
{
    public class GameGrid : MonoBehaviour
    {
        public GameObject GridCellPrefab;
        public int ChunkSize = 32; // in cells
        public int GridSize = 32; // in chunks

        public Vector2Int CenterScenePosition
        {
            get
            {
                var centerCoordinate = (int)Math.Floor(ChunkSize * 1.5f);
                return new Vector2Int(centerCoordinate, centerCoordinate);
            }
        }

        private GridCellGenerator _cellGenerator;
        private GridChunk[,] _loadedChunkSlots;
        private Vector2Int _focalChunkPosition;
        private Bounds _focalChunkBounds;

        private CameraMovement _camera;

        private void Awake()
        {
            _cellGenerator = new GridCellGenerator(GridCellPrefab, GridSize * ChunkSize);
            _loadedChunkSlots = new GridChunk[3,3];
            _focalChunkPosition = new Vector2Int(GridSize / 2, GridSize / 2);
            _focalChunkBounds = new Bounds((Vector2)CenterScenePosition, new Vector2(ChunkSize, ChunkSize));

            _camera = GetComponent<CameraMovement>();
        }

        private void Start()
        {
            _camera.ScenePostion = CenterScenePosition;
            LoadChunks();
        }

        private void FixedUpdate()
        {
            if (ScenePositionIsOutsideFocalChunk(_camera.ScenePostion))
            {
                var focalChunkOffset = Vector2Int.FloorToInt(_camera.ScenePostion / ChunkSize) - Vector2Int.one;
                var newFocalChunkPosition = _focalChunkPosition + focalChunkOffset;

                if (ChunkPositionIsOutsideGrid(newFocalChunkPosition))
                {
                    _camera.ScenePostion = _focalChunkBounds.ClosestPoint(_camera.ScenePostion);
                }
                else
                {
                    _camera.ScenePostion = _camera.ScenePostion - (focalChunkOffset * ChunkSize);
                    _focalChunkPosition = newFocalChunkPosition;
                    ShiftChunks(focalChunkOffset);
                    LoadChunks();
                }
            }
        }

        private void LoadChunks()
        {
            ForEachLoadedChunkSlot(slot =>
            {
                var scenePosition = new Vector2Int(slot.x * ChunkSize, slot.y * ChunkSize);
                var chunkPosition = _focalChunkPosition + new Vector2Int(slot.x - 1, slot.y - 1);

                if (ChunkPositionIsOutsideGrid(chunkPosition) || ChunkIsLoaded(chunkPosition))
                {
                    return;
                }

                var newChunk = new GridChunk(ChunkSize, chunkPosition, scenePosition, _cellGenerator);

                _loadedChunkSlots[slot.x, slot.y] = newChunk;
            });
        }

        private bool ScenePositionIsOutsideFocalChunk(Vector2 scenePostion)
        {
            return ChunkSize > scenePostion.x
                || scenePostion.x > ChunkSize * 2
                || ChunkSize > scenePostion.y
                || scenePostion.y > ChunkSize * 2;
        }

        private bool ChunkPositionIsOutsideGrid(Vector2Int chunkPosition)
        {
            return 0 > chunkPosition.x
                || chunkPosition.x > GridSize
                || 0 > chunkPosition.y
                || chunkPosition.y > GridSize;
        }

        private bool ChunkIsLoaded(Vector2Int chunkPosition)
        {
            for (var x = 0; x < 3; ++x)
            {
                for (var y = 0; y < 3; ++y)
                {
                    var chunk = _loadedChunkSlots[x, y];
                    if (chunk != null && chunk.ChunkPosition == chunkPosition)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ForEachLoadedChunkSlot(Action<GridChunk> action)
        {
            ForEachLoadedChunkSlot(slot => action(_loadedChunkSlots[slot.x, slot.y]));
        }

        private void ForEachLoadedChunkSlot(Action<Vector2Int> action)
        {
            for (var x = 0; x < 3; ++x)
            {
                for (var y = 0; y < 3; ++y)
                {
                    action(new Vector2Int(x, y));
                }
            }
        }

        private void ShiftChunks(Vector2Int focalChunkOffset)
        {
            var shiftedChunks = new GridChunk[3, 3];

            ForEachLoadedChunkSlot(slot =>
            {
                var oldChunk = _loadedChunkSlots[slot.x, slot.y];

                if (oldChunk != null)
                {
                    var newChunkPosition = slot - focalChunkOffset;

                    if (LoadedChunkSlotExists(newChunkPosition) == false)
                    {
                        oldChunk.Dispose();
                        return;
                    }

                    oldChunk.MoveTo(newChunkPosition * ChunkSize);
                    shiftedChunks[newChunkPosition.x, newChunkPosition.y] = oldChunk;
                }
            });

            _loadedChunkSlots = shiftedChunks;
        }

        private bool LoadedChunkSlotExists(Vector2Int slot)
        {
            return 0 <= slot.x && slot.x <= 2 && 0 <= slot.y && slot.y <= 2;
        }
    }
}
