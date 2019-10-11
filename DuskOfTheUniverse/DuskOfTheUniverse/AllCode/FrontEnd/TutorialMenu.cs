using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class TutorialMenu
    {
        // The current page
        private int m_currPage;
        // the minimum page
        private const int m_minPage = 0;
        // THe maximum page
        private const int m_maxPage = 3;

        // flag for tutorial end
        private bool m_tutorialEnded;

        // Graphics for display
        private List<StaticGraphic> m_pages;

        public bool TutorialEnded { get { return m_tutorialEnded; } }
        public int CurrentPage { get { return m_currPage; } set { m_currPage = value; } }

        public TutorialMenu(ContentManager content)
        {
            m_pages = new List<StaticGraphic>();
            m_currPage = 0;
            m_tutorialEnded = false;

            for (int i = 0; i < 4; i++)
            {
                m_pages.Add(new StaticGraphic(content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\TutorialMenu\\TutoPage" + i), Vector2.Zero, Color.White));
            }
        }

        public void UpdateMenu(InputManager input)
        {
            // Increments page on click
            if (input.LeftPressed)
                m_currPage++;

            // Checks if the tutorial has ended
            if (m_currPage > m_maxPage)
                m_tutorialEnded = true;
            else
                m_tutorialEnded = false;
        }

        public void DrawMenu(SpriteBatch sb)
        {
            // display current page
            sb.Begin();
            m_pages[m_currPage].DrawMe(sb);
            sb.End();
        }
    }
}
