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
    class NormalMenu
    {
        // List of buttons within the menu.
        private List<NormalButton> m_buttons;

        // If true the menu is horizontal, otherwise it is a regular vertical menu.
        private bool m_isHorizontal;

        // Determines the number of buttons in the menu.
        private int m_menuSize;

        // This value represents the distance between buttons.
        private int m_spacing;

        public List<NormalButton> Buttons
        {
            get { return m_buttons; }
            set { m_buttons = value; }
        }

        /// <summary>
        /// Creates a regular menu.
        /// </summary>
        /// <param name="content"> Gives the constructor the game content manager. </param>
        /// <param name="pathAndName"> Tells the constructor where to get the textures from. Give the path and the name without the index if the buttons do not look alike. The constructor will get the index of the texture if the buttons are unique. If not, then enter the name and index of the texture. </param>
        /// <param name="isHorizontalMenu"> Tells the constructor whether or not the menu is a sideways menu or a normal one. </param>
        /// <param name="menuSize"> Determines the size of the menu. </param>
        /// <param name="spacing"> Determines the space between buttons. </param>
        /// <param name="menuBegin"> Where the first button is placed on the screen. </param>
        public NormalMenu(ContentManager content, string pathAndName, bool isHorizontalMenu, bool buttonsAreTheSame, int menuSize, int spacing, Vector2 menuBegin)
        {
            // Set up the variables given in the constructor.
            m_buttons = new List<NormalButton>();

            m_isHorizontal = isHorizontalMenu;
            m_menuSize = menuSize;
            m_spacing = spacing;

            // If the buttons are the same, construct the menu as such.
            if (buttonsAreTheSame)
            {
                // If the menu is horizontal then create the buttons with the given texture, and position them with respect to their width and spacing between.
                if (m_isHorizontal)
                {
                    for (int i = 0; i < menuSize; i++)
                    {
                        m_buttons.Add(new NormalButton(content.Load<Texture2D>(pathAndName), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, null, ""));
                        m_buttons[i].Position = new Vector2(menuBegin.X + (i * (m_buttons[i].Txr.Width + spacing)), menuBegin.Y);
                    }
                }
                else // Otherwise do the same in a vertical direction.
                {
                    for (int i = 0; i < menuSize; i++)
                    {
                        m_buttons.Add(new NormalButton(content.Load<Texture2D>(pathAndName), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, null, ""));
                        m_buttons[i].Position = new Vector2(menuBegin.X, menuBegin.Y + (i * (m_buttons[i].Txr.Height + spacing)));
                    }
                }
            }
            else // If the buttons are all unique.
            {
                // If the menu is horizontal then create the buttons with the given texture path (index will be added in the constructor), and position them with respect to their width and spacing between.
                if (m_isHorizontal)
                {
                    for (int i = 0; i < menuSize; i++)
                    {
                        m_buttons.Add(new NormalButton(content.Load<Texture2D>(pathAndName + i), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, null, ""));
                        m_buttons[i].Position = new Vector2(menuBegin.X + (i * (m_buttons[i].Txr.Width + spacing)), menuBegin.Y);
                    }
                }
                else // Otherwise do the same in a vertical direction.
                {
                    for (int i = 0; i < menuSize; i++)
                    {
                        m_buttons.Add(new NormalButton(content.Load<Texture2D>(pathAndName + i), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, null, ""));
                        m_buttons[i].Position = new Vector2(menuBegin.X, menuBegin.Y + (i * (m_buttons[i].Txr.Height + spacing)));
                    }
                }
            }
        }

        public void UpdateMe(InputManager input)
        {
            for (int i = 0; i < m_buttons.Count; i++)
            {
                m_buttons[i].UpdateMe(input);
            }
        }

        public void DrawMe(SpriteBatch sb, GameTime gt)
        {
            for (int i = 0; i < m_buttons.Count; i++)
            {
                m_buttons[i].DrawMe(sb, gt);
            }
        }
    }

    class FreeFormMenu
    {
        // List of buttons within the menu.
        private List<NormalButton> m_buttons;

        // Array of vectors to be used for button positions.
        private Vector2[] m_buttonPos;

        public List<NormalButton> Buttons
        {
            get { return m_buttons; }
            set { m_buttons = value; }
        }

        /// <summary>
        /// Creates a menu in which the buttons can be in any place desired.
        /// </summary>
        /// <param name="content"> Content manager for the game is required for the textures</param>
        /// <param name="pathAndName"> Tells the constructor where to get the textures from. Give the path and the name without the index if the buttons do not look alike. The constructor will get the index of the texture if the buttons are unique. If not, then enter the name and index of the texture. </param>
        /// <param name="buttonsAreTheSame"> Are the buttons identical or unique. </param>
        /// <param name="positions"> Give the menu and array of positions to work from. </param>
        public FreeFormMenu(ContentManager content, string pathAndName, bool buttonsAreTheSame, Vector2[] positions)
        {
            // Set up the variables given in the constructor.
            m_buttons = new List<NormalButton>();
            m_buttonPos = positions;

            // If buttons are all the same, then create the menu with buttons looking the same.
            if (buttonsAreTheSame)
            {
                for (int i = 0; i < m_buttonPos.Length; i++)
                {
                    m_buttons.Add(new NormalButton(content.Load<Texture2D>(pathAndName), m_buttonPos[i], Color.White, 1, 1, 1, 1, content, null, ""));
                }
            }
            else // Otherwise create the menu with buttons according to their index.
            {
                for (int i = 0; i < m_buttonPos.Length; i++)
                {
                    m_buttons.Add(new NormalButton(content.Load<Texture2D>(pathAndName + i), m_buttonPos[i], Color.White, 1, 1, 1, 1, content, null, ""));
                }
            }
        }

        public void UpdateMe(InputManager input, PlayerStats userData)
        {
            for (int i = 0; i < m_buttons.Count; i++)
            {
                m_buttons[i].UpdateMe(input);
            }
        }

        public void DrawMe(SpriteBatch sb, GameTime gt)
        {
            for (int i = 0; i < m_buttons.Count; i++)
            {
                m_buttons[i].DrawMe(sb, gt);
            }
        }
    }

    class GameplayMenu
    {
        // Set up a free form menu.
        private FreeFormMenu m_menu;

        // This is the background image for the game menu (menu bar)
        private CenteredStaticGraphic m_menuBackground;

        // This is the border that will surround the tower buttons
        private CenteredStaticGraphic m_selectionBorder;
        // This is the current page
        private int m_page;
        // This is the maximum number of pages
        private int m_maxPages;
        // This is the number of 
        private int m_numberOfTowers;

        // This is the minimum index button to be displayed
        private int m_minIndex;
        // This is the maximum index to be displayed
        private int m_maxIndex;

        // This is the list of tower buttons to be selected from
        private List<TowerButton> m_towerOptions;

        // These are variable concerning the pawn bar, to show the player when the next wave is imminent
        private Texture2D m_spawnBarTex;
        private Vector2 m_spawnBarPos;
        private Rectangle m_spawnBarSrc;

        public FreeFormMenu Menu
        {
            get { return m_menu; }
            set { m_menu = value; }
        }

        public List<TowerButton> TowerOptions
        {
            get { return m_towerOptions; }
            set { m_towerOptions = value; }
        }

        public GameplayMenu(ContentManager content)
        {
            m_page = 0;
            m_maxPages = 0;
            m_numberOfTowers = 0;

            // A page consists of 5 tower buttons
            m_minIndex = m_page * 5;
            m_maxIndex = m_minIndex + 5;

            m_towerOptions = new List<TowerButton>();

            // Create the wave bar
            m_spawnBarTex = content.Load<Texture2D>("Art\\PlaceholderArt\\GameplayGUI\\WaveBar");
            m_spawnBarPos = new Vector2(828, 874);
            m_spawnBarSrc = new Rectangle(0, 0, m_spawnBarTex.Width, m_spawnBarTex.Height);

            // Set up the main tower menu.
            m_menu = new FreeFormMenu(content, "Art\\GameArt\\GameplayGUI\\TowerBar\\GameplayButton", false, new Vector2[] { new Vector2(72, 1044), new Vector2(864, 936), new Vector2(180, 936)});
            m_menu.Buttons[0].Ttip = new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Construct a new tower.");
            m_menu.Buttons[1].Ttip = new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Next Page");
            m_menu.Buttons[2].Ttip = new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Previous Page");
            m_selectionBorder = new CenteredStaticGraphic(content.Load<Texture2D>("Art\\PlaceholderArt\\GameplayGUI\\TowersBorder"), new Vector2(522, 1008), Color.White, 1);
            m_menuBackground = new CenteredStaticGraphic(content.Load<Texture2D>("Art\\PlaceholderArt\\GameplayGUI\\MenuBackground"), new Vector2(960, 976), Color.White, 1);
        }

        public void UpdateMe(InputManager input, List<Tower> savedTowers, ContentManager content, GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, PlayerStats userData, EnemySpawnManager enemySpawnManager)
        {
            // Update the freeform menu component
            m_menu.UpdateMe(input, userData);

            // Modify the width of the source rectangle according to the time left before spawning
            m_spawnBarSrc.Width = (int)(m_spawnBarTex.Width * (enemySpawnManager.CurrTime / enemySpawnManager.WaveTime));

            // Recreate new set of buttons if the number of saved towers have changed
            if (savedTowers.Count != m_numberOfTowers)
            {
                m_towerOptions = new List<TowerButton>();
                m_numberOfTowers = savedTowers.Count;

                for (int i = 0; i < savedTowers.Count; i++)
                {
                    TowerButton button = new TowerButton(content.Load<Texture2D>("Art\\PlaceholderArt\\GameplayGUI\\TowerHighlight"), new Vector2(180 + (72 * (i - m_minIndex)), 1044), Color.White, 2, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "" + savedTowers[i].TowerName + "\nHealth: " + savedTowers[i].Health + "\nCost: " + savedTowers[i].TowerCost), i, "");
                    button.Tower = new Tower(content, savedTowers[i], new Vector2(216 + (144 * (i - m_minIndex)), 1008));
                    button.Tower.UpdateAllProps();
                    button.Tower.UpdatePosition();
                    m_towerOptions.Add(button);
                }
            }

            // Page switching logic
            if (m_menu.Buttons[1].IsPressed)
                m_page++;
            if (m_menu.Buttons[2].IsPressed)
                m_page--;

            // Since each page contains 5 tower buttons
            m_maxPages = m_towerOptions.Count / 5;

            // Lock the number of pages
            LockPages();

            // Set the lowest display index and max display index
            m_minIndex = m_page * 5;
            m_maxIndex = m_minIndex + 5;

            for (int i = 0; i < m_towerOptions.Count; i++)
            {
                m_towerOptions[i].Position = new Vector2(216 + (144 * (i - m_minIndex)), 1008);
                m_towerOptions[i].UpdateMe(input, gt, enemies, projectiles, content);

                // Make sure there is a tower to display
                if (m_towerOptions[i].Tower == null)
                {
                    m_towerOptions[i].Tower = new Tower(content, savedTowers[i], m_towerOptions[i].Position);
                }

                if (i >= m_minIndex && i < m_maxIndex)
                {
                    m_towerOptions[i].IsClickable = true;
                }
                else
                {
                    m_towerOptions[i].IsClickable = false;
                }
            }
        }

        private void LockPages()
        {
            if (m_page > m_maxPages)
                m_page = m_maxPages;
            if (m_page < 0)
                m_page = 0;
        }

        public void DrawShaded(SpriteBatch sb, GameTime gt, List<EnemyChar> enemies, List<BaseProjectile> projectiles, ContentManager content, EnemySpawnManager enemySpawnManager)
        {
            sb.Begin();
            m_menuBackground.DrawMe(sb);
            m_selectionBorder.DrawMe(sb);
            sb.DrawString(Game1.gameFontNorm, "Page: " + (m_page + 1) + "\nCount: " + m_towerOptions.Count, new Vector2(72, 972), Color.White, 0, Game1.gameFontNorm.MeasureString("Page: " + (m_page + 1) + "\nCount: " + m_towerOptions.Count) / 2, 1, SpriteEffects.None, 0);
            sb.End();

            m_menu.DrawMe(sb, gt);

            sb.Begin();
            sb.Draw(m_spawnBarTex, m_spawnBarPos, m_spawnBarSrc, Color.White);
            sb.DrawString(Game1.gameFontNorm, "Wave: " + enemySpawnManager.CurrentWave, new Vector2(1380, 875), Color.Black);
            sb.End();

            for (int i = 0; i < m_towerOptions.Count; i++)
            {
                if (i >= m_minIndex && i < m_maxIndex)
                {
                    m_towerOptions[i].DrawMe(sb, gt);
                    m_towerOptions[i].IsClickable = true;
#if DEBUG
                    sb.Begin();
                    sb.DrawString(Game1.debugFont, "Button: " + m_towerOptions[i].Tower.PartProps.Count, m_towerOptions[i].Position, Color.White);
                    sb.End();
#endif
                }
                else
                {
                    m_towerOptions[i].IsClickable = false;
                }
            }
        }
    }

    class DropMenu
    {
        // THis is the list of buttons within the drop menu
        private List<NormalButton> m_selectionButtons;
        // This is the deploy button for the menu, to toggle the drop
        private NormalButton m_dropButton;
        // This rectangle is used for scrolling in the menu
        private Rectangle m_scrollRect;

        // Min and Max work in a similar manner to the gameplay tower selection pages
        // Instead it uses scrolling to shift the selections as opposed to pages
        private int m_min;
        private int m_max;
        private int m_gap;
        private int m_dropX;
        private int m_dropY;

        // Bool to cheack if the menu us activated
        private bool m_hasDropped;

        // Variable used for scrolling
        private int m_scroll;

        public List<NormalButton> SelectionButtons { get { return m_selectionButtons; } set { m_selectionButtons = value; } }
        public NormalButton DropButton { get { return m_dropButton; } }

        public Rectangle ScrollRect { get { return m_scrollRect; } }

        public bool HasDropped { get { return m_hasDropped; } set { m_hasDropped = value; } }

        public int Min { get { return m_min; } }
        public int Max { get { return m_max; } }

        public DropMenu(ContentManager content, int scrollRectX, int scrollRectY, int scrollRectWidth, int scrollRectHeight, int mainX, int mainY, int gap, int dropX, int dropY, string pathAndName)
        {
            m_selectionButtons = new List<NormalButton>();
            m_dropButton = new NormalButton(content.Load<Texture2D>(pathAndName), new Vector2(mainX, mainY), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Open Menu."), "");

            m_scroll = 1;
            m_scrollRect = new Rectangle(scrollRectX, scrollRectY, scrollRectWidth, scrollRectHeight);
            m_gap = gap;

            m_dropX = dropX;
            m_dropY = dropY;

            m_hasDropped = false;
        }

        public void UpdateMenu(InputManager input)
        {
            m_dropButton.UpdateMe(input);

            if (m_dropButton.IsPressed && !m_hasDropped)
                m_hasDropped = true;
            else if (m_dropButton.IsPressed && m_hasDropped)
                m_hasDropped = false;

            // If the menu has dropped/been activated
            if (m_hasDropped)
            {
                // Update each button
                for (int i = 0; i < m_selectionButtons.Count; i++)
                {
                    m_selectionButtons[i].UpdateMe(input);
                }

                // Scroll through menu options
                if (m_scrollRect.Contains(input.Mouse.X, input.Mouse.Y))
                {
                    if (input.Mouse.ScrollWheelValue > m_scroll)
                    {
                        m_min--;
                    }
                    else if (input.Mouse.ScrollWheelValue < m_scroll)
                    {
                        m_min++;
                    }

                    // Lock the scrolling mechanism, so that when there are 5 or more buttons, only 5 will show
                    if (m_min < 0)
                        m_min = 0;
                    else if (m_selectionButtons.Count < 5)
                        m_min = 0;
                    else if (m_min > m_selectionButtons.Count - 5)
                        m_min = m_selectionButtons.Count - 5;
                }

                m_max = m_min + 5;

                if (m_hasDropped)
                {
                    // Set the "Clickableness" of the button
                    for (int i = 0; i < m_selectionButtons.Count; i++)
                    {
                        if (i >= m_min && i < m_max)
                        {
                            m_selectionButtons[i].IsClickable = true;
                        }
                        else
                        {
                            m_selectionButtons[i].IsClickable = false;
                        }
                    }
                }
            }

            m_scroll = input.Mouse.ScrollWheelValue;
        }

        public void DrawMenu(SpriteBatch sb, GameTime gt)
        {
            m_dropButton.DrawMe(sb, gt);

#if DEBUG
            sb.Begin();
            sb.DrawString(Game1.debugFont, "Has Dropped?: " + m_hasDropped + " Min: " + m_min, m_dropButton.Position + new Vector2(-100, 0), Color.White);
            sb.End();
#endif
            
            if (m_hasDropped)
            {
                for (int i = 0; i < m_selectionButtons.Count; i++)
                {
                    m_selectionButtons[i].Position = new Vector2(m_dropX, m_dropY + (m_gap * (i - m_min)));

                    if (i >= m_min && i < m_max)
                    {
                        m_selectionButtons[i].DrawMe(sb, gt);
#if DEBUG
                        sb.Begin();
                        sb.DrawString(Game1.debugFont, "Button: " + i, m_selectionButtons[i].Position + new Vector2(-100, 0), Color.White);
                        sb.End();
#endif
                    }
                }
            }
        }
    }

    class RadialMenu
    {
        // This is the number of buttons in the menu
        private int m_number;
        // This is the radius of the menu
        private float m_radius;
        // This is the list of buttons within the menu
        private List<NormalButton> m_buttons;
        // This is a list of rotations to be used for the individual buttons
        private List<float> m_rotations;
        // This is the list of messages to be used in tooltips over the buttons
        private List<string> m_messageList;

        // This is a list of positions for the buttons
        private List<Vector2> m_buttonPositions;

        // This is the centre position of the menu
        private Vector2 m_centrePosition;
        // This is the centre of the game map (used to rotate the whole menu to prevent the menu going off screen
        private Point m_middlePos = new Point((36 * 80) / 2, (36 * 45) / 2);

        // This is the deploy button for the menu
        private NormalButton m_deployButton;
        private bool m_isDeployed;

        public Vector2 MenuCentre { get { return m_centrePosition; } set { m_centrePosition = value; } }

        public List<NormalButton> Buttons { get { return m_buttons; } }

        public bool IsDeployed { get { return m_isDeployed; } set { m_isDeployed = value; } }
        public NormalButton DeployButton { get { return m_deployButton; } }

        public RadialMenu(int number, float radius, string pathAndName, float deployScale, float buttonScale, Vector2 position, ContentManager content)
        {
            m_isDeployed = false;
            m_centrePosition = position;
            m_number = number;
            m_radius = radius;

            m_buttons = new List<NormalButton>();
            m_rotations = new List<float>();
            m_buttonPositions = new List<Vector2>();
            m_messageList = new List<string>();

            m_messageList.Add("Move tower.");
            m_messageList.Add("Sell tower.");

            m_deployButton = new NormalButton(content.Load<Texture2D>("Art\\PlaceholderArt\\Tiles\\TowerPlacementTiles"), m_centrePosition, Color.White, deployScale, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Open Tower\nMenu."), "");

            // Create the butons
            for (int i = 0; i < number; i++)
            {
                // This gets the fraction of Pi to be used for button separation
                float piChunk = MathHelper.Pi / number;
                // The rotation depends on the current iteration
                m_rotations.Add(0 + (i * piChunk));

                // Make a button position according to the new rotation
                m_buttonPositions.Add(new Vector2(-(float)Math.Cos(m_rotations[i]), -(float)Math.Sin(m_rotations[i])) * radius);

                // Add the button at the new position
                m_buttons.Add(new NormalButton(content.Load<Texture2D>(pathAndName + i), m_centrePosition + m_buttonPositions[i], Color.White, buttonScale, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, m_messageList[i]), ""));
            }
        }

        public void UpdateMenu(GameTime gt, InputManager input, Camera cam)
        {
            m_deployButton.Position = m_centrePosition;
            m_deployButton.UpdateMeCam(input, cam);

            if (m_isDeployed)
            {
                // Keep the buttons from disappearing off screen by rotating them
                for (int i = 0; i < m_buttons.Count; i++)
                {
                    if (m_centrePosition.X < m_middlePos.X && m_centrePosition.Y < m_middlePos.Y)
                    {
                        m_buttonPositions[i] = new Vector2((float)Math.Cos(m_rotations[i]), (float)Math.Sin(m_rotations[i])) * m_radius;
                    }
                    else if (m_centrePosition.X >= m_middlePos.X && m_centrePosition.Y < m_middlePos.Y)
                    {
                        m_buttonPositions[i] = new Vector2(-(float)Math.Cos(m_rotations[i]), (float)Math.Sin(m_rotations[i])) * m_radius;
                    }
                    else if (m_centrePosition.X >= m_middlePos.X && m_centrePosition.Y >= m_middlePos.Y)
                    {
                        m_buttonPositions[i] = new Vector2(-(float)Math.Cos(m_rotations[i]), -(float)Math.Sin(m_rotations[i])) * m_radius;
                    }
                    else
                    {
                        m_buttonPositions[i] = new Vector2((float)Math.Cos(m_rotations[i]), -(float)Math.Sin(m_rotations[i])) * m_radius;
                    }

                    //m_buttonPositions[i] = new Vector2(-(float)Math.Cos(m_rotations[i]), -(float)Math.Sin(m_rotations[i])) * m_radius;
                    m_buttons[i].Position = m_centrePosition + m_buttonPositions[i];
                    m_buttons[i].UpdateMeCam(input, cam);
                }
            }
        }

        public void DrawMenu(SpriteBatch sb, GameTime gt, Camera cam, InputManager input)
        {
            //m_deployButton.DrawMeCam(sb, cam);

            if (m_isDeployed)
            {
                for (int i = 0; i < m_buttons.Count; i++)
                {
                    m_buttons[i].DrawMeCam(sb, gt, cam);

#if DEBUG
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.Transform);
                    sb.DrawString(Game1.debugFont, "Button i Pos: " + m_buttons[i].Position + "\nIs Deploy Pressed: " + m_buttons[i].IsPressed + "\nMouse Pos: " + new Vector2(input.Mouse.X, input.Mouse.Y), m_centrePosition + new Vector2(0, 150 + (i * 72)), Color.Red);
                    sb.End();
#endif
                }
            }

#if DEBUG
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.Transform);
            sb.DrawString(Game1.debugFont, "MainButton Pos: " + m_deployButton.Position + "\nIs Deploy Pressed: " + m_deployButton.IsPressed + "\nIs Deployed: " + m_isDeployed + "\nMouse Pos: " + new Vector2(input.Mouse.X, input.Mouse.Y), m_centrePosition + new Vector2(0, 50), Color.Red);
            sb.End();
#endif
        }
    }
}
