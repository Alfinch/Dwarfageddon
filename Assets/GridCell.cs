using System;
using UnityEngine;

namespace Dwarfageddon
{
    public class GridCell : IDisposable
    {
        private GameObject _gridCell;

        public GridCell(GameObject gridCell)
        {
            _gridCell = gridCell;
        }

        public void Dispose()
        {
            GameObject.Destroy(_gridCell);
        }
    }
}
