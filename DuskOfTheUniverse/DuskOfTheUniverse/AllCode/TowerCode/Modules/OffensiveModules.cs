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
    /// Normal Cannon Module 
    /// </summary>
    class NormalCannonModule : OffensiveModule
    {
        public NormalCannonModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, List<Vector2> offsets, int typeIndex, int subIndex) 
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_damageTypeIndex = 0;

            m_baseDamage = 5;
            m_baseRange = 6;
        }

        public virtual void UpdateModule(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            base.UpdateModule(gt, projectiles, enemies, content);

            FiringMechanics(content, projectiles, new NormalProjectile(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\NormalProjectile"), BlastPos(), Color.White, Vector2.Zero, 0.5f, 4, 4, 1, m_finalDamage, 6, m_finalRange, m_damageTypeIndex, this), gt);
        }
    }

    /// <summary>
    /// Heavy Duty Cannon Module
    /// </summary>
    class HeavyCannonModule : OffensiveModule
    {
        public HeavyCannonModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_damageTypeIndex = 0;

            m_baseDamage = 10;
            m_baseRange = 7;
            m_baseFireRate = 0.75f;

            m_partHealth = 100;
            m_partCost = 100;
        }

        public virtual void UpdateModule(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            base.UpdateModule(gt, projectiles, enemies, content);

            FiringMechanics(content, projectiles, new HeavyProjectile(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\HeavyProjectile"), BlastPos(), Color.White, Vector2.Zero, 0.5f, 4, 4, 1, m_finalDamage, 4, m_finalRange, m_damageTypeIndex, this), gt);
        }
    }

    /// <summary>
    /// Laser Cannon Module
    /// </summary>
    class LaserCannonModule : OffensiveModule
    {
        Laser laser;
        public Laser LaserBeam { get { return laser; } set { laser = value; } }

        public LaserCannonModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_damageTypeIndex = 0;

            m_baseDamage = 1;
            m_baseRange = 7;
            m_baseFireRate = 9;

            m_partHealth = 75;
            m_partCost = 75;

            Laser laser = new Laser(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\LaserParticle"), BlastPos(), Color.White, Vector2.Zero, 1, 4, 4, 3, m_finalDamage, 0, m_finalRange, m_damageTypeIndex, this, BlastPos(), m_rot);
        }

        public virtual void UpdateModule(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            base.UpdateModule(gt, projectiles, enemies, content);

            // Update Laser and Replace Laser
            if (m_currentTime <= 0)
            {
                projectiles.Remove(laser);
                laser = new Laser(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\LaserParticle"), BlastPos(), Color.White, Vector2.Zero, 1, 4, 4, 3, m_finalDamage, 0, m_finalRange, m_damageTypeIndex, this, BlastPos(), m_rot);
            }

            // Laser Firing Mechanics
            LaserFiringMechanics(content, projectiles, laser, gt);
        }
    }

    /// <summary>
    /// Missile Launcher Module
    /// </summary>
    class MissileCannonModule : OffensiveModule
    {
        public MissileCannonModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_damageTypeIndex = 0;

            m_baseDamage = 10;
            m_baseRange = 8;
            m_baseFireRate = 2;

            m_partHealth = 85;
            m_partCost = 85;
        }

        public virtual void UpdateModule(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            base.UpdateModule(gt, projectiles, enemies, content);

            FiringMechanics(content, projectiles, new Missile(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\MissileProjectile"), BlastPos(), Color.White, Vector2.Zero, 1, 12, 4, 1, m_finalDamage, 3, m_finalRange, m_damageTypeIndex, this), gt);
        }
    }

    /// <summary>
    /// Flame Thrower Module
    /// </summary>
    class FlameCannonModule : OffensiveModule
    {
        public FlameCannonModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_damageTypeIndex = 0;

            m_baseDamage = 1;
            m_baseRange = 5;
            m_baseFireRate = 1;

            m_partHealth = 80;
            m_partCost = 80;
        }

        public virtual void UpdateModule(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            base.UpdateModule(gt, projectiles, enemies, content);

            FiringMechanics(content, projectiles, new FlameProjectile(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\FlameProjectileMK2"), BlastPos(), Color.White, Vector2.Zero, 0.7f, 8, 4, 1, m_finalDamage, 1, m_finalRange, m_damageTypeIndex, this), gt);
        }
    }

    /// <summary>
    /// Shockwave Module
    /// </summary>
    class SplashCannonModule : OffensiveModule
    {
        public SplashCannonModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_damageTypeIndex = 0;

            m_baseDamage = 1;
            m_baseRange = 3;
            m_baseFireRate = 0.75f;

            m_partHealth = 80;
            m_partCost = 80;
        }

        public virtual void UpdateModule(GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            base.UpdateModule(gt, projectiles, enemies, content);

            FiringMechanics(content, projectiles, new SplashWave(content.Load<Texture2D>("Art\\GameArt\\Projectiles\\SplashProjectile"), BlastPos(), Color.White, Vector2.Zero, 0.5f, 1, 1, 1, m_finalDamage, 0, m_finalRange, m_damageTypeIndex, this), gt);
        }
    }
}
