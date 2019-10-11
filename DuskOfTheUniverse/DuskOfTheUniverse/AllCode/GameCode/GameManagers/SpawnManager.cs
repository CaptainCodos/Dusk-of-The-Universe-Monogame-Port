using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    struct AllySpawnZone
    {
        public Point PointCoords { get; set; }

        public Rectangle SpawnArea { get; set; }
    }

    struct EnemySpawnZone
    {
        public Point PointCoords { get; set; }

        public Rectangle SpawnArea { get; set; }
        public bool IsActive { get; set; }
    }

    class AllySpawnManager
    {
        private AllySpawnZone m_sZone;
    }

    class EnemySpawnManager
    {
        // List of all the spawn zones of the enemy.
        private List<EnemySpawnZone> m_sZones;

        // Store the game map.
        private Map m_map;
        private UniMapData m_data;

        // Bool to start the wave system.
        private bool m_wavesActive;

        private bool m_isSpawning;

        // Variables for alien spawning.
        private int m_currWave;
        private const float BASEWAVETIMER = 30;
        private float m_waveTimer;
        private float m_currTime;

        private int m_numberToSpawn1;
        private int m_num1;
        private float m_shortTime1;

        private int m_numberToSpawn2;
        private int m_num2;
        private float m_shortTime2;

        private int m_numberToSpawn3;
        private int m_num3;
        private float m_shortTime3;

        public float WaveTime { get { return m_waveTimer; } }
        public float CurrTime { get { return m_currTime; } }

        public int CurrentWave { get { return m_currWave; } }

        public EnemySpawnManager(UniMapData data, Map map)
        {
            m_wavesActive = false;
            m_isSpawning = false;
            m_waveTimer = BASEWAVETIMER;
            m_currTime = m_waveTimer;

            m_numberToSpawn1 = 5;
            m_num1 = m_numberToSpawn1;
            m_shortTime1 = 1;

            m_numberToSpawn2 = 2;
            m_num2 = m_numberToSpawn2;
            m_shortTime2 = 1;

            m_numberToSpawn3 = 0;
            m_num3 = m_numberToSpawn3;
            m_shortTime3 = 3.2f;

            m_map = map;
            m_data = data;

            m_currWave = 1;

            m_sZones = new List<EnemySpawnZone>();

            // Loop through from the top of the map to the bottom.
            // If either the left most tile at this y coordinate or the right most tile at y coordinate is walkable, then create and add spawn zone.
            CreateSpawnZones(data);
        }

        private void CreateSpawnZones(UniMapData data)
        {
            for (int i = 0; i < data.WalkableGrid.GetLength(1); i++)
            {
                // Make Spawn zones on the left
                if (data.WalkableGrid[0, i] <= 0)
                {
                    EnemySpawnZone newZone = new EnemySpawnZone();
                    newZone.IsActive = true;
                    newZone.PointCoords = new Point(0, i);
                    newZone.SpawnArea = new Rectangle(-36 * 4 - 18, 36 * i - 18 + (36 * 4), 36 * 8, 36 * 8);
                    m_sZones.Add(newZone);
                }

                // Make spawn zones on the right
                if (data.WalkableGrid[data.WalkableGrid.GetLength(0) - 1, i] <= 0)
                {
                    EnemySpawnZone newZone = new EnemySpawnZone();
                    newZone.IsActive = true;
                    newZone.PointCoords = new Point(data.WalkableGrid.GetLength(0) - 1, i);
                    newZone.SpawnArea = new Rectangle((data.WalkableGrid.GetLength(0) - 1) * 36 + (-36 * 4 - 18), 36 * i - 18 + (36 * 4), 36 * 8, 36 * 8);
                    m_sZones.Add(newZone);
                }
            }
        }

        public void UpdateManager(List<EnemyChar> enemies, GameTime gt, ContentManager content, string mainPath, PlayerStats userData)
        {
            SpawnMechanics(enemies, gt, content, mainPath, userData);
        }

        private void SpawnMechanics(List<EnemyChar> enemies, GameTime gt, ContentManager content, string mainPath, PlayerStats userData)
        {
            if (m_currTime <= 0)
            {
                // Increase the numbers of each alien to spawn each wave
                IncreaseSpawnNumbers();

                // Spawn regular aliens
                SpawnNormals(enemies, gt, content, mainPath, userData);

                // Spawn scout aliens
                SpawnScouts(enemies, gt, content, mainPath, userData);

                // Spawn tank aliens
                SpawnTanks(enemies, gt, content, mainPath, userData);

                // Reset individual spawn timers
                ResetTimers();
            }
            else
            {
                m_currTime -= (float)gt.ElapsedGameTime.TotalSeconds;
            }
        }

        private void ResetTimers()
        {
            // When all aliens are spawned reset timers
            if (m_num1 <= 0 && m_num2 <= 0 && m_num3 <= 0)
            {
                m_currTime = m_waveTimer;
                m_isSpawning = false;
                m_shortTime1 = 1;
                m_shortTime2 = 1;
                m_shortTime3 = 3.2f;
                m_currWave++;
            }
        }

        #region Spawn Aliens
        /// <summary>
        /// Spawn tank aliens at random spawn zones
        /// </summary>
        /// <param name="enemies"> list of enemies to add to</param>
        /// <param name="gt"> gametime timer</param>
        /// <param name="content"> content loading</param>
        /// <param name="mainPath"> path to alien art resources</param>
        /// <param name="userData"> data of the currently logged in player </param>
        private void SpawnTanks(List<EnemyChar> enemies, GameTime gt, ContentManager content, string mainPath, PlayerStats userData)
        {
            if (m_num3 > 0)
            {
                if (m_shortTime3 > 0)
                {
                    m_shortTime3 -= (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    int randomZone = Game1.RNG.Next(0, m_sZones.Count);

                    while (!m_sZones[randomZone].IsActive)
                    {
                        randomZone = Game1.RNG.Next(0, m_sZones.Count);
                    }

                    m_shortTime3 = 3.2f;

                    AlienType3 newAlien = new AlienType3(content.Load<Texture2D>(mainPath + "\\Level1EnemySheet"), m_map.Tiles[m_sZones[randomZone].PointCoords.X, m_sZones[randomZone].PointCoords.Y].Position, Color.White, 1, 4, 4, 12, (int)Math.Pow(500 + (10 * m_map.LevelNumber), 1.05), 2, 2, 15, 3, m_map, m_data, content, m_map.LevelNumber, m_currWave, userData.Difficulty);
                    newAlien.CoordsX = m_sZones[randomZone].PointCoords.X;
                    newAlien.CoordsY = m_sZones[randomZone].PointCoords.Y;
                    enemies.Add(newAlien);

                    m_num3--;
                }
            }
        }

        /// <summary>
        /// Spawn scout aliens at random spawn zones
        /// </summary>
        /// <param name="enemies"> list of enemies to add to</param>
        /// <param name="gt"> gametime timer</param>
        /// <param name="content"> content loading</param>
        /// <param name="mainPath"> path to alien art resources</param>
        /// <param name="userData"> data of the currently logged in player </param>
        private void SpawnScouts(List<EnemyChar> enemies, GameTime gt, ContentManager content, string mainPath, PlayerStats userData)
        {
            if (m_num2 > 0)
            {
                if (m_shortTime2 > 0)
                {
                    m_shortTime2 -= (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    int randomZone = Game1.RNG.Next(0, m_sZones.Count);

                    while (!m_sZones[randomZone].IsActive)
                    {
                        randomZone = Game1.RNG.Next(0, m_sZones.Count);
                    }

                    m_shortTime2 = 1;

                    AlienType2 newAlien = new AlienType2(content.Load<Texture2D>(mainPath + "\\Level1EnemySheet"), m_map.Tiles[m_sZones[randomZone].PointCoords.X, m_sZones[randomZone].PointCoords.Y].Position, Color.White, 1, 4, 4, 12, (int)Math.Pow(80 + (10 * m_map.LevelNumber), 1.1), 8, 2, 9, 2, m_map, m_data, content, m_map.LevelNumber, m_currWave, userData.Difficulty);
                    newAlien.CoordsX = m_sZones[randomZone].PointCoords.X;
                    newAlien.CoordsY = m_sZones[randomZone].PointCoords.Y;
                    enemies.Add(newAlien);

                    m_num2--;
                }
            }
        }

        /// <summary>
        /// Spawn regular aliens at random spawn zones
        /// </summary>
        /// <param name="enemies"> list of enemies to add to</param>
        /// <param name="gt"> gametime timer</param>
        /// <param name="content"> content loading</param>
        /// <param name="mainPath"> path to alien art resources</param>
        /// <param name="userData"> data of the currently logged in player </param>
        private void SpawnNormals(List<EnemyChar> enemies, GameTime gt, ContentManager content, string mainPath, PlayerStats userData)
        {
            if (m_num1 > 0)
            {
                if (m_shortTime1 > 0)
                {
                    m_shortTime1 -= (float)gt.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    int randomZone = Game1.RNG.Next(0, m_sZones.Count);

                    while (!m_sZones[randomZone].IsActive)
                    {
                        randomZone = Game1.RNG.Next(0, m_sZones.Count);
                    }

                    m_shortTime1 = 1;

                    AlienType1 newAlien = new AlienType1(content.Load<Texture2D>(mainPath + "\\Level1EnemySheet"), m_map.Tiles[m_sZones[randomZone].PointCoords.X, m_sZones[randomZone].PointCoords.Y].Position, Color.White, 1, 4, 4, 12, (int)Math.Pow(100 + (10 * m_map.LevelNumber), 1.05), 4, 2, 3, 1, m_map, m_data, content, m_map.LevelNumber, m_currWave, userData.Difficulty);
                    newAlien.CoordsX = m_sZones[randomZone].PointCoords.X;
                    newAlien.CoordsY = m_sZones[randomZone].PointCoords.Y;
                    enemies.Add(newAlien);

                    m_num1--;
                }
            }
        }
        #endregion

        private void IncreaseSpawnNumbers()
        {
            // Increase numbers of aliens to spawn
            if (!m_isSpawning)
            {
                m_num1 = 5 + (3 * m_currWave);
                m_num2 = 2 + (1 * m_currWave);
                m_num3 = 0 + (int)(0.5f * m_currWave);
            }

            m_isSpawning = true;
        }

        // basic debug spawning
        public void DrawData(SpriteBatch sb)
        {
            sb.Begin();
#if DEBUG
            sb.DrawString(Game1.debugFont, "NUM to spawn: " + m_numberToSpawn1 + "  NUM left: " + m_num1 + "\nShort TimeLeft: " + m_shortTime1 + "\nTimeLeft: " + m_currTime, new Vector2(1200, 1), Color.Red);
#endif
            sb.End();
        }
    }
}
