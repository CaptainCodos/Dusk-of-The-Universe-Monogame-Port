using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class TileMerger
    {
        private Color[] m_colours1;
        private Color[] m_colours2;
        private Texture2D m_alterSheet;

        private Map m_map;
        private UniMapData m_data;

        private int m_sizeX;
        private int m_sizeY;

        public TileMerger(Map map, UniMapData data)
        {
            m_map = map;
            m_data = data;

            m_sizeX = m_data.WalkableGrid.GetLength(0);
            m_sizeY = m_data.WalkableGrid.GetLength(1);
        }

        public void UpdateMerger(GraphicsDevice graphics)
        {
            for (int y = 0; y < m_sizeY; y++)
            {
                for (int x = 0; x < m_sizeX; x++)
                {
                    m_colours1 = new Color[m_map.Tiles[x, y].Txr.Width * m_map.Tiles[x, y].Txr.Height];
                    m_colours2 = new Color[m_map.Tiles[x, y].Txr.Width * m_map.Tiles[x, y].Txr.Height];
                    
                    IterateNeighbours(x, y, ref m_map.Tiles[x, y].m_txr, graphics);
                }
            }
        }

        private void IterateNeighbours(int X, int Y, ref Texture2D txrSheet, GraphicsDevice graphics)
        {
            #region OuterCorners
            int lowerIndex = 0;

            if ((X - 1 >= 0 && Y - 1 >= 0) && (m_data.WalkableGrid[X - 1, Y] < m_data.WalkableGrid[X, Y] && m_data.WalkableGrid[X, Y - 1] < m_data.WalkableGrid[X, Y]))
            {
                txrSheet = CreateTextureMemory(txrSheet, graphics);

                // Checks which tile has a lower index
                if (m_data.WalkableGrid[X - 1, Y] < m_data.WalkableGrid[X, Y - 1])
                    lowerIndex = m_data.WalkableGrid[X, Y - 1];
                else
                    lowerIndex = m_data.WalkableGrid[X - 1, Y];

                CreateTopLeftCorner(txrSheet, 2 + (m_data.WalkableGrid[X, Y] * 40), 2 + ((m_map.LevelNumber - 1) * 40), m_data.WalkableGrid[X, Y], lowerIndex);
            }
            if ((X + 1 < m_sizeX && Y - 1 >= 0) && (m_data.WalkableGrid[X + 1, Y] < m_data.WalkableGrid[X, Y] && m_data.WalkableGrid[X, Y - 1] < m_data.WalkableGrid[X, Y]))
            {
                txrSheet = CreateTextureMemory(txrSheet, graphics);

                // Checks which tile has a lower index
                if (m_data.WalkableGrid[X + 1, Y] < m_data.WalkableGrid[X, Y - 1])
                    lowerIndex = m_data.WalkableGrid[X, Y - 1];
                else
                    lowerIndex = m_data.WalkableGrid[X + 1, Y];

                CreateTopRightCorner(txrSheet, 2 + (m_data.WalkableGrid[X, Y] * 40) + 30, 2 + ((m_map.LevelNumber - 1) * 40), m_data.WalkableGrid[X, Y], lowerIndex);
            }
            if ((X - 1 >= 0 && Y + 1 < m_sizeY) && (m_data.WalkableGrid[X - 1, Y] < m_data.WalkableGrid[X, Y] && m_data.WalkableGrid[X, Y + 1] < m_data.WalkableGrid[X, Y]))
            {
                txrSheet = CreateTextureMemory(txrSheet, graphics);

                // Checks which tile has a lower index
                if (m_data.WalkableGrid[X - 1, Y] < m_data.WalkableGrid[X, Y + 1])
                    lowerIndex = m_data.WalkableGrid[X, Y + 1];
                else
                    lowerIndex = m_data.WalkableGrid[X - 1, Y];

                CreateBottomLeftCorner(txrSheet, 2 + (m_data.WalkableGrid[X, Y] * 40), 2 + ((m_map.LevelNumber - 1) * 40) + 30, m_data.WalkableGrid[X, Y], lowerIndex);
            }
            if ((X + 1 < m_sizeX && Y + 1 < m_sizeY) && (m_data.WalkableGrid[X + 1, Y] < m_data.WalkableGrid[X, Y] && m_data.WalkableGrid[X, Y + 1] < m_data.WalkableGrid[X, Y]))
            {
                txrSheet = CreateTextureMemory(txrSheet, graphics);

                // Checks which tile has a lower index
                if (m_data.WalkableGrid[X + 1, Y] < m_data.WalkableGrid[X, Y + 1])
                    lowerIndex = m_data.WalkableGrid[X, Y + 1];
                else
                    lowerIndex = m_data.WalkableGrid[X + 1, Y];

                CreateBottomRightCorner(txrSheet, 2 + (m_data.WalkableGrid[X, Y] * 40) + 30, 2 + ((m_map.LevelNumber - 1) * 40) + 30, m_data.WalkableGrid[X, Y], lowerIndex);
            }
            #endregion
        }

        /// <summary>
        /// Allocate unique memory for the tiles texture, to separate the main loaded texture from this tile's texture that is about to be editted
        /// </summary>
        /// <param name="txrSheet"> Sheet to copy </param>
        /// <param name="graphics"> Make new texture </param>
        /// <returns></returns>
        private static Texture2D CreateTextureMemory(Texture2D txrSheet, GraphicsDevice graphics)
        {
            Color[] m_txrData = new Color[txrSheet.Width * txrSheet.Height];
            txrSheet.GetData(m_txrData);
            Texture2D m_txr = new Texture2D(graphics, txrSheet.Width, txrSheet.Height);
            m_txr.SetData(m_txrData);
            txrSheet = m_txr;
            return txrSheet;
        }

        // Blends top left corner of the current tile with the corner of the appropriate tile (Texture editting)
        private void CreateTopLeftCorner(Texture2D txrSheet, int startX, int startY, int upperGridIndex, int lowerGridIndex)
        {
            m_alterSheet = txrSheet;
            m_alterSheet.GetData(m_colours1);
            m_alterSheet.GetData(m_colours2);

            int minX = startX;
            int maxX = startX + 6;

            int minY = startY;
            int maxY = startY + 6;

            for (int y = 0; y < m_alterSheet.Height; y++)
            {
                for (int x = 0; x < m_alterSheet.Width; x++)
                {
                    // Converting 2D array index into 1D array index (since XNA texture data is a 1D array
                    int index = (x + (y * m_alterSheet.Width));

                    // Edit Corner
                    if ((index >= (minX + (y * m_alterSheet.Width)) && index < (maxX + (y * m_alterSheet.Width))) && (y >= minY && y < maxY))
                    {
                        m_colours2[index] = m_colours1[index - (40 * (upperGridIndex - lowerGridIndex))];
                        
                        if (index >= (maxX + (y * m_alterSheet.Width) - 1))
                        {
                            maxX--;
                        }
                    }
                }
            }

            m_alterSheet.SetData(m_colours2);
        }

        // Blends top right corner of the current tile with the corner of the appropriate tile (Texture editting)
        private void CreateTopRightCorner(Texture2D txrSheet, int startX, int startY, int upperGridIndex, int lowerGridIndex)
        {
            m_alterSheet = txrSheet;
            m_alterSheet.GetData(m_colours1);
            m_alterSheet.GetData(m_colours2);

            int minX = startX;
            int maxX = startX + 6;

            int minY = startY;
            int maxY = startY + 6;

            for (int y = 0; y < m_alterSheet.Height; y++)
            {
                for (int x = 0; x < m_alterSheet.Width; x++)
                {
                    // Converting 2D array index into 1D array index (since XNA texture data is a 1D array
                    int index = (x + (y * m_alterSheet.Width));

                    // Edit Corner
                    if ((index >= (minX + (y * m_alterSheet.Width)) && index < (maxX + (y * m_alterSheet.Width))) && (y >= minY && y < maxY))
                    {
                        m_colours2[index] = m_colours1[index - 40];

                        if (index >= (maxX + (y * m_alterSheet.Width) - 1))
                        {
                            minX++;
                        }
                    }
                }
            }

            m_alterSheet.SetData(m_colours2);
        }

        // Blends Bottom left corner of the current tile with the corner of the appropriate tile (Texture editting)
        private void CreateBottomLeftCorner(Texture2D txrSheet, int startX, int startY, int upperGridIndex, int lowerGridIndex)
        {
            m_alterSheet = txrSheet;
            m_alterSheet.GetData(m_colours1);
            m_alterSheet.GetData(m_colours2);

            int minX = startX;
            int maxX = startX + 6;

            int minY = startY;
            int maxY = startY + 6;

            for (int y = m_alterSheet.Height - 1; y >= 0; y--)
            {
                for (int x = m_alterSheet.Width - 1; x >= 0; x--)
                {
                    // Converting 2D array index into 1D array index (since XNA texture data is a 1D array
                    int index = (x + (y * m_alterSheet.Width));

                    // Edit Corner
                    if ((index >= (minX + (y * m_alterSheet.Width)) && index < (maxX + (y * m_alterSheet.Width))) && (y >= minY && y < maxY))
                    {
                        m_colours2[index] = m_colours1[index - 40];

                        if (index < (minX + (y * m_alterSheet.Width) + 1))
                        {
                            maxX--;
                        }
                    }
                }
            }

            m_alterSheet.SetData(m_colours2);
        }

        // Blends bottom right corner of the current tile with the corner of the appropriate tile (Texture editting)
        private void CreateBottomRightCorner(Texture2D txrSheet, int startX, int startY, int upperGridIndex, int lowerGridIndex)
        {
            m_alterSheet = txrSheet;
            m_alterSheet.GetData(m_colours1);
            m_alterSheet.GetData(m_colours2);

            int minX = startX;
            int maxX = startX + 6;

            int minY = startY;
            int maxY = startY + 6;

            for (int y = m_alterSheet.Height - 1; y >= 0; y--)
            {
                for (int x = m_alterSheet.Width - 1; x >= 0; x--)
                {
                    // Converting 2D array index into 1D array index (since XNA texture data is a 1D array
                    int index = (x + (y * m_alterSheet.Width));

                    // Edit Corner
                    if ((index >= (minX + (y * m_alterSheet.Width)) && index < (maxX + (y * m_alterSheet.Width))) && (y >= minY && y < maxY))
                    {
                        m_colours2[index] = m_colours1[index - 40];

                        if (index < (minX + (y * m_alterSheet.Width) + 1))
                        {
                            minX++;
                        }
                    }
                }
            }

            m_alterSheet.SetData(m_colours2);
        }
    }
}
