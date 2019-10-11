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
    /// Base Projectile on which all other projectiles are based on
    /// </summary>
    class BaseProjectile : AnimGraphic
    {
        // Properties of the projectile.
        protected float m_damage;
        protected float m_dmgTick;
        protected float m_projectileSpeed;
        protected float m_range;

        protected float m_timer;

        // This is the projectile's collision circle.
        protected Circle m_collisionCircle;

        // Special collision circle for the splash projectile.
        protected Circle m_collisionCircleRear;

        protected Circle m_rangeCircle;

        // Checks it's current range from the turret.
        // If the projectile leaves this range or collides with enemy, it has collided.
        protected float m_currentRange;
        protected bool m_hasCollided;

        protected Vector2 m_startPos;

        protected int m_projectileIndex;

        protected OffensiveModule m_parentModule;

        public float ProjectileSpeed { get { return m_projectileSpeed; } }
        public int ProjIndex { get { return m_projectileIndex; } }

        public OffensiveModule ParentModule { get { return m_parentModule; } }

        public BaseProjectile(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent) 
            : base(txr, position, tint, startSpeed, 0, scale, fps, framesX, framesY)
        {
            m_damage = damage;
            m_dmgTick = damage / 600;
            m_projectileSpeed = speed;
            m_range = range;

            m_timer = 0.05f;

            m_startPos = position;

            m_velocity = startSpeed;

            m_collisionCircle = new Circle(m_position.X, m_position.Y, 18 * scale);
            m_collisionCircleRear = new Circle(m_position.X, m_position.Y, 18 * scale);
            m_rangeCircle = new Circle(m_position.X, m_position.Y, 36 * range);

            m_currentRange = 0;

            m_projectileIndex = damageIndex;

            m_parentModule = parent;
        }

        public virtual void UpdateMe(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies)
        {
            base.UpdateMe(gt);
        }

        protected void NormalBehaviour(GameTime gt)
        {
            base.UpdateMe(gt);

            // Position the circle
            m_collisionCircle.Centre = m_position;

            // Check if projectile has left range
            if (!m_rangeCircle.Contains(m_position))
                m_hasCollided = true;
            else
                m_hasCollided = false;
        }

        protected void ProjectileBehaviour(List<BaseProjectile> projectiles, List<EnemyChar> enemies)
        {
            // Check if projectile has intersected the target.
            for (int i = 0; i < enemies.Count; i++)
            {
                if (m_collisionCircle.Intersects(enemies[i].CollisionCircle) && enemies[i].Health > 0)
                {
                    m_hasCollided = true;
                    enemies[i].Health -= (int)m_damage;
                    break;
                }
            }

            if (m_hasCollided)
                projectiles.Remove(this);
        }

        protected void MissileBehaviour(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            m_collisionCircle.Centre = m_position;

            float minDist = 9999;
            EnemyChar target = null;

            for (int i = 0; i < enemies.Count; i++)
            {
                if (Vector2.Distance(m_position, enemies[i].Position) < minDist && enemies[i].Health > 0)
                {
                    minDist = Vector2.Distance(m_position, enemies[i].Position);
                    target = enemies[i];
                }

                if (m_collisionCircle.Intersects(enemies[i].CollisionCircle) && enemies[i].Health > 0)
                {
                    Game1.sfxList[3].Play();

                    m_hasCollided = true;
                    enemies[i].Health -= (int)m_damage;
                    projectiles.Add(new SplashWave(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\SplashProjectile"), m_position, Color.White, Vector2.Zero, 0.5f, 1, 1, 1, 1, 0, 3, 3, null));
                    break;
                }
            }

            if (target != null)
            {
                m_velocity = target.Position - m_position;
                m_velocity.Normalize();
                m_velocity *= m_projectileSpeed;
                m_rot = (float)Math.Atan2(m_velocity.Y, m_velocity.X) + 1.5707f;
            }

            base.UpdateMe(gt);
        }

        protected void SplashBehaviour(List<BaseProjectile> projectiles, List<EnemyChar> enemies, GameTime gt)
        {
            m_scale += 0.03f;

            m_collisionCircle.Radius = 72 * m_scale;
            m_collisionCircleRear.Radius = 66 * m_scale;

            if (m_timer > 0)
            {
                m_timer -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (m_collisionCircle.Intersects(enemies[i].CollisionCircle) && enemies[i].Health > 0)
                    {
                        enemies[i].Health -= m_damage;
                        enemies[i].Tint = Color.Orange;
                    }
                }

                m_timer = 0.05f;
            }

            if (m_collisionCircle.Radius > (m_range * 36))
            {
                m_hasCollided = true;
            }

            if (m_hasCollided)
                projectiles.Remove(this);
        }

        protected void FlameBehaviour(List<EnemyChar> enemies, List<BaseProjectile> projectiles, GameTime gt)
        {
            m_scale += 0.03f;

            m_collisionCircle.Radius = 12 * m_scale;

            if (m_timer > 0)
            {
                m_timer -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (m_collisionCircle.Intersects(enemies[i].CollisionCircle) || m_collisionCircle.Contains(enemies[i].CollisionCircle) && enemies[i].Health > 0)
                    {
                        enemies[i].Health -= m_damage;
                        enemies[i].Tint = Color.Orange;
                    }
                }

                m_timer = 0.03f;
            }

            if (m_hasCollided)
                projectiles.Remove(this);
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            base.DrawMe(sb, gt);

#if DEBUG
            sb.DrawString(Game1.debugFont, "Col Circle: " + m_collisionCircle.Centre + " " + m_collisionCircle.Radius + "\nIs in range: " + m_rangeCircle.Contains(m_position) + "\nPos: " + m_position, m_position, Color.Purple);
#endif
        }
    }

    /// <summary>
    /// Enemy Projectile
    /// </summary>
    class EnemyProjectile : BaseProjectile
    {
        public EnemyProjectile(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent)
            : base(txr, position, tint, startSpeed, scale, fps, framesX, framesY, damage, speed, range, damageIndex, parent)
        {
            m_projectileIndex = 6;
        }

        public virtual void UpdateMe(GameTime gt, List<Tower> towers, List<BaseProjectile> projectiles)
        {
            base.UpdateMe(gt);

            m_collisionCircle.Centre = m_position;

            // Checks if the projectile has collided with a tower
            for (int i = 0; i < towers.Count; i++)
            {
                if (m_collisionCircle.Contains(towers[i].TowerPos))
                {
                    towers[i].Health -= m_damage;
                    projectiles.Remove(this);
                    break;
                }
            }

            // Remove the projectile off map
            if (m_position.X < 0 || m_position.X > 3000 || m_position.Y < 0 || m_position.Y > 2000)
                projectiles.Remove(this);
        }
    }

    /// <summary>
    /// Regular Projectile
    /// </summary>
    class NormalProjectile : BaseProjectile
    {
        public NormalProjectile(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent) 
            : base(txr, position, tint, startSpeed, scale, fps, framesX, framesY, damage, speed, range, damageIndex, parent)
        {
            m_projectileIndex = 0;
        }

        public override void UpdateMe(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies)
        {
            base.UpdateMe(gt, projectiles, enemies);

            NormalBehaviour(gt);
            ProjectileBehaviour(projectiles, enemies);
        }
    }

    /// <summary>
    /// Heavy Duty Projectile
    /// </summary>
    class HeavyProjectile : BaseProjectile
    {
        public HeavyProjectile(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent)
            : base(txr, position, tint, startSpeed, scale, fps, framesX, framesY, damage, speed, range, damageIndex, parent)
        {
            m_projectileIndex = 1;
        }

        public override void UpdateMe(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies)
        {
            base.UpdateMe(gt, projectiles, enemies);

            NormalBehaviour(gt);
            ProjectileBehaviour(projectiles, enemies);
        }
    }

    /// <summary>
    /// Laser Beam (Special code)
    /// </summary>
    class Laser : BaseProjectile
    {
        // List of bits that constitute the laser
        List<LaserBit> m_bits;
        // Array of colours to cycle through to colour the laser
        private Color[] m_colours;

        // Colour changing varibales
        private float m_elapsed;
        private float m_changeTime;
        private int m_nextColour;
        private int m_currentColour;

        public Laser(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent, Vector2 pos, float rotation)
            : base(txr, position, tint, startSpeed, scale, fps, framesX, framesY, damage, speed, range, damageIndex, parent)
        {
            m_projectileIndex = 2;

            m_colours = new Color[12];
            m_changeTime = m_parentModule.FinalFireRate / 12;
            m_elapsed = 0;
            m_nextColour = 1;
            m_currentColour = 0;

            #region Rainbow Colours
            m_colours[0] = Color.Red;
            m_colours[1] = Color.Orange;
            m_colours[2] = Color.Yellow;
            m_colours[3] = Color.YellowGreen;
            m_colours[4] = Color.Green;
            m_colours[5] = Color.Turquoise;
            m_colours[6] = Color.Cyan;
            m_colours[7] = Color.DodgerBlue;
            m_colours[8] = Color.Blue;
            m_colours[9] = new Color(128, 0, 255);
            m_colours[10] = Color.Magenta;
            m_colours[11] = new Color(255, 0, 128);
            #endregion

            m_rot = rotation;

            m_timer = 0.25f;

            m_bits = new List<LaserBit>();

            // Lock laser to the weapon
            m_position = m_parentModule.BlastPos();
            Vector2 nextParticlePos = GetRotationVector(m_rot) * 3;
            Vector2 spawnVec = m_position;

            for (int i = 0; m_parentModule.RangeCircle.Contains(spawnVec); i++)
            {
                spawnVec = m_position + (i * nextParticlePos);
                m_bits.Add(new LaserBit(m_txr, spawnVec, m_colours[0], new Circle(spawnVec.X, spawnVec.X, 2)));
            }
        }

        // Get normalized vector given the rotation (will always have a normalized result)
        private Vector2 GetRotationVector(float rotation)
        {
            return new Vector2((float)Math.Cos(m_rot - 1.5707f), (float)Math.Sin(m_rot - 1.5707f));
        }

        public override void UpdateMe(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies)
        {
            base.UpdateMe(gt, projectiles, enemies);

            LaserBehaviour(enemies, projectiles, gt);
        }

        // Update the laser
        private void LaserBehaviour(List<EnemyChar> enemies, List<BaseProjectile> projectiles, GameTime gt)
        {
            m_rot = m_parentModule.Rotation;

            ColourChanger(gt);

            m_position = m_parentModule.BlastPos();
            Vector2 nextParticlePos = GetRotationVector(m_rot) * 3;

            if (m_currentColour >= 12)
                projectiles.Remove(this);

            LockColourIndex();

            PositionLaserBits(nextParticlePos);

            DamageApplication(enemies, gt);
        }

        // Changes current colour
        private void ColourChanger(GameTime gt)
        {
            if (m_elapsed >= m_changeTime)
            {
                m_elapsed = 0;
                m_nextColour++;
                m_currentColour++;
            }
            else
                m_elapsed += (float)gt.ElapsedGameTime.TotalSeconds;
        }

        // Applies damage to all enemies hits by the laser
        private void DamageApplication(List<EnemyChar> enemies, GameTime gt)
        {
            if (m_timer > 0)
            {
                m_timer -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    for (int k = 0; k < m_bits.Count; k++)
                    {
                        if (enemies[i].CollisionCircle.Intersects(m_bits[k].CollisionCircle))
                        {
                            enemies[i].Health -= m_damage;
                            break;
                        }
                    }
                }

                m_timer = 0.25f;
            }
        }

        // Position the laser
        private void PositionLaserBits(Vector2 nextParticlePos)
        {
            for (int i = 0; i < m_bits.Count; i++)
            {
                Vector2 positionVec = m_position + (i * nextParticlePos);
                m_bits[i].Position = positionVec;
                m_bits[i].CollisionCircle.Centre = m_bits[i].Position;
                m_bits[i].Rotation = m_rot;

                m_bits[i].Tint = Color.Lerp(m_bits[i].Tint, m_colours[m_nextColour], m_elapsed / m_changeTime);
            }
        }

        // Prevent reading non existant array items
        private void LockColourIndex()
        {
            if (m_nextColour >= m_colours.Length)
                m_nextColour = 0;

            if (m_currentColour >= m_colours.Length)
                m_currentColour = 0;
        }

        public virtual void DrawLaser(SpriteBatch sb)
        {
            for (int i = 0; i < m_bits.Count; i++)
            {
                m_bits[i].DrawMe(sb);
            }
        }
    }

    /// <summary>
    /// Missile
    /// </summary>
    class Missile : BaseProjectile
    {
        public Missile(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent)
            : base(txr, position, tint, startSpeed, scale, fps, framesX, framesY, damage, speed, range, damageIndex, parent)
        {
            m_projectileIndex = 3;
        }

        public virtual void UpdateMe(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies, ContentManager content)
        {
            base.UpdateMe(gt, projectiles, enemies);

            MissileBehaviour(gt, enemies, projectiles, content);
            ProjectileBehaviour(projectiles, enemies);
        }
    }

    /// <summary>
    /// Flames
    /// </summary>
    class FlameProjectile : BaseProjectile
    {
        public FlameProjectile(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent)
            : base(txr, position, tint, startSpeed, scale, fps, framesX, framesY, damage, speed, range, damageIndex, parent)
        {
            m_projectileIndex = 4;
        }

        public override void UpdateMe(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies)
        {
            base.UpdateMe(gt, projectiles, enemies);

            NormalBehaviour(gt);
            FlameBehaviour(enemies, projectiles, gt);
        }
    }

    /// <summary>
    /// Shockwave
    /// </summary>
    class SplashWave : BaseProjectile
    {
        public SplashWave(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, float scale, int fps, int framesX, int framesY, int damage, float speed, float range, int damageIndex, OffensiveModule parent)
            : base(txr, position, tint, startSpeed, scale, fps, framesX, framesY, damage, speed, range, damageIndex, parent)
        {
            m_srcRect = new Rectangle(0, 0, m_txr.Width, m_txr.Height);

            m_collisionCircle.Centre = m_position;

            m_projectileIndex = 5;
        }

        public override void UpdateMe(GameTime gt, List<BaseProjectile> projectiles, List<EnemyChar> enemies)
        {
            base.UpdateMe(gt, projectiles, enemies);

            SplashBehaviour(projectiles, enemies, gt);
        }
    }
}
