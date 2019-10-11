using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class PathGen
    {
        // Current point from which a length of walkable nodes / tiles will be generated. (these values will be used like indexs for arrays / lists)
        private int m_currX;
        private int m_currY;

        // The direction the path has just been generated in (helps stopping it from generating on itself).
        /*
        0 = up
        1 = right
        2 = down
        Paths will not generate backward (Left).
        */
        private int m_currDir;
        private int m_newDir;

        // Length that the path will generate before it works out how to generate next length of walkable nodes / tiles.
        private int m_nextLength;
        private int m_maxLength;

        // Number of paths to be generated.
        private int m_pathNo;

        // Give the generator a grid of ints to work with. 0 is walkable, anything else is not. 2 or 3 are unplaceable tiles.
        private UniMapData m_data;

        // Size of the map in X and Y dimensions.
        private int m_mapSizeX;
        private int m_mapSizeY;

        public UniMapData Data { get { return m_data; } set { m_data = value; } }

        public PathGen(int sizeX, int sizeY, int maxLength)
        {
            m_data = new UniMapData(sizeX, sizeY);

            m_mapSizeX = sizeX;
            m_mapSizeY = sizeY;

            m_maxLength = maxLength;

            // Get a random number of paths to generate.
            m_pathNo = Game1.RNG.Next(3, 7);

            // For each path to be generated.
            for (int i = 0; i < m_pathNo; i++)
            {
                // This boolean is used to see if the generator has drawn the first path.
                // This will ensure that the first section made is not going up or down.
                bool hasDrawn = false;

                // Set the current X index to 0. 
                m_currX = 0;

                // Set the current direction as forward.
                m_currDir = 3;
                m_newDir = 1;

                // Pick a starting Y coordinate.
                m_currY = Game1.RNG.Next(1, sizeY - 1);

                // Checks to see if a path has already been generated next to this point. anything 0 or less is walkable, anything else is not.
                if (m_currY > 0 && m_currY < sizeY - 1)
                {
                    while (m_data.WalkableGrid[0, m_currY + 1] <= 0 || m_data.WalkableGrid[0, m_currY - 1] <= 0)
                    {
                        m_currY = Game1.RNG.Next(1, sizeY - 1);
                    }
                }

                m_data.WalkableGrid[0, m_currY] = 0;

                // It will generate path until it reaches the end.
                while (m_currX < sizeX - 1)
                {
                    if (hasDrawn)
                        m_newDir = Game1.RNG.Next(0, 3);

                    switch (m_newDir)
                    {
                        case 0:
                            GenerateUpward();
                            hasDrawn = true;
                            break;
                        case 1:
                            GenerateForward(sizeX);
                            hasDrawn = true;
                            break;
                        case 2:
                            GenerateDownward(sizeY);
                            hasDrawn = true;
                            break;
                        default:
                            // Try and get a new path direction.
                            m_newDir = Game1.RNG.Next(0, 3);
                            break;

                    }
                }
            }

            for (int y = 0; y < m_mapSizeY; y++)
            {
                for (int x = 0; x < m_mapSizeX; x++)
                {
                    MakeUnplaceableTiles(x, y);
                }
            }

            MakeFoliage();
        }

        // This method creates foliage on rough, unplaceable tiles
        private void MakeFoliage()
        {
            // Checks if tile is an unplaceable, then has a chance to make some foliage
            for (int y = 0; y < m_mapSizeY; y++)
            {
                for (int x = 0; x < m_mapSizeX; x++)
                {
                    int randChance = Game1.RNG.Next(0, 10);

                    if (randChance == 0 && m_data.WalkableGrid[x, y] > 1)
                    {
                        m_data.WalkableGrid[x, y] = 3;
                    }
                }
            }
        }

        // This method creates tiles that are rough and cannon have towers place on them
        private void MakeUnplaceableTiles(int x, int y)
        {
            // If The tile is unwalkable
            if (m_data.WalkableGrid[x, y] == 1)
            {
                int randX = Game1.RNG.Next(2, 4);
                int randY = Game1.RNG.Next(2, 4);

                // Take a small radom area and search around the tile
                for (int Y = -randY; Y < randY; Y++)
                {
                    for (int X = -randX; X < randX; X++)
                    {
                        int checkX = X + x;
                        int checkY = Y + y;

                        bool isOnGrid = false;

                        if ((checkX >= 0 && checkX < m_mapSizeX) && (checkY >= 0 && checkY < m_mapSizeY))
                            isOnGrid = true;
                        else
                            isOnGrid = false;

                        // If a walkable tile is found then return, otherwise continue
                        if (isOnGrid)
                        {
                            if (m_data.WalkableGrid[checkX, checkY] == 0)
                                return;
                            else
                                continue;
                        }
                    }
                }

                // Set tile as an unplaceable tile
                m_data.WalkableGrid[x, y] = 2;
            }
        }

        // This method generates path downward.
        private void GenerateDownward(int sizeY)
        {
            // If the new direction generated is not going back on the path, and there is room to generate, create the length of path.
            if (m_currDir != 0 && m_currY < sizeY - 1 && m_currDir != 2)
            {
                m_nextLength = Game1.RNG.Next(2, m_maxLength);

                for (int k = 0; k < m_nextLength && m_currY + k < sizeY - 1; k++)
                {
                    // Create downward path with width of 2.
                    m_currY++;
                    m_data.WalkableGrid[m_currX, m_currY] = 0;
                }

                // Set path direction.
                m_currDir = m_newDir;
            }
        }
        // This method generates path rightward (int this case forward)
        private void GenerateForward(int sizeX)
        {
            // If there is room to generate a length of path.
            if (m_currX < sizeX - 1 && m_currDir != 1)
            {
                m_nextLength = Game1.RNG.Next(3, m_maxLength);

                for (int k = 1; k < m_nextLength && m_currX + k < sizeX; k++)
                {
                    // Create forward path with width of 2.
                    m_currX++;
                    m_data.WalkableGrid[m_currX, m_currY] = 0;
                }

                // Set path direction.
                m_currDir = m_newDir;
            }
        }
        // This method generates path upward
        private void GenerateUpward()
        {
            // If the new direction generated is not going back on the path, and there is room to generate, create the length of path.
            if (m_currDir != 2 && m_currDir != 0 && m_currY > 0)
            {
                m_nextLength = Game1.RNG.Next(2, m_maxLength);

                for (int k = 1; k < m_nextLength && m_currY - k > 0; k++)
                {
                    // Create upward path with width of 2.
                    m_currY--;
                    m_data.WalkableGrid[m_currX, m_currY] = 0;
                }

                // Set path direction.
                m_currDir = m_newDir;
            }
        }
    }
}
