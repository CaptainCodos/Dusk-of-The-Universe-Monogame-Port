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
    /// This is a construction part to be used to create tower constructs
    /// </summary>
    class BasePart : AnimGraphic
    {
        // This is a list for positions for part slots
        protected List<Vector2> m_slotPositions;
        // This is a list for the final positions of part slots (according to rotation)
        private List<Vector2> m_finalPositions;

        // This is the list of slots
        protected List<Slot> m_slots;

        // This is a list of parts held by this part
        private List<BasePart> m_parts;

        // This is the height of the part on the tower
        protected int m_partHeight;

        // This is the type of part (base, rotor, weapon etc...)
        protected int m_typeIndex;
        // This is the index of that type (if it's a weapon, which weapon)
        protected int m_subIndex;
        // This is the index of the part within the held list of parts
        private int m_index;
        // This is the index of the part on thw tower as a whole
        private int m_extendedIndex;

        // This is the rotation relative to the parent part (this is where the m_index comes in)
        protected float m_relativeRot;
        // The matrix to be used for positioning
        private Matrix m_rotationMatrix;

        public int PartHeight { get { return m_partHeight; } set { m_partHeight = value; } }

        public int Index { get { return m_index; } set { m_index = value; } }
        public int ExtendedIndex { get { return m_extendedIndex; } set { m_extendedIndex = value; } }

        public float RelativeRot { get { return m_relativeRot; } set { m_relativeRot = value; } }

        public List<Slot> Slots { get { return m_slots; } set { m_slots = value; } }
        public List<BasePart> Parts { get { return m_parts; } set { m_parts = value; } }

        public int TypeIndex { get { return m_typeIndex; } set { m_typeIndex = value; } }
        public int SubIndex { get { return m_subIndex; } set { m_subIndex = value; } }

        public BasePart(Texture2D txr, Vector2 position, Color tint, float scale, int height, ContentManager content, List<Vector2> slotPosList, int typeI, int subI, int index, int fps, int framesX, int framesY) 
            : base(txr, position, tint, Vector2.Zero, 0, scale, fps, framesX, framesY)
        {
            m_partHeight = height;
            m_index = index;

            m_parts = new List<BasePart>();

            m_slotPositions = slotPosList;
            m_finalPositions = new List<Vector2>();
            m_slots = new List<Slot>();

            // For every position, add a slot
            for (int i = 0; i < m_slotPositions.Count; i++)
            {
                m_finalPositions.Add(new Vector2(0, 0));
                m_slots.Add(new Slot(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\PartSlot"), SlotPos(i), Color.White, 0.3f, m_partHeight, i));
                m_slots[i].Rotation = m_rot;
            }

            // sets the indexs of the part
            m_subIndex = subI;
            m_typeIndex = typeI;

            // This is for animation purposes
            switch(m_typeIndex)
            {
                case 0:
                    m_srcRect.Y = 40 * SubIndex + 2;
                    break;
                case 3:
                    m_srcRect.Y = 40 * SubIndex + 2;
                    break;
                case 4:
                    m_srcRect.Y = 40 * SubIndex + 2;
                    break;
                default:
                    m_srcRect.Y = 2;
                    break;
            }
            
        }

        public override void UpdateMe(GameTime gt)
        {
            base.UpdateMe(gt);

            // Sets the slot position according to the index within the list
            for (int i = 0; i < m_slots.Count; i++)
            {
                m_slots[i].Position = SlotPos(m_slots[i].Index);
                m_slots[i].UpdateMe();
            }

            // Sets the part position according to the index within the list
            for (int i = 0; i < m_parts.Count; i++)
            {
                m_parts[i].Position = SlotPos(m_parts[i].Index);
                m_parts[i].UpdateMe(gt);
            }
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            base.DrawMe(sb, gt);

            for (int i = 0; i < m_slots.Count; i++)
            {
                m_slots[i].DrawMe(sb);

#if DEBUG
                sb.DrawString(Game1.debugFont, "Drop Circle: " + m_slots[i].DropCircle.Centre + ", " + m_slots[i].DropCircle.Radius + "\nIndex: " + m_slots[i].Index + "\nRelativeRot: " + m_relativeRot, m_position + new Vector2(200, 0 + (i * 50)), Color.White);
#endif
            }

#if DEBUG
            sb.DrawString(Game1.debugFont, "Pos: " + m_position + "\nIndex: " + m_index + "\nType Index: " + m_typeIndex + "\nSub-Index: " + m_subIndex + "\nPart Height: " + m_partHeight, m_position, Color.White);
#endif
        }

        // Calculates final vector with respect to the rotation of the part
        public virtual Vector2 SlotPos(int i)
        {
            m_rotationMatrix = Matrix.CreateRotationZ(m_rot);

            m_finalPositions[i] = Vector2.Transform(m_slotPositions[i], m_rotationMatrix);

            return (m_finalPositions[i] + m_position);
        }
    }
}
