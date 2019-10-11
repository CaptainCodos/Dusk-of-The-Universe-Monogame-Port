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
    class ConstructionControls
    {
        // This is the Construction Menu
        private ConstructionMenu m_menu;

        // This is the current height on the tower the mouse is at
        private int m_hoverHeight;

        // These are the base parts of the schematic and tower respectively (both are constructed at the same time)
        // As stated, the schematic is just visual and has far less functions than the tower and thus easier to handle
        private BasePart m_part;
        private TowerMasterPart m_towerPart;

        // This boolean is to check if the player is currently typing the name (so cam controls cease)
        private bool m_isTyping;
        private Rectangle m_typingRect;
        private Color m_textColour;

        private int m_rotorIndex;
        private int m_foundationsIndex;

        public bool IsTyping { get { return m_isTyping; } }

        public int RotorIndex { get { return m_rotorIndex; } set { m_rotorIndex = value; } }
        public int FoundationsIndex { get { return m_foundationsIndex; } set { m_foundationsIndex = value; } }

        public ConstructionControls(ConstructionMenu menu)
        {
            m_menu = menu;

            m_isTyping = false;
            m_typingRect = new Rectangle(800, 10, 350, 70);
            m_textColour = Color.Black;

            m_rotorIndex = 999;
            m_foundationsIndex = 999;
        }

        public void UpdateControls(InputManager input, TypingManager typingManager, List<MessagePopup> popups, TowerConstruct construct, Tower tower, ContentManager content, Camera cam, GameTime gt)
        {
            // Transform mouse pos to screen coordinates
            Vector2 mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);
            mousePos = Vector2.Transform(mousePos, Matrix.Invert(cam.Transform));

            // Toggle Typing
            // Checks if the typing rectangle contains non transformed mosue coordinates
            TypingLogic(input, typingManager, popups, tower, content);

            // Part creation logic
            PartCreation(input, construct, tower, content);

            // Control selected part
            PartDragControl(input, cam, gt, mousePos);

            // Mouse hover height logic
            MouseHoverHeight(construct, mousePos);

            // Iterate through the construct parts and their part slots
            for (int i = 0; i < construct.Parts.Count; i++)
            {
                for (int k = 0; k < construct.Parts[i].Slots.Count; k++)
                {
                    // If the slot contains mouse position
                    if (construct.Parts[i].Slots[k].DropCircle.Contains(mousePos))
                    {
                        if (input.Mouse.RightButton == ButtonState.Pressed && m_hoverHeight >= construct.Parts[i].Slots[k].SlotHeight)
                        {
                            //BasePart part = construct.Parts[i].Slots[k].StoredPart;
                            //TowerMasterPart tPart = tower.TowerParts[i].TowerParts[k];

                            //construct.Parts[i].Parts.Remove(part);
                            //construct.Parts.Remove(part);
                            //tower.TowerParts[i].TowerParts.Remove(tPart);
                            //tower.TowerParts.Remove(tPart);
                        }

                        // Add tower part to tower and part to construct
                        AddPartMechanics(input, construct, tower, i, k);
                    }
                }
            }

            // Is the part dropped in blank space?
            SpaceDrop(input);
        }

        #region Part Control
        private void AddPartMechanics(InputManager input, TowerConstruct construct, Tower tower, int i, int k)
        {
            // If left buttons is pressed, if hoverheight is at part, if there is no stored part, 
            // if part is not null and there are still components spaces to spare
            if (input.Mouse.LeftButton == ButtonState.Released
                && m_hoverHeight == construct.Parts[i].PartHeight
                && construct.Parts[i].Slots[k].StoredPart == null
                && m_part != null
                && construct.Parts.Count < tower.MaxComponents)
            {
                // If (tower part is a rotr and there is no rotor) or (tower part is not a rotor), then add part and give values
                if ((m_towerPart is BaseRotor && m_menu.RotorCount < 1) || !(m_towerPart is BaseRotor))
                {
                    // If tower part is a rotor, then increase rotor count
                    if (m_towerPart is BaseRotor)
                    {
                        m_menu.RotorCount++;
                        m_rotorIndex = m_part.SubIndex;
                        construct.RotorIndex = m_part.SubIndex;
                    }

                    // Set part height, position, relative rotation, internal index for rotation and external index for construct
                    m_part.PartHeight = construct.Parts[i].PartHeight + 1;
                    m_part.Position = construct.Parts[i].Slots[k].Position;
                    m_part.RelativeRot = m_part.Rotation - construct.Parts[i].Rotation;
                    m_part.Index = construct.Parts[i].Slots[k].Index;
                    m_part.ExtendedIndex = construct.Parts.Count;

                    // set tower part offset, relative rotation, internal index for rotation, parent index (to track parent), sub index, type index and external index on the tower
                    m_towerPart.Position = tower.TowerParts[i].Offsets[k];
                    m_towerPart.RelativeRotation = m_towerPart.Rotation - tower.TowerParts[i].Rotation;
                    m_towerPart.TowerIndex = construct.Parts[i].Slots[k].Index;
                    m_towerPart.ParentIndex = tower.TowerParts[i].ExtendedIndex;
                    m_towerPart.SubIndex = m_part.SubIndex;
                    m_towerPart.TypeIndex = m_part.TypeIndex;
                    m_towerPart.ExtendedIndex = tower.TowerParts.Count;

                    // add part to construct and tower part to tower
                    construct.Parts.Add(m_part);
                    construct.Parts[i].Parts.Add(m_part);
                    construct.Parts[i].Slots[k].StoredPart = m_part;

                    m_towerPart.UpdateProps();
                    tower.TowerParts.Add(m_towerPart);
                    tower.TowerParts[i].TowerParts.Add(m_towerPart);
                    tower.TowerCost += m_towerPart.PartCost;
                    tower.UpdateAllProps();
                }

                // nullify part and tower part
                m_part = null;
                m_towerPart = null;
            }
        }

        private void SpaceDrop(InputManager input)
        {
            // If tower part is not add to the tower by now, then if mouse left button is release, nullify parts
            if (input.Mouse.LeftButton == ButtonState.Released && (m_part != null || m_towerPart != null))
            {
                m_part = null;
                m_towerPart = null;
            }
        }

        private void MouseHoverHeight(TowerConstruct construct, Vector2 mousePos)
        {
            m_hoverHeight = 0;

            // Iterates through construct parts and their slots to find the heighest slot containing the mouse
            for (int i = 0; i < construct.Parts.Count; i++)
            {
                for (int k = 0; k < construct.Parts[i].Slots.Count; k++)
                {
                    if (construct.Parts[i].Slots[k].DropCircle.Contains(mousePos) && m_hoverHeight < construct.Parts[i].PartHeight)
                    {
                        m_hoverHeight = construct.Parts[i].PartHeight;
                    }
                }
            }
        }

        private void PartCreation(InputManager input, TowerConstruct construct, Tower tower, ContentManager content)
        {
            #region FOUNDATIONS
            FoundationButtonBehaviours(construct, tower, content);
            #endregion

            #region ROTORS
            RotorButtonBehaviours(input, construct, tower, content);
            #endregion

            #region COMPONENTS
            ComponentButtonBehaviours(input, construct, tower, content);
            #endregion

            #region OFFENSE
            OffenseButtonBehaviours(input, construct, tower, content);
            #endregion

            #region UTILITY
            UtilityButtonBehaviours(input, construct, tower, content);
            #endregion
        }

        private void PartDragControl(InputManager input, Camera cam, GameTime gt, Vector2 mousePos)
        {
            // If left mouse button is held and part exists
            if (m_part != null && input.Mouse.LeftButton == ButtonState.Pressed)
            {
                // Disable cam scroll function, update part to mouse position and toggle scroll to rotation controls
                cam.ScrollAvailable = false;
                m_part.Position = mousePos;
                m_part.UpdateMe(gt);

                if (input.ScrolledUp)
                {
                    m_part.Rotation += (2 * MathHelper.Pi) / 8;
                }
                if (input.ScrolledDown)
                {
                    m_part.Rotation -= (2 * MathHelper.Pi) / 8;
                }
            }

            // Move tower part to mouse position
            if (m_towerPart != null && input.Mouse.LeftButton == ButtonState.Pressed)
            {
                m_towerPart.Position = mousePos;
            }

            // Give tower part the construct part's rotation
            if (m_towerPart != null && m_part != null)
            {
                m_towerPart.Rotation = m_part.Rotation;
            }
        }
        #endregion

        private void TypingLogic(InputManager input, TypingManager typingManager, List<MessagePopup> popups, Tower tower, ContentManager content)
        {
            if (m_typingRect.Contains((int)input.Mouse.X, (int)input.Mouse.Y))
            {
                // If the left button is pressed, enable typing
                if (input.LeftPressed)
                {
                    m_isTyping = true;
                    m_textColour = Color.White;
                }
            }
            else
            {
                // If left button is pressed disable typing
                if (input.LeftPressed)
                {
                    m_isTyping = false;
                    m_textColour = Color.Black;
                }
            }

            // If typing is toggled, then cam WASD controls switch to typing controls
            if (m_isTyping)
            {
                tower.TowerName = typingManager.UpdateManager(input, tower.TowerName, content, popups);
            }
        }

        #region Button Behaviours
        private void UtilityButtonBehaviours(InputManager input, TowerConstruct construct, Tower tower, ContentManager content)
        {
            if (m_menu.PartMenus[4].SelectionButtons[0].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 4, 0, construct.Parts.Count, 4, 4, 7);
                m_towerPart = new RangeBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 7, vecList, 4, 0);
            }
            if (m_menu.PartMenus[4].SelectionButtons[1].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 4, 1, construct.Parts.Count, 4, 4, 7);
                m_towerPart = new DamageBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 7, vecList, 4, 1);
            }
            if (m_menu.PartMenus[4].SelectionButtons[2].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 4, 2, construct.Parts.Count, 4, 4, 7);
                m_towerPart = new RateBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 7, vecList, 4, 2);
            }
            if (m_menu.PartMenus[4].SelectionButtons[3].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 4, 3, construct.Parts.Count, 4, 4, 7);
                m_towerPart = new HealthBoostModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 7, vecList, 4, 3);
            }
            if (m_menu.PartMenus[4].SelectionButtons[4].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 4, 4, construct.Parts.Count, 4, 4, 7);
                m_towerPart = new HealingModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 7, 5, vecList, 4, 4);
            }
            if (m_menu.PartMenus[4].SelectionButtons[5].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 4, 5, construct.Parts.Count, 4, 4, 7);
                m_towerPart = new MotivationModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 7, 5, vecList, 4, 5);
            }
            if (m_menu.PartMenus[4].SelectionButtons[6].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 4, 6, construct.Parts.Count, 4, 4, 7);
                m_towerPart = new TimeDilationModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\UtilitySheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 7, 5, vecList, 4, 6);
            }
        }

        private void OffenseButtonBehaviours(InputManager input, TowerConstruct construct, Tower tower, ContentManager content)
        {
            if (m_menu.PartMenus[3].SelectionButtons[0].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 3, 0, construct.Parts.Count, 8, 4, 6);
                m_towerPart = new NormalCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 8, 4, 6, content, vecList, 3, 0);
            }
            if (m_menu.PartMenus[3].SelectionButtons[1].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 3, 1, construct.Parts.Count, 8, 4, 6);
                m_towerPart = new HeavyCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 8, 4, 6, content, vecList, 3, 1);
            }
            if (m_menu.PartMenus[3].SelectionButtons[2].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 3, 2, construct.Parts.Count, 8, 4, 6);
                m_towerPart = new LaserCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 8, 4, 6, content, vecList, 3, 2);
            }
            if (m_menu.PartMenus[3].SelectionButtons[3].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 3, 3, construct.Parts.Count, 8, 4, 6);
                m_towerPart = new MissileCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 8, 4, 6, content, vecList, 3, 3);
            }
            if (m_menu.PartMenus[3].SelectionButtons[4].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 3, 4, construct.Parts.Count, 8, 4, 6);
                m_towerPart = new FlameCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 8, 4, 6, content, vecList, 3, 4);
            }
            if (m_menu.PartMenus[3].SelectionButtons[5].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 3, 5, construct.Parts.Count, 8, 4, 6);
                m_towerPart = new SplashCannonModule(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\WeaponsSheet"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 8, 4, 6, content, vecList, 3, 5);
            }
        }

        private void ComponentButtonBehaviours(InputManager input, TowerConstruct construct, Tower tower, ContentManager content)
        {
            if (m_menu.PartMenus[2].SelectionButtons[0].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, -12));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component1"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 2, 0, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new BasicComponentSingle(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 2, 0);
            }
            if (m_menu.PartMenus[2].SelectionButtons[1].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, -12));
                vecList.Add(new Vector2(0, 12));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component2"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 2, 1, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new BasicComponentDouble(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Components\\Component"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 2, 1);
            }
        }

        private void RotorButtonBehaviours(InputManager input, TowerConstruct construct, Tower tower, ContentManager content)
        {
            if (m_menu.PartMenus[1].SelectionButtons[0].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, -5));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor1"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 0, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new SmallBasicRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor1"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 1, 0);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[1].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(-11, 6));
                vecList.Add(new Vector2(11, 6));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor2"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 1, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new SmallAdvancedRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor2"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 1, 1);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[2].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, -8));
                vecList.Add(new Vector2(-11, 6));
                vecList.Add(new Vector2(11, 6));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor3"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 2, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new SmallPrototypeRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Light\\SmallRotor3"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 1, 2);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[3].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, -21));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor1"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 3, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new MediumBasicRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor1"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 1, 3);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[4].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(-22, 13));
                vecList.Add(new Vector2(22, 13));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor2"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 4, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new MediumAdvancedRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor2"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 1, 4);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[5].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, -21));
                vecList.Add(new Vector2(-22, 13));
                vecList.Add(new Vector2(22, 13));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor3"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 5, construct.Parts.Count, 1, 1, 1);
                m_towerPart = new MediumPrototypeRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Medium\\MediumRotor3"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 1, 1, 1, vecList, 1, 5);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[6].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, -11));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor1"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 6, construct.Parts.Count, 4, 4, 1);
                m_towerPart = new HeavyBasicRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor1"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 1, vecList, 1, 6);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[7].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(-34, 27));
                vecList.Add(new Vector2(34, 27));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor2"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 7, construct.Parts.Count, 4, 4, 1);
                m_towerPart = new HeavyAdvancedRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor2"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 1, vecList, 1, 7);
                //m_menu.RotorCount++;
            }
            if (m_menu.PartMenus[1].SelectionButtons[8].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(-24, -40));
                vecList.Add(new Vector2(24, -40));
                vecList.Add(new Vector2(-34, 27));
                vecList.Add(new Vector2(34, 27));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor3"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 0, content, vecList, 1, 8, construct.Parts.Count, 4, 4, 1);
                m_towerPart = new HeavyPrototypeRotor(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\Rotors\\Heavy\\LargeRotor3"), new Vector2(input.Mouse.X, input.Mouse.Y), Color.White, 1, 4, 4, 1, vecList, 1, 8);
                //m_menu.RotorCount++;
            }

            if (m_towerPart != null)
            {
                m_towerPart.SourceRectY = 2;
            }
        }

        private void FoundationButtonBehaviours(TowerConstruct construct, Tower tower, ContentManager content)
        {
            if (m_menu.PartMenus[0].SelectionButtons[0].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, 0));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(126, 126), Color.White, 1, 0, content, vecList, 0, 0, construct.Parts.Count, 4, 4, 3);
                m_towerPart = new LightFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(126, 126), Color.White, 1, 4, 4, 3, vecList, 0, 0);
                if (construct.Parts[0] != null)
                {
                    construct.Parts.RemoveAt(0);
                    construct.Parts.Add(m_part);
                    construct.Parts[0].ExtendedIndex = 0;
                    tower.TowerDimensions = 1;
                    m_foundationsIndex = 0;
                    construct.FoundationsIndex = 0;

                    if (tower.TowerParts.Count > 0 && tower.TowerParts[0] != null)
                    {
                        tower.TowerParts.RemoveAt(0);
                        tower.TowerParts.Add(m_towerPart);
                        tower.TowerCost = m_towerPart.PartCost;
                        tower.TowerParts[0].ParentIndex = 999;
                        tower.TowerParts[0].ExtendedIndex = 0;
                        tower.MaxComponents = 10;
                        m_menu.FoundationCount = 1;
                        m_foundationsIndex = 0;
                        construct.FoundationsIndex = 0;
                    }
                    else
                    {
                        tower.TowerParts.Add(m_towerPart);
                        tower.TowerCost = m_towerPart.PartCost;
                        tower.TowerParts[0].ParentIndex = 999;
                        tower.TowerParts[0].ExtendedIndex = 0;
                        tower.MaxComponents = 10;
                        m_menu.FoundationCount = 1;
                        m_foundationsIndex = 0;
                        construct.FoundationsIndex = 0;
                    }
                    
                    m_part = null;
                    m_towerPart = null;
                }
                else
                {
                    construct.Parts.Add(m_part);
                    construct.Parts[0].ExtendedIndex = 0;
                    tower.TowerDimensions = 1;
                    tower.MaxComponents = 10;
                    m_menu.FoundationCount = 1;
                    m_foundationsIndex = 0;
                    construct.FoundationsIndex = 0;
                    m_part = null;
                    m_towerPart = null;
                }
            }
            else if (m_menu.PartMenus[0].SelectionButtons[1].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, 0));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(126, 126), Color.White, 2, 0, content, vecList, 0, 1, construct.Parts.Count, 4, 4, 3);
                m_towerPart = new MediumFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(126, 126), Color.White, 2, 4, 4, 3, vecList, 0, 1);

                if (construct.Parts[0] != null)
                {
                    construct.Parts.RemoveAt(0);
                    construct.Parts.Add(m_part);
                    tower.TowerDimensions = 2;
                    m_foundationsIndex = 1;
                    construct.FoundationsIndex = 1;

                    if (tower.TowerParts.Count > 0 && tower.TowerParts[0] != null)
                    {
                        tower.TowerParts.RemoveAt(0);
                        tower.TowerParts.Add(m_towerPart);
                        tower.TowerCost = m_towerPart.PartCost;
                        tower.TowerParts[0].ParentIndex = 999;
                        tower.TowerParts[0].ExtendedIndex = 0;
                        tower.MaxComponents = 15;
                        m_menu.FoundationCount = 2;
                        m_foundationsIndex = 1;
                        construct.FoundationsIndex = 1;
                    }
                    else
                    {
                        tower.TowerParts.Add(m_towerPart);
                        tower.TowerCost = m_towerPart.PartCost;
                        tower.TowerParts[0].ParentIndex = 999;
                        tower.TowerParts[0].ExtendedIndex = 0;
                        tower.MaxComponents = 15;
                        m_menu.FoundationCount = 2;
                        m_foundationsIndex = 1;
                        construct.FoundationsIndex = 1;
                    }

                    m_part = null;
                    m_towerPart = null;
                }
                else
                {
                    construct.Parts.Add(m_part);
                    tower.TowerDimensions = 2;
                    tower.MaxComponents = 15;
                    m_menu.FoundationCount = 1;
                    m_foundationsIndex = 1;
                    construct.FoundationsIndex = 1;
                    m_part = null;
                    m_towerPart = null;
                }
            }
            else if (m_menu.PartMenus[0].SelectionButtons[2].IsPressed)
            {
                List<Vector2> vecList = new List<Vector2>();
                vecList.Add(new Vector2(0, 0));
                m_part = new BasePart(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(126, 126), Color.White, 3, 0, content, vecList, 0, 2, construct.Parts.Count, 4, 4, 3);
                m_towerPart = new HeavyFoundations(content.Load<Texture2D>("Art\\GameArt\\TowerPartArt\\FoundationsSheet"), new Vector2(126, 126), Color.White, 3, 4, 4, 3, vecList, 0, 2);
                if (construct.Parts[0] != null)
                {
                    construct.Parts.RemoveAt(0);
                    construct.Parts.Add(m_part);
                    tower.TowerDimensions = 3;
                    m_foundationsIndex = 2;
                    construct.FoundationsIndex = 2;

                    if (tower.TowerParts.Count > 0 && tower.TowerParts[0] != null)
                    {
                        tower.TowerParts.RemoveAt(0);
                        tower.TowerParts.Add(m_towerPart);
                        tower.TowerCost = m_towerPart.PartCost;
                        tower.TowerParts[0].ParentIndex = 999;
                        tower.TowerParts[0].ExtendedIndex = 0;
                        tower.MaxComponents = 20;
                        m_menu.FoundationCount = 3;
                        m_foundationsIndex = 2;
                        construct.FoundationsIndex = 2;
                    }
                    else
                    {
                        tower.TowerParts.Add(m_towerPart);
                        tower.TowerCost = m_towerPart.PartCost;
                        tower.TowerParts[0].ParentIndex = 999;
                        tower.TowerParts[0].ExtendedIndex = 0;
                        tower.MaxComponents = 20;
                        m_menu.FoundationCount = 3;
                        m_foundationsIndex = 2;
                        construct.FoundationsIndex = 2;
                    }

                    m_part = null;
                    m_towerPart = null;
                }
                else
                {
                    construct.Parts.Add(m_part);
                    tower.TowerDimensions = 3;
                    tower.MaxComponents = 20;
                    m_menu.FoundationCount = 3;
                    m_foundationsIndex = 2;
                    construct.FoundationsIndex = 2;
                    m_part = null;
                    m_towerPart = null;
                }
            }
        }
        #endregion

        public void DrawPart(SpriteBatch sb, GameTime gt)
        {
            if (m_part != null)
                m_part.DrawMe(sb, gt);
        }

        public void CurrentTowerStats(SpriteBatch sb, TowerConstruct construct, Tower tower, TypingManager typingManager)
        {
            // Display Tower Stats
            tower.TowerCost = 0;
            tower.UpdateTowerProperties();
            int[] counts = CountTowerModules(tower);

            string f_id;
            string r_id;

            switch(m_foundationsIndex)
            {
                case 0:
                    f_id = "Light";
                    break;
                case 1:
                    f_id = "Medium";
                    break;
                case 2:
                    f_id = "Heavy";
                    break;
                default:
                    f_id = "Null";
                    break;
            }
            switch(m_rotorIndex)
            {
                case 0:
                    r_id = "Small Basic";
                    break;
                case 1:
                    r_id = "Small Advanced";
                    break;
                case 2:
                    r_id = "Small Prototype";
                    break;
                case 3:
                    r_id = "Medium Basic";
                    break;
                case 4:
                    r_id = "Medium Advanced";
                    break;
                case 5:
                    r_id = "Medium Prototype";
                    break;
                case 6:
                    r_id = "Large Basic";
                    break;
                case 7:
                    r_id = "Large Advanced";
                    break;
                case 8:
                    r_id = "Large Prototype";
                    break;
                default:
                    r_id = "Null";
                    break;
            }

            sb.Begin();
            sb.DrawString(Game1.gameFontNorm, "Name: " + tower.TowerName, new Vector2(800, 20), m_textColour);

            if (m_isTyping)
                typingManager.DrawTyper(sb, "Name: " + tower.TowerName, Game1.gameFontNorm, new Vector2(800, 30), Game1.gameFontNorm.MeasureString("Name: " + tower.TowerName).Y, false);
            
            sb.End();

            sb.Begin();
            sb.DrawString(Game1.debugFont, "Health: " + tower.Health + 
                "\nCost: " + tower.TowerCost + 
                "\n\n Foundations\n" + 
                "\n     Tower Base: " + f_id +
                "\n\n Rotors\n" + 
                "\n     Rotor: " + r_id +
                "\n\nConstruct Parts: " + construct.Parts.Count + " / " + tower.MaxComponents +
                "\nHover Height: " + m_hoverHeight, new Vector2(1200, 1), Color.White);
            sb.End();
        }

        private int[] CountTowerModules(Tower tower)
        {
            // Holds the number of each module
            int[] counts = new int[27];

            // Iterate through tower parts and add according to the type index and sub index
            for (int i = 0; i < tower.TowerParts.Count; i++)
            {
                switch (tower.TowerParts[i].TypeIndex)
                {
                    case 2:
                        CountComponents(tower, counts, i);
                        break;
                    case 3:
                        CountWeapons(tower, counts, i);
                        break;
                    case 4:
                        CountUtilities(tower, counts, i);
                        break;
                }
            }

            return counts;
        }
        #region Count Parts
        private static void CountUtilities(Tower tower, int[] counts, int i)
        {
            switch (tower.TowerParts[i].SubIndex)
            {
                case 0:
                    counts[20]++;
                    break;
                case 1:
                    counts[21]++;
                    break;
                case 2:
                    counts[22]++;
                    break;
                case 3:
                    counts[23]++;
                    break;
                case 4:
                    counts[24]++;
                    break;
                case 5:
                    counts[25]++;
                    break;
                case 6:
                    counts[26]++;
                    break;
            }
        }

        private static void CountWeapons(Tower tower, int[] counts, int i)
        {
            switch (tower.TowerParts[i].SubIndex)
            {
                case 0:
                    counts[14]++;
                    break;
                case 1:
                    counts[15]++;
                    break;
                case 2:
                    counts[16]++;
                    break;
                case 3:
                    counts[17]++;
                    break;
                case 4:
                    counts[18]++;
                    break;
                case 5:
                    counts[19]++;
                    break;
            }
        }

        private static void CountComponents(Tower tower, int[] counts, int i)
        {
            switch (tower.TowerParts[i].SubIndex)
            {
                case 0:
                    counts[12]++;
                    break;
                case 1:
                    counts[13]++;
                    break;
            }
        }

        private static void CountRotors(Tower tower, int[] counts, int i)
        {
            switch (tower.TowerParts[i].SubIndex)
            {
                case 0:
                    counts[3]++;
                    break;
                case 1:
                    counts[4]++;
                    break;
                case 2:
                    counts[5]++;
                    break;
                case 3:
                    counts[6]++;
                    break;
                case 4:
                    counts[7]++;
                    break;
                case 5:
                    counts[8]++;
                    break;
                case 6:
                    counts[9]++;
                    break;
                case 7:
                    counts[10]++;
                    break;
                case 8:
                    counts[11]++;
                    break;
            }
        }

        private static void CountFoundations(Tower tower, int[] counts, int i)
        {
            switch (tower.TowerParts[i].SubIndex)
            {
                case 0:
                    counts[0]++;
                    break;
                case 1:
                    counts[1]++;
                    break;
                case 2:
                    counts[2]++;
                    break;
            }
        }
        #endregion

        public void DebugControls(SpriteBatch sb, TowerConstruct construct)
        {
            sb.Begin();
            sb.DrawString(Game1.debugFont, "Part Exists: " + (m_part != null) + "\nConstruct Parts: " + construct.Parts.Count + "\nHover Height: " + m_hoverHeight, new Vector2(1000, 700), Color.White);
            sb.End();
        }
    }
}
