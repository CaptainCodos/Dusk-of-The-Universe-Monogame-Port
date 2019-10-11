using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class CharHealthBar
    {
        // temporary shader
        private Effect m_tmpShader;
        // Other effects
        private Effect[] m_shaders;
        // Highlight shader for
        private Effect m_healthStatusShader;

        // Border of the health bar
        private CenteredStaticGraphic m_border;
        // The health bar itself
        private AnimGraphic m_healthBar;
        // Background of the health bar
        private CenteredStaticGraphic m_healthBacking;
        // Path of the texture
        private string m_path;

        // Centre of the health bar
        private Vector2 m_barCentre;
        // Offset of the health bar
        private Vector2 m_offset;

        // Max Health
        private int m_maxHP;
        // Current Health
        private int m_currHP;

        // Owning character of the health bar
        private GameChar m_parentChar;
        // Owning tower of the health bar
        private Tower m_parentTower;

        public Effect[] Shaders { get { return m_shaders; } }
        public Effect CurrShader { get { return m_tmpShader; } set { m_tmpShader = value; } }

        public CharHealthBar(string path)
        {
            m_path = path;
        }
        public CharHealthBar SetPosition(Vector2 pos)
        {
            m_barCentre = pos;
            m_offset = new Vector2(0, 20);
            return this;
        } // Set bar position
        public CharHealthBar LoadTextures(ContentManager content, string borderName, string barName, string backingName, float scale)
        {
            m_border = new CenteredStaticGraphic(content.Load<Texture2D>(m_path + borderName), m_barCentre, Color.White, scale);
            m_healthBar = new AnimGraphic(content.Load<Texture2D>(m_path + barName), m_barCentre, Color.White, Vector2.Zero, 0, scale, 1, 1, 1);
            m_healthBacking = new CenteredStaticGraphic(content.Load<Texture2D>(m_path + backingName), m_barCentre, Color.White, scale);
            m_healthBacking.Tint = Color.Red;
            return this;
        } // Load bar textures
        public CharHealthBar LoadShader(ContentManager content, string pathAndName)
        {
            m_shaders = new Effect[2];
            m_shaders[0] = null;
            m_shaders[1] = content.Load<Effect>(pathAndName);
            m_tmpShader = m_shaders[0];
            m_healthStatusShader = content.Load<Effect>("Shaders\\HealthBarStatus");
            return this;
        } // Load shaders
        public CharHealthBar SetHpValues(int hp)
        {
            m_maxHP = hp;
            m_currHP = hp;
            return this;
        } // Enter HP value
        public CharHealthBar SetColours(Color borderColour, Color barColour, Color backingColour)
        {
            m_border.Tint = borderColour;
            m_healthBar.Tint = barColour;
            m_healthBacking.Tint = backingColour;
            return this;
        } // Set Colours
        public CharHealthBar SetParentOfBar(GameChar parent)
        {
            m_parentChar = parent;
            return this;
        } // Set Character Parent
        public CharHealthBar SetParentOfBar(Tower parent)
        {
            m_parentTower = parent;
            return this;
        } // Set Tower Parent
        public CharHealthBar SetScaleBorder(float scale)
        {
            m_border.Scale = scale;
            return this;
        } // Set Border Scale
        public CharHealthBar SetScaleBar(float scale)
        {
            m_healthBar.Scale = scale;
            return this;
        } // Set Bar Scale
        public CharHealthBar SetScaleBacking(float scale)
        {
            m_healthBacking.Scale = scale;
            return this;
        } // Set Background Scale

        // Update Static Healthbar (Used in game GUI)
        public void UpdateBar()
        {
            if (m_parentChar != null)
            {
                m_healthBar.SourceRectX = 2;
                m_healthBar.SourceRectY = 2;
                m_healthBar.SourceRectWidth = (int)((m_healthBar.Txr.Width - 4) * (m_parentChar.Health / m_parentChar.MaxHealth));
            }
            else if (m_parentTower != null)
            {
                m_healthBar.SourceRectX = 2;
                m_healthBar.SourceRectY = 2;
                m_healthBar.SourceRectWidth = (int)((m_healthBar.Txr.Width - 4) * (m_parentTower.Health / m_parentTower.MaxHP));
            }
        }

        // Update Positioned Health Bar (Used for towers and enemy characters)
        public void UpdateBar(Vector2 pos)
        {
            m_barCentre = pos + m_offset;
            m_border.Position = m_barCentre;
            m_healthBar.Position = m_barCentre;
            m_healthBar.Position += new Vector2(0, -0.5f);
            m_healthBacking.Position = m_barCentre;

            if (m_parentChar != null)
            {
                m_healthBar.SourceRectX = 2;
                m_healthBar.SourceRectY = 2;
                m_healthBar.SourceRectWidth = (int)((m_healthBar.Txr.Width - 4) * (m_parentChar.Health / m_parentChar.MaxHealth));
            }
            else if (m_parentTower != null)
            {
                m_healthBar.SourceRectX = 2;
                m_healthBar.SourceRectY = 2;
                m_healthBar.SourceRectWidth = (int)((m_healthBar.Txr.Width - 4) * (m_parentTower.Health / m_parentTower.MaxHP));
            }

            if (m_parentChar != null && m_parentChar.IsShaded)
            {
                m_healthBar.Tint = new Color(80, 80, 0);
                m_healthBacking.Tint = new Color(80, 0, 0);
            }
            else
            {
                m_healthBar.Tint = Color.Yellow;
                m_healthBacking.Tint = Color.Red;
            }
        }

        // Used for ally health bars (game GUI)
        private void NewDrawMeHealthBar(SpriteBatch sb)
        {
            sb.Draw(m_healthBar.Txr, m_healthBar.Position, m_healthBar.SourceRectangle, m_healthBar.Tint, 0, m_healthBar.Origin, 1, SpriteEffects.None, 0);
        }

        // Draw Info (game GUI)
        private void DrawText(SpriteBatch sb)
        {
            sb.DrawString(Game1.gameFontNorm, "" + m_parentChar.Health + " / " + m_parentChar.BaseMaxHealth + "( + " + (int)(m_parentChar.MaxHealth - m_parentChar.BaseMaxHealth) + ")", m_barCentre + new Vector2(0, -3), Color.Black, 0, Game1.gameFontNorm.MeasureString("" + m_parentChar.Health + " / " + m_parentChar.BaseMaxHealth + "( + " + (m_parentChar.MaxHealth - m_parentChar.BaseMaxHealth) + ")") / 2, 0.9f, SpriteEffects.None, 0);
        }

        // Draw Bar (game GUI)
        public void DrawMe(SpriteBatch sb)
        {
            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null);
            m_healthBacking.DrawMe(sb);
            sb.End();

            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, m_healthStatusShader);
            SetStatusShaderParams(m_healthStatusShader, 1, 1, 0, 1, 0, 1, 1, 0.2f, (m_parentChar.BaseMaxHealth / m_parentChar.MaxHealth));
            NewDrawMeHealthBar(sb);
            sb.End();

            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, m_tmpShader);
            SetShaderParamenters(m_tmpShader, 1, 1, 1);
            m_border.DrawMe(sb);
            sb.End();

            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null);
            DrawText(sb);
            sb.End();
        }

        // Draw Bar (enemy characters and towers)
        public void DrawMe(SpriteBatch sb, Camera cam)
        {
            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, cam.Transform);
            m_healthBacking.DrawMe(sb);
            NewDrawMeHealthBar(sb);
            sb.End();

            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, m_tmpShader, cam.Transform);
            SetShaderParamenters(m_tmpShader, 0, 0, 0);
            m_border.DrawMe(sb);
            sb.End();
        }

        // Set shader paramenters (enemy characters and towers)
        private void SetShaderParamenters(Effect effect, float r, float g, float b)
        {
            if (effect != null)
            {
                effect.Parameters["r"].SetValue(r);
                effect.Parameters["g"].SetValue(g);
                effect.Parameters["b"].SetValue(b);
            }
        }

        // Set shader parameters (game GUI)
        // This shader allows for the display of over heal (character has more health than their base max health)
        private void SetStatusShaderParams(Effect effect, float r1, float g1, float b1, float a1, float r2, float g2, float b2, float a2, float ratio)
        {
            if (effect != null)
            {
                effect.Parameters["r1"].SetValue(r1);
                effect.Parameters["g1"].SetValue(g1);
                effect.Parameters["b1"].SetValue(b1);
                effect.Parameters["a1"].SetValue(a1);

                effect.Parameters["r2"].SetValue(r2);
                effect.Parameters["g2"].SetValue(g2);
                effect.Parameters["b2"].SetValue(b2);
                effect.Parameters["a2"].SetValue(a2);

                effect.Parameters["ratio"].SetValue(ratio);
            }
        }
    }
}
