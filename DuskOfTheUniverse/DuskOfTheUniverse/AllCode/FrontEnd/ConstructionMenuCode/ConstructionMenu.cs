using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    /// <summary>
    /// This is a special menu for constructing towers
    /// </summary>
    class ConstructionMenu
    {
        // This acts as the open menu
        private DropMenu m_dropMenu;
        // This is the menu background
        private StaticGraphic m_backGround;

        // This is the list of various part menus
        private List<DropMenu> m_partMenus;

        // This is a button that allows the player to get back to the game
        private NormalButton m_backButton;
        // This is a button that allows the player to save a tower (not in file though!)
        private NormalButton m_saveButton;
        // This clears the board and makes a blank slate for the player
        private NormalButton m_newTowerButton;

        // This is the visual construct
        private TowerConstruct m_construct;
        // This is the tower that, when saved will be added to a list of saved towers
        private Tower m_tower;

        // This is the control class for actions involving the building of the tower
        private ConstructionControls m_controls;

        // This represents the index of the currently open tower within the saved towers list
        private int m_openedIndex;

        // These variable ensure that you cannot have multiple foundations and rotors
        private int m_foundationCount;
        private int m_rotorCount;

        public NormalButton BackButton { get { return m_backButton; } }
        public NormalButton SaveButton { get { return m_saveButton; } }
        public NormalButton NewTowerButton { get { return m_newTowerButton; } }

        public List<DropMenu> PartMenus { get { return m_partMenus; } }
        public DropMenu OpenMenu { get { return m_dropMenu; } }

        public int FoundationCount { get { return m_foundationCount; } set { m_foundationCount = value; } }
        public int RotorCount { get { return m_rotorCount; } set { m_rotorCount = value; } }

        public ConstructionControls Controls { get { return m_controls; } set { m_controls = value; } }

        public ConstructionMenu(ContentManager content)
        {
            // Create general menu buttons and open menu
            m_backGround = new StaticGraphic(content.Load<Texture2D>("Art\\GameArt\\ConstructionMenu\\Background"), Vector2.Zero, Color.White);

            m_newTowerButton = new NormalButton(content.Load<Texture2D>("Art\\GameArt\\ConstructionMenu\\Buttons\\NewTower"), new Vector2(150, 50), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "New Tower."), "");

            m_dropMenu = new DropMenu(content, 292, 114, 216, 252, 400, 50, 50, 400, 150, "Art\\GameArt\\ConstructionMenu\\Buttons\\OpenTower");

            m_saveButton = new NormalButton(content.Load<Texture2D>("Art\\GameArt\\ConstructionMenu\\Buttons\\SaveTower"), new Vector2(650, 50), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Save Tower."), "");

            m_foundationCount = 0;
            m_rotorCount = 0;

            // Create the part menus and if necessary adjust the source rectangles
            m_partMenus = new List<DropMenu>();

            // Create foundations menu
            CreateFoundationsMenu(content);

            // Create rotors menu
            CreateRotorMenu(content);

            // Create components/struts menu
            CreateComponentMenu(content);

            // Create weapons menu
            CreateWeaponsMenu(content);

            // Create utilities menu
            CreateUtilityMenu(content);

            // Finally create the back button
            m_backButton = new NormalButton(content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\MapGenMenu\\GenButton0"), new Vector2(150, 1000), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Return to the level."), "");

            // Create tower info and controls class
            m_construct = new TowerConstruct(content);
            m_tower = new Tower(content, new Vector2(126, 126));
            m_openedIndex = 0;

            m_controls = new ConstructionControls(this);
        }

        #region CreatePartMenus
        
        private void CreateFoundationsMenu(ContentManager content)
        {
            m_partMenus.Add(new DropMenu(content, 1920 - 216, 180, 216, 900, 1920 - 108, 0 + 18, 150, 1920 - 108, 316, "Art\\GameArt\\ConstructionMenu\\Buttons\\Foundations"));
            m_partMenus[0].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(0, 0), Color.White, 2, 4, 4, 3, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Light Foundations\nHealth: 250\nCost: 100\nMax Components: 10"), ""));
            m_partMenus[0].SelectionButtons[0].SourceRectY = 2;
            m_partMenus[0].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 3, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Medium Foundations\nHealth: 500\nCost: 400\nMax Components: 15"), ""));
            m_partMenus[0].SelectionButtons[1].SourceRectY = 42;
            m_partMenus[0].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(0, 0), Color.White, 4, 4, 4, 3, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Heavy Foundations\nHealth: 1000\nCost: 1200\nMax Components: 20"), ""));
            m_partMenus[0].SelectionButtons[2].SourceRectY = 82;
        }

        private void CreateRotorMenu(ContentManager content)
        {
            m_partMenus.Add(new DropMenu(content, 1920 - 216, 180, 216, 900, 1920 - 108, 36 + 18, 150, 1920 - 108, 316, "Art\\GameArt\\ConstructionMenu\\Buttons\\Rotors"));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor1"), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Small Basic Rotor\nHealth: 100\nCost: 150\nSlots: 1"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor2"), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Small Advanced Rotor\nHealth: 150\nCost: 225\nSlots: 2"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor3"), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Small Prototype Rotor\nHealth: 200\nCost: 300\nSlots: 3"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor1"), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Medium Basic Rotor\nHealth: 200\nCost: 300\nSlots: 1"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor2"), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Medium Advanced Rotor\nHealth: 300\nCost: 450\nSlots: 2"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor3"), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Medium Prototype Rotor\nHealth: 400\nCost: 600\nSlots: 3"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor1"), new Vector2(0, 0), Color.White, 1, 4, 4, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Large Basic Rotor\nHealth: 300\nCost: 450\nSlots: 1"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor2"), new Vector2(0, 0), Color.White, 1, 4, 4, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Large Advanced Rotor\nHealth: 450\nCost: 675\nSlots: 2"), ""));
            m_partMenus[1].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor3"), new Vector2(0, 0), Color.White, 1, 4, 4, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Large Prototype Rotor\nHealth: 600\nCost: 900\nSlots: 4"), ""));
        }

        private void CreateComponentMenu(ContentManager content)
        {
            m_partMenus.Add(new DropMenu(content, 1920 - 216, 180, 216, 900, 1920 - 108, 72 + 18, 150, 1920 - 108, 316, "Art\\GameArt\\ConstructionMenu\\Buttons\\Components"));
            m_partMenus[2].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component1"), new Vector2(0, 0), Color.White, 3, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Single Slot Component\nHealth: 100\nCost: 100\nSlots: 1"), ""));
            m_partMenus[2].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component2"), new Vector2(0, 0), Color.White, 3, 1, 1, 1, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Double Slot Component\nHealth: 150\nCost: 150\nSlots: 2"), ""));
        }

        private void CreateWeaponsMenu(ContentManager content)
        {
            m_partMenus.Add(new DropMenu(content, 1920 - 216, 180, 216, 900, 1920 - 108, 108 + 18, 150, 1920 - 108, 316, "Art\\GameArt\\ConstructionMenu\\Buttons\\Offense"));
            m_partMenus[3].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(0, 0), Color.White, 3, 2, 4, 6, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Basic Cannon\nDamage: 5\nRoF: 2\nRange(Tiles): 6\nHealth: 50\nCost: 50"), ""));
            m_partMenus[3].SelectionButtons[0].SourceRectY = 2;
            m_partMenus[3].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(0, 0), Color.White, 3, 2, 4, 6, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Heavy Cannon\nDamage: 10\nRoF: 1.33\nRange(Tiles): 7\nHealth: 100\nCost: 100"), ""));
            m_partMenus[3].SelectionButtons[1].SourceRectY = 42;
            m_partMenus[3].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(0, 0), Color.White, 3, 2, 4, 6, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Laser Cannon\nDamage: 4/s (constant)\nRoF: Constant\nRange(Tiles): 7\nHealth: 75\nCost: 75\nSpecial: Hit all enemies in line"), ""));
            m_partMenus[3].SelectionButtons[2].SourceRectY = 82;
            m_partMenus[3].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(0, 0), Color.White, 3, 2, 4, 6, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Missile Launcher\nDamage: 10\nRoF: 0.5\nRange(Tiles): 8\nHealth: 85\nCost: 85\nSpecial: Emits shockwave"), ""));
            m_partMenus[3].SelectionButtons[3].SourceRectY = 122;
            m_partMenus[3].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(0, 0), Color.White, 3, 2, 4, 6, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Flame Cannon\nDamage: 33.33/s\nRoF: 1\nRange(Tiles): 5\nHealth: 80\nCost: 80"), ""));
            m_partMenus[3].SelectionButtons[4].SourceRectY = 162;
            m_partMenus[3].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(0, 0), Color.White, 3, 2, 4, 6, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Splash Cannon\nDamage: 20/s\nRoF: 1.33\nRange(Tiles): 3\nHealth: 80\nCost: 80"), ""));
            m_partMenus[3].SelectionButtons[5].SourceRectY = 202;
        }

        private void CreateUtilityMenu(ContentManager content)
        {
            m_partMenus.Add(new DropMenu(content, 1920 - 216, 180, 216, 900, 1920 - 108, 144 + 18, 150, 1920 - 108, 316, "Art\\GameArt\\ConstructionMenu\\Buttons\\Utility"));
            m_partMenus[4].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 7, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Range Booster\nBoost: 1.5 (Deminishing Returns)\nRoF: 1.33\nHealth: 50\nCost: 50"), ""));
            m_partMenus[4].SelectionButtons[0].SourceRectY = 2;
            m_partMenus[4].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 7, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Damage Booster\nDamage: 1.5 (Deminishing Returns)\nRoF: 1.33\nHealth: 50\nCost: 50"), ""));
            m_partMenus[4].SelectionButtons[1].SourceRectY = 42;
            m_partMenus[4].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 7, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Rate of Fire Booster\nDamage: 1.5 (Deminishing Returns)\nRoF: 1.33\nHealth: 50\nCost: 50"), ""));
            m_partMenus[4].SelectionButtons[2].SourceRectY = 82;
            m_partMenus[4].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 7, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Tower Health Booster\nDamage: 1.5 (Deminishing Returns)\nRoF: 1.33\nHealth: 50\nCost: 50"), ""));
            m_partMenus[4].SelectionButtons[3].SourceRectY = 122;
            m_partMenus[4].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 7, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Healing Module (ally)\nHealing: 1/s\nRoF: 1.33\nRange(Tiles): 5\nDescription: Heal nearby allies.\nHealth: 50\nCost: 50"), ""));
            m_partMenus[4].SelectionButtons[4].SourceRectY = 162;
            m_partMenus[4].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 7, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Motivation Module\nAttribute Boost: 1.1\nRoF: 1.33\nRange(Tiles): 5\nDescription: Boost maxhealth, damage \nand attack rate of allies.\nHealth: 50\nCost: 50"), ""));
            m_partMenus[4].SelectionButtons[5].SourceRectY = 202;
            m_partMenus[4].SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(0, 0), Color.White, 3, 4, 4, 7, content, new ToolTip(content.Load<Texture2D>("Art\\GameArt\\ToolTipBacking"), Game1.debugFont, new Vector2(0, 0), Color.White, Color.Black, Vector2.Zero, "Time Dilation Module\nSlow Factor: 2\nRoF: 1.33\nRange(Tiles): 5\nDescription: Slows nearby enemies by half.\nHealth: 50\nCost: 50"), ""));
            m_partMenus[4].SelectionButtons[6].SourceRectY = 242;
        }

        #endregion

        public void UpdateMenu(InputManager input, TypingManager typingManager, List<MessagePopup> popups, ContentManager content, Camera cam, GameTime gt, List<TowerConstruct> schematics, List<Tower> towers, PlayerStats userData)
        {
            m_dropMenu.UpdateMenu(input);

            // Open a tower from the menu
            OpenTower(schematics, towers);

            // Update the general buttons
            m_backButton.UpdateMe(input);
            m_saveButton.UpdateMe(input);
            m_newTowerButton.UpdateMe(input);

            // Update the controls
            m_controls.UpdateControls(input, typingManager, popups, m_construct, m_tower, content, cam, gt);

            // Create new tower
            CreateNewTower(content);

            // Update the construct
            m_construct.UpdateConstruct(gt);

            // Saving mechanics
            SaveTower(popups, content, schematics, towers);

            // Handle menus and menu options
            PartMenuHandler(input, userData);
        }

        private void PartMenuHandler(InputManager input, PlayerStats userData)
        {
            for (int i = 0; i < m_partMenus.Count; i++)
            {
                // Update part menu
                m_partMenus[i].UpdateMenu(input);

                for (int k = 0; k < m_partMenus[i].SelectionButtons.Count; k++)
                {
                    // Divide the number of options, and unlock the one below the result
                    int divisor = (4 - userData.LevelsComplete);

                    // Prevents dividing by weird numbers or dividing by 0
                    if (divisor <= 1)
                        divisor = 1;

                    // Locks and unlocks parts. Will also lock if they are not within the min and max index
                    if (k <= m_partMenus[i].SelectionButtons.Count / divisor && k >= m_partMenus[i].Min && k < m_partMenus[i].Max)
                        m_partMenus[i].SelectionButtons[k].IsClickable = true;
                    else
                    {
                        m_partMenus[i].SelectionButtons[k].IsClickable = false;
                    }
                }

                // Deploy menu
                if (m_partMenus[i].DropButton.IsPressed)
                {
                    // deploy
                    m_partMenus[i].HasDropped = true;

                    // shut every other part menu
                    for (int k = 0; k < m_partMenus.Count; k++)
                    {
                        if (k != i)
                            m_partMenus[k].HasDropped = false;
                    }
                }
            }
        }

        #region TowerHandling

        private void SaveTower(List<MessagePopup> popups, ContentManager content, List<TowerConstruct> schematics, List<Tower> towers)
        {
            // Only save if the tower has at least 1 base and 1 rotor
            if (m_saveButton.IsPressed && m_foundationCount > 0 && m_rotorCount > 0)
            {
                // overwrite the currently opened tower with the changes, but only if the name exists
                if (schematics.Contains(m_construct) && m_tower.TowerName.Length > 0)
                {
                    schematics[m_openedIndex] = m_construct;
                    towers[m_openedIndex] = m_tower;
                    m_dropMenu.SelectionButtons[m_openedIndex].Text = m_tower.TowerName;
                }
                else if (!schematics.Contains(m_construct) && m_tower.TowerName.Length > 0) // Create a new tower as long as the name exists
                {
                    schematics.Add(m_construct);
                    towers.Add(m_tower);
                    m_dropMenu.SelectionButtons.Add(new NormalButton(content.Load<Texture2D>("Art\\GameArt\\ConstructionMenu\\Buttons\\TowerSelectButton"), new Vector2(0, 0), Color.White, 1, 1, 1, 1, content, null, m_tower.TowerName));
                }
                else // If the name does not contain any characters, then do not save and display an error
                {
                    popups.Add(new MessagePopup(content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\MessageBacking"), new Vector2(800, 70), Color.White, -Vector2.UnitY, "Cannot Save!", 0.3f, Game1.gameFontNorm));
                }
            }
            else if (m_saveButton.IsPressed && m_foundationCount <= 0 && m_rotorCount <= 0) // If a base or rotor is missing, display an error
            {
                popups.Add(new MessagePopup(content.Load<Texture2D>("Art\\GameArt\\MenuGUI\\MessageBacking"), new Vector2(800, 70), Color.White, -Vector2.UnitY, "Cannot Save!", 0.3f, Game1.gameFontNorm));
            }
        }

        private void CreateNewTower(ContentManager content)
        {
            // If new tower button is pressed, make a blank slate
            if (m_newTowerButton.IsPressed)
            {
                m_construct = new TowerConstruct(content);
                m_tower = new Tower(content, new Vector2(126, 126));
                m_foundationCount = 0;
                m_rotorCount = 0;

                m_controls.RotorIndex = 999;
                m_controls.FoundationsIndex = 999;
            }
        }

        private void OpenTower(List<TowerConstruct> schematics, List<Tower> towers)
        {
            // Open tower according to the index of the button that was pressed
            for (int i = 0; i < m_dropMenu.SelectionButtons.Count; i++)
            {
                if (m_dropMenu.SelectionButtons[i].IsPressed)
                {
                    m_construct = schematics[i];
                    m_tower = towers[i];
                    m_openedIndex = i;
                    m_foundationCount = 1;
                    m_rotorCount = 0;

                    for (int k = 0; k < towers[i].TowerParts.Count; k++)
                    {
                        if (towers[i].TowerParts[k] is BaseRotor)
                        {
                            m_rotorCount++;
                        }
                    }

                    m_controls.RotorIndex = schematics[i].RotorIndex;
                    m_controls.FoundationsIndex = schematics[i].FoundationsIndex;
                }
            }
        }

        #endregion

        public void DrawMenu(SpriteBatch sb, GameTime gt, Camera cam, TypingManager typingManager)
        {
            sb.Begin();
            m_backGround.DrawMe(sb);
            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, cam.Transform);
            DrawConstruct(sb, gt);
            m_controls.DrawPart(sb, gt);
            sb.End();

            m_dropMenu.DrawMenu(sb, gt);

            m_backButton.DrawMe(sb, gt);
            m_saveButton.DrawMe(sb, gt);
            m_newTowerButton.DrawMe(sb, gt);

            m_controls.CurrentTowerStats(sb, m_construct, m_tower, typingManager);

#if DEBUG
            m_controls.DebugControls(sb, m_construct);

            sb.Begin();
            sb.DrawString(Game1.debugFont, "Foundations: " + m_foundationCount + "\nRotors: " + m_rotorCount, new Vector2(100, 500), Color.White);
            sb.End();
#endif



            for (int i = 0; i < m_partMenus.Count; i++)
            {
                m_partMenus[i].DrawMenu(sb, gt);
            }
        }

        public void DrawConstruct(SpriteBatch sb, GameTime gt)
        {
            m_construct.DrawMe(sb, gt);
        }
    }
}
