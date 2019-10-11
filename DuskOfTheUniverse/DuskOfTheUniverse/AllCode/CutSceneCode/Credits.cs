using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class Credits
    {
        private StaticGraphic m_background;

        private Vector2 m_position;
        private List<Vector2> m_centreOffsets;
        private List<string> m_messages;

        private Rectangle m_rect;

        private SpriteFont m_font;

        private bool m_creditsEnded;

        public bool CreditsEnded { get { return m_creditsEnded; } set { m_creditsEnded = value; } }

        public Credits(SpriteFont font, StaticGraphic background, List<Vector2> vectorOffsets, List<string> messages)
        {
            m_background = background;
            m_font = font;

            m_centreOffsets = vectorOffsets;
            m_messages = messages;

            float minY = 99999;
            float maxY = -9999;

            for (int i = 0; i < m_centreOffsets.Count; i++)
            {
                if (m_centreOffsets[i].Y < minY)
                    minY = m_centreOffsets[i].Y;

                if (m_centreOffsets[i].Y > maxY)
                    maxY = m_centreOffsets[i].Y;
            }

            int height = (int)(maxY - minY);

            m_rect = new Rectangle(0, 1080, 1920, height + 100);

            m_position = new Vector2(960, 1700);

            m_creditsEnded = false;
        }

        public void UpdateCredits(GameTime gt)
        {
            m_position.Y -= 50 * (float)gt.ElapsedGameTime.TotalSeconds;

            m_rect.Y = (int)m_position.Y - (m_rect.Width / 2);

            if (FinalPosition(m_centreOffsets.Count - 1).Y < -20)
            {
                m_creditsEnded = true;
            }
        }

        public void DrawCredits(SpriteBatch sb)
        {
            sb.Begin();

            m_background.DrawMe(sb);

            for (int i = 0; i < m_messages.Count; i++)
            {
                sb.DrawString(m_font, m_messages[i], FinalPosition(i), Color.White);
            }

            sb.End();
        }

        private Vector2 FinalPosition(int i)
        {
            return (m_position + m_centreOffsets[i]);
        }
    }
}
