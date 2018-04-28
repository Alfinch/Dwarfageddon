using UnityEngine;

namespace Dwarfageddon
{
    public class GridCellGenerator
    {
        private const float LargeVariation = .02f;
        private const float MediumVariation = .04f;
        private const float SmallVariation = .1f;

        private GameObject _cellPrefab;
        private int _heightSeed;
        private int _variationSeed;
        private float _gridSize;
        
        /// <summary>
        /// A class used to procedurally generate grid cells
        /// </summary>
        /// <param name="cellPrefab">The game object which all cells are cloned from</param>
        /// <param name="gridSize">The total size of the grid in both the x and y directions</param>
        public GridCellGenerator(GameObject cellPrefab, int gridSize)
        {
            _cellPrefab = cellPrefab;
            _heightSeed = Random.Range(0, 9999);
            _variationSeed = Random.Range(10000, 19999);
            _gridSize = gridSize;
        }

        /// <summary>
        /// Generates a grid cell
        /// </summary>
        /// <param name="gridPosition">The position of the cell in the entire grid</param>
        /// <param name="chunkPosition">The position of the cell in the parent chunk</param>
        /// <param name="chunk">The transform of the parent chunk</param>
        /// <returns></returns>
        public GridCell GetCell(Vector2Int gridPosition, Vector2Int chunkPosition, Transform chunk)
        {
            var heightModifier = GetHeightModifier(gridPosition);
            var height = GetPerlinValue(gridPosition, _heightSeed) * heightModifier;
            var variation = GetPerlinValue(gridPosition * 2, _variationSeed);

            var gridCell = GameObject.Instantiate(_cellPrefab, (Vector2)chunkPosition, Quaternion.identity, chunk);
            var spriteRenderer = gridCell.GetComponent<SpriteRenderer>();

            if (height > .68f)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("Tiles/Snow");
            }
            else if (height > .56f)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("Tiles/Stone");
            }
            else if (height > .4f)
            {
                if (variation > .6f)
                {
                    spriteRenderer.sprite = Resources.Load<Sprite>("Tiles/Dirt");
                }
                else
                {
                    spriteRenderer.sprite = Resources.Load<Sprite>("Tiles/Grass");
                }
            }
            else if (height > .35f)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("Tiles/Sand");
            }
            else
            {
                spriteRenderer.sprite = Resources.Load<Sprite>("Tiles/Water");
                var brightness = ((height * (1 / .35f)) + 1) / 2;
                spriteRenderer.color = new Color(brightness, brightness, brightness);
            }

            return new GridCell(gridCell);
        }

        private float GetHeightModifier(Vector2Int gridPosition)
        {
            var xProportion = (gridPosition.x / _gridSize);
            var yProportion = (gridPosition.y / _gridSize);

            var xModifier = xProportion < .5f ? Mathf.Min(xProportion * 5, 1) : Mathf.Min((1f - xProportion) * 5, 1);
            var yModifier = yProportion < .5f ? Mathf.Min(yProportion * 5, 1) : Mathf.Min((1f - yProportion) * 5, 1);

            return xModifier * yModifier;
        }

        private float GetPerlinValue(Vector2Int gridPosition, int seed = 0)
        {
            var x = gridPosition.x + seed;
            var y = gridPosition.y + seed;

            var largeVariation = Mathf.PerlinNoise(x * LargeVariation, y * LargeVariation);
            var mediumVariation = Mathf.PerlinNoise(x * MediumVariation, y * MediumVariation);
            var smallVariation = Mathf.PerlinNoise(x * SmallVariation, y * SmallVariation);

            return (largeVariation + (mediumVariation / 2) + (smallVariation / 4)) / 1.75f;
        }
    }
}