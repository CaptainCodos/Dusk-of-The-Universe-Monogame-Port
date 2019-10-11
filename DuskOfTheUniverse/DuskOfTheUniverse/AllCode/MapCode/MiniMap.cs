using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class MiniMap
    {
        // This is a 2D array of map "pixels"
        private CenteredStaticGraphic[,] m_mapPixels;
        // This is the rectangle representing the current camera view
        private MotionGraphic m_mapBox;

        // Map dimensions
        private int m_mapSizeX;
        private int m_mapSizeY;

        public MiniMap(ContentManager content, UniMapData data, Camera cam)
        {
            m_mapSizeX = data.WalkableGrid.GetLength(0);
            m_mapSizeY = data.WalkableGrid.GetLength(1);

            m_mapPixels = new CenteredStaticGraphic[m_mapSizeX, m_mapSizeY];

            m_mapBox = new MotionGraphic(content.Load<Texture2D>("Art\\PlaceholderArt\\GameplayGUI\\MiniMapView"), new Vector2(1920 - 56, 1080 -31), Color.White, Vector2.Zero, 0, 1 / cam.Zoom);

            FillMinimapPixels(content, data);
        }

        private void FillMinimapPixels(ContentManager content, UniMapData data)
        {
            for (int y = 0; y < m_mapSizeY; y++)
            {
                for (int x = 0; x < m_mapSizeX; x++)
                {
                    m_mapPixels[x, y] = new CenteredStaticGraphic(content.Load<Texture2D>("Art\\PlaceholderArt\\GameplayGUI\\MiniMapPixel"), new Vector2(1920 - (240 - (x * 3)), 1080 - (135 - (y * 3))), Color.White, 1);

                    // Add colours to the pixels for colour coding
                    if (data.WalkableGrid[x, y] <= 0)
                    {
                        m_mapPixels[x, y].AltTints.Add(Color.White);
                        m_mapPixels[x, y].AltTints.Add(new Color(100, 255, 100));
                        m_mapPixels[x, y].AltTints.Add(Color.Blue);
                        m_mapPixels[x, y].AltTints.Add(Color.Yellow);
                        m_mapPixels[x, y].AltTints.Add(Color.Orange);
                        m_mapPixels[x, y].AltTints.Add(Color.Red);
                    }
                    else
                    {
                        m_mapPixels[x, y].AltTints.Add(Color.Black);
                        m_mapPixels[x, y].AltTints.Add(Color.Magenta);
                    }
                }
            }
        }

        // Position and scale the view box according to camera zoom and position
        public void UpdateMe(Camera cam, List<Tower> towers, List<EnemyChar> enemies, List<GameChar> friendlies)
        {
            float factorX = 2880 / 239;
            float factorY = 1620 / 134;

            Vector2 camDiff = cam.Position;

            int vecX = 1920 - (240 + (int)(-camDiff.X / factorX));
            int vecY = 1080 - (135 + (int)(-camDiff.Y / factorY));

            m_mapBox.Position = new Vector2(vecX, vecY);
            m_mapBox.Scale = 1 / cam.Zoom;

            m_mapBox.Layer = 0;

            ColourMap(towers, friendlies, enemies);
        }

        public void DrawMe(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            foreach (CenteredStaticGraphic n in m_mapPixels)
            {
                n.DrawMe(sb);
            }

            m_mapBox.DrawMe(sb);
            sb.End();
        }

        private void ColourMap(List<Tower> towers, List<GameChar> friendlies, List<EnemyChar> enemies)
        {
            ResetPixelColours();

            SetAllyPixelColours(friendlies);

            SetTowerPixelColours(towers);

            SetEnemyPixelColours(enemies);
        }

        #region Enemy Pixels
        // Colour pixels according to the enemy on tile
        private void SetEnemyPixelColours(List<EnemyChar> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Etype == EnemyChar.EnemyType.Normal)
                {
                    SetEnemyPixelColoursNorm(enemies, i);
                }
                else if (enemies[i].Etype == EnemyChar.EnemyType.Scout)
                {
                    SetEnemyPixelColoursScout(enemies, i);
                }
                else
                {
                    SetEnemyPixelColoursTank(enemies, i);
                }
            }
        }

        // Red
        private void SetEnemyPixelColoursTank(List<EnemyChar> enemies, int i)
        {
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Tint = m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].AltTints[5];
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Scale = 2;
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Layer = 0.1f;
        }
        // Orange
        private void SetEnemyPixelColoursScout(List<EnemyChar> enemies, int i)
        {
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Tint = m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].AltTints[4];
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Scale = 2;
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Layer = 0.1f;
        }
        // Yellow
        private void SetEnemyPixelColoursNorm(List<EnemyChar> enemies, int i)
        {
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Tint = m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].AltTints[3];
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Scale = 2;
            m_mapPixels[enemies[i].CoordsX, enemies[i].CoordsY].Layer = 0.1f;
        }
        #endregion

        #region Tower Pixels
        // Colour pixels according to the tower position and dimesnions (Magenta)
        private void SetTowerPixelColours(List<Tower> towers)
        {
            for (int i = 0; i < towers.Count; i++)
            {
                switch (towers[i].TowerDimensions)
                {
                    case 1:
                        SetTowerPixelColoursSingle(towers, i);
                        break;
                    case 2:
                        SetTowerPixelColoursDouble(towers, i);
                        break;
                    case 3:
                        SetTowerPixelColoursTriple(towers, i);
                        break;
                }
            }
        }

        // Colour pixel at position and each neighbouring pixel
        private void SetTowerPixelColoursTriple(List<Tower> towers, int i)
        {
            for (int Y = -1; Y < 2; Y++)
            {
                for (int X = -1; X < 2; X++)
                {
                    m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].Tint = m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].AltTints[1];
                    m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].Scale = 1;
                    m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].Layer = 0.1f;
                }
            }
        }
        // Colour pixel and pixels to right and below
        private void SetTowerPixelColoursDouble(List<Tower> towers, int i)
        {
            for (int Y = 0; Y < 2; Y++)
            {
                for (int X = 0; X < 2; X++)
                {
                    m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].Tint = m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].AltTints[1];
                    m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].Scale = 1;
                    m_mapPixels[towers[i].Coords.X + X, towers[i].Coords.Y + Y].Layer = 0.1f;
                }
            }
        }
        // Colour pixel at position
        private void SetTowerPixelColoursSingle(List<Tower> towers, int i)
        {
            m_mapPixels[towers[i].Coords.X, towers[i].Coords.Y].Tint = m_mapPixels[towers[i].Coords.X, towers[i].Coords.Y].AltTints[1];
            m_mapPixels[towers[i].Coords.X, towers[i].Coords.Y].Scale = 1;
            m_mapPixels[towers[i].Coords.X, towers[i].Coords.Y].Layer = 0.1f;
        }
        #endregion

        // Colour pixels with ally positions (VIP = light green, Guards = Blue)
        private void SetAllyPixelColours(List<GameChar> friendlies)
        {
            for (int i = 0; i < friendlies.Count; i++)
            {
                if (friendlies[i] is ImportantChar)
                {
                    m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].Tint = m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].AltTints[1];
                    m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].Scale = 2;
                    m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].Layer = 0.1f;
                }
                else
                {
                    m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].Tint = m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].AltTints[2];
                    m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].Scale = 2;
                    m_mapPixels[friendlies[i].CoordsX, friendlies[i].CoordsY].Layer = 0.1f;
                }
            }
        }

        // Reset Colours
        private void ResetPixelColours()
        {
            for (int y = 0; y < m_mapSizeY; y++)
            {
                for (int x = 0; x < m_mapSizeX; x++)
                {
                    m_mapPixels[x, y].Tint = m_mapPixels[x, y].AltTints[0];
                    m_mapPixels[x, y].Scale = 1;
                    m_mapPixels[x, y].Layer = 0.2f;
                }
            }
        }
    }
}
