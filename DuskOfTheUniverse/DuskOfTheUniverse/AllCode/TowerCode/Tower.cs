using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DuskOfTheUniverse
{
    [Serializable]
    class Tower
    {
        // Name of the tower
        private string m_name;

        // List of all parts on the tower
        private List<TowerMasterPart> m_towerParts;

        // Tower position
        private Vector2 m_towerPos;

        // List of all properties for each tower part
        private List<PartProps> m_propsList;

        // Properties of the tower itself such as boost factors
        private TowerProperties m_towerProps;

        // Tower health variables
        private float m_towerHP;
        private int m_maxTowerHP;
        private CharHealthBar m_healthBar;

        // Tower cost
        private int m_towerCost;

        // Tower dimensions (Size)
        private int m_towerDimensions;

        // Max number of tower components
        private int m_maxComponents;

        // Collision Rectangle
        private Rectangle m_collisionRect;
        
        // Tower coordinates
        private Point m_coords;

        public string TowerName { get { return m_name; } set { m_name = value; } }

        public List<TowerMasterPart> TowerParts { get { return m_towerParts; } set { m_towerParts = value; } }

        public Vector2 TowerPos { get { return m_towerPos; } set { m_towerPos = value; } }

        public List<PartProps> PartProps { get { return m_propsList; } set { m_propsList = value; } }
        public TowerProperties TowerProperties { get { return m_towerProps; } set { m_towerProps = value; } }

        public int TowerDimensions { get { return m_towerDimensions; } set { m_towerDimensions = value; } }
        public int MaxComponents { get { return m_maxComponents; } set { m_maxComponents = value; } }

        public Point Coords { get { return m_coords; } set { m_coords = value; } }

        public float Health { get { return m_towerHP; } set { m_towerHP = value; } }
        public int MaxHP { get { return m_maxTowerHP; } }
        public int TowerCost { get { return m_towerCost; } set { m_towerCost = value; } }
        public CharHealthBar HealthBar { get { return m_healthBar; } }

        /// <summary>
        /// Create basic tower with nothing in it (for construction)
        /// </summary>
        /// <param name="content"> Content Manager </param>
        /// <param name="position"> Tower position </param>
        public Tower(ContentManager content, Vector2 position)
        {
            m_towerPos = position;

            m_name = "";

            m_towerParts = new List<TowerMasterPart>();

            m_propsList = new List<PartProps>();
        }

        /// <summary>
        /// Constructs a tower based on another (for placing towers on a map)
        /// Prevents the game from making copies of towers memory wise (gives tower unique memory to work with so as not to alter other towers)
        /// </summary>
        /// <param name="content"> Content Manager </param>
        /// <param name="tower"> Tower the new tower is based on </param>
        /// <param name="position"> Position of the new tower</param>
        public Tower(ContentManager content, Tower tower, Vector2 position)
        {
            // Set position and coordinates
            m_towerPos = position;
            m_coords = new Point((int)position.X / 36, (int)position.Y / 36);

            m_name = tower.m_name;

            // Create health bar
            m_healthBar = new CharHealthBar("Art\\GameArt\\GameplayGUI")
                .SetPosition(new Vector2(1300, 960))
                .LoadShader(content, "Shaders\\SmallHighlightEffect")
                .LoadTextures(content, "\\EnemyHealthBarBorder", "\\EnemyHealthBar", "\\EnemyHealthBar", 1)
                .SetHpValues((int)m_towerHP)
                .SetColours(Color.White, Color.Yellow, Color.Red)
                .SetParentOfBar(this)
                .SetScaleBorder(0.5f)
                .SetScaleBacking(0.9f);

            // Create new lists
            m_towerParts = new List<TowerMasterPart>();
            m_propsList = new List<PartProps>();

            // Add the properties from the tower "mold"
            for (int i = 0; i < tower.PartProps.Count; i++)
            {
                m_propsList.Add(tower.m_propsList[i]);
            }

            // Set dimensions
            m_towerDimensions = tower.TowerDimensions;

            // Make collision rectangle
            m_collisionRect = new Rectangle((int)m_towerPos.X - (18 * m_towerDimensions), (int)m_towerPos.Y - (18 * m_towerDimensions), 36 * m_towerDimensions, 36 * m_towerDimensions);

            // Set Parent of foundations to 999 (999 is used in place of null here)
            if (m_propsList.Count > 0)
            {
                m_propsList[0].ParentIndex = 999;
            }

            // Add all the towers parts
            AddParts(content);

            // Create and update the towers properties
            m_towerProps = new TowerProperties(m_towerParts);
            m_towerProps.UpdateProperties(ref m_towerHP, ref m_towerCost);
            m_maxTowerHP = (int)m_towerHP;

            // Update all tower modules
            for (int i = 0; i < m_towerParts.Count; i++)
            {
                UpdateModuleParts(i);
            }

            // Update the tower rotor
            for (int i = 0; i < m_towerParts.Count; i++)
            {
                if (m_towerParts[i] is BaseRotor)
                {
                    UpdateRotorRange((BaseRotor)m_towerParts[i]);
                    break;
                }
            }
        }

        // Add all the tower parts
        private void AddParts(ContentManager content)
        {
            for (int i = 0; i < m_propsList.Count; i++)
            {
                switch (m_propsList[i].TypeIndex)
                {
                    case 0:
                        AddFoundations(content, i);
                        break;
                    case 1:
                        AddRotor(content, i);
                        break;
                    case 2:
                        AddComponents(content, i);
                        break;
                    case 3:
                        AddWeapons(content, i);
                        break;
                    case 4:
                        AddUtilities(content, i);
                        break;
                }
            }
        }

        // Update modules
        private void UpdateModuleParts(int i)
        {
            switch (m_towerParts[i].TypeIndex)
            {
                case 3:
                    switch (m_towerParts[i].SubIndex)
                    {
                        case 0:
                            NormalCannonModule cannon1 = (NormalCannonModule)m_towerParts[i];
                            cannon1.FinalRange = (float)(cannon1.BaseRange * m_towerProps.RangeBoostFactor);
                            cannon1.FinalFireRate = (float)(cannon1.BaseFireRate / m_towerProps.RateBoostFactor);
                            cannon1.FinalDamage = (int)(cannon1.BaseDamage * m_towerProps.DamageBoostFactor);
                            break;
                        case 1:
                            HeavyCannonModule cannon2 = (HeavyCannonModule)m_towerParts[i];
                            cannon2.FinalRange = (float)(cannon2.BaseRange * m_towerProps.RangeBoostFactor);
                            cannon2.FinalFireRate = (float)(cannon2.BaseFireRate / m_towerProps.RateBoostFactor);
                            cannon2.FinalDamage = (int)(cannon2.BaseDamage * m_towerProps.DamageBoostFactor);
                            break;
                        case 2:
                            LaserCannonModule cannon3 = (LaserCannonModule)m_towerParts[i];
                            cannon3.FinalRange = (float)(cannon3.BaseRange * m_towerProps.RangeBoostFactor);
                            cannon3.FinalFireRate = (float)(cannon3.BaseFireRate / m_towerProps.RateBoostFactor);
                            cannon3.FinalDamage = (int)(cannon3.BaseDamage * m_towerProps.DamageBoostFactor);
                            break;
                        case 3:
                            MissileCannonModule cannon4 = (MissileCannonModule)m_towerParts[i];
                            cannon4.FinalRange = (float)(cannon4.BaseRange * m_towerProps.RangeBoostFactor);
                            cannon4.FinalFireRate = (float)(cannon4.BaseFireRate / m_towerProps.RateBoostFactor);
                            cannon4.FinalDamage = (int)(cannon4.BaseDamage * m_towerProps.DamageBoostFactor);
                            break;
                        case 4:
                            FlameCannonModule cannon5 = (FlameCannonModule)m_towerParts[i];
                            cannon5.FinalRange = (float)(cannon5.BaseRange * m_towerProps.RangeBoostFactor);
                            cannon5.FinalFireRate = (float)(cannon5.BaseFireRate / m_towerProps.RateBoostFactor);
                            cannon5.FinalDamage = (int)(cannon5.BaseDamage * m_towerProps.DamageBoostFactor);
                            break;
                        case 5:
                            SplashCannonModule cannon6 = (SplashCannonModule)m_towerParts[i];
                            cannon6.FinalRange = (float)(cannon6.BaseRange * m_towerProps.RangeBoostFactor);
                            cannon6.FinalFireRate = (float)(cannon6.BaseFireRate / m_towerProps.RateBoostFactor);
                            cannon6.FinalDamage = (int)(cannon6.BaseDamage * m_towerProps.DamageBoostFactor);
                            break;
                    }
                    break;
                case 4:
                    switch (m_towerParts[i].SubIndex)
                    {
                        case 4:
                            HealingModule util1 = (HealingModule)m_towerParts[i];
                            util1.FinalHealingTime = (float)(util1.BaseHealingTime / m_towerProps.RateBoostFactor);
                            util1.FinalRange = (float)(util1.BaseRange * m_towerProps.RangeBoostFactor);
                            break;
                        case 5:
                            MotivationModule util2 = (MotivationModule)m_towerParts[i];
                            util2.FinalEffectiveness = (float)(util2.BaseEffectiveness * m_towerProps.DamageBoostFactor);
                            util2.FinalRange = (float)(util2.BaseRange * m_towerProps.RangeBoostFactor);
                            break;
                        case 6:
                            TimeDilationModule util3 = (TimeDilationModule)m_towerParts[i];
                            util3.FinalEffectiveness = (float)(util3.BaseEffectiveness * m_towerProps.DamageBoostFactor);
                            util3.FinalRange = (float)(util3.BaseRange * m_towerProps.RangeBoostFactor);
                            break;
                    }
                    break;
            }
        }
        // Update range of rotor
        private void UpdateRotorRange(BaseRotor rotor)
        {
            for (int i = 0; i < m_towerParts.Count; i++)
            {
                switch (m_towerParts[i].TypeIndex)
                {
                    case 3:
                        OffensiveModule mod = (OffensiveModule)m_towerParts[i];

                        if (36 * mod.FinalRange > rotor.RangeCircle.Radius)
                        {
                            rotor.RangeCircle.Radius = 36 * (mod.FinalRange + 1);
                        }
                        break;
                    case 4:
                        switch (m_towerParts[i].SubIndex)
                        {
                            case 4:
                                HealingModule u1 = (HealingModule)m_towerParts[i];

                                if (36 * u1.FinalRange > rotor.RangeCircle.Radius)
                                {
                                    rotor.RangeCircle.Radius = 36 * (u1.FinalRange + 1);
                                }
                                break;
                            case 5:
                                MotivationModule u2 = (MotivationModule)m_towerParts[i];

                                if (36 * u2.FinalRange > rotor.RangeCircle.Radius)
                                {
                                    rotor.RangeCircle.Radius = 36 * (u2.FinalRange + 1);
                                }
                                break;
                            case 6:
                                TimeDilationModule u3 = (TimeDilationModule)m_towerParts[i];

                                if (36 * u3.FinalRange > rotor.RangeCircle.Radius)
                                {
                                    rotor.RangeCircle.Radius = 36 * (u3.FinalRange + 1);
                                }
                                break;
                        }
                        break;
                }
            }
        }

        #region Add Parts Methods
        // Add all utility modules (boosters and misc modules)
        private void AddUtilities(ContentManager content, int i)
        {
            switch (m_propsList[i].SubIndex)
            {
                case 0:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        RangeBoostModule util = new RangeBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 7, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        util.ParentIndex = m_propsList[i].ParentIndex;
                        util.TowerIndex = m_propsList[i].SlotIndex;
                        util.ExtendedIndex = m_propsList[i].StoredIndex;
                        util.RelativeRotation = m_propsList[i].RelativeRotation;
                        util.UpdateProps();
                        m_towerParts.Add(util);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(util);
                    }
                    break;
                case 1:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        DamageBoostModule util = new DamageBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 7, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        util.ParentIndex = m_propsList[i].ParentIndex;
                        util.TowerIndex = m_propsList[i].SlotIndex;
                        util.ExtendedIndex = m_propsList[i].StoredIndex;
                        util.RelativeRotation = m_propsList[i].RelativeRotation;
                        util.UpdateProps();
                        m_towerParts.Add(util);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(util);
                    }
                    break;
                case 2:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        RateBoostModule util = new RateBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 7, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        util.ParentIndex = m_propsList[i].ParentIndex;
                        util.TowerIndex = m_propsList[i].SlotIndex;
                        util.ExtendedIndex = m_propsList[i].StoredIndex;
                        util.RelativeRotation = m_propsList[i].RelativeRotation;
                        util.UpdateProps();
                        m_towerParts.Add(util);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(util);
                    }
                    break;
                case 3:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        HealthBoostModule util = new HealthBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 7, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        util.ParentIndex = m_propsList[i].ParentIndex;
                        util.TowerIndex = m_propsList[i].SlotIndex;
                        util.ExtendedIndex = m_propsList[i].StoredIndex;
                        util.RelativeRotation = m_propsList[i].RelativeRotation;
                        util.UpdateProps();
                        m_towerParts.Add(util);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(util);
                    }
                    break;
                case 4:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        HealingModule util = new HealingModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 7, 5, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        util.ParentIndex = m_propsList[i].ParentIndex;
                        util.TowerIndex = m_propsList[i].SlotIndex;
                        util.ExtendedIndex = m_propsList[i].StoredIndex;
                        util.RelativeRotation = m_propsList[i].RelativeRotation;
                        util.UpdateProps();
                        m_towerParts.Add(util);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(util);
                    }
                    break;
                case 5:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        MotivationModule util = new MotivationModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 7, 5, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        util.ParentIndex = m_propsList[i].ParentIndex;
                        util.TowerIndex = m_propsList[i].SlotIndex;
                        util.ExtendedIndex = m_propsList[i].StoredIndex;
                        util.RelativeRotation = m_propsList[i].RelativeRotation;
                        util.UpdateProps();
                        m_towerParts.Add(util);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(util);
                    }
                    break;
                case 6:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        TimeDilationModule util = new TimeDilationModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 7, 5, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        util.ParentIndex = m_propsList[i].ParentIndex;
                        util.TowerIndex = m_propsList[i].SlotIndex;
                        util.ExtendedIndex = m_propsList[i].StoredIndex;
                        util.RelativeRotation = m_propsList[i].RelativeRotation;
                        util.UpdateProps();
                        m_towerParts.Add(util);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(util);
                    }
                    break;
            }
        }

        // Add all weapon modules
        private void AddWeapons(ContentManager content, int i)
        {
            switch (m_propsList[i].SubIndex)
            {
                case 0:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        NormalCannonModule cannon = new NormalCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 8, 4, 6, content, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        cannon.ParentIndex = m_propsList[i].ParentIndex;
                        cannon.TowerIndex = m_propsList[i].SlotIndex;
                        cannon.ExtendedIndex = m_propsList[i].StoredIndex;
                        cannon.RelativeRotation = m_propsList[i].RelativeRotation;
                        cannon.UpdateProps();
                        m_towerParts.Add(cannon);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(cannon);
                    }
                    break;
                case 1:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        HeavyCannonModule cannon = new HeavyCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 8, 4, 6, content, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        cannon.ParentIndex = m_propsList[i].ParentIndex;
                        cannon.TowerIndex = m_propsList[i].SlotIndex;
                        cannon.ExtendedIndex = m_propsList[i].StoredIndex;
                        cannon.RelativeRotation = m_propsList[i].RelativeRotation;
                        cannon.UpdateProps();
                        m_towerParts.Add(cannon);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(cannon);
                    }
                    break;
                case 2:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        LaserCannonModule cannon = new LaserCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 8, 4, 6, content, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        cannon.ParentIndex = m_propsList[i].ParentIndex;
                        cannon.TowerIndex = m_propsList[i].SlotIndex;
                        cannon.ExtendedIndex = m_propsList[i].StoredIndex;
                        cannon.RelativeRotation = m_propsList[i].RelativeRotation;
                        cannon.UpdateProps();
                        m_towerParts.Add(cannon);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(cannon);
                    }
                    break;
                case 3:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        MissileCannonModule cannon = new MissileCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 8, 4, 6, content, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        cannon.ParentIndex = m_propsList[i].ParentIndex;
                        cannon.TowerIndex = m_propsList[i].SlotIndex;
                        cannon.ExtendedIndex = m_propsList[i].StoredIndex;
                        cannon.RelativeRotation = m_propsList[i].RelativeRotation;
                        cannon.UpdateProps();
                        m_towerParts.Add(cannon);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(cannon);
                    }
                    break;
                case 4:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        FlameCannonModule cannon = new FlameCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 8, 4, 6, content, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        cannon.ParentIndex = m_propsList[i].ParentIndex;
                        cannon.TowerIndex = m_propsList[i].SlotIndex;
                        cannon.ExtendedIndex = m_propsList[i].StoredIndex;
                        cannon.RelativeRotation = m_propsList[i].RelativeRotation;
                        cannon.UpdateProps();
                        m_towerParts.Add(cannon);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(cannon);
                    }
                    break;
                case 5:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        SplashCannonModule cannon = new SplashCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 8, 4, 6, content, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        cannon.ParentIndex = m_propsList[i].ParentIndex;
                        cannon.TowerIndex = m_propsList[i].SlotIndex;
                        cannon.ExtendedIndex = m_propsList[i].StoredIndex;
                        cannon.RelativeRotation = m_propsList[i].RelativeRotation;
                        cannon.UpdateProps();
                        m_towerParts.Add(cannon);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(cannon);
                    }
                    break;
            }
        }

        // Add all tower structural struts
        private void AddComponents(ContentManager content, int i)
        {
            switch (m_propsList[i].SubIndex)
            {
                case 0:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        BasicComponentSingle component = new BasicComponentSingle(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        component.ParentIndex = m_propsList[i].ParentIndex;
                        component.TowerIndex = m_propsList[i].SlotIndex;
                        component.ExtendedIndex = m_propsList[i].StoredIndex;
                        component.RelativeRotation = m_propsList[i].RelativeRotation;
                        component.UpdateProps();
                        m_towerParts.Add(component);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(component);
                    }
                    break;
                case 1:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        BasicComponentDouble component = new BasicComponentDouble(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        component.ParentIndex = m_propsList[i].ParentIndex;
                        component.TowerIndex = m_propsList[i].SlotIndex;
                        component.ExtendedIndex = m_propsList[i].StoredIndex;
                        component.RelativeRotation = m_propsList[i].RelativeRotation;
                        component.UpdateProps();
                        m_towerParts.Add(component);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(component);
                    }
                    break;
            }
        }

        // Add rotor
        private void AddRotor(ContentManager content, int i)
        {
            switch (m_propsList[i].SubIndex)
            {
                case 0:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        SmallBasicRotor rotor = new SmallBasicRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor1"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 1:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        SmallAdvancedRotor rotor = new SmallAdvancedRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor2"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 2:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        SmallPrototypeRotor rotor = new SmallPrototypeRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor3"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 3:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        MediumBasicRotor rotor = new MediumBasicRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor1"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 4:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        MediumAdvancedRotor rotor = new MediumAdvancedRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor2"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 5:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        MediumPrototypeRotor rotor = new MediumPrototypeRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor3"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 1, 1, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 6:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        HeavyBasicRotor rotor = new HeavyBasicRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor1"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 7:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        HeavyAdvancedRotor rotor = new HeavyAdvancedRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor2"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
                case 8:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        HeavyPrototypeRotor rotor = new HeavyPrototypeRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor3"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 1, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        rotor.ParentIndex = m_propsList[i].ParentIndex;
                        rotor.TowerIndex = m_propsList[i].SlotIndex;
                        rotor.ExtendedIndex = m_propsList[i].StoredIndex;
                        rotor.RelativeRotation = m_propsList[i].RelativeRotation;
                        rotor.UpdateProps();
                        m_towerParts.Add(rotor);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(rotor);
                    }
                    break;
            }
        }

        // Add foundations
        private void AddFoundations(ContentManager content, int i)
        {
            switch (m_propsList[i].SubIndex)
            {
                case 0:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        LightFoundations founds = new LightFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 1, 4, 4, 3, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        founds.ParentIndex = m_propsList[i].ParentIndex;
                        founds.TowerIndex = m_propsList[i].SlotIndex;
                        founds.ExtendedIndex = m_propsList[i].StoredIndex;
                        founds.RelativeRotation = m_propsList[i].RelativeRotation;
                        founds.SourceRectY = 2;
                        founds.UpdateProps();
                        m_towerParts.Add(founds);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(founds);
                    }
                    else
                    {
                        LightFoundations founds = new LightFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), m_towerPos, Color.White, 1, 4, 4, 3, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        founds.ParentIndex = m_propsList[i].ParentIndex;
                        founds.TowerIndex = m_propsList[i].SlotIndex;
                        founds.ExtendedIndex = m_propsList[i].StoredIndex;
                        founds.RelativeRotation = m_propsList[i].RelativeRotation;
                        founds.SourceRectY = 2;
                        founds.UpdateProps();
                        m_towerParts.Add(founds);
                    }
                    break;
                case 1:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        MediumFoundations founds = new MediumFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 2, 4, 4, 3, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        founds.ParentIndex = m_propsList[i].ParentIndex;
                        founds.TowerIndex = m_propsList[i].SlotIndex;
                        founds.ExtendedIndex = m_propsList[i].StoredIndex;
                        founds.RelativeRotation = m_propsList[i].RelativeRotation;
                        founds.SourceRectY = 42;
                        founds.UpdateProps();
                        m_towerParts.Add(founds);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(founds);
                    }
                    else
                    {
                        MediumFoundations founds = new MediumFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), m_towerPos, Color.White, 2, 4, 4, 3, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        founds.ParentIndex = m_propsList[i].ParentIndex;
                        founds.TowerIndex = m_propsList[i].SlotIndex;
                        founds.ExtendedIndex = m_propsList[i].StoredIndex;
                        founds.RelativeRotation = m_propsList[i].RelativeRotation;
                        founds.SourceRectY = 42;
                        founds.UpdateProps();
                        m_towerParts.Add(founds);
                    }
                    break;
                case 2:
                    if (m_propsList[i].ParentIndex < 500)
                    {
                        HeavyFoundations founds = new HeavyFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), m_towerParts[m_propsList[i].ParentIndex].SlotPos(m_propsList[i].SlotIndex), Color.White, 3, 4, 4, 3, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        founds.ParentIndex = m_propsList[i].ParentIndex;
                        founds.TowerIndex = m_propsList[i].SlotIndex;
                        founds.ExtendedIndex = m_propsList[i].StoredIndex;
                        founds.RelativeRotation = m_propsList[i].RelativeRotation;
                        founds.SourceRectY = 82;
                        founds.UpdateProps();
                        m_towerParts.Add(founds);
                        m_towerParts[m_propsList[i].ParentIndex].TowerParts.Add(founds);
                    }
                    else
                    {
                        HeavyFoundations founds = new HeavyFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), m_towerPos, Color.White, 3, 4, 4, 3, m_propsList[i].Offsets, m_propsList[i].TypeIndex, m_propsList[i].SubIndex);
                        founds.ParentIndex = m_propsList[i].ParentIndex;
                        founds.TowerIndex = m_propsList[i].SlotIndex;
                        founds.ExtendedIndex = m_propsList[i].StoredIndex;
                        founds.RelativeRotation = m_propsList[i].RelativeRotation;
                        founds.SourceRectY = 82;
                        founds.UpdateProps();
                        m_towerParts.Add(founds);
                    }
                    break;
            }
        }
        #endregion

        // Update the part properties
        public void UpdateAllProps()
        {
            m_propsList = new List<PartProps>();

            for (int i = 0; i < m_towerParts.Count; i++)
            {
                m_towerParts[i].UpdateProps();
                m_propsList.Add(m_towerParts[i].Props);
            }
        }

        // Update the tower properties
        public void UpdateTowerProperties()
        {
            m_towerProps = new TowerProperties(m_towerParts);
            m_towerProps.UpdateProperties(ref m_towerHP, ref m_towerCost);
        }

        // Update all part positions and rotations according to their parents
        public void UpdatePosition()
        {
            m_propsList = new List<PartProps>();
            UpdateAllProps();

            m_coords = new Point((int)m_towerPos.X / 36, (int)m_towerPos.Y / 36);

            if (m_towerParts.Count > 0 && m_towerParts[0].Position != m_towerPos)
            {
                m_towerParts[0].Position = m_towerPos;

                for (int i = 0; i < m_towerParts.Count; i++)
                {
                    m_propsList.Add(m_towerParts[i].Props);
                    m_towerParts[i].Position = m_towerPos;
                    m_towerParts[i].UpdateMe();
                }
            }

            m_collisionRect = new Rectangle((int)m_towerPos.X - (18 * m_towerDimensions), (int)m_towerPos.Y - (18 * m_towerDimensions), 36 * m_towerDimensions, 36 * m_towerDimensions);
        }

        // Update the tower
        public void UpdateTower(GameTime gt, List<EnemyChar> enemies, List<GameChar> friendlies, List<BaseProjectile> projectiles, ContentManager content, List<Tower> towers)
        {
            m_collisionRect = new Rectangle((int)m_towerPos.X - (18 * m_towerDimensions), (int)m_towerPos.Y - (18 * m_towerDimensions), 36 * m_towerDimensions, 36 * m_towerDimensions);

            m_towerHP = (int)m_towerHP;

            m_healthBar.UpdateBar(m_towerPos);

            RemoveTower(friendlies, projectiles, towers);

            UpdateAllTowerParts(gt, enemies, friendlies, projectiles, content);
        }

        #region Update Tower Parts
        // Update all parts
        private void UpdateAllTowerParts(GameTime gt, List<EnemyChar> enemies, List<GameChar> friendlies, List<BaseProjectile> projectiles, ContentManager content)
        {
            for (int i = 0; i < m_towerParts.Count; i++)
            {
                switch (m_towerParts[i].TypeIndex)
                {
                    case 0:
                        UpdateFoundations(i);
                        break;
                    case 1:
                        UpdateRotors(gt, enemies, projectiles, content, i);
                        break;
                    case 2:
                        UpdateComponents(gt, enemies, projectiles, content, i);
                        break;
                    case 3:
                        UpdateWeapons(gt, enemies, projectiles, content, i);
                        break;
                    case 4:
                        UpdateUtilities(gt, enemies, friendlies, i);
                        break;
                }
            }
        }

        private void UpdateUtilities(GameTime gt, List<EnemyChar> enemies, List<GameChar> friendlies, int i)
        {
            switch (m_towerParts[i].SubIndex)
            {
                case 0:
                    RangeBoostModule util1 = (RangeBoostModule)m_towerParts[i];
                    util1.UpdateMe();
                    break;
                case 1:
                    DamageBoostModule util2 = (DamageBoostModule)m_towerParts[i];
                    util2.UpdateMe();
                    break;
                case 2:
                    RateBoostModule util3 = (RateBoostModule)m_towerParts[i];
                    util3.UpdateMe();
                    break;
                case 3:
                    HealthBoostModule util4 = (HealthBoostModule)m_towerParts[i];
                    util4.UpdateMe();
                    break;
                case 4:
                    HealingModule util5 = (HealingModule)m_towerParts[i];
                    util5.UpdateModule(gt, friendlies);
                    break;
                case 5:
                    MotivationModule util6 = (MotivationModule)m_towerParts[i];
                    util6.UpdateModule(gt, friendlies);
                    break;
                case 6:
                    TimeDilationModule util7 = (TimeDilationModule)m_towerParts[i];
                    util7.UpdateModule(gt, enemies);
                    break;
            }
        }

        private void UpdateWeapons(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content, int i)
        {
            switch (m_towerParts[i].SubIndex)
            {
                case 0:
                    NormalCannonModule cannon1 = (NormalCannonModule)m_towerParts[i];
                    cannon1.UpdateModule(gt, enemies, projectiles, content);
                    break;
                case 1:
                    HeavyCannonModule cannon2 = (HeavyCannonModule)m_towerParts[i];
                    cannon2.UpdateModule(gt, enemies, projectiles, content);
                    break;
                case 2:
                    LaserCannonModule cannon3 = (LaserCannonModule)m_towerParts[i];
                    cannon3.UpdateModule(gt, enemies, projectiles, content);
                    break;
                case 3:
                    MissileCannonModule cannon4 = (MissileCannonModule)m_towerParts[i];
                    cannon4.UpdateModule(gt, enemies, projectiles, content);
                    break;
                case 4:
                    FlameCannonModule cannon5 = (FlameCannonModule)m_towerParts[i];
                    cannon5.UpdateModule(gt, enemies, projectiles, content);
                    break;
                case 5:
                    SplashCannonModule cannon6 = (SplashCannonModule)m_towerParts[i];
                    cannon6.UpdateModule(gt, enemies, projectiles, content);
                    break;
            }
        }

        private void UpdateComponents(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content, int i)
        {
            switch (m_towerParts[i].SubIndex)
            {
                case 0:
                    BasicComponentSingle comp1 = (BasicComponentSingle)m_towerParts[i];
                    comp1.UpdateMe(gt, enemies, projectiles, content);
                    break;
                case 1:
                    BasicComponentDouble comp2 = (BasicComponentDouble)m_towerParts[i];
                    comp2.UpdateMe(gt, enemies, projectiles, content);
                    break;
            }
        }

        private void UpdateRotors(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content, int i)
        {
            BaseRotor rotor = (BaseRotor)m_towerParts[i];
            rotor.UpdateMe(enemies, gt, projectiles, content, m_towerParts);
        }

        private void UpdateFoundations(int i)
        {
            switch (m_towerParts[i].SubIndex)
            {
                case 0:
                    LightFoundations founds1 = (LightFoundations)m_towerParts[i];
                    founds1.UpdateMe();
                    break;
                case 1:
                    MediumFoundations founds2 = (MediumFoundations)m_towerParts[i];
                    founds2.UpdateMe();
                    break;
                case 2:
                    HeavyFoundations founds3 = (HeavyFoundations)m_towerParts[i];
                    founds3.UpdateMe();
                    break;
            }
        }
        #endregion

        // Removes the tower from the list of game towers
        private void RemoveTower(List<GameChar> friendlies, List<BaseProjectile> projectiles, List<Tower> towers)
        {
            // Remove tower, lasers and demotivate allies
            if (m_towerHP <= 0)
            {
                for (int i = 0; i < m_towerParts.Count; i++)
                {
                    switch (m_towerParts[i].TypeIndex)
                    {
                        case 3:
                            switch (m_towerParts[i].SubIndex)
                            {
                                case 2:
                                    LaserCannonModule cannon3 = (LaserCannonModule)m_towerParts[i];
                                    projectiles.Remove(cannon3.LaserBeam);
                                    break;
                            }
                            break;
                        case 4:
                            switch (m_towerParts[i].SubIndex)
                            {
                                case 5:
                                    MotivationModule motiv = (MotivationModule)m_towerParts[i];

                                    for (int k = 0; k < friendlies.Count; k++)
                                    {
                                        if (motiv.MotivationCircle.Contains(friendlies[k].Position) && friendlies[k].Health > 0)
                                        {
                                            friendlies[k].MaxHealth = friendlies[k].BaseMaxHealth;
                                            friendlies[k].Damage = friendlies[k].BaseDamage;
                                            friendlies[k].DamageTick = friendlies[k].BaseTick;
                                        }
                                    }
                                    break;
                            }
                            break;
                    }
                }

                towers.Remove(this);
            }
        }

        public void DrawTower(SpriteBatch sb, GameTime gt)
        {
            for (int i = 0; i < m_towerParts.Count; i++)
            {
                m_towerParts[i].DrawMe(sb, gt);
            }
        }
    }
}
