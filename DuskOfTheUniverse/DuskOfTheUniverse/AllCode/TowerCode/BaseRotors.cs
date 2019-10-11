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
    /// Rotor on which all game rotors are based
    /// </summary>
    class BaseRotor : TowerMasterPart
    {
        // Range of the rotor (enemies within it will be tracked, it will look at the closest enemy)
        protected Circle m_maxRange;

        public Circle RangeCircle { get { return m_maxRange; } set { m_maxRange = value; } }

        public BaseRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, Vector2.Zero, 0, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_offsets = offsets;
            m_transformedPositions = new List<Vector2>();

            for (int i = 0; i < m_offsets.Count; i++)
            {
                m_transformedPositions.Add(m_offsets[i]);
            }

            // Range will be set later using the ranges of the tower modules
            m_maxRange = new Circle(position.X, position.Y, 36 * 0);
        }

        public virtual void UpdateMe(List<EnemyChar> enemies, GameTime gt, List<BaseProjectile> projectiles, ContentManager content, List<TowerMasterPart> towerParts)
        {
            base.UpdateMe();

            m_maxRange.Centre = m_position;

            // Find nearest enemy and track them
            GetNearestEnemy(enemies);
        }

        private void GetNearestEnemy(List<EnemyChar> enemies)
        {
            float minDist = 9999;

            for (int i = 0; i < enemies.Count; i++)
            {
                float currDist = Vector2.Distance(enemies[i].Position, m_position);
                Vector2 dirVec = enemies[i].Position - m_position;

                if (m_maxRange.Contains(enemies[i].Position) && currDist < minDist && enemies[i].Health > 0)
                {
                    minDist = currDist;
                    m_rot = (float)Math.Atan2(dirVec.Y, dirVec.X) + 1.5707f;
                    m_relativeRot = m_rot;
                }
            }
        }
    }

    #region Small Rotors
    class SmallBasicRotor : BaseRotor
    {
        public SmallBasicRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partCost = 150;
        }
    }

    class SmallAdvancedRotor : BaseRotor
    {
        public SmallAdvancedRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 150;
            m_partCost = 225;
        }
    }

    class SmallPrototypeRotor : BaseRotor
    {
        public SmallPrototypeRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 200;
            m_partCost = 300;
        }
    }
#endregion

    #region Medium Rotors
    class MediumBasicRotor : BaseRotor
    {
        public MediumBasicRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 200;
            m_partCost = 300;
        }
    }

    class MediumAdvancedRotor : BaseRotor
    {
        public MediumAdvancedRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 300;
            m_partCost = 450;
        }
    }

    class MediumPrototypeRotor : BaseRotor
    {
        public MediumPrototypeRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 400;
            m_partCost = 600;
        }
    }
    #endregion

    #region Heavy Rotors
    class HeavyBasicRotor : BaseRotor
    {
        public HeavyBasicRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 300;
            m_partCost = 450;
        }
    }

    class HeavyAdvancedRotor : BaseRotor
    {
        public HeavyAdvancedRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 450;
            m_partCost = 675;
        }
    }

    class HeavyPrototypeRotor : BaseRotor
    {
        public HeavyPrototypeRotor(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 600;
            m_partCost = 900;
        }
    }
    #endregion
}
