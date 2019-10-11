using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class SelectionManager
    {
        // This is the list of characters for selection
        private List<GameChar> m_characters;
        // Current character
        private GameChar m_currChar;

        // This is the list of tower menus
        private List<RadialMenu> m_menus;

        // This is the mouse position
        private Vector2 m_mousePos;

        // This is 
        private UniMapData m_map;

        public SelectionManager(UniMapData map)
        {
            m_map = map;
        }

        public void UpdateManager(List<GameChar> friendlies, InputManager input, Camera cam, List<RadialMenu> towerMenus)
        {
            m_characters = friendlies;
            m_menus = towerMenus;

            CreateMouseCamTransform(input, cam);

            // Update ally selection
            Selection(input, cam);
            // Update tower menu selection
            TowerMenuSelection(input, cam);
        }

        // Transform the mouse position
        private void CreateMouseCamTransform(InputManager input, Camera cam)
        {
            m_mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);
            m_mousePos = Vector2.Transform(m_mousePos, Matrix.Invert(cam.Transform));
            m_mousePos = Vector2.Clamp(m_mousePos, Vector2.Zero, new Vector2(2875, 1615));
        }

        private void Selection(InputManager input, Camera cam)
        {
            Point checkCoords = new Point((int)m_mousePos.X / 36, (int)m_mousePos.Y / 36);

            SelectAlly(input);

            DeselectAlly(input, checkCoords);
        }

        // Deselect the currently selected ally
        private void DeselectAlly(InputManager input, Point checkCoords)
        {
            if (m_map.WalkableGrid[checkCoords.X, checkCoords.Y] > 0)
            {
                if (input.LeftPressed && m_currChar != null)
                {
                    m_currChar.IsSelected = false;
                    m_currChar = null;
                }
            }
        }

        // If an ally is selected, deselect all others
        private void SelectAlly(InputManager input)
        {
            for (int i = 0; i < m_characters.Count; i++)
            {
                if (m_characters[i].SelectionCircle.Contains(m_mousePos))
                {
                    if (input.LeftPressed)
                    {
                        m_currChar = m_characters[i];
                        m_characters[i].IsSelected = true;

                        for (int k = 0; k < m_characters.Count; k++)
                        {
                            if (k != i)
                                m_characters[k].IsSelected = false;
                        }
                    }
                }
            }

            #region Selection Keys
            if (input.Keys.IsKeyDown(Keys.D1))
            {
                for (int i = 0; i < m_characters.Count; i++)
                {
                    if (i == 0)
                    {
                        m_characters[i].IsSelected = true;
                    }
                    else
                    {
                        m_characters[i].IsSelected = false;
                    }
                }
            }

            if (input.Keys.IsKeyDown(Keys.D2))
            {
                for (int i = 0; i < m_characters.Count; i++)
                {
                    if (i == 1)
                    {
                        m_characters[i].IsSelected = true;
                    }
                    else
                    {
                        m_characters[i].IsSelected = false;
                    }
                }
            }

            if (input.Keys.IsKeyDown(Keys.D3))
            {
                for (int i = 0; i < m_characters.Count; i++)
                {
                    if (i == 2)
                    {
                        m_characters[i].IsSelected = true;
                    }
                    else
                    {
                        m_characters[i].IsSelected = false;
                    }
                }
            }
            #endregion
        }

        private void TowerMenuSelection(InputManager input, Camera cam)
        {
            Point checkCoords = new Point((int)m_mousePos.X / 36, (int)m_mousePos.Y / 36);

            SelectTowerMenu();
        }

        // Deploy tower menu and close all other menus
        private void SelectTowerMenu()
        {
            for (int i = 0; i < m_menus.Count; i++)
            {
                if (m_menus[i].DeployButton.IsPressed)
                {
                    if (!m_menus[i].IsDeployed)
                        m_menus[i].IsDeployed = true;
                    else
                        m_menus[i].IsDeployed = false;

                    for (int k = 0; k < m_menus.Count; k++)
                    {
                        if (k != i)
                            m_menus[k].IsDeployed = false;
                    }
                }
            }
        }
    }
}
