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
    /// This is a part on which all tower base parts will inherit from (this is the most basic part)
    /// </summary>
    class TowerMasterPart : AnimGraphic
    {
        // Health, cost and index within the parent
        protected int m_partHealth;
        protected int m_partCost;
        protected int m_towerIndex;

        // List of vector positions at which child parts will sit
        protected List<Vector2> m_offsets;
        protected List<Vector2> m_transformedPositions;

        // Matrix for positioning child parts
        protected Matrix m_rotationMatrix;

        // List of child parts held by this part
        protected List<TowerMasterPart> m_towerparts;

        private PartProps m_props;

        // What type of part is this
        protected int m_typeIndex;
        // What kind of the above is this (eg type is rotor, sub is small basic rotor)
        protected int m_subIndex;
        // Index at which this is in the tower
        protected int m_extendedIndex;
        // What is the index of this part's parent (if applicable)
        protected int m_parentIndex;

        // Part's rotation relative to the parent
        protected float m_relativeRot;

        public int PartHealth { get { return m_partHealth; } set { m_partHealth = value; } }
        public int PartCost { get { return m_partCost; } set { m_partCost = value; } }
        public int TowerIndex { get { return m_towerIndex; } set { m_towerIndex = value; } }
        public int ParentIndex { get { return m_parentIndex; } set { m_parentIndex = value; } }
        public int ExtendedIndex { get { return m_extendedIndex; } set { m_extendedIndex = value; } }

        public int TypeIndex { get { return m_typeIndex; } set { m_typeIndex = value; } }
        public int SubIndex { get { return m_subIndex; } set { m_subIndex = value; } }

        public float RelativeRotation { get { return m_relativeRot; } set { m_relativeRot = value; } }

        public List<TowerMasterPart> TowerParts { get { return m_towerparts; } set { m_towerparts = value; } }
        public List<Vector2> Offsets { get { return m_offsets; } set { m_offsets = value; } }

        public PartProps Props { get { return m_props; } set { m_props = value; } }

        // Set up part
        public TowerMasterPart(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float rotationSpeed, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, startSpeed, rotationSpeed, scale, fps, framesX, framesY)
        {
            m_partHealth = 100;

            m_offsets = offsets;
            m_transformedPositions = new List<Vector2>();

            for (int i = 0; i < m_offsets.Count; i++)
            {
                m_transformedPositions.Add(m_offsets[i]);
            }

            m_towerparts = new List<TowerMasterPart>();

            m_props = new PartProps();

            m_typeIndex = typeIndex;
            m_subIndex = subIndex;
        }

        // Update the part's properties
        public void UpdateProps()
        {
            m_props.Offsets = m_offsets;
            m_props.StoredIndex = m_extendedIndex;
            m_props.ParentIndex = m_parentIndex;
            m_props.SlotIndex = m_towerIndex;
            m_props.SubIndex = m_subIndex;
            m_props.TypeIndex = m_typeIndex;
            m_props.RelativeRotation = m_relativeRot;
        }

        // Update all the child part positions and rotations
        public virtual void UpdateMe()
        {
            for (int i = 0; i < m_towerparts.Count; i++)
            {
                m_towerparts[i].Position = SlotPos(m_towerparts[i].TowerIndex);
                m_towerparts[i].Rotation = m_rot + m_towerparts[i].RelativeRotation;
            }
        }

        // Draw the tower part
        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            base.DrawMe(sb, gt);
        }

        // Positions for parts at index i
        public virtual Vector2 SlotPos(int i)
        {
            m_rotationMatrix = Matrix.CreateRotationZ(m_rot);

            m_transformedPositions[i] = Vector2.Transform(m_offsets[i], m_rotationMatrix);

            return (m_transformedPositions[i] + m_position);
        }
    }

    /// <summary>
    /// Contains properties of the part
    /// </summary>
    public class PartProps
    {
        // Index in the tower
        private int m_storedIndex;
        // Index of the parent within the tower
        private int m_parentIndex;
        // Index of slot this will be positioned at in the parent (index within the parent)
        private int m_slotIndex;
        
        // Determines the kind of part this is (catagory, part type)
        private int m_typeIndex;
        private int m_subIndex;

        // Rotation relative to the parent
        private float m_relativeRot;

        // Offset positions for the part's children
        private List<Vector2> m_offsets;

        public int StoredIndex { get { return m_storedIndex; } set { m_storedIndex = value; } }
        public int ParentIndex { get { return m_parentIndex; } set { m_parentIndex = value; } }
        public int SlotIndex { get { return m_slotIndex; } set { m_slotIndex = value; } }
        public int TypeIndex { get { return m_typeIndex; } set { m_typeIndex = value; } }
        public int SubIndex { get { return m_subIndex; } set { m_subIndex = value; } }

        public float RelativeRotation { get { return m_relativeRot; } set { m_relativeRot = value; } }

        public List<Vector2> Offsets { get { return m_offsets; } set { m_offsets = value; } }
    }
}
