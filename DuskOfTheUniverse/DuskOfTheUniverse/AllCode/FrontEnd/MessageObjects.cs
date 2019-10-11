using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class MessagePopup : MotionGraphic
    {
        // Times for which the message will fade in, remain and then fade out
        private float m_fadeInTime;
        private float m_fadeOutTime;
        private float m_solidTime;
        // Used to get the alpha adjustment amount
        private float m_timeVar;

        // Current alpha amount
        private int m_alpha;

        // This is the message to be displayed on screen
        private string m_message;

        // This is the centre of the message (much like spritebatch origin)
        private Vector2 m_textOrigin;

        private SpriteFont m_font;

        public MessagePopup(Texture2D txr, Vector2 position, Color tint, Vector2 startSpeed, string message, float fadeTime, SpriteFont font)
            : base(txr, position, tint, startSpeed, 0, 1)
        {
            m_tint.A = 0;
            m_alpha = 0;

            m_fadeInTime = fadeTime;
            m_fadeOutTime = fadeTime;
            m_solidTime = 3 * fadeTime;

            m_timeVar = fadeTime;

            m_message = message;

            m_font = font;

            m_textOrigin = new Vector2(Game1.gameFontNorm.MeasureString(m_message).X / 2, Game1.gameFontNorm.MeasureString(m_message).Y / 2);

            m_rect = new Rectangle((int)(m_position.X - m_textOrigin.X) - 10, (int)(m_position.Y - m_textOrigin.Y) - 2, (int)Game1.gameFontNorm.MeasureString(m_message).X + 10, (int)Game1.gameFontNorm.MeasureString(m_message).Y + 2);
        }

        public virtual void UpdateMe(GameTime gt, List<MessagePopup> popups)
        {
            m_textOrigin = new Vector2(Game1.gameFontNorm.MeasureString(m_message).X / 2, Game1.gameFontNorm.MeasureString(m_message).Y / 2);

            // Move the message
            m_position += 30 * m_velocity * (float)gt.ElapsedGameTime.TotalSeconds;
            m_rect.X = (int)(m_position.X);
            m_rect.Y = (int)(m_position.Y);
            m_rect.Width = (int)Game1.gameFontNorm.MeasureString(m_message).X + 10;
            m_rect.Height = (int)Game1.gameFontNorm.MeasureString(m_message).Y + 2;

            byte alphaAdjust = (byte)(255 / (float)gt.ElapsedGameTime.TotalSeconds);

            // Pass through each stage (fade the message in, keep it on screen and then fade it out)
            if (m_fadeInTime > 0)
            {
                m_fadeInTime -= (float)gt.ElapsedGameTime.TotalSeconds;

                m_alpha += (int)((255 * (float)gt.ElapsedGameTime.TotalSeconds) / m_timeVar);

                if (m_alpha > 255)
                    m_alpha = 255;
            }
            else if (m_solidTime > 0)
            {
                m_solidTime -= (float)gt.ElapsedGameTime.TotalSeconds;
                m_tint.A = 255;
            }
            else if (m_fadeOutTime > 0)
            {
                m_fadeOutTime -= (float)gt.ElapsedGameTime.TotalSeconds;

                m_alpha -= (int)((255 * (float)gt.ElapsedGameTime.TotalSeconds) / m_timeVar);

                if (m_alpha <= 0)
                    m_alpha = 0;
            }
            else
            {
                // When finished remove self
                popups.Remove(this);
            }

            m_tint.A = (byte)m_alpha;
        }

        public override void DrawMe(SpriteBatch sb)
        {
            sb.Draw(m_txr, m_rect, null, m_tint, 0, m_origin, SpriteEffects.None, 0);

            sb.DrawString(m_font, m_message, m_position, m_tint, 0, m_textOrigin, 1, SpriteEffects.None, 0);
        }
    }

    class ToolTip : MotionGraphic
    {
        // This is the message to be displayed
        private string m_message;

        // This is the text origin (Think spritebatch origin)
        private Vector2 m_textOrigin;

        // This is the text's tint
        private Color m_textTint;
        // This is the font to be used
        private SpriteFont m_font;

        public string Message { get { return m_message; } set { m_message = value; } }

        public ToolTip(Texture2D txr, SpriteFont font, Vector2 position, Color tint, Color textTint, Vector2 startSpeed, string message)
            : base(txr, position, tint, startSpeed, 0, 1)
        {
            m_message = message;
            m_textTint = textTint;
            m_font = font;

            m_textOrigin = new Vector2(m_font.MeasureString(m_message).X / 2, m_font.MeasureString(m_message).Y / 2);

            m_rect = new Rectangle((int)(m_position.X - m_textOrigin.X) - 10, (int)(m_position.Y - m_textOrigin.Y) - 2, (int)m_font.MeasureString(m_message).X + 10, (int)m_font.MeasureString(m_message).Y + 2);
        }

        public virtual void UpdateMe(Vector2 position)
        {
            m_textOrigin = new Vector2(m_font.MeasureString(m_message).X / 2, m_font.MeasureString(m_message).Y / 2);

            // Keep the tooltip centred on the position (mainly the mouse)
            Point pos = new Point((int)position.X, (int)position.Y);

            m_position = new Vector2(pos.X, pos.Y);
            m_rect.X = pos.X;
            m_rect.Y = pos.Y;
            m_rect.Width = (int)m_font.MeasureString(m_message).X + 10;
            m_rect.Height = (int)m_font.MeasureString(m_message).Y + 2;
        }

        public override void DrawMe(SpriteBatch sb)
        {
            sb.Draw(m_txr, m_rect, null, m_tint, 0, m_origin, SpriteEffects.None, 0);

            sb.DrawString(m_font, m_message, m_position, m_textTint, 0, m_textOrigin, 1, SpriteEffects.None, 0);
        }
    }
}
