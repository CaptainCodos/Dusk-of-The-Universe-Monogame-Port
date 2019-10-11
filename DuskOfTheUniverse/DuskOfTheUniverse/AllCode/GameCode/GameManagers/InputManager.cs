using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuskOfTheUniverse
{
    class InputManager
    {
        // Mouse Variables
        private MouseState m_mouse;
        private MouseState m_oldmouse;

        // Keyboard Variables
        private KeyboardState m_keys;
        private KeyboardState m_oldkeys;

        // Button Presses
        private bool m_leftIsPressed;
        private bool m_rightIsPressed;

        // Scrolling Variables
        private bool m_scrolledUp;
        private bool m_scrolledDown;

        // Scroll Val
        private int m_scroll;

        public bool LeftPressed { get { return m_leftIsPressed; } }
        public bool RightPressed { get { return m_rightIsPressed; } }

        public bool ScrolledUp { get { return m_scrolledUp; } }
        public bool ScrolledDown { get { return m_scrolledDown; } }

        public MouseState Mouse { get { return m_mouse; } }
        public MouseState OldMouse { get { return m_oldmouse; } }
        public KeyboardState Keys { get { return m_keys; } }
        public KeyboardState Oldkeys { get { return m_oldkeys; } }

        public InputManager()
        {
            m_leftIsPressed = false;
            m_rightIsPressed = false;

            m_scrolledUp = false;
            m_scrolledDown = false;

            m_scroll = 1;
        }

        public void HandleInput(MouseState mouse, KeyboardState keys)
        {
            m_mouse = mouse;
            m_keys = keys;

            // Check if mouse left button is pressed
            if (m_mouse.LeftButton == ButtonState.Pressed && m_oldmouse.LeftButton == ButtonState.Released)
                m_leftIsPressed = true;
            else
                m_leftIsPressed = false;

            // Check if mouse right button is pressed
            if (m_mouse.RightButton == ButtonState.Pressed && m_oldmouse.RightButton == ButtonState.Released)
                m_rightIsPressed = true;
            else
                m_rightIsPressed = false;

            // Check if scrolling up
            if (m_mouse.ScrollWheelValue > m_scroll && m_oldmouse.ScrollWheelValue == m_scroll)
                m_scrolledUp = true;
            else
                m_scrolledUp = false;

            // Check if scrolling down
            if (m_mouse.ScrollWheelValue < m_scroll && m_oldmouse.ScrollWheelValue == m_scroll)
                m_scrolledDown = true;
            else
                m_scrolledDown = false;

            m_scroll = m_mouse.ScrollWheelValue;
        }

        // Finish handling mouse and keyboard
        public void FinishHandling()
        {
            m_oldmouse = m_mouse;
            m_oldkeys = m_keys;
        }
    }
}
