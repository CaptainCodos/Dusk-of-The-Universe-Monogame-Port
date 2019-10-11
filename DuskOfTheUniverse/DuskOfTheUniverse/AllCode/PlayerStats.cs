using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    [Serializable]
    public class PlayerStats
    {
        // Login information
        public string Username { get; set; }
        public string Password { get; set; }

        // How much progress the player has made
        public int LevelsComplete { get; set; }
        // How many levels and repeated levels the player has completed
        public int NumberOfLevelsCompleted { get; set; }
        // Game difficulty
        public int Difficulty { get; set; }

        // Check if the player has already watched the intro
        public bool IntroPlayed { get; set; }

        // Weapons used
        public int OffensiveModulesUsed { get; set; }
        // Misc modules used
        public int UtilityModulesUsed { get; set; }
        // How long the player has been in gameplay
        public float TotalGamePlayTime { get; set; }

        // Cash the player has
        public int Cash { get; set; }

        // Player level information
        public int PlayerXP { get; set; }
        public int PlayerLVL { get; set; }

        // XP needed to reach the next level
        public int XPrequired { get; set; }

        // Achievement information
        public int[] Achievements { get; set; }
        // Counts used to tell when achievement is unlocked
        public int[] AchievementCounts { get; set; }

        // Ending to be shown
        public int CurrentEnding { get; set; }

        public PlayerStats()
        {
            Achievements = new int[27];
            AchievementCounts = new int[10];
            Difficulty = 1;
            IntroPlayed = false;
        }

        public void UpdateUserStats()
        {
            HandleLevelData();

            AchievementCounts[0] = PlayerLVL;
            AchievementCounts[4] = Cash;
            AchievementCounts[5] = Difficulty;

            EndingHandling();

            HandleAchievements();
        }

        // Handles leveling up mechanics
        private void HandleLevelData()
        {
            XPrequired = (int)(100 * Math.Pow(1.1, PlayerLVL));

            if (PlayerXP >= XPrequired)
            {
                PlayerXP = 0 + (PlayerXP - XPrequired);
                PlayerLVL++;
            }
        }

        // Handle which ending is to be displayed.
        private void EndingHandling()
        {
            if (LevelsComplete >= 4)
            {
                int averageOffense;
                int averageUtility;
                float averageLevelTime;
                GetPlayerAverages(out averageOffense, out averageUtility, out averageLevelTime);

                int endingScore = (int)((10 * averageUtility) + (-10 * averageOffense) + (0.5f * (averageLevelTime - 180)));

                if (endingScore < -50)
                {
                    int averageDifference = averageUtility - averageOffense;

                    if (averageDifference < -5)
                        IncrementEndingValues(6, 24);
                    else
                        IncrementEndingValues(7, 25);
                }
                else if (endingScore > 50)
                {
                    int averageDifference = averageUtility - averageOffense;

                    if (averageDifference < -5)
                        IncrementEndingValues(7, 25);
                    else
                        IncrementEndingValues(8, 26);
                }
                else
                    IncrementEndingValues(8, 26);

                OffensiveModulesUsed = 0;
                UtilityModulesUsed = 0;
                TotalGamePlayTime = 0;
                NumberOfLevelsCompleted = 0;
            }
        }

        // Get the average modules used and the average time it took to complete a level
        private void GetPlayerAverages(out int averageOffense, out int averageUtility, out float averageLevelTime)
        {
            averageOffense = (int)(OffensiveModulesUsed / NumberOfLevelsCompleted);
            averageUtility = (int)(UtilityModulesUsed / NumberOfLevelsCompleted);
            averageLevelTime = TotalGamePlayTime / NumberOfLevelsCompleted;
        }

        private void IncrementEndingValues(int achCountsIndex, int achIndex)
        {
            AchievementCounts[achCountsIndex]++;
            Achievements[achIndex] = 1;
            CurrentEnding = 0;
            Difficulty++;
        }

        // Achievement handler
        private void HandleAchievements()
        {
            HandleLevelAchievements();

            HandleKills1Achievements();

            HandleKills2Achievements();

            HandleKills3Achievements();

            HandleCashAchievements();

            HandleDifficultyAchievements();

            HandleEndingAchievements();
        }

        #region Achievements
        private void HandleEndingAchievements()
        {
            if (AchievementCounts[6] > 0)
                Achievements[24] = 1;
            if (AchievementCounts[7] > 0)
                Achievements[25] = 1;
            if (AchievementCounts[8] > 0)
                Achievements[26] = 1;
        }

        private void HandleDifficultyAchievements()
        {
            if (AchievementCounts[5] >= 2)
                Achievements[20] = 1;
            if (AchievementCounts[5] >= 3)
                Achievements[21] = 1;
            if (AchievementCounts[5] >= 6)
                Achievements[22] = 1;
            if (AchievementCounts[5] >= 11)
                Achievements[23] = 1;
        }

        private void HandleCashAchievements()
        {
            if (AchievementCounts[4] >= 5000)
                Achievements[16] = 1;
            if (AchievementCounts[4] >= 10000)
                Achievements[17] = 1;
            if (AchievementCounts[4] >= 25000)
                Achievements[18] = 1;
            if (AchievementCounts[4] >= 100000)
                Achievements[19] = 1;
        }

        private void HandleKills3Achievements()
        {
            if (AchievementCounts[3] >= 1)
                Achievements[12] = 1;
            if (AchievementCounts[3] >= 5)
                Achievements[13] = 1;
            if (AchievementCounts[3] >= 10)
                Achievements[14] = 1;
            if (AchievementCounts[3] >= 100)
                Achievements[15] = 1;
        }

        private void HandleKills2Achievements()
        {
            if (AchievementCounts[2] >= 10)
                Achievements[8] = 1;
            if (AchievementCounts[2] >= 50)
                Achievements[9] = 1;
            if (AchievementCounts[2] >= 100)
                Achievements[10] = 1;
            if (AchievementCounts[2] >= 1000)
                Achievements[11] = 1;
        }

        private void HandleKills1Achievements()
        {
            if (AchievementCounts[1] >= 10)
                Achievements[4] = 1;
            if (AchievementCounts[1] >= 50)
                Achievements[5] = 1;
            if (AchievementCounts[1] >= 100)
                Achievements[6] = 1;
            if (AchievementCounts[1] >= 1000)
                Achievements[7] = 1;
        }

        private void HandleLevelAchievements()
        {
            if (AchievementCounts[0] >= 10)
                Achievements[0] = 1;
            if (AchievementCounts[0] >= 25)
                Achievements[1] = 1;
            if (AchievementCounts[0] >= 50)
                Achievements[2] = 1;
            if (AchievementCounts[0] >= 100)
                Achievements[3] = 1;
        }
        #endregion
    }

    /// <summary>
    /// Stores all player data in an array and can convert to list
    /// </summary>
    [Serializable]
    public class StoredDataContainer
    {
        public PlayerStats[] MyPlayerStats { get; set; }

        public StoredDataContainer() { }

        // Converts list of stats to array of stats
        public StoredDataContainer(List<PlayerStats> myStats)
        {
            MyPlayerStats = myStats.ToArray();
        }

        // Converts array of stats to loist of stats
        public List<PlayerStats> ToList()
        {
            List<PlayerStats> classList = new List<PlayerStats>();

            if (MyPlayerStats != null)
            {
                for (int i = 0; i < MyPlayerStats.Length; i++)
                    classList.Add(MyPlayerStats[i]);
            }

            return classList;
        }
    }
}
