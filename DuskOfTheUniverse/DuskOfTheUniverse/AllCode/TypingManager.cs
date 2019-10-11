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
    class TypingManager
    {
        // This is the little line you normally see when typing on a computer
        private CenteredStaticGraphic m_typer;

        // This bool checks if the string has no characters
        public bool TextIsNothing { get; set; }

        // Checks if enter has been pressed
        public bool EnterPressed { get; set; }
        // Checks if back has been pressed
        public bool BackPressed { get; set; }

        /// <summary>
        /// Manager Constructor
        /// </summary>
        /// <param name="content"> Content Manager </param>
        public TypingManager(ContentManager content)
        {
            TextIsNothing = false;
            EnterPressed = false;
            BackPressed = false;

            m_typer = new CenteredStaticGraphic(content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\TyperHighlighter"), new Vector2(0, 0), Color.White, 1);
        }

        // Update the whole manager
        public string UpdateManager(InputManager input, string text, ContentManager content, List<MessagePopup> popups)
        {
            CheckTextLength(text);

            EnterPressed = false;
            BackPressed = false;

            text = CheckKeyPresses(input, text, content, popups);

            return text;
        }

        // Add characters to the string of text
        private string CheckKeyPresses(InputManager input, string text, ContentManager content, List<MessagePopup> popups)
        {
            foreach (Keys key in input.Keys.GetPressedKeys())
            {
                if (input.Oldkeys.IsKeyUp(key))
                {
                    text = KeyCases(text, content, popups, key);
                }
            }
            return text;
        }

        // This has lots of key press cases, so that invalid input is not entered
        private string KeyCases(string text, ContentManager content, List<MessagePopup> popups, Keys key)
        {
            switch (key)
            {
                case Keys.A:
                    text += key.ToString();
                    break;
                case Keys.B:
                    text += key.ToString();
                    break;
                case Keys.C:
                    text += key.ToString();
                    break;
                case Keys.D:
                    text += key.ToString();
                    break;
                case Keys.E:
                    text += key.ToString();
                    break;
                case Keys.F:
                    text += key.ToString();
                    break;
                case Keys.G:
                    text += key.ToString();
                    break;
                case Keys.H:
                    text += key.ToString();
                    break;
                case Keys.I:
                    text += key.ToString();
                    break;
                case Keys.J:
                    text += key.ToString();
                    break;
                case Keys.K:
                    text += key.ToString();
                    break;
                case Keys.L:
                    text += key.ToString();
                    break;
                case Keys.M:
                    text += key.ToString();
                    break;
                case Keys.N:
                    text += key.ToString();
                    break;
                case Keys.O:
                    text += key.ToString();
                    break;
                case Keys.P:
                    text += key.ToString();
                    break;
                case Keys.Q:
                    text += key.ToString();
                    break;
                case Keys.R:
                    text += key.ToString();
                    break;
                case Keys.S:
                    text += key.ToString();
                    break;
                case Keys.T:
                    text += key.ToString();
                    break;
                case Keys.U:
                    text += key.ToString();
                    break;
                case Keys.V:
                    text += key.ToString();
                    break;
                case Keys.W:
                    text += key.ToString();
                    break;
                case Keys.X:
                    text += key.ToString();
                    break;
                case Keys.Y:
                    text += key.ToString();
                    break;
                case Keys.Z:
                    text += key.ToString();
                    break;
                case Keys.D0:
                    text += "0";
                    break;
                case Keys.D1:
                    text += "1";
                    break;
                case Keys.D2:
                    text += "2";
                    break;
                case Keys.D3:
                    text += "3";
                    break;
                case Keys.D4:
                    text += "4";
                    break;
                case Keys.D5:
                    text += "5";
                    break;
                case Keys.D6:
                    text += "6";
                    break;
                case Keys.D7:
                    text += "7";
                    break;
                case Keys.D8:
                    text += "8";
                    break;
                case Keys.D9:
                    text += "9";
                    break;
                case Keys.Back:
                    BackPressed = true;

                    if (!TextIsNothing)
                    {
                        text = text.Remove(text.Length - 1, 1);
                    }
                    break;
                case Keys.Enter:
                    EnterPressed = true;
                    break;
                case Keys.Space:
                    text += " ";
                    break;
                default:
                    popups.Add(new MessagePopup(content.Load<Texture2D>("Art\\PlaceholderArt\\MenuGUI\\MessageBacking"), new Vector2(960, 540), Color.White, new Vector2(0, -0.3f), "Invalid input!", 0.3f, Game1.gameFontNorm));
                    break;
            }
            return text;
        }

        // Checks if the text has no characters
        private void CheckTextLength(string text)
        {
            if (text.Length <= 0)
                TextIsNothing = true;
            else
                TextIsNothing = false;
        }

        // Draw to vertical line you see when typing
        public void DrawTyper(SpriteBatch sb, string text, SpriteFont font, Vector2 stringPos, float size, bool isCentred)
        {
            m_typer.Scale = size / 96;

            if (isCentred)
                m_typer.Position = stringPos + new Vector2(font.MeasureString(text).X / 2 + 2, 8);
            else
                m_typer.Position = stringPos + new Vector2(font.MeasureString(text).X + 2, 8);

            m_typer.DrawMe(sb);
        }
    }
}
