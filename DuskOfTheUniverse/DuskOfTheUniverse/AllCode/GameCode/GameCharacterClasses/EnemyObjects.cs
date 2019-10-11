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
    /// All The classes here are really only constructors for enemies when the spawn to set up their variables
    /// Set health, types, rewards etc...
    /// </summary>

    // Normal Alien
    class AlienType1 : EnemyChar
    {
        public AlienType1(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, int maxHealth, float speed, float detectionRadius, float damage, float tickTime, Map map, UniMapData data, ContentManager content, int currentLevel, int currentWave, int difficulty) 
            : base(txr, position, tint, scale, fps, framesX, framesY, maxHealth, speed, detectionRadius, damage, tickTime, map, data, content, currentLevel, currentWave)
        {
            SetEnums();

            SetRewards(currentLevel, currentWave, 30, 90, 15, 25, 3, 6);

            m_health = (int)Math.Pow(100 + (50 * (difficulty - 1)) + (20 * currentLevel),( 1 + (currentWave * (1 / 20))));
            m_maxHealth = m_health;

            m_srcBaseY = 2;
        }

        private void SetEnums()
        {
            m_enemyType = EnemyType.Normal;
            m_combatType = CombatType.Melee;
        }
    }

    // Scout alien
    class AlienType2 : EnemyChar
    {
        public AlienType2(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, int maxHealth, float speed, float detectionRadius, float damage, float tickTime, Map map, UniMapData data, ContentManager content, int currentLevel, int currentWave, int difficulty)
            : base(txr, position, tint, scale, fps, framesX, framesY, maxHealth, speed, detectionRadius, damage, tickTime, map, data, content, currentLevel, currentWave)
        {
            SetEnums();

            SetRewards(currentLevel, currentWave, 45, 150, 20, 80, 10, 15);

            m_health = (int)Math.Pow(80 + (40 * (difficulty - 1)) + (15 * currentLevel), (1 + (currentWave * (1 / 20))));
            m_maxHealth = m_health;

            m_srcBaseY = 162;
        }

        private void SetEnums()
        {
            m_enemyType = EnemyType.Scout;
            m_combatType = CombatType.Melee;
        }
    }

    // Tank Alien
    class AlienType3 : EnemyChar
    {
        public AlienType3(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, int maxHealth, float speed, float detectionRadius, float damage, float tickTime, Map map, UniMapData data, ContentManager content, int currentLevel, int currentWave, int difficulty)
            : base(txr, position, tint, scale, fps, framesX, framesY, maxHealth, speed, detectionRadius, damage, tickTime, map, data, content, currentLevel, currentWave)
        {
            SetEnums();

            SetRewards(currentLevel, currentWave, 300, 1500, 100, 250, 30, 40);

            m_health = (int)Math.Pow(500 + (250 * (difficulty - 1)) + (50 * currentLevel), 1 + (currentWave * (1 / 20)));
            m_maxHealth = m_health;

            m_srcBaseY = 322;
        }

        private void SetEnums()
        {
            m_enemyType = EnemyType.Tank;
            m_combatType = CombatType.Melee;
        }
    }
}
