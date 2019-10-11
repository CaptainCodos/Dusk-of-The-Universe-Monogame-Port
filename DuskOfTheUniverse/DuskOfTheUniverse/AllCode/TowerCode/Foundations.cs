using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    /// <summary>
    /// Foundations on which all game tower foundations are based
    /// </summary>
    class BaseFoundations : TowerMasterPart
    {
        // How many components the foundations can hold
        protected int m_baseMaxComponents;
        protected int m_maxComponents;

        public int MaxComponents { get { return m_maxComponents; } }

        public BaseFoundations(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, Vector2.Zero, 0, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 250;

            m_baseMaxComponents = 4;
            m_maxComponents = m_baseMaxComponents;

            m_partCost = 100;
        }
    }

    class LightFoundations : BaseFoundations
    {
        public LightFoundations(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_baseMaxComponents = 10;
            m_maxComponents = m_baseMaxComponents;
        }
    }

    class MediumFoundations : BaseFoundations
    {
        public MediumFoundations(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 500;

            m_baseMaxComponents = 15;
            m_maxComponents = m_baseMaxComponents;

            m_partCost = 400;
        }
    }

    class HeavyFoundations : BaseFoundations
    {
        public HeavyFoundations(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 1000;

            m_baseMaxComponents = 20;
            m_maxComponents = m_baseMaxComponents;

            m_partCost = 1200;
        }
    }
}
