using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    /// <summary>
    /// Slot holds a part (only used in construction)
    /// </summary>
    class Slot : MotionGraphic
    {
        // Parts are dragged into this circle to be dropped
        protected Circle m_dropCircle;

        // Used to determine where a part should be dropped
        protected int m_slotHeight;

        // Type index will determine if the part in the slot is rotor, component, offensive or utility module. (0 = null, 1 = rotor, 2 = component, 3 = offense module, 4 = utility module)
        // Sub-index determines the specific part within that type.
        protected int m_typeIndex;
        protected int m_subIndex;

        // Slots knows it's own index to keep track of where it should be if the part is rotated
        private int m_index;
        // Part that is stored in the slot
        private BasePart m_storedPart;

        public Circle DropCircle { get { return m_dropCircle; } }

        public int TypeIndex { get { return m_typeIndex; } set { m_typeIndex = value; } }
        public int SubIndex { get { return m_subIndex; } set { m_subIndex = value; } }

        public int SlotHeight { get { return m_slotHeight; } }

        public int Index { get { return m_index; } set { m_index = value; } }

        public BasePart StoredPart { get { return m_storedPart; } set { m_storedPart = value; } }

        public Slot(Texture2D txr, Vector2 position, Color tint, float scale, int height, int index) 
            : base(txr, position, tint, Vector2.Zero, 0, scale)
        {
            m_dropCircle = new Circle(position.X, position.Y, 7);
            m_slotHeight = height;
            m_index = index;
        }

        public void UpdateMe()
        {
            m_dropCircle.Centre = m_position;
        }

        // Gets the slot
        public Slot GetSlot(Camera cam, InputManager input)
        {
            Vector2 mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);
            mousePos = Vector2.Transform(mousePos, Matrix.Invert(cam.Transform));

            if (m_dropCircle.Contains(mousePos))
            {
                return this;
            }
            else
            {
                return null;
            }
        }
    }
}
