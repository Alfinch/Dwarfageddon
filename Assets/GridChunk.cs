using System;
using UnityEngine;

namespace Dwarfageddon
{
    public class GridChunk : IDisposable
    {
        private GameObject _container;
        private GridCell[,] _cells;
        private int _chunkSize;

        public Vector2Int ChunkPosition { get; private set; }

        public GridChunk(int chunkSize, Vector2Int chunkPosition, Vector2 scenePosition, GridCellGenerator cellGenerator)
        {
            _chunkSize = chunkSize;

            ChunkPosition = chunkPosition;

            _container = new GameObject();

            _cells = new GridCell[chunkSize, chunkSize];

            ForEachCell(position =>
            {
                var gridPosition = (chunkPosition * _chunkSize) + position;
                var cell = cellGenerator.GetCell(gridPosition, position, _container.transform);
                _cells[position.x, position.y] = cell;
            });

            MoveTo(scenePosition);
        }

        public void MoveTo(Vector2 scenePosition)
        {
            _container.transform.SetPositionAndRotation(scenePosition, Quaternion.identity);
        }

        public GridCell GetCell(Vector2Int position)
        {
            return _cells[position.x, position.y];
        }

        public void Dispose()
        {
            ForEachCell(cell => cell.Dispose());
            GameObject.Destroy(_container);
        }

        private void ForEachCell(Action<GridCell> action)
        {
            ForEachCell(position => action(_cells[position.x, position.y]));
        }

        private void ForEachCell(Action<Vector2Int> action)
        {
            for (var x = 0; x < _chunkSize; ++x)
            {
                for (var y = 0; y < _chunkSize; ++y)
                {
                    action(new Vector2Int(x, y));
                }
            }
        }
    }
}
