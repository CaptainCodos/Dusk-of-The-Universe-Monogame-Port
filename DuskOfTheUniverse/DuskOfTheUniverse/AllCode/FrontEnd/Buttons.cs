using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class NormalButton : AnimGraphic
    {
        // Is the button pressed?
        protected bool m_isPressed;

        protected bool m_isClickable;

        // Where is the mouse?
        protected Vector2 m_mousePos;

        protected ToolTip m_toolTip;
        protected bool m_drawToolTip;

        protected string m_text;

        protected Effect m_tempShader;
        protected List<Effect> m_shaders;

        public bool IsPressed
        {
            get { return m_isPressed; }
            set { m_isPressed = value; }
        }

        public bool IsClickable 
        { 
            get { return m_isClickable; } 
            set { m_isClickable = value; } 
        }

        public string Text
        {
            get { return m_text; }
            set { m_text = value; }
        }

        public ToolTip Ttip { get { return m_toolTip; } set { m_toolTip = value; } }

        public NormalButton(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, ToolTip tooltip, string text) 
            : base(txr, position, tint, Vector2.Zero, 0, scale, fps, framesX, framesY)
        {
            m_isPressed = false;
            m_isClickable = true;
            m_shaders = new List<Effect>();
            m_shaders.Add(content.Load<Effect>("Shaders\\HighlightEffect"));
            m_shaders.Add(content.Load<Effect>("Shaders\\LockedEffect"));

            m_rect.Width = (int)(m_txr.Width / framesX * scale);
            m_rect.Height = (int)(m_txr.Height / framesY * scale);

            m_toolTip = tooltip;
            m_drawToolTip = false;

            m_text = text;
        }

        public void UpdateMe(InputManager input)
        {
            m_mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);

            // Position the rectangle to the corner of the button.
            m_rect.X = (int)m_position.X - (int)(m_origin.X * m_scale);
            m_rect.Y = (int)m_position.Y - (int)(m_origin.Y * m_scale);

            // Checks if the button is clickable
            if (m_isClickable)
            {
                // If the button is clickable and it contains the mouse position, then highlight it
                if (m_rect.Contains((int)m_mousePos.X, (int)m_mousePos.Y))
                {
                    m_drawToolTip = true;

                    // if the button has a tooltip then update it
                    if (m_toolTip != null)
                        m_toolTip.UpdateMe(m_mousePos);

                    // If left mouse button is clicked, set is pressed to true
                    if (input.LeftPressed)
                        m_isPressed = true;
                    else
                        m_isPressed = false;

                    m_tempShader = m_shaders[0];
                }
                else
                {
                    m_tempShader = null;
                    m_isPressed = false;
                    m_drawToolTip = false;
                }
            }
            else
            {
                m_tempShader = m_shaders[1];
                m_isPressed = false;
                m_drawToolTip = false;
            }
        }

        public void UpdateMeCam(InputManager input, Camera cam)
        {
            m_mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);
            m_mousePos = Vector2.Transform(m_mousePos, Matrix.Invert(cam.Transform));
            m_mousePos = Vector2.Clamp(m_mousePos, Vector2.Zero, new Vector2(2875, 1615));

            // Position the rectangle to the corner of the button.
            m_rect.X = (int)m_position.X - (int)(m_origin.X * m_scale);
            m_rect.Y = (int)m_position.Y - (int)(m_origin.Y * m_scale);

            // Checks if the button is clickable
            if (m_isClickable)
            {
                // If the button is clickable and the button contains the mouse position, then highlight it
                if (m_rect.Contains((int)m_mousePos.X, (int)m_mousePos.Y))
                {
                    m_drawToolTip = true;

                    // If the button has a tooltip, update it
                    if (m_toolTip != null)
                        m_toolTip.UpdateMe(m_mousePos);

                    // If the left mouse button is pressed, set is pressed to true
                    if (input.LeftPressed)
                        m_isPressed = true;
                    else
                        m_isPressed = false;

                    m_tempShader = m_shaders[0];
                }
                else
                {
                    m_tempShader = null;
                    m_isPressed = false;
                    m_drawToolTip = false;
                }
            }
            else
            {
                m_tempShader = m_shaders[1];
                m_isPressed = false;
                m_drawToolTip = false;
            }
        }

        // Draw button
        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, m_tempShader);
            base.DrawMe(sb, gt);
            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null);
            // If the button has a tooltip, displlay it
            if (m_toolTip != null && m_drawToolTip)
                m_toolTip.DrawMe(sb);
            if (m_text.Length > 0)
                sb.DrawString(Game1.debugFont, m_text, m_position, Color.White, 0, Game1.debugFont.MeasureString(m_text) / 2, 1, SpriteEffects.None, 0);
            sb.End();
        }

        // Draw button
        public virtual void DrawMeCam(SpriteBatch sb, GameTime gt, Camera cam)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, m_tempShader, cam.Transform);
            base.DrawMe(sb, gt);
            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.Transform);
            // If the button has a tooltip, display it
            if (m_toolTip != null && m_drawToolTip)
                m_toolTip.DrawMe(sb);
            if (m_text.Length > 0)
                sb.DrawString(Game1.debugFont, m_text, m_position, Color.White, 0, Game1.debugFont.MeasureString(m_text) / 2, 1, SpriteEffects.None, 0);
            sb.End();
        }
    }

    class TowerButton : NormalButton
    {
        // This is the tower for display
        private Tower m_tower;

        // This is the current index of the button
        private int m_index;

        public Tower Tower { get { return m_tower; } set { m_tower = value; } }

        public TowerButton(Texture2D txr, Vector2 position, Color tint, float scale, int fps, int framesX, int framesY, ContentManager content, ToolTip tooltip, int index, string text) 
            : base(txr, position, tint, scale, fps, framesX, framesY, content, tooltip, text)
        {
            m_index = index;
            m_isClickable = false;
        }

        public void UpdateMe(InputManager input, GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content)
        {
            m_mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);

            // Position the rectangle to the corner of the button.
            m_rect.X = (int)m_position.X - (int)(m_origin.X * m_scale);
            m_rect.Y = (int)m_position.Y - (int)(m_origin.Y * m_scale);

            // Checks if the button is clickable
            if (m_isClickable)
            {
                // If the button is clickable and it contains the mouse position, highlight it
                if (m_rect.Contains((int)m_mousePos.X, (int)m_mousePos.Y))
                {
                    m_drawToolTip = true;

                    // If the button has a tooltip, update it
                    if (m_toolTip != null)
                        m_toolTip.UpdateMe(m_mousePos);

                    // If the left mouse button is pressed, set is pressed to true
                    if (input.LeftPressed)
                        m_isPressed = true;
                    else
                        m_isPressed = false;

                    m_tempShader = m_shaders[0];
                }
                else
                {
                    m_tempShader = null;
                    m_isPressed = false;
                    m_drawToolTip = false;
                }
            }
            else
            {
                m_tempShader = null;
                m_isPressed = false;
                m_drawToolTip = false;
            }

            if (m_tower != null)
            {
                m_tower.TowerPos = m_position;
                m_tower.UpdatePosition();

            }
        }

        public override void DrawMe(SpriteBatch sb, GameTime gt)
        {
            base.DrawMe(sb, gt);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, m_tempShader);
            m_tower.DrawTower(sb, gt);
            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null);
            // If the button has a tooltip, display it
            if (m_toolTip != null && m_drawToolTip)
                m_toolTip.DrawMe(sb);
            sb.End();
        }
    }
}
