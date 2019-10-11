using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    // Class for a single tile.
    class MapTile : AnimGraphic
    {
        // Map tile's grid coordinates.
        private int m_gridX;
        private int m_gridY;

        // Parent and child tiles.
        private MapTile m_parentTile;
        private MapTile m_childTile;

        // Distance from target.
        private int m_distance;

        // Is the tile walkable?
        private bool m_isWalkable;
        private bool m_isPlaceable;

        // Has the tile been visited? (For use in pathfinding)
        private bool m_visited;

        // Checks if the map tile is occupied by an ally.
        private bool m_isOccupied;

        private GameChar m_occupiedChar;

        private bool m_isWithinPlacing;

        private bool m_isUnderTower;

        public int GridX { get { return m_gridX; } }
        public int GridY { get { return m_gridY; } }

        public MapTile ParentTile { get { return m_parentTile; } set { m_parentTile = value; } }
        public MapTile ChildTile { get { return m_childTile; } set { m_childTile = value; } }
        public GameChar OccupiedChar { get { return m_occupiedChar; } set { m_occupiedChar = value; } }

        public int Distance { get { return m_distance; } set { m_distance = value; } }
        public bool IsWalkable { get { return m_isWalkable; } set { m_isWalkable = value; } }
        public bool IsPlaceable { get { return m_isPlaceable; } set { m_isPlaceable = value; } }

        public bool Visited { get { return m_visited; } set { m_visited = value; } }
        public bool IsOccupied { get { return m_isOccupied; } set { m_isOccupied = value; } }
        public bool IsWithinPlacing { get { return m_isWithinPlacing; } set { m_isWithinPlacing = value; } }
        public bool IsUnderTower { get { return m_isUnderTower; } set { m_isUnderTower = value; } }

        public int Index { get; set; }

        public MapTile(Texture2D txr, Vector2 position, Color tint, int framesX, int framesY, int x, int y, GraphicsDevice graphicsDevice) 
            : base(txr, position, tint, Vector2.Zero, 0, 1, 0, framesX, framesY)
        {
            m_isWalkable = false;
            m_isPlaceable = false;
            m_visited = false;
            m_isOccupied = false;
            m_isWithinPlacing = false;
            m_isUnderTower = false;

            m_gridX = x;
            m_gridY = y;

            m_rect = new Rectangle((int)(m_position.X - m_origin.X), (int)(m_position.Y - m_origin.Y), 36, 36);
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            sb.Draw(m_txr, m_position, m_srcRect, m_tint, m_rot, m_origin, m_scale, SpriteEffects.None, m_layer);

#if DEBUG
            sb.DrawString(Game1.debugFont, "" + Index, m_position, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
#endif
        }
    }

    // Class for an enitre map.
    class Map
    {
        // Data from generation.
        private UniMapData m_data;

        // Tiles that the map contains.
        private MapTile[,] m_tiles;

        private int m_mapSizeX;
        private int m_mapSizeY;

        private int m_levelNumber;

        public MapTile[,] Tiles
        {
            get { return m_tiles; }
            set { m_tiles = value; }
        }

        public int LevelNumber { get { return m_levelNumber; } set { m_levelNumber = value; } }

        public Map(UniMapData data, ContentManager content, string pathAndName, GraphicsDevice graphicsDevice)
        {
            m_data = data;
            m_mapSizeX = m_data.WalkableGrid.GetLength(0);
            m_mapSizeY = m_data.WalkableGrid.GetLength(1);

            m_tiles = new MapTile[m_mapSizeX, m_mapSizeY];

            for (int y = 0; y < m_mapSizeY; y++)
            {
                for (int x = 0; x < m_mapSizeX; x++)
                {
                    m_tiles[x, y] = new MapTile(content.Load<Texture2D>(pathAndName), new Vector2(18 + (x * 36), 18 + (y * 36)), Color.White, 4, 4, x, y, graphicsDevice);
                    m_tiles[x, y].Index = m_data.WalkableGrid[x, y];

                    // Setup bool values for tiles according to their index
                    if (m_data.WalkableGrid[x, y] <= 0)
                    {
                        m_tiles[x, y].IsWalkable = true;
                        m_tiles[x, y].IsPlaceable = false;
                    }
                    else if (m_data.WalkableGrid[x, y] == 1)
                    {
                        m_tiles[x, y].IsWalkable = false;
                        m_tiles[x, y].IsPlaceable = true;
                    }
                    else if (m_data.WalkableGrid[x, y] > 1)
                    {
                        m_tiles[x, y].IsWalkable = false;
                        m_tiles[x, y].IsPlaceable = false;
                    }
                }
            }
        }

        public void UpdateMap(List<Tower> towers)
        {
            // Reset all tiles
            foreach (MapTile m in m_tiles)
            {
                m.IsOccupied = false;
                m.OccupiedChar = null;
                m.IsWithinPlacing = false;
                m.SourceRectY = 40 * (m_levelNumber - 1) + 2;
                m.SourceRectX = (40 * m_data.WalkableGrid[m.GridX, m.GridY]) + 2;
                m.Tint = new Color(60, 60, 60);
            }
        }

        public void DrawMe(SpriteBatch sb, GameTime gt)
        {
            foreach (MapTile n in m_tiles)
            {
                n.DrawMe(sb, gt);
            }
        }
    }
}
