using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class AchievementsMenu
    {
        // This is a list of rows of buttons that will represent player achievements
        private List<NormalMenu> m_achievemenyRows;
        private List<Vector2> m_rowYPositions;
        private List<string> m_names;

        // This is the back button
        private NormalButton m_backButton;

        public NormalButton BackButton { get { return m_backButton; } set { m_backButton = value; } }

        public AchievementsMenu(ContentManager Content)
        {
            m_achievemenyRows = new List<NormalMenu>();
            m_rowYPositions = new List<Vector2>();
            m_names = new List<string>();

            AddAchievementShorthands();

            AddAchievements(Content);

            m_backButton = new NormalButton(Content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\SettingsMenu\\MenuButton1"), new Vector2(100, 960), Color.White, 1, 1, 1, 1, Content, null, "");
        }

        private void AddAchievementShorthands()
        {
            m_names.Add("\\L");
            m_names.Add("\\A1Kills");
            m_names.Add("\\A2Kills");
            m_names.Add("\\A3Kills");
            m_names.Add("\\C");
            m_names.Add("\\D");
            m_names.Add("\\E");
        }

        private void AddAchievements(ContentManager Content)
        {
            for (int i = 0; i < 6; i++)
            {
                m_rowYPositions.Add(new Vector2(250, 70 + (i * 120)));
                m_achievemenyRows.Add(new NormalMenu(Content, "Art\\GameArt\\MenuGUI\\AchievementsMenu" + m_names[i], true, false, 4, 250, m_rowYPositions[i]));
            }

            m_rowYPositions.Add(new Vector2(250, 70 + (6 * 120)));
            m_achievemenyRows.Add(new NormalMenu(Content, "Art\\GameArt\\MenuGUI\\AchievementsMenu" + m_names[6], true, false, 3, 250, m_rowYPositions[6]));
        }

        public void UpdateMenu(PlayerStats userData, InputManager input)
        {
            m_backButton.UpdateMe(input);

            for (int i = 0; i < m_achievemenyRows.Count; i++)
            {
                for (int k = 0; k < m_achievemenyRows[i].Buttons.Count; k++)
                {
                    m_achievemenyRows[i].Buttons[k].UpdateMe(input);

                    HandleLockedAchievements(userData, i, k);
                }
            }
        }

        private void HandleLockedAchievements(PlayerStats userData, int i, int k)
        {
            if (userData.Achievements[k + (i * 4)] >= 1)
            {
                m_achievemenyRows[i].Buttons[k].IsClickable = true;
            }
            else
            {
                m_achievemenyRows[i].Buttons[k].IsClickable = false;
            }
        }

        public void DrawMe(SpriteBatch sb, GameTime gt)
        {
            for (int i = 0; i < m_achievemenyRows.Count; i++)
            {
                m_achievemenyRows[i].DrawMe(sb, gt);
            }

            m_backButton.DrawMe(sb, gt);
        }
    }
}
