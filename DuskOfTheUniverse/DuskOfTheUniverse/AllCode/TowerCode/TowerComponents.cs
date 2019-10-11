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
    /// Component on which other components are based
    /// </summary>
    class BaseTowerComponent : TowerMasterPart
    {
        // Holds the index at which it will be offset from it;s parent
        protected int m_offsetIndex;

        // Holds the index within the tower
        protected int m_index;

        public int OffsetIndex { get { return m_offsetIndex; } set { m_offsetIndex = value; } }
        public int Index { get { return m_index; } set { m_index = value; } }

        public BaseTowerComponent(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, Vector2.Zero, 0, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_offsets = offsets;
            m_transformedPositions = new List<Vector2>();

            for (int i = 0; i < m_offsets.Count; i++)
            {
                m_transformedPositions.Add(m_offsets[i]);
            }
        }

        public virtual void UpdateMe(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            base.UpdateMe();
        }
    }

    class BasicComponentSingle : BaseTowerComponent
    {
        public BasicComponentSingle(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partCost = 100;
            m_partHealth = 100;
        }
    }
    class BasicComponentDouble : BaseTowerComponent
    {
        public BasicComponentDouble(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partCost = 150;
            m_partHealth = 150;
        }
    }
}
