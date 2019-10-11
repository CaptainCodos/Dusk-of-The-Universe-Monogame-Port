using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class SettingsMenu
    {
        // The normal menu is used for regular button functions, the freeform menu is for volume control
        private NormalMenu m_menu;
        private FreeFormMenu m_volumeMenu;
        private CenteredStaticGraphic m_backing;

        // A boolean to recognize if the player has come from the pause menu
        private bool m_fromGameplay;

        public FreeFormMenu VolumeMenu { get { return m_volumeMenu; } }
        public NormalMenu Menu { get { return m_menu; } }

        public bool FromGamePlay { get { return m_fromGameplay; } set { m_fromGameplay = value; } }

        public SettingsMenu(ContentManager content)
        {
            m_menu = new NormalMenu(content, "Art\\GameArt\\MenuGUI\\SettingsMenu\\MenuButton", false, false, 2, 200, new Vector2(960, 300));
            m_volumeMenu = new FreeFormMenu(content, "Art\\GameArt\\MenuGUI\\SettingsMenu\\VolumeButton", false, new Vector2[] { new Vector2(816, 400), new Vector2(1104, 400) });

            m_backing = new CenteredStaticGraphic(content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\SettingsMenu\\VolumePercentBacking"), new Vector2(960, 400), Color.White, 1);
        }

        public void UpdateMenu(InputManager input, PlayerStats userData, MusicManager musicManager)
        {
            m_menu.UpdateMe(input);
            m_volumeMenu.UpdateMe(input, userData);

            // Adjust volume control
            if (m_volumeMenu.Buttons[0].IsPressed && musicManager.VolumeMult > 0)
                musicManager.VolumeMult -= 0.1f;
            if (m_volumeMenu.Buttons[1].IsPressed && musicManager.VolumeMult < 1)
                musicManager.VolumeMult += 0.1f;
        }

        public void DrawMenu(SpriteBatch sb, GameTime gt, MusicManager musicManager)
        {
            sb.Begin();
            m_backing.DrawMe(sb);
            sb.DrawString(Game1.gameFontNorm, "Volume", new Vector2(960, 400), Color.White, 0, Game1.gameFontNorm.MeasureString("Volume") / 2, 1, SpriteEffects.None, 0);
            sb.DrawString(Game1.gameFontNorm, "" + (int)(musicManager.VolumeMult * 100), new Vector2(960, 420), Color.White, 0, Game1.gameFontNorm.MeasureString("" + (int)(musicManager.VolumeMult * 100)) / 2, 1, SpriteEffects.None, 0);
            sb.End();

            m_menu.DrawMe(sb, gt);
            m_volumeMenu.DrawMe(sb, gt);
        }
    }
}
