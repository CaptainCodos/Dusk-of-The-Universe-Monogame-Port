using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class BaseModule : TowerMasterPart
    {
        // Index of the slot in which the module will sit
        protected int m_offsetIndex;

        public int OffsetIndex { get { return m_offsetIndex; } set { m_offsetIndex = value; } }

        public BaseModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex) 
            : base(txr, position, tint, Vector2.Zero, 0, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_partHealth = 50;
            m_partCost = 50;
        }
    }

    class OffensiveModule : BaseModule
    {
        // Weapon damage
        protected int m_baseDamage;
        protected int m_finalDamage;

        // Weapon fire rate
        protected float m_baseFireRate;
        protected float m_finalFireRate;
        protected float m_currentTime;

        // Weapon range (int tiles)
        protected float m_baseRange;
        protected float m_finalRange;

        // Weapon damage type
        protected int m_damageTypeIndex;

        // Weapon projectile offset and shooting position
        protected Vector2 m_shootoffset;
        protected Vector2 m_shootPos;

        // Matrix to be used to convert the offset to a shooting position
        protected Matrix m_rotationMatrix;

        // Check if the module should be firing
        protected bool m_isFiring;

        protected BaseProjectile m_projectile;

        protected Circle m_rangeCircle;

        public float BaseFireRate { get { return m_baseFireRate; } }
        public int BaseDamage { get { return m_baseDamage; } }
        public float BaseRange { get { return m_baseRange; } }

        public float FinalFireRate { get { return m_finalFireRate; } set { m_finalFireRate = value; } }
        public int FinalDamage { get { return m_finalDamage; } set { m_finalDamage = value; } }
        public float FinalRange { get { return m_finalRange; } set { m_finalRange = value; } }

        public float CurrentTime { get { return m_currentTime; } }

        public Vector2 ShootOffset { get { return m_shootoffset; } }
        public Circle RangeCircle { get { return m_rangeCircle; } }

        public OffensiveModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_baseDamage = 2;
            m_finalDamage = 0;

            m_baseFireRate = 0.5f;
            m_finalFireRate = 0;

            m_baseRange = 6;
            m_rangeCircle = new Circle(m_position.X, m_position.Y, 36 * m_baseRange);
            m_finalRange = 0;

            m_damageTypeIndex = 0;

            m_shootoffset = new Vector2(0, -15);

            m_isFiring = false;

            m_srcRect.Y = 40 * m_subIndex + 2;
        }

        public virtual void UpdateModule(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies, ContentManager content)
        {
            m_rangeCircle.Centre = m_position;
            m_rangeCircle.Radius = m_finalRange * 36;

            // Initiate firing mechanics
            // Check if there are enemies around and if one is in distance.
            // If so then flag firing
            for (int i = 0; i < enemies.Count; i++)
            {
                float distance = Vector2.Distance(m_position, enemies[i].Position);

                if (m_rangeCircle.Contains(enemies[i].Position) && enemies[i].Health > 0)
                {
                    m_isFiring = true;
                    m_fps = 8;
                    break;
                }
                else
                {
                    m_isFiring = false;
                    m_fps = 2;
                }
            }

            if (enemies.Count <= 0)
            {
                m_isFiring = false;
                m_fps = 2;
            }
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            base.DrawMe(sb, gt);
        }

        // Outputs the vector offset a projectile is fired from
        public Vector2 BlastPos()
        {
            m_rotationMatrix = Matrix.CreateRotationZ(m_rot);

            m_shootPos = Vector2.Transform(m_shootoffset, m_rotationMatrix);

            return (m_shootPos + m_position);
        }

        protected void FiringMechanics(ContentManager content, List<BaseProjectile> projectiles, BaseProjectile projectile, GameTime gt)
        {
            // Firing Mechanics
            if (m_isFiring)
            {
                if (m_currentTime <= 0)
                {
                    switch(m_subIndex)
                    {
                        case 0:
                            Game1.sfxList[0].Play();
                            break;
                        case 1:
                            Game1.sfxList[0].Play();
                            break;
                        case 3:
                            Game1.sfxList[0].Play();
                            break;
                        case 4:
                            Game1.sfxList[3].Play();
                            break;
                        case 5:
                            Game1.sfxList[3].Play();
                            break;
                    }

                    m_currentTime = m_finalFireRate;
                    m_projectile = projectile;
                    m_projectile.Position = BlastPos();
                    m_projectile.Velocity = Vector2.Zero;
                    m_projectile.Velocity = new Vector2((float)Math.Cos(m_rot - 1.5707f), (float)Math.Sin(m_rot - 1.5707f));
                    m_projectile.Velocity.Normalize();
                    m_projectile.Velocity *= m_projectile.ProjectileSpeed;
                    m_projectile.Rotation = m_rot;
                    projectiles.Add(m_projectile);
                }
                else
                {
                    m_currentTime -= (float)gt.ElapsedGameTime.TotalSeconds;
                }
            }
            else
            {
                m_currentTime -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }

        protected void LaserFiringMechanics(ContentManager content, List<BaseProjectile> projectiles, BaseProjectile projectile, GameTime gt)
        {
            // Firing Mechanics for a laser (LAZARS ARE SPECIAL! :D)
            if (m_isFiring)
            {
                if (m_currentTime <= 0)
                {
                    Game1.sfxList[1].Play();

                    m_currentTime = m_baseFireRate;
                    m_projectile = projectile;
                    m_projectile.Position = BlastPos();
                    m_projectile.Rotation = m_rot;

                    if (projectiles.Contains(m_projectile))
                    {
                        projectiles.Remove(m_projectile);
                    }

                    projectiles.Add(m_projectile);
                }
                else
                {
                    m_currentTime -= (float)gt.ElapsedGameTime.TotalSeconds;
                }
            }
            else
            {
                m_currentTime -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }

        // Debug
        public void DrawModuleInfo(SpriteBatch sb)
        {
#if DEBUG
            sb.DrawString(Game1.debugFont, "Damage: " + m_finalDamage + " Range: " + m_finalRange + " FIreRate: " + m_finalFireRate, m_position + new Vector2(100, 0), Color.Red);
#endif
        }
    }

    class UtilityModule : BaseModule
    {
        // This is the base effectiveness of the module and it's final effectiveness
        protected float m_baseEffectivenessFactor;
        protected float m_finalEffectivenessFactor;

        protected float m_boostFactor;

        public float Boostfactor { get { return m_boostFactor; } }

        public float BaseEffectiveness { get { return m_baseEffectivenessFactor; } }
        public float FinalEffectiveness { get { return m_finalEffectivenessFactor; } set { m_finalEffectivenessFactor = value; } }

        public UtilityModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_baseEffectivenessFactor = 1;
            m_finalEffectivenessFactor = m_baseEffectivenessFactor;

            m_boostFactor = 1.5f;
            m_partCost = 50;
            m_partHealth = 50;

            SourceRectY = (40 * m_subIndex) + 2;
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            base.DrawMe(sb, gt);
        }
    }
}
