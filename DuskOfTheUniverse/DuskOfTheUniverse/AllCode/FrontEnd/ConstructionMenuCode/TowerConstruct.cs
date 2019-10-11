using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    /// <summary>
    /// This is the tower construct to be used in the construction menu
    /// It is effectively a display and visual schematic for a tower
    /// </summary>
    class TowerConstruct
    {
        // This is the list of parts in the tower
        private List<BasePart> m_parts;

        private int m_foundationsIndex;
        private int m_rotorIndex;

        public List<BasePart> Parts { get { return m_parts; } set { m_parts = value; } }

        public int FoundationsIndex { get { return m_foundationsIndex; } set { m_foundationsIndex = value; } }
        public int RotorIndex { get { return m_rotorIndex; } set { m_rotorIndex = value; } }

        public TowerConstruct(ContentManager content)
        {
            m_parts = new List<BasePart>();

            // This merely sets up a start point
            List<Vector2> vecList = new List<Vector2>();
            vecList.Add(new Vector2(0, 0));

            m_parts.Add(new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\PartSlot"), new Vector2(126, 126), Color.White, 0.3f, 0, content, vecList, 0, 0, 0, 1, 1, 1));
        }

        public void UpdateConstruct(GameTime gt)
        {
            // Snaps the position to integer locations
            for (int i = 0; i < m_parts.Count; i++)
            {
                m_parts[i].UpdateMe(gt);
                m_parts[i].Position = new Vector2((int)m_parts[i].Position.X, (int)m_parts[i].Position.Y);
            }
        }

        public void DrawMe(SpriteBatch sb, GameTime gt)
        {
            for (int i = 0; i < m_parts.Count; i++)
            {
                m_parts[i].DrawMe(sb, gt);
            }
        }
    }
}
