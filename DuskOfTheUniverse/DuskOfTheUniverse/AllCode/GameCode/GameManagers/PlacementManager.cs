using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class PlacementManager
    {
        // 2D array of tiles for placing and moving towers
        private MapTile[,] m_tiles;

        // Determines if placing a tower or moving a tower
        private bool m_placingActive;
        private bool m_movingActive;

        // Vector to hold mouse position to be transform to world coordinates
        private Vector2 m_mousePos;

        // Tower to be moved and placed with the moving tower's menu
        private Tower m_placingTower;
        private Tower m_movingTower;
        private RadialMenu m_movingMenu;

        public MapTile[,] Tiles { get { return m_tiles; } set { m_tiles = value; } }

        public bool MovingActive { get { return m_movingActive; } set { m_movingActive = value; } }
        public Tower MovingTower { get { return m_movingTower; } set { m_movingTower = value; } }
        public RadialMenu MovingMenu { get { return m_movingMenu; } set { m_movingMenu = value; } }

        public PlacementManager(UniMapData data, ContentManager content, GraphicsDevice graphicsDevice)
        {
            m_tiles = new MapTile[data.WalkableGrid.GetLength(0), data.WalkableGrid.GetLength(1)];

            m_placingActive = false;

            // Create the grid of overlay tiles
            FillPlacementTiles(data, content, graphicsDevice);
        }

        private void FillPlacementTiles(UniMapData data, ContentManager content, GraphicsDevice graphicsDevice)
        {
            for (int y = 0; y < m_tiles.GetLength(1); y++)
            {
                for (int x = 0; x < m_tiles.GetLength(0); x++)
                {
                    m_tiles[x, y] = new MapTile(content.Load<Texture2D>("Art\\PlaceholderArt\\Tiles\\TowerPlacementTiles"), new Vector2(18 + (x * 36), 18 + (y * 36)), Color.White, 1, 1, x, y, graphicsDevice);

                    // Check the tile "heights" to add tints for highlighting
                    // This gives the player visual aide when placing and moving towers
                    if (data.WalkableGrid[x, y] <= 0)
                    {
                        m_tiles[x, y].IsPlaceable = false;
                        m_tiles[x, y].AltTints.Add(Color.HotPink);
                        m_tiles[x, y].AltTints.Add(new Color(100, 100, 100));
                        m_tiles[x, y].Tint = m_tiles[x, y].AltTints[1];
                        m_tiles[x, y].SourceRectangle = new Rectangle(0, 0, 36, 36);
                        m_tiles[x, y].Origin += new Vector2(2, 2);
                    }
                    else if (data.WalkableGrid[x, y] == 1)
                    {
                        m_tiles[x, y].IsPlaceable = true;
                        m_tiles[x, y].AltTints.Add(Color.LightGreen);
                        m_tiles[x, y].AltTints.Add(Color.Gray);
                        m_tiles[x, y].Tint = m_tiles[x, y].AltTints[1];
                        m_tiles[x, y].SourceRectangle = new Rectangle(0, 0, 36, 36);
                        m_tiles[x, y].Origin += new Vector2(2, 2);
                    }
                    else if (data.WalkableGrid[x, y] > 1)
                    {
                        m_tiles[x, y].IsPlaceable = false;
                        m_tiles[x, y].AltTints.Add(Color.HotPink);
                        m_tiles[x, y].AltTints.Add(new Color(100, 100, 100));
                        m_tiles[x, y].Tint = m_tiles[x, y].AltTints[1];
                        m_tiles[x, y].SourceRectangle = new Rectangle(0, 0, 36, 36);
                        m_tiles[x, y].Origin += new Vector2(2, 2);
                    }
                }
            }
        }

        public void UpdateManager(InputManager input, Camera cam, List<Tower> towers, List<Tower> savedTowers, ContentManager content, GameplayMenu gameMenu, GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, List<RadialMenu> towerMenus, PlayerStats userData)
        {
            m_mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);
            m_mousePos = Vector2.Transform(m_mousePos, Matrix.Invert(cam.Transform));
            m_mousePos = Vector2.Clamp(m_mousePos, Vector2.Zero, new Vector2(2875, 1615));

            foreach(MapTile m in m_tiles)
            {
                TowerShifting(input, towers, content, towerMenus, userData, m);

                SetTileColours(m);
            }

            for (int i = 0; i < gameMenu.TowerOptions.Count; i++)
            {
                if (gameMenu.TowerOptions[i].IsPressed && m_placingActive && !m_movingActive)
                {
                    m_placingActive = false;
                }
                if (gameMenu.TowerOptions[i].IsPressed && !m_placingActive && !m_movingActive)
                {
                    if (userData.Cash >= savedTowers[i].TowerCost)
                    {
                        savedTowers[i].UpdateAllProps();
                        m_placingTower = new Tower(content, savedTowers[i], m_mousePos);
                        m_placingActive = true;
                    }
                }
            }
        }

        // This sets tile colour depending if it is within placing dimensions
        private static void SetTileColours(MapTile m)
        {
            if (m.IsWithinPlacing)
            {
                if (m.IsPlaceable)
                {
                    m.Tint = m.AltTints[0];
                    m.IsWithinPlacing = false;
                }
                else
                {
                    m.Tint = m.AltTints[0];
                    m.IsWithinPlacing = false;
                }
            }
            else
            {
                m.Tint = m.AltTints[1];
            }
        }

        private void TowerShifting(InputManager input, List<Tower> towers, ContentManager content, List<RadialMenu> towerMenus, PlayerStats userData, MapTile m)
        {
            // If placing or moving, check if it is possible to place a tower in the area
            if ((m_placingActive || m_movingActive) && m.Rect.Contains((int)m_mousePos.X, (int)m_mousePos.Y))
            {
                bool canPlace = false;

                if (m_placingActive)
                {
                    canPlace = ValidateTileArea(m, canPlace, m_placingTower);
                }
                else if (m_movingActive)
                {
                    canPlace = ValidateTileArea(m, canPlace, m_movingTower);
                }

                // If placing single
                if (input.LeftPressed && !input.Keys.IsKeyDown(Keys.LeftShift) && canPlace)
                {
                    if (m_placingActive)
                    {
                        PlaceTowerSingle(towers, content, towerMenus, userData, m);
                    }
                    else if (m_movingActive && canPlace)
                    {
                        DropMovingTower(userData, m);
                    }
                }
                else if (input.LeftPressed && input.Keys.IsKeyDown(Keys.LeftShift) && canPlace) // If placing multiple
                {
                    if (m_placingActive && userData.Cash >= m_placingTower.TowerCost)
                    {
                        PlaceTowerMultiple(towers, content, towerMenus, userData, m);
                    }
                    else if (m_movingActive && canPlace)
                    {
                        DropMovingTower(userData, m);
                    }
                    else
                    {
                        m_placingActive = false;
                    }
                }
            }
        }

        private void PlaceTowerSingle(List<Tower> towers, ContentManager content, List<RadialMenu> towerMenus, PlayerStats userData, MapTile m)
        {
            if (userData.Cash >= m_placingTower.TowerCost)
            {
                // Offset the tower according to the tower dimensions
                if (m_placingTower.TowerDimensions == 2)
                {
                    m_placingTower.TowerPos = m.Position + new Vector2(18, 18);
                }
                else
                {
                    m_placingTower.TowerPos = m.Position;
                }

                // Update all the tower part positions and create a new tower (root the tower into the ground)
                m_placingTower.UpdatePosition();
                m_placingTower.Coords = new Point(m.GridX, m.GridY);
                RootTower(m_placingTower, m);
                towers.Add(m_placingTower);

                // Check how many modules of each type were used
                for (int i = 0; i < m_placingTower.TowerParts.Count; i++)
                {
                    switch (m_placingTower.TowerParts[i].TypeIndex)
                    {
                        case 3:
                            userData.OffensiveModulesUsed++;
                            break;
                        case 4:
                            userData.UtilityModulesUsed++;
                            break;
                    }
                }

                // Add menu and take cash
                towerMenus.Add(new RadialMenu(2, 40 * ((m_placingTower.TowerDimensions / 2) + 1), "Art\\GameArt\\GameplayGUI\\TowerMenu\\MenuButton", m_placingTower.TowerDimensions, (m_placingTower.TowerDimensions * 0.15f), m_placingTower.TowerPos, content));
                userData.Cash -= m_placingTower.TowerCost;
                m_placingActive = false;
            }
            else
            {
                m_placingActive = false;
                return;
            }
        }

        private void PlaceTowerMultiple(List<Tower> towers, ContentManager content, List<RadialMenu> towerMenus, PlayerStats userData, MapTile m)
        {
            if (userData.Cash >= m_placingTower.TowerCost)
            {
                // Offset tower according to dimensions
                if (m_placingTower.TowerDimensions == 2)
                {
                    m_placingTower.TowerPos = m.Position + new Vector2(18, 18);
                }
                else
                {
                    m_placingTower.TowerPos = m.Position;
                }

                // Place and root a temporary tower based on the placing tower
                m_placingTower.UpdatePosition();
                m_placingTower.Coords = new Point(m.GridX, m.GridY);
                Tower tmpTower = new Tower(content, m_placingTower, m_placingTower.TowerPos);
                tmpTower.UpdatePosition();
                RootTower(tmpTower, m);
                towers.Add(tmpTower);

                // Check how many of each type of module was used
                for (int i = 0; i < m_placingTower.TowerParts.Count; i++)
                {
                    switch (m_placingTower.TowerParts[i].TypeIndex)
                    {
                        case 3:
                            userData.OffensiveModulesUsed++;
                            break;
                        case 4:
                            userData.UtilityModulesUsed++;
                            break;
                    }
                }

                // Add menu and take cash
                towerMenus.Add(new RadialMenu(2, 40 * ((m_placingTower.TowerDimensions / 2) + 1), "Art\\GameArt\\GameplayGUI\\TowerMenu\\MenuButton", m_placingTower.TowerDimensions, (m_placingTower.TowerDimensions * 0.15f), m_placingTower.TowerPos, content));
                userData.Cash -= m_placingTower.TowerCost;
            }
            else
            {
                m_placingActive = false;
                return;
            }
        }

        private void DropMovingTower(PlayerStats userData, MapTile m)
        {
            // Offset menu and tower according to dimensions
            if (m_movingTower.TowerDimensions == 2)
            {
                m_movingTower.TowerPos = m.Position + new Vector2(18, 18);
                m_movingMenu.MenuCentre = m.Position + new Vector2(18, 18);
            }
            else
            {
                m_movingTower.TowerPos = m.Position;
                m_movingMenu.MenuCentre = m.Position;
            }

            // Drop moving tower
            m_movingTower.UpdatePosition();
            m_movingTower.Coords = new Point(m.GridX, m.GridY);
            RootTower(m_movingTower, m);
            userData.Cash -= 100;
            m_movingActive = false;
        }

        private bool ValidateTileArea(MapTile m, bool canPlace, Tower tower)
        {
            int availableCount = 0;

            int min = 0;
            int max = 0;

            // Set up neightbouring tiles to be checked
            switch (tower.TowerDimensions)
            {
                case 1:
                    min = 0;
                    max = 1;
                    break;
                case 2:
                    min = 0;
                    max = 2;
                    break;
                case 3:
                    min = -1;
                    max = 2;
                    break;
            }

            // Check neighbouring tiles and count how many are available
            for (int y = min; y < max; y++)
            {
                for (int x = min; x < max; x++)
                {
                    bool isOnGrid = false;

                    int checkX = m.GridX + x;
                    int checkY = m.GridY + y;

                    if (checkX >= 0 && checkX < m_tiles.GetLength(0) && checkY >= 0 && checkY < m_tiles.GetLength(1))
                        isOnGrid = true;
                    else
                        isOnGrid = false;

                    if (isOnGrid && m_tiles[checkX, checkY].IsPlaceable)
                    {
                        m_tiles[checkX, checkY].IsWithinPlacing = true;
                        availableCount++;
                    }
                    else if (isOnGrid && !m_tiles[checkX, checkY].IsPlaceable)
                    {
                        m_tiles[checkX, checkY].IsWithinPlacing = true;
                    }
                }
            }

            // Since tower bases are square, the tiles needed are equal to the dimensions squared (1^2 = 1, 2^2 = 4, 3^3 = 9)
            // If the number of tiles needed are available, then you can place the tower
            if (availableCount >= (tower.TowerDimensions * tower.TowerDimensions))
            {
                canPlace = true;
            }
            else
            {
                canPlace = false;
            }

            return canPlace;
        }

        // This method prevents towers from being placed on top of eachother and making "Towers of Towers".
        private void RootTower(Tower tower, MapTile m)
        {
            int min = 0;
            int max = 0;

            switch (tower.TowerDimensions)
            {
                case 1:
                    min = 0;
                    max = 1;
                    break;
                case 2:
                    min = 0;
                    max = 2;
                    break;
                case 3:
                    min = -1;
                    max = 2;
                    break;
            }

            for (int y = min; y < max; y++)
            {
                for (int x = min; x < max; x++)
                {
                    int X = m.GridX + x;
                    int Y = m.GridY + y;

                    m_tiles[X, Y].IsPlaceable = false;
                }
            }
        }

        // This method does the opposite of root tower, where it will free up the previously used tiles.
        public void UprootTower(Tower tower, MapTile m)
        {
            int min = 0;
            int max = 0;

            switch (tower.TowerDimensions)
            {
                case 1:
                    min = 0;
                    max = 1;
                    break;
                case 2:
                    min = 0;
                    max = 2;
                    break;
                case 3:
                    min = -1;
                    max = 2;
                    break;
            }

            for (int y = min; y < max; y++)
            {
                for (int x = min; x < max; x++)
                {
                    int X = m.GridX + x;
                    int Y = m.GridY + y;

                    m_tiles[X, Y].IsPlaceable = true;
                }
            }
        }

        public void DrawMe(SpriteBatch sb)
        {
            if (m_placingActive ^ m_movingActive)
            {
                foreach (MapTile m in m_tiles)
                {
                    m.DrawMe(sb);
                }
            }
        }

    }
}
