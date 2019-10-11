using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    /// <summary>
    /// Range Booster Module
    /// </summary>
    class RangeBoostModule : UtilityModule
    {
        public RangeBoostModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
        }
    }

    /// <summary>
    /// Damage Booster Module
    /// </summary>
    class DamageBoostModule : UtilityModule
    {
        public DamageBoostModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
        }
    }

    /// <summary>
    /// Rate of Fire Booster Module
    /// </summary>
    class RateBoostModule : UtilityModule
    {
        public RateBoostModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
        }
    }

    /// <summary>
    /// Health Booster Module
    /// </summary>
    class HealthBoostModule : UtilityModule
    {
        public HealthBoostModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_boostFactor = 1.1f;
        }
    }

    /// <summary>
    /// Healing Module
    /// Heals nearby allies
    /// </summary>
    class HealingModule : UtilityModule
    {
        // Allies within this circle get healed
        private Circle m_healingCircle;

        // Healing time can be boosted (base is 10/s)
        private float m_baseHealingTime;
        private float m_finalHealingTime;
        private float m_currTime;

        // Range of the healing
        private float m_baseRange;
        private float m_finalRange;

        public float BaseHealingTime { get { return m_baseHealingTime; } }
        public float FinalHealingTime { get { return m_finalHealingTime; } set { m_finalHealingTime = value; } }

        public float BaseRange { get { return m_baseRange; } }
        public float FinalRange { get { return m_finalRange; } set { m_finalRange = value; } }

        public Circle HealingCircle { get { return m_healingCircle; } set { m_healingCircle = value; } }

        public HealingModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, float range, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            m_baseHealingTime = 0.1f;
            m_finalHealingTime = m_baseHealingTime;

            m_baseRange = range;

            m_healingCircle = new Circle(position.X, position.Y, m_finalRange * 36);
        }

        public virtual void UpdateModule(GameTime gt, List<GameChar> friendlies)
        {
            base.UpdateMe(gt);

            m_healingCircle.Radius = 36 * m_finalRange;
            m_healingCircle.Centre = m_position;

            // Heal if healing time is 0
            if (m_currTime <= 0)
            {
                m_currTime = m_finalHealingTime;

                for (int i = 0; i < friendlies.Count; i++)
                {
                    if (m_healingCircle.Contains(friendlies[i].Position) && friendlies[i].Health > 0 && friendlies[i].Health < friendlies[i].MaxHealth)
                    {
                        friendlies[i].Health += 1;
                    }
                }
            }
            else
            {
                m_currTime -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }
    }

    /// <summary>
    /// Motivation Module
    /// Motivates allies by boosting max health, damage output and rate of damage output
    /// For maximal effect, add a healing module with this to heal the extra space given for health
    /// </summary>
    class MotivationModule : UtilityModule
    {
        // Allies within this are motivated
        private Circle m_motivationCircle;

        // Range can be extended
        private float m_baseRange;
        private float m_finalRange;

        public float BaseRange { get { return m_baseRange; } }
        public float FinalRange { get { return m_finalRange; } set { m_finalRange = value; } }

        public Circle MotivationCircle { get { return m_motivationCircle; } }

        public MotivationModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, float range, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            // Effectiveness of the module can be boosted
            m_baseEffectivenessFactor = 1.2f;
            m_finalEffectivenessFactor = 1.2f;

            m_baseRange = range;

            m_motivationCircle = new Circle(position.X, position.Y, m_finalRange * 36);
        }

        public virtual void UpdateModule(GameTime gt, List<GameChar> friendlies)
        {
            base.UpdateMe(gt);

            m_motivationCircle.Radius = 36 * m_finalRange;
            m_motivationCircle.Centre = m_position;

            // Motivate nearby allies
            for (int i = 0; i < friendlies.Count; i++)
            {
                // If the ally can be motivated further by this module, then motivate, otherwise continue
                if (m_motivationCircle.Contains(friendlies[i].Position) && friendlies[i].Health > 0 && (int)(friendlies[i].BaseMaxHealth * m_finalEffectivenessFactor) > friendlies[i].MaxHealth)
                {
                    Game1.sfxList[2].Play();

                    friendlies[i].MaxHealth = (int)(friendlies[i].BaseMaxHealth * m_finalEffectivenessFactor);
                    friendlies[i].Damage = friendlies[i].BaseDamage * m_finalEffectivenessFactor;
                    friendlies[i].DamageTick = friendlies[i].BaseTick * m_finalEffectivenessFactor;
                }
                else
                {
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// Time Dilation Module
    /// Slows nearby enemies to a crawl
    /// </summary>
    class TimeDilationModule : UtilityModule
    {
        // Enemies within this are slowed
        private Circle m_dilationCircle;

        // Range can be boosted
        private float m_baseRange;
        private float m_finalRange;

        public float BaseRange { get { return m_baseRange; } }
        public float FinalRange { get { return m_finalRange; } set { m_finalRange = value; } }

        public Circle DilationCircle { get { return m_dilationCircle; } set { m_dilationCircle = value; } }

        public TimeDilationModule(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, float range, List<Vector2> offsets, int typeIndex, int subIndex)
            : base(txr, position, tint, scale, fps, framesX, framesY, offsets, typeIndex, subIndex)
        {
            // Effectiveness can be boosted
            m_baseEffectivenessFactor = 2;
            m_finalEffectivenessFactor = 2;

            m_baseRange = range;

            m_dilationCircle = new Circle(position.X, position.Y, m_finalRange * 36);
        }

        public virtual void UpdateModule(GameTime gt, List<EnemyChar> enemies)
        {
            base.UpdateMe(gt);

            m_dilationCircle.Radius = 36 * m_finalRange;
            m_dilationCircle.Centre = m_position;

            // Slow all enemies within range
            for (int i = 0; i < enemies.Count; i++)
            {
                if (m_dilationCircle.Contains(enemies[i].Position))
                {
                    enemies[i].Speed = enemies[i].BaseSpeed / m_finalEffectivenessFactor;
                }
                else
                {
                    enemies[i].Speed = enemies[i].BaseSpeed;
                }
            }
        }
    }
}
