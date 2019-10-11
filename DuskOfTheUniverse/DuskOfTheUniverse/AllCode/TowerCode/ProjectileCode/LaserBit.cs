using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    /// <summary>
    /// Simple piece of a laser beam
    /// </summary>
    class LaserBit : MotionGraphic
    {
        public Circle CollisionCircle { get; set; }

        public LaserBit(Texture2D txr, Vector2 position, Color tint, Circle circle)
            : base (txr, position, tint, Vector2.Zero, 0, 1)
        {
            CollisionCircle = circle;
        }
    }
}
