using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class Cutscene
    {
        private List<MotionGraphic> m_backgroundImageList;
        private int m_currentImage;
        private float m_fadeTime;
        private float m_currentAlpha;
        private Color m_imageColour;

        private List<MessagePopup> m_popupList;
        private int m_currentPopup;

        public bool CutsceneEnded { get; set; }

        public Cutscene(ContentManager content, List<MotionGraphic> backgroundList, List<string> messages)
        {
            m_backgroundImageList = backgroundList;
            m_currentImage = 0;
            m_currentAlpha = 2;
            m_fadeTime = 5;
            m_imageColour = new Color(255, 255, 255, 2);

            m_popupList = new List<MessagePopup>();
            m_currentPopup = 0;

            CutsceneEnded = false;

            for (int i = 0; i < messages.Count; i++)
            {
                m_popupList.Add(new MessagePopup(content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\MessageBacking"), new Vector2(960, 800), Color.White, Vector2.Zero, messages[i], 1f, Game1.cutsceneFont));
            }
        }

        public void UpdateCutScene(GameTime gt)
        {
            if (m_backgroundImageList.Count > 0)
            {
                m_backgroundImageList[m_currentImage].UpdateMe(gt);

                if (m_backgroundImageList[m_currentImage].Position.Y < 400 && m_fadeTime > 0)
                {
                    m_fadeTime -= (float)gt.ElapsedGameTime.TotalSeconds;
                    m_currentAlpha += (255 * (float)gt.ElapsedGameTime.TotalSeconds) / 5;

                    if (m_fadeTime <= 0)
                    {
                        m_fadeTime = 5;
                    }
                }
                else if (m_backgroundImageList[m_currentImage].Position.Y > 600 && m_fadeTime > 0)
                {
                    m_fadeTime -= (float)gt.ElapsedGameTime.TotalSeconds;
                    m_currentAlpha -= (255 * (float)gt.ElapsedGameTime.TotalSeconds) / 5;

                    if (m_fadeTime <= 0)
                    {
                        m_fadeTime = 5;
                    }
                }

                if (m_currentAlpha > 255)
                    m_currentAlpha = 255;
                if (m_currentAlpha < 0)
                    m_currentAlpha = 0;

                m_imageColour.A = (byte)m_currentAlpha;
                m_backgroundImageList[m_currentImage].Tint = m_imageColour;

                if (m_backgroundImageList[m_currentImage].Tint.A <= 0)
                {
                    m_backgroundImageList.RemoveAt(m_currentImage);
                    m_currentAlpha = 2;
                }
            }
            else
            {
                CutsceneEnded = true;
            }

            if (m_popupList.Count > 0)
            {
                m_popupList[m_currentPopup].UpdateMe(gt, m_popupList);
            }
        }

        public void DrawCutScene(SpriteBatch sb)
        {
            if (m_backgroundImageList.Count > 0)
            {
                m_backgroundImageList[m_currentImage].DrawMe(sb);
            }

            if (m_popupList.Count > 0)
            {
                m_popupList[m_currentPopup].DrawMe(sb);
            }
        }
    }
}
