using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    // Class contains universal map data that can be used for both the level map and pathfinding node graphs.
    class UniMapData
    {
        // Multidimensional array of walkable tiles.
        // Can also be used to determine where textures should be different (Corners for example).
        private int[,] m_walkableGrid;

        // Accessor for the walkable data, to be used in pathfinding graph creation and tile map paths.
        public int[,] WalkableGrid
        {
            get { return m_walkableGrid; }
            set { m_walkableGrid = value; }
        }

        public UniMapData(int mapSizeX, int mapSizeY)
        {
            m_walkableGrid = new int[mapSizeX, mapSizeY];

            for (int y = 0; y < mapSizeY; y++)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    m_walkableGrid[x, y] = 1;
                }
            }
        }
    }
}
